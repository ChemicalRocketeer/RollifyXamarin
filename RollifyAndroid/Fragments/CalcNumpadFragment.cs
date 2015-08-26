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
	public class CalcNumpadFragment : Fragment
	{
		IUserInterface ui;

		public CalcNumpadFragment(IUserInterface ui) {
			this.ui = ui;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			// Inflate the layout for this fragment
			var layout = inflater.Inflate(Resource.Layout.CalcNumpad, container, false);
			int[] buttonIDs = new int[] {
				Resource.Id.calc_0, Resource.Id.calc_1, Resource.Id.calc_2, 
				Resource.Id.calc_3, Resource.Id.calc_4, Resource.Id.calc_5, 
				Resource.Id.calc_6, Resource.Id.calc_7, Resource.Id.calc_8, 
				Resource.Id.calc_9, Resource.Id.calc_close_paren, Resource.Id.calc_open_paren
			};
			foreach (int id in buttonIDs) {
				Button butt = (Button)layout.FindViewById (id);
				butt.Click += delegate { this.ui.InsertFormulaText(butt.Text); };
			}
			return layout;
		}
	}
}

