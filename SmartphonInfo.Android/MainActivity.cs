using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace SmartphonInfo.Droid {
   [Activity(Label = "SmartphonInfo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
   public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

      FSofTUtils.Android.PermissonRequest permissonRequest = new FSofTUtils.Android.PermissonRequest(new string[] { 
         Android.Manifest.Permission.ReadPhoneState,
         Android.Manifest.Permission.AccessCoarseLocation,
         Android.Manifest.Permission.AccessFineLocation,
         Android.Manifest.Permission.AccessMockLocation,
         Android.Manifest.Permission.AccessNetworkState,
         Android.Manifest.Permission.Flashlight,
         Android.Manifest.Permission.Vibrate,
         Android.Manifest.Permission.BatteryStats,
         Android.Manifest.Permission.Camera,
         Android.Manifest.Permission.DevicePower,
      });


      protected override void OnCreate(Bundle savedInstanceState) {
         TabLayoutResource = Resource.Layout.Tabbar;
         ToolbarResource = Resource.Layout.Toolbar;

         base.OnCreate(savedInstanceState);

         Xamarin.Essentials.Platform.Init(this, savedInstanceState);
         global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
         MyInit();
         LoadApplication(new App());
      }

      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
         Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
         permissonRequest.OnRequestPermissionsResult(requestCode, grantResults);
         base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }




      void MyInit() {
         permissonRequest.CheckAndRequest(this);
         OsInfos.activity = this;

         //Xamarin.Forms.Platform.Android.Resource.Styleable.TextAppearance_textAllCaps = 0;

      }


      /// <summary>
      /// reagiert auf den Software-Backbutton (fkt. NUR mit SetSupportActionBar() in OnCreate())
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public override bool OnOptionsItemSelected(IMenuItem item) {
         // check if the current item id is equals to the back button id
         if (item.ItemId == Android.Resource.Id.Home) {
            Xamarin.Forms.Application myapplication = Xamarin.Forms.Application.Current;
            if (myapplication.MainPage.SendBackButtonPressed())
               return false;
         }
         return base.OnOptionsItemSelected(item);
      }

      /// <summary>
      /// Hardware-Backbutton
      /// </summary>
      public override void OnBackPressed() {
         // this is not necessary, but in Android user has both Nav bar back button and physical back button its safe to cover the both events
         Xamarin.Forms.Application myapplication = Xamarin.Forms.Application.Current;
         if (myapplication.MainPage.SendBackButtonPressed())
            return;
         base.OnBackPressed();
      }

   }

}