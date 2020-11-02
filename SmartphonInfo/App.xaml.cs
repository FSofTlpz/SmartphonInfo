using Xamarin.Forms;

namespace SmartphonInfo {
   public partial class App : Application {
      public App() {
         InitializeComponent();

         //MainPage = new MainPage();

         MainPage = new NavigationPage(new MainPage()) {
            BarBackgroundColor = Color.FromRgb(0.3, 0.3, 0.3),
            BarTextColor = Color.White,
         };
      }

      protected override void OnStart() {
      }

      protected override void OnSleep() {
      }

      protected override void OnResume() {
      }
   }
}
