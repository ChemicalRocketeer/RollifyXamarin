using System;

using Android.Widget;

namespace Rollify.Extensions
{
	public static class ExtensionsAndroid
	{

		/// <returns><c>true</c>, if any text was replaced by the insert operation, <c>false</c> otherwise.</returns>
		public static bool InsertTextAtCursor(this EditText ed, string text) {
			int start = Math.Max(ed.SelectionStart, 0);
			int end = Math.Max(ed.SelectionEnd, 0);
			ed.EditableText.Replace(Math.Min(start, end), Math.Max(start, end), new Java.Lang.String(text), 0, text.Length);
			return start != end;
		}

		//public static void Backspace(
	}
}

