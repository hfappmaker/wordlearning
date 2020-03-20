//#define debug
using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Collections.Generic;
using Android.Content;
using static Android.Widget.AdapterView;
using Android.Views;
using Android.Transitions;
using System.Xml.Linq;
using Android.Graphics;
using System.Linq;
using System;
using System.Xml;
using System.Collections;
using Android.Content.Res;
using Android.Util;
using Android.Runtime;
using Firebase.Iid;
using Android.Gms.Common;
using static Android.Support.V7.Widget.RecyclerView;
using Android.Support.V7.Widget;

namespace WordLearning
{
    [Activity(Label = "@string/app_name", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class Start : CustomActivity
    {
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        private ListView listView;
        private Dictionary<List<int>, XElement> AllFolder;
        private int position/* selectedposition on listview */;
        //private int position_dlg;
        private string Itemtype = "Wordlist";
        /// <summary>
        /// Run Method
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Utility.GetLocaleId();//言語設定を取得
            XDocument xml;
#if debug
            //RootNodeのみのxmlを作成
            xml = new XDocument(
                                     new XDeclaration("1.0", "utf-8", "true"),
                                        new XElement("Root",
                                           new XElement("Folder", new XAttribute("Name", Constant.ToolbarTitle_Wordlist[Utility.language])),
                                           new XElement("Themecolor",
                                              new XElement("Red", defaultthemecolor[0]),
                                              new XElement("Green", defaultthemecolor[1]),
                                              new XElement("Blue", defaultthemecolor[2])
                                                       ),
                                           new XElement("Tagcolor",
                                              new XElement("Tagcolor1",
                                                 new XElement("Red", string.Empty),
                                                 new XElement("Green", string.Empty),
                                                 new XElement("Blue", string.Empty),
                                                 new XElement("Meaning",string.Empty),
                                                 new XElement("Shape",string.Empty)
                                                          ),
                                              new XElement("Tagcolor2",
                                                 new XElement("Red", string.Empty),
                                                 new XElement("Green", string.Empty),
                                                 new XElement("Blue", string.Empty),
                                                 new XElement("Meaning", string.Empty),
                                                 new XElement("Shape", string.Empty)
                                                          ),
                                              new XElement("Tagcolor3",
                                                 new XElement("Red", string.Empty),
                                                 new XElement("Green", string.Empty),
                                                 new XElement("Blue", string.Empty),
                                                 new XElement("Meaning", string.Empty),
                                                 new XElement("Shape", string.Empty)
                                                          ),
                                              new XElement("Tagcolor4",
                                                 new XElement("Red", string.Empty),
                                                 new XElement("Green", string.Empty),
                                                 new XElement("Blue", string.Empty),
                                                 new XElement("Meaning", string.Empty),
                                                 new XElement("Shape", string.Empty)
                                                          ),
                                              new XElement("Tagcolor5",
                                                 new XElement("Red", string.Empty),
                                                 new XElement("Green", string.Empty),
                                                 new XElement("Blue", string.Empty),
                                                 new XElement("Meaning", string.Empty),
                                                 new XElement("Shape", string.Empty)
                                                          )
                                                       )
                                                    )
                                    );
            xml.Save(Utility.WordListPath);
#endif
            if (!File.Exists(Utility.WordListPath))
            {
                //RootNodeのみのxmlを作成
                xml = new XDocument(
                                         new XDeclaration("1.0", "utf-8", "true"),
                                            new XElement("Root",
                                               new XElement("Folder", new XAttribute("Name", Constant.ToolbarTitle_Wordlist[Utility.language])),
                                               new XElement("Themecolor",
                                                  new XElement("Red", defaultthemecolor[0]),
                                                  new XElement("Green", defaultthemecolor[1]),
                                                  new XElement("Blue", defaultthemecolor[2])
                                                           ),
                                               new XElement("Tagcolor",
                                                  new XElement("Tagcolor1",
                                                     new XElement("Red", string.Empty),
                                                     new XElement("Green", string.Empty),
                                                     new XElement("Blue", string.Empty),
                                                     new XElement("Meaning", string.Empty),
                                                     new XElement("Shape", string.Empty)
                                                              ),
                                                  new XElement("Tagcolor2",
                                                     new XElement("Red", string.Empty),
                                                     new XElement("Green", string.Empty),
                                                     new XElement("Blue", string.Empty),
                                                     new XElement("Meaning", string.Empty),
                                                     new XElement("Shape", string.Empty)
                                                              ),
                                                  new XElement("Tagcolor3",
                                                     new XElement("Red", string.Empty),
                                                     new XElement("Green", string.Empty),
                                                     new XElement("Blue", string.Empty),
                                                     new XElement("Meaning", string.Empty),
                                                     new XElement("Shape", string.Empty)
                                                              ),
                                                  new XElement("Tagcolor4",
                                                     new XElement("Red", string.Empty),
                                                     new XElement("Green", string.Empty),
                                                     new XElement("Blue", string.Empty),
                                                     new XElement("Meaning", string.Empty),
                                                     new XElement("Shape", string.Empty)
                                                              ),
                                                  new XElement("Tagcolor5",
                                                     new XElement("Red", string.Empty),
                                                     new XElement("Green", string.Empty),
                                                     new XElement("Blue", string.Empty),
                                                     new XElement("Meaning", string.Empty),
                                                     new XElement("Shape", string.Empty)
                                                              )
                                                           )
                                                        )
                                        );
                xml.Save(Utility.WordListPath);
            } 
            xml = XDocument.Load(Utility.WordListPath);
            if (!xml.Root.Element("Tagcolor").Elements().First().Elements().Any(p => p.Name == "Shape"))
            {
                xml.Root.Element("Tagcolor").Elements().ToList().ForEach(p => p.Add(new XElement("Shape", "Square")));
                xml.Save(Utility.WordListPath);
            }
            xml = XDocument.Load(Utility.WordListPath);
            xml.Root.Descendants("Word").ToList().ForEach(elm =>
            {
                if (!elm.Elements().ToList().Exists(elm2 => elm2.Name == "Memo"))
                {
                    elm.Add(new XElement("Memo", string.Empty));
                    xml.Save(Utility.WordListPath);
                }
            });
            xml = XDocument.Load(Utility.WordListPath);
            //If There are Activities except Start. Destroy this Start activity.
            if (Utility.MultipleActivityFlag)
            {
                Finish();
                return;
            }
            SetlistView();
            IsPlayServicesAvailable();
            CreateNotificationChannel();
            //string h = FirebaseInstanceId.Instance.Token;
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            IsPlayServicesAvailable();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        #region event

        #region listview click
        /// <summary>
        /// When lv_Register's item is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void lv_Start_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (basetoolbarResID == Resource.Id.tbStart_Deletemode)
            {
                Utility.selectpositions[e.Position] = !Utility.selectpositions[e.Position];
                ResetlistView();
                if (Utility.selectpositions.Count(p => p) != 1)
                {
                    if (!Utility.selectpositions.Any(p => p))
                    {
                        menu.SetGroupVisible(0, false);
                    }
                    else
                    {
                        menu.GetItem(2).SetVisible(true);
                        menu.GetItem(3).SetVisible(true);
                        menu.GetItem(1).SetVisible(false);
                        ArrayAdapter_Start a = (ArrayAdapter_Start)listView.Adapter;
                        if (Utility.selectpositions.Select((p, i) => new { p, i }).Where(q => q.p).All(p => a.list[p.i].Type == "Wordlist"))
                        {
                            menu.GetItem(0).SetVisible(true);
                        }
                        else
                        {
                            menu.GetItem(0).SetVisible(false);
                        }
                    }
                }
                else
                {
                    menu.GetItem(0).SetVisible(false);
                    menu.GetItem(1).SetVisible(true);
                    menu.GetItem(2).SetVisible(true);
                    menu.GetItem(3).SetVisible(true);
                    position = Utility.selectpositions.IndexOf(true);
                }
            }
            else
            {
                switch (Utility.Start_Elements[e.Position].Type)
                {
                    case "Folder":
                        Utility.cd.Add(Utility.Start_Elements[e.Position].Number);
                        SetlistView();
                        break;
                    case "Wordlist":
                        position = e.Position;
                        Intent intent;
                        Utility.cd.Add(Utility.Start_Elements[position].Number);
                        Utility.MultipleActivityFlag = true;
                        intent = new Intent(this, typeof(Edit_Wordlist));
                        StartActivity(intent);
                        break;
                }
            }
        }

        /// <summary>
        /// When lv_Register's item is clicked long time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void lv_Start_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {
            if (basetoolbarResID == Resource.Id.tbStart_Init)
            {
                position = e.Position;
                listView.GetChildAt(position - listView.FirstVisiblePosition).SetBackgroundColor(Constant.SelectColor[true]);
                Utility.selectpositions[position] = true;
                listView.ChoiceMode = ChoiceMode.Multiple;
                ChangeToolbarTransition(Transitionmode.ToMultiple);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            Intent intent;
            //string h = FirebaseInstanceId.Instance.Token;
            var xml = XDocument.Load(Utility.WordListPath);
            switch (item.ItemId)
            {
                case Resource.Id.action_add_Start_Init:
                    var dlgCreate = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlgCreate.SetItems(Message.Add[Utility.language], Add);
                    dlgCreate.SetPositiveButton("CANCEL", (_s, _e) => { });
                    dlgCreate.Show();
                    if (Utility.cd.Count == 2)
                    {
                        SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                        SupportActionBar.SetHomeButtonEnabled(false);
                    }
                    break;
                case Resource.Id.action_settings_Start_Init:
                    Utility.MultipleActivityFlag = true;
                    intent = new Intent(this, typeof(Settings));
                    StartActivity(intent);
                    break;
                case Android.Resource.Id.Home:
                    if (basetoolbarResID == Resource.Id.tbStart_Deletemode)
                    {
                        listView.ChoiceMode = ChoiceMode.Single;
                        KeyValuePairs[Transitionmode.ToInit][TransitionSet.Title] = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value);
                        ChangeToolbarTransition(Transitionmode.ToInit);
                        for (int i = 0; i < listView.Count; i++)
                        {
                            listView.GetChildAt(i)?.SetBackgroundColor(Constant.SelectColor[false]);
                            Utility.selectpositions[i] = false;
                        }
                    }
                    else
                    {
                        Utility.cd.RemoveAt(Utility.cd.Count - 1);
                        SetContentViewAndToolbar(Resource.Layout.Start, Resource.Id.tbStart, Resource.Menu.menu_Start_Init);
                        listView = FindViewById<ListView>(Resource.Id.lv_Start);
                        Utility.Start_Elements = Utility.Start_Elements.OrderBy(elm => (elm.Name.Length, elm.Type, elm.Name)).ToList();
                        listView.Adapter = new ArrayAdapter_Start(this, Resource.Layout.row_Explorer, Utility.Start_Elements);
                        Utility.selectpositions = new List<bool>();
                        for (int i = 0; i < Utility.Start_Elements.Count; i++)
                        {
                            Utility.selectpositions.Add(false);
                        }
                    }
                    if (Utility.cd.Count == 2)
                    {
                        SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                        SupportActionBar.SetHomeButtonEnabled(false);
                    }
                    break;
                case Resource.Id.action_delete_Start_Deletemode:
                    var dlgDeleteCheck = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlgDeleteCheck.SetMessage(Message.DeleteConfirm[Utility.language]);
                    dlgDeleteCheck.SetNegativeButton("OK", DeleteMultipleMode);
                    dlgDeleteCheck.SetNeutralButton("CANCEL", (_s, _e) =>
                    {
                        KeyValuePairs[Transitionmode.ToInit][TransitionSet.Title] = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value);
                        ChangeToolbarTransition(Transitionmode.ToInit);
                        return;
                    });
                    dlgDeleteCheck.Show();
                    break;
                case Resource.Id.action_move_Start_Deletemode:
                    //2019.07.26 ExpandableListView
                    //ExpandableListView expandableListView = new ExpandableListView(this);
                    var dlgMove = new Android.Support.V7.App.AlertDialog.Builder(this);
                    AllFolder = GetAllFolder(xml);
                    List<int> ChooseItemPath = new List<int>(Utility.cd);
                    Predicate<List<int>> predicate = list =>
                    {
                        for (int i = 0; i < Utility.selectpositions.Count; i++)
                        {
                            if (Utility.selectpositions[i])
                            {
                                Utility.cd.Add(Utility.Start_Elements[i].Number);
                                ChooseItemPath = new List<int>(Utility.cd);
                                if (list.Count >= ChooseItemPath.Count)
                                {
                                    List<int> vs = list.GetRange(0, ChooseItemPath.Count);
                                    if (vs.SequenceEqual(ChooseItemPath))
                                    {
                                        Utility.cd.RemoveAt(Utility.cd.Count - 1);
                                        return true;
                                    }
                                }
                                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                            }
                        }
                        return false;
                    };
                    //var AllFolder_exceptmyselfandparent = new List<KeyValuePair<List<int>, XElement>>((from folder in AllFolder
                    //                                                                                   where (!folder.Key.SequenceEqual(Utility.cd) && !predicate(folder.Key))
                    //                                                                                   select folder).ToList());
                    //AllFolder = new Dictionary<List<int>, XElement>(AllFolder_exceptmyselfandparent);
                    LayoutInflater Inflater;
                    Inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
                    View layout = Inflater.Inflate(Resource.Layout.Dialog_Move_Start, (ViewGroup)FindViewById(Resource.Id.ll_Dialog_Move_Start));
                    //ListView listFolder = layout.FindViewById<ListView>(Resource.Id.lv_Dialog_Move_Start);
                    RecyclerView listFolder = layout.FindViewById<RecyclerView>(Resource.Id.lv_Dialog_Move_Start);
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
                    dlgMove.SetMessage(Message.Selectdestination[Utility.language]);
                    dlgMove.SetNegativeButton(Message.Wordofmove[Utility.language], Move);
                    dlgMove.SetNeutralButton("CANCEL", (_s, _e) => { for (int i = 0; i < Utility.selectpositions.Count; i++) { Utility.selectpositions[i] = false; } ChangeToolbarTransition(Transitionmode.ToInit); ResetlistView(); });
                    dlgMove.SetView(layout);
                    dlgMove.SetCancelable(false);
                    dlgMove.Show();
                    break;
                case Resource.Id.action_rename_Start_Deletemode:
                    var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                    EditText etxtName = new EditText(this)
                    {
                        Id = Constant.FreeDlgId
                    };
                    dlg.SetView(etxtName);
                    dlg.SetMessage(Message.Setname[Utility.language]);
                    dlg.SetNegativeButton("OK", Rename);
                    dlg.SetPositiveButton("CANCEL", (_s, _e) => { });
                    dlg.SetCancelable(false);
                    var dlg2 = dlg.Create();
                    dlg2.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
                    dlg2.Show();
                    etxtName.RequestFocus();
                    break;
                case Resource.Id.action_integration_Start_Deletemode:
                    var dlgintegration = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlgintegration.SetMessage(Message.IntegrationConfirm[Utility.language]);
                    dlgintegration.SetPositiveButton("OK", Integrate);
                    dlgintegration.SetNegativeButton("CANCEL", (_s, _e) =>
                    {
                        for (int i = 0; i < Utility.selectpositions.Count; i++) { Utility.selectpositions[i] = false; }
                        ChangeToolbarTransition(Transitionmode.ToInit);
                        ResetlistView();
                    });
                    dlgintegration.Show();
                    break;
            }
            listView = FindViewById<ListView>(Resource.Id.lv_Start);
            return base.OnOptionsItemSelected(item);
        }
        /// <summary>
        /// Integrate wordlists.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void Integrate(object sender, DialogClickEventArgs e)
        {
            var ed = new EditText(this) { Id = Constant.FreeDlgId };
            var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
            dlg.SetMessage(Message.Setname[Utility.language]);
            dlg.SetView(ed);
            dlg.SetPositiveButton("OK", (s1, e1) =>
            {
                var dlg2 = (Android.Support.V7.App.AlertDialog)s1;
                string filename = XmlConvert.EncodeLocalName(dlg2.FindViewById<EditText>(Constant.FreeDlgId).Text);
                if (NameCheck(filename, dlg2))
                {
                    var xml = XDocument.Load(Utility.WordListPath);
                    var Integratelist = new List<XElement>();
                    var Newwordlist = new XElement("Wordlist", new XAttribute("Name", filename),new XAttribute("Sleepcount",5),new XAttribute("VoicelanguageWord","English"),new XAttribute("VoicelanguageMeaning",Utility.language));
                    for (int i = 0; i < Utility.selectpositions.Count; i++)
                    {
                        if (Utility.selectpositions[i])
                        {
                            Utility.cd.Add(Utility.Start_Elements[i].Number);
                            Integratelist.Add(Utility.GetXElement(Utility.cd, xml));
                            Utility.cd.RemoveAt(Utility.cd.Count - 1);
                        }
                    }
                    Integratelist.ForEach(elm =>
                    {
                        Newwordlist.Add(elm.Elements().Where(elm1 => elm1.Name == "Word"));
                        elm.Remove();
                    });
                    //Newwordlist.Add(new XElement("SleepCount",5));
                    //Newwordlist.Add(new XElement("VoicelanguageWord", "English"));
                    //Newwordlist.Add(new XElement("VoicelanguageMeaning", Utility.language));
                    Utility.GetXElement(Utility.cd, xml).Add(Newwordlist);
                    xml.Save(Utility.WordListPath);
                    ChangeToolbarTransition(Transitionmode.ToInit);
                    SetlistView();
                }
            });
            dlg.SetNegativeButton("CANCEL", (s1, e1) =>
            {
                for (int i = 0; i < Utility.selectpositions.Count; i++) { Utility.selectpositions[i] = false; }
                ChangeToolbarTransition(Transitionmode.ToInit);
                ResetlistView();
            });
            dlg.Show();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseAction_Wordlist(object sender, DialogClickEventArgs e)
        {
            Intent intent;
            var xml = XDocument.Load(Utility.WordListPath);
            Utility.cd.Add(Utility.Start_Elements[position].Number);
            Utility.WordListName = Utility.Start_Elements[position].Name;
            XElement Wordlist = Utility.GetXElement(Utility.cd, xml);
            var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
            switch (e.Which)
            {
                case 0:// Learn wordlist
                    if (Wordlist.HasElements && Wordlist.Name == "Wordlist")
                    {
                        LayoutInflater inflater = LayoutInflater.From(this);
                        View view = inflater.Inflate(Resource.Layout.Dialog_SetLearnWord, null);
                        var TagList = view.FindViewById<ListView>(Resource.Id.lvTagList_Dialog_SetLearnWord);
                        var xelm = XDocument.Load(Utility.WordListPath);
                        var wkTags = new List<(int, int[], string, bool)>();
                        var Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
                        var dlgEditTagofWord = new Android.Support.V7.App.AlertDialog.Builder(this);
                        for (int i = 0; i < Tags_xml.Count; i++)
                        {
                            if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                            {
                                wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value), false));
                            }
                        }
                        wkTags.Add((5, new int[] { 255, 255, 255 }, Message.NoTag[Utility.language], true));
                        TagList.Adapter = new ArrayAdapter_SelectedTag(this, Resource.Layout.row_SelectedTag, wkTags);
                        dlg.SetView(view);
                        var dlgins = dlg.Create();
                        dlgins.SetButton((int)DialogButtonType.Negative, "OK", MoveLearn_Wordlist);
                        dlgins.SetButton((int)DialogButtonType.Positive, "CANCEL", (_s, _e) =>
                        {
                            Utility.cd.RemoveAt(Utility.cd.Count - 1);
                            Utility.WordListName = string.Empty;
                        });
                        dlgins.Show();
                    }
                    else
                    {
                        dlg.SetMessage(Message.NoRegisteredword[Utility.language]);
                        dlg.SetPositiveButton("OK", (_s, _e) => { return; });
                        dlg.Show();
                        Utility.cd.RemoveAt(Utility.cd.Count - 1);
                        Utility.WordListName = string.Empty;
                    }
                    break;
                case 1:// Edit wordlist
                    if (Wordlist.Name == "Wordlist")
                    {
                        Utility.MultipleActivityFlag = true;
                        intent = new Intent(this, typeof(Edit_Wordlist));
                        StartActivity(intent);
                    }
                    break;
            }
        }
        /// <summary>
        /// Handles the event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void MoveLearn_Wordlist(object sender, DialogClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog dlg = (Android.Support.V7.App.AlertDialog)sender;
            Intent intent = new Intent(this, typeof(Learn_Wordlist));
            RadioGroup radioGroup = dlg.FindViewById<RadioGroup>(Resource.Id.rdogSelectOrder_Dialog_SetLearnWord);
            ListView Taglist = dlg.FindViewById<ListView>(Resource.Id.lvTagList_Dialog_SetLearnWord);
            ArrayAdapter_SelectedTag taglistadapter = (ArrayAdapter_SelectedTag)Taglist.Adapter;
            var list = taglistadapter.list;
            string tag = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item5)
                {
                    tag += list[i].Item1.ToString();
                }
            }
            bool predicate(KeyValuePair<int, (string Wordname, string Wordmeaning, string Tag, string Memo)> wordandmeaning)
            {
                foreach (char selectTag in tag)
                {
                    if (int.Parse(selectTag.ToString()) < 5)
                    {
                        if (wordandmeaning.Value.Tag[int.Parse(selectTag.ToString())] == '1') return true;
                    }
                    else
                    {
                        if (wordandmeaning.Value.Tag == "00000") return true;
                    }
                }
                return false;
            }
            Utility.Read_Wordlist();
            Utility.WordandMeanings = (from wordandmeaning in Utility.WordandMeanings
                                       where predicate(wordandmeaning)
                                       select wordandmeaning).ToDictionary(p => p.Key, p => p.Value);
            if (Utility.WordandMeanings.Count == 0)
            {
                Android.Support.V7.App.AlertDialog.Builder dlgbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlgbuilder.SetMessage(Message.NoMatchedword[Utility.language]);
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                Utility.WordListName = string.Empty;
                dlgbuilder.SetPositiveButton("OK", (_s, _e) => { });
                dlgbuilder.Show();
            }
            else
            {
                Utility.MultipleActivityFlag = true;
                intent.PutExtra("RadioButton", radioGroup.CheckedRadioButtonId);
                StartActivity(intent);
            }
        }
        /// <summary>
        /// Get wordandmeanings.
        /// </summary>
        private void GetWordandMeanings()
        {
            Predicate<KeyValuePair<int, (string Wordname, string Wordmeaning, string Tag, string Memo)>> predicate = wordandmeaning =>
             {
                 foreach (char selectTag in Intent.GetStringExtra("Tag"))
                 {
                     if (int.Parse(selectTag.ToString()) < 5)
                     {
                         if (wordandmeaning.Value.Tag[int.Parse(selectTag.ToString())] == '1') return true;
                     }
                     else
                     {
                         if (wordandmeaning.Value.Tag == "00000") return true;
                     }
                 }
                 return false;
             };
            Utility.WordandMeanings = (from wordandmeaning in Utility.WordandMeanings
                                       where predicate(wordandmeaning)
                                       select wordandmeaning).ToDictionary(p => p.Key, p => p.Value);
            switch (Intent.GetIntExtra("RadioButton", Resource.Id.rdoAscendant))
            {
                case Resource.Id.rdoAscendant:
                    Utility.WordandMeanings.OrderBy(p => p.Value.Wordname);
                    break;
                case Resource.Id.rdoDescendant:
                    Utility.WordandMeanings.OrderByDescending(p => p.Value.Wordname);
                    break;
                case Resource.Id.rdoRandomize:
                    Utility.WordandMeanings.OrderBy(p => Guid.NewGuid());
                    break;
            }
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
        /// <summary>
        /// Add new wordlist or folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add(object sender, DialogClickEventArgs e)
        {
            EditText etxtName;
            var dlgNameSet = new Android.Support.V7.App.AlertDialog.Builder(this);
            Android.Support.V7.App.AlertDialog dlgNameSet2;
            switch (e.Which)
            {
                case 0://add Wordlist
                    Itemtype = "Wordlist";
                    break;
                case 1://add Folder
                    Itemtype = "Folder";
                    break;
            }
            etxtName = new EditText(this)
            {
                Id = Constant.FreeDlgId
            };
            dlgNameSet.SetView(etxtName);
            dlgNameSet.SetMessage(Message.Setname[Utility.language]);
            dlgNameSet.SetNegativeButton("CANCEL", (_sender, _e) => { return; });
            dlgNameSet.SetPositiveButton("OK", CreateNewItem);
            dlgNameSet2 = dlgNameSet.Create();
            dlgNameSet2.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
            dlgNameSet2.Show();
            etxtName.RequestFocus();

        }
        /// <summary>
        /// When returning from Edit_Wordlist
        /// </summary>
        protected override void OnRestart()
        {
            base.OnRestart();
            try
            {
                Utility.MultipleActivityFlag = false;
                SetlistView();
            }
            catch (Exception)
            {
                Utility.cd = new List<int>() { 0, 0 };
                SetlistView();
            }
        }
        #endregion

        #region DelteItem
        /// <summary>
        /// Deletes the multiple.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void DeleteMultiple(object sender, DialogClickEventArgs e)
        {
            listView.ChoiceMode = ChoiceMode.Multiple;
            ChangeToolbarTransition(Transitionmode.ToMultiple);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete(object sender, DialogClickEventArgs e)
        {
            var xml = XDocument.Load(Utility.WordListPath);
            Utility.cd.Add(Utility.Start_Elements[position].Number);
            Utility.GetXElement(Utility.cd, xml).Remove();
            Utility.cd.RemoveAt(Utility.cd.Count - 1);
            xml.Save(Utility.WordListPath);
            SetlistView();
            Toast.MakeText(this, Message.Delete[Utility.language], ToastLength.Short).Show();
        }
        /// <summary>
        /// Ons the key down.
        /// </summary>
        /// <returns><c>true</c>, if key down was oned, <c>false</c> otherwise.</returns>
        /// <param name="keyCode">Key code.</param>
        /// <param name="e">E.</param>
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && basetoolbarResID == Resource.Id.tbStart_Deletemode)
            {
                listView.ChoiceMode = ChoiceMode.Single;
                var xml = XDocument.Load(Utility.WordListPath);
                KeyValuePairs[Transitionmode.ToInit][TransitionSet.Title] = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value);
                ChangeToolbarTransition(Transitionmode.ToInit);
                for (int i = 0; i < listView.Count; i++)
                {
                    listView.GetChildAt(i)?.SetBackgroundColor(Constant.SelectColor[false]);
                    Utility.selectpositions[i] = false;
                }
                return false;
            }
            else if (keyCode == Keycode.Back && !Utility.cd.SequenceEqual(new int[] { 0, 0 }))
            {
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                SetlistView();
                return false;
            }
            return base.OnKeyDown(keyCode, e);
        }
        /// <summary>
        /// Deletes the multiple mode.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void DeleteMultipleMode(object sender, DialogClickEventArgs e)
        {
            var xml = XDocument.Load(Utility.WordListPath);
            var Removelist = new List<XElement>();
            for (int i = 0; i < listView.Count; i++)
            {
                if (Utility.selectpositions[i] == true)
                {
                    Utility.cd.Add(Utility.Start_Elements[i].Number);
                    Removelist.Add(Utility.GetXElement(Utility.cd, xml));
                    Utility.cd.RemoveAt(Utility.cd.Count - 1);
                }
            }
            Removelist.ForEach(elm => elm.Remove());
            xml.Save(Utility.WordListPath);
            ChangeToolbarTransition(Transitionmode.ToInit);
            SetlistView();
            Toast.MakeText(this, Message.Delete[Utility.language], ToastLength.Short).Show();
            return;
        }
        #endregion

        #region MoveItem
        /// <summary>
        /// Move the specified sender and e.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void Move(object sender, DialogClickEventArgs e)
        {
            var dlg = (Android.Support.V7.App.AlertDialog)sender;
            RecyclerView listFolder = dlg.FindViewById<RecyclerView>(Resource.Id.lv_Dialog_Move_Start);
            ArrayAdapter_Start_Move adapter = listFolder.GetAdapter() as ArrayAdapter_Start_Move;
            var xml = XDocument.Load(Utility.WordListPath);
            if (adapter.selectedfolderposition.SequenceEqual(Utility.cd))
            {
                var dlgSameLocateAlart = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlgSameLocateAlart.SetMessage(Message.Movevalidate[Utility.language]);
                dlgSameLocateAlart.SetNegativeButton("OK", (_s, _e) => { dlg.Show(); return; });
                dlgSameLocateAlart.SetCancelable(false);
                dlgSameLocateAlart.Show();
                return;
            }
            var xmlcdNode = Utility.GetXElement(adapter.selectedfolderposition, xml);
            for (int i = 0; i < Utility.selectpositions.Count; i++)
            {
                if (Utility.selectpositions[i])
                {
                    Utility.cd.Add(Utility.Start_Elements[i].Number);

                    if(!(adapter.selectedfolderposition.Count >= Utility.cd.Count &&
                        adapter.selectedfolderposition.Take(Utility.cd.Count).SequenceEqual(Utility.cd)))
                    {
                        xmlcdNode.Add(Utility.GetXElement(Utility.cd, xml));
                    }
                    Utility.cd.RemoveAt(Utility.cd.Count - 1);
                }
            }
            xml.Save(Utility.WordListPath);
            var Removelist = new List<XElement>();
            for (int i = 0; i < listView.Count; i++)
            {
                if (Utility.selectpositions[i])
                {
                    Utility.cd.Add(Utility.Start_Elements[i].Number);

                    if (!(adapter.selectedfolderposition.Count >= Utility.cd.Count &&
                        adapter.selectedfolderposition.Take(Utility.cd.Count).SequenceEqual(Utility.cd)))
                    {
                        Removelist.Add(Utility.GetXElement(Utility.cd, xml));
                    }
                    Utility.cd.RemoveAt(Utility.cd.Count - 1);
                }
            }
            Removelist.ForEach(elm => elm.Remove());
            xml.Save(Utility.WordListPath);
            SetlistView();
            Toast.MakeText(this, Message.Move[Utility.language], ToastLength.Short).Show();
            return;
        }
        #endregion

        #region RenameItem
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rename(object sender, DialogClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog dlg = (Android.Support.V7.App.AlertDialog)sender;
            string Newname = dlg.FindViewById<EditText>(Constant.FreeDlgId).Text;
            if (NameCheck(XmlConvert.DecodeName(Newname), dlg))
            {
                var xml = XDocument.Load(Utility.WordListPath);
                Utility.cd.Add(Utility.Start_Elements[position].Number);
                Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value = XmlConvert.EncodeLocalName(Newname);
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                xml.Save(Utility.WordListPath);
                Utility.selectpositions[position] = !Utility.selectpositions[position];
                SetlistView();
            }
        }
        #endregion

        #region CreateItem
        /// <summary>
        /// Create NewItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewItem(object sender, DialogClickEventArgs e)
        {
            var dlgNameSet = (Android.Support.V7.App.AlertDialog)sender;
            string file_name = XmlConvert.EncodeLocalName(dlgNameSet.FindViewById<EditText>(Constant.FreeDlgId).Text);
            if (NameCheck(XmlConvert.DecodeName(file_name), dlgNameSet))
            {
                var xml = XDocument.Load(Utility.WordListPath);
                var Folder = Utility.GetXElement(Utility.cd, xml);
                if (Folder.Name == "Folder")
                {
                    if (Itemtype == "Folder")
                    {
                        Folder.Add(new XElement(Itemtype, new XAttribute("Name", file_name)));
                    }
                    else if(Itemtype == "Wordlist") 
                    {
                        Folder.Add(new XElement(Itemtype, new XAttribute("Name", file_name),new XAttribute("Sleepcount",5),new XAttribute("VoicelanguageWord","English"),new XAttribute("VoicelanguageMeaning",Utility.language)));
                    }
                }
                xml.Save(Utility.WordListPath);
                SetlistView();
            }
        }
        /// <summary>
        /// Check name
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private bool NameCheck(string file_name, Android.Support.V7.App.AlertDialog dlg)
        {
            var dlgWarning = new Android.Support.V7.App.AlertDialog.Builder(this);
            if (string.IsNullOrEmpty(file_name))
            {
                dlgWarning.SetMessage(Message.Setname[Utility.language]);
                dlgWarning.SetNeutralButton("OK", (_sender, _e) => { dlg.Show(); });
                dlgWarning.Show();
                return false;
            }
            return true;
        }

        #endregion

        #region Constant
        Dictionary<Transitionmode, Dictionary<TransitionSet, object>> KeyValuePairs { get; set; } = new Dictionary<Transitionmode, Dictionary<TransitionSet, object>>()
        {
            {
                Transitionmode.ToMultiple,
                new Dictionary<TransitionSet,object>()
                {
                  { TransitionSet.SceneLayout,Resource.Layout.Toolbar_Start_Deletemode},
                  { TransitionSet.ToolbarLayout,Resource.Id.tbStart_Deletemode },
                  { TransitionSet.menuResID, Resource.Menu.menu_Start_Deletemode},
                  { TransitionSet.HomeButtonEnabled,true},
                  { TransitionSet.Title, string.Empty}
                }
            },
            {
                Transitionmode.ToInit,
                new Dictionary<TransitionSet,object>()
                {
                    { TransitionSet.SceneLayout,Resource.Layout.Toolbar_Start_Init},
                    { TransitionSet.ToolbarLayout,Resource.Id.tbStart_Init},
                    { TransitionSet.menuResID,Resource.Menu.menu_Start_Init},
                    { TransitionSet.HomeButtonEnabled,true},
                    { TransitionSet.Title, Utility.WordListName}
                }
            }
        };

        private enum TransitionSet { SceneLayout, ToolbarLayout, menuResID, HomeButtonEnabled, Title }

        private enum Transitionmode { ToMultiple, ToInit }

        //private static Dictionary<string, string> TypeName = new Dictionary<string, string>() { { "Folder", "フォルダ" }, { "Wordlist", "単語帳" } };

        #endregion

        #region Method
        /// <summary>
        /// Set listview.
        /// </summary>
        void SetlistView()
        {
            SetContentViewAndToolbar(Resource.Layout.Start, Resource.Id.tbStart, Resource.Menu.menu_Start_Init);
            listView = FindViewById<ListView>(Resource.Id.lv_Start);
            Utility.selectpositions = new List<bool>();
            for (int i = 0; i < Utility.Start_Elements.Count; i++)
            {
                Utility.selectpositions.Add(false);
            }
            Utility.Start_Elements = Utility.Start_Elements.OrderBy(elm => (elm.Name.Length, elm.Type, elm.Name)).ToList();
            listView.Adapter = new ArrayAdapter_Start(this, Resource.Layout.row_Explorer, Utility.Start_Elements);
        }
        /// <summary>
        /// Reset listview.
        /// </summary>
        void ResetlistView()
        {
            Utility.Get_Wordlists();
            int Y = 0, position_scroll = 0;
            if (listView != null)
            {
                position_scroll = listView.FirstVisiblePosition;
                Y = listView.GetChildAt(0).Top;
            }
            Utility.Start_Elements = Utility.Start_Elements.OrderBy(elm => (elm.Name.Length, elm.Type, elm.Name)).ToList();
            listView.Adapter = new ArrayAdapter_Start(this, Resource.Layout.row_Explorer, Utility.Start_Elements);
            listView.SetSelectionFromTop(position_scroll, Y);
        }

        /// <summary>
        /// Transition Toolbar When Transitionmode was changed
        /// </summary>
        /// <param name="transitionmode"></param>
        private void ChangeToolbarTransition(Transitionmode transitionmode)
        {
            ViewGroup rootscene;
            Scene scene;
            Fade transition;
            rootscene = (ViewGroup)FindViewById(Resource.Id.flToolbar_Start);
            scene = Scene.GetSceneForLayout(rootscene, int.Parse(KeyValuePairs[transitionmode][TransitionSet.SceneLayout].ToString()), this);
            transition = new Fade();
            transition.SetDuration(150L);
            TransitionManager.Go(scene, transition);
            menuResID = int.Parse(KeyValuePairs[transitionmode][TransitionSet.menuResID].ToString());
            basetoolbarResID = int.Parse(KeyValuePairs[transitionmode][TransitionSet.ToolbarLayout].ToString());
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(int.Parse(KeyValuePairs[transitionmode][TransitionSet.ToolbarLayout].ToString()));
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(bool.Parse(KeyValuePairs[transitionmode][TransitionSet.HomeButtonEnabled].ToString()));
            SupportActionBar.SetHomeButtonEnabled(bool.Parse(KeyValuePairs[transitionmode][TransitionSet.HomeButtonEnabled].ToString()));
            SupportActionBar.Title = KeyValuePairs[transitionmode][TransitionSet.Title].ToString();
            if (Utility.cd.Count == 2 && transitionmode == Transitionmode.ToInit)
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            listView = FindViewById<ListView>(Resource.Id.lv_Start);
            if (transitionmode == Transitionmode.ToInit)
            {
                var xml = XDocument.Load(Utility.WordListPath);
                SupportActionBar.Title = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value);
                toolbar.BackgroundTintList = ColorStateList.ValueOf(themecolor);
                for (int i = 0; i < listView.Count; i++)
                {
                    listView.GetChildAt(i)?.SetBackgroundColor(Constant.SelectColor[false]);
                    Utility.selectpositions[i] = false;
                }
            }
        }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                     GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    //msgText.Text = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                //msgText.Text = "Google Play Services is available.";
                return true;
            }
        }

        #endregion

        #region Message
        private static class Message
        {
            public static Dictionary<string, string> DeleteConfirm = new Dictionary<string, string>()
            {
                {"日本語","選択した項目を削除します。" + System.Environment.NewLine + "宜しいですか？"},
                {"English","Delete the selected items." + System.Environment.NewLine + "Is it OK？"},
                {"繁體中文","刪除所選項目。" + System.Environment.NewLine + "可以嗎？"},
                {"简体中文","删除所选项目。" + System.Environment.NewLine + "可以吗？"},
                {"Deutsch","Löschen Sie die ausgewählten Elemente." + System.Environment.NewLine + "Ist es o.k?"},
                {"Français","Supprimer les éléments sélectionnés." + System.Environment.NewLine + "Est-ce que c'est bon?"},
                {"한국어","선택한 항목을 삭제하십시오." + System.Environment.NewLine + "괜찮습니까?"},
                {"русский","Удалить выбранные элементы." + System.Environment.NewLine + "Это нормально?"},
                {"इंडिया",""}
             };
            public static Dictionary<string, string> Selectdestination = new Dictionary<string, string>()
            {
                { "日本語","移動先を選択して下さい" },
                { "English","Please select a destination" },
                {"繁體中文","請選擇目的地"},
                {"简体中文","请选择目的地"},
                {"Deutsch","Bitte wählen Sie ein Ziel aus"},
                {"Français","S'il vous plaît sélectionner une destination"},
                {"한국어","목적지를 선택하십시오."},
                {"русский","Пожалуйста, выберите пункт назначения"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Setname = new Dictionary<string, string>()
            {
                { "日本語","名前（ファイル、単語帳）を入力して下さい。" },
                { "English","Please set name(file,wordlist)." },
                { "繁體中文","請設置名稱（文件，詞彙書）" },
                { "简体中文","请设置名称（文件，词汇书）" },
                { "Deutsch","Bitte geben Sie den Namen (Datei, Vokabeln) an." },
                { "Français","S'il vous plaît définir le nom (fichier, vocabulaire)" },
                { "한국어","이름 (파일, 어휘집)을 설정하십시오." },
                { "русский","Пожалуйста, укажите имя (файл, словарный запас)" },
                {"इंडिया",""}

            };
            public static Dictionary<string, string> IntegrationConfirm = new Dictionary<string, string>()
            {
                { "日本語","選択した単語帳を一つの単語帳に統合します。" + System.Environment.NewLine + "宜しいですか？" },
                { "English","Merge selected wordlists into one wordlist." + System.Environment.NewLine + "Is it OK?"},
                { "繁體中文","將選定的單詞列表合併為一個單詞表。" + System.Environment.NewLine + "可以嗎？"},//2019.03.02
                { "简体中文","将选定的单词列表合并为一个单词表。" + System.Environment.NewLine + "可以吗？"},
                { "Deutsch","Ausgewählte Wortlisten in einer Wortliste zusammenführen." + System.Environment.NewLine + "Ist es o.k?"},
                { "Français","Fusionnez les listes de mots sélectionnées en une seule." + System.Environment.NewLine + "Est-ce que c'est bon?"},
                { "한국어","선택한 단어 목록을 하나의 단어 목록으로 병합합니다." + System.Environment.NewLine + "괜찮습니까?"},
                { "русский","Объединить выбранные списки слов в один список слов." + System.Environment.NewLine + "Это нормально?"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Setwordlistname = new Dictionary<string, string>()
            {
                {"日本語","単語帳名を入力して下さい。" },
                {"English","Please set name of wordlist." },
                {"繁體中文","請設置詞彙書的名稱。" },
                {"简体中文","请设置词汇书的名称。" },
                {"Deutsch","Bitte geben Sie den Namen des Vokabulars an." },
                {"Français","Veuillez indiquer le nom du vocabulaire." },
                {"한국어","어휘집의 이름을 정하십시오." },
                {"русский","Пожалуйста, укажите название словарного запаса." },
                {"इंडिया",""}
             };
            public static Dictionary<string, string> NoRegisteredword = new Dictionary<string, string>()
            {
                {"日本語","単語が登録されていません。"},
                {"English","Words are not registered."},
                {"繁體中文","單詞未註冊。"},
                {"简体中文","单词未注册。"},
                {"Deutsch","Wörter sind nicht registriert."},
                {"Français","Les mots ne sont pas enregistrés."},
                {"한국어","단어가 등록되지 않았습니다." },
                {"русский","Слова не зарегистрированы." },
                {"इंडिया",""}
            };
            public static Dictionary<string, string> NoMatchedword = new Dictionary<string, string>()
            {
                {"日本語","条件を満たす単語が存在しません。"},
                {"English","Matched words does not exist."},
                {"繁體中文","匹配的單詞不存在。"},
                {"简体中文","匹配的单词不存在。"},
                {"Deutsch","passende Wörter gibt es nicht."},
                {"Français","les mots correspondants n'existent pas."},
                {"한국어","일치하는 단어가 존재하지 않습니다." },
                {"русский","совпавших слов не существует." },
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Delete = new Dictionary<string, string>()
            {
                {"日本語","削除されました。"},
                {"English","Deletion is completed."},
                {"繁體中文","刪除完成。"},
                {"简体中文","删除完成。"},
                {"Deutsch","Die Löschung ist abgeschlossen."},
                {"Français","La suppression est terminée."},
                {"한국어","삭제가 완료되었습니다." },
                {"русский","Удаление завершено." },
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Movevalidate = new Dictionary<string, string>()
            {
                {"日本語","移動先が現在位置と同じです。"},
                {"English","The destination is the same as the current position"},
                {"繁體中文","目的地與當前位置相同"},
                {"简体中文","目的地与当前位置相同"},
                {"Deutsch","Das Ziel entspricht der aktuellen Position"},
                {"Français","La destination est la même que la position actuelle."},
                {"한국어","목적지는 현재 위치와 같습니다." },
                {"русский","Пункт назначения совпадает с текущей позицией." },
                {"इंडिया",""}
            };//2019.03.03
            public static Dictionary<string, string> Move = new Dictionary<string, string>()
            {
                {"日本語","移動されました。"},
                {"English","Move is completed."},
                {"繁體中文","移動完成。"},
                {"简体中文","移动完成。"},
                {"Deutsch","Der Umzug ist abgeschlossen."},
                {"Français","Le déménagement est terminé."},
                {"한국어","이동이 완료되었습니다." },
                {"русский","Перемещение завершено." },
                {"इंडिया",""}
            };
            public static Dictionary<string, string> Wordofmove = new Dictionary<string, string>()
            {
                {"日本語","移動"},
                {"English","Move"},
                {"繁體中文","移動"},
                {"简体中文","移动"},
                {"Deutsch","Bewegung"},
                {"Français","Bouge toi"},
                {"한국어","움직임"},
                {"русский","Переехать"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string> NoTag = new Dictionary<string, string>()
            {
                {"日本語","タグなし"},
                {"English","No tag"},
                {"繁體中文","沒有標籤"},
                {"简体中文","没有标签"},
                {"Deutsch","Keine Markierung"},
                {"Français","Aucun tag"},
                {"한국어","태그 없음"},
                {"русский","Нет тега"},
                {"इंडिया",""}
            };
            public static Dictionary<string, string[]> Add = new Dictionary<string, string[]>()
            {
                { "日本語",new string[]{ "単語帳の追加", "フォルダの追加" } },
                { "English",new string[]{ "Add wordlist", "Add folder" } },
                { "繁體中文",new string[]{ "添加詞彙書", "新增文件夾" } },
                { "简体中文",new string[]{ "添加词汇书", "新增文件夹" } },
                { "Deutsch",new string[]{ "Vokabeln hinzufügen", "Ordner hinzufügen" } },
                { "Français",new string[]{ "Ajouter un livre de vocabulaire", "Ajouter le dossier" } },
                { "한국어",new string[]{ "어휘 책 추가", "폴더 추가" } },
                { "русский",new string[]{ "Добавить словарный запас", "Добавить папку" } },
                { "इंडिया",new string[] { } }
            };
        }
        #endregion
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    [Obsolete]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";

        [Obsolete]
        public override void OnTokenRefresh()
        {
            string refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }
        void SendRegistrationToServer(string token)
        {
            // Add custom implementation, as needed.
        }
    }

    #region Adapter
    /// <summary>
    /// Array adapter start.
    /// </summary>
    public class ArrayAdapter_Start : CustomArrayAdapter
    {
        public List<(string Type, string Name, int Number)> list;
        readonly Start start;
        public ArrayAdapter_Start(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(string Type, string Name, int Number)>)objects;
            start = (Start)context;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            (string Type, string Name, int Number) item2 = new ValueTuple<string, string, int>();
            ListView listView = (ListView)parent;
            if (list != null)
            {
                View view;
                if (convertView != null)
                {
                    view = convertView;
                }
                else
                {
                    view = Inflater.Inflate(layoutid, null);
                    view.SetPaddingRelative(48, 48, 48, 48);
                }
                item2 = list[position];
                TextView text = null;
                ImageView imageView = null;
                ImageButton imageButton = null;
                text = view.FindViewById<TextView>(Resource.Id.tvRow_Explorer);
                imageView = view.FindViewById<ImageView>(Resource.Id.ivRow_Explorer);
                text.Text = item2.Name;
                imageButton = view.FindViewById<ImageButton>(Resource.Id.ibRow_Explorer);
                switch (item2.Type)
                {
                    case "Folder":
                        imageView.SetImageResource(Resource.Drawable.ic_folder2_48pt);
                        imageView.SetColorFilter(CustomActivity.themecolor);
                        imageView.Visibility = ViewStates.Visible;
                        imageButton.Visibility = ViewStates.Invisible;
                        break;
                    case "Wordlist":
                        imageView.SetImageResource(Resource.Drawable.ic_wordlist2_48pt);
                        imageView.SetColorFilter(CustomActivity.themecolor);
                        imageView.Visibility = ViewStates.Visible;
                        imageButton.Visibility = ViewStates.Visible;
                        imageButton.SetTag(Constant.FreeTagKey2, position);
                        if(imageButton.GetTag(Constant.FreeTagKey) == null) 
                        {
                            imageButton.Click += ImageButton_Click;
                            imageButton.SetTag(Constant.FreeTagKey,"Hasevent");
                        }
                        break;
                }
                view.SetBackgroundColor(Constant.SelectColor[Utility.selectpositions[position]]);

                return view;
            }
            return base.GetView(position, convertView, parent);
        }

        void ImageButton_Click(object sender, EventArgs e)
        {
            ImageButton imageButton = sender as ImageButton;
            Utility.cd.Add(Utility.Start_Elements[int.Parse(imageButton.GetTag(Constant.FreeTagKey2).ToString())].Number);
            var xml = XDocument.Load(Utility.WordListPath);
            var elm = Utility.GetXElement(Utility.cd, xml);
            LayoutInflater inflater = LayoutInflater.From(start);
            View view = inflater.Inflate(Resource.Layout.Dialog_SettingWordlist, null);
            EditText edittext = view.FindViewById<EditText>(Resource.Id.etxtInterval_Dialog_SettingWordlist);
            var dlg = new Android.Support.V7.App.AlertDialog.Builder(start);
            Spinner spinnerWord = view.FindViewById<Spinner>(Resource.Id.spChooselanguageWord_Dialog_SettingWordlist);
            Spinner spinnerMeaning = view.FindViewById<Spinner>(Resource.Id.spChooselanguageMeaning_Dialog_SettingWordlist);
            spinnerWord.Adapter = new ArrayAdapter(start, Android.Resource.Layout.SimpleSpinnerItem, Utility.Localedict.Keys.ToList());
            spinnerMeaning.Adapter = new ArrayAdapter(start,Android.Resource.Layout.SimpleSpinnerItem,Utility.Localedict.Keys.ToList());
            spinnerMeaning.SetSelection(Utility.Localedict.Keys.ToList().IndexOf("日本語"));
            edittext.Text = "5";
            if (elm.Attribute("Sleepcount") != null)
            {
                if (int.TryParse(elm.Attribute("Sleepcount").Value,out int result))
                {
                    edittext.Text = result.ToString();
                }
            }
            if (elm.Attribute("VoicelanguageWord") != null)
            {
                if (Utility.Localedict.Keys.ToList().Contains(elm.Attribute("VoicelanguageWord").Value))
                {
                    spinnerWord.SetSelection(Utility.Localedict.Keys.ToList().IndexOf(elm.Attribute("VoicelanguageWord").Value));
                }
            }
            if (elm.Attribute("VoicelanguageMeaning") != null)
            {
                if (Utility.Localedict.Keys.ToList().Contains(elm.Attribute("VoicelanguageMeaning").Value))
                {
                    spinnerMeaning.SetSelection(Utility.Localedict.Keys.ToList().IndexOf(elm.Attribute("VoicelanguageMeaning").Value));
                }
            }
            dlg.SetView(view);
            dlg.SetPositiveButton("OK", (s1, e1) => {
                Utility.cd.Add(Utility.Start_Elements[int.Parse(imageButton.GetTag(Constant.FreeTagKey2).ToString())].Number);
                xml = XDocument.Load(Utility.WordListPath);
                elm = Utility.GetXElement(Utility.cd, xml);
                int Sleepcount = 5;
                if (view.FindViewById<EditText>(Resource.Id.etxtInterval_Dialog_SettingWordlist).Text != string.Empty)
                {
                    int.TryParse(view.FindViewById<EditText>(Resource.Id.etxtInterval_Dialog_SettingWordlist).Text, out Sleepcount);
                }
                string VoicelanguageWord = view.FindViewById<Spinner>(Resource.Id.spChooselanguageWord_Dialog_SettingWordlist).SelectedItem.ToString();
                string VoicelanguageMeaning = view.FindViewById<Spinner>(Resource.Id.spChooselanguageMeaning_Dialog_SettingWordlist).SelectedItem.ToString();
                if(Sleepcount > 60 || Sleepcount < 1) 
                {
                    if (Sleepcount > 60) Sleepcount = 60;
                    if (Sleepcount < 1) Sleepcount = 1;
                }
                if (elm.Attribute("Sleepcount") == null) 
                {
                    elm.Add(new XAttribute("Sleepcount", Sleepcount));
                }
                elm.Attribute("Sleepcount").Value = Sleepcount.ToString();
                if (elm.Attribute("VoicelanguageWord") == null) 
                {
                    elm.Add(new XAttribute("VoicelanguageWord", VoicelanguageWord));
                }
                elm.Attribute("VoicelanguageWord").Value = VoicelanguageWord;
                if (elm.Attribute("VoicelanguageMeaning") == null)
                {
                    elm.Add(new XAttribute("VoicelanguageMeaning", VoicelanguageMeaning));
                }
                elm.Attribute("VoicelanguageMeaning").Value = VoicelanguageMeaning;
                xml.Save(Utility.WordListPath);
                view.FindViewById<EditText>(Resource.Id.etxtInterval_Dialog_SettingWordlist).Text = Sleepcount.ToString();
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
            });
            dlg.SetNegativeButton("CANCEL", (s1, e1) => { });
            dlg.Show();
            Utility.cd.RemoveAt(Utility.cd.Count - 1);
        }
    }
    /// <summary>
    /// Array adapter start move.
    /// </summary>
    //public class ArrayAdapter_Start_Move : CustomArrayAdapter
    //{
    //    List<KeyValuePair<List<int>, XElement>> list;
    //    public ArrayAdapter_Start_Move(Context context, int resource, IList objects) : base(context, resource, objects)
    //    {
    //        list = (List<KeyValuePair<List<int>, XElement>>)objects;
    //    }

    //    public override View GetView(int position, View convertView, ViewGroup parent)
    //    {
    //        KeyValuePair<List<int>, XElement> item3 = new KeyValuePair<List<int>, XElement>();
    //        ListView listView = (ListView)parent;
    //        if (list != null)
    //        {
    //            View view;
    //            if (convertView != null)
    //            {
    //                view = convertView;
    //            }
    //            else
    //            {
    //                view = Inflater.Inflate(layoutid, null);
    //                view.SetPaddingRelative(48, 48, 48, 48);
    //            }
    //            item3 = list[position];
    //            TextView text = null;
    //            TextView HiddenField = null;
    //            text = view.FindViewById<TextView>(Resource.Id.tvRow);
    //            text.Text = XmlConvert.DecodeName(item3.Value.Attribute("Name").Value);
    //            HiddenField = view.FindViewById<TextView>(Resource.Id.tvHiddenField);
    //            if (listView.GetTag(Constant.FreeTagKey) != null && (int)listView.GetTag(Constant.FreeTagKey) == position)
    //            {
    //                view.SetBackgroundColor(Color.Green);
    //            }
    //            else
    //            {
    //                view.SetBackgroundColor(Color.White);
    //            }
    //            return view;
    //        }
    //        return base.GetView(position, convertView, parent);
    //    }
    //}

    /// <summary>
    /// Array adapter start move.
    /// </summary>
    public class ArrayAdapter_Start_Move : RecyclerAdapter
    {
        List<KeyValuePair<List<int>, XElement>> list;
        private List<List<int>> expandposition = new List<List<int>>();
        public List<int> selectedfolderposition = new List<int>();
        public ArrayAdapter_Start_Move(Context context, int resource, IList objects,List<int> selectedposition):base(context,resource,objects)
        {
            list = (List<KeyValuePair<List<int>, XElement>>)objects;
            list = list.OrderBy(p => p.Key,new ListKeyCompare()).ToList();
            ImageClick += ArrayAdapter_Start_Move_ImageClick;
            TextClick += ArrayAdapter_Start_Move_TextClick;
            selectedposition = new List<int>(selectedposition);
            List<int> topposition = new List<int> { 0 };
            expandposition.Add(topposition);
            List<int> temp = new List<int>(selectedposition.SkipLast(1).ToList());
            while (!temp.SequenceEqual(topposition))
            {
                expandposition.Add(temp);
                temp = new List<int>(temp.SkipLast(1).ToList());
            }
        }

        private void ArrayAdapter_Start_Move_ImageClick(object sender, int e)
        {
            ArrayAdapter_Start_Move aasm = sender as ArrayAdapter_Start_Move;
            if (!expandposition.Contains(aasm.list[e].Key, new ListCompare()))
            {
                expandposition.Add(aasm.list[e].Key);
            }
            else
            {
                for(int i = 0; i < expandposition.Count; i++)
                {
                    if(expandposition[i].Count >= aasm.list[e].Key.Count && expandposition[i].Take(aasm.list[e].Key.Count).ToList().SequenceEqual(aasm.list[e].Key))
                    {
                        expandposition.RemoveAt(i);
                        i--;
                    }
                }
            }
            NotifyDataSetChanged();
        }

        private void ArrayAdapter_Start_Move_TextClick(object sender, int e)
        {
            ArrayAdapter_Start_Move aasm = sender as ArrayAdapter_Start_Move;
            Dictionary<bool, List<int>> selectfolderlist = new Dictionary<bool, List<int>>() { { true, new List<int>() }, { false, aasm.list[e].Key } };
            selectedfolderposition = new List<int>(selectfolderlist[selectedfolderposition.SequenceEqual(aasm.list[e].Key)]);
            NotifyDataSetChanged();
        }

        public override void OnBindViewHolder(ViewHolder holder, int position)
        {
            Folderlist vh = holder as Folderlist;
            vh.ItemView.Visibility = ViewStates.Visible;
            if (expandposition.Contains(list[position].Key, new ListCompare()))
            {
                // Load the photo image resource from the photo album:
                vh.ItemView.SetPaddingRelative(48 * (list[position].Key.Count - 2), 0, 48 * (list[position].Key.Count - 2), 0);
                vh.Image.SetImageResource(Resource.Drawable.ic_below_48pt);
            }
            else if(expandposition.Contains(list[position].Key.SkipLast(1).ToList(), new ListCompare()))
            {
                vh.ItemView.SetPaddingRelative(48*(list[position].Key.Count - 2), 0, 48 * (list[position].Key.Count - 2), 0);
                vh.Image.SetImageResource(Resource.Drawable.ic_switchdirectory_48pt);
            }
            else
            {
                vh.ItemView.Visibility = ViewStates.Invisible;
            }
            Dictionary<bool, Color> selectcolorlist = new Dictionary<bool, Color>() { { true, Color.Green }, { false, Color.White } };
            vh.Foldername.SetBackgroundColor(selectcolorlist[selectedfolderposition.SequenceEqual(list[position].Key)]);
            vh.Foldername.Text = XmlConvert.DecodeName(list[position].Value.Attribute("Name").Value);
        }

        private class ListCompare : IEqualityComparer<List<int>>
        {
            bool IEqualityComparer<List<int>>.Equals(List<int> x, List<int> y)
            {
                return x.SequenceEqual(y);
            }

            int IEqualityComparer<List<int>>.GetHashCode(List<int> obj)
            {
                return obj.Aggregate((val,next) => val << 2 ^ next);
            }
        }

        private class ListKeyCompare : IComparer<List<int>>
        {
            public int Compare(List<int> x, List<int> y)
            {
                int length = x.Count > y.Count ? x.Count : y.Count;
                for(int i = 0; i < length; i++)
                {
                    if(i == x.Count)
                    {
                        return -1;
                    }
                    else if(i == y.Count)
                    {
                        return 1;
                    }
                    if(x[i] < y[i])
                    {
                        return -1;
                    }
                    else if(x[i] > y[i])
                    {
                        return 1;
                    }
                }
                return 0;
                //throw new NotImplementedException();
            }
        }

        public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.row_SelectDestination, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            Folderlist vh = new Folderlist(itemView, OnImageClick,OnTextClick);
            return vh;
        }


        public override int ItemCount
        {
            get { return list.Count; }
        }
    }


    public class Folderlist : ViewHolder
    {
        public ImageView Image { get; private set; }
        public TextView Foldername { get; private set; }

        public Folderlist(View itemView, Action<int> imageclicklistener,Action<int> textclicklistener): base(itemView)
        {
            Image = itemView.FindViewById<ImageView>(Resource.Id.iv_row_SelectDestination);
            Foldername = itemView.FindViewById<TextView>(Resource.Id.tvFoldername_row_SelectDestination);
            Image.Click += (sender, e) => imageclicklistener(base.LayoutPosition);
            Foldername.Click += (sender, e) => textclicklistener(LayoutPosition);
        }
    }

    #endregion
}