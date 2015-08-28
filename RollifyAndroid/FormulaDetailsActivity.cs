
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Rollify.Core;

namespace RollifyAndroid
{
	[Activity (
		Label = "FormulaDetailsActivity",
		WindowSoftInputMode=SoftInput.StateVisible|SoftInput.AdjustResize 
	)]
	public class FormulaDetailsActivity : Activity
	{
		EditText nameText;
		EditText expressionText;

		Formula formula;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.FormulaDetails);

			Globals.Logic = new AppLogic (null, Globals.Logic);

			// Get our edittext views
			nameText = (EditText)FindViewById (Resource.Id.formulaName);
			expressionText = (EditText)FindViewById (Resource.Id.formulaExpression);

			FindViewById (Resource.Id.saveButton).Click += delegate { OnSave (); };
			FindViewById (Resource.Id.cancelButton).Click += delegate { OnCancel (); };

			int formulaID = Intent.GetIntExtra ("formulaID", DatabaseConstants.ID_UNASSIGNED);
			if (formulaID != DatabaseConstants.ID_UNASSIGNED) {
				// we are editing an existing formula
				formula = Globals.Logic.GetFormula (formulaID);
				nameText.Text = formula.Name;
				expressionText.Text = formula.Expression;
			} else {
				// we are creating a new formula
				formula = new Formula();
			}
			string formulaExpression = Intent.GetStringExtra("formulaExpression");
			if (formulaExpression != null) {
				expressionText.Text = formulaExpression;
			}
		}

		public void OnCancel() {
			Finish ();
		}

		public void OnSave() {
			formula.Name = nameText.Text;
			formula.Expression = expressionText.Text;
			Globals.Logic.SaveFormula (formula);
			Finish ();
		}
	}
}

