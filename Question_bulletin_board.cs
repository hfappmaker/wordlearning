
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Firebase.Iid;
using Firebase.ML.Vision.Common;
using static Android.Widget.AdapterView;

namespace WordLearning
{
    [Activity(Label = "Question_bulletin_board")]
    public class Question_bulletin_board : CustomActivity
    {
        private DatabaseReference mDatabase;
        static ListView listView;
        //LinearLayout Layout;
        static List<(string,string,DateTime)> listboard = new List<(string,string,DateTime)>();
        static List<(string,DateTime)> listtitle = new List<(string,DateTime)>();
        int position;
        string selecttitle;
        string selecttitledate;
        enum Mode {Title,Board};
        Mode mode;
        string yourname = string.Empty;
        protected override void OnCreate(Bundle savedInstanceState)
        {
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
            SetContentViewAndToolbar(Resource.Layout.Question_bulletin_board, Resource.Id.tbQuestion_bulletin_board,Resource.Menu.menu_Question_bulletin_board,true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            listView = FindViewById<ListView>(Resource.Id.lv_Question_bulletin_board);
            GetDatabaseQuery();
        }

        /// <summary>
        /// When Destroying this activity.
        /// </summary>
        protected override void OnDestroy()
        {
            Utility.MultipleActivityFlag = false;
            base.OnDestroy();
        }

        /// <summary>
        /// データベースのクエリを取得
        /// </summary>
        /// <returns>設定値に応じたデータベースへの参照</returns>
        private void GetDatabaseQuery()
        {
            mDatabase = FirebaseDatabase.GetInstance("https://api-4876079148476953481-415794.firebaseio.com/").Reference;
            mDatabase.AddValueEventListener(new listtitleevent(this));
            Setmode(Mode.Title);
        }

        private void Setmode(Mode mode)
        {
            this.mode = mode;
            InvalidateOptionsMenu();
        }

        public void lv_Question_bulletin_board_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (mode == Mode.Board) return;
            DateTime dateTime;
            
            var title = listView.Adapter.GetItem(e.Position).ToString();
            var titlename = XmlConvert.EncodeName(title.TrimStart('(').TrimEnd(')').Split(',')[0]);
            var titledate = title.TrimStart('(').TrimEnd(')').Split(',')[1].Trim();
            DateTime.TryParse(titledate, null, System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AdjustToUniversal, out dateTime);
            titledate = XmlConvert.EncodeName(dateTime.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-us")));
            var test = mDatabase.Child(titlename).Child(titledate);
            position = e.Position;
            selecttitle = titlename;
            selecttitledate = titledate;
            SupportActionBar.Title = XmlConvert.DecodeName(selecttitle);
            test.AddValueEventListener(new listboardevent(this));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            Setmode(Mode.Board);
        }

        private async void Writeonboard(object sender, DialogClickEventArgs e)
        {
            var test = mDatabase.Child(selecttitle).Child(selecttitledate);
            var dlg = sender as Android.Support.V7.App.AlertDialog;
            EditText editText = dlg.FindViewById<EditText>(Constant.FreeDlgId);
            TextInputEditText textInputEditText = dlg.FindViewById<TextInputEditText>(Constant.FreeDlgId2);
            if (string.IsNullOrEmpty(editText.Text)) return;
            if (string.IsNullOrEmpty(textInputEditText.Text)) textInputEditText.Text = "Guest";
            string nowtime = DateTime.UtcNow.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-us"));
            await test.Child(XmlConvert.EncodeName(nowtime)).SetValueAsync(string.Empty); //SetValue(XmlConvert.EncodeName(editText.Text));
            await test.Child(XmlConvert.EncodeName(nowtime)).Child(XmlConvert.EncodeName(textInputEditText.Text)).SetValueAsync(XmlConvert.EncodeName(editText.Text));
            test.AddValueEventListener(new listboardevent(this));
            yourname = textInputEditText.Text;
        }

        public void lv_Question_bulletin_board_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {

        }

        public override bool OnOptionsItemSelected(IMenuItem item) 
        {
            var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (mode == Mode.Board)
                    {
                        Setmode(Mode.Title);
                        SetContentViewAndToolbar(Resource.Layout.Question_bulletin_board, Resource.Id.tbQuestion_bulletin_board, Resource.Menu.menu_Question_bulletin_board, true);
                        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                        SupportActionBar.SetHomeButtonEnabled(true);
                        SupportActionBar.Title = Resources.GetString(Resource.String.Question_bulletin_board);
                        listView = FindViewById<ListView>(Resource.Id.lv_Question_bulletin_board);
                        GetDatabaseQuery();
                    }
                    else 
                    {
                        Finish();
                    }
                    break;
                case Resource.Id.action_add_Question_bulletin_board:
                    TextInputLayout tlititle = new TextInputLayout(this)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };
                    TextInputEditText textInputEditTexttitle = new TextInputEditText(this)
                    {
                        Hint = "Title"
                    };
                    textInputEditTexttitle.SetMaxHeight(140);
                    textInputEditTexttitle.Id = Constant.FreeDlgId;
                    tlititle.AddView(textInputEditTexttitle);
                    dlg.SetTitle("Create title");
                    dlg.SetPositiveButton("OK", Createtitle);
                    dlg.SetNegativeButton("CANCEL", (s2, e2) => { });
                    dlg.SetView(tlititle);
                    dlg.Show();
                    break;
                case Resource.Id.action_write_Question_bulletin_board:
                    LinearLayout linearLayout = new LinearLayout(this)
                    {
                        Orientation = Orientation.Vertical,
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };
                    EditText editText = new EditText(this)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };
                    TextInputLayout tli = new TextInputLayout(this)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };
                    TextInputEditText textInputEditText = new TextInputEditText(this)
                    {
                        Hint = "Name"
                    };
                    textInputEditText.SetMaxHeight(140);
                    textInputEditText.Text = yourname;
                    textInputEditText.Id = Constant.FreeDlgId2;
                    tli.AddView(textInputEditText);
                    editText.SetMaxHeight(280);
                    editText.Id = Constant.FreeDlgId;
                    linearLayout.AddView(tli);
                    linearLayout.AddView(editText);
                    dlg.SetTitle("Write");
                    dlg.SetPositiveButton("OK", Writeonboard);
                    dlg.SetNegativeButton("CANCEL", (s2, e2) => { });
                    dlg.SetView(linearLayout);
                    dlg.Show();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (menuResID.HasValue)
            {
                MenuInflater.Inflate((int)menuResID, menu);
                this.menu = menu;
                switch (mode) 
                { 
                    case Mode.Title:
                        menu.GetItem(0).SetVisible(true);
                        menu.GetItem(1).SetVisible(false);
                        break;
                    case Mode.Board:
                        menu.GetItem(0).SetVisible(false);
                        menu.GetItem(1).SetVisible(true);
                        break; 
                }
            }
            return true;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (mode == Mode.Board)
            {
                Setmode(Mode.Title);
                SetContentViewAndToolbar(Resource.Layout.Question_bulletin_board, Resource.Id.tbQuestion_bulletin_board, Resource.Menu.menu_Question_bulletin_board, true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.Title = Resources.GetString(Resource.String.Question_bulletin_board);
                listView = FindViewById<ListView>(Resource.Id.lv_Question_bulletin_board);
                GetDatabaseQuery();
                return false;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private async void Createtitle(object sender, DialogClickEventArgs e)
        {
            var test = mDatabase;
            var dlg = sender as Android.Support.V7.App.AlertDialog;
            TextInputEditText textInputEditText = dlg.FindViewById<TextInputEditText>(Constant.FreeDlgId);
            if (string.IsNullOrEmpty(textInputEditText.Text)) return;
            //FirebaseVisionImage
            if (listtitle.Select(p => p.Item1).Contains(textInputEditText.Text))
            {
                var dlg2 = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlg2.SetMessage(Message.AnotherTitle[Utility.language]);
                dlg2.SetPositiveButton("OK",(s1,e1) => { dlg.Show(); });
                dlg2.Show();
                return;
            }
            await test.Child(XmlConvert.EncodeName(textInputEditText.Text)).SetValueAsync(string.Empty);
            string datenow = DateTime.UtcNow.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-us"));
            //var pp = FirebaseInstanceId.Instance.Id;
            await test.Child(XmlConvert.EncodeName(textInputEditText.Text)).Child(XmlConvert.EncodeName(datenow)).SetValueAsync(string.Empty);
            test.AddValueEventListener(new listtitleevent(this));
        }

        public class tt : Java.Lang.Object, IChildEventListener
        {
            public void OnCancelled(DatabaseError error)
            {
                // throw new NotImplementedException();
            }

            public void OnChildAdded(DataSnapshot snapshot, string previousChildName)
            {
                //throw new NotImplementedException();
            }

            public void OnChildChanged(DataSnapshot snapshot, string previousChildName)
            {
                //throw new NotImplementedException();
            }

            public void OnChildMoved(DataSnapshot snapshot, string previousChildName)
            {
                //throw new NotImplementedException();
            }

            public void OnChildRemoved(DataSnapshot snapshot)
            {
                //throw new NotImplementedException();
            }
        }

        public class listboardevent : Java.Lang.Object, IValueEventListener
        {
            private Question_bulletin_board question_bulletin_board;

            public listboardevent(Question_bulletin_board question_bulletin_board)
            {
                this.question_bulletin_board = question_bulletin_board;
            }

            public void OnCancelled(DatabaseError error)
            {
                //throw new NotImplementedException();
            }

            public void OnDataChange(DataSnapshot snapshot)
            {
                listboard = new List<(string,string,DateTime)>();
                foreach (DataSnapshot dss in snapshot.Children.ToEnumerable())
                {
                    DateTime datetime;
                    var nameandsentence = dss.GetValue(true) as JavaDictionary;
                    if (nameandsentence == null) continue;
                    var name = nameandsentence.Keys as JavaSet;
                    var sentence = nameandsentence.Values as JavaCollection;
                    var namestr = name.OfType<string>().First();
                    var sentencestr = sentence.GetEnumerator();
                    sentencestr.MoveNext();

                    DateTime.TryParse(XmlConvert.DecodeName(dss.Key), null, System.Globalization.DateTimeStyles.AssumeUniversal, out datetime);
                    listboard.Add((XmlConvert.DecodeName(sentencestr.Current.ToString()), XmlConvert.DecodeName(namestr), datetime));
                }
                listboard = listboard.OrderByDescending(elm => elm.Item3).ToList();
                if (question_bulletin_board.mode == Mode.Board)
                {
                    listView.Adapter = new ArrayAdapter_Post(question_bulletin_board, Resource.Layout.row_Post, listboard);
                }
            }
        }

        public class listtitleevent : Java.Lang.Object, IValueEventListener
        {
            private Question_bulletin_board question_bulletin_board;

            public listtitleevent(Question_bulletin_board question_bulletin_board)
            {
                this.question_bulletin_board = question_bulletin_board;
            }

            public void OnCancelled(DatabaseError error)
            {
                //throw new NotImplementedException();
            }

            public void OnDataChange(DataSnapshot snapshot)
            {
                listtitle = new List<(string,DateTime)>();
                foreach (DataSnapshot dss in snapshot.Children.ToEnumerable())
                { 
                    DateTime datetime;
                    var date = dss.GetValue(true) as JavaDictionary;
                    var date2 = dss.GetValue(true) as Java.Lang.String;
                    if (date == null && date2 != null)
                    {
                        DateTime.TryParse(XmlConvert.DecodeName(date2.ToString()),null,System.Globalization.DateTimeStyles.AssumeUniversal, out datetime);
                        listtitle.Add((XmlConvert.DecodeName(dss.Key), datetime));
                    }
                    else if (date != null)
                    {
                        var teststr = date.Keys as JavaSet;
                        var datestr = teststr.OfType<string>().First();
                        DateTime.TryParse(XmlConvert.DecodeName(datestr), null, System.Globalization.DateTimeStyles.AssumeUniversal, out datetime);
                        listtitle.Add((XmlConvert.DecodeName(dss.Key), datetime));
                    }
                    
                }
                listtitle = listtitle.OrderByDescending(elm => elm.Item2).ToList();
                if (question_bulletin_board.mode == Mode.Title)
                {
                    listView.Adapter = new ArrayAdapter_Title(question_bulletin_board, Resource.Layout.row_Post, listtitle);
                }
            }
        }

    }
    /// <summary>
    /// Array adapter board.
    /// </summary>
    public class ArrayAdapter_Title : CustomArrayAdapter
    {
        List<(string,DateTime)> list;
        Question_bulletin_board question_Bulletin_Board;
        public ArrayAdapter_Title(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(string, DateTime)>)objects;
            question_Bulletin_Board = context as Question_bulletin_board;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!Utility.MultipleActivityFlag) return new View(question_Bulletin_Board);
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
                (string, DateTime) item = list[position];
                TextView text = view.FindViewById<TextView>(Resource.Id.tvRow_Post);
                TextView date = view.FindViewById<TextView>(Resource.Id.tvDate_row_Post);
                text.Text = item.Item1;
                date.Text = item.Item2.ToLocalTime().ToString();
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }

    /// <summary>
    /// Array adapter board.
    /// </summary>
    public class ArrayAdapter_Post : CustomArrayAdapter
    {
        List<(string, string,DateTime)> list;
        Question_bulletin_board question_Bulletin_Board;
        public ArrayAdapter_Post(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(string, string,DateTime)>)objects;
            question_Bulletin_Board = context as Question_bulletin_board;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!Utility.MultipleActivityFlag) return new View(question_Bulletin_Board);
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
                (string, string,DateTime) item = list[position];
                TextView text = view.FindViewById<TextView>(Resource.Id.tvRow_Post);
                //TextView HiddenField = null;
                TextView date = view.FindViewById<TextView>(Resource.Id.tvDate_row_Post);
                text.Text = item.Item1;
                date.Text = item.Item2 + "   " + item.Item3.ToLocalTime().ToString();
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }

    public static class Message
    {
        public static Dictionary<string, string> AnotherTitle = new Dictionary<string, string>()
            {
                {"日本語","タイトルが重複しています。他のタイトル名に変更して下さい。"},
                {"English","Title is duplicated.Please set another title."},
                {"繁體中文","標題是重複的。請設置另一個標題。"},
                {"简体中文","标题重复。请设置另一个标题。"},
                {"Deutsch","Titel ist doppelt vorhanden. Bitte setzen Sie einen anderen Titel."},
                {"Français","Le titre est dupliqué. Veuillez définir un autre titre."},
                {"한국어","제목이 중복되었습니다. 다른 제목을 설정하십시오."},
                {"русский","Название дублируется.Пожалуйста, установите другое название."},
                {"इंडिया","शीर्षक डुप्लिकेट है। कृपया एक और शीर्षक सेट करें।"}
            };
    }
}