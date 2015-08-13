using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;

using Rollify.Core;

namespace RollifyAndroid
{
	[Obsolete]
	/// <summary>
	/// Use Database<Formula> and Database<Category> instead.
	/// </summary>
	public class FormulaDatabase
	{
		private static object locker = new object ();

		public string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "formula_database.db3";

		public FormulaDatabase (string dbPath)
		{
			this.dbPath = dbPath;
			using (SqliteConnection connection = getConnection()) {
				connection.Open ();
				if (!TableExists ("Formulas", connection)) {
					using (SqliteCommand createFormulaTable = connection.CreateCommand ()) {
						createFormulaTable.CommandText = 
							"CREATE TABLE [Formulas] " +
							"(_id INTEGER PRIMARY KEY ASC, Category INTEGER, Name NTEXT, Expression NTEXT, Uses INTEGER)";
						createFormulaTable.ExecuteNonQuery ();
					} 
				}
				if (!TableExists ("Categories", connection)) {
					using (SqliteCommand createCategoryTable = connection.CreateCommand ()) {
						createCategoryTable.CommandText = 
							"CREATE TABLE [Categories] " +
							"(_id INTEGER PRIMARY KEY ASC, Name NTEXT, Color INTEGER)";
						createCategoryTable.ExecuteNonQuery ();
					}
				}
			}
		}

		private bool TableExists(string tablename, SqliteConnection conn) {
			try {
				DataTable dTable = conn.GetSchema("TABLES", new string[] { null, null, tablename });
				return dTable.Rows.Count > 0;
			} catch (SqliteException) {
				return false;
			}
		}

		private Formula ReadFormula(SqliteDataReader r) {
			Formula f = new Formula ();
			f.ID = Convert.ToInt32 (r ["_id"]);
			f.CategoryID = Convert.ToInt32 (r ["Category"]);
			f.Name = r ["Name"].ToString();
			f.Expression = r ["Expression"].ToString ();
			f.Uses = Convert.ToInt32 (r ["Uses"]);
			return f;
		}

		private Category ReadCategory(SqliteDataReader r) {
			Category c = new Category ();
			c.ID = Convert.ToInt32 (r ["_id"]);
			c.Name = r ["Name"].ToString ();
			c.Color = Convert.ToInt32 (r ["Color"]);
			return c;
		}

		/// <summary>
		/// Gets the formulas in the given category. If no category id is given or if it is -1, returns all formulas.
		/// </summary>
		/// <returns>The formulas.</returns>
		/// <param name="categoryID">ID of the category to filter through</param>
		public List<Formula> getFormulas(int categoryID = -1) {
			List<Formula> list = new List<Formula> ();
			lock (locker) {
				using (SqliteConnection connection = new SqliteConnection ("Data Source=" + dbPath)) {
					connection.Open ();
					using (SqliteCommand command = connection.CreateCommand ()) {
						if (categoryID == -1) {
							command.CommandText = "SELECT * " +
								"FROM [Formulas]";
						} else {
							command.CommandText = 
								"SELECT * " +
								"FROM [Formulas] " +
								"WHERE [Category] = @cat";
							command.Parameters.AddWithValue ("@cat", categoryID);
						}
						SqliteDataReader steve = command.ExecuteReader ();
						while (steve.Read ()) {
							list.Add (ReadFormula (steve));
						}
					}
				}
			}
			return list;
		}

		public List<Category> getCategories() {
			List<Category> list = new List<Category> ();
			lock (locker) {
				using (SqliteConnection connection = getConnection()) {
					connection.Open ();
					using (SqliteCommand command = connection.CreateCommand ()) {
						command.CommandText = 
							"SELECT * " +
							"FROM [Categories]";
						SqliteDataReader steve = command.ExecuteReader ();
						while (steve.Read ()) {
							list.Add (ReadCategory (steve));
						}
					}
				}
			}
			return list;
		}

		public int SaveFormula(Formula f) {
			lock (locker) {
				using (SqliteConnection connection = getConnection()) {
					connection.Open ();
					using (SqliteCommand cmd = connection.CreateCommand ()) {
						if (f.ID != 0) {
							// the formula is already in the database
							cmd.CommandText = 
								"UPDATE [Formulas] " +
								"SET [Name] = @name, [Category] = @cat, [Expression] = @expr, [Uses] = @uses " +
								"WHERE [_id] = @_id;";
							cmd.Parameters.Add (new SqliteParameter (DbType.Int32) { Value = f.ID });
							cmd.Parameters.AddWithValue ("@_id", f.ID);
						} else {
							// this is a new formula
							cmd.CommandText = 
								"INSERT INTO [Formulas] " +
								"([Name], [Category], [Expression], [Uses]) " +
								"VALUES (@name, @cat, @expr, @uses)";
						}
						cmd.Parameters.AddWithValue ("@name", f.Name);
						cmd.Parameters.AddWithValue ("@cat", f.CategoryID);
						cmd.Parameters.AddWithValue ("@expr", f.Expression);
						cmd.Parameters.AddWithValue ("@uses", f.Uses);
						return cmd.ExecuteNonQuery ();
					}
				}
			}
		}

		public int SaveCategory(Category cat) {
			lock (locker) {
				using (SqliteConnection connection = getConnection()) {
					connection.Open ();
					using (SqliteCommand cmd = connection.CreateCommand ()) {
						if (cat.ID != 0) {
							// the formula is already in the database
							cmd.CommandText =
								"UPDATE [Formulas] " +
								"SET [Name] = @name, [Color] = @color " +
								"WHERE [_id] = @_id;";
							cmd.Parameters.AddWithValue ("@_id", cat.ID);
						} else {
							// this is a new formula
							cmd.CommandText = "INSERT INTO [Formulas] " +
								"([Name], [Color]) " +
								"VALUES (@name, @color);";
						}
						cmd.Parameters.AddWithValue ("@name", cat.Name);
						cmd.Parameters.AddWithValue ("@color", cat.Color);
						return cmd.ExecuteNonQuery ();
					}
				}
			}
		}
		
		public int DeleteFormula(int id) {
			lock (locker) {
				using (SqliteConnection connection = getConnection()) {
					connection.Open ();
					using (SqliteCommand cmd = connection.CreateCommand ()) {
						cmd.CommandText = 
							"DELETE FROM [Formulas] " +
							"WHERE [_id] = @_id;";
						cmd.Parameters.AddWithValue ("@_id", id);
						return cmd.ExecuteNonQuery ();
					}
				}
			}
		}
		
		public int DeleteCategory(int id) {
			lock (locker) {
				using (SqliteConnection connection = getConnection()) {
					connection.Open ();
					using (SqliteCommand cmd = connection.CreateCommand ()) {
						cmd.CommandText = 
							"DELETE FROM [Categories] " +
							"WHERE [_id] = @_id;";
						cmd.Parameters.AddWithValue ("@_id", id);
						return cmd.ExecuteNonQuery ();
					}
				}
			}
		}
		
		private SqliteConnection getConnection() {
			return new SqliteConnection ("Data Source=" + dbPath);
		}
	}
}

