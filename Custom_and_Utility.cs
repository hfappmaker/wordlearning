using Android.App;
using Android.Widget;
using System;
using System.IO;
using System.Collections.Generic; // ← 必須
using System.Linq; // ← こっちは任意。あれば便利機能が使える。
using System.Reflection;
using System.Collections;
using Android.Content;
using static Android.Widget.AdapterView;
using static Android.Widget.RadioGroup;
using static Android.Views.View;
using Android.Support.V7.App;
using Android.Views;
using System.Xml.Linq;
using System.Xml;
using Android.Graphics;
using Android.Content.Res;
using Android.Content.PM;
using Android.Support.V7.Widget;
using static Android.Support.V7.Widget.RecyclerView;
using Java.Lang;
//using Java.Util;

namespace WordLearning
{
    [Activity(Label = "Inherit this activity")]
    public abstract class CustomActivity : AppCompatActivity
    {
        public int? menuResID = null;
        public Android.Support.V7.Widget.Toolbar toolbar = null;
        public int? basetoolbarResID = null;
        public static int[] defaultthemecolor = { 0, 200, 0 };
        public static Color themecolor = Color.Rgb(defaultthemecolor[0], defaultthemecolor[1], defaultthemecolor[2]);
        public static Color themecolor_dark = Color.Rgb((int)(defaultthemecolor[0] * 0.8), (int)(defaultthemecolor[1] * 0.8), (int)(defaultthemecolor[2] * 0.8));
        public IMenu menu;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layoutResID"></param>
        /// <param name="toolbarResID"></param>
        /// <param name="menuResID"></param>
        public void SetContentViewAndToolbar(int layoutResID, int? toolbarResID = null, int? menuResID = null, bool HomeButtonEnabled = true)
        {
            RequestedOrientation = ScreenOrientation.Portrait;
            this.menuResID = menuResID;
            if (File.Exists(Utility.WordListPath))
            {
                switch (layoutResID)
                {
                    case Resource.Layout.Edit_Wordlist:
                    case Resource.Layout.Wordlist_Addword:
                    case Resource.Layout.Wordlist_Editword:
                        Utility.Read_Wordlist();
                        break;
                    case Resource.Layout.Start:
                    case Resource.Layout.Settings:
                        Utility.Get_Wordlists();
                        break;
                }
            }
            SetContentView(layoutResID);
            var xml = XDocument.Load(Utility.WordListPath);
            var Themecolor_xml = xml.Root.Element("Themecolor");
            themecolor = Color.Rgb(int.Parse(Themecolor_xml.Element("Red").Value), int.Parse(Themecolor_xml.Element("Green").Value), int.Parse(Themecolor_xml.Element("Blue").Value));
            themecolor_dark = Color.Rgb((int)(int.Parse(Themecolor_xml.Element("Red").Value) * 0.8), (int)(int.Parse(Themecolor_xml.Element("Green").Value) * 0.8), (int)(int.Parse(Themecolor_xml.Element("Blue").Value) * 0.8));
            Window.SetStatusBarColor(themecolor_dark);
            Window.SetNavigationBarColor(themecolor);
            //Utility.locale = Utility.Localedict[xml.Root.Element("Voicelanguage").Value];
            if (toolbarResID != null)
            {
                toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById((int)toolbarResID);
                toolbar.BackgroundTintList = ColorStateList.ValueOf(themecolor);
                if (toolbar != null)
                {
                    SetSupportActionBar(toolbar);
                    switch (toolbarResID)
                    {
                        case Resource.Id.tbEdit_Wordlist:
                        case Resource.Id.tbLearn_Wordlist:
                            SupportActionBar.Title = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd, xml).Attribute("Name").Value);
                            break;
                        case Resource.Id.tbStart:
                            SupportActionBar.Title = XmlConvert.DecodeName(Utility.GetXElement(Utility.cd,xml).Attribute("Name").Value);
                            break;
                        default:
                            SupportActionBar.Title = Utility.ToolbarTitle[layoutResID];
                            break;
                    }
                    FieldInfo[] fieldInfos = typeof(Resource.Id).GetFields();
                    string str = fieldInfos.First(f1 => f1.GetValue(null).Equals((int)toolbarResID)).Name + "_Init";
                    IEnumerable<FieldInfo> a = fieldInfos.Where(f2 => f2.Name.Equals(str));
                    if (!a.Count().Equals(default(int)))
                    {
                        basetoolbarResID = int.Parse(a.First().GetValue(null).ToString());
                    }
                    SupportActionBar.SetDisplayHomeAsUpEnabled(HomeButtonEnabled);
                    SupportActionBar.SetHomeButtonEnabled(HomeButtonEnabled);
                    if (Utility.cd.Count == 2 && toolbarResID != Resource.Id.tbSettings)
                    {
                        SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                        SupportActionBar.SetHomeButtonEnabled(false);
                    }
                }
            }
        }

        /// <summary>
        /// Set Display from axml file
        /// </summary>
        /// <param name="layoutResID"></param>
        public override void SetContentView(int layoutResID)
        {
            base.SetContentView(layoutResID);
            Button button;
            ListView listView;
            TextView textview;
            RelativeLayout relativeLayout;
            RadioGroup radioGroup;
            EditText editText;
            ImageButton imageButton;
            Type t = typeof(Resource.Id);
            EventHandler eventHandler = null;
            EventHandler<CheckedChangeEventArgs> CheckedChangeeventHandler = null;
            EventHandler<FocusChangeEventArgs> FocusChangeeeventHandler = null;
            EventHandler<TouchEventArgs> TouchEventeeventHandler = null;
            EventHandler<ItemClickEventArgs> ItemClickeventHandler = null;
            EventHandler<ItemLongClickEventArgs> ItemLongClickeventHandler = null;
            foreach (FieldInfo Id in t.GetFields())
            {
                if (Id.Name.Length > 2)
                {
                    if (Id.Name.Substring(0, 3) == "btn")
                    {
                        button = FindViewById<Button>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (button != null)
                        {
                            eventHandler = null;
                            eventHandler = System.Delegate.CreateDelegate(typeof(EventHandler), this, GetType().GetMethod(Id.Name + "_Click")) as EventHandler;
                            if (eventHandler != null)
                            {
                                button.Click += eventHandler;
                            }
                        }
                    }
                    if (Id.Name.Substring(0, 3) == "txt")
                    {
                        textview = FindViewById<TextView>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (textview != null)
                        {
                            switch (Id.Name)
                            {
                                case "txtPrev":
                                case "txtNext":
                                    eventHandler = null;
                                    eventHandler = System.Delegate.CreateDelegate(typeof(EventHandler), this, GetType().GetMethod(Id.Name + "_Click")) as EventHandler;
                                    if (eventHandler != null)
                                    {
                                        textview.Click += eventHandler;
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (Id.Name.Length > 3)
                {
                    if (Id.Name.Substring(0, 4) == "rdog")
                    {
                        radioGroup = FindViewById<RadioGroup>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (radioGroup != null)
                        {
                            CheckedChangeeventHandler = null;
                            CheckedChangeeventHandler = System.Delegate.CreateDelegate(typeof(EventHandler<CheckedChangeEventArgs>), this, GetType().GetMethod(Id.Name + "_CheckedChange")) as EventHandler<CheckedChangeEventArgs>;
                            if (CheckedChangeeventHandler != null) 
                            {
                                radioGroup.CheckedChange += CheckedChangeeventHandler;
                            }
                        }
                    }
                    if (Id.Name.Substring(0, 4) == "etxt")
                    {
                        editText = FindViewById<EditText>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (editText != null)
                        {
                            FocusChangeeeventHandler = null;
                            FocusChangeeeventHandler = System.Delegate.CreateDelegate(typeof(EventHandler<FocusChangeEventArgs>), this, GetType().GetMethod(Id.Name + "_FocusChange")) as EventHandler<FocusChangeEventArgs>;
                            if(FocusChangeeeventHandler != null) 
                            {
                                editText.FocusChange += FocusChangeeeventHandler;
                            }
                        }
                    }
                }
                if (Id.Name.Length > 1)
                {
                    if (Id.Name.Substring(0, 2) == "rl")
                    {
                        relativeLayout = FindViewById<RelativeLayout>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (relativeLayout != null)
                        {
                            TouchEventeeventHandler = null;
                            TouchEventeeventHandler = System.Delegate.CreateDelegate(typeof(EventHandler<TouchEventArgs>), this, GetType().GetMethod(Id.Name + "_Touch")) as EventHandler<TouchEventArgs>;
                            if(TouchEventeeventHandler != null) 
                            {
                                relativeLayout.Touch += TouchEventeeventHandler; 
                            }
                        }
                    }
                    if (Id.Name.Substring(0, 2) == "lv")
                    {
                        listView = FindViewById<ListView>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if (listView != null)
                        {
                            ItemClickeventHandler = null;
                            try
                            {
                                ItemClickeventHandler = System.Delegate.CreateDelegate(typeof(EventHandler<ItemClickEventArgs>), this, GetType().GetMethod(Id.Name + "_ItemClick")) as EventHandler<ItemClickEventArgs>;
                            }
                            catch
                            { }
                            if (ItemClickeventHandler != null) 
                            {
                                listView.ItemClick += ItemClickeventHandler;
                            }
                            ItemLongClickeventHandler = null;
                            try
                            {
                                ItemLongClickeventHandler = System.Delegate.CreateDelegate(typeof(EventHandler<ItemLongClickEventArgs>), this, GetType().GetMethod(Id.Name + "_ItemLongClick")) as EventHandler<ItemLongClickEventArgs>; 
                            }
                            catch
                            { }
                            if (ItemLongClickeventHandler != null)
                            {
                                listView.ItemLongClick += ItemLongClickeventHandler;
                            }
                        }
                    }
                    if(Id.Name.Substring(0, 2) == "ib")
                    {
                        imageButton = FindViewById<ImageButton>(int.Parse(t.GetField(Id.Name).GetValue(null).ToString()));
                        if(imageButton != null)
                        {
                            eventHandler = null;
                            eventHandler = System.Delegate.CreateDelegate(typeof(EventHandler), this, GetType().GetMethod(Id.Name + "_Click")) as EventHandler;
                            if (eventHandler != null)
                            {
                                imageButton.Click += eventHandler;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (menuResID.HasValue)
            {
                MenuInflater.Inflate((int)menuResID, menu);
                switch (menuResID)
                { 
                    case Resource.Menu.menu_Start_Deletemode:
                            menu.GetItem(0).SetVisible(false);
                            break;
                    case Resource.Menu.menu_Question_bulletin_board:
                            break;
                 }
                this.menu = menu;
            }
            return base.OnCreateOptionsMenu(menu);
        }

    }

    public static class Utility
    {
        public static string language = "English";
        public static List<int> cd = new List<int>(){0,0};
        public static List<bool> selectpositions;
        public static List<(string Type,string Name,int Number)> Start_Elements = new List<(string Type, string Name,int Number)>();
        public static List<string> WordLists = new List<string>();
        public static Dictionary<int,(string Wordname,string Wordmeaning,string Tag,string Memo)> WordandMeanings = new Dictionary<int, (string Wordname, string Wordmeaning,string Tag,string Memo)>();
        public static bool MultipleActivityFlag = false;
        public static Java.Util.Locale localeWord = Java.Util.Locale.English;
        public static Java.Util.Locale localeMeaning = Java.Util.Locale.Japan;
        public static int Sleepcount = 5;
        public static Dictionary<int, string> ToolbarTitle = new Dictionary<int, string>()
        {
            {Resource.Layout.Start,Constant.ToolbarTitle_Wordlist[language]},
            {Resource.Layout.Learn_Wordlist,"Learn_Wordlist"},
            {Resource.Layout.Edit_Wordlist,"Edit_Wordlist"},
            {Resource.Layout.Wordlist_Addword,Constant.ToolbarTitle_Registerword[language]},
            {Resource.Layout.Wordlist_Editword,Constant.ToolbarTitle_Editword[language]},
            {Resource.Layout.Settings,Constant.ToolbarTitle_Settings[language]},
            {Resource.Layout.Question_bulletin_board,Constant.ToolbarTitle_Question_bulletin_board[language]}
        };
        public static Dictionary<string, Java.Util.Locale> Localedict = new Dictionary<string, Java.Util.Locale>()
        {
            {"English",Java.Util.Locale.English},
            {"日本語",Java.Util.Locale.Japanese},
            {"简体中文",Java.Util.Locale.SimplifiedChinese},
            {"繁體中文",Java.Util.Locale.TraditionalChinese},
            {"Deutsch",Java.Util.Locale.Germany},
            {"Français",Java.Util.Locale.French},
            {"한국어",Java.Util.Locale.Korean},
            {"русский",new Java.Util.Locale("rus")},
            {"इंडिया",new Java.Util.Locale("ind")},
            {"音声なし",null}
        };
        public static int WordNumber;
        public static string WordListName = string.Empty;
        public static string WordListPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Wordlist.xml";
        public static string Backuppath = Android.OS.Environment.GetExternalStoragePublicDirectory("/WordLearning").AbsolutePath + "/Wordlist.xml";
        /// <summary>
        /// 
        /// </summary>
        public static void Get_Wordlists()
        {
            var xml = XDocument.Load(WordListPath);
            Start_Elements.Clear();
            XElement CurrentElement = GetXElement(cd,xml);
            int i = 0;
            foreach (XElement ChildElement in CurrentElement.Elements())
            {
                Start_Elements.Add((ChildElement.Name.ToString(), XmlConvert.DecodeName(ChildElement.Attribute("Name").Value),i));
                i++;
            }
            Start_Elements = Start_Elements.OrderBy(elm => (elm.Type, elm.Name)).ToList();
        }
        /// <summary>
        /// Read wordlist
        /// </summary>
        public static void Read_Wordlist()
        {
            int key = 0;
            WordandMeanings.Clear();
            var xelm = XDocument.Load(WordListPath);
            var xmlcdNode = GetXElement(cd,xelm)?.Elements();
            foreach (XElement xmlNodecd in xmlcdNode)
            {
                if (xmlNodecd.Name == "Word")
                {
                    WordandMeanings.Add(key, new ValueTuple<string, string, string, string>(XmlConvert.DecodeName(xmlNodecd.Element("Wordname").Value), XmlConvert.DecodeName(xmlNodecd.Element("Wordmeaning").Value), xmlNodecd.Element("Tag").Value, XmlConvert.DecodeName(xmlNodecd.Element("Memo").Value)));
                    key++; 
                }
                else 
                {
                    key++;
                }
            }
            WordandMeanings = WordandMeanings.OrderBy(p => p.Value.Wordname).ToDictionary(p=>p.Key,p=>p.Value);
        }
        /// <summary>
        /// Gets the XE lement.
        /// </summary>
        /// <returns>The XE lement.</returns>
        /// <param name="path">Path.</param>
        public static XElement GetXElement(List<int> path,XDocument xml)
        {
            XElement xElement = xml.Root.Element("Folder");
            List<int> vs = new List<int>(path);
            vs.RemoveRange(0, 2);
            foreach (int dir in vs)
            {
                xElement = xElement.Elements().ElementAt(dir);
            }
            return xElement;
        }

        public static void GetLocaleId()
        {
            string defaultlanguage;
            string country;
            Dictionary<string, string> languagedict = new Dictionary<string, string>()
            {
                { "ja","日本語" },
                { "en","English" },
                { "zh","简体中文" },
                { "de","Deutsch" },
                { "fr","Français" },
                { "ko","한국어" },
                { "ru","русский" },
                { "in","English" }
            };

            // ロケールの取得
            using (Java.Util.Locale locale = Java.Util.Locale.GetDefault(Java.Util.Locale.Category.Display))
            {
                defaultlanguage = locale.Language;
                country = locale.Country;
            }
            if (languagedict.ContainsKey(defaultlanguage))
            {
                language = languagedict[defaultlanguage];
                if(language == "zh")
                {
                    if(country.Equals("TW") || country.Equals("HK"))
                    {
                        language = "繁體中文";
                    }
                }
            }
            else
            {
                language = "English";
            }
            ToolbarTitle = new Dictionary<int, string>()
            {
                {Resource.Layout.Start,Constant.ToolbarTitle_Wordlist[language]},
                {Resource.Layout.Learn_Wordlist,"Learn_Wordlist"},
                {Resource.Layout.Edit_Wordlist,"Edit_Wordlist"},
                {Resource.Layout.Wordlist_Addword,Constant.ToolbarTitle_Registerword[language]},
                {Resource.Layout.Wordlist_Editword,Constant.ToolbarTitle_Editword[language]},
                {Resource.Layout.Settings,Constant.ToolbarTitle_Settings[language]},
                {Resource.Layout.Question_bulletin_board,Constant.ToolbarTitle_Question_bulletin_board[language]}
            };
        }
    }

    public static class Constant
    {
        public const int FreeDlgId = 99999999;
        public const int FreeDlgId2 = 99999998;
        public const int FreeTagKey = 99999999;
        public const int FreeTagKey2 = 99999998;
        public static Dictionary<bool, Color> SelectColor = new Dictionary<bool, Color>() { { true, Color.Argb(127, 255, 87, 34) }, { false, Color.Transparent } };
        public static Dictionary<string, string> ToolbarTitle_Wordlist = new Dictionary<string, string>()
        {
            {"日本語","単語帳"},
            {"English","Word book"},
            {"简体中文","单词书"},
            {"繁體中文","單詞書"},
            {"Deutsch","Wortbuch"},
            {"Français","Livre de mots"},
            {"한국어","단어장"},
            {"русский","Слово книга"},
            {"इंडिया",""}
        };
        public static Dictionary<string, string> ToolbarTitle_Registerword = new Dictionary<string, string>()
        {
            {"日本語","単語の登録"},
            {"English","Register words"},
            {"简体中文","注册单词"},
            {"繁體中文","註冊單詞"},
            {"Deutsch","Wörter registrieren"},
            {"Français","Enregistrer les mots"},
            {"한국어","단어 등록"},
            {"русский","Регистрировать слова"},
            {"इंडिया",""}
        };
        public static Dictionary<string, string> ToolbarTitle_Editword = new Dictionary<string, string>()
        {
            {"日本語","単語の編集"},
            {"English","Edit words"},
            {"简体中文","编辑单词"},
            {"繁體中文","編輯單詞"},
            {"Deutsch","Wörter bearbeiten"},
            {"Français","Modifier les mots"},
            {"한국어","단어 수정"},
            {"русский","Изменить слова"},
            {"इंडिया",""}
        };
        public static Dictionary<string, string> ToolbarTitle_Settings = new Dictionary<string, string>()
        {
            {"日本語","設定"},
            {"English","Settings"},
            {"简体中文","设置"},
            {"繁體中文","設置"},
            {"Deutsch","die Einstellungen"},
            {"Français","Réglages"},
            {"한국어","설정"},
            {"русский","настройки"},
            {"इंडिया",""}
        };
        public static Dictionary<string, string> ToolbarTitle_Question_bulletin_board = new Dictionary<string, string>()
        {
            {"日本語","質問掲示板"},
            {"English","Question_bulletin_board"},
            {"简体中文","Question_bulletin_board"},
            {"繁體中文","Question_bulletin_board"},
            {"Deutsch","Question_bulletin_board"},
            {"Français","Question_bulletin_board"},
            {"한국어","Question_bulletin_board"},
            {"русский","Question_bulletin_board"},
            {"इंडिया","Question_bulletin_board"}
        };
    }

    public abstract class CustomArrayAdapter : ArrayAdapter
    {
        public int layoutid;
        public LayoutInflater Inflater;
        protected CustomArrayAdapter(Context context, int resource,IList objects):base(context,resource,objects)
        {
            layoutid = resource;
            Inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return base.GetView(position,convertView,parent);
        }
    }
    
    public abstract class RecyclerAdapter : RecyclerView.Adapter
    {
        private readonly Context mContext;
        public event EventHandler<int> ImageClick;
        public event EventHandler<int> TextClick;
        public int layoutid;
        //public LayoutInflater Inflater;
        protected RecyclerAdapter(Context context,int resource, IList objects)
        {
            mContext = context;
            layoutid = resource;
            //Inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        protected void OnImageClick(int position)
        {
            ImageClick?.Invoke(this, position);
        }

        protected void OnTextClick(int position)
        {
            TextClick?.Invoke(this, position);
        }

    }

    public static class XExtensions
    {
        /// <summary>
        /// Get the absolute XPath to a given XElement, including the namespace.
        /// (e.g. "/a:people/b:person[6]/c:name[1]/d:last[1]").
        /// </summary>
        public static string GetAbsoluteXPath(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }


            Func<XElement, string> relativeXPath = e =>
            {
                int index = e.IndexPosition();

                var currentNamespace = e.Name.Namespace;

                string name;
                if (string.IsNullOrEmpty(currentNamespace.ToString()))
                {
                    name = e.Name.ToString();
                }
                else
                {
                    name = "*[local-name()='" + e.Name.LocalName + "']";
                    //string namespacePrefix = e.GetPrefixOfNamespace(currentNamespace);
                    //name = namespacePrefix + ":" + e.Name.LocalName;
                }

                // If the element is the root or has no sibling elements, no index is required
                return ((index == -1) || (index == -2)) ? "/" + name : string.Format
                (
                                               "/{0}[{1}]",
                                               name,
                                               index.ToString()
                );
            };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned or -2 if element has no sibling elements.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static int IndexPosition(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.Parent == null)
            {
                // Element is root
                return -1;
            }

            if (element.Parent.Elements(element.Name).Count() == 1)
            {
                // Element has no sibling elements
                return -2;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException
                ("element has been removed from its parent.");
        }
    }

}
