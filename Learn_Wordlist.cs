using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Linq; // ← こっちは任意。あれば便利機能が使える。
using static Android.Widget.RadioGroup;
using static Android.Views.View;
using Android.Views;
using static Android.Views.GestureDetector;
using Android.Runtime;
using System.Xml.Linq;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Xml;
using Android.Content;
using Android.Speech.Tts;
using Java.Util;
using Android.Views.InputMethods;
using System.Threading.Tasks;
using static System.Threading.Thread;
using System.Threading;

namespace WordLearning
{
    [Activity(Label = "Learn_Wordlist")]
    public class Learn_Wordlist : CustomActivity, TextToSpeech.IOnInitListener
    {
        private GestureDetector mGestureDetector;
        private GestureListener mOnGestureListener;
        TextToSpeech ttsword,ttsmeaning;
        TextView txtWord;
        TextView txtMeaning;
        TextView txtPageNo;
        GridLayout gridLayout;
        private bool IsAutomode = false;
        delegate void Click(object sender,EventArgs e);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Handler handler = new Handler();
        /// <summary>
        /// Ons the create.
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //If There isn't Start activity although EditWordlist activity exists. Finish this activity and start Start activity.
            if (!Utility.MultipleActivityFlag || Utility.cd.SequenceEqual(new List<int>() { 0, 0 }) || !Utility.cd.Any())
            {
                Finish();
                Utility.cd = new List<int>() { 0, 0 };
                Intent intent = new Intent(this, typeof(Start));
                StartActivity(intent);
                return;
            }
            SetContentViewAndToolbar(Resource.Layout.Learn_Wordlist, Resource.Id.tbLearn_Wordlist,Resource.Menu.menu_Learn_Wordlist);
            GetWordandMeanings();
            mOnGestureListener = new GestureListener(this);
            mGestureDetector = new GestureDetector(this, mOnGestureListener);
            txtWord = FindViewById<TextView>(Resource.Id.txtWord);
            txtMeaning = FindViewById<TextView>(Resource.Id.txtMeaning);
            txtWord.Text = Utility.WordandMeanings.First().Value.Wordname;
            txtMeaning.Text = Utility.WordandMeanings.First().Value.Wordmeaning;
            txtPageNo = FindViewById<TextView>(Resource.Id.txtPageNo);
            txtPageNo.Text = "1/" + Utility.WordandMeanings.Count;
            gridLayout = FindViewById<GridLayout>(Resource.Id.gl_Taglist_Learn_Wordlist);
            SetGridLayout(Utility.WordandMeanings.First().Value.Tag);
            ttsword = new TextToSpeech(this, this);
            ttsmeaning = new TextToSpeech(this, this);
            ImageButton voicebtn_word = FindViewById<ImageButton>(Resource.Id.ibVoice_Word);
            ImageButton voicebtn_meaning = FindViewById<ImageButton>(Resource.Id.ibVoice_Meaning);
            ImageButton memobtn = FindViewById<ImageButton>(Resource.Id.ibMemo);
            voicebtn_word.SetColorFilter(themecolor, PorterDuff.Mode.SrcIn);
            voicebtn_meaning.SetColorFilter(themecolor,PorterDuff.Mode.SrcIn);
            memobtn.SetColorFilter(themecolor, PorterDuff.Mode.SrcIn);
            Utility.WordNumber = 0;
            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyWord));
        }
        /// <summary>
        /// Gets the wordand meanings.
        /// </summary>
        private void GetWordandMeanings()
        {
            switch(Intent.GetIntExtra("RadioButton", Resource.Id.rdoAscendant))
            {
                case Resource.Id.rdoAscendant:
                    Utility.WordandMeanings = Utility.WordandMeanings.OrderBy(p => p.Value.Wordname).ToDictionary(p => p.Key,p=>p.Value);
                    break;
                case Resource.Id.rdoDescendant:
                    Utility.WordandMeanings = Utility.WordandMeanings.OrderByDescending(p => p.Value.Wordname).ToDictionary(p => p.Key, p => p.Value);
                    break;
                case Resource.Id.rdoRandomize:
                    Utility.WordandMeanings = Utility.WordandMeanings.OrderBy(p => Guid.NewGuid()).ToDictionary(p => p.Key, p => p.Value);
                    break;
            }
        }

        /// <summary>
        /// Sets the grid layout.
        /// </summary>
        private void SetGridLayout(string Tag)
        {
            if (gridLayout.ChildCount != 0)
            {
                gridLayout.RemoveViews(0, gridLayout.ChildCount);
            }
            ImageView imageView;
            var xelm = XDocument.Load(Utility.WordListPath);
            List<XElement> Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
            List<(int, int[], string,string, bool)> wkTags = new List<(int, int[], string,string, bool)>();
            bool[] tagselect = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                tagselect[i] = Convert.ToBoolean(int.Parse(Tag.Substring(i, 1)));
            }
            for (int i = 0; i < Tags_xml.Count; i++)
            {
                if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                {
                    wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value, tagselect[i]));
                }
            }
            List<ImageView> listimageview = new List<ImageView>();
            int j = 0;
            for (int i = 0; i < wkTags.Count; i++)
            {
                if (wkTags[i].Item5)
                {
                    imageView = new ImageView(this);
                    switch(wkTags[i].Item4)
                    {
                        case "Square":
                            var drawable1 = GetDrawable(Resource.Drawable.square);
                            drawable1.Mutate();
                            drawable1.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                            imageView.SetImageDrawable(drawable1);
                            break;
                        case "Star":
                            var drawable2 = GetDrawable(Resource.Drawable.star);
                            drawable2.Mutate();
                            drawable2.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                            imageView.SetImageDrawable(drawable2);
                            break;
                        case "Heart":
                            var drawable3 = GetDrawable(Resource.Drawable.heart);
                            drawable3.Mutate();
                            drawable3.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                            imageView.SetImageDrawable(drawable3);
                            break;
                    }
                    var param = new GridLayout.LayoutParams
                    {
                        RowSpec = GridLayout.InvokeSpec(j),
                        ColumnSpec=GridLayout.InvokeSpec(0)
                    };
                    j++;
                    imageView.LayoutParameters = param;
                    gridLayout.AddView(imageView);
                }
            }
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

            }
            return base.OnKeyDown(keyCode, e);
        }
        /// <summary>
        /// When radiobox's check is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rdogSelectVisible_CheckedChange(object sender, CheckedChangeEventArgs e)
        {
            txtWord.Visibility = ViewStates.Visible;
            txtMeaning.Visibility = ViewStates.Visible;
            switch (e.CheckedId)
            {
                case Resource.Id.rdoOnlyWord:
                    txtMeaning.Visibility = ViewStates.Invisible;
                    break;
                case Resource.Id.rdoOnlyMeaning:
                    txtWord.Visibility = ViewStates.Invisible;
                    break;
            }
        }
        /// <summary>
        /// Click main display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void llMain_Learn_Wordlist_Touch(object sender, TouchEventArgs e)
        {

        }
        /// <summary>
        /// This event don't arise.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void llMoveWord_Learn_Wordlist_Touch(object sender, TouchEventArgs e)
        {

        }
        /// <summary>
        /// Click txtPrev
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void txtPrev_Click(object sender, EventArgs e)
        {
            RadioGroup rdog = FindViewById<RadioGroup>(Resource.Id.rdogSelectVisible);
            Utility.WordNumber--;
            if(Utility.WordNumber < 0) 
            {
                Utility.WordNumber = Utility.WordandMeanings.Count - 1;
             }
            txtWord.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordname;
            txtMeaning.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordmeaning;
            SetGridLayout(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag);
            rdog.Check(Resource.Id.rdoOnlyWord);
            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyWord));
            txtPageNo.Text = Utility.WordNumber + 1 + "/" + Utility.WordandMeanings.Count;
        }
        /// <summary>
        /// Click txtNext
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void txtNext_Click(object sender, EventArgs e)
        {
            RadioGroup rdog = FindViewById<RadioGroup>(Resource.Id.rdogSelectVisible);
            Utility.WordNumber++;
            if(Utility.WordNumber == Utility.WordandMeanings.Count) 
            {
                Utility.WordNumber = 0;
             }
            txtWord.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordname;
            txtMeaning.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordmeaning;
            SetGridLayout(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag);
            if (IsAutomode)
            {
                rdog.Check(Resource.Id.rdoBoth);
                rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoBoth));
            }
            else
            {
                rdog.Check(Resource.Id.rdoOnlyWord);
                rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyWord));
            }
            txtPageNo.Text = Utility.WordNumber + 1 + "/" + Utility.WordandMeanings.Count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if(e.Action == MotionEventActions.Down)
            {

            }
            else if(e.Action == MotionEventActions.Move)
            {

            }
            bool result = mGestureDetector.OnTouchEvent(e);
            if (e.Action == MotionEventActions.Up)
            {
                RadioGroup rdog = FindViewById<RadioGroup>(Resource.Id.rdogSelectVisible);
                if (!rdog.Selected && e.EventTime - e.DownTime > 50 && !result)
                {
                    switch (rdog.CheckedRadioButtonId)
                    {
                        case Resource.Id.rdoBoth:
                            rdog.Check(Resource.Id.rdoOnlyWord);
                            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyWord));
                            break;
                        case Resource.Id.rdoOnlyWord:
                            rdog.Check(Resource.Id.rdoOnlyMeaning);
                            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyMeaning));
                            break;
                        case Resource.Id.rdoOnlyMeaning:
                            rdog.Check(Resource.Id.rdoBoth);
                            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoBoth));
                            break;
                    }
                }
            }
            return result;          
        }
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
                    Finish();
                    break;
                case Resource.Id.action_edit_Learn_Wordlist:
                    if (IsAutomode) break;
                    var xelm = XDocument.Load(Utility.WordListPath);
                    List<(int, int[], string,string, bool)>  wkTags = new List<(int, int[], string,string, bool)>();
                    List<XElement> Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
                    var dlgEditTagofWord = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlgEditTagofWord.SetTitle(Message.Edittag[Utility.language]);
                    bool[] tagselect = new bool[5];
                    for (int i = 0; i < 5; i++)
                    {
                        tagselect[i] = Convert.ToBoolean(int.Parse(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag.Substring(i, 1)));
                    }
                    for (int i = 0; i < Tags_xml.Count; i++)
                    {
                        if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                        {
                            wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value, tagselect[i]));
                        }
                    }
                    ListView SelectedTag = new ListView(this)
                    {
                        Adapter = new ArrayAdapter_SelectedTag(this, Resource.Layout.row_SelectedTag, wkTags),
                        Id = Constant.FreeDlgId,
                        ChoiceMode = ChoiceMode.Multiple
                    };
                    int k = 0;
                    foreach ((int, int[], string,string, bool) wkTag in wkTags)
                    {
                        SelectedTag.SetTag(0x7000000 + k, wkTag.Item1);
                        k++;
                    }
                    SelectedTag.SetTag(Constant.FreeTagKey, Utility.WordandMeanings.ElementAt(Utility.WordNumber).Key);
                    var dlgEditTagofWordInstance = dlgEditTagofWord.Create();
                    dlgEditTagofWordInstance.SetView(SelectedTag);
                    dlgEditTagofWordInstance.SetButton((int)DialogButtonType.Negative, "OK", SetTagofWord);
                    dlgEditTagofWordInstance.SetButton((int)DialogButtonType.Positive, "CANCEL", (s3, e3) => { return; });
                    dlgEditTagofWordInstance.Show();
                    break;
                case Resource.Id.action_jump_Learn_Wordlist:
                    if (IsAutomode) break;
                    var dlgJump = new Android.Support.V7.App.AlertDialog.Builder(this);
                    var ed = new EditText(this) { Id = Constant.FreeDlgId };
                    ed.InputType = Android.Text.InputTypes.ClassNumber;
                    ed.FocusChange += (s1, e1) => {
                        if (e1.HasFocus)
                        {
                            Window.SetSoftInputMode(SoftInput.AdjustNothing);
                        }
                    };
                    dlgJump.SetMessage(Message.SpecifyDestination[Utility.language]);
                    dlgJump.SetView(ed);
                    dlgJump.SetPositiveButton("OK", JumpPage);
                    dlgJump.SetNegativeButton("CANCEL", (s1, e1) => { });
                    dlgJump.Show();
                    ed.RequestFocus();
                    break;
                case Resource.Id.action_auto_Learn_Wordlist:
                    var dlgAuto = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlgAuto.SetMessage(Message.AutoPlayConfirm[Utility.language]);
                    dlgAuto.SetPositiveButton("OK", Autoplaystart);
                    dlgAuto.SetNegativeButton("CANCEL", (s1, e1) => { });
                    dlgAuto.Show();
                    break;
                case Resource.Id.action_pause_Learn_Wordlist:
                    IsAutomode = false;
                    InvalidateOptionsMenu();
                    tokenSource.Cancel();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            base.OnPrepareOptionsMenu(menu);
            var auto = menu.FindItem(Resource.Id.action_auto_Learn_Wordlist);
            var pause = menu.FindItem(Resource.Id.action_pause_Learn_Wordlist);
            auto.SetVisible(!IsAutomode);
            pause.SetVisible(IsAutomode);
            return true;
        }
        private void Autoplaystart(object sender, DialogClickEventArgs e)
        {
            var xml = XDocument.Load(Utility.WordListPath);
            IsAutomode = true;
            tokenSource = new CancellationTokenSource();
            InvalidateOptionsMenu();
            Task.Run(() => 
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    handler.Post(() => 
                    {
                        txtNext_Click(new object(), new EventArgs());
                        if (Utility.localeWord != null)
                        {
                            ibVoice_Word_Click(new object(), new EventArgs());
                        }
                    });
                    if (Utility.localeWord != null)
                    {
                        Sleep(Utility.Sleepcount * 1000);
                    }
                    handler.Post(() => 
                    {
                        if (Utility.localeMeaning != null)
                        {
                            ibVoice_Meaning_Click(new object(), new EventArgs());
                        }
                    });
                    if (Utility.localeMeaning != null)
                    {
                        Sleep(Utility.Sleepcount * 1000);
                    }
                    if(Utility.localeWord == null && Utility.localeMeaning == null) 
                    {
                        Sleep(Utility.Sleepcount * 1000);
                    }
                }
            }
            , tokenSource.Token);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Jumps the page.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void JumpPage(object sender, DialogClickEventArgs e)
        {
            var dlgJump = (Android.Support.V7.App.AlertDialog)sender;
            if(!int.TryParse(dlgJump.FindViewById<EditText>(Constant.FreeDlgId).Text,out int PageNo) || PageNo < 1 || PageNo > Utility.WordandMeanings.Count)
            {
                var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlg.SetMessage(Message.WarningPageSelect[Utility.language]);
                dlg.SetPositiveButton("OK", (s1, e1) => { dlgJump.Show(); });
                dlg.Show();
                return;
            }
            Utility.WordNumber = PageNo - 1;
            RadioGroup rdog = FindViewById<RadioGroup>(Resource.Id.rdogSelectVisible);
            txtWord.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordname;
            txtMeaning.Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordmeaning;
            SetGridLayout(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag);
            rdog.Check(Resource.Id.rdoOnlyWord);
            rdogSelectVisible_CheckedChange(new object(), new CheckedChangeEventArgs(Resource.Id.rdoOnlyWord));
            FindViewById<TextView>(Resource.Id.txtPrev).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.txtNext).Visibility = ViewStates.Visible;
            if (Utility.WordNumber == 0)
            {
                FindViewById<TextView>(Resource.Id.txtPrev).Visibility = ViewStates.Invisible;
            }
            if (Utility.WordNumber == Utility.WordandMeanings.Count - 1)
            {
                FindViewById<TextView>(Resource.Id.txtNext).Visibility = ViewStates.Invisible;
            }
            txtPageNo.Text = Utility.WordNumber + 1 + "/" + Utility.WordandMeanings.Count;
        }

        /// <summary>
        /// Sets the tagof word.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void SetTagofWord(object sender, DialogClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog dlgEditTagofWord = (Android.Support.V7.App.AlertDialog)sender;
            ListView lv = dlgEditTagofWord.FindViewById<ListView>(Constant.FreeDlgId);
            List<(int, int[], string,string, bool)> tagselectlist = ((ArrayAdapter_SelectedTag)lv.Adapter).list;
            var xml = XDocument.Load(Utility.WordListPath);
            Utility.cd.Add((int)lv.GetTag(Constant.FreeTagKey));
            XElement Word_xml = Utility.GetXElement(Utility.cd, xml);
            for (int i = 0; i < lv.Count; i++)
            {
                Word_xml.Element("Tag").Value = Word_xml.Element("Tag").Value.Substring(0, (int)lv.GetTag(i + 0x7000000)) + Convert.ToInt32(tagselectlist[i].Item5).ToString() + Word_xml.Element("Tag").Value.Substring((int)lv.GetTag(i + 0x7000000) + 1);
            }
            xml.Save(Utility.WordListPath);
            Utility.WordandMeanings[Utility.WordandMeanings.ElementAt(Utility.WordNumber).Key] = new ValueTuple<string, string, string,string>
            (Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordname,
             Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordmeaning,
             Word_xml.Element("Tag").Value,
             Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Memo);
            Utility.cd.RemoveAt(Utility.cd.Count - 1);
            SetGridLayout(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag);
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            ttsword.SetLanguage(Utility.localeWord);
            ttsmeaning.SetLanguage(Utility.localeMeaning);
        }

        public void ibVoice_Word_Click(object sender,EventArgs e)
        {
            ttsword.Speak(txtWord.Text, QueueMode.Flush,null,"Word");
        }

        public void ibVoice_Meaning_Click(object sender, EventArgs e)
        {
            ttsmeaning.Speak(txtMeaning.Text, QueueMode.Flush, null, "Meaning");
        }

        public void ibMemo_Click(object sender, EventArgs e)
        {
            if (IsAutomode) return;
            var dlgMemo = new Android.Support.V7.App.AlertDialog.Builder(this) 
            {

            };
            EditText textView = new EditText(this)
            {
                Text = Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Memo,
                Id = Constant.FreeDlgId
            };
            textView.SetMaxHeight(120 * 4);　//2019.03.24 add
            textView.Focusable = false;
            textView.LongClickable = false;
            dlgMemo.SetTitle(Message.Memo[Utility.language]);
            dlgMemo.SetPositiveButton("OK", (s1, e1) =>
            {
                var xml = XDocument.Load(Utility.WordListPath);
                Utility.cd.Add(Utility.WordandMeanings.ElementAt(Utility.WordNumber).Key);
                var elm = Utility.GetXElement(Utility.cd, xml);
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                elm.Element("Memo").Value = XmlConvert.EncodeLocalName(textView.Text);
                xml.Save(Utility.WordListPath);
                Utility.WordandMeanings[Utility.WordandMeanings.ElementAt(Utility.WordNumber).Key] = new ValueTuple<string, string, string, string>
                (Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordname,
                 Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Wordmeaning,
                 Utility.WordandMeanings.ElementAt(Utility.WordNumber).Value.Tag,
                 textView.Text
                );
            });
            dlgMemo.SetNegativeButton("CANCEL", (s1, e1) => { });
            dlgMemo.SetNeutralButton("EDIT", (s1,e1) => { });
            dlgMemo.SetView(textView);
            var dlgMemoins = dlgMemo.Create();
            dlgMemoins.Show();
            var neutralbtn = dlgMemoins.GetButton((int)DialogButtonType.Neutral);
            neutralbtn.Click += (s1, e1) =>
            {
                textView.Focusable = true;
                textView.FocusableInTouchMode = true;
                textView.LongClickable = true;
                textView.RequestFocus();
                var imm = GetSystemService(InputMethodService) as InputMethodManager;
                imm.ShowSoftInput(textView, ShowFlags.Implicit);
            };
        }

        protected override void OnPause()
        {
            //IsAutomode = false; 2019.03.24 comment out
            //InvalidateOptionsMenu(); 2019.03.24 comment out
            //tokenSource.Cancel(); 2019.03.24 comment out
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            ttsword.Shutdown();
            ttsmeaning.Shutdown();
            //2019.03.24 add start
            IsAutomode = false;
            InvalidateOptionsMenu();
            tokenSource.Cancel();
            //2019.03.24 add end
            //tokenSource.Cancel();
            Utility.localeWord = Locale.Japan;
            Utility.localeMeaning = Locale.English;
            base.OnDestroy();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            //If There isn't Start activity although LearnWordlist activity exists. Finish this activity and start Start activity.
            if (!Utility.MultipleActivityFlag || Utility.cd.SequenceEqual(new List<int>() { 0, 0 }) || !Utility.cd.Any())
            {
                Finish();
                Utility.cd = new List<int>() { 0, 0 };
                Intent intent = new Intent(this, typeof(Start));
                StartActivity(intent);
                return;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
        public static class Message 
        {
            public static Dictionary<string, string> Edittag = new Dictionary<string, string>()
            {
                {"日本語","タグの編集"},
                {"English","Edit tag"},
                {"繁體中文","編輯標籤"},
                {"简体中文","编辑标签"},
                {"Deutsch","Tag bearbeiten"},
                {"Français","Modifier la balise"},
                {"한국어","태그 수정"},
                {"русский","Изменить тег"}
            }; 
            public static Dictionary<string, string> SpecifyDestination = new Dictionary<string, string>()
            {
                {"日本語","移動先のページ番号を指定して下さい"},
                {"English","Please specify the page number of the destination."},
                {"繁體中文","請指定目的地的頁碼。"},
                {"简体中文","请指定目的地的页码。"},
                {"Deutsch","Bitte geben Sie die Seitennummer des Ziels an."},
                {"Français","Veuillez spécifier le numéro de page de la destination."},
                {"한국어","대상 페이지 번호를 지정하십시오."},
                {"русский","Пожалуйста, укажите номер страницы назначения."}
            };
            public static Dictionary<string, string> AutoPlayConfirm = new Dictionary<string, string>()
            {
                {"日本語","自動再生を開始します。宜しいですか？"},
                {"English","Start auto playing.Are you sure?"},
                {"繁體中文","開始自動播放。你確定嗎？"},
                {"简体中文","开始自动播放。你确定吗？"},
                {"Deutsch","Starten Sie die automatische Wiedergabe. Sind Sie sicher?"},
                {"Français","Commencez la lecture automatique. Êtes-vous sûr?"},
                {"한국어","자동 연주 시작. 정말이야?"},
                {"русский","Запустите автоматическое воспроизведение. Вы уверены?"}
            };
            public static Dictionary<string, string> WarningPageSelect = new Dictionary<string, string>()
            {
                {"日本語","ページは1から" + Utility.WordandMeanings.Count + "までの整数で指定して下さい。"},
                {"English","The page must be an integer from 1 to "+Utility.WordandMeanings.Count},
                {"繁體中文","頁面必須是1到"+Utility.WordandMeanings.Count+"之間的整數。"},
                {"简体中文","页面必须是1到"+Utility.WordandMeanings.Count+"之间的整数。"},
                {"Deutsch","Die Seite muss eine ganze Zahl von 1 bis "+Utility.WordandMeanings.Count+" sein."},
                {"Français","La page doit être un entier compris entre 1 et "+Utility.WordandMeanings.Count},
                {"한국어","페이지는 1에서 "+Utility.WordandMeanings.Count+" 사이의 정수 여야합니다."},
                {"русский","Страница должна быть целым числом от 1 до "+Utility.WordandMeanings.Count}
            };
            public static Dictionary<string, string> Memo = new Dictionary<string, string>()
            {
                {"日本語","メモ"},
                {"English","Memo"},
                {"繁體中文","備忘錄"},
                {"简体中文","备忘录"},
                {"Deutsch","Memo"},
                {"Français","Note"},
                {"한국어","메모"},
                {"русский","напоминание"}
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public class GestureListener : SimpleOnGestureListener
        {
            private const int SWIPE_THRESHOLD = 100;
            private const int SWIPE_VELOCITY_THRESHOLD = 100;
            private Learn_Wordlist learn_Wordlist;
            
            public GestureListener(Learn_Wordlist learn_Wordlist)
            {
                this.learn_Wordlist = learn_Wordlist;
            }
            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                bool result = false;

                float diffY = e2.GetY() - e1.GetY();
                float diffX = e2.GetX() - e1.GetX();
                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (diffX > 0)
                        {
                            OnSwipeRight();
                        }
                        else
                        {
                            OnSwipeLeft();
                        }
                        result = true;
                    }
                }
                else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                {
                    if (diffY > 0)
                    {
                        OnSwipeBottom();
                    }
                    else
                    {
                        OnSwipeTop();
                    }
                    result = true;
                }
                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            public void OnSwipeRight()
            {
                learn_Wordlist.txtPrev_Click(new object(), new EventArgs());
            }
            /// <summary>
            /// 
            /// </summary>
            public void OnSwipeLeft()
            {
                learn_Wordlist.txtNext_Click(new object(), new EventArgs());
            }

            private void OnSwipeTop()
            {

            }

            private void OnSwipeBottom()
            {

            }
        }

    }
}