using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.IO;
using Android.Content;
using static Android.Views.View;
using Android.Views;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace WordLearning
{
    [Activity(Label = "Wordlist_Editword", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class Wordlist_Editword : CustomActivity
    {
        EditText etxtWord, etxtMeaning;
        int position;
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
            SetContentViewAndToolbar(Resource.Layout.Wordlist_Editword, Resource.Id.tbWordlist_Editword);
            etxtWord = FindViewById<EditText>(Resource.Id.etxtWord_Wordlist_Editword);
            etxtMeaning = FindViewById<EditText>(Resource.Id.etxtMeaning_Wordlist_Editword);
            position = Intent.GetIntExtra("Position", 0);
            etxtWord.Text = Utility.WordandMeanings[position].Wordname;
            etxtMeaning.Text = Utility.WordandMeanings[position].Wordmeaning;
            Utility.WordNumber = position;
            etxtWord.RequestFocus();
        }

        #region Wordlist_Editword
        /// <summary>
        /// Click register button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnRegister_Wordlist_Editword_Click(object sender, EventArgs e)
        {
            string etxtWord = this.etxtWord.Text;
            string etxtMeaning = this.etxtMeaning.Text;
            if (string.IsNullOrEmpty(etxtWord))
            {
                var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlg.SetTitle(Message.Enterword[Utility.language]);
                dlg.SetPositiveButton("OK", (_sender, _e) => { return; });
                dlg.Show();
            }
            else
            {
                Registerword(etxtWord, etxtMeaning);
                Finish();
            }
        }
        /// <summary>
        /// Click cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCancel_Wordlist_Editword_Click(object sender, EventArgs e)
        {
            Finish();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rlMain_Wordlist_Editword_Touch(object sender, TouchEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void etxtWord_Wordlist_Editword_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void etxtMeaning_Wordlist_Editword_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);
            }
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
            }
            return base.OnOptionsItemSelected(item);
        }
        /// <summary>
        /// Register word
        /// </summary>
        /// <param name="etxtWord"></param>
        /// <param name="etxtMeaning"></param>
        private void Registerword(string etxtWord, string etxtMeaning)
        {
            var xelm = XDocument.Load(Utility.WordListPath);
            var xelemcd =
                Utility.GetXElement(Utility.cd, xelm)
                .Elements()
                .Where(elm => elm.Name == "Word")
                .First(elm => elm.Element("Wordname").Value == XmlConvert.EncodeLocalName(Utility.WordandMeanings[position].Wordname)
                           && elm.Element("Wordmeaning").Value == XmlConvert.EncodeLocalName(Utility.WordandMeanings[position].Wordmeaning));
            xelemcd.Element("Wordname").Value = XmlConvert.EncodeLocalName(etxtWord);
            xelemcd.Element("Wordmeaning").Value = XmlConvert.EncodeLocalName(etxtMeaning);
            xelm.Save(Utility.WordListPath);
        }
        #endregion

        public static class Message
        {
            public static Dictionary<string, string> Enterword = new Dictionary<string, string>()
            {
                {"日本語","単語を入力してください"},
                {"English","Please Enter words."},
                {"繁體中文","請輸入單詞"},
                {"简体中文","请输入单词"},
                {"Deutsch","Bitte geben Sie Wörter ein"},
                {"Français","S'il vous plaît entrer des mots"},
                {"한국어","단어를 입력하십시오."},
                {"русский","Пожалуйста, введите слова"},
                {"इंडिया","कृपया शब्द दर्ज करें"}
            };
        }
    }
}