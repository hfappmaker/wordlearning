using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using Android.Content;
using static Android.Widget.AdapterView;
using Android.Views;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content.Res;
using Android.Graphics;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using System.Xml.Linq;
using Android.Provider;
using File = System.IO.File;
using Android.Database;
using System.Xml;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision;
using Android.Gms.Tasks;
using Java.Lang;
using Firebase.ML.Vision.Text;
using Android.Support.V7.Widget;

namespace WordLearning
{
    [Activity(Label = "Settings")]
    public class Settings : CustomActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        ListView listView_BU_And_RC;//,listView_Settingvoicelanguage,listView_SettingAutomode,listView_CSVfileimport;
        //ListView listView_Changehomename;
        bool recoverflag = false;
        XElement xelmnow = XDocument.Load(Utility.WordListPath).Root;
        SeekBar[] seekBars = new SeekBar[3];
        List<(string, List<string[]>)> contentdata = new List<(string, List<string[]>)>();
        private int position_dlg;
        private Dictionary<List<int>, XElement> AllFolder;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //frame.
            base.OnCreate(savedInstanceState);
            //If There isn't Start activity although EditWordlist activity exists. Finish this activity and start Start activity.
            if (!Utility.MultipleActivityFlag || !Utility.cd.Any())
            {
                Finish();
                Utility.cd = new List<int>() { 0, 0 };
                Intent intent = new Intent(this, typeof(Start));
                StartActivity(intent);
                return;
            }
            SetContentViewAndToolbar(Resource.Layout.Settings, Resource.Id.tbSettings);
            listView_BU_And_RC = FindViewById<ListView>(Resource.Id.lv_Settings_Backup_And_Recover);
            if (File.Exists(Utility.Backuppath))
            {
                listView_BU_And_RC.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Message.BackUp[Utility.language]);
            }
        }

        #region listView(Backup And Recover) Click
        /// <summary>
        /// Lvs the settings click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Backup_And_Recover_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.EventHandler<DialogClickEventArgs>[] handler = new System.EventHandler<DialogClickEventArgs>[2];
            handler[0] = CreateBackupCheck;
            handler[1] = RecoverfromBackupCheck;
            var dlgConfirm = new Android.Support.V7.App.AlertDialog.Builder(this);
            dlgConfirm.SetMessage(Message.BU_And_RC[Utility.language][e.Position]);
            dlgConfirm.SetNegativeButton("OK", handler[e.Position]);
            dlgConfirm.SetPositiveButton("CANCEL", (_s, _e) => { });
            dlgConfirm.Show();
        }

        /// <summary>
        /// Lvs the settings click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Backup_And_Recover_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {

        }
        #endregion

        #region listView(Change themecolor) Click
        /// <summary>
        /// Lvs the settings click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Changethemecolor_ItemClick(object sender, ItemClickEventArgs e)
        {
            LayoutInflater Inflater;
            Inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            View layout = Inflater.Inflate(Resource.Layout.Dialog_Changethemecolor, (ViewGroup)FindViewById(Resource.Id.ll_Dialog_Changethemecolor));
            var dlgChangethemecolor = new Android.Support.V7.App.AlertDialog.Builder(this);
            var xml = XDocument.Load(Utility.WordListPath);
            XElement Themecolor_xml = xml.Root.Element("Themecolor");
            int[] oldcolor = { defaultthemecolor[0], defaultthemecolor[1], defaultthemecolor[2] };
            if (Themecolor_xml != null)
            {
                oldcolor = new int[] { int.Parse(Themecolor_xml.Element("Red").Value), int.Parse(Themecolor_xml.Element("Green").Value), int.Parse(Themecolor_xml.Element("Blue").Value) };
            }
            seekBars[0] = layout.FindViewById<SeekBar>(Resource.Id.Sb_Red_Dialog_Changethemecolor);
            seekBars[1] = layout.FindViewById<SeekBar>(Resource.Id.Sb_Green_Dialog_Changethemecolor);
            seekBars[2] = layout.FindViewById<SeekBar>(Resource.Id.Sb_Blue_Dialog_Changethemecolor);
            Color[] Thumbcolor = { Color.Red, Color.Green, Color.Blue };
            int[][] ProgressTintColor = { new int[] { 255, 0, 0 }, new int[] { 0, 255, 0 }, new int[] { 0, 0, 255 } };
            for (int i = 0; i < seekBars.Count(); i++)
            {
                seekBars[i].Tag = i;
                seekBars[i].Progress = oldcolor[i];
                seekBars[i].ProgressTintList = ColorStateList.ValueOf(Color.Argb(seekBars[i].Progress, ProgressTintColor[i][0], ProgressTintColor[i][1], ProgressTintColor[i][2]));
                seekBars[i].ThumbTintList = ColorStateList.ValueOf(Thumbcolor[i]);
                seekBars[i].ProgressChanged += (_s, _e) =>
                {
                    _e.SeekBar.ProgressTintList = ColorStateList.ValueOf(Color.Argb(_e.Progress, ProgressTintColor[(int)_e.SeekBar.Tag][0], ProgressTintColor[(int)_e.SeekBar.Tag][1], ProgressTintColor[(int)_e.SeekBar.Tag][2]));
                    themecolor = Color.Rgb(seekBars[0].Progress, seekBars[1].Progress, seekBars[2].Progress);
                    themecolor_dark = Color.Rgb((int)(seekBars[0].Progress * 0.8), (int)(seekBars[1].Progress * 0.8), (int)(seekBars[2].Progress * 0.8));
                    toolbar.BackgroundTintList = ColorStateList.ValueOf(themecolor);
                    Window.SetStatusBarColor(themecolor_dark);
                    Window.SetNavigationBarColor(themecolor);
                };
            }
            //base.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            dlgChangethemecolor.SetMessage(Message.Changethemecolor[Utility.language]);
            dlgChangethemecolor.SetView(layout);
            dlgChangethemecolor.SetNegativeButton("OK", Setnewthemecolor);
            dlgChangethemecolor.SetPositiveButton("CANCEL", (_s, _e) =>
            {
                themecolor = Color.Rgb(oldcolor[0], oldcolor[1], oldcolor[2]);
                themecolor_dark = Color.Rgb((int)(oldcolor[0] * 0.8), (int)(oldcolor[1] * 0.8), (int)(oldcolor[2] * 0.8));
                toolbar.BackgroundTintList = ColorStateList.ValueOf(themecolor);
                Window.SetStatusBarColor(themecolor_dark);
                Window.SetNavigationBarColor(themecolor);
            });
            dlgChangethemecolor.Show();
        }
        /// <summary>
        /// Setnewthemecolor the specified sender and e.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void Setnewthemecolor(object sender, DialogClickEventArgs e)
        {
            var xml = XDocument.Load(Utility.WordListPath);
            XElement Themecolor_elm = xml.Root.Element("Themecolor");
            Themecolor_elm.Element("Red").SetValue(((int)(themecolor.R)).ToString());
            Themecolor_elm.Element("Green").SetValue(((int)(themecolor.G)).ToString());
            Themecolor_elm.Element("Blue").SetValue(((int)(themecolor.B)).ToString());
            xml.Save(Utility.WordListPath);
            return;
        }
        /// <summary>
        /// Lvs the settings click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Changethemecolor_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {

        }
        #endregion

        #region listView(Change homename) Click
        /// <summary>
        /// Lvs the settings changehomename item click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Changehomename_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        /// <summary>
        /// Lvs the settings changehomename item long click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void lv_Settings_Changehomename_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {

        }
        #endregion

        #region listview(CSVfile import) Click

        public void lv_Settings_CSVfileimport_ItemClick(object sender, ItemClickEventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            StartActivityForResult(Intent.CreateChooser(intent, "Select csvfile"), (int)RequestCode.SELECT_CSVFILE);
        }

        public void lv_Settings_CSVfileimport_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {


        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            contentdata.Clear();
            if (requestCode == (int)RequestCode.SELECT_CSVFILE && resultCode == Result.Ok)
            {
                var uri = data.Data;
                if (uri != null) //１つだけ選択した場合
                {
                    (string, List<string[]>) csvdata = new ValueTuple<string, List<string[]>>
                    {
                        Item2 = new List<string[]>()
                    };
                    string name = "test";
                    bool skipflag = false;
                    var st = ContentResolver.OpenInputStream(uri);
                    string[] projection = { MediaStore.MediaColumns.DisplayName };
                    ICursor cursor = ContentResolver.Query(uri, projection, null, null, null);
                    if (cursor != null)
                    {
                        //name = null;
                        if (cursor.MoveToFirst())
                        {
                            name = cursor.GetString(0);
                            if (name.Length > 3)
                            {
                                if (name.Substring(name.Length - 4, 4) != ".csv")
                                {
                                    skipflag = true;
                                }
                            }
                        }
                        cursor.Close();
                    }
                    if (!skipflag)
                    {
                        try
                        {
                            // csvファイルを開く
                            using (var sr = new StreamReader(st, System.Text.Encoding.GetEncoding("UTF-8")))
                            {
                                csvdata.Item1 = new string(name.SkipLast(4).ToArray());
                                // ストリームの末尾まで繰り返す
                                while (!sr.EndOfStream)
                                {
                                    // ファイルから一行読み込む
                                    var line = sr.ReadLine();
                                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                                    var values = line.Split(',');
                                    // 出力する
                                    if (values.Length > 1)
                                    {
                                        if (values.Length == 2)
                                        {
                                            csvdata.Item2.Add(new string[] { values[0], values[1], string.Empty });
                                        }
                                        else
                                        {
                                            csvdata.Item2.Add(new string[] { values[0], values[1], values[2] });
                                        }
                                    }
                                }
                                //contentdataに保存
                                contentdata.Add(csvdata);
                            }
                        }
                        catch (System.Exception e)
                        {
                            // ファイルを開くのに失敗したとき
                            System.Console.WriteLine(e.Message);
                        }
                    }

                }
                else if (data.ClipData != null)//２つ以上選択した場合
                {
                    for (int i = 0; i < data.ClipData.ItemCount; i++)
                    {
                        var uri2 = data.ClipData.GetItemAt(i).Uri;
                        (string, List<string[]>) csvdata = new ValueTuple<string, List<string[]>>
                        {
                            Item2 = new List<string[]>()
                        };
                        string name = "test";
                        var st = ContentResolver.OpenInputStream(uri2);
                        string[] projection = { MediaStore.MediaColumns.DisplayName };
                        ICursor cursor = ContentResolver.Query(uri2, projection, null, null, null);
                        if (cursor != null)
                        {
                            if (cursor.MoveToFirst())
                            {
                                name = cursor.GetString(0);
                                if (name.Length > 3)
                                {
                                    if (name.Substring(name.Length - 4, 4) != ".csv")
                                    {
                                        cursor.Close();
                                        continue;
                                    }
                                }
                            }
                            cursor.Close();
                        }
                        try
                        {
                            // csvファイルを開く
                            using (var sr = new StreamReader(st, System.Text.Encoding.GetEncoding("UTF-8")))
                            {
                                csvdata.Item1 = new string(name.SkipLast(4).ToArray());
                                // ストリームの末尾まで繰り返す
                                while (!sr.EndOfStream)
                                {
                                    // ファイルから一行読み込む
                                    var line = sr.ReadLine();
                                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                                    var values = line.Split(',');
                                    // 出力する
                                    if (values.Length > 1)
                                    {
                                        if (values.Length == 2)
                                        {
                                            csvdata.Item2.Add(new string[] { values[0], values[1], string.Empty });
                                        }
                                        else
                                        {
                                            csvdata.Item2.Add(new string[] { values[0], values[1], values[2] });
                                        }
                                    }
                                }
                                //contentdataに保存
                                contentdata.Add(csvdata);
                            }
                        }
                        catch (System.Exception e)
                        {
                            // ファイルを開くのに失敗したとき
                            System.Console.WriteLine(e.Message);
                        }

                    }
                }
                if (contentdata.Count > 0)
                {
                    var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlg.SetMessage(Message.Registerwordlistconfirm[Utility.language]);
                    ListView listView = new ListView(this)
                    {
                        Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, (from dataelm in contentdata select dataelm.Item1).ToList())
                    };
                    dlg.SetView(listView);
                    dlg.SetPositiveButton("OK", Addcsvdata);
                    dlg.SetNegativeButton("CANCEL", (s, e) => { });
                    dlg.Show();
                }
            }
            else if (requestCode == (int)RequestCode.RESLUT_CAMERA && resultCode == Result.Ok)
            {
                if (data.Extras != null)
                {
                    Bitmap bitmap = (Bitmap)data.Extras.Get("data");
                    if (bitmap != null)
                    {
                        FirebaseVisionImage firebaseVisionImage = FirebaseVisionImage.FromBitmap(bitmap);
                        var detector = FirebaseVision.Instance.OnDeviceTextRecognizer;
                        var result = detector.ProcessImage(firebaseVisionImage).AddOnSuccessListener(new Recognizertext());
                        // 画像サイズを計測
                        int bmpWidth = bitmap.Width;
                        int bmpHeight = bitmap.Height;
                    }
                    // dataから画像を取り出す
                }
            }
        }

        private void Addcsvdata(object sender, DialogClickEventArgs e)
        {
            var dlgMove = new Android.Support.V7.App.AlertDialog.Builder(this);
            var xml = XDocument.Load(Utility.WordListPath);
            AllFolder = GetAllFolder(xml);
            LayoutInflater Inflater;
            Inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            View layout = Inflater.Inflate(Resource.Layout.Dialog_Move_Start, (ViewGroup)FindViewById(Resource.Id.ll_Dialog_Move_Start));
            //ListView listFolder = layout.FindViewById<ListView>(Resource.Id.lv_Dialog_Move_Start);
            RecyclerView listFolder = layout.FindViewById<RecyclerView>(Resource.Id.lv_Dialog_Move_Start);
            //listFolder.Adapter = new ArrayAdapter_Start_Move(this, Resource.Layout.row, AllFolder.ToList());
            listFolder.SetLayoutManager(new LinearLayoutManager(this));
            listFolder.SetAdapter(new ArrayAdapter_Start_Move(this, Resource.Layout.row_SelectDestination, AllFolder.ToList(),Utility.cd));
            //listFolder.ItemClick += (_s, _e) =>
            //{

            //    if (listFolder.GetTag(Constant.FreeTagKey) != null)
            //    {
            //        int previousselectpositon = (int)listFolder.GetTag(Constant.FreeTagKey) - listFolder.FirstVisiblePosition;
            //        if (previousselectpositon >= 0 && previousselectpositon <= listFolder.LastVisiblePosition)
            //        {
            //            listFolder.GetChildAt(previousselectpositon).SetBackgroundColor(Constant.SelectColor[false]);
            //        }
            //    }
            //    listFolder.SetTag(Constant.FreeTagKey, _e.Position);
            //    listFolder.GetChildAt(_e.Position - listFolder.FirstVisiblePosition).SetBackgroundColor(Color.Green);
            //    position_dlg = _e.Position;
            //    return;
            //};
            dlgMove.SetMessage(Message.Selectplace[Utility.language]);
            dlgMove.SetPositiveButton(Message.Add[Utility.language], Addcsvdata2);
            dlgMove.SetNegativeButton("CANCEL", (_s, _e) => { });
            dlgMove.SetView(layout);
            dlgMove.SetCancelable(false);
            dlgMove.Show();
        }

        /// <summary>
        /// Move the specified sender and e.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void Addcsvdata2(object sender, DialogClickEventArgs e)
        {
            var dlg = (Android.Support.V7.App.AlertDialog)sender;
            ListView listFolder = dlg.FindViewById<ListView>(Resource.Id.lv_Dialog_Move_Start);
            var xml = XDocument.Load(Utility.WordListPath);
            var xmlcdNode = Utility.GetXElement(AllFolder.ElementAt(position_dlg).Key, xml);
            foreach ((string, List<string[]>) data in contentdata)
            {
                var wordlistxml = new XElement("Wordlist", new XAttribute("Name", XmlConvert.EncodeLocalName(data.Item1)));
                foreach (string[] wordandmeaning in data.Item2)
                {
                    wordlistxml.Add(new XElement("Word", new XElement("Wordname", XmlConvert.EncodeLocalName(wordandmeaning[0])), new XElement("Wordmeaning", wordandmeaning[1]), new XElement("Tag", "00000"), new XElement("Memo", XmlConvert.EncodeLocalName(wordandmeaning[2]))));
                }
                xmlcdNode.Add(wordlistxml);
            }
            xml.Save(Utility.WordListPath);
            Toast.MakeText(this, Message.Addcomplete[Utility.language], ToastLength.Short).Show();
            return;
        }

        /// <summary>
        /// Get all folders.
        /// </summary>
        /// <returns>The all folder.</returns>
        /// <param name="xDocument">X document.</param>
        private Dictionary<List<int>, XElement> GetAllFolder(XDocument xDocument)
        {
            XElement currentElement = xDocument.Root.Element("Folder");
            List<int> path = new List<int>() { 0, 0 };
            Dictionary<List<int>, XElement> retAllFolder = new Dictionary<List<int>, XElement>();
            Action<XElement> func = null;
            func = (ChildElement) =>
            {
                foreach ((XElement, int) xElement in ChildElement.Elements().Select((v, i) => (v, i)))
                {
                    if (xElement.Item1.Name == "Folder")
                    {
                        path.Add(xElement.Item2);
                        func(xElement.Item1);
                        path.RemoveAt(path.Count - 1);
                    }
                }
                if (ChildElement.Name == "Folder")
                {
                    List<int> vs = new List<int>(path);
                    retAllFolder.Add(vs, ChildElement);
                }
            };
            func(currentElement);
            return retAllFolder;
        }

        #endregion

        #region listview(Question_bulletin_board) Click

        public void lv_Settings_Question_bulletin_board_ItemClick(object sender, ItemClickEventArgs e)
        {
            //return;
            Utility.MultipleActivityFlag = true;
            Intent intent = new Intent(this, typeof(Question_bulletin_board));
            StartActivity(intent);
        }

        public void lv_Settings_Question_bulletin_board_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {


        }

        #endregion

        #region listview(ImportFromImage) Click


        public void lv_Settings_ImportFromImage_ItemClick(object sender, ItemClickEventArgs e)
        {
            CameraCheck();
            //FirebaseVisionImage
        }

        public void lv_Settings_ImportFromImage_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {


        }
        #endregion

        #region seekbar

        #endregion

        /// <summary>
        /// When Destroying this activity.
        /// </summary>
        protected override void OnDestroy()
        {
            Utility.MultipleActivityFlag = false;
            base.OnDestroy();
        }

        #region Method
        /// <summary>
        /// Check whether creating backup is enabled or not.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CreateBackupCheck(object sender, DialogClickEventArgs e)
        {
            Permission permission = CheckSelfPermission(Manifest.Permission.WriteExternalStorage);
            if (permission != Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, (int)PermissionRequestCode.Create_Backup);
            }
            else
            {
                CreateBackup();
            }
        }
        /// <summary>
        /// Create backup.
        /// </summary>
        void CreateBackup()
        {
            var dlgResult = new Android.Support.V7.App.AlertDialog.Builder(this);
            try
            {
                if (File.Exists(Utility.Backuppath))
                {
                    File.Delete(Utility.Backuppath);
                }
                else
                {
                    if (!Directory.Exists(Android.OS.Environment.GetExternalStoragePublicDirectory("/WordLearning").AbsolutePath))
                    {
                        Directory.CreateDirectory(Android.OS.Environment.GetExternalStoragePublicDirectory("/WordLearning").AbsolutePath);
                    }
                    else
                    {
                        File.Create(Utility.Backuppath);
                    }
                }
                File.Copy(Utility.WordListPath, Utility.Backuppath, true);
                dlgResult.SetMessage(Message.CreateBackupcomplete[Utility.language]);
            }
            catch (System.UnauthorizedAccessException)
            {
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
                dlgResult.SetTitle(Message.Error[Utility.language]);
                dlgResult.SetMessage(Message.RequireStorageAccess[Utility.language]);
            }
            catch (System.Exception)
            {
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
                dlgResult.SetTitle(Message.Error[Utility.language]);
                dlgResult.SetMessage(Message.CreateBackupfailure[Utility.language]);
            }
            finally
            {
                dlgResult.SetPositiveButton("OK", (_s, _e) => { });
                dlgResult.Show();
            }
        }
        /// <summary>
        /// Recover from backup.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void RecoverfromBackupCheck(object sender, DialogClickEventArgs e)
        {
            Permission permission = CheckSelfPermission(Manifest.Permission.WriteExternalStorage);
            if (permission != Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, (int)PermissionRequestCode.RecoverfromBackup);
            }
            else
            {
                RecoverfromBackup();
            }
        }
        /// <summary>
        /// Recover from backup.
        /// </summary>
        void RecoverfromBackup()
        {
            var dlgResult = new Android.Support.V7.App.AlertDialog.Builder(this);
            bool toomanyflag = false;
            if (File.Exists(Utility.Backuppath))
            {
                try
                {
                    var xelmBU = XDocument.Load(Utility.Backuppath).Root.Element("Folder");
                    var xelmtmp = XDocument.Load(Utility.WordListPath).Root.Element("Folder");
                    var temp = xelmnow.Elements();
                    XElement Themecolorelm = null;
                    XElement Tagcolorelm = null;
                    if (temp.Any())
                    {
                        Themecolorelm = temp.First(elm => elm.Name == "Themecolor");
                        Tagcolorelm = temp.First(elm => elm.Name == "Tagcolor");
                    }
                    if (Themecolorelm != null)
                    {
                        Themecolorelm.Remove();
                    }
                    if (Tagcolorelm != null)
                    {
                        Tagcolorelm.Remove();
                    }
                    XDocument.Load(Utility.Backuppath).Root.Elements().ToList().ForEach(elm =>
                    {
                        if (elm.Name != "Folder")
                        {
                            xelmnow.Add(elm);
                        }
                    });
                    foreach (XElement elm in xelmBU.Elements())
                    {
                        if (xelmnow.Element("Folder").Elements().Count() > 1000)
                        {
                            toomanyflag = true;
                            break;
                        }
                        xelmnow.Element("Folder").Add(elm);
                    }
                    xelmnow.Save(Utility.WordListPath);
                    var xml = XDocument.Load(Utility.WordListPath);
                    var Themecolor_xml = xml.Root.Elements()?.SingleOrDefault((elm) => { return elm?.Name == "Themecolor"; });
                    if (Themecolor_xml != null)
                    {
                        themecolor = Color.Rgb(int.Parse(Themecolor_xml.Element("Red").Value), int.Parse(Themecolor_xml.Element("Green").Value), int.Parse(Themecolor_xml.Element("Blue").Value));
                        themecolor_dark = Color.Rgb((int)(int.Parse(Themecolor_xml.Element("Red").Value) * 0.8), (int)(int.Parse(Themecolor_xml.Element("Green").Value) * 0.8), (int)(int.Parse(Themecolor_xml.Element("Blue").Value) * 0.8));
                    }
                    Window.SetStatusBarColor(themecolor_dark);
                    Window.SetNavigationBarColor(themecolor);
                    toolbar.BackgroundTintList = ColorStateList.ValueOf(themecolor);
                    if (toomanyflag)
                    {
                        dlgResult.SetMessage(Message.Restorecompletedtoomanyfolder[Utility.language]);
                    }
                    dlgResult.SetMessage(Message.Restorecompleted[Utility.language]);
                    recoverflag = true;
                }
                catch (UnauthorizedAccessException)
                {
                    dlgResult.SetTitle(Message.Error[Utility.language]);
                    dlgResult.SetMessage(Message.RequireStorageAccess[Utility.language]);
                }
                catch (System.Exception)
                {
                    dlgResult.SetTitle(Message.Error[Utility.language]);
                    dlgResult.SetMessage(Message.Restorefaliure[Utility.language]);
                }
            }
            else
            {
                dlgResult.SetMessage(Message.Noneofbackup[Utility.language]);
            }
            dlgResult.SetPositiveButton("OK", (_s, _e) => { });
            dlgResult.Show();
        }
        /// <summary>
        /// Recover the specified xelm and xelmBU.
        /// </summary>
        /// <param name="xelm">Xelm.</param>
        /// <param name="xelmBU">Xelm bu.</param>
        private void Recover(XElement xelm, XElement xelmBU)
        {
            bool duplicateflag = false;
            foreach (XElement elmBU in xelmBU.Elements())
            {
                foreach (XElement elm in xelm.Elements())
                {
                    if (elmBU.Name == elm.Name && elmBU.Attribute("type").Value == elm.Attribute("type").Value)
                    {
                        duplicateflag = true;
                        if (elmBU.Attribute("type").Value == "Wordlist")
                        {
                            xelmnow.Elements().ToList().Find((p) => { return p.Name == elmBU.Name && p.Attribute("type").Value == elmBU.Attribute("type").Value; }).Remove();
                            duplicateflag = false;
                            break;
                        }
                        else if (elmBU.Attribute("type").Value == "Themecolor")
                        {
                            xelmnow.Elements().Single((p) => { return p.Attribute("type").Value == "Themecolor"; }).Remove();
                            duplicateflag = false;
                            break;
                        }
                        xelmnow = xelmnow.Elements().ToList().Find((p) => { return p.Name == elmBU.Name && p.Attribute("type").Value == elmBU.Attribute("type").Value; });
                        Recover(elm, elmBU);
                        xelmnow = xelmnow.Ancestors().First();
                    }
                }
                if (!duplicateflag)
                {
                    xelmnow.Add(elmBU);
                }
            }

        }
        /// <summary>
        /// Check whether Camera is enabled or not.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CameraCheck()
        {
            Permission permission = CheckSelfPermission(Manifest.Permission.Camera);
            if (permission != Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.Camera }, (int)PermissionRequestCode.ImportFromImage);
            }
            else
            {
                ImportFromImage();
            }
        }

        private void ImportFromImage()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, (int)RequestCode.RESLUT_CAMERA);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (recoverflag)
                    {
                        Utility.cd = new List<int>() { 0, 0 };
                    }
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        /// <summary>
        /// Ons the key down.
        /// </summary>
        /// <returns><c>true</c>, if key down was oned, <c>false</c> otherwise.</returns>
        /// <param name="keyCode">Key code.</param>
        /// <param name="e">E.</param>
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                if (recoverflag)
                {
                    Utility.cd = new List<int>() { 0, 0 };
                }
            }
            return base.OnKeyDown(keyCode, e);
        }
        /// <summary>
        /// Select method from permissionsresult
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="permissions">Permissions.</param>
        /// <param name="grantResults">Grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionRequestCode prc = (PermissionRequestCode)requestCode;
            switch (prc)
            {
                case PermissionRequestCode.Create_Backup:
                    CreateBackup();
                    break;
                case PermissionRequestCode.RecoverfromBackup:
                    RecoverfromBackup();
                    break;
                case PermissionRequestCode.ImportFromImage:
                    ImportFromImage();
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }
        /// <summary>
        /// Message.
        /// </summary>
        public static class Message
        {
            public static Dictionary<string, string[]> BackUp = new Dictionary<string, string[]>()
            {
                {"日本語",new string[]{"バックアップの作成","バックアップから復元" + System.Environment.NewLine + "場所:" + System.Environment.NewLine + Utility.Backuppath} },
                {"English",new string[]{"Create backup","Recover from backup" + System.Environment.NewLine + "locate:" + System.Environment.NewLine + Utility.Backuppath} },
                {"繁體中文",new string[]{ "創建備份", "從備份中恢復" + System.Environment.NewLine + "定位:" + System.Environment.NewLine + Utility.Backuppath} },
                {"简体中文",new string[]{ "创建备份", "从备份中恢复" + System.Environment.NewLine + "定位:" + System.Environment.NewLine + Utility.Backuppath} },
                {"Deutsch",new string[]{ "Ein Backup erstellen", "Vom Backup wiederherstellen" + System.Environment.NewLine + "Lokalisieren:" + System.Environment.NewLine + Utility.Backuppath} },
                {"Français",new string[]{"Create backup","バックアップから復元" + System.Environment.NewLine + "場所:" + System.Environment.NewLine + Utility.Backuppath} },
                {"한국어",new string[]{"Create backup","バックアップから復元" + System.Environment.NewLine + "場所:" + System.Environment.NewLine + Utility.Backuppath} },
                {"русский",new string[]{"Create backup","バックアップから復元" + System.Environment.NewLine + "場所:" + System.Environment.NewLine + Utility.Backuppath} },
                {"ंडिया",new string[]{ }}
            };

            public static Dictionary<string, string[]> BU_And_RC = new Dictionary<string, string[]>()
            {
                {"日本語",new string[]{"現在登録されている単語帳から" +System.Environment.NewLine + "バックアップを作成します。" + System.Environment.NewLine + "宜しいですか？","バックアップから単語帳を復元し、"+ System.Environment.NewLine + "現在の単語帳に統合します。" + System.Environment.NewLine + "宜しいですか？"}},
                {"English",new string[]{"Create a backup from" +System.Environment.NewLine + "the currently registered word book." + System.Environment.NewLine + "Are you sure？", "Restore word book from backup," + System.Environment.NewLine + "And merge into the current word book." + System.Environment.NewLine + "Are you sure？"}},
                {"繁體中文",new string[]{ "從當前註冊的單詞簿創建備份。" + System.Environment.NewLine + "你確定嗎？", "從備份中恢復單詞簿，" + System.Environment.NewLine + "並合併到當前的單詞簿。" + System.Environment.NewLine + "你確定嗎？"}},
                {"简体中文",new string[]{ "从当前注册的单词簿创建备份。" + System.Environment.NewLine + "你确定吗？", "从备份中恢复单词簿，" + System.Environment.NewLine + "并合并到当前的单词簿。" + System.Environment.NewLine + "你确定吗？"}},
                {"Deutsch",new string[]{ "Erstellen Sie eine Sicherung"  + System.Environment.NewLine + "aus dem aktuell registrierten Word-Buch." + System.Environment.NewLine + "Bist du sicher?", "Wordbook aus einem Backup wiederherstellen," + System.Environment.NewLine + "und verschmelzen mit dem aktuellen Wortbuch." + System.Environment.NewLine + "Bist du sicher?"}},
                {"Français",new string[]{ "Créez une sauvegarde à partir " + System.Environment.NewLine + "du livre de mots actuellement enregistré." + System.Environment.NewLine + "Êtes-vous sûr?", "Restaurer le livre de mots à partir d'une sauvegarde," + System.Environment.NewLine + "et fusionner dans le dictionnaire de mots actuel." + System.Environment.NewLine + "Êtes-vous sûr?"}},
                {"한국어",new string[]{ "현재 등록 된 단어장에서 백업을 만듭니다." + System.Environment.NewLine + "확실해?", "백업에서 단어장 복원," + System.Environment.NewLine + "현재 단어장에 병합합니다." + System.Environment.NewLine + "확실해?"}},
                {"русский",new string[]{ "Создайте резервную копию из " + System.Environment.NewLine + "текущей зарегистрированной книги слов." + System.Environment.NewLine + "Уверены ли вы?", "Восстановить книгу слов из резервной копии," + System.Environment.NewLine + "и влиться в текущую книгу слов." + System.Environment.NewLine + "Уверены ли вы?"}},
                {"ंडिया",new string[]{ }}
            };

            public static Dictionary<string, string> Registerwordlistconfirm = new Dictionary<string, string>()
            {
                {"日本語","以下の単語帳を追加します。宜しいですか？"},
                {"English","Add these word books. Are you sure?"},
                {"繁體中文","添加這些單詞書。你確定嗎？"},
                {"简体中文","添加这些单词书。你确定吗？"},
                {"Deutsch","Fügen Sie diese Wortbücher hinzu. Bist du sicher?"},
                {"Français","Ajoutez ces livres de mots. Êtes-vous sûr?"},
                {"한국어","이 단어장을 추가하십시오. 확실해?"},
                {"русский","Добавьте эти слова книги. Уверены ли вы?"},
                {"इंडिया",""}
            };

            public static Dictionary<string, string> Selectplace = new Dictionary<string, string>()
            {
                 {"日本語","追加先を選択して下さい。" },
                 {"English","Please select a destination folder." },
                 {"繁體中文","請選擇目標文件夾。" },
                 {"简体中文","请选择目标文件夹。" },
                 {"Deutsch","Bitte wählen Sie einen Zielordner." },
                 {"Français","Veuillez sélectionner un dossier de destination." },
                 {"한국어","대상 폴더를 선택하십시오." },
                 {"русский","Пожалуйста, выберите папку назначения." },
                 {"इंडिया",""}
            };
            public static Dictionary<string, string> Add = new Dictionary<string, string>()
            {
                {"日本語","追加"},
                {"English","Add"},
                {"繁體中文","加"},
                {"简体中文","加"},
                {"Deutsch","hinzufügen"},
                {"Français","ajouter"},
                {"한국어","더하다"},
                {"русский","добавлять"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Addcomplete = new Dictionary<string, string>()
            {
                {"日本語","追加されました。"},
                {"English","Addition was completed."},
                {"繁體中文","增加完成了"},
                {"简体中文","增加完成了"},
                {"Deutsch","Die Zugabe wurde abgeschlossen"},
                {"Français","L'addition a été complétée"},
                {"한국어","추가가 완료되었습니다."},
                {"русский","Дополнение было завершено"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> CreateBackupcomplete = new Dictionary<string, string>()
            {
                {"日本語","バックアップの作成に成功しました。"},
                {"English","Creating a backup was completed."},
                {"繁體中文","創建備份已完成。"},
                {"简体中文","创建备份已完成。"},
                {"Deutsch","Das Erstellen eines Backups wurde abgeschlossen."},
                {"Français","la création d'une sauvegarde était terminée."},
                {"한국어","백업 작성이 완료되었습니다."},
                {"русский","создание резервной копии было завершено."},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Error = new Dictionary<string, string>()
            {
                {"日本語","エラー"},
                {"English","Error"},
                {"繁體中文","錯誤"},
                {"简体中文","错误"},
                {"Deutsch","Error"},
                {"Français","Erreur"},
                {"한국어","오류"},
                {"русский","ошибка"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> RequireStorageAccess = new Dictionary<string, string>()
            {
                {"日本語","復元に失敗しました。\nストレージへのアクセスを許可して下さい。"},
                {"English","Failed to restore.\nPlease Allow access to storage."},
                {"繁體中文","無法恢復。\n請允許訪問存儲空間。"},
                {"简体中文","无法恢复。\n请允许访问存储空间。"},
                {"Deutsch","Wiederherstellung fehlgeschlagen\nBitte erlauben Sie den Zugriff auf Speicher."},
                {"Français","Échec de la restauration.\nVeuillez autoriser l'accès au stockage."},
                {"한국어","복원하지 못했습니다.\n저장소에 대한 액세스를 허용하십시오."},
                {"русский","Не удалось восстановить.\nПожалуйста, разрешите доступ к хранилищу."},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> CreateBackupfailure = new Dictionary<string, string>()
            {
                {"日本語","バックアップの作成に失敗しました。"},
                {"English","Failed to create backup."},
                {"繁體中文","無法創建備份。"},
                {"简体中文","无法创建备份。"},
                {"Deutsch","Sicherung konnte nicht erstellt werden"},
                {"Français","Échec de la création de la sauvegarde."},
                {"한국어","백업을 만들지 못했습니다."},
                {"русский","Не удалось создать резервную копию."},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Restorecompletedtoomanyfolder = new Dictionary<string, string>()
            {
                {"日本語","復元に成功しました。(フォルダ数が多いため、一部のみ復元しました。)"},
                {"English","Restoration was successful. (Since there were many folders, only a part was restored.)"},
                {"繁體中文","恢復成功。 （由於存在許多文件夾，因此只恢復了一部分。）"},
                {"简体中文","恢复成功。 （由于存在许多文件夹，因此只恢复了一部分。）"},
                {"Deutsch","Wiederherstellen war erfolgreich. (Nur ein Teil wurde wiederhergestellt, da viele Ordner vorhanden sind.)"},
                {"Français","la restauration a réussi. (Seule une partie a été restaurée car il y a beaucoup de dossiers.)"},
                {"한국어","복원이 성공했습니다. 폴더가 많기 때문에 부품 만 복원되었습니다."},
                {"русский","восстановление прошло успешно. (Только часть была восстановлена, потому что есть много папок.)"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Restorecompleted = new Dictionary<string, string>()
            {
                {"日本語","復元に成功しました。"},
                {"English","Restoration was successful."},
                {"繁體中文","恢復成功。"},
                {"简体中文","恢复成功。"},
                {"Deutsch","Wiederherstellen war erfolgreich."},
                {"Français","la restauration a réussi."},
                {"한국어","복원이 성공했습니다."},
                {"русский","восстановление прошло успешно."},
                {"इंडिया",""}

            };
            public static Dictionary<string, string> Restorefaliure = new Dictionary<string, string>()
            {
                {"日本語","復元中にエラーが発生しました。バックアップが壊れています。"},
                {"English","An error occurred during restore. Backup is broken."},
                {"繁體中文","還原期間發生錯誤。備份被破壞了。"},
                {"简体中文","还原期间发生错误。备份被破坏了。"},
                {"Deutsch","Bei der Wiederherstellung ist ein Fehler aufgetreten. Sicherung ist defekt"},
                {"Français","Une erreur s'est produite lors de la restauration. La sauvegarde est cassée."},
                {"한국어","복원 중 오류가 발생했습니다. 백업이 끊어졌습니다."},
                {"русский","Произошла ошибка во время восстановления. Бэкап сломан."},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Noneofbackup = new Dictionary<string, string>()
            {
                {"日本語","バックアップが存在しません。"},
                {"English","Backup does not exist."},
                {"繁體中文","備份不存在。"},
                {"简体中文","备份不存在。"},
                {"Deutsch","Backup existiert nicht."},
                {"Français","La sauvegarde n'existe pas."},
                {"한국어","백업이 존재하지 않습니다."},
                {"русский","Резервная копия не существует."},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Changethemecolor = new Dictionary<string, string>()
            {
                {"日本語",""},
                {"English",""},
                {"繁體中文",""},
                {"简体中文",""},
                {"Deutsch",""},
                {"Français",""},
                {"한국어",""},
                {"русский",""},
                {"इंडिया",""}
            };


        }

        private enum RequestCode
        {
            RESLUT_CAMERA = 0,
            SELECT_CSVFILE = 1
        };

        private enum PermissionRequestCode
        {
            Create_Backup = 0,
            RecoverfromBackup = 1,
            ImportFromImage = 2
        };

        private class Recognizertext : Java.Lang.Object, IOnSuccessListener
        {
            //IntPtr IJavaObject.Handle => throw new NotImplementedException();
            IntPtr IJavaObject.Handle => base.Handle;

            void IOnSuccessListener.OnSuccess(Java.Lang.Object result)
            {
                var rslt = result as FirebaseVisionText;
                //throw new NotImplementedException();
            }

            #region IDisposable Support
            private bool disposedValue = false; // 重複する呼び出しを検出するには

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    }

                    // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                    // TODO: 大きなフィールドを null に設定します。

                    disposedValue = true;
                }
            }

            // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
            // ~Recognizertext()
            // {
            //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            //   Dispose(false);
            // }

            // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
            void IDisposable.Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
                Dispose(true);
                // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
                // GC.SuppressFinalize(this);
            }
            #endregion
        }

    }
}
