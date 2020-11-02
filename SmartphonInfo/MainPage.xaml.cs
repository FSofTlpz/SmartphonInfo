using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SmartphonInfo {
   public partial class MainPage : ContentPage {

      string[] os_infonames;


      public MainPage() {
         InitializeComponent();

         //if (DeviceInfo.Platform == DevicePlatform.Android) {
         os_infonames = DependencyService.Get<IOsInfos>().InfoGroupNames();
         foreach (var item in os_infonames) {
            pickerExtData.Items.Add(item);
         }
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         registerCallBacks();

         buttonSampleData_Clicked(null, null);
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();
         deRegisterCallBacks();
      }


      void deRegisterCallBacks() {
         Xamarin.Essentials.DeviceDisplay.MainDisplayInfoChanged -= deviceDisplay_MainDisplayInfoChanged;
         Xamarin.Essentials.Battery.BatteryInfoChanged -= battery_BatteryInfoChanged;
         Xamarin.Essentials.Battery.EnergySaverStatusChanged -= battery_EnergySaverStatusChanged;
         Xamarin.Essentials.Connectivity.ConnectivityChanged -= connectivity_ConnectivityChanged;

         Xamarin.Essentials.Barometer.ReadingChanged -= barometer_ReadingChanged;
         if (Xamarin.Essentials.Barometer.IsMonitoring)
            Xamarin.Essentials.Barometer.Stop();

         Xamarin.Essentials.Compass.ReadingChanged -= compass_ReadingChanged;
         if (Xamarin.Essentials.Compass.IsMonitoring)
            Xamarin.Essentials.Compass.Stop();

         Xamarin.Essentials.Accelerometer.ReadingChanged -= accelerometer_ReadingChanged;
         Xamarin.Essentials.Accelerometer.ShakeDetected -= accelerometer_ShakeDetected;
         if (Xamarin.Essentials.Accelerometer.IsMonitoring)
            Xamarin.Essentials.Accelerometer.Stop();

         Xamarin.Essentials.Gyroscope.ReadingChanged -= gyroscope_ReadingChanged;
         if (Xamarin.Essentials.Gyroscope.IsMonitoring)
            Xamarin.Essentials.Gyroscope.Stop();

         Xamarin.Essentials.Magnetometer.ReadingChanged -= magnetometer_ReadingChanged;
         if (Xamarin.Essentials.Magnetometer.IsMonitoring)
            Xamarin.Essentials.Magnetometer.Stop();

         Xamarin.Essentials.OrientationSensor.ReadingChanged -= orientationSensor_ReadingChanged;
         if (Xamarin.Essentials.OrientationSensor.IsMonitoring)
            Xamarin.Essentials.OrientationSensor.Stop();

      }

      void registerCallBacks() {
         Xamarin.Essentials.Battery.BatteryInfoChanged += battery_BatteryInfoChanged;
         showBatteryInfo(Battery.ChargeLevel, Battery.PowerSource, Battery.State);
         Xamarin.Essentials.Battery.EnergySaverStatusChanged += battery_EnergySaverStatusChanged;
         showEnergySaverStatus(Battery.EnergySaverStatus);

         Xamarin.Essentials.Barometer.ReadingChanged += barometer_ReadingChanged;
         try {
            Xamarin.Essentials.Barometer.Start(SensorSpeed.UI);
         } catch {
            labelBarometer.Text = "Barometer: nicht unterstützt";
         }

         Xamarin.Essentials.Compass.ReadingChanged += compass_ReadingChanged;
         try {
            Xamarin.Essentials.Compass.Start(SensorSpeed.UI);
         } catch {
            labelCompass.Text = "Kompass: nicht unterstützt";
         }

         Xamarin.Essentials.Connectivity.ConnectivityChanged += connectivity_ConnectivityChanged;
         showConnectivity(Xamarin.Essentials.Connectivity.NetworkAccess, Xamarin.Essentials.Connectivity.ConnectionProfiles);

         Xamarin.Essentials.Accelerometer.ReadingChanged += accelerometer_ReadingChanged;
         Xamarin.Essentials.Accelerometer.ShakeDetected += accelerometer_ShakeDetected;
         try {
            Xamarin.Essentials.Accelerometer.Start(SensorSpeed.UI);
         } catch {
            labelAccelerometer.Text = "Beschleunigungsmesser: nicht unterstützt";
         }

         Xamarin.Essentials.DeviceDisplay.MainDisplayInfoChanged += deviceDisplay_MainDisplayInfoChanged;
         showDisplayInfo(Xamarin.Essentials.DeviceDisplay.MainDisplayInfo);

         Xamarin.Essentials.Gyroscope.ReadingChanged += gyroscope_ReadingChanged;
         try {
            Xamarin.Essentials.Gyroscope.Start(SensorSpeed.UI);
         } catch {
            labelCompass.Text = "Gyroscope: nicht unterstützt";
         }

         Xamarin.Essentials.Magnetometer.ReadingChanged += magnetometer_ReadingChanged;
         try {
            Xamarin.Essentials.Magnetometer.Start(SensorSpeed.UI);
         } catch {
            labelCompass.Text = "Magnetometer: nicht unterstützt";
         }

         Xamarin.Essentials.OrientationSensor.ReadingChanged += orientationSensor_ReadingChanged;
         try {
            Xamarin.Essentials.OrientationSensor.Start(SensorSpeed.UI);
         } catch {
            labelCompass.Text = "OrientationSensor: nicht unterstützt";
         }

      }

      #region Callback-Funktionen

      private void deviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e) {
         showDisplayInfo(e.DisplayInfo);
      }

      private void accelerometer_ShakeDetected(object sender, EventArgs e) { }

      private void accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e) {
         showAccelerometer(e.Reading.Acceleration);
      }

      private void connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
         showConnectivity(e.NetworkAccess, e.ConnectionProfiles);
      }

      private void battery_EnergySaverStatusChanged(object sender, EnergySaverStatusChangedEventArgs e) {
         showEnergySaverStatus(e.EnergySaverStatus);
      }

      private void battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e) {
         showBatteryInfo(e.ChargeLevel, e.PowerSource, e.State);
      }

      private void barometer_ReadingChanged(object sender, BarometerChangedEventArgs e) {
         showBarometer(e.Reading.PressureInHectopascals);
      }

      private void compass_ReadingChanged(object sender, CompassChangedEventArgs e) {
         showCompass(e.Reading.HeadingMagneticNorth);
      }

      private void magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e) {
         showMagneticField(e.Reading.MagneticField);
      }

      private void gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e) {
         showAngularVelocity(e.Reading.AngularVelocity);
      }

      private void orientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e) {
         showOrientation(e.Reading.Orientation);
      }

      #endregion

      #region Anzeige-Funktionen

      void showDisplayInfo(DisplayInfo displayInfo) {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("Display");
         sb.AppendLine("  Größe: " + displayInfo.Width.ToString() + "x" + displayInfo.Height.ToString());
         sb.AppendLine("  Auflösung: " + displayInfo.Density.ToString() + " -> " + (displayInfo.Density * 160).ToString("f0")+ " dpi");  // screen's density scale, (160dpi the "baseline" density)
         sb.AppendLine("  Ausrichtung: " + displayInfo.Orientation.ToString());
         sb.AppendLine("  Drehung: " + (displayInfo.Rotation == DisplayRotation.Rotation0 ? "0°" :
                                        displayInfo.Rotation == DisplayRotation.Rotation90 ? "90°" :
                                        displayInfo.Rotation == DisplayRotation.Rotation180 ? "180°" :
                                        displayInfo.Rotation == DisplayRotation.Rotation270 ? "270°" : "?"));
         outputDisplay.Text = sb.ToString();
      }

      void showBatteryInfo(double chargeLevel, BatteryPowerSource batteryPowerSource, BatteryState batteryState) {
         labelBatteryInfo.Text = string.Format("Batterieladung {0}%{3}PowerSource {1}{3}Status {2}",
                                               100.0 * chargeLevel,
                                               batteryPowerSource.ToString(),
                                               batteryState.ToString(),
                                               System.Environment.NewLine);
      }

      void showEnergySaverStatus(EnergySaverStatus energySaverStatus) {
         labelEnergySaverStatus.Text = string.Format("Energiesparmodus: {0}", energySaverStatus.ToString());
      }

      void showBarometer(double pressureinhectopascals) {
         labelBarometer.Text = string.Format("Barometer: {0:F1} hPa", pressureinhectopascals);
      }

      void showCompass(double degree) {
         labelCompass.Text = string.Format("Kompass: {0:F1}°", degree);
      }

      void showConnectivity(NetworkAccess networkAccess, IEnumerable<ConnectionProfile> connectionProfiles) {
         StringBuilder sb = new StringBuilder();
         sb.Append("Netzwerk: ");
         sb.Append(networkAccess.ToString());
         if (connectionProfiles.Count<ConnectionProfile>() > 0) {
            sb.AppendLine("");
            foreach (var item in connectionProfiles) {
               sb.Append("   ");
               sb.AppendLine(item.ToString());
            }
         }
         labelConnectivity.Text = sb.ToString();
      }

      void showAccelerometer(Vector3 v) {
         labelAccelerometer.Text = string.Format("Beschleunigung{3}   x={0:F2} m/s²{3}   y={1:F2} m/s²{3}   z={2:F2} m/s²",
                                                 v.X * 9.81,
                                                 v.Y * 9.81,
                                                 v.Z * 9.81,
                                                 System.Environment.NewLine);
      }

      void showMagneticField(Vector3 v) {
         labelMagneticField.Text = string.Format("Magnetfeld (in Mikrotesla){3}   x={0:F2}{3}   y={1:F2}{3}   z={2:F2}",
                                                 v.X,
                                                 v.Y,
                                                 v.Z,
                                                 System.Environment.NewLine);

      }

      void showAngularVelocity(Vector3 v) {
         labelAngularVelocity.Text = string.Format("Drehbewegung{3}   x={0:F2} rad/s{3}   y={1:F2} rad/s{3}   z={2:F2} rad/s",
                                                   v.X,
                                                   v.Y,
                                                   v.Z,
                                                   System.Environment.NewLine);
      }

      void showOrientation(Quaternion quaternion) {
         labelOrientationSensor.Text = string.Format("Ausrichtung{4}   x={0:F2}{4}   y={1:F2}{4}   z={2:F2}{4}   Drehung={3:F2}",
                                                     quaternion.X,
                                                     quaternion.Y,
                                                     quaternion.Z,
                                                     quaternion.W,
                                                     System.Environment.NewLine);
         /*
         The device (generally a phone or tablet) has a 3D coordinate system with the following axes:

               The positive X axis points to the right of the display in portrait mode.
               The positive Y axis points to the top of the device in portrait mode.
               The positive Z axis points out of the screen.

         The 3D coordinate system of the Earth has the following axes:

               The positive X axis is tangent to the surface of the Earth and points east.
               The positive Y axis is also tangent to the surface of the Earth and points north.
               The positive Z axis is perpendicular to the surface of the Earth and points up.

         The Quaternion describes the rotation of the device's coordinate system relative to the Earth's coordinate system.

         A Quaternion value is very closely related to rotation around an axis. If an axis of rotation is the normalized vector (ax, ay, az), 
         and the rotation angle is Θ, then the (X, Y, Z, W) components of the quaternion are:

         (ax·sin(Θ/2), ay·sin(Θ/2), az·sin(Θ/2), cos(Θ/2))

         These are right-hand coordinate systems, so with the thumb of the right hand pointed in the positive direction of the rotation axis, 
         the curve of the fingers indicate the direction of rotation for positive angles.

         Examples:

            When the device lies flat on a table with its screen facing up, with the top of the device (in portrait mode) pointing north, 
            the two coordinate systems are aligned. The Quaternion value represents the identity quaternion (0, 0, 0, 1). All rotations can be 
            analyzed relative to this position.

            When the device lies flat on a table with its screen facing up, and the top of the device (in portrait mode) pointing west, the Quaternion value 
            is (0, 0, 0.707, 0.707). The device has been rotated 90 degrees around the Z axis of the Earth.

            When the device is held upright so that the top (in portrait mode) points towards the sky, and the back of the device faces north, the device has 
            been rotated 90 degrees around the X axis. The Quaternion value is (0.707, 0, 0, 0.707).

            If the device is positioned so its left edge is on a table, and the top points north, the device has been rotated –90 degrees around the Y axis 
            (or 90 degrees around the negative Y axis). The Quaternion value is (0, -0.707, 0, 0.707).

          */
      }

      #endregion

      #region Button-Funktionen

      private void buttonSampleData_Clicked(object sender, EventArgs e) {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("Gerät");
         sb.AppendLine("  Gerätetyp: " + Xamarin.Essentials.DeviceInfo.Idiom.ToString());
         sb.AppendLine("  Hersteller: " + Xamarin.Essentials.DeviceInfo.Manufacturer.ToString());
         sb.AppendLine("  Typ: " + Xamarin.Essentials.DeviceInfo.DeviceType.ToString());
         sb.AppendLine("  Modell: " + Xamarin.Essentials.DeviceInfo.Model.ToString());
         sb.AppendLine("  Name: " + Xamarin.Essentials.DeviceInfo.Name.ToString());
         sb.AppendLine("  OS: " + Xamarin.Essentials.DeviceInfo.Platform.ToString());
         sb.AppendLine("  Version: " + Xamarin.Essentials.DeviceInfo.Version.ToString());
         sb.AppendLine("  VersionString: " + Xamarin.Essentials.DeviceInfo.VersionString.ToString());

         outputDevice.Text = sb.ToString();
      }

      private void buttonFlashlightOn_Clicked(object sender, EventArgs e) {
         Xamarin.Essentials.Flashlight.TurnOnAsync();
         buttonFlashlightOff.IsEnabled = true;
         buttonFlashlightOn.IsEnabled = false;
      }

      private void buttonFlashlightOff_Clicked(object sender, EventArgs e) {
         Xamarin.Essentials.Flashlight.TurnOffAsync();
         buttonFlashlightOff.IsEnabled = false;
         buttonFlashlightOn.IsEnabled = true;
      }

      async private void buttonVibrationOn_Clicked(object sender, EventArgs e) {
         try {
            Xamarin.Essentials.Vibration.Vibrate(1000); // Zeit in ms
         } catch (Exception ex) {
            await DisplayAlert("Fehler", ex.Message, "weiter ...");
         }
      }

      private void buttonVibrationOff_Clicked(object sender, EventArgs e) {
         Xamarin.Essentials.Vibration.Cancel();
      }

      async private void buttonLastKnownLocation_Clicked(object sender, EventArgs e) {
         try {
            Location location = await Xamarin.Essentials.Geolocation.GetLastKnownLocationAsync();
            if (location != null)
               outputLastKnownLocation.Text = getLocationString(location);
         } catch (Exception ex) {
            outputLastKnownLocation.Text = ex.Message;
         }
      }

      async private void buttonLocation_Clicked(object sender, EventArgs e) {
         try {
            Location location = await Xamarin.Essentials.Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if (location != null)
               outputLocation.Text = getLocationString(location);
         } catch (Exception ex) {
            outputLocation.Text = ex.Message;
         }
      }

      #endregion

      string getLocationString(Location location) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("lat={0:F1}°, lon={1:F1}°, Höhe={2}",
                         location.Latitude,
                         location.Longitude,
                         location.Altitude != null ? string.Format("{0:F1} m", location.Altitude) : "-");
         sb.AppendLine("");
         sb.AppendFormat("Genauigkeit={0}, vert. Genauigkeit={1}",
                         location.Accuracy != null ? string.Format("{0:F1} m", location.Accuracy) : "-",
                         location.VerticalAccuracy != null ? string.Format("{0:F1} m", location.VerticalAccuracy) : "-");
         sb.AppendLine("");
         sb.AppendFormat("Richtung={0}, Geschwindigkeit={1}",
                         location.Course != null ? string.Format("{0:F1}°", location.Course) : "-",
                         location.Speed != null ? string.Format("{0:F1} m/s", location.Speed) : "-");
         sb.AppendLine("");
         sb.AppendLine("Zeitpunkt=" + location.Timestamp.ToString("G"));
         return sb.ToString();
      }

      async private void pickerExtData_SelectedIndexChanged(object sender, EventArgs e) {
         if (0 <= pickerExtData.SelectedIndex && pickerExtData.SelectedIndex < os_infonames.Length) {

            string tmp = DependencyService.Get<IOsInfos>().Info(pickerExtData.SelectedIndex);
            if (!string.IsNullOrEmpty(tmp)) {
               StringBuilder sb = new StringBuilder();
               sb.AppendLine("OS-interne Infos:");
               sb.AppendLine("");
               sb.AppendLine(tmp);

               await Navigation.PushAsync(new OSinternPage(sb.ToString()) {
                  Title = "OS-Info: " + os_infonames[pickerExtData.SelectedIndex],
               });
            }
         }
      }

      //Xamarin.Essentials.Geocoding.GetLocationsAsync(""); 
      //Xamarin.Essentials.Geocoding.GetPlacemarksAsync(...);


   }
}
