using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.OS.Storage;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Provider;

namespace FSofTUtils.Android {

   /// <summary>
   /// StorageVolumes und StorageVolume erst ab API 24 (Nougat, 7.0)
   /// </summary>
   public class StorageHelperAndroid {

      /*    Achtung
       *    
       *    Auf dem dem primary external storage (emuliert; interne SD-Karte) werden die Funktionen mit URI, DocumentsContract und DocumentFile NICHT benötigt 
       *    (und funktionieren auch nicht). Hier sollten alle normalen .Net-Funktionen funktionieren.
       *    
       *    Erst auf dem secondary external storage (echte SD-Karte und/oder USB-Stick) sind diese Funktionen nötig. Dafür sind auch nochmal zusätzliche Rechte erforderlich.
       *    "PersistentPermissions".
       */

      /// <summary>
      /// Hilfsfunktionen für den Secondary External Storage
      /// </summary>
      class SecondaryExternalStorageHelper {

         const string AUTHORITY_EXTERNALSTORAGE_DOCUMENTS = "com.android.externalstorage.documents";
         
         readonly Activity Activity;


         /// <summary>
         /// Hilfsfunktionen für den Secondary External Storage
         /// </summary>
         /// <param name="activity"></param>
         public SecondaryExternalStorageHelper(Activity activity) {
            if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Lollipop)  // "Lollipop", 5.0
               throw new Exception("Need Version 5.0 ('Lollipop', API 21) or higher.");
            Activity = activity as Activity;
         }

         /// <summary>
         /// liefert die Data- und Cache-Pfade (Index 0 immer mit den internen Pfaden)
         /// <para>Nicht für jedes Volume existieren diese Pfade!</para>
         /// </summary>
         /// <param name="DataPaths"></param>
         /// <param name="CachePaths"></param>
         public void GetSpecPaths(out List<string> DataPaths, out List<string> CachePaths) {
            DataPaths = new List<string>();
            CachePaths = new List<string>();

            DataPaths.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            Java.IO.File[] paths = Activity.ApplicationContext.GetExternalFilesDirs("");
            for (int i = 0; i < paths.Length; i++)
               DataPaths.Add(paths[i].CanonicalPath);

            CachePaths.Add(System.IO.Path.GetTempPath());
            paths = Activity.ApplicationContext.GetExternalCacheDirs();
            for (int i = 0; i < paths.Length; i++)
               CachePaths.Add(paths[i].CanonicalPath);
         }


         /// <summary>
         /// z.B.: //com.android.externalstorage.documents/tree/19F4-0903:abc/def/ghi.txt aus /abc/def/ghi.txt (API level 21 / Lollipop / 5.0)
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <returns></returns>
         global::Android.Net.Uri GetTreeDocumentUri(string storagename, string volpath = "") {
            if (volpath.Length > 0 &&
                volpath[0] == '/')
               volpath = volpath.Substring(1);
            // Build URI representing access to descendant documents of the given Document#COLUMN_DOCUMENT_ID. (API level 21 / Lollipop / 5.0)
            return DocumentsContract.BuildTreeDocumentUri(AUTHORITY_EXTERNALSTORAGE_DOCUMENTS, storagename + ":" + volpath);
         }

         /// <summary>
         /// z.B.: //com.android.externalstorage.documents/tree/19F4-0903:/document/19F4-0903:abc/def/ghi.txt aus /abc/def/ghi.txt (API level 21 / Lollipop / 5.0)
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <returns></returns>
         global::Android.Net.Uri GetDocumentUriUsingTree(string storagename, string volpath) {
            if (volpath.Length > 0 &&
                volpath[0] == '/')
               volpath = volpath.Substring(1);

            // Build URI representing the target Document#COLUMN_DOCUMENT_ID in a document provider. (API level 21 / Lollipop / 5.0)
            return DocumentsContract.BuildDocumentUriUsingTree(GetTreeDocumentUri(storagename), storagename + ":" + volpath);
         }

         /// <summary>
         /// liefert das DocumentFile für eine Datei oder ein Verzeichnis, wenn es existiert (sonst null) (API level 14)
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <returns></returns>
         public DocumentFile GetExistingDocumentFile(string storagename, string volpath) {
            global::Android.Net.Uri rooturi = GetTreeDocumentUri(storagename);
            DocumentFile document = DocumentFile.FromTreeUri(Activity.ApplicationContext, rooturi);

            string[] pathelems = volpath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; document != null && i < pathelems.Length; i++) {
               DocumentFile nextDocument = document.FindFile(pathelems[i]);
               document = nextDocument;
            }

            System.Diagnostics.Debug.WriteLine("AndroidGetExistingDocumentFile(" + storagename + ", " + volpath + ") = " + (document != null).ToString());

            return document;
         }

         /// <summary>
         /// liefert das DocumentFile für eine Datei oder ein Verzeichnis (API level 14)
         /// <para>Wenn das Objekt noch nicht ex., wird es erzeugt. Auch ein notwendiger Pfad wird vollständig erzeugt.</para>
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <param name="isDirectory">Pfad bezieht sich auf eine Datei oder ein Verzeichnis</param>
         /// <returns></returns>
         public DocumentFile GetDocumentFile(string storagename, string volpath, bool isDirectory) {
            global::Android.Net.Uri rooturi = GetTreeDocumentUri(storagename);
            DocumentFile document = DocumentFile.FromTreeUri(Activity.ApplicationContext, rooturi);

            string[] pathelems = volpath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; document != null && i < pathelems.Length; i++) {
               DocumentFile nextDocument = document.FindFile(pathelems[i]);
               if (nextDocument == null) {
                  if ((i < pathelems.Length - 1) || isDirectory) {
                     nextDocument = document.CreateDirectory(pathelems[i]);
                  } else {
                     nextDocument = document.CreateFile("", pathelems[i]);
                  }
               }
               document = nextDocument;
            }

            System.Diagnostics.Debug.WriteLine("AndroidGetDocumentFile(" + storagename + ", " + volpath + ", " + isDirectory.ToString() + ") = " + (document != null && (isDirectory == document.IsDirectory)).ToString());

            return document != null ?
                     (isDirectory == document.IsDirectory ? document : null) :
                     null;
         }

         /// <summary>
         /// löscht das Objekt
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <returns></returns>
         public bool Delete(string storagename, string volpath) {
            DocumentFile doc = GetExistingDocumentFile(storagename, volpath);
            bool ok = doc != null && doc.Delete();

            System.Diagnostics.Debug.WriteLine("AndroidDelete(" + storagename + ", " + volpath + ") = " + ok.ToString());

            return ok;
         }

         /// <summary>
         /// erzeugt ein Verzeichnis
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <returns></returns>
         public bool CreateDirectory(string storagename, string volpath) {
            return GetDocumentFile(storagename, volpath, true) != null;
         }


         /// <summary>
         /// Die Variante mit File-Uri fkt. nur im primary external storage und der Stream ist genauso eingeschränkt wie bei der Content-Uri.
         /// </summary>
         /// <param name="fullpath"></param>
         /// <param name="mode"></param>
         /// <returns></returns>
         //public Stream CreateOpenFile(string fullpath, string mode) {
         //   Android.Net.Uri uri = Android.Net.Uri.FromFile(new Java.IO.File(fullpath));
         //   return mode == "r" ?
         //               Activity.ContentResolver.OpenInputStream(uri) :
         //               Activity.ContentResolver.OpenOutputStream(uri, mode);
         //}

         /// <summary>
         /// liefert einen Dateistream (erzeugt die Datei bei Bedarf)
         /// <para>ACHTUNG: Der Stream erfüllt nur primitivste Bedingungen. Er ist nicht "seekable", es wird keine Position oder Länge geliefert.
         /// Auch ein "rwt"-Stream scheint nur beschreibbar zu sein.</para>
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <param name="mode">May be "w", "wa", "rw", or "rwt". This value must never be null. Zusatz: "r" verwendet OpenInputStream()</param>
         /// <returns></returns>
         public Stream CreateOpenFile(string storagename, string volpath, string mode) {
            DocumentFile doc = GetDocumentFile(storagename, volpath, false);
            if (doc != null) {
               /*
               InputStream openInputStream(Uri uri)                        Open a stream on to the content associated with a content URI. 
                                                                           If there is no data associated with the URI, FileNotFoundException is thrown. 
               Accepts the following URI schemes:
                  content(SCHEME_CONTENT)
                  android.resource (SCHEME_ANDROID_RESOURCE)
                     A Uri object can be used to reference a resource in an APK file. 
                        z.B.:
                        Uri uri = Uri.parse("android.resource://com.example.myapp/" + R.raw.my_resource");
                        Uri uri = Uri.parse("android.resource://com.example.myapp/raw/my_resource");
                  file(SCHEME_FILE)
               
               OutputStream openOutputStream(Uri uri)                      Synonym for openOutputStream(uri, "w")
               OutputStream openOutputStream(Uri uri, String mode)         Open a stream on to the content associated with a content URI. 
                                                                           If there is no data associated with the URI, FileNotFoundException is thrown. 
                                                                           mode: May be "w", "wa", "rw", or "rwt". This value must never be null.
               Accepts the following URI schemes:
                  content(SCHEME_CONTENT)
                  file(SCHEME_FILE)
               */
               return mode == "r" ?
                           Activity.ContentResolver.OpenInputStream(doc.Uri) :
                           Activity.ContentResolver.OpenOutputStream(doc.Uri, mode);
            }
            return null;
         }

         /// <summary>
         /// Ändert den Objektnamen (API level 14)
         /// </summary>
         /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpath">abs. Pfad im Volume</param>
         /// <param name="newfilename"></param>
         /// <returns></returns>
         public bool Rename(string storagename, string volpath, string newfilename) {
            if (newfilename.Contains('/'))
               throw new Exception("Path for newname is not allowed.");
            /*
               boolean renameTo (String displayName)

               Renames this file to displayName.
               Note that this method does not throw IOException on failure. Callers must check the return value.
               Some providers may need to create a new document to reflect the rename, potentially with a different MIME type, so getUri() and getType() may change to reflect the rename.
               When renaming a directory, children previously enumerated through listFiles() may no longer be valid.

               Parameters
               displayName 	String: the new display name.

               Returns
               boolean 	true on success.
            */
            DocumentFile doc = GetExistingDocumentFile(storagename, volpath);
            bool ok = doc.RenameTo(newfilename);

            System.Diagnostics.Debug.WriteLine("AndroidRename(" + storagename + ", " + volpath + ", " + newfilename + ") = " + ok.ToString());

            return ok;
         }

         /// <summary>
         /// verschiebt das Objekt (API level 14)
         /// </summary>
         /// <param name="storagenamesrc">z.B. "primary" oder "19F4-0903"</param>
         /// <param name="volpathsrc">abs. Pfad im Volume</param>
         /// <param name="storagenamenewparent">Zielvolume</param>
         /// <param name="volpathnewparent">Zielverzeichnis</param>
         /// <returns></returns>
         public bool Move(string storagenamesrc, string volpathsrc, string storagenamenewparent, string volpathnewparent) {
            /*
            DocumentFile srcdoc = GetExistingDocumentFile(volpathsrc);
            DocumentFile dstdoc = GetDocumentFile(volpathdst, true); // Zielverzeichnis (wird notfalls erzeugt)
            Android.Net.Uri srcuri = srcdoc.Uri;
            Android.Net.Uri srcparenturi = srcdoc.ParentFile.Uri;
            Android.Net.Uri dstparenturi = dstdoc.Uri;
            */
            DocumentFile dstdoc = GetDocumentFile(storagenamesrc, volpathnewparent, true); // Zielverzeichnis (wird notfalls erzeugt)

            /*
            public static Uri moveDocument (ContentResolver content, 
                                            Uri sourceDocumentUri,            document with FLAG_SUPPORTS_MOVE
                                            Uri sourceParentDocumentUri,      parent document of the document to move
                                            Uri targetParentDocumentUri)      document which will become a new parent of the source document
            return the moved document, or null if failed.

            Moves the given document under a new parent.
            Added in API level 24; Deprecated in API level Q

            neu:
            ContentProviderClient provclient = Activity.ContentResolver.AcquireUnstableContentProviderClient(srcdoc.Uri.Authority);
            DocumentsContract.MoveDocument(provclient, srcdoc.Uri, srcparenturi.Uri, dstparentdoc.Uri);

                  intern:
                  Bundle inbundle = new Bundle();
                  inbundle.PutParcelable(DocumentsContract.EXTRA_URI, srcdoc.Uri);                    "uri"
                  inbundle.PutParcelable(DocumentsContract.EXTRA_PARENT_URI, srcparenturi.Uri);       "parentUri"
                  inbundle.PutParcelable(DocumentsContract.EXTRA_TARGET_URI, dstparentdoc.Uri);       "android.content.extra.TARGET_URI"
                  Bundle outbundle = provclient.Call(METHOD_MOVE_DOCUMENT, null, inbundle);           "android:moveDocument"
                  return outbundle.GetParcelable(DocumentsContract.EXTRA_URI);                        "uri"
            */
            global::Android.Net.Uri newuri = DocumentsContract.MoveDocument(Activity.ContentResolver,
                                                                            GetDocumentUriUsingTree(storagenamesrc, volpathsrc),
                                                                            GetDocumentUriUsingTree(storagenamenewparent, Path.GetDirectoryName(volpathsrc)),
                                                                            dstdoc.Uri);

            System.Diagnostics.Debug.WriteLine("AndroidMove(" + storagenamesrc + ", " + volpathsrc + ", " + storagenamenewparent + ", " + volpathnewparent + ") = " + (newuri != null).ToString());

            return newuri != null;
         }

         public List<string> ObjectList(string storagename, string volpath, bool dir) {
            List<string> lst = new List<string>();
            DocumentFile doc = GetExistingDocumentFile(storagename, volpath);
            if (doc != null) {
               // Returns an array of files contained in the directory represented by this file.
               foreach (DocumentFile file in doc.ListFiles()) {
                  if (dir && file.IsDirectory)
                     lst.Add(file.Name);
                  if (!dir && file.IsFile)
                     lst.Add(file.Name);
               }
            }
            return lst;
         }

         public bool ObjectExists(string storagename, string volpath, bool dir) {
            DocumentFile doc = GetExistingDocumentFile(storagename, volpath);
            if (doc != null) {
               if (dir && doc.IsDirectory)
                  return true;
               if (!dir && doc.IsFile)
                  return true;
            }
            return false;
         }

         /// <summary>
         /// liefert selbst innerhalb des secondary ext. storage bei Oreo eine Exception: "Copy not supported"
         /// <para>Das scheint eine Exception der abstrakten Klasse DocumentProvider zu sein, d.h. die Copy-Funktion wurd in der konkreten Klasse nicht implemetiert.
         /// Das Flag FLAG_SUPPORTS_COPY für Dokumente fehlt (deswegen).</para>
         /// </summary>
         /// <param name="storagenamesrc"></param>
         /// <param name="volpathsrc"></param>
         /// <param name="storagenamenewparent"></param>
         /// <param name="volpathnewparent"></param>
         /// <returns></returns>
         public bool Copy(string storagenamesrc, string volpathsrc, string storagenamenewparent, string volpathnewparent) {
            DocumentFile srcdoc = GetExistingDocumentFile(storagenamesrc, volpathsrc);
            DocumentFile dstparentdoc = GetDocumentFile(storagenamenewparent, volpathnewparent, true); // Zielverzeichnis (wird notfalls erzeugt)
            /*    Added in API level 24; Nougat, 7.0, Deprecated in API level Q

                  public static Uri copyDocument (ContentResolver content, 
                                                  Uri sourceDocumentUri,            Document mit FLAG_SUPPORTS_COPY
                                                  Uri targetParentDocumentUri)

            neu:
            ContentProviderClient provclient = Activity.ContentResolver.AcquireUnstableContentProviderClient(srcdoc.Uri.Authority);
            DocumentsContract.CopyDocument(provclient, srcdoc.Uri, dstparentdoc.Uri);

                  intern:
                  Bundle inbundle = new Bundle();
                  inbundle.PutParcelable(DocumentsContract.EXTRA_URI, srcdoc.Uri);                    "uri"
                  inbundle.PutParcelable(DocumentsContract.EXTRA_TARGET_URI, dstparentdoc.Uri);       "android.content.extra.TARGET_URI"
                  Bundle outbundle = provclient.Call(METHOD_COPY_DOCUMENT, null, inbundle);           "android:copyDocument"
                  return outbundle.GetParcelable(DocumentsContract.EXTRA_URI);                        "uri"
            */
            global::Android.Net.Uri newuri = DocumentsContract.CopyDocument(Activity.ContentResolver,
                                                                            srcdoc.Uri,
                                                                            dstparentdoc.Uri);

            System.Diagnostics.Debug.WriteLine("AndroidCopy(" + storagenamesrc + ", " + volpathsrc + ", " + storagenamenewparent + ", " + volpathnewparent + ") = " + (newuri != null).ToString());

            return newuri != null;
         }



         public void Test(string storagename, string volpath) {
            DocumentFile srcdoc = GetExistingDocumentFile(storagename, volpath);

            ShowUriInfos(srcdoc.Uri, Activity.ContentResolver);

            ContentResolver resolver = Activity.ContentResolver;


            ContentValues values = new ContentValues();
            values.Put("document_id", "14F1-0B07:tmp1/tmp2/test2.txt");
            values.Put("mime_type", "text/plain");
            values.Put("_display_name", "test2.txt");
            values.Put("last_modified", 1553853408000);
            values.Put("flags", 326);
            values.Put("_size", 20);
            //int res = resolver.Update(srcdoc.Uri, values, null, null);  // Exception: Update not supported

            values = new ContentValues();
            values.Put("abc", "xyz");
            //resolver.Insert(srcdoc.Uri, values); // Exception: Insert not supported



         }

         void ShowUriInfos(global::Android.Net.Uri uri, ContentResolver resolver) {
            System.Diagnostics.Debug.WriteLine("URI-Scheme/SchemeSpecificPart: " + uri.Scheme + "; " + uri.SchemeSpecificPart);
            try {
               System.Diagnostics.Debug.WriteLine("GetDocumentId: " + DocumentsContract.GetDocumentId(uri));
            } catch (Exception ex) {
            }
            try {
               System.Diagnostics.Debug.WriteLine("GetTreeDocumentId: " + DocumentsContract.GetTreeDocumentId(uri));
            } catch (Exception ex) {
            }
            try {
               System.Diagnostics.Debug.WriteLine("GetRootId: " + DocumentsContract.GetRootId(uri));
            } catch (Exception ex) {
            }
            global::Android.Database.ICursor cursor = resolver.Query(uri, null, null, null, null);
            if (cursor != null) {
               while (cursor.MoveToNext()) {
                  for (int i = 0; i < cursor.ColumnCount; i++) {
                     string val = "";
                     switch (cursor.GetType(i)) {
                        case global::Android.Database.FieldType.String: val = cursor.GetString(i); break;
                        case global::Android.Database.FieldType.Integer: val = cursor.GetLong(i).ToString(); break;    // GetInt() ist hier falsch
                        case global::Android.Database.FieldType.Float: val = cursor.GetFloat(i).ToString(); break;
                        case global::Android.Database.FieldType.Blob: val = "(blob)"; break;
                        case global::Android.Database.FieldType.Null: val = "(null)"; break;
                     }
                     System.Diagnostics.Debug.WriteLine(string.Format("Column={0}; ColumnName={1}; ColumnType={2}: {3}", i, cursor.GetColumnName(i), cursor.GetType(i).ToString(), val));
                  }
               }
            }
         }

         /*
         /// <summary>
         /// liefert die schemaspezifische Pfadangabe(für DocumentFile)
         /// <para>
         /// z.B. //com.android.externalstorage.documents/tree/19F4-0903:/document/19F4-0903:abc/def.txt
         /// </para>
         /// </summary>
         /// <param name = "abspath" ></ param >
         /// < returns ></ returns >
         public string GetAsSchemeSpecificPath(string abspath, string storagepath = null) {
            string volpath = GetAsVolumePath(abspath, storagepath);
            if (volpath.StartsWith('/'))
               volpath = volpath.Substring(1);

            string storagename = StorageName;
            if (!string.IsNullOrEmpty(storagepath)) {
               int idx = storagepath.LastIndexOf('/');
               if (idx > 0)
                  storagename = storagepath.Substring(idx + 1);

               if (storagepath == "0")       // !!! Das ist keine sehr gute Variante !!!
                  storagepath = "primary";
            }

            return "//" + AUTHORITY_EXTERNALSTORAGE_DOCUMENTS + "/tree/" + storagename + ":/document/" + storagename + ":" + volpath;
         }

         bool IsSchemeSpecificPath(string path, out string volpath) {
            volpath = "";
            if (path.StartsWith("//" + AUTHORITY_EXTERNALSTORAGE_DOCUMENTS)) { // SchemeSpecific
               int start = path.IndexOf(':', AUTHORITY_EXTERNALSTORAGE_DOCUMENTS.Length + 2, 2);
               if (start >= 0) {
                  path = path.Substring(start);
                  volpath = path.StartsWith('/') ? path : "/" + path;
                  return true;
               }
            }
            return false;
         }
         */

      }

      /// <summary>
      /// Status eines Volumes
      /// </summary>
      public enum VolumeState {
         /// <summary>
         /// Unknown storage state, such as when a path isn't backed by known storage media.
         /// </summary>
         MEDIA_UNKNOWN,
         /// <summary>
         /// Storage state if the media is not present. 
         /// </summary>
         MEDIA_REMOVED,
         /// <summary>
         /// Storage state if the media is present but not mounted. 
         /// </summary>
         MEDIA_UNMOUNTED,
         /// <summary>
         /// Storage state if the media is present and being disk-checked. 
         /// </summary>
         MEDIA_CHECKING,
         /// <summary>
         /// Storage state if the media is in the process of being ejected.
         /// </summary>
         MEDIA_EJECTING,
         /// <summary>
         /// Storage state if the media is present but is blank or is using an unsupported filesystem. 
         /// </summary>
         MEDIA_NOFS,
         /// <summary>
         /// Storage state if the media is present and mounted at its mount point with read/write access. 
         /// </summary>
         MEDIA_MOUNTED,
         /// <summary>
         /// Storage state if the media is present and mounted at its mount point with read-only access. 
         /// </summary>
         MEDIA_MOUNTED_READ_ONLY,
         /// <summary>
         /// Storage state if the media is present not mounted, and shared via USB mass storage.  
         /// </summary>
         MEDIA_SHARED,
         /// <summary>
         /// Storage state if the media was removed before it was unmounted. 
         /// </summary>
         MEDIA_BAD_REMOVAL,
         /// <summary>
         /// Storage state if the media is present but cannot be mounted. Typically this happens if the file system on the media is corrupted. 
         /// </summary>
         MEDIA_UNMOUNTABLE,
      }

      /// <summary>
      /// Daten eines Volumes
      /// </summary>
      public class VolumeData {
         /// <summary>
         /// Anzahl der vorhandenen Volumes
         /// </summary>
         public int Volumes;
         /// <summary>
         /// Index des abgefragten Volumes
         /// </summary>
         public int VolumeNo;
         /// <summary>
         /// Pfad zum Volume, z.B.: "/storage/emulated/0" und "/storage/19F4-0903"
         /// </summary>
         public string Path;
         /// <summary>
         /// Name des Volumes, z.B. "primary" oder "19F4-0903"
         /// </summary>
         public string Name;
         /// <summary>
         /// Beschreibung des Volumes
         /// </summary>
         public string Description;
         /// <summary>
         /// Gesamtspeicherplatz des Volumes
         /// </summary>
         public long TotalBytes;
         /// <summary>
         /// freier Speicherplatz des Volumes
         /// </summary>
         public long AvailableBytes;
         /// <summary>
         /// Ist das abgefragte Volume ein primäres Volume?
         /// </summary>
         public bool IsPrimary;
         /// <summary>
         /// Ist das abgefragte Volume entfernbar?
         /// </summary>
         public bool IsRemovable;
         /// <summary>
         /// Ist das abgefragte Volume nur emuliert?
         /// </summary>
         public bool IsEmulated;
         /// <summary>
         /// Status des Volumes
         /// </summary>
         public VolumeState State;

         public VolumeData() {
            Volumes = 0;
            VolumeNo = -1;
            Path = Name = Description = "";
            TotalBytes = AvailableBytes = 0;
            IsPrimary = IsRemovable = IsEmulated = false;
            State = VolumeState.MEDIA_UNKNOWN;
         }

         public VolumeData(VolumeData vd) {
            Volumes = vd.Volumes;
            VolumeNo = vd.VolumeNo;
            Path = vd.Path;
            Name = vd.Name;
            Description = vd.Description;
            TotalBytes = vd.TotalBytes;
            AvailableBytes = vd.AvailableBytes;
            IsPrimary = vd.IsPrimary;
            IsRemovable = vd.IsRemovable;
            IsEmulated = vd.IsEmulated;
            State = vd.State;
         }

         public override string ToString() {
            return string.Format("VolumeNo={0}, Name={1}, Description={2}, Path={3}", VolumeNo, Name, Description, Path);
         }
      }

      /// <summary>
      /// liefert Hilfsfunktionen für Volumes und Dateioperationen
      /// </summary>
      class StorageVolumeHelper {

         public const string ROOT_ID_PRIMARY_EMULATED = "primary";
         //const string ROOT_ID_HOME = "home";

         /*
               Intent intent = new Intent("android.os.storage.action.OPEN_EXTERNAL_DIRECTORY");
               Bundle b = new Bundle();
               b.PutParcelable("android.os.storage.extra.STORAGE_VOLUME", sv);
               b.PutString("android.os.storage.extra.DIRECTORY_NAME", null);
          */


         /// <summary>
         /// ext. StorageVolume-Pfade beim letzten <see cref="RefreshStorageVolumePathsAndNames"/>(), z.B. "/storage/emulated/0" oder "/storage/19F4-0903"
         /// </summary>
         public List<string> StorageVolumePaths { get; protected set; }

         /// <summary>
         /// ext. StorageVolume-Namen beim letzten <see cref="RefreshStorageVolumePathsAndNames"/>(), z.B. "primary" oder "19F4-0903"
         /// </summary>
         public List<string> StorageVolumeNames { get; protected set; }

         /// <summary>
         /// Anzahl der Volumes
         /// </summary>
         public int Volumes {
            get {
               return StorageVolumePaths.Count;
            }
         }


         public StorageVolumeHelper() {
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)  // "Nougat", 7.0
               throw new Exception("Need Version 7.0 ('Nougat', API 24) or higher.");
            StorageVolumePaths = new List<string>();
            StorageVolumeNames = new List<string>();
            RefreshVolumes();
         }

         VolumeState GetVolumeState(string strstate) {
            VolumeState state = VolumeState.MEDIA_UNKNOWN;
            try {
               /*
                MEDIA_UNKNOWN            Unknown storage state, such as when a path isn't backed by known storage media.  "unknown"  
                MEDIA_REMOVED            Storage state if the media is not present.  "removed" 
                MEDIA_UNMOUNTED          Storage state if the media is present but not mounted.  "unmounted"
                MEDIA_CHECKING           Storage state if the media is present and being disk-checked. "checking"
                MEDIA_EJECTING           Storage state if the media is in the process of being ejected. "ejecting"
                MEDIA_NOFS               Storage state if the media is present but is blank or is using an unsupported filesystem.  "nofs"
                MEDIA_MOUNTED            Storage state if the media is present and mounted at its mount point with read/write access. "mounted" 
                MEDIA_MOUNTED_READ_ONLY  Storage state if the media is present and mounted at its mount point with read-only access.  "mounted_ro" 
                MEDIA_SHARED             Storage state if the media is present not mounted, and shared via USB mass storage.  "shared"
                MEDIA_BAD_REMOVAL        Storage state if the media was removed before it was unmounted. "bad_removal"
                MEDIA_UNMOUNTABLE        Storage state if the media is present but cannot be mounted. Typically this happens if the file system on the media is corrupted.  "unmountable" 
               */
               if (strstate == global::Android.OS.Environment.MediaRemoved)
                  state = VolumeState.MEDIA_REMOVED;
               else if (strstate == global::Android.OS.Environment.MediaUnmounted)
                  state = VolumeState.MEDIA_UNMOUNTED;
               else if (strstate == global::Android.OS.Environment.MediaChecking)
                  state = VolumeState.MEDIA_CHECKING;
               else if (strstate == global::Android.OS.Environment.MediaEjecting)
                  state = VolumeState.MEDIA_EJECTING;
               else if (strstate == global::Android.OS.Environment.MediaNofs)
                  state = VolumeState.MEDIA_NOFS;
               else if (strstate == global::Android.OS.Environment.MediaMounted)
                  state = VolumeState.MEDIA_MOUNTED;
               else if (strstate == global::Android.OS.Environment.MediaMountedReadOnly)
                  state = VolumeState.MEDIA_MOUNTED_READ_ONLY;
               else if (strstate == global::Android.OS.Environment.MediaShared)
                  state = VolumeState.MEDIA_SHARED;
               else if (strstate == global::Android.OS.Environment.MediaBadRemoval)
                  state = VolumeState.MEDIA_BAD_REMOVAL;
               else if (strstate == global::Android.OS.Environment.MediaUnmountable)
                  state = VolumeState.MEDIA_UNMOUNTABLE;
               else
                  state = VolumeState.MEDIA_UNKNOWN;
            } catch {
               state = VolumeState.MEDIA_UNKNOWN;
            }
            return state;
         }

         /// <summary>
         /// Hilfsfunktion für <see cref="global::Android.OS.Storage.StorageVolume"/>: die Methode getPath() ist z.Z. noch nicht umgesetzt und wird per JNI realisiert (API level 24 / Nougat / 7.0)
         /// </summary>
         /// <param name="sv"></param>
         /// <returns></returns>
         string Path4StorageVolume(StorageVolume sv) {
            string path = "";
            try {
               // http://journals.ecs.soton.ac.uk/java/tutorial/native1.1/implementing/method.html
               IntPtr methodID = JNIEnv.GetMethodID(sv.Class.Handle, "getPath", "()Ljava/lang/String;");
               IntPtr lref = JNIEnv.CallObjectMethod(sv.Handle, methodID);
               using (var value = new Java.Lang.Object(lref, JniHandleOwnership.TransferLocalRef)) {
                  path = value.ToString();
               }
            } catch {
               path = "";
            }
            return path;
         }

         /// <summary>
         /// liefert einen StorageManager (API level 19 / Kitkat / 4.4)
         /// </summary>
         /// <returns></returns>
         StorageManager GetStorageManager() {
            Java.Lang.Object ss = global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.StorageService);   // STORAGE_SERVICE  "storage" 
            return ss as global::Android.OS.Storage.StorageManager;
         }


         /// <summary>
         /// setzt die Pfade und Namen der akt. ext. StorageVolumes, z.B. "/storage/emulated/0" und "/storage/19F4-0903" (API level 24 / Nougat / 7.0)
         /// </para>
         /// </summary>
         /// <returns>Anzahl der Volumes</returns>
         public int RefreshVolumes() {
            StorageVolumePaths.Clear();
            StorageVolumeNames.Clear();
            StorageManager sm = GetStorageManager();
            // StorageVolumes und StorageVolume erst ab API 24 (Nougat)
            foreach (global::Android.OS.Storage.StorageVolume sv in sm.StorageVolumes) {
               StorageVolumePaths.Add(Path4StorageVolume(sv));
               if (sv.IsPrimary)
                  StorageVolumeNames.Add(ROOT_ID_PRIMARY_EMULATED);
               else
                  StorageVolumeNames.Add(sv.Uuid);
            }
            return StorageVolumePaths.Count;
         }

         /// <summary>
         /// liefert die akt. Daten des Volumes (API level 24 / Nougat / 7.0)
         /// </summary>
         /// <param name="volumeno">Nummer des gewünschten Volumes (wenn nicht vorhanden, wird nur die Volumeanzahl geliefert)</param>
         /// <returns></returns>
         public VolumeData GetVolumeData(int volumeno) {
            VolumeData data = new VolumeData {
               VolumeNo = volumeno
            };
            if (volumeno >= 0) {

               StorageManager sm = GetStorageManager();
               if (sm != null) {
                  if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.N) { // "Nougat", 7.0
                     data.Volumes = sm.StorageVolumes.Count;
                     if (0 <= volumeno && volumeno < data.Volumes) {
                        StorageVolume sv = sm.StorageVolumes[volumeno];

                        data.IsPrimary = sv.IsPrimary;
                        data.IsRemovable = sv.IsRemovable;
                        data.IsEmulated = sv.IsEmulated;
                        data.Path = Path4StorageVolume(sv);
                        data.State = GetVolumeState(sv.State);
                        data.Description = sv.GetDescription(global::Android.App.Application.Context);
                        data.Name = data.IsPrimary ? ROOT_ID_PRIMARY_EMULATED : sv.Uuid;

                        try {
                           StatFs statfs = new StatFs(data.Path);
                           data.AvailableBytes = statfs.AvailableBytes;
                           data.TotalBytes = statfs.TotalBytes;
                        } catch {
                           data.TotalBytes = data.AvailableBytes = 0;
                        }
                     }
                  }
               }

            } else { // Daten des internal Storage holen

               data.IsPrimary = false;
               data.IsRemovable = false;
               data.IsEmulated = false;
               data.Path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile); // == Xamarin.Essentials.FileSystem.AppDataDirectory;
               data.Path = data.Path.Substring(0, data.Path.LastIndexOf(Path.DirectorySeparatorChar));
               data.State = VolumeState.MEDIA_MOUNTED;
               data.Description = "intern";
               data.Name = "intern";
               try {
                  StatFs statfs = new StatFs(global::Android.OS.Environment.RootDirectory.CanonicalPath);
                  data.AvailableBytes = statfs.AvailableBytes;
                  data.TotalBytes = statfs.TotalBytes;
               } catch {
                  data.TotalBytes = data.AvailableBytes = 0;
               }

            }
            return data;
         }

         /// <summary>
         /// liefert die Nummer, den Namen und Pfad des StorageVolumes zum Pfad
         /// <para>-1, nicht gefunden</para>
         /// <para>0, internal</para>
         /// <para>1, primary external</para>
         /// <para>2 usw., secondary external</para>
         /// </summary>
         /// <param name="fullpath"></param>
         /// <param name="volpath"></param>
         /// <param name="volname"></param>
         /// <returns></returns>
         public int GetKnownElems4FullPath(string fullpath, out string volpath, out string volname) {
            int v = GetVolumeNo(fullpath);
            if (v >= 0) {
               volpath = StorageVolumePaths[v];
               volname = StorageVolumeNames[v];
            } else {
               volpath = volname = "";
            }
            return v;
         }

         /// <summary>
         /// liefert die Nummer des StorageVolumes zum Pfad
         /// <para>-1, nicht gefunden</para>
         /// <para>0, internal</para>
         /// <para>1, primary external</para>
         /// <para>2 usw., secondary external</para>
         /// </summary>
         /// <param name="fullpath"></param>
         /// <returns></returns>
         public int GetVolumeNo(string fullpath) {
            for (int i = 0; i < StorageVolumePaths.Count; i++) {
               if (fullpath.StartsWith(StorageVolumePaths[i])) {
                  if (fullpath.Length == StorageVolumePaths[i].Length ||
                      fullpath[StorageVolumePaths[i].Length] == '/')
                     return i;
               }
            }
            return -1;
         }

      }

      StorageVolumeHelper svh;
      readonly SecondaryExternalStorageHelper seh;


      public StorageHelperAndroid(object activity) {
         if (activity is Activity) {
            svh = new StorageVolumeHelper();
            seh = new SecondaryExternalStorageHelper(activity as Activity);

            RefreshVolumesAndSpecPaths();
         } else
            throw new Exception("'activity' must be a valid Activity.");
      }

      /// <summary>
      /// ext. StorageVolume-Pfade beim letzten <see cref="RefreshStorageVolumePathsAndNames"/>(), z.B. "/storage/emulated/0" oder "/storage/19F4-0903"
      /// </summary>
      public List<string> VolumePaths { get; protected set; }

      /// <summary>
      /// ext. StorageVolume-Namen beim letzten <see cref="RefreshStorageVolumePathsAndNames"/>(), z.B. "primary" oder "19F4-0903"
      /// </summary>
      public List<string> VolumeNames { get; protected set; }

      /// <summary>
      /// Anzahl der Volumes
      /// </summary>
      public int Volumes {
         get {
            return VolumePaths.Count;
         }
      }

      /// <summary>
      /// die "privaten" Verzeichnisse der App, z.B.: "/data/user/0/APKNAME/files" oder "/storage/emulated/0/Android/data/APKNAME/files" oder "/storage/19F4-0903/Android/data/APKNAME/files"
      /// <para>Bei Index 0 steht der "interne" Android-Pfad.</para>
      /// </summary>
      public List<string> AppDataPaths { get; protected set; }

      /// <summary>
      /// die "privaten" Verzeichnisse für temp. Daten der App z.B.: "/data/user/0/APKNAME/cache" oder "/storage/emulated/0/Android/data/APKNAME/cache" oder "/storage/19F4-0903/Android/data/APKNAME/cache"
      /// <para>Bei Index 0 steht der "interne" Android-Pfad.</para>
      /// </summary>
      public List<string> AppTmpPaths { get; protected set; }

      /// <summary>
      /// Volumenamen und spez. Pfadangaben aktualisieren
      /// </summary>
      public void RefreshVolumesAndSpecPaths() {
         seh.GetSpecPaths(out List<string> datapaths, out List<string> cachepaths);

         for (int i = 0; i < datapaths.Count; i++)
            if (datapaths[i].EndsWith('/'))
               datapaths[i] = datapaths[i].Substring(0, datapaths[i].Length - 1);

         for (int i = 0; i < cachepaths.Count; i++)
            if (cachepaths[i].EndsWith('/'))
               cachepaths[i] = cachepaths[i].Substring(0, cachepaths[i].Length - 1);

         AppDataPaths = new List<string>(datapaths);
         AppTmpPaths = new List<string>(cachepaths);

         svh.RefreshVolumes();
         VolumePaths = new List<string>(svh.StorageVolumePaths);
         VolumeNames = new List<string>(svh.StorageVolumeNames);
      }

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="volidx">Volume-Index</param>
      /// <returns></returns>
      public VolumeData GetVolumeData(int volidx) {
         return volidx < Volumes ?
                     svh.GetVolumeData(volidx) :
                     null;
      }

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
      /// <returns></returns>
      public VolumeData GetVolumeData(string storagename) {
         int volidx = svh.GetVolumeNo(storagename);
         return svh.GetVolumeData(volidx);
      }

   }

}