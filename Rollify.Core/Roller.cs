using System;
using System.Collections.Generic;
using System.Text;

using Rollify.Core.Extensions;

namespace Rollify.Core
{

	class Operator {
		public float Precedence;
		public Action<Stack<long>> Operate;
	}

	class Token
	{
		public string Content = "";
		public bool IsNumber = false;
		public long Num {
			get { return Int64.Parse (Content); }
			set { Content = value.ToString (); }
		}

		public override string ToString ()
		{
			return Content;
		}
	}

	static class Ext {
		public static void Add(this List<Token> lst, long c) {
			lst.Add (new Token { Num = c, IsNumber = true });
		}

		public static void Add(this List<Token> lst, string c) {
			lst.Add (new Token { Content = c });
		}
	}

	// TODO use biginteger instead of long
	public class Roller
	{

		private class RollDefinition {
			public long DieCount = 1;
			public long DieType = 1;
			public long KeepCount = 0;
			public KeepStrategy KeepStrategy = KeepStrategy.ALL;
			public bool Sort = false;
		}

		private static Random RAND = new Random ();

		private static Dictionary<string, Operator> operators = new Dictionary<string, Operator> () {
			{ "+", new Operator() { Precedence = 1f, Operate = delegate(Stack<long> stack) { stack.Push (stack.Pop () + stack.Pop ()); } } },
			{ "-", new Operator() { Precedence = 1f, Operate = delegate(Stack<long> stack) 
					{
						long b = stack.Pop();
						long a = stack.Pop();
						stack.Push (a - b); 
					} 
				} 
			},
			{ "*", new Operator() { Precedence = 2f, Operate = delegate(Stack<long> stack) { stack.Push (stack.Pop () * stack.Pop ()); } } },
			{ "/", new Operator() { Precedence = 2f, Operate = delegate(Stack<long> stack) 
					{
						long b = stack.Pop();
						if (b == 0)
							throw new InvalidExpressionException("Division by zero");
						long a = stack.Pop();
						stack.Push (a / b); 
					} 
				} 
			},
		};
			

		public string DebugString = "";
		private Database<Formula> formulas;
		private Stack<string> formulaNest; // used to prevent cyclic formula references

		public Roller (Database<Formula> formulas) {
			this.formulas = formulas;
			this.formulaNest = new Stack<string> ();
		}

		public long Evaluate(string expression) {
			return Evaluate (expression, 0);
		}

		// Evaluates a dice expression
		// Throws InvalidExpressionException with details if the expression is invalid
		private long Evaluate(string expression, int index, string formulaName = null) {

			if (formulaName != null) {
				// we are evaluating a formula, and will push it to the formulaNest so that we can prevent self-referential formulas
				if (formulaNest.Contains (formulaName)) {
					
					throw new InvalidExpressionException ("[" + formulaName + "] is self-referential");
				}
				formulaNest.Push(formulaName);
			}

			List<Token> pfExpression = InfixToPostfix (expression, index);
			DebugMessage (expression + " in postfix: " + string.Join (" ", pfExpression));

			// now that we have an arithmetic expression in postfix notation, we can easily process it
			Stack<long> stack = new Stack<long> ();
			for (int i = 0; i < pfExpression.Count; i++) {
				string token = pfExpression [i].Content;
				long numToken = 0;
				if (Int64.TryParse(token, out numToken)) { 
					// token is a number
					stack.Push (numToken);
				} else { 
					// token is operator
					Operator op = operators[token];
					if (op == null)
						throw new InvalidExpressionException("invalid operator " + token);
					op.Operate(stack);
				}
			}
			if (stack.Count != 1) {
				throw InvalidExpressionException.DEFAULT;
			}

			if (formulaName != null) {
				formulaNest.Pop ();
			}

			return stack.Pop();
		}


		// converts an infix dice formula to a postfix list of tokens. Can return throw InvalidExpressionException if the expression is invalid.
		private List<Token> InfixToPostfix(string input, int index) {
			StringScanner steve = new StringScanner (input, index);
			Stack<string> operatorStack = new Stack<string> ();
			List<Token> output = new List<Token>();
			steve.skipWhitespace ();
			bool lastTokenWasNumber = false;
			while (steve.HasNext()) {
				bool tokenIsNumber = false;
				if (Char.ToLower (steve.Peek ()) == 'd') {
					output.Add (ProcessDieDef (steve, operatorStack, output, lastTokenWasNumber));
					tokenIsNumber = true;
				} else if (IsCharOperator (steve.Peek ())) {
					ProcessOperator (steve, operatorStack, output, lastTokenWasNumber, ref tokenIsNumber);
				} else if (Char.IsDigit (steve.Peek ())) {
					output.Add (steve.ReadLong ());
					tokenIsNumber = true;
				} else if (steve.Peek () == '(') {
					ProcessParentheses(steve, output, lastTokenWasNumber);
					tokenIsNumber = true;
				} else if (steve.Peek() == '[') {
					ProcessFormula(steve, output, lastTokenWasNumber);
					tokenIsNumber = true;
				} else if (steve.Peek () == ')') {
					// processParentheses reads all the valid close-parens, so if we find one here it must be mismatched
					throw new InvalidExpressionException ("mismatched parentheses");
				} else {
					throw new InvalidExpressionException("invalid symbol: " + steve.Peek());
				}
				steve.skipWhitespace ();
				lastTokenWasNumber = tokenIsNumber;
			}
			while (operatorStack.Count > 0) {
				if (operatorStack.Peek () == "(")
					throw new InvalidExpressionException("mismatched parentheses");
				output.Add(operatorStack.Pop ());
			}
			return output;
		}

		private long ProcessDieDef(
				StringScanner steve, 
				Stack<string> opStack, 
				List<Token> output,
				bool lastTokenWasNumber) {
			steve.TrySkip(); // move past the d\
			if (!steve.HasNext())
				throw new InvalidExpressionException("no die type given");
			if (Char.IsDigit (steve.Peek ())) { // check that the syntax is valid before just trying to read it
				bool negative = false; // do we negate this die after rolling it?
				long dieCount = 1;
				if (lastTokenWasNumber) {
					// the last number was the die count, because it was followed by a 'd'
					dieCount = output.Last().Num;
					if (dieCount < 0) {
						// if the diecount is negative we will roll with the positive diecount, but negate the end result.
						// basically, "-5d6" is treated as "-(5d6)"
						negative = true;
						DebugMessage (dieCount.ToString() + "d" + dieCount + " is negative");
						dieCount = -dieCount;
					}
					output.RemoveAt (output.Count - 1);
				}
				long dieType = steve.ReadLong();
				// we now know that die type and the die count, now we need to see if there are extra instructions for the roll
				long keepCount = dieCount;
				KeepStrategy keepstrat = KeepStrategy.ALL;
				if (steve.HasNext () && char.IsLetter (steve.Peek ()) && Char.ToLower (steve.Peek ()) != 'd') {
					char extension = Char.ToLower (steve.Read ());
					if (extension == 'h') {
						keepstrat = KeepStrategy.HIGHEST;
						keepCount = 1;
						steve.TryReadLong (ref keepCount);
					} else if (extension == 'l') {
						keepstrat = KeepStrategy.LOWEST;
						keepCount = 1;
						steve.TryReadLong (ref keepCount);
					} else {
						throw new InvalidExpressionException("invalid die extension " + extension);
					}
				}
				// calculate and output the die roll
				long result = roll (dieType, dieCount, keepCount, keepstrat);
				if (negative)
					result = -result;
				return result;
			} else {
				throw new InvalidExpressionException("no die type given");
			}
		}

		// looks for a number in the list of tokens to use as a multiplier. 
		// If it finds one, it executes toIterate that number of times, else executes it only once.
		private long LookForMultiplier(List<Token> tokens, bool lastTokenWasNumber, Func<long> toIterate) {
			long iterationCount = 1;
			if (lastTokenWasNumber) {
				iterationCount = tokens.Last ().Num;
				tokens.RemoveAt (tokens.Count - 1);
			}
			bool negative = false;
			if (iterationCount < 0) {
				negative = true;
				iterationCount = -iterationCount;
			}
			long result = 0;
			for (int i = 0; i < iterationCount; i++) {
				result += toIterate();
			}
			return negative ? -result : result;
		}

		// computes and returns the value of an expression in parentheses.
		// The scanner cursor should be pointing at the opening paren when this is called.
		// When this method returns, the scanner cursor will be pointing at the character directly after the closing paren.
		// Returns the expression in parentheses
		private string ExtractParentheses(StringScanner scanner) {
			StringBuilder steve = new StringBuilder ();
			int nestCount = 1; // track nested parentheses
			scanner.TrySkip (); // move past open-paren
			if (!scanner.HasNext ())
				throw new InvalidExpressionException ("mismatched parentheses");
			while (!(scanner.Peek () == ')' && nestCount == 1)) {
				if (scanner.Peek () == '(') {
					nestCount++;
				} else if (scanner.Peek () == ')') {
					nestCount--;
				}
				steve.Append (scanner.Read ());
				if (!scanner.HasNext ())
					throw new InvalidExpressionException ("mismatched parentheses");
			}
			scanner.Read (); // move past close-paren
			DebugMessage ("evaluating \"" + steve.ToString () + "\" in parentheses");
			return steve.ToString ();
		}

		private void ProcessParentheses(StringScanner scanner, List<Token> output, bool lastTokenWasNumber) {
			// If the last token was a number, it's a count of how many times to execute the parenthetic expression.
			// in normal non-random-number math, this is just multiplication, but since we use random numbers,
			// it is implemented as iterative addition, allowing us to re-roll the dice every iteration.
			// If there are no dice defs in the parentheses, then this will yield the same result as normal multiplication.
			string expr = ExtractParentheses (scanner);
			output.Add (LookForMultiplier (output, lastTokenWasNumber, () => Evaluate (expr, 0)));
		}

		private void ProcessFormula(StringScanner scanner, List<Token> output, bool lastTokenWasNumber) {
			Formula f = ExtractFormula (scanner);
			output.Add (LookForMultiplier (output, lastTokenWasNumber, () => Evaluate (f.Expression, 0, f.Name)));
		}

		// reads the name of a formula, evaluates its expression, and returns the result.
		// the scanner cursor should be pointing at the opening bracket when this is called.
		// When this method returns, the cursor will be pointing at the character after the closing bracket.
		private Formula ExtractFormula(StringScanner scanner) {
			StringBuilder steve = new StringBuilder ();
			scanner.TrySkip();
			if (!scanner.HasNext ())
				throw new InvalidExpressionException ("mismatched brackets");
			while (scanner.Peek () != ']') {
				steve.Append (scanner.Read ());
				if (!scanner.HasNext ())
					throw new InvalidExpressionException ("mismatched brackets");
			}
			scanner.Read ();
			Formula f = formulas [steve.ToString ()];
			if (f == null)
				throw new InvalidExpressionException ("No formula " + steve.ToString ());
			return f;
		}

		private void ProcessOperator(StringScanner steve, Stack<string> opStack, List<Token> output,
				bool lastTokenWasNumber, ref bool tokenIsNumber) {
			string op = steve.Read ().ToString();
			if (lastTokenWasNumber) {
				PushOperatorToStack (op, opStack, output);
			} else if (op.Equals ("-")) {
				// The last token wasn't a number, but we encountered an operator, so this must be a negative sign
				steve.skipWhitespace ();
				if (!steve.HasNext ()) {
					throw new InvalidExpressionException("misplaced operator: " + op);
				}
				long num = 1;
				// if there's an expression after the minus sign, just process it and negate it
				if (steve.TryReadLong (ref num) || Char.ToLower (steve.Peek ()) == 'd' || steve.Peek() == '(' || steve.Peek() == '[') {
					output.Add (-num);
					// if the next token is a diedef, paren, or formula, iterativelyAdd will detect the -1 and use it to negate the expression
					tokenIsNumber = true;
				} else { 
					// there is no number after the minus sign, so it can't be negating a number, and it can't be doing subtraction
					throw new InvalidExpressionException ("misplaced operator: " + op);
				}
			} else {
				throw new InvalidExpressionException ("misplaced operator" + op);
			}
		}

		private static void PushOperatorToStack(String op, Stack<string> operatorStack, List<Token> output) {
			float precedence = OperatorPrecedence (op);
			while (operatorStack.Count > 0 && OperatorPrecedence (operatorStack.Peek ()) >= precedence) {
				output.Add (operatorStack.Pop ());
			}
			operatorStack.Push (op);
		}

		private static float OperatorPrecedence(string op) {
			Operator o;
			if (operators.TryGetValue (op, out o)) {
				return o.Precedence;
			} else {
				return -1f;
			}
		}

		private static bool IsCharOperator(char c) {
			return c == '+' || c == '-' || c == '*' || c == '/';
		}


		// If we are only keeping some dice, which ones do we keep?
		private enum KeepStrategy {
			ALL, HIGHEST, LOWEST
		}

		// TODO redesign keep strategy to allow keeping both highest and lowest
		private static long roll(long dieType, long dieCount, long keepCount, KeepStrategy keepStrategy) {
			if (dieType <= 1) {
				throw new InvalidExpressionException ("invalid die");
			}

			keepCount = Math.Min (keepCount, dieCount);

			// roll the dice and keep them in an array
			long[] results = new long[dieCount];
			for (long i = 0; i < dieCount; i++) {
				byte[] buf = new byte[8]; // Random has no long return type, so we have to make our own
				RAND.NextBytes(buf);
				results[i] = Math.Abs(BitConverter.ToInt64(buf, 0) % dieType) + 1;
			}

			// add up the results based on the strategy used
			long result = 0;
			if (keepStrategy == KeepStrategy.ALL) {
				for (long i = 0; i < dieCount; i++) {
					result += results [i];
				}
			} else { // we are only keeping some, so sort the list
				Array.Sort (results);
				if (keepStrategy == KeepStrategy.HIGHEST) {
					for (long i = dieCount - 1; i >= dieCount - keepCount; i--) {
						result += results [i];
					}
				} else if (keepStrategy == KeepStrategy.LOWEST) {
					for (long i = 0; i < keepCount; i++) {
						result += results [i];
					}
				}
			}
			return result;
		}

		private void DebugMessage(string message) {
			DebugString += message + '\n';
		}
	}
}

