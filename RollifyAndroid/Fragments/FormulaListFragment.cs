
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

		FormulaAdapter formulaAdapter;
		ListView formulaListView;

		public FormulaListFragment() : base() {
			
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// inflate our view
			View layout = inflater.Inflate(Resource.Layout.FormulaList, container, false);

			// Get our data
			formulaAdapter = new FormulaAdapter (this.Activity, Globals.Logic.GetFormulasSorted ().ToArray());

			// populate the listview
			formulaListView = layout.FindViewById<ListView> (RollifyAndroid.Resource.Id.formulaListView);
			formulaListView.Adapter = formulaAdapter;

			// register click events
			formulaListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Formula f = (Formula) formulaAdapter [e.Position];
				Globals.Logic.UseFormula(f);
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
				Globals.Logic.UseFormula (f);
				break;
			case 1:
				var intent = new Intent (this.Activity, typeof(FormulaDetailsActivity));
				intent.PutExtra ("formulaID", f.ID);
				StartActivity (intent);
				break;
			case 2:
				Globals.Logic.DeleteFormula (f);
				break;
			}
			return true;
		}
	}
}

