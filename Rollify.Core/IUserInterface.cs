using System;

using Rollify.SQLite;

namespace Rollify.Core
{
	/// <summary>
	/// A User Interface presents information from the app to the user, and is in charge of calling
	/// methods in AppLogic.
	/// </summary>
	public interface IUserInterface
	{
		string DatabaseLocation { get; }
		bool RollingEnabled { get; set; }

		Database<T> GetDatabase<T>(string path) where T : IDatabaseObject, new ();
		void UpdateFormulaList();
		void InsertFormulaText(string text);
		void DisplayRollResult(string result);
		void DisplayRollError(string error);
		void DisplayDebug(string text);
	}
}

