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

		public AppLogic(IUserInterface ui, AppLogic oldLogic = null) {
			this.ui = ui;
			if (oldLogic == null) {
				this.formulaDatabase = ui.GetDatabase<Formula> (ui.DatabaseLocation + "formula_database.db3");
				this.categoryDatabase = ui.GetDatabase<Category> (ui.DatabaseLocation + "category_database.db3");
			} else {
				this.formulaDatabase = oldLogic.formulaDatabase;
				this.categoryDatabase = oldLogic.categoryDatabase;
			}
		}

		public void Roll(string expression) {
			if (ui != null) {
				ui.RollingEnabled = false;
				Roller r = new Roller (formulaDatabase);
				try {
					ui.DisplayRollResult (r.Evaluate (expression).ToString ());
				} catch (InvalidExpressionException e) {
					ui.DisplayRollError (e.Message);
				} catch (Exception e) {
					ui.DisplayRollError ("Unknown error");
					ui.DisplayDebug (e.Message);
				}
				ui.DisplayDebug (r.DebugString);
				ui.RollingEnabled = true;
			}
		}

		public void UseFormula(Formula f) {
			if (ui != null) {
				ui.InsertFormulaText ("[" + f.Name + "]");
			}
			f.Uses++;
			formulaDatabase.Save (f);
			// don't update the ui because it would be confusing to have formulas move around as you use them.
		}

		public void SaveFormula (Formula f) {
			formulaDatabase.Save (f);
			if (ui != null) {
				ui.UpdateFormulaList (GetFormulasSorted ());
			}
		}

		public void DeleteFormula(Formula f) {
			formulaDatabase.Delete (f.ID);
			if (ui != null) {
				ui.UpdateFormulaList (GetFormulasSorted ());
			}
		}

		public Formula GetFormula(int id) {
			return formulaDatabase [id];
		}

		public IEnumerable<Formula> GetFormulasSorted() {
			return 
				from f in formulaDatabase.GetItems ()
				orderby f.Uses descending
				select f;
		}
	}
}

