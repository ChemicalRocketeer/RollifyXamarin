using System;

using Android.App;
using Android.Runtime;
using Rollify.Core;
using Android.Content.Res;

namespace RollifyAndroid
{
	[Application]
	public class RollifyApplication : Application
	{
		
		public AppLogic Logic { get; set; }

		public RollifyApplication (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate() {
			base.OnCreate ();
		}

		public override void OnConfigurationChanged(Configuration newConfig) {
			base.OnConfigurationChanged (newConfig);
		}

		public override void OnLowMemory() {
			base.OnLowMemory ();
		}

		public override void OnTerminate() {
			base.OnTerminate ();
		}
	}
}

