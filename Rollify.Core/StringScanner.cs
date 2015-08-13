using System;
using System.Text;

namespace Rollify.Core
{
	public class StringScanner
	{
		string s;
		public int Cursor { get; private set; } // index of the next char to be read

		public StringScanner (string s)
		{
			this.s = s;
			this.Cursor = 0;
		}

		public StringScanner(string s, int index) {
			this.s = s;
			this.Cursor = index;
		}

		public StringScanner(StringScanner other) {
			this.s = other.s;
			this.Cursor = other.Cursor;
		}

		// read the current char without advancing the cursor
		public char Peek() {
			return s [Cursor];
		}

		// read a char
		public char Read() {
			return s[Cursor++];
		}

		public bool TryRead(ref char result) {
			if (HasNext ()) {
				result = Read ();
				return true;
			}
			return false;
		}

		public bool TrySkip() {
			if (HasNext ()) {
				Read ();
				return true;
			}
			return false;
		}

		// reads until the cursor hits a non-digit
		public long ReadLong() {
			StringBuilder steve = new StringBuilder ();
			while (HasNext() && Char.IsDigit (Peek())) {
				steve.Append(Read ());
			}
			if (steve.Length > 18) {
				throw new InvalidExpressionException ("Number too large: " + steve.ToString ());
			}
			return Int64.Parse (steve.ToString());
		}

		// reads until the cursor hits a non-digit, and assigns the result to the given result variable.
		// returns true if parse was successful. If parse is unsuccessful, result is unchanged.
		public bool TryReadLong(ref long result) {
			StringBuilder steve = new StringBuilder ();
			while (HasNext() && Char.IsDigit (Peek())) {
				steve.Append(Read ());
			}
			if (steve.Length > 18) {
				throw new InvalidExpressionException ("Number too large: " + steve.ToString ());
			}
			long temp = result;
			if (Int64.TryParse (steve.ToString(), out temp)) {
				result = temp;
				return true;
			} else {
				return false;
			}
		}

		// reads until a non-whitespace character is found
		public void skipWhitespace() {
			while (HasNext () && Char.IsWhiteSpace (Peek ())) {
				Read ();
			}
		}

		public bool HasNext() {
			return s.Length > Cursor;
		}
	}
}

