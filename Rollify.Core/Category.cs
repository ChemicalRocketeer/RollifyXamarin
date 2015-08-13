using System;

using Rollify.SQLite;

namespace Rollify.Core
{
	public class Category : IDatabaseObject
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string Name { get; set; }
		public int Color { get; set; }
	}
}

