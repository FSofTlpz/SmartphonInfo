# SmartphonInfo
A demo how to get infos about smartphon with Xamarin/Android 

The App get infos about the Volumes, RAM, Battery, CPU, SIM-Card, Telephon-Number, ...
- get infos with Xamarin.Essentials
- Android-Infos with Android.OS.Build, Android.Content.Context.StorageService, Android.OS.BatteryProperty, a IntentFilter(Intent.ActionBatteryChanged), Android.Telecom.TelecomManager, Android.Telephony.TelephonyManager
- Android-Infos from files (/proc/cpuinfo, /proc/meminfo) and internal commands (/system/bin/free, /system/bin/uname")

The App shows, how to get Permissons and how to use Dependency-Service.

It use a multiline Label (for the infos) wehre you can copy parts of text to the clipboard (thanks to Anna Domashych https://medium.com/@anna.domashych/selectable-read-only-multiline-text-field-in-xamarin-forms-69d09276d580)
