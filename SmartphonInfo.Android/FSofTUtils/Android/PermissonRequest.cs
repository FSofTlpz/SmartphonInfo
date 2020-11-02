using System.Collections.Generic;
using Android.Content.PM;
using Android.Runtime;
using Android.Support.V4.App;
using Java.Lang;

namespace FSofTUtils.Android {
   class PermissonRequest {

      const int REQUEST_PERMISSIONS_BASE = 100;

      /// <summary>
      /// Liste der gewünschten Permissions
      /// </summary>
      public readonly List<string> Permissions;

      /// <summary>
      /// Liste der Ergebnisse für die Permissions
      /// </summary>
      List<bool> IsPresent;


      /// <summary>
      ///  Liste der Permissions: https://developer.android.com/reference/android/Manifest.permission.html
      /// </summary>
      /// <param name="permissions">Liste der gewünschten Permissions</param>
      public PermissonRequest(IList<string> permissions) {
         Permissions = new List<string>(permissions);
         IsPresent = new List<bool>();
         for (int i = 0; i < Permissions.Count; i++)
            IsPresent.Add(false);
      }

      List<string> neededperms = new List<string>();

      /// <summary>
      /// sollte in MainActivity.OnCreate vor dem LoadApplication() eingebunden werden
      /// </summary>
      /// <param name="mainActivity"></param>
      public void CheckAndRequest(global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity mainActivity) {
         neededperms.Clear();
         for (int i = 0; i < Permissions.Count; i++) {
            if (mainActivity.CheckSelfPermission(Permissions[i]) == Permission.Denied)   // wenn noch nicht vorhanden, dann anforden
               neededperms.Add(Permissions[i]);
            else
               IsPresent[i] = true;
         }

         if (neededperms.Count > 0) {
            string[] perms = new string[neededperms.Count];
            for (int i = 0; i < neededperms.Count; i++)
               perms[i] = neededperms[i];
            ActivityCompat.RequestPermissions(mainActivity,
                                              perms,
                                              REQUEST_PERMISSIONS_BASE);
         }
      }

      /// <summary>
      /// muss in MainActivity.OnRequestPermissionsResult() eingebunden werden
      /// </summary>
      /// <param name="requestCode"></param>
      /// <param name="grantResults"></param>
      public void OnRequestPermissionsResult(int requestCode, [GeneratedEnum] Permission[] grantResults) {
         if (REQUEST_PERMISSIONS_BASE == requestCode) {
            for (int i = 0; i < grantResults.Length; i++) {
               if (grantResults[i] == Permission.Granted) {
                  int idx = idx4Permission(neededperms[i]);
                  if (idx >= 0)
                     IsPresent[idx] = true;
               }
            }
         }
      }

      /// <summary>
      /// Ist die Permission erteilt?
      /// </summary>
      /// <param name="permission"></param>
      /// <returns></returns>
      public bool Present(string permission) {
         int idx = idx4Permission(permission);
         if (idx >= 0)
            return IsPresent[idx];
         return false;
      }

      int idx4Permission(string permission) {
         if (Permissions.Contains(permission)) {
            for (int i = 0; i < Permissions.Count; i++)
               if (Permissions[i] == permission)
                  return i;
         }
         return -1;
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append(Permissions.Count.ToString() + " Permissions:");
         for (int i = 0; i < Permissions.Count; i++) {
            sb.Append(" ");
            sb.Append(Permissions[i]);
            sb.Append("=");
            sb.Append(IsPresent[i].ToString());
         }
         return sb.ToString();
      }


   }
}