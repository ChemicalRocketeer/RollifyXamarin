using System;
using System.Collections.Generic;

namespace Rollify.Core.Extensions
{
	public static class Extensions
	{
		public static void Push (this Stack<string> str, object c) {
			str.Push (c.ToString());
		}

		public static void Add(this List<string> lst, object c) {
			lst.Add (c.ToString());
		}

		public static T Last<T>(this List<T> lst) {
			return lst[lst.Count - 1];
		}
	}
}

