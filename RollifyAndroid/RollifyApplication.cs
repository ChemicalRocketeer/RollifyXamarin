using System;

using Android.App;
using Android.Runtime;

namespace RollifyAndroid
{
	[Application]
	public class RollifyApplication : Application
	{
		public RollifyApplication (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate() {
			base.OnCreate ();

		}

		public override void OnConfigurationChanged() {
			base.OnConfigurationChanged ();
		}

		public override void OnLowMemory() {
			base.OnLowMemory ();
		}

		public override void OnTerminate() {
			base.OnTerminate ();
		}
	}
}

