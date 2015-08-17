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
		AppLogic logic;

		FormulaAdapter formulaAdapter;

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
		ListView formulaListView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Set up AppLogic
			logic = new AppLogic(this);

			// Get our data
			formulaAdapter = new FormulaAdapter (this, logic.GetFormulasSorted ().ToArray());

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
			formulaListView = FindViewById<ListView> (RollifyAndroid.Resource.Id.formulaListView);

			// Link our views to data and register actions with them
			formulaListView.Adapter = formulaAdapter;
			RegisterForContextMenu (formulaListView);

			calcAdd.Click += delegate { InsertFormulaText("+"); };
			calcSub.Click += delegate { InsertFormulaText("-"); };
			calcMul.Click += delegate { InsertFormulaText("*"); };
			calcDiv.Click += delegate { InsertFormulaText("/"); };
			rollButton.Click += delegate { logic.Roll(rollFormulaEditor.Text); };
			addFormulaButton.Click += delegate { logic.AddFormula(rollFormulaEditor.Text, rollFormulaEditor.Text, -1); };

			formulaListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Formula f = (Formula) formulaAdapter [e.Position];
				logic.UseFormula(f);
			};
		}

		public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo) {
			if (v.Id == RollifyAndroid.Resource.Id.formulaListView) {
				AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo)menuInfo;
				menu.SetHeaderTitle (formulaAdapter [info.Position].Name);
				String[] menuItems = {"Use", "Edit", "Delete" };
				for (int i = 0; i < menuItems.Length; i++) {
					menu.Add (Menu.None, i, i, menuItems [i]);
				}
			}
		}

		public override bool OnContextItemSelected(IMenuItem menuItem) {
			AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo)menuItem.MenuInfo;
			Formula f = formulaAdapter [info.Position];
			switch (menuItem.ItemId) {
			case 0:
				logic.UseFormula (f);
				break;
			case 1:
				break; //TODO implement editing
			case 2:
				logic.DeleteFormula (f);
				break;
			}
			return true;
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

		public void UpdateFormulaList() {
			IEnumerable<Formula> sorted = logic.GetFormulasSorted ();
			formulaAdapter.formulas = sorted.ToArray();
			formulaAdapter.NotifyDataSetChanged();
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


