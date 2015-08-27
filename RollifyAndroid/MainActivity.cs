using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Rollify.Core;
using Rollify.SQLite;
using Rollify.Extensions;

namespace RollifyAndroid
{
	[Activity (
		Label = "Rollify", 
		//Theme = "@android:style/Theme.Material.Dark",
		MainLauncher = true, 
		Icon = "@drawable/icon", 
		WindowSoftInputMode=SoftInput.StateHidden|SoftInput.AdjustPan
	)]
	public class MainActivity : Activity, IUserInterface
	{
		public AppLogic Logic 
		{
			get { return ((RollifyApplication)this.Application).Logic; }
			set { ((RollifyApplication)this.Application).Logic = value; }
		}

		FormulaListFragment formulaList;

		Button calcAdd;
		Button calcSub;
		Button calcMul;
		Button calcDiv;

		Button rollButton;
		ImageButton backspaceButton;
		Button addFormulaButton;

		EditText rollFormulaEditor;
		TextView rollResult;
		TextView debugTextView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Set up AppLogic
			if (Logic == null) {
				Logic = new AppLogic (this);
			}

			// Attach fragments
			var fragTransaction = FragmentManager.BeginTransaction ();
			var numpadFragment = new CalcNumpadFragment(this);
			formulaList = new FormulaListFragment (Logic);
			fragTransaction.Add (Resource.Id.numpadContainer, numpadFragment, "numpad");
			fragTransaction.Add (Resource.Id.formulaListContainer, formulaList, "formulaList");
			fragTransaction.Commit ();

			// Reference the calculator buttons
			calcAdd = FindViewById<Button> (Resource.Id.calcAdd);
			calcSub = FindViewById<Button> (Resource.Id.calcSub);
			calcMul = FindViewById<Button> (Resource.Id.calcMul);
			calcDiv = FindViewById<Button> (Resource.Id.calcDiv);

			backspaceButton = FindViewById<ImageButton> (RollifyAndroid.Resource.Id.backspaceImageButton);
			rollButton = FindViewById<Button> (RollifyAndroid.Resource.Id.rollButton);
			addFormulaButton = FindViewById<Button> (RollifyAndroid.Resource.Id.addFormulaButton);

			// Get references to our views
			rollFormulaEditor = FindViewById<EditText> (RollifyAndroid.Resource.Id.rollFormulaEditText);
			rollResult = FindViewById<TextView> (RollifyAndroid.Resource.Id.rollResult);
			debugTextView = FindViewById<TextView> (RollifyAndroid.Resource.Id.debugTextView);

			// Link our views to data and register actions with them

			calcAdd.Click += delegate { InsertFormulaText("+"); };
			calcSub.Click += delegate { InsertFormulaText("-"); };
			calcMul.Click += delegate { InsertFormulaText("*"); };
			calcDiv.Click += delegate { InsertFormulaText("/"); };
			rollButton.Click += delegate { Logic.Roll(rollFormulaEditor.Text); };
			addFormulaButton.Click += delegate { Logic.AddFormula(rollFormulaEditor.Text, rollFormulaEditor.Text, -1); };
		}

		public string DatabaseLocation {
			get { return GetDir ("rollify_data", FileCreationMode.Private).AbsolutePath; }
		}

		public bool RollingEnabled {
			get { return rollButton.Enabled; }
			set { rollButton.Enabled = value; }
		}

		public Database<T> GetDatabase<T>(string path) 
			where T : IDatabaseObject, new () {
			return new Database<T> (new SqliteConnection (path));
		}

		public void UpdateFormulaList(IEnumerable<Formula> formulas) {
			this.formulaList.UpdateFormulaList (formulas);
		}

		public void InsertFormulaText(string text) {
			rollFormulaEditor.InsertTextAtCursor (text);
		}

		public void DisplayRollResult(string result) {
			rollResult.Text = result;
		}

		public void DisplayRollError(string error) {
			rollResult.Text = error;
		}

		public void DisplayDebug(string text) {
			debugTextView.Text = text;
		}
	}
}


