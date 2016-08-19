using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Xamarin.Droid.CrossWalkLite.MultiAbi.DemoApp
{
    [Activity(Label = "Xamarin.Droid.CrossWalkLite.MultiAbi.DemoApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Org.Xwalk.Core.XWalkActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        protected override void OnXWalkReady()
        {
            var view = new RelativeLayout(this.BaseContext);
            var mp = ViewGroup.LayoutParams.MatchParent;
            var xwv = new Org.Xwalk.Core.XWalkView(this.BaseContext, this);
            view.AddView(xwv);
            this.AddContentView(view, new ViewGroup.LayoutParams(mp, mp));

            xwv.Load("http://www.google.com", null);
        }
    }
}

