
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
using static Android.Widget.AdapterView;

namespace WordLearning
{
    [Activity(Label = "Question_bulletin_board")]
    public class Question_bulletin_board : CustomActivity
    {
        private DatabaseReference mDatabase;
        static ListView listView;
        //LinearLayout Layout;
        static List<(string,string)> listboard = new List<(string,string)>();
        static List<string> listtitle = new List<string>();
        int position;
        string selecttitle;
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
            var test = mDatabase.Child(listView.Adapter.GetItem(e.Position).ToString());
            position = e.Position;
            selecttitle = XmlConvert.EncodeName(listView.Adapter.GetItem(e.Position).ToString());
            SupportActionBar.Title = XmlConvert.DecodeName(selecttitle);
            test.AddValueEventListener(new listboardevent(this));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            Setmode(Mode.Board);
        }

        private async void Writeonboard(object sender, DialogClickEventArgs e)
        {
            var test = mDatabase.Child(selecttitle);
            var dlg = sender as Android.Support.V7.App.AlertDialog;
            EditText editText = dlg.FindViewById<EditText>(Constant.FreeDlgId);
            TextInputEditText textInputEditText = dlg.FindViewById<TextInputEditText>(Constant.FreeDlgId2);
            await test.Child(XmlConvert.EncodeName(textInputEditText.Text + "    "+DateTime.Now.ToString())).SetValueAsync(XmlConvert.EncodeName(editText.Text)); //SetValue(XmlConvert.EncodeName(editText.Text));
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
            await test.Child(XmlConvert.EncodeName(textInputEditText.Text)).SetValueAsync(string.Empty); //SetValue(XmlConvert.EncodeName(editText.Text));
            test.AddValueEventListener(new listtitleevent(this));
            //throw new NotImplementedException();
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
                listboard = new List<(string,string)>();
                foreach (DataSnapshot dss in snapshot.Children.ToEnumerable())
                {
                    listboard.Add((XmlConvert.DecodeName(dss.Key),XmlConvert.DecodeName(dss.GetValue(true).ToString())));
                }
                listboard = listboard.OrderByDescending(elm => DateTime.Parse(elm.Item1.Substring(elm.Item1.Length - "XXXX/XX/XX XX:XX:XX".Length))).ToList();
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
                listtitle = new List<string>();
                foreach (DataSnapshot dss in snapshot.Children.ToEnumerable())
                {
                    listtitle.Add(XmlConvert.DecodeName(dss.Key));
                }
                if (question_bulletin_board.mode == Mode.Title)
                {
                    listView.Adapter = new ArrayAdapter(question_bulletin_board, Android.Resource.Layout.SimpleListItem1, listtitle);
                }
            }
        }

    }
    /// <summary>
    /// Array adapter board.
    /// </summary>
    public class ArrayAdapter_Post : CustomArrayAdapter
    {
        List<(string,string)> list;
        Question_bulletin_board question_Bulletin_Board;
        public ArrayAdapter_Post(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(string, string)>)objects;
            question_Bulletin_Board = context as Question_bulletin_board;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!Utility.MultipleActivityFlag) return new View(question_Bulletin_Board);
            (string,string) item = new ValueTuple<string,string>();
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
                item = list[position];
                TextView text = null;
                //TextView HiddenField = null;
                TextView date = null;
                text = view.FindViewById<TextView>(Resource.Id.tvRow_Post);
                date = view.FindViewById<TextView>(Resource.Id.tvDate_row_Post);
                text.Text = item.Item2;
                date.Text = item.Item1;
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }
}