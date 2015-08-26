using System;
using System.Collections.Generic;
using System.Linq;

namespace Rollify.Core
{
	public class AppLogic
	{
		private IUserInterface ui;

		private Database<Formula> formulaDatabase;
		private Database<Category> categoryDatabase;

		public AppLogic(IUserInterface ui) {
			this.ui = ui;
			this.formulaDatabase = ui.GetDatabase<Formula> (ui.DatabaseLocation + "formula_database.db3");
			this.categoryDatabase = ui.GetDatabase<Category> (ui.DatabaseLocation + "category_database.db3");
		}

		public void Roll(string expression) {
			ui.RollingEnabled = false;
			Roller r = new Roller (formulaDatabase);
			try {
				ui.DisplayRollResult(r.Evaluate(expression).ToString());
			} catch (InvalidExpressionException e) {
				ui.DisplayRollError(e.Message);
			} catch (Exception) {
				ui.DisplayRollError("Unknown error");
			}
			ui.DisplayDebug(r.DebugString);
			ui.RollingEnabled = true;
		}

		public void AddFormula(string name, string expression, int categoryID) {
			Formula f = new Formula {
				Name = name,
				Expression = expression,
				CategoryID = categoryID,
				Uses = 0,
			};
			formulaDatabase.Save (f);
			ui.UpdateFormulaList (GetFormulasSorted());
		}

		public void UseFormula(Formula f) {
			ui.InsertFormulaText ("[" + f.Name + "]");
			f.Uses++;
			formulaDatabase.Save (f);
			// don't update the view because it would be confusing to have formulas move around as you use them.
		}

		public void UpdateFormula (Formula f) {
			formulaDatabase.Save (f);
		}

		public void DeleteFormula(Formula f) {
			formulaDatabase.Delete (f.ID);
			ui.UpdateFormulaList (GetFormulasSorted());
		}

		public IEnumerable<Formula> GetFormulasSorted() {
			return 
				from f in formulaDatabase.GetItems ()
				orderby f.Uses descending
				select f;
		}
	}
}

