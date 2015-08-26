
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Rollify.Core;

namespace RollifyAndroid
{
	public class FormulaListFragment : Fragment
	{

		AppLogic logic;

		FormulaAdapter formulaAdapter;
		ListView formulaListView;

		public FormulaListFragment(AppLogic logic) : base() {
			this.logic = logic;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// inflate our view
			View layout = inflater.Inflate(Resource.Layout.FormulaList, container, false);

			// Get our data
			formulaAdapter = new FormulaAdapter (this.Activity, logic.GetFormulasSorted ().ToArray());

			// populate the listview
			formulaListView = layout.FindViewById<ListView> (RollifyAndroid.Resource.Id.formulaListView);
			formulaListView.Adapter = formulaAdapter;

			// register click events
			formulaListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Formula f = (Formula) formulaAdapter [e.Position];
				logic.UseFormula(f);
			};
			RegisterForContextMenu (formulaListView);

			return layout;
		}

		public void UpdateFormulaList(IEnumerable<Formula> formulas) {
			formulaAdapter.formulas = formulas.ToArray();
			formulaAdapter.NotifyDataSetChanged();
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
	}
}

