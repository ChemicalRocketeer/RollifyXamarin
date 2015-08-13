using System;
using System.Collections.Generic;
using System.Linq;

using Rollify.SQLiteBase;
using Rollify.SQLite;

namespace Rollify.Core
{
	public class Database<T> where T : IDatabaseObject, new ()
	{
		static object locker = new object();

		SQLiteWrapper connection;

		public Database (SQLiteWrapper conn)
		{
			connection = conn;
			connection.CreateTable<T> ();
		}

		public IEnumerable<T> GetItems() {
			lock (locker) {
				return (from i in connection.Table<T> ()
				        select i);
			}
		}

		public T this[int id] {
			get {
				lock (locker) {
					return connection.Table<T> ().FirstOrDefault (x => x.ID == id);
				}
			}
		}

		public T this[string name] {
			get {
				lock (locker) {
					return connection.Table<T> ().FirstOrDefault (x => x.Name == name);
				}
			}
		}

		/// <summary>
		/// Save the specified item and return its ID.
		/// </summary>
		public int Save(T item) {
			lock (locker) {
				if (item.ID == 0) {
					connection.Insert (item);
					return item.ID;
				} else {
					return connection.Update (item);
				}
			}
		}

		public int Delete(int id) {
			lock (locker) {
				return connection.Delete (new T () { ID = id });
			}
		}
	}
}

