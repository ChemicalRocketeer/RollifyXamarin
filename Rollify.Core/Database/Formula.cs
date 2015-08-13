using System;

using Rollify.SQLite;

namespace Rollify.Core
{
	public class Formula : IDatabaseObject
	{
		/// <summary>
		/// A category ID representing uncategorized formulas
		/// </summary>
		public const int NO_CATEGORY = -2;

		[PrimaryKey, AutoIncrement]
		public int ID { get; set; } 
		public string Name { get; set; }
		public string Expression { get; set; }
		public int CategoryID { get; set; }
		public int Uses { get; set; }
	}
}

