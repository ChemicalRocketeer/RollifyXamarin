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
		public BigInteger ReadLong() {
			StringBuilder steve = new StringBuilder ();
			while (HasNext() && Char.IsDigit (Peek())) {
				steve.Append(Read ());
			}
			return new BigInteger(steve.ToString(), 10);
		}

		// reads until the cursor hits a non-digit, and assigns the result to the given result variable.
		// returns true if parse was successful. If parse is unsuccessful, result is unchanged.
		public bool TryReadLong(ref BigInteger result) {
			// read the digits into a string
			StringBuilder steve = new StringBuilder ();
			while (HasNext() && Char.IsDigit (Peek())) {
				steve.Append(Read ());
			}
			// parse the string
			try {
				result = new BigInteger(steve.ToString(), 10);
				return true;
			} catch (ArithmeticException) {
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

