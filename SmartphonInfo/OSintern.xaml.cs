using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartphonInfo {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OSinternPage : ContentPage {

      public OSinternPage(string infotext = null) {
         InitializeComponent();

         outputInfo.Text = infotext;
      }

   }
}