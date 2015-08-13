﻿using System;
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
	[Activity (Label = "Rollify", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{

		Database<Formula> formulaDatabase;
		Database<Category> categoryDatabase;
		FormulaAdapter formulaAdapter;

		EditText rollFormulaEditor;
		Button rollButton;
		Button addFormulaButton;
		TextView rollResult;
		TextView debugTextView;
		ListView formulaListView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our data
			formulaDatabase = new Database<Formula> (
				new SqliteConnection(
					GetDir("rollify_data", FileCreationMode.Private).AbsolutePath + "formula_database.db3"));
			categoryDatabase = new Database<Category> (
				new SqliteConnection(
					GetDir("rollify_data", FileCreationMode.Private).AbsolutePath + "category_database.db3"));
			formulaAdapter = new FormulaAdapter (this, GetFormulasSorted ().ToArray());

			// Get references to our views
			rollFormulaEditor = FindViewById<EditText> (RollifyAndroid.Resource.Id.rollFormulaEditText);
			rollButton = FindViewById<Button> (RollifyAndroid.Resource.Id.rollButton);
			addFormulaButton = FindViewById<Button> (RollifyAndroid.Resource.Id.addFormulaButton);
			rollResult = FindViewById<TextView> (RollifyAndroid.Resource.Id.rollResult);
			debugTextView = FindViewById<TextView> (RollifyAndroid.Resource.Id.debugTextView);
			formulaListView = FindViewById<ListView> (RollifyAndroid.Resource.Id.formulaListView);

			// Link our views to data and register actions with them
			formulaListView.Adapter = formulaAdapter;
			RegisterForContextMenu (formulaListView);

			rollButton.Click += delegate {
				Roller r = new Roller(formulaDatabase);
				try {
					rollResult.Text = r.Evaluate(rollFormulaEditor.Text).ToString();
				} catch (InvalidExpressionException e) {
					rollResult.Text = e.Message;
				}
				debugTextView.Text = r.DebugString;
			};

			addFormulaButton.Click += delegate {
				Formula f = new Formula() {
					Name = rollFormulaEditor.Text,
					Expression = rollFormulaEditor.Text,
					Uses = 0,
					CategoryID = -1
				};
				formulaDatabase.Save (f);
				UpdateFormulaAdapter();
			};

			formulaListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Formula f = (Formula) formulaAdapter [e.Position];
				rollFormulaEditor.InsertTextAtCursor("[" + f.Name + "]");
				f.Uses++;
				formulaDatabase.Save (f);
				// formula adapter is not updated because list items moving around 
				// while you're using them would be confusing and annoying
			};
		}

		public void UpdateFormulaAdapter() {
			var sorted = GetFormulasSorted ();
			formulaAdapter.formulas = sorted.ToArray();
			formulaAdapter.NotifyDataSetChanged();
		}

		public IEnumerable<Formula> GetFormulasSorted() {
			return 
				from f in formulaDatabase.GetItems ()
				orderby f.Uses descending
				select f;
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
				rollFormulaEditor.Text += "[" + f.Name + "]";
				f.Uses++;
				break;
			case 1:
				break; //TODO implement editing
			case 2:
				formulaDatabase.Delete (f.ID);
				UpdateFormulaAdapter ();
				break;
			}
			return true;
		}
	}
}


