using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telecom;
using FSofTUtils.Android;

[assembly: Xamarin.Forms.Dependency(typeof(SmartphonInfo.Droid.OsInfos))]
namespace SmartphonInfo.Droid {
   internal class OsInfos : IOsInfos {

      public static Activity activity;

      private static readonly Android.Content.Context context = global::Android.App.Application.Context;

      const int NAME1POS = 3;
      const int NAME2POS = 3;
      const int CONTENT1POS = 30;


      string getRam(Context context) {
         StringBuilder sb = new StringBuilder();
         try {
            Android.App.ActivityManager.MemoryInfo memInfo = new Android.App.ActivityManager.MemoryInfo();
            Android.App.ActivityManager actManager = context.GetSystemService(Android.Content.Context.ActivityService) as Android.App.ActivityManager;
            actManager.GetMemoryInfo(memInfo);
            sb.AppendLine(getItemString("RAM:", string.Format("{0} B ({1:N1} kb / {2:N1} MB / {3:N1} GB)",
                                                               memInfo.TotalMem,
                                                               memInfo.TotalMem / 1024.0,
                                                               memInfo.TotalMem / (1024.0 * 1024.0),
                                                               memInfo.TotalMem / (1024.0 * 1024.0 * 1024.0)), NAME1POS, CONTENT1POS));
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getVolumeInfos(Activity activity) {
         StringBuilder sb = new StringBuilder();
         try {
            FSofTUtils.Android.StorageHelperAndroid storageHelperAndroid = new FSofTUtils.Android.StorageHelperAndroid(activity);
            for (int i = 0; i < storageHelperAndroid.Volumes; i++) {
               StorageHelperAndroid.VolumeData vd = storageHelperAndroid.GetVolumeData(i);
               if (i > 0)
                  sb.AppendLine("");
               sb.AppendLine(getItemString("Volume " + i.ToString() + ":", vd.Name + ", " + vd.Description, NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("Pfad", vd.Path, NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("Bytes", string.Format("{0} B ({1:N1} kb / {2:N1} MB / {3:N1} GB), davon {4} B ({5:N1} kb / {6:N1} MB / {7:N1} GB) frei",
                                                                  vd.TotalBytes,
                                                                  vd.TotalBytes / 1024.0,
                                                                  vd.TotalBytes / (1024.0 * 1024.0),
                                                                  vd.TotalBytes / (1024.0 * 1024.0 * 1024.0),
                                                                  vd.AvailableBytes,
                                                                  vd.AvailableBytes / 1024.0,
                                                                  vd.AvailableBytes / (1024.0 * 1024.0),
                                                                  vd.AvailableBytes / (1024.0 * 1024.0 * 1024.0)), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("primär", vd.IsPrimary.ToString(), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("emuliert", vd.IsEmulated.ToString(), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("entfernbar", vd.IsRemovable.ToString(), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("Status", vd.State.ToString(), NAME2POS, CONTENT1POS));
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getFreeSpace() {
         StringBuilder sb = new StringBuilder();
         try {
            List<long> freeBytes = new List<long>();
            List<long> usableBytes = new List<long>();      // tatsächlich frei (nutzbar)
            List<long> totalBytes = new List<long>();
            List<string> sNames = new List<string>();
            //freeBytes.Add(context.FilesDir.FreeSpace);
            //totalBytes.Add(context.FilesDir.TotalSpace);
            //sNames.Add("primary");
            foreach (var item in context.GetExternalFilesDirs(null)) {
               freeBytes.Add(item.FreeSpace);
               usableBytes.Add(item.UsableSpace);
               totalBytes.Add(item.TotalSpace);
               sNames.Add(item.Path);
               // StatFs stat=new StatFs(file.getPath());
               // long availableSizeInBytes=stat.getBlockSize()*stat.getAvailableBlocks();
            }

            for (int i = 0; i < sNames.Count; i++) {
               if (i > 0)
                  sb.AppendLine("");
               sb.AppendLine(getItemString("Volume " + i.ToString() + ":", sNames[i], NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("Bytes gesamt", string.Format("{0} B ({1:N1} kb / {2:N1} MB / {3:N1} GB)",
                                                                  totalBytes[i],
                                                                  totalBytes[i] / 1024.0,
                                                                  totalBytes[i] / (1024.0 * 1024.0),
                                                                  totalBytes[i] / (1024.0 * 1024.0 * 1024.0)), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("Bytes frei", string.Format("{0} B ({1:N1} kb / {2:N1} MB / {3:N1} GB)",
                                                                  freeBytes[i],
                                                                  freeBytes[i] / 1024.0,
                                                                  freeBytes[i] / (1024.0 * 1024.0),
                                                                  freeBytes[i] / (1024.0 * 1024.0 * 1024.0)), NAME2POS, CONTENT1POS));
               sb.AppendLine(getItemString("Bytes nutzbar", string.Format("{0} B ({1:N1} kb / {2:N1} MB / {3:N1} GB)",
                                                                  usableBytes[i],
                                                                  usableBytes[i] / 1024.0,
                                                                  usableBytes[i] / (1024.0 * 1024.0),
                                                                  usableBytes[i] / (1024.0 * 1024.0 * 1024.0)), NAME2POS, CONTENT1POS));
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getBatteryInfo(Context context) {
         StringBuilder sb = new StringBuilder();
         try {
            Intent batteryIntent = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));

            bool present = batteryIntent.GetBooleanExtra(BatteryManager.ExtraPresent, false);         // indicating whether a battery is present
            sb.AppendLine(getItemString("vorhanden", present.ToString(), NAME1POS, CONTENT1POS));
            if (present) {
               Android.OS.BatteryPlugged plugged = (Android.OS.BatteryPlugged)batteryIntent.GetIntExtra(BatteryManager.ExtraPlugged, 0); // indicating whether the device is plugged in to a power source; 0 means it is on battery, other constants are different types of power sources
               int voltage = batteryIntent.GetIntExtra(BatteryManager.ExtraVoltage, 0);                  // current battery voltage level (off. mV)
               int temperature = batteryIntent.GetIntExtra(BatteryManager.ExtraTemperature, 0);          // current battery temperature      1/10°
               string technology = batteryIntent.GetStringExtra(BatteryManager.ExtraTechnology);         // String describing the technology of the current battery.    
               Android.OS.BatteryHealth health = (Android.OS.BatteryHealth)batteryIntent.GetIntExtra(BatteryManager.ExtraHealth, (int)Android.OS.BatteryHealth.Unknown);
               //int status2 = batteryIntent.GetIntExtra(BatteryManager.ExtraStatus, 0);                   // current status constant                                  <-> status
               //int scale = batteryIntent.GetIntExtra(BatteryManager.ExtraScale, 0);                      // maximum battery level
               //int level = batteryIntent.GetIntExtra(BatteryManager.ExtraLevel, 0);                      // current battery level, from 0 to EXTRA_SCALE             <-> capacity

               BatteryManager bm = context.GetSystemService(Android.Content.Context.BatteryService) as BatteryManager;
               int currentNow = bm.GetIntProperty((int)Android.OS.BatteryProperty.CurrentNow);           // Instantaneous battery current in microamperes, as an integer. 
               int currentAverage = bm.GetIntProperty((int)Android.OS.BatteryProperty.CurrentAverage);   // Average battery current in microamperes, as an integer.
               int energyCounter = bm.GetIntProperty((int)Android.OS.BatteryProperty.EnergyCounter);     // Battery remaining energy in nanowatt-hours, as a long integer. 
               int chargeCounter = bm.GetIntProperty((int)Android.OS.BatteryProperty.ChargeCounter);     // Battery capacity in microampere-hours, as an integer.  mAh ?? (https://source.android.com/devices/tech/power/device: "Remaining battery capacity in microampere-hours")
               Android.OS.BatteryStatus status = (Android.OS.BatteryStatus)bm.GetIntProperty((int)Android.OS.BatteryProperty.Status); // Battery charge status
               int capacity = bm.GetIntProperty((int)Android.OS.BatteryProperty.Capacity);               // 

               sb.AppendLine(getItemString("Technologie", technology, NAME1POS, CONTENT1POS));
               if (chargeCounter != Int32.MinValue) {
                  int maxCharge = Int32.MinValue;
                  if (capacity != Int32.MinValue)
                     maxCharge = 100 * (int)Math.Round(chargeCounter / (1000.0 * capacity));   // auf 100mAh gerundet
                  string txt = "noch " + (chargeCounter / 1000).ToString() + " mAh";
                  if (maxCharge != Int32.MinValue)
                     txt += " (gesamt etwa " + maxCharge.ToString() + " mAh)"; ;
                  sb.AppendLine(getItemString("Kapazität", txt, NAME1POS, CONTENT1POS));
               }
               if (capacity != Int32.MinValue)
                  sb.AppendLine(getItemString("Ladezustand", capacity.ToString() + " %", NAME1POS, CONTENT1POS));
               if (voltage != Int32.MinValue)
                  sb.AppendLine(getItemString("Spannung", string.Format("{0:F3} V", voltage / 1000.0), NAME1POS, CONTENT1POS));
               if (temperature != Int32.MinValue)
                  sb.AppendLine(getItemString("Temperatur", string.Format("{0:F1} °C", temperature / 10.0), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("an Stromquelle", (plugged > 0).ToString(), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("Status", status.ToString(), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("Zustand", health.ToString(), NAME1POS, CONTENT1POS));
               if (currentNow != Int32.MinValue)
                  sb.AppendLine(getItemString("akt. Stromstärke", string.Format("{0:F0} mA", currentNow / 1000.0), NAME1POS, CONTENT1POS));
               if (currentAverage != Int32.MinValue)
                  sb.AppendLine(getItemString("⌀ Stromstärke", string.Format("{0:F0} mA", currentAverage / 1000.0), NAME1POS, CONTENT1POS));
               if (energyCounter != Int32.MinValue)
                  sb.AppendLine(getItemString("verbleibende Kapazität", string.Format("{0:F0} mWh", energyCounter / 1000000.0), NAME1POS, CONTENT1POS));
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getInfoFromOSCommandOutput(string[] args) {
         StringBuilder sb = new StringBuilder();
         try {
            string prefix = new string(' ', NAME1POS);
            string[] txt = getOSCommandOutput(args).Split(System.Environment.NewLine);
            for (int i = 0; i < txt.Length; i++)
               txt[i] = prefix + txt[i];
            sb.Append(string.Join(System.Environment.NewLine, txt));
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getInfoFromTelecomManager(Context context) {
         StringBuilder sb = new StringBuilder();
         try {
            Android.Telecom.TelecomManager tm = context.GetSystemService(Context.TelecomService) as TelecomManager;
            if (tm != null) {
               if (tm.DefaultDialerPackage != null)
                  sb.AppendLine(getItemString("DefaultDialerPackage", tm.DefaultDialerPackage, NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("IsInCall", tm.IsInCall.ToString(), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("IsInManagedCall", tm.IsInManagedCall.ToString(), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("IsTtySupported", tm.IsTtySupported.ToString(), NAME1POS, CONTENT1POS));
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getBuildVersion() {
         StringBuilder sb = new StringBuilder();
         try {
            if (!string.IsNullOrEmpty(Build.VERSION.BaseOs))
               sb.AppendLine(getItemString("BaseOs", Build.VERSION.BaseOs, NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.VERSION.Codename))
               sb.AppendLine(getItemString("Codename", Build.VERSION.Codename, NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.VERSION.Incremental))
               sb.AppendLine(getItemString("Incremental", Build.VERSION.Incremental, NAME1POS, CONTENT1POS));
            sb.AppendLine(getItemString("PreviewSdkInt", Build.VERSION.PreviewSdkInt.ToString(), NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.VERSION.Release))
               sb.AppendLine(getItemString("Release", Build.VERSION.Release, NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.VERSION.Sdk))
               sb.AppendLine(getItemString("Sdk", Build.VERSION.Sdk, NAME1POS, CONTENT1POS));
            sb.AppendLine(getItemString("SdkInt", Build.VERSION.SdkInt.ToString(), NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.VERSION.SecurityPatch))
               sb.AppendLine(getItemString("SecurityPatch", Build.VERSION.SecurityPatch, NAME1POS, CONTENT1POS));
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getBuildInfos() {
         StringBuilder sb = new StringBuilder();
         try {
            IList<string> tmparr;

            if (!string.IsNullOrEmpty(Build.Manufacturer))
               sb.AppendLine(getItemString("Manufacturer:", Build.Manufacturer, NAME1POS, CONTENT1POS)); // The manufacturer of the product/hardware.
            if (!string.IsNullOrEmpty(Build.Product))
               sb.AppendLine(getItemString("Product:", Build.Product, NAME1POS, CONTENT1POS));          // The name of the overall product.
            if (!string.IsNullOrEmpty(Build.Model))
               sb.AppendLine(getItemString("Model:", Build.Model, NAME1POS, CONTENT1POS));              // The end-user-visible name for the end product.
            if (!string.IsNullOrEmpty(Build.Device))
               sb.AppendLine(getItemString("Device:", Build.Device, NAME1POS, CONTENT1POS));            // The name of the industrial design. 
            if (!string.IsNullOrEmpty(Build.Display))
               sb.AppendLine(getItemString("Display:", Build.Display, NAME1POS, CONTENT1POS));          // A build ID string meant for displaying to the user
            if (!string.IsNullOrEmpty(Build.Hardware))
               sb.AppendLine(getItemString("Hardware:", Build.Hardware, NAME1POS, CONTENT1POS));        // The name of the hardware (from the kernel command line or /proc).
            /*
             This field was deprecated in API level 26. Use getSerial() instead. -> Gets the hardware serial number, if available. 
             From Android 10, Build.GetSerial() returns Build.UNKNOWN. 
             */
            //tmpstr = Build.GetSerial();
            //if (!string.IsNullOrEmpty(tmpstr)) sb.AppendLine("Serial: " + tmpstr);
            if (!string.IsNullOrEmpty(Build.Brand))
               sb.AppendLine(getItemString("Brand:", Build.Brand, NAME1POS, CONTENT1POS));              // The consumer-visible brand with which the product/hardware will be associated, if any.

            if (!string.IsNullOrEmpty(Build.Board))
               sb.AppendLine(getItemString("Board:", Build.Board, NAME1POS, CONTENT1POS));              // The name of the underlying board, like "goldfish". 
            if (!string.IsNullOrEmpty(Build.Bootloader))
               sb.AppendLine(getItemString("Bootloader:", Build.Bootloader, NAME1POS, CONTENT1POS));    // The system bootloader version number.
            if (!string.IsNullOrEmpty(Build.Fingerprint))
               sb.AppendLine(getItemString("Fingerprint:", Build.Fingerprint, NAME1POS, CONTENT1POS));  // A string that uniquely identifies this build. (Get build information about partitions that have a separate fingerprint defined.)
            if (!string.IsNullOrEmpty(Build.Host))
               sb.AppendLine(getItemString("Host:", Build.Host, NAME1POS, CONTENT1POS));
            if (!string.IsNullOrEmpty(Build.Id))
               sb.AppendLine(getItemString("Id:", Build.Id, NAME1POS, CONTENT1POS));                    // Either a changelist number, or a label like "M4-rc20".
                                                                                                        // if (Build.Radio != null) sb.AppendLine("Radio: " + Build.Radio); // This field was deprecated in API level 15. The radio firmware version is frequently not available when this class is initialized, leading to a blank or "unknown" value for this string. Use getRadioVersion() instead.
            if (!string.IsNullOrEmpty(Build.RadioVersion))
               sb.AppendLine(getItemString("RadioVersion:", Build.RadioVersion, NAME1POS, CONTENT1POS)); // Returns the version string for the radio firmware.
            if (!string.IsNullOrEmpty(Build.Tags))
               sb.AppendLine(getItemString("Tags:", Build.Tags, NAME1POS, CONTENT1POS));                // Comma-separated tags describing the build, like "unsigned,debug".

            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            time = time.AddMilliseconds(Build.Time);                                                     // The time at which the build was produced, given in milliseconds since the UNIX epoch. 
            sb.AppendLine(getItemString("Time:", time.ToString("G"), NAME1POS, CONTENT1POS));

            if (!string.IsNullOrEmpty(Build.Type))
               sb.AppendLine(getItemString("Type:", Build.Type, NAME1POS, CONTENT1POS));                // The type of build, like "user" or "eng". 
            if (!string.IsNullOrEmpty(Build.User))
               sb.AppendLine(getItemString("User:", Build.User, NAME1POS, CONTENT1POS));
            // if (Build.CpuAbi != null) sb.AppendLine("CpuAbi: " + Build.CpuAbi); // This field was deprecated in API level 21. Use SUPPORTED_ABIS instead.
            // if (Build.CpuAbi2 != null) sb.AppendLine("CpuAbi2: " + Build.CpuAbi2); // This field was deprecated in API level 21. Use SUPPORTED_ABIS instead.
            tmparr = Build.SupportedAbis; // An ordered list of ABIs supported by this device. 
            if (tmparr != null && tmparr.Count > 0)
               sb.AppendLine(getItemString("SupportedAbis:", string.Join(';', tmparr), NAME1POS, CONTENT1POS));
            tmparr = Build.Supported32BitAbis; // An ordered list of 32 bit ABIs supported by this device. 
            if (tmparr != null && tmparr.Count > 0)
               sb.AppendLine(getItemString("Supported32BitAbis:", string.Join(';', tmparr), NAME1POS, CONTENT1POS));
            tmparr = Build.Supported64BitAbis; // An ordered list of 64 bit ABIs supported by this device. 
            if (tmparr != null && tmparr.Count > 0)
               sb.AppendLine(getItemString("Supported64BitAbis:", string.Join(';', tmparr), NAME1POS, CONTENT1POS));

         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getInfosFromTelephonyManager(Context context) {
         StringBuilder sb = new StringBuilder();
         try {

            Android.Telephony.TelephonyManager telm = context.GetSystemService(Android.Content.Context.TelephonyService) as Android.Telephony.TelephonyManager;
            if (telm != null) {

               if (!string.IsNullOrEmpty(telm.Line1Number))
                  sb.AppendLine(getItemString("Line1Number", telm.Line1Number, NAME1POS, CONTENT1POS));                        // Returns the phone number string for line 1, for example, the MSISDN for a GSM phone for a particular subscription. Return null if it is unavailable. 
               sb.AppendLine(getItemString("PhoneType", telm.PhoneType.ToString(), NAME1POS, CONTENT1POS));                    // Returns a constant indicating the device phone type. This indicates the type of radio used to transmit voice calls.
               sb.AppendLine(getItemString("PhoneCount", telm.PhoneCount.ToString(), NAME1POS, CONTENT1POS));                  // Returns the number of phones available. Returns 0 if none of voice, sms, data is not supported Returns 1 for Single standby mode (Single SIM functionality). Returns 2 for Dual standby mode (Dual SIM functionality). Returns 3 for Tri standby mode (Tri SIM functionality).
               if (!string.IsNullOrEmpty(telm.Imei))
                  sb.AppendLine(getItemString("Imei", telm.Imei, NAME1POS, CONTENT1POS));                                      // Returns the IMEI (International Mobile Equipment Identity). Return null if IMEI is not available.
               if (!string.IsNullOrEmpty(telm.Meid))
                  sb.AppendLine(getItemString("Meid", telm.Meid, NAME1POS, CONTENT1POS));                                      // Returns the MEID (Mobile Equipment Identifier). Return null if MEID is not available. 

               sb.AppendLine("");

               if (!string.IsNullOrEmpty(telm.SubscriberId))
                  sb.AppendLine(getItemString("SubscriberId", telm.SubscriberId, NAME1POS, CONTENT1POS));                      // Returns the unique subscriber ID, for example, the IMSI for a GSM phone. 
               sb.AppendLine(getItemString("SimState", telm.SimState.ToString(), NAME1POS, CONTENT1POS));                      // Returns a constant indicating the state of the default SIM card.
               if (!string.IsNullOrEmpty(telm.SimSerialNumber))
                  sb.AppendLine(getItemString("SimSerialNumber", telm.SimSerialNumber, NAME1POS, CONTENT1POS));                // Returns the serial number of the SIM, if applicable.
               if (!string.IsNullOrEmpty(telm.SimOperatorName))
                  sb.AppendLine(getItemString("SimOperatorName", telm.SimOperatorName, NAME1POS, CONTENT1POS));                // Returns the Service Provider Name (SPN). 
               if (!string.IsNullOrEmpty(telm.SimOperator))
                  sb.AppendLine(getItemString("SimOperator", telm.SimOperator, NAME1POS, CONTENT1POS));                        // Returns the MCC+MNC (mobile country code + mobile network code) of the provider of the SIM. 5 or 6 decimal digits. 
               if (!string.IsNullOrEmpty(telm.SimCountryIso))
                  sb.AppendLine(getItemString("SimCountryIso", telm.SimCountryIso, NAME1POS, CONTENT1POS));                    // Returns the ISO-3166-1 alpha-2 country code equivalent for the SIM provider's country code. 
               if (!string.IsNullOrEmpty(telm.SimCarrierIdName))
                  sb.AppendLine(getItemString("SimCarrierIdName", telm.SimCarrierIdName, NAME1POS, CONTENT1POS));              // Returns carrier id name of the current subscription. 
               sb.AppendLine(getItemString("SimCarrierId", telm.SimCarrierId.ToString(), NAME1POS, CONTENT1POS));              // Returns carrier id of the current subscription. The carrier ID is an Android platform-wide identifier for a carrier.

               sb.AppendLine("");

               sb.AppendLine(getItemString("NetworkType", telm.NetworkType.ToString(), NAME1POS, CONTENT1POS));                // Return the current data network type.
               if (!string.IsNullOrEmpty(telm.NetworkSpecifier))
                  sb.AppendLine(getItemString("NetworkSpecifier", telm.NetworkSpecifier.ToString(), NAME1POS, CONTENT1POS));   // Returns the network specifier of the subscription ID pinned to the TelephonyManager. 
               if (!string.IsNullOrEmpty(telm.NetworkOperatorName))
                  sb.AppendLine(getItemString("NetworkOperatorName", telm.NetworkOperatorName, NAME1POS, CONTENT1POS));        // Returns the alphabetic name of current registered operator. 
               if (!string.IsNullOrEmpty(telm.NetworkOperator))
                  sb.AppendLine(getItemString("NetworkOperator", telm.NetworkOperator, NAME1POS, CONTENT1POS));                // Returns the numeric name (MCC+MNC) of current registered operator. 
               if (!string.IsNullOrEmpty(telm.NetworkCountryIso))
                  sb.AppendLine(getItemString("NetworkCountryIso", telm.NetworkCountryIso, NAME1POS, CONTENT1POS));            // Returns the ISO-3166-1 alpha-2 country code equivalent of the MCC (Mobile Country Code) of the current registered operator or the cell nearby, if available. 
               if (!string.IsNullOrEmpty(telm.Nai))
                  sb.AppendLine(getItemString("Nai", telm.Nai, NAME1POS, CONTENT1POS));                                        // Returns the Network Access Identifier (NAI). Return null if NAI is not available. 
               sb.AppendLine(getItemString("IsNetworkRoaming", telm.IsNetworkRoaming.ToString(), NAME1POS, CONTENT1POS));      // Returns true if the device is considered roaming on the current network, for GSM purposes. 

               sb.AppendLine("");

               sb.AppendLine(getItemString("DataState", telm.DataState.ToString(), NAME1POS, CONTENT1POS));                    // Returns a constant indicating the current data connection state (cellular).
               sb.AppendLine(getItemString("DataNetworkType", telm.DataNetworkType.ToString(), NAME1POS, CONTENT1POS));        // Returns a constant indicating the radio technology (network type) currently in use on the device for data transmission. DATA_DISCONNECTEDDATA_CONNECTING,DATA_CONNECTED,DATA_SUSPENDED,DATA_DISCONNECTING
               sb.AppendLine(getItemString("DataEnabled", telm.DataEnabled.ToString(), NAME1POS, CONTENT1POS));                // Returns whether mobile data is enabled or not per user setting. There are other factors that could disable mobile data, but they are not considered here.
               sb.AppendLine(getItemString("DataActivity", telm.DataActivity.ToString(), NAME1POS, CONTENT1POS));              // Returns a constant indicating the type of activity on a data connection (cellular). DATA_ACTIVITY_NONE,DATA_ACTIVITY_IN,DATA_ACTIVITY_OUT,DATA_ACTIVITY_INOUT,DATA_ACTIVITY_DORMANT

               sb.AppendLine("");

               if (!string.IsNullOrEmpty(telm.MmsUserAgent))
                  sb.AppendLine(getItemString("MmsUserAgent", telm.MmsUserAgent, NAME1POS, CONTENT1POS));                      // Returns the MMS user agent.
               if (!string.IsNullOrEmpty(telm.MmsUAProfUrl))
                  sb.AppendLine(getItemString("MmsUAProfUrl", telm.MmsUAProfUrl, NAME1POS, CONTENT1POS));                      // Returns the MMS user agent profile URL.
               sb.AppendLine(getItemString("IsWorldPhone", telm.IsWorldPhone.ToString(), NAME1POS, CONTENT1POS));              // Whether the device is a world phone.
               sb.AppendLine(getItemString("IsVoiceCapable", telm.IsVoiceCapable.ToString(), NAME1POS, CONTENT1POS));          // "Voice capable" means that this device supports circuit-switched (i.e. voice) phone calls over the telephony network, and is allowed to display the in-call UI while a cellular voice call is active. This will be false on "data only" devices which can't make voice calls and don't support any in-call UI. 
               sb.AppendLine(getItemString("IsSmsCapable", telm.IsSmsCapable.ToString(), NAME1POS, CONTENT1POS));              // If true, this means that the device supports both sending and receiving sms via the telephony network. 
               sb.AppendLine(getItemString("IsHearingAidCompatibilitySupported", telm.IsHearingAidCompatibilitySupported.ToString(), NAME1POS, CONTENT1POS));   // Whether the phone supports hearing aid compatibility.
               sb.AppendLine(getItemString("IsConcurrentVoiceAndDataSupported", telm.IsConcurrentVoiceAndDataSupported.ToString(), NAME1POS, CONTENT1POS));  // Whether the device is currently on a technology (e.g. UMTS or LTE) which can support voice and data simultaneously. 
               sb.AppendLine(getItemString("HasIccCard", telm.HasIccCard.ToString(), NAME1POS, CONTENT1POS));
               sb.AppendLine(getItemString("HasCarrierPrivileges", telm.HasCarrierPrivileges.ToString(), NAME1POS, CONTENT1POS)); // Has the calling application been granted carrier privileges by the carrier.
               if (!string.IsNullOrEmpty(telm.GroupIdLevel1))
                  sb.AppendLine(getItemString("GroupIdLevel1", telm.GroupIdLevel1, NAME1POS, CONTENT1POS));                    // Returns the Group Identifier Level1 for a GSM phone. Return null if it is unavailable. 
               if (!string.IsNullOrEmpty(telm.DeviceSoftwareVersion))
                  sb.AppendLine(getItemString("DeviceSoftwareVersion", telm.DeviceSoftwareVersion, NAME1POS, CONTENT1POS));    // Returns the software version number for the device, for example, the IMEI/SV for GSM phones. Return null if the software version is not available. 

               sb.AppendLine("");

               sb.AppendLine(getItemString("CallState", telm.CallState.ToString(), NAME1POS, CONTENT1POS));                    // Returns the state of all calls on the device. 
               sb.AppendLine(getItemString("CanChangeDtmfToneLength", telm.CanChangeDtmfToneLength().ToString(), NAME1POS, CONTENT1POS)); // 
               if (!string.IsNullOrEmpty(telm.VisualVoicemailPackageName))
                  sb.AppendLine(getItemString("VisualVoicemailPackageName", telm.VisualVoicemailPackageName, NAME1POS, CONTENT1POS));     // 
               if (!string.IsNullOrEmpty(telm.VoiceMailAlphaTag))
                  sb.AppendLine(getItemString("VoiceMailAlphaTag", telm.VoiceMailAlphaTag, NAME1POS, CONTENT1POS));                       // 
               if (!string.IsNullOrEmpty(telm.VoiceMailNumber))
                  sb.AppendLine(getItemString("VoiceMailNumber", telm.VoiceMailNumber, NAME1POS, CONTENT1POS));                           // 
               sb.AppendLine(getItemString("VoiceNetworkType", telm.VoiceNetworkType.ToString(), NAME1POS, CONTENT1POS));                 // 

               sb.AppendLine("");

               if (telm.SignalStrength != null)
                  sb.AppendLine(getItemString("SignalStrength", telm.SignalStrength.ToString(), NAME1POS, CONTENT1POS));       // Get the most recent SignalStrength information reported by the modem. Due to power saving this information may not always be current.
               if (telm.ServiceState != null)
                  sb.AppendLine(getItemString("ServiceState", telm.ServiceState.ToString(), NAME1POS, CONTENT1POS));           // Returns the current ServiceState information.

               sb.AppendLine("");

               foreach (var item in telm.AllCellInfo) {
                  sb.AppendLine(getItemString("CellInfo.CellConnectionStatus", item.CellConnectionStatus.ToString(), NAME1POS, CONTENT1POS));
                  sb.AppendLine(getItemString("CellInfo.IsRegistered", item.IsRegistered.ToString(), NAME1POS, CONTENT1POS));
                  TimeSpan ts = new TimeSpan(item.TimeStamp / 100);
                  sb.AppendLine(getItemString("CellInfo.TimeStamp", ts.Days.ToString() + " Tage, " +
                                                                    ts.Hours.ToString() + " Stunden, " +
                                                                    ts.Minutes.ToString() + " Minuten, " +
                                                                    ts.Seconds.ToString() + " Sekunden", NAME1POS, CONTENT1POS));
               }
            }

         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }



      //string x() {
      //   StringBuilder sb = new StringBuilder();
      //   try {

      //   } catch (Exception ex) {
      //      sb.AppendLine("Exception: " + ex.Message);
      //   }
      //   return sb.ToString();
      //}

      /*
      fkt. nur als DeviceOwner

      sb.AppendLine("");
      sb.AppendLine("HardwarePropertiesManager:");
      try {
         Android.OS.HardwarePropertiesManager hwm = global::Android.App.Application.Context.GetSystemService(Android.Content.Context.HardwarePropertiesService) as Android.OS.HardwarePropertiesManager;

         CpuUsageInfo[] cpuUsageInfos = hwm.GetCpuUsages();
         if (cpuUsageInfos != null) {
            for (int i = 0; i < cpuUsageInfos.Length; i++)
               sb.AppendLine(getItemString("CpuUsageInfo:", cpuUsageInfos[i].ToString(), NAME1POS, CONTENTPOS));
         }

         float[] fanSpeeds = hwm.GetFanSpeeds();
         if (fanSpeeds != null && fanSpeeds.Length > 0) {
            for (int i = 0; i < fanSpeeds.Length; i++)
               sb.AppendLine(getItemString("FanSpeeds:", fanSpeeds[i].ToString(), NAME1POS, CONTENTPOS));
         }
         //float[] hwm.GetDeviceTemperatures();
      } catch (Exception ex) {
         sb.AppendLine("Exception: " + ex.Message);
      }
      */

      public string[] InfoGroupNames() {
         return new string[] {
               "Build-Version",
               "Build",
               "Hauptspeicher",
               "Speicher",
               "Speicher 2",
               "Batterie",
               "/proc/cpuinfo",
               "/proc/meminfo",
               "/proc/version",
               "/system/bin/free",
               "/system/bin/uname",
               "TelephonyManager",
               "TelecomManager",
            };
      }

      public string Info(int group) {
         StringBuilder sb = new StringBuilder();

         try {
            switch (group) {
               case 0:
                  sb.AppendLine("Build-Version:");
                  sb.AppendLine(getBuildVersion());
                  break;

               case 1:
                  sb.AppendLine("Build:");
                  sb.Append(getBuildInfos());
                  break;

               case 2:
                  sb.AppendLine("Hauptspeicher:");
                  sb.Append(getRam(context));
                  break;

               case 3:
                  sb.AppendLine("Speicher:");
                  sb.Append(getVolumeInfos(activity));
                  break;

               case 4:
                  sb.AppendLine("Speicher 2:");
                  sb.Append(getFreeSpace());
                  break;

               case 5:
                  sb.AppendLine("Batterie:");
                  sb.Append(getBatteryInfo(context));
                  break;

               case 6:
                  sb.AppendLine("/proc/cpuinfo:");
                  sb.Append(getInfoFromOSCommandOutput(new string[] { "/system/bin/cat", "/proc/cpuinfo" }));
                  break;

               case 7:
                  sb.AppendLine("/proc/meminfo:");
                  sb.Append(getInfoFromOSCommandOutput(new string[] { "/system/bin/cat", "/proc/meminfo" }));
                  break;

               case 8:
                  sb.AppendLine("/proc/version:");
                  sb.Append(getInfoFromOSCommandOutput(new string[] { "/system/bin/cat", "/proc/version" }));
                  break;

               case 9:
                  sb.AppendLine("/system/bin/free:");
                  sb.Append(getInfoFromOSCommandOutput(new string[] { "/system/bin/free" }));
                  break;

               case 10:
                  sb.AppendLine("/system/bin/uname:");
                  sb.Append(getInfoFromOSCommandOutput(new string[] { "/system/bin/uname", "-a" }));
                  break;

               case 11:
                  sb.AppendLine("TelephonyManager:");
                  sb.Append(getInfosFromTelephonyManager(context));
                  break;

               case 12:
                  sb.AppendLine("TelecomManager:");
                  sb.AppendLine(getInfoFromTelecomManager(context));
                  break;
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }

         return sb.ToString();
      }

      public string Info() {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine("Android-Infos:");
         try {
            string[] name = InfoGroupNames();
            for (int i = 0; i < name.Length; i++) {
               sb.AppendLine("");
               sb.Append(Info(i));
            }
         } catch (Exception ex) {
            sb.AppendLine("Exception: " + ex.Message);
         }
         return sb.ToString();
      }

      string getItemString(string name, string content, int namepos, int contentpos) {
         StringBuilder sb = new StringBuilder();
         if (namepos > 0)
            sb.Append(new string(' ', namepos));
         sb.Append(name);
         if (sb.Length < contentpos)
            sb.Append(new string(' ', contentpos - sb.Length));
         sb.Append(content);
         return sb.ToString();
      }

      string getOSCommandOutput(string[] args) {
         Java.Lang.ProcessBuilder pb = new Java.Lang.ProcessBuilder(args);
         Java.Lang.Process process = pb.Start();
         if (process.InputStream != null)
            using (StreamReader reader = new StreamReader(process.InputStream)) {
               return reader.ReadToEnd();
            }
         return null;
      }

   }
}