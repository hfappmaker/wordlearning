using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Collections.Generic; // ← 必須
using System.Linq; // ← こっちは任意。あれば便利機能が使える。
using Android.Content;
using static Android.Widget.AdapterView;
using Android.Views;
using Android.Transitions;
using System;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Linq;
using Android.Runtime;
using System.Collections;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V4.Content.Res;
using Android.Util;

namespace WordLearning
{
    [Activity(Label = "Edit_Wordlist")]
    public class Edit_Wordlist : CustomActivity
    {
        ListView listView;
        ListView Taglist;
        SeekBar[] seekBars = new SeekBar[3];
        TextInputEditText NewTagMeaning;
        string TagShape = "Square";
        int SelectTagPositon;
        Android.Support.V7.App.AlertDialog dlgChoose;
        //public bool imagebuttonsetflag;
        /// <summary>
        /// Ons the create.
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            //imagebuttonsetflag = false;
            base.OnCreate(savedInstanceState);
            //If There isn't Start activity although EditWordlist activity exists. Finish this activity and start Start activity.
            if (!Utility.MultipleActivityFlag || Utility.cd.SequenceEqual(new List<int> { 0, 0 }) || !Utility.cd.Any())
            {
                Finish();
                Utility.cd = new List<int> { 0, 0 };
                Intent intent = new Intent(this, typeof(Start));
                StartActivity(intent);
                return;
            }
            SetlistView();
        }
        /// <summary>
        /// Ons the key down.
        /// </summary>
        /// <returns><c>true</c>, if key down was oned, <c>false</c> otherwise.</returns>
        /// <param name="keyCode">Key code.</param>
        /// <param name="e">E.</param>
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && basetoolbarResID == Resource.Id.tbEdit_Wordlist_Deletemode)
            {
                listView.ClearChoices();
                listView.ChoiceMode = ChoiceMode.None;
                var state = listView.OnSaveInstanceState();
                listView.Adapter = new ArrayAdapter_Edit_Wordlist(this, Resource.Layout.row_Latest, Utility.WordandMeanings.Select(kvp => (kvp.Key, kvp.Value)).ToList());
                listView.OnRestoreInstanceState(state);
                ChangeToolbarTransition(Transitionmode.ToInit);
                return false;
            }
            else if (keyCode == Keycode.Back)
            {
                Utility.cd.RemoveAt(Utility.cd.Count - 1);
            }
            return base.OnKeyDown(keyCode, e);
        }
        //protected override void OnResume()
        //{
        //    base.OnResume();
        //}

        //protected override void OnPause()
        //{
        //    base.OnPause();
        //}

        #region Edit_Wordlist
        /// <summary>
        /// Click lv_Edit_Wordlist's item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void lv_Edit_Wordlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (basetoolbarResID == Resource.Id.tbEdit_Wordlist_Init)
            {
                ArrayAdapter_Edit_Wordlist adapter = (ArrayAdapter_Edit_Wordlist)listView.Adapter;
                if (adapter.layoutid.Equals(Android.Resource.Layout.SimpleListItemMultipleChoice)) { return; }
                Intent intent = new Intent(this, typeof(Wordlist_Editword));
                intent.PutExtra("Position", Utility.WordandMeanings.ElementAt(e.Position).Key);
                StartActivity(intent);
            }
            else
            {
                listView.GetChildAt(e.Position - listView.FirstVisiblePosition).SetBackgroundColor(Constant.SelectColor[!Utility.selectpositions[e.Position]]);
                Utility.selectpositions[e.Position] = !Utility.selectpositions[e.Position];
            }
        }
        //public void lv_Edit_Wordlist_Click(object sender ClickEventArgs e)
        //{

        //}
        /// <summary>
        /// Click lv_Edit_Wordlist's item long time 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void lv_Edit_Wordlist_ItemLongClick(object sender, ItemLongClickEventArgs e)
        {
            if (basetoolbarResID == Resource.Id.tbEdit_Wordlist_Init)
            {
                var state = listView.OnSaveInstanceState();
                listView.OnRestoreInstanceState(state);
                listView.ChoiceMode = ChoiceMode.Multiple;
                listView.SetItemChecked(e.Position, true);
                Utility.selectpositions[e.Position] = true;
                ChangeToolbarTransition(Transitionmode.ToDeletemode);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            LayoutInflater inflater = LayoutInflater.From(this);
            View view;
            ListView TagList;
            var xml = XDocument.Load(Utility.WordListPath);
            var wkTags = new List<(int, int[], string,string, bool)>();
            var Tags_xml = xml.Root.Element("Tagcolor").Elements().ToList();
            switch (item.ItemId)
            {
                case Resource.Id.action_add_Edit_Wordlist_Init:
                    Intent intent = new Intent(this, typeof(Wordlist_Addword));
                    StartActivity(intent);
                    break;
                case Resource.Id.action_delete_Edit_Wordlist_Deletemode:
                    var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                    dlg.SetMessage(Message.DeletewordConfirm[Utility.language]);
                    dlg.SetNegativeButton("CANCEL", Cancel);
                    dlg.SetPositiveButton("OK", Delete);
                    dlg.Show();
                    break;
                case Resource.Id.action_edit_Edit_Wordlist_Init:
                    var dlgEditTag = new Android.Support.V7.App.AlertDialog.Builder(this);
                    view = inflater.Inflate(Resource.Layout.Dialog_EditTag, null);
                    Android.Support.V7.Widget.Toolbar toolbar_dlg = view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbDialog_EditTag_include);
                    Taglist = view.FindViewById<ListView>(Resource.Id.lv_Dialog_EditTag);
                    List<(int, int[], string,string)> Taglistcontent = new List<(int, int[], string,string)>();
                    for (int i = 0; i < Tags_xml.Count; i++)
                    {
                        if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                        {
                            Taglistcontent.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value));
                        }
                    }
                    Taglist.Adapter = new ArrayAdapter_Taglist(this, Resource.Layout.row_Taglist, Taglistcontent);
                    Taglist.ItemClick += (s, e) =>
                    {
                        var dlgChoosebuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
                        ListView chooseitem = new ListView(this)
                        {
                            Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Message.Actionselectedtag[Utility.language])
                        };
                        chooseitem.ItemClick += (s1, e1) =>
                        {
                            switch (e1.Position)
                            {
                                case 0://edit
                                    xml = XDocument.Load(Utility.WordListPath);
                                    Tags_xml = xml.Root.Element("Tagcolor").Elements().ToList();
                                    Taglistcontent.Clear();
                                    for (int i = 0; i < Tags_xml.Count; i++)
                                    {
                                        if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                                        {
                                            Taglistcontent.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value), Tags_xml[i].Element("Shape").Value));
                                        }
                                    }
                                    var dlgNewTag = new Android.Support.V7.App.AlertDialog.Builder(this);
                                    SelectTagPositon = e.Position;
                                    View view_AddTag = inflater.Inflate(Resource.Layout.Dialog_CreateNewTag, null);
                                    int[] oldcolor = { defaultthemecolor[0], defaultthemecolor[1], defaultthemecolor[2] };
                                    seekBars[0] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Red_Dialog_CreateNewTag);
                                    seekBars[1] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Green_Dialog_CreateNewTag);
                                    seekBars[2] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Blue_Dialog_CreateNewTag);
                                    var rdogshape = view_AddTag.FindViewById<RadioGroup>(Resource.Id.rdogSelectshape);
                                    NewTagMeaning = view_AddTag.FindViewById<TextInputEditText>(Resource.Id.etxtMeaning_Dialog_CreateNewTag);
                                    ImageView imageView = view_AddTag.FindViewById<ImageView>(Resource.Id.ivDialog_CreateNewTag);
                                    Color Tagcolor = Color.Rgb(Taglistcontent[e.Position].Item2[0], Taglistcontent[e.Position].Item2[1], Taglistcontent[e.Position].Item2[2]);
                                    rdogshape.CheckedChange += (s2, e2) =>
                                    {
                                        switch (e2.CheckedId)
                                        {
                                            case Resource.Id.rdorect:
                                                var _drawable = GetDrawable(Resource.Drawable.square) as GradientDrawable;
                                                _drawable.Mutate();
                                                _drawable.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                imageView.SetImageDrawable(_drawable);
                                                TagShape = "Square";
                                                break;
                                            case Resource.Id.rdostar:
                                                var _drawable2 = GetDrawable(Resource.Drawable.star) as VectorDrawable;
                                                _drawable2.Mutate();
                                                _drawable2.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                imageView.SetImageDrawable(_drawable2);
                                                TagShape = "Star";
                                                break;
                                            case Resource.Id.rdoheart:
                                                var _drawable3 = GetDrawable(Resource.Drawable.heart) as VectorDrawable;
                                                _drawable3.Mutate();
                                                _drawable3.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                imageView.SetImageDrawable(_drawable3);
                                                TagShape = "Heart";
                                                break;
                                        }
                                    };
                                    switch (Taglistcontent[e.Position].Item4)
                                    {
                                        case "Square":
                                            var drawable1 = GetDrawable(Resource.Drawable.square) as GradientDrawable;
                                            drawable1.Mutate();
                                            drawable1.SetColorFilter(Tagcolor,PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(drawable1);
                                            TagShape = "Square";
                                            rdogshape.FindViewById<RadioButton>(Resource.Id.rdorect).Checked = true;
                                            break;
                                        case "Star":
                                            var drawable2 = GetDrawable(Resource.Drawable.star) as VectorDrawable;
                                            drawable2.Mutate();
                                            drawable2.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(drawable2);
                                            TagShape = "Star";
                                            rdogshape.FindViewById<RadioButton>(Resource.Id.rdostar).Checked = true;
                                            break;
                                        case "Heart":
                                            var drawable3 = GetDrawable(Resource.Drawable.heart) as VectorDrawable;
                                            drawable3.Mutate();
                                            drawable3.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(drawable3);
                                            TagShape = "Heart";
                                            rdogshape.FindViewById<RadioButton>(Resource.Id.rdoheart).Checked = true;
                                            break;
                                    };
                                    NewTagMeaning.Text = Taglistcontent[e.Position].Item3;
                                    Color[] Thumbcolor = { Color.Red, Color.Green, Color.Blue };
                                    int[][] ProgressTintColor = { new int[] { 255, 0, 0 }, new int[] { 0, 255, 0 }, new int[] { 0, 0, 255 } };
                                    for (int i = 0; i < seekBars.Count(); i++)
                                    {
                                        seekBars[i].Tag = i;
                                        seekBars[i].Progress = Taglistcontent[e.Position].Item2[i];
                                        seekBars[i].ProgressTintList = ColorStateList.ValueOf(Color.Argb(seekBars[i].Progress, ProgressTintColor[i][0], ProgressTintColor[i][1], ProgressTintColor[i][2]));
                                        seekBars[i].ThumbTintList = ColorStateList.ValueOf(Thumbcolor[i]);
                                        seekBars[i].ProgressChanged += (s2, e2) =>
                                        {
                                            e2.SeekBar.ProgressTintList = ColorStateList.ValueOf(Color.Argb(e2.Progress, ProgressTintColor[(int)e2.SeekBar.Tag][0], ProgressTintColor[(int)e2.SeekBar.Tag][1], ProgressTintColor[(int)e2.SeekBar.Tag][2]));
                                            Tagcolor = Color.Rgb(seekBars[0].Progress, seekBars[1].Progress, seekBars[2].Progress);
                                            switch (imageView.Drawable.GetType().Name)
                                            {
                                                case "GradientDrawable":
                                                    var _drawable = imageView.Drawable as GradientDrawable;
                                                    _drawable.Mutate();
                                                    _drawable.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                    break;
                                                case "VectorDrawable":
                                                    var _drawable2 = imageView.Drawable as VectorDrawable;
                                                    _drawable2.Mutate();
                                                    _drawable2.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                    break;
                                            }
                                            //drawable.SetColor(Tagcolor); // set stroke width and stroke color 
                                        };
                                    }
                                    dlgNewTag.SetView(view_AddTag);
                                    dlgNewTag.SetPositiveButton("CANCEL", (s2, e2) => { return; });
                                    dlgNewTag.SetNegativeButton("OK", EditTag);
                                    dlgNewTag.Show();
                                    break;
                                case 1://delete
                                    var dlgDelete = new Android.Support.V7.App.AlertDialog.Builder(this);
                                    dlgDelete.SetMessage(Message.Notimplement[Utility.language]);
                                    dlgDelete.SetNegativeButton("OK",(s2,e2) =>{});
                                    dlgDelete.Show();
                                    break;
                            }
                            dlgChoose.Dismiss();
                        };
                        dlgChoosebuilder.SetView(chooseitem);
                        dlgChoosebuilder.SetPositiveButton("CANCEL", (s1, e1) => { });
                        dlgChoose = dlgChoosebuilder.Create();
                        dlgChoose.Show();
                    };
                    toolbar_dlg.Title = Message.Edittag[Utility.language];
                    dlgEditTag.SetView(view);
                    toolbar_dlg.InflateMenu(Resource.Menu.menu_Dialog_EditTag);
                    toolbar_dlg.MenuItemClick += (_s, _e) =>
                    {
                        switch (_e.Item.ItemId)
                        {
                            case Resource.Id.action_add_Dialog_EditTag:
                                var dlgNewTag = new Android.Support.V7.App.AlertDialog.Builder(this);
                                View view_AddTag = inflater.Inflate(Resource.Layout.Dialog_CreateNewTag, null);
                                int[] oldcolor = { defaultthemecolor[0], defaultthemecolor[1], defaultthemecolor[2] };
                                if (Taglist.Count > 4)
                                {
                                    dlgNewTag.SetMessage(Message.MaxCountofTag[Utility.language]);
                                    dlgNewTag.SetNegativeButton("OK", (__s, __e) => { return; });
                                    dlgNewTag.Show();
                                    break;
                                }
                                seekBars[0] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Red_Dialog_CreateNewTag);
                                seekBars[1] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Green_Dialog_CreateNewTag);
                                seekBars[2] = view_AddTag.FindViewById<SeekBar>(Resource.Id.Sb_Blue_Dialog_CreateNewTag);
                                var rdogshape = view_AddTag.FindViewById<RadioGroup>(Resource.Id.rdogSelectshape);
                                NewTagMeaning = view_AddTag.FindViewById<TextInputEditText>(Resource.Id.etxtMeaning_Dialog_CreateNewTag);
                                ImageView imageView = view_AddTag.FindViewById<ImageView>(Resource.Id.ivDialog_CreateNewTag);
                                var drawable = GetDrawable(Resource.Drawable.square) as GradientDrawable;
                                Color Tagcolor = Color.Rgb(oldcolor[0], oldcolor[1], oldcolor[2]);
                                drawable.Mutate();
                                drawable.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable);
                                TagShape = "Square";
                                Color[] Thumbcolor = { Color.Red, Color.Green, Color.Blue };
                                int[][] ProgressTintColor = { new int[] { 255, 0, 0 }, new int[] { 0, 255, 0 }, new int[] { 0, 0, 255 } };
                                for (int i = 0; i < seekBars.Count(); i++)
                                {
                                    seekBars[i].Tag = i;
                                    seekBars[i].Progress = oldcolor[i];
                                    seekBars[i].ProgressTintList = ColorStateList.ValueOf(Color.Argb(seekBars[i].Progress, ProgressTintColor[i][0], ProgressTintColor[i][1], ProgressTintColor[i][2]));
                                    seekBars[i].ThumbTintList = ColorStateList.ValueOf(Thumbcolor[i]);
                                    seekBars[i].ProgressChanged += (__s, __e) =>
                                    {
                                        __e.SeekBar.ProgressTintList = ColorStateList.ValueOf(Color.Argb(__e.Progress, ProgressTintColor[(int)__e.SeekBar.Tag][0], ProgressTintColor[(int)__e.SeekBar.Tag][1], ProgressTintColor[(int)__e.SeekBar.Tag][2]));
                                        Tagcolor = Color.Rgb(seekBars[0].Progress, seekBars[1].Progress, seekBars[2].Progress);
                                        switch(imageView.Drawable.GetType().Name)
                                        {
                                            case "GradientDrawable":
                                                var _drawable = imageView.Drawable as GradientDrawable;
                                                _drawable.Mutate();
                                                _drawable.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                break;
                                            case "VectorDrawable":
                                                var _drawable2 = imageView.Drawable as VectorDrawable;
                                                _drawable2.Mutate();
                                                _drawable2.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                                break;
                                        }
                                    };
                                }
                                rdogshape.CheckedChange += (s1, e1) =>
                                {
                                    switch (e1.CheckedId)
                                    {
                                        case Resource.Id.rdorect:
                                            var _drawable = GetDrawable(Resource.Drawable.square) as GradientDrawable;
                                            _drawable.Mutate();
                                            _drawable.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(_drawable);
                                            TagShape = "Square";
                                            break;
                                        case Resource.Id.rdostar:
                                            var _drawable2 = GetDrawable(Resource.Drawable.star) as VectorDrawable;
                                            _drawable2.Mutate();
                                            _drawable2.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(_drawable2);
                                            TagShape = "Star";
                                            break;
                                        case Resource.Id.rdoheart:
                                            var _drawable3 = GetDrawable(Resource.Drawable.heart) as VectorDrawable;
                                            _drawable3.Mutate();
                                            _drawable3.SetColorFilter(Tagcolor, PorterDuff.Mode.Multiply);
                                            imageView.SetImageDrawable(_drawable3);
                                            TagShape = "Heart";
                                            break;
                                    }
                                };
                                dlgNewTag.SetView(view_AddTag);
                                dlgNewTag.SetPositiveButton("CANCEL", (__s, __e) => { return; });
                                dlgNewTag.SetNegativeButton("OK", CreateNewTag);
                                dlgNewTag.Show();
                                break;
                        }
                    };
                    dlgEditTag.SetPositiveButton("OK", (_s, _e) => { return; });
                    dlgEditTag.Show();
                    break;
                case Resource.Id.action_play_Edit_Wordlist_Init:
                    //inflater = LayoutInflater.From(this);
                    XElement Wordlist = Utility.GetXElement(Utility.cd, xml);
                    var dlgEditTagofWord = new Android.Support.V7.App.AlertDialog.Builder(this);
                    if (Wordlist.HasElements)
                    {
                        view = inflater.Inflate(Resource.Layout.Dialog_SetLearnWord, null);
                        TagList = view.FindViewById<ListView>(Resource.Id.lvTagList_Dialog_SetLearnWord);
                        for (int i = 0; i < Tags_xml.Count; i++)
                        {
                            if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                            {
                                wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value, false));
                            }
                        }
                        wkTags.Add((5, new int[] { 255, 255, 255 }, Message.NoTag[Utility.language],"Square", true));
                        TagList.Adapter = new ArrayAdapter_SelectedTag(this, Resource.Layout.row_SelectedTag, wkTags);
                        dlgEditTagofWord.SetView(view);
                        var dlgins = dlgEditTagofWord.Create();
                        dlgins.SetButton((int)DialogButtonType.Negative, "OK", MoveLearn_Wordlist);
                        dlgins.SetButton((int)DialogButtonType.Positive, "CANCEL", (_s, _e) => {});
                        dlgins.Show();
                    }
                    else
                    {
                        dlgEditTagofWord.SetMessage(Message.NoRegisteredWord[Utility.language]);
                        dlgEditTagofWord.SetPositiveButton("OK", (_s, _e) => { return; });
                        dlgEditTagofWord.Show();
                    }
                    break;
                case Android.Resource.Id.Home:
                    if (Utility.MultipleActivityFlag)
                    {
                        switch (basetoolbarResID)
                        {
                            case Resource.Id.tbEdit_Wordlist_Deletemode:
                                {
                                    listView.ClearChoices();
                                    listView.ChoiceMode = ChoiceMode.None;
                                    var state = listView.OnSaveInstanceState();
                                    listView.Adapter = new ArrayAdapter_Edit_Wordlist(this, Resource.Layout.row_Latest, Utility.WordandMeanings.Select(kvp => (kvp.Key, kvp.Value)).ToList());
                                    listView.OnRestoreInstanceState(state);
                                    ChangeToolbarTransition(Transitionmode.ToInit);
                                    break;
                                }
                            case Resource.Id.tbEdit_Wordlist_Init:
                                Utility.cd.RemoveAt(Utility.cd.Count - 1);
                                Finish();
                                break;
                        }
                    }
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Handles the event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void MoveLearn_Wordlist(object sender, DialogClickEventArgs e)
        {
            var xml = XDocument.Load(Utility.WordListPath);
            var elm = Utility.GetXElement(Utility.cd, xml);
            string voicelangWord = "English", voicelangMeaning = Utility.language;
            int Sleepcount = 5;
            try
            {
                voicelangWord = elm.Attribute("VoicelanguageWord").Value;
                voicelangMeaning = elm.Attribute("VoicelanguageMeaning").Value;
                Sleepcount = int.Parse(elm.Attribute("Sleepcount").Value);
            }
            catch (Exception)
            {
                voicelangWord = "English";
                voicelangMeaning = Utility.language;
                Sleepcount = 5;
            }
            Utility.localeWord = Utility.Localedict[voicelangWord];
            Utility.localeMeaning = Utility.Localedict[voicelangMeaning];
            Utility.Sleepcount = Sleepcount;
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
            bool predicate(KeyValuePair<int, (string Wordname, string Wordmeaning, string Tag,string Memo)> wordandmeaning)
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
                dlgbuilder.SetMessage(Message.NoMatchWord[Utility.language]);
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
        /// Edits the tag.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void EditTag(object sender, DialogClickEventArgs e)
        {
            var xelm = XDocument.Load(Utility.WordListPath);
            XElement EditTag_xml = xelm.Root.Element("Tagcolor").Elements().ToList()[SelectTagPositon];
            EditTag_xml.Element("Red").Value = seekBars[0].Progress.ToString();
            EditTag_xml.Element("Green").Value = seekBars[1].Progress.ToString();
            EditTag_xml.Element("Blue").Value = seekBars[2].Progress.ToString();
            EditTag_xml.Element("Meaning").Value = XmlConvert.EncodeLocalName(NewTagMeaning.Text);
            EditTag_xml.Element("Shape").Value = TagShape;
            xelm.Save(Utility.WordListPath);
            List<XElement> Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
            List<(int, int[], string,string)> Taglistcontent = new List<(int, int[], string,string)>();
            for (int i = 0; i < Tags_xml.Count; i++)
            {
                if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                {
                    Taglistcontent.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value));
                }
            }
            Taglist.Adapter = new ArrayAdapter_Taglist(this, Resource.Layout.row_Taglist, Taglistcontent);
        }

        /// <summary>
        /// Creates the new tag.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CreateNewTag(object sender, DialogClickEventArgs e)
        {
            var xelm = XDocument.Load(Utility.WordListPath);
            List<XElement> Tags_xml = xelm.Root.Elements("Tagcolor").Elements().ToList();
            foreach (XElement Tag_xml in Tags_xml)
            {
                if (!int.TryParse(Tag_xml.Element("Red").Value, out _))
                {
                    Tag_xml.Element("Red").Value = seekBars[0].Progress.ToString();
                    Tag_xml.Element("Green").Value = seekBars[1].Progress.ToString();
                    Tag_xml.Element("Blue").Value = seekBars[2].Progress.ToString();
                    Tag_xml.Element("Meaning").Value = XmlConvert.EncodeLocalName(NewTagMeaning.Text);
                    Tag_xml.Element("Shape").Value = TagShape;
                    break;
                }
            }
            xelm.Save(Utility.WordListPath);
            Tags_xml = xelm.Root.Elements("Tagcolor").Elements().ToList();
            List<(int, int[], string,string)> Taglistcontent = new List<(int, int[], string,string)>();
            for (int i = 0; i < Tags_xml.Count; i++)
            {
                if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                {
                    Taglistcontent.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value));
                }
            }
            Taglist.Adapter = new ArrayAdapter_Taglist(this, Resource.Layout.row_Taglist, Taglistcontent);

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Buttons the add dialog edit tag click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void btnAdd_Dialog_EditTag_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Buttons the edit dialog edit tag click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void btnEdit_Dialog_EditTag_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transitionmode"></param>
        private void ChangeToolbarTransition(Transitionmode transitionmode)
        {
            ViewGroup rootscene;
            Scene scene;
            Fade transition;
            rootscene = (ViewGroup)FindViewById(Resource.Id.flToolbar_Edit_Wordlist);
            scene = Scene.GetSceneForLayout(rootscene, int.Parse(KeyValuePairs[transitionmode][TransitionSet.SceneLayout].ToString()), this);
            transition = new Fade();
            transition.SetDuration(150L);
            TransitionManager.Go(scene, transition);
            menuResID = int.Parse(KeyValuePairs[transitionmode][TransitionSet.menuResID].ToString());
            basetoolbarResID = int.Parse(KeyValuePairs[transitionmode][TransitionSet.ToolbarLayout].ToString());
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(int.Parse(KeyValuePairs[transitionmode][TransitionSet.ToolbarLayout].ToString()));
            SetSupportActionBar(toolbar);
            OnCreateOptionsMenu(toolbar.Menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(bool.Parse(KeyValuePairs[transitionmode][TransitionSet.HomeButtonEnabled].ToString()));
            SupportActionBar.SetHomeButtonEnabled(bool.Parse(KeyValuePairs[transitionmode][TransitionSet.HomeButtonEnabled].ToString()));
            SupportActionBar.Title = KeyValuePairs[transitionmode][TransitionSet.Title].ToString();
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
        /// <summary>
        /// When returning from Wordlist_Addword or Wordlist_Editword
        /// </summary>
        protected override void OnRestart()
        {
            base.OnRestart();
            //If There isn't Start activity although EditWordlist activity exists. Finish this activity and start Start activity.
            if (!Utility.MultipleActivityFlag || Utility.cd.SequenceEqual(new List<int> { 0, 0 }) || !Utility.cd.Any())
            {
                Finish();
                Utility.cd = new List<int> { 0, 0 };
                Intent intent = new Intent(this, typeof(Start));
                StartActivity(intent);
                return;
            }
            SetlistView();
        }

        protected override void OnPause()
        {

            base.OnPause();
        }

        protected override void OnDestroy()
        {
            Utility.MultipleActivityFlag = false;
            base.OnDestroy();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Delete(object sender, DialogClickEventArgs e)
        {
            var xelm = XDocument.Load(Utility.WordListPath);
            ArrayAdapter_Edit_Wordlist adapter = (ArrayAdapter_Edit_Wordlist)listView.Adapter;
            int j = 0;
            for (int i = 0; i < Utility.selectpositions.Count; i++)
            {
                if (Utility.selectpositions[i] == true)
                {
                    var XElement = Utility.GetXElement(Utility.cd, xelm)
                        .Elements()
                        .Where(elm => elm.Name == "Word")
                        .FirstOrDefault(elm => elm.Element("Wordname").Value == XmlConvert.EncodeLocalName(adapter.list[j].Item2.Wordname)
                        && elm.Element("Wordmeaning").Value == XmlConvert.EncodeLocalName(adapter.list[j].Item2.Wordmeaning)
                        && elm.Element("Tag").Value == adapter.list[j].Item2.Tag);
                    if(XElement != null)
                    {
                        XElement.Remove();
                        Utility.selectpositions.RemoveAt(i);
                        i--;
                    }
                }
                j++;
            }
            xelm.Save(Utility.WordListPath);
            Utility.Read_Wordlist();
            listView.Adapter = new ArrayAdapter_Edit_Wordlist(this, Resource.Layout.row_Latest, Utility.WordandMeanings.Select(kvp => (kvp.Key, kvp.Value)).ToList());
            listView.ChoiceMode = ChoiceMode.None;
            ChangeToolbarTransition(Transitionmode.ToInit);
            Toast.MakeText(this,Message.Deleted[Utility.language], ToastLength.Short).Show();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Cancel(object sender, DialogClickEventArgs e)
        {
            Utility.Read_Wordlist();
            listView.Adapter = new ArrayAdapter_Edit_Wordlist(this, Resource.Layout.row_Latest, Utility.WordandMeanings.Select(kvp => (kvp.Key, kvp.Value)).ToList());
            listView.ChoiceMode = ChoiceMode.None;
            ChangeToolbarTransition(Transitionmode.ToInit);
        }
        /// <summary>
        /// Setlists the view.
        /// </summary>
        public void SetlistView()
        {
            SetContentViewAndToolbar(Resource.Layout.Edit_Wordlist, Resource.Id.tbEdit_Wordlist, Resource.Menu.menu_Edit_Wordlist_Init);
            listView = FindViewById<ListView>(Resource.Id.lv_Edit_Wordlist);
            listView.Adapter = new ArrayAdapter_Edit_Wordlist(this, Resource.Layout.row_Latest, Utility.WordandMeanings.Select(p => (p.Key, p.Value)).ToList());
            Utility.selectpositions = new List<bool>();
            for (int i = 0; i < Utility.WordandMeanings.Count; i++)
            {
                Utility.selectpositions.Add(false);
            }
        }

        private Dictionary<Transitionmode, Dictionary<TransitionSet, object>> KeyValuePairs { get; set; } = new Dictionary<Transitionmode, Dictionary<TransitionSet, object>>
        {
            {
                Transitionmode.ToDeletemode,
                new Dictionary<TransitionSet,object>
                {
                  { TransitionSet.SceneLayout,Resource.Layout.Toolbar_Edit_Wordlist_Deletemode},
                  { TransitionSet.ToolbarLayout,Resource.Id.tbEdit_Wordlist_Deletemode },
                  { TransitionSet.menuResID, Resource.Menu.menu_Edit_Wordlist_Deletemode},
                  { TransitionSet.HomeButtonEnabled,true},
                  { TransitionSet.Title, Message.DeleteWord[Utility.language]}
                }
            },
            {
                Transitionmode.ToInit,
                new Dictionary<TransitionSet,object>
                {
                    { TransitionSet.SceneLayout,Resource.Layout.Toolbar_Edit_Wordlist_Init},
                    { TransitionSet.ToolbarLayout,Resource.Id.tbEdit_Wordlist_Init},
                    { TransitionSet.menuResID,Resource.Menu.menu_Edit_Wordlist_Init},
                    { TransitionSet.HomeButtonEnabled,true},
                    { TransitionSet.Title,Utility.WordListName}
                }
            }
        };

        private enum TransitionSet { SceneLayout, ToolbarLayout, menuResID, HomeButtonEnabled, Title }

        private enum Transitionmode { ToDeletemode, ToInit }
        public static class Message 
        {
            public static Dictionary<string, string> DeletewordConfirm = new Dictionary<string, string>
            {
                {"日本語","選択した単語を削除します。\r\n宜しいですか？"},
                {"English","Delete the selected word.\r\nIs it OK?"},
                {"繁體中文","刪除所選單詞。\r\n可以嗎？"},
                {"简体中文","删除所选单词。\r\n可以吗？"},
                {"Deutsch","Löschen Sie das ausgewählte Wort.\r\nIst es o.k?"},
                {"Français","Supprimer le mot sélectionné.\r\nEst-ce que c'est bon?"},
                {"한국어","선택한 단어를 삭제하십시오.\r\n괜찮습니까?"},
                {"русский","Удалить выбранное слово.\r\nЭто нормально?"}
            };

            public static Dictionary<string, string[]> Actionselectedtag = new Dictionary<string, string[]>
            {
                {"日本語",new string[] { "選択したタグを編集する", "選択したタグを削除する" }},
                {"English",new string[]{"Edit the selected tag.", "Delete the selected tag." } },
                {"繁體中文",new string[]{ "編輯選定的標籤。", "刪除所選標籤。" } },
                {"简体中文",new string[]{ "编辑选定的标签。", "删除所选标签。" } },
                {"Deutsch",new string[]{ "Bearbeiten Sie das ausgewählte Tag.", "Löschen Sie das ausgewählte Tag." } },
                {"Français",new string[]{ "Modifier la balise sélectionnée.", "Supprimer la balise sélectionnée." } },
                {"한국어",new string[]{ "선택한 태그를 편집하십시오.", "선택한 태그를 삭제하십시오." } },
                {"русский",new string[]{ "Редактировать выбранный тег.", "Удалить выбранный тег." } }
            };
            public static Dictionary<string, string> Notimplement = new Dictionary<string, string>
            {
                {"日本語","すみません、まだ未実装です。m(_ _)m"},
                {"English","Sorry, not implemented yet."},
                {"繁體中文","對不起，還沒有實現。"},
                {"简体中文","对不起，还没有实现。"},
                {"Deutsch","Sorry, noch nicht implementiert."},
                {"Français","Désolé, pas encore implémenté."},
                {"한국어","죄송합니다. 아직 구현되지 않았습니다."},
                {"русский","Извините, пока не реализовано."}
            };
            public static Dictionary<string, string> Edittag = new Dictionary<string, string>
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
            public static Dictionary<string, string> MaxCountofTag = new Dictionary<string, string>
            {
                {"日本語","タグは最大5つまでです"},
                {"English","Tag is allowed 5 at the most."},
                {"繁體中文","標籤最多允許5。"},
                {"简体中文","标签最多允许5。"},
                {"Deutsch","Tag ist maximal 5 erlaubt."},
                {"Français","La balise est autorisée 5 au maximum."},
                {"한국어","태그는 최대 5 개까지 허용됩니다."},
                {"русский","Метка допускается максимум 5."}
            };
            public static Dictionary<string, string> NoTag = new Dictionary<string, string>
            {
                {"日本語","タグなし"},
                {"English","No tag"},
                {"繁體中文","沒有標籤"},
                {"简体中文","没有标签"},
                {"Deutsch","Keine Markierung"},
                {"Français","Aucun tag"},
                {"한국어","태그 없음"},
                {"русский","Нет тега"},
            }; 
            public static Dictionary<string, string> NoRegisteredWord = new Dictionary<string, string>
            {
                {"日本語","単語が登録されていません"},
                {"English","Word is not registered."},
                {"繁體中文","單詞未註冊"},
                {"简体中文","单词未注册"},
                {"Deutsch","Wörter sind nicht registriert"},
                {"Français","Les mots ne sont pas enregistrés"},
                {"한국어","단어가 등록되지 않았습니다."},
                {"русский","Слова не зарегистрированы"},
            }; 
            public static Dictionary<string, string> NoMatchWord = new Dictionary<string, string>
            {
                {"日本語","条件を満たす単語が存在しません"},
                {"English","matched word is not registered."},
                {"繁體中文","匹配的單詞未註冊"},
                {"简体中文","匹配的单词未注册"},
                {"Deutsch","übereinstimmendes Wort ist nicht registriert"},
                {"Français","le mot correspondant n'est pas enregistré"},
                {"한국어","일치하는 단어가 등록되지 않았습니다."},
                {"русский","подходящее слово не зарегистрировано"},
            };
            public static Dictionary<string, string> Deleted = new Dictionary<string, string>
            {
                {"日本語","削除されました"},
                {"English","Delete is done"},
                {"繁體中文","刪除完成"},
                {"简体中文","删除完成"},
                {"Deutsch","Löschen ist fertig"},
                {"Français","Supprimer est terminé"},
                {"한국어","삭제가 완료되었습니다."},
                {"русский","Удаление сделано"},
            }; 
            public static Dictionary<string, string> DeleteWord = new Dictionary<string, string>
            {
                {"日本語","単語の削除"},
                {"English","Delete word"},
                {"繁體中文","刪除單詞"},
                {"简体中文","删除单词"},
                {"Deutsch","Wort löschen"},
                {"Français","Supprimer le mot"},
                {"한국어","단어 삭제"},
                {"русский","Удалить слово"},
            };
            public static Dictionary<string, string> InputMemo = new Dictionary<string, string>
            {
                {"日本語","メモを入力して下さい"},
                {"English","Please Input memo"},
                {"繁體中文","請輸入備忘錄"},
                {"简体中文","请输入备忘录"},
                {"Deutsch","Bitte Memo eingeben"},
                {"Français","S'il vous plaît entrer un mémo"},
                {"한국어","메모를 입력하십시오."},
                {"русский","Пожалуйста, введите памятку"},
            };

        }


        #endregion
    }
    /// <summary>
    /// Array adapter edit wordlist.
    /// </summary>
    public class ArrayAdapter_Edit_Wordlist : CustomArrayAdapter
    {
        public List<(int, (string Wordname, string Wordmeaning, string Tag,string Memo))> list;
        readonly Edit_Wordlist edit_Wordlist;
        public ArrayAdapter_Edit_Wordlist(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(int, (string Wordname, string Wordmeaning, string Tag,string Memo))>)objects;
            edit_Wordlist = (Edit_Wordlist)context;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!Utility.MultipleActivityFlag) return new View(edit_Wordlist);
            (int, (string Wordname, string Wordmeaning, string Tag,string Memo)) item1 = new ValueTuple<int, (string Wordname, string Wordmeaning, string Tag,string Memo)>();
            ListView listView = (ListView)parent;
            if (list != null)
            {
                bool imagebuttonflag;
                View view;
                //Because reusing convertview causes being disturbed display.Don't reuse convertview.
                //if (convertView != null)
                //{
                //    imagebuttonflag = true;
                //    view = convertView;
                //}
                //else
                {
                    imagebuttonflag = true;
                    view = Inflater.Inflate(layoutid, null);
                    view.SetPaddingRelative(48, 48, 48, 48);
                }
                item1 = list[position];
                TextView text = null;
                TextView text2 = null;
                ImageButton imageButton = null;
                CheckedTextView ctext = null;
                GridLayout gridLayout = null;
                ImageView imageView = null;
                GradientDrawable Gdrawable = null;
                VectorDrawable Vdrawable = null;
                switch (layoutid)
                {
                    case Resource.Layout.row_Latest:
                        view.SetBackgroundColor(Constant.SelectColor[Utility.selectpositions[position]]);
                        text = view.FindViewById<TextView>(Resource.Id.tvWord_row_Latest);
                        text.Text = item1.Item2.Wordname;
                        text2 = view.FindViewById<TextView>(Resource.Id.tvWordmeaning_row_Latest);
                        text2.Text = item1.Item2.Wordmeaning;
                        imageButton = view.FindViewById<ImageButton>(Resource.Id.ibrow_Latest);
                        gridLayout = view.FindViewById<GridLayout>(Resource.Id.gl_Taglist_row_Latest);
                        var xelm = XDocument.Load(Utility.WordListPath);
                        List<XElement> Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
                        List<(int, int[], string, string,bool)> wkTags = new List<(int, int[], string,string, bool)>();
                        bool[] tagselect = new bool[5];
                        string strtagselect = item1.Item2.Tag;
                        for (int i = 0; i < 5; i++)
                        {
                            tagselect[i] = Convert.ToBoolean(int.Parse(strtagselect.Substring(i, 1)));
                        }
                        for (int i = 0; i < Tags_xml.Count; i++)
                        {
                            if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                            {
                                wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value),Tags_xml[i].Element("Shape").Value, tagselect[i]));
                            }
                        }
                        gridLayout.RemoveAllViews();
                        int j = 0;
                        for (int i = 0; i < wkTags.Count;i++)
                        {
                            if (wkTags[i].Item5)
                            {
                                imageView = new ImageView(edit_Wordlist);
                                switch(wkTags[i].Item4)
                                {
                                    case "Square":
                                        Gdrawable = edit_Wordlist.GetDrawable(Resource.Drawable.square) as GradientDrawable;
                                        Gdrawable.Mutate();
                                        Gdrawable.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                                        imageView.SetImageDrawable(Gdrawable);
                                        break;
                                    case "Star":
                                        Vdrawable = edit_Wordlist.GetDrawable(Resource.Drawable.star) as VectorDrawable;
                                        Vdrawable.Mutate();
                                        Vdrawable.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                                        imageView.SetImageDrawable(Vdrawable);
                                        break;
                                    case "Heart":
                                        Vdrawable = edit_Wordlist.GetDrawable(Resource.Drawable.heart) as VectorDrawable;
                                        Vdrawable.Mutate();
                                        Vdrawable.SetColorFilter(Color.Rgb(wkTags[i].Item2[0], wkTags[i].Item2[1], wkTags[i].Item2[2]), PorterDuff.Mode.Multiply);
                                        imageView.SetImageDrawable(Vdrawable);
                                        break;
                                }
                                var param = new GridLayout.LayoutParams
                                {
                                    ColumnSpec = GridLayout.InvokeSpec(5 - j)
                                };
                                j++;
                                imageView.LayoutParameters = param;
                                gridLayout.AddView(imageView);
                            }
                        }
                        if (imagebuttonflag)
                        {
                            imageButton.Click += (_s, _e) =>
                            {
                                if (edit_Wordlist.basetoolbarResID == Resource.Id.tbEdit_Wordlist_Init)
                                {
                                    var menu = new PopupMenu(edit_Wordlist, imageButton);
                                    menu.Inflate(Resource.Menu.menu_popup_Dialog_EditTag);
                                    menu.MenuItemClick += (s, e) =>
                                    {
                                        switch (e.Item.ItemId)
                                        {
                                            case Resource.Id.menuitem_EditTag_Dialog_EditTag:
                                                xelm = XDocument.Load(Utility.WordListPath);
                                                menu.Dismiss();
                                                wkTags = new List<(int, int[], string, string, bool)>();
                                                Tags_xml = xelm.Root.Element("Tagcolor").Elements().ToList();
                                                var dlgEditTagofWord = new Android.Support.V7.App.AlertDialog.Builder(edit_Wordlist);
                                                for (int i = 0; i < Tags_xml.Count; i++)
                                                {
                                                    if (int.TryParse(Tags_xml[i].Element("Red").Value, out _))
                                                    {
                                                        wkTags.Add((i, new int[] { int.Parse(Tags_xml[i].Element("Red").Value), int.Parse(Tags_xml[i].Element("Green").Value), int.Parse(Tags_xml[i].Element("Blue").Value) }, XmlConvert.DecodeName(Tags_xml[i].Element("Meaning").Value), Tags_xml[i].Element("Shape").Value, tagselect[i]));
                                                    }
                                                }
                                                ListView SelectedTag = new ListView(edit_Wordlist)
                                                {
                                                    Adapter = new ArrayAdapter_SelectedTag(edit_Wordlist, Resource.Layout.row_SelectedTag, wkTags),
                                                    Id = Constant.FreeDlgId,
                                                    ChoiceMode = ChoiceMode.Multiple
                                                };
                                                int k = 0;
                                                foreach ((int, int[], string, string, bool) wkTag in wkTags)
                                                {
                                                    SelectedTag.SetTag(0x7000000 + k, wkTag.Item1);
                                                    k++;
                                                }
                                                SelectedTag.SetTag(Constant.FreeTagKey, item1.Item1);
                                                //not working??
                                                //SelectedTag.ItemClick += (s1,e1) => {
                                                //    CheckBox chk = SelectedTag.FindViewById<CheckBox>(Resource.Id.chk_row_SelectedTag);
                                                //    chk.Checked = !chk.Checked;
                                                //};
                                                //SparseBooleanArray checks = SelectedTag.CheckedItemPositions;
                                                //chk.CheckedChange += (s1, e1) => { Utility.selectpositions[};
                                                var dlgEditTagofWordInstance = dlgEditTagofWord.Create();
                                                dlgEditTagofWordInstance.SetView(SelectedTag);
                                                dlgEditTagofWordInstance.SetButton((int)DialogButtonType.Negative, "OK", SetTagofWord);
                                                dlgEditTagofWordInstance.SetButton((int)DialogButtonType.Positive, "CANCEL", (s3, e3) => { return; });
                                                dlgEditTagofWordInstance.Show();
                                                break;
                                            case Resource.Id.menuitem_EditMemo_Dialog_EditTag:
                                                item1 = list[position];
                                                xelm = XDocument.Load(Utility.WordListPath);
                                                var dlgEditMemo = new Android.Support.V7.App.AlertDialog.Builder(edit_Wordlist) 
                                                {
                                                   // LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, 70)
                                                };
                                                EditText memo = new EditText(edit_Wordlist)
                                                {
                                                    Id = Constant.FreeDlgId,
                                                    Text = item1.Item2.Memo,
                                                };
                                                memo.SetMaxHeight(70 * 4); //2019.03.24 add
                                                dlgEditMemo.SetMessage(Edit_Wordlist.Message.InputMemo[Utility.language]);
                                                dlgEditMemo.SetView(memo);
                                                dlgEditMemo.SetPositiveButton("OK",(s1,e1) => 
                                                {
                                                    xelm = XDocument.Load(Utility.WordListPath);
                                                    Utility.cd.Add(item1.Item1);
                                                    var elm = Utility.GetXElement(Utility.cd, xelm);
                                                    Utility.cd.RemoveAt(Utility.cd.Count - 1);
                                                    elm.Element("Memo").Value = XmlConvert.EncodeLocalName(memo.Text);
                                                    xelm.Save(Utility.WordListPath);
                                                    Utility.Read_Wordlist();
                                                    list[position] = new ValueTuple<int, (string Wordname, string Wordmeaning, string Tag, string Memo)>(item1.Item1, (item1.Item2.Wordname, item1.Item2.Wordmeaning, item1.Item2.Tag, memo.Text));
                                                });
                                                dlgEditMemo.SetNegativeButton("CANCEL", (s1, e1) => {});
                                                var dlgEditMemo2 = dlgEditMemo.Create();
                                                dlgEditMemo2.Show();
                                                memo.FocusChange += (s1, e1) =>
                                                {
                                                    dlgEditMemo2.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
                                                };
                                                memo.RequestFocus();
                                                break;
                                        }
                                    };
                                    menu.Show();
                                }
                            };
                        }
                        break;
                    case Android.Resource.Layout.SimpleListItemMultipleChoice:
                        ctext = view.FindViewById<CheckedTextView>(Android.Resource.Id.Text1);
                        ctext.Text = item1.Item2.Wordname;
                        break;
                }
                return view;
            }
            return base.GetView(position, convertView, parent);
        }

        private void SetTagofWord(object sender, DialogClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog dlgEditTagofWord = (Android.Support.V7.App.AlertDialog)sender;
            ListView lv = dlgEditTagofWord.FindViewById<ListView>(Constant.FreeDlgId);
            List<(int, int[], string,string, bool)> tagselectlist = ((ArrayAdapter_SelectedTag)lv.Adapter).list;
            var xml = XDocument.Load(Utility.WordListPath); 
            Utility.cd.Add((int)lv.GetTag(Constant.FreeTagKey));
            XElement Word_xml = Utility.GetXElement(Utility.cd, xml);
            for (int i = 0; i < tagselectlist.Count; i++)
            {
                Word_xml.Element("Tag").Value = Word_xml.Element("Tag").Value.Substring(0, (int)lv.GetTag(i + 0x7000000)) + Convert.ToInt32(tagselectlist[i].Item5).ToString() + Word_xml.Element("Tag").Value.Substring((int)lv.GetTag(i + 0x7000000) + 1);
            }
            xml.Save(Utility.WordListPath);
            Utility.cd.RemoveAt(Utility.cd.Count -1);
            edit_Wordlist.SetlistView();
        }
    }
    /// <summary>
    /// Array adapter taglist.
    /// </summary>
    public class ArrayAdapter_Taglist : CustomArrayAdapter
    {
        public List<(int, int[], string,string)> list;
        readonly Edit_Wordlist edit_Wordlist;
        public ArrayAdapter_Taglist(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(int, int[], string,string)>)objects;
            edit_Wordlist = (Edit_Wordlist)context;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            (int, int[], string,string) item1 = new ValueTuple<int, int[], string,string>();
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
                item1 = list[position];
                TextView text = null;
                ImageView imageView = null;
                switch (layoutid)
                {
                    case Resource.Layout.row_Taglist:
                        text = view.FindViewById<TextView>(Resource.Id.tvMeaning_row_Taglist);
                        text.Text = item1.Item3;
                        imageView = view.FindViewById<ImageView>(Resource.Id.iv_row_Taglist);
                        switch(item1.Item4)
                        {
                            case "Square":
                                var drawable1 = edit_Wordlist.GetDrawable(Resource.Drawable.square);//imageView.Drawable as GradientDrawable;
                                drawable1.Mutate();
                                drawable1.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable1);
                                break;
                            case "Star":
                                var drawable2 = edit_Wordlist.GetDrawable(Resource.Drawable.star);
                                drawable2.Mutate();
                                drawable2.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable2);
                                break;
                            case "Heart":
                                var drawable3 = edit_Wordlist.GetDrawable(Resource.Drawable.heart);
                                drawable3.Mutate();
                                drawable3.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable3);
                                break;
                        }
                        break;
                }
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }
    /// <summary>
    /// Array adapter selected tag.
    /// </summary>
    public class ArrayAdapter_SelectedTag : CustomArrayAdapter
    {
        public List<(int, int[], string,string, bool)> list;
        private readonly Context context;
        public ArrayAdapter_SelectedTag(Context context, int resource, IList objects) : base(context, resource, objects)
        {
            list = (List<(int, int[], string,string, bool)>)objects;
            this.context = context;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            bool convertflag;
            (int, int[], string,string, bool) item1;
            ListView listView = (ListView)parent;
            if (list != null)
            {
                View view;
                //if (convertView != null)
                //{
                    convertflag = true;
                    view = convertView;
                //}
                //else
                //{
                    convertflag = false;
                    view = Inflater.Inflate(layoutid, null);
                    view.SetPaddingRelative(48, 48, 48, 48);
                //}
                item1 = list[position];
                TextView text = null;
                ImageView imageView = null;
                CheckBox checkBox = null;
                switch (layoutid)
                {
                    case Resource.Layout.row_SelectedTag:
                        view.SetTag(Constant.FreeTagKey,position);
                        text = view.FindViewById<TextView>(Resource.Id.tvMeaning_row_SelectedTag);
                        imageView = view.FindViewById<ImageView>(Resource.Id.iv_row_SelectedTag);
                        checkBox = view.FindViewById<CheckBox>(Resource.Id.chk_row_SelectedTag);
                        checkBox.SetTag(Constant.FreeTagKey, position);
                        checkBox.Checked = item1.Item5;
                        text.Text = item1.Item3;
                        view.Click += (s, e) => { checkBox.Checked = !checkBox.Checked; };
                        switch (item1.Item4)
                        {
                            case "Square":
                                var drawable1 = context.GetDrawable(Resource.Drawable.square);
                                drawable1.Mutate();
                                drawable1.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable1);
                                break;
                            case "Star":
                                var drawable2 = context.GetDrawable(Resource.Drawable.star);
                                drawable2.Mutate();
                                drawable2.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable2);
                                break;
                            case "Heart":
                                var drawable3 = context.GetDrawable(Resource.Drawable.heart);
                                drawable3.Mutate();
                                drawable3.SetColorFilter(Color.Rgb(item1.Item2[0], item1.Item2[1], item1.Item2[2]), PorterDuff.Mode.Multiply);
                                imageView.SetImageDrawable(drawable3);
                                break;
                        }
                        if(!convertflag)
                        {
                            checkBox.CheckedChange += (s, e) => 
                            {
                                int pst = int.Parse(((CheckBox)s).GetTag(Constant.FreeTagKey).ToString());
                                list[pst] = new ValueTuple<int, int[], string,string, bool>(list[pst].Item1, list[pst].Item2, list[pst].Item3, list[pst].Item4,!list[pst].Item5);
                            };
                        }
                        imageView.Visibility = ViewStates.Visible;
                        if (item1.Item1 == 5)
                        {
                            imageView.Visibility = ViewStates.Invisible;
                        }
                        break;
                }
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }
    /// <summary>
    /// Array adapter selected tag grid view.
    /// </summary>
    public class ArrayAdapter_SelectedTagGridView : CustomArrayAdapter
    {
        public List<ImageView> list;
        public ArrayAdapter_SelectedTagGridView(Context context,int resource,IList objects):base(context,resource,objects)
        {
            list = (List<ImageView>)objects;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView item;
            if(list!=null)
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
                ImageView imageView = view.FindViewById<ImageView>(Resource.Id.iv_selectedtag_griditem_SelectedTag);
                imageView = item;
                return view;
            }
            return base.GetView(position, convertView, parent);
        }
    }
}