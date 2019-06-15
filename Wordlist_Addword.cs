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
using Android.Support.Design.Widget;
using static Android.Widget.AdapterView;
using System.Collections.Generic;

namespace WordLearning
{
    [Activity(Label = "Wordlist_Addword", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class Wordlist_Addword : CustomActivity
    {
        EditText etxtWord, etxtMeaning;
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
            SetContentViewAndToolbar(Resource.Layout.Wordlist_Addword, Resource.Id.tbWordlist_Addword);
            etxtWord = FindViewById<EditText>(Resource.Id.etxtWord_Wordlist_Addword);
            etxtMeaning = FindViewById<EditText>(Resource.Id.etxtMeaning_Wordlist_Addword);
            etxtWord.RequestFocus();
        }

        #region event
        /// <summary>
        /// Click register button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnRegister_Wordlist_Addword_Click(object sender, EventArgs e)
        {
            Registerword();
        }
        /// <summary>
        /// Click cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCancel_Wordlist_Addword_Click(object sender, EventArgs e)
        {
            Finish();
        }
        /// <summary>
        /// Click register and next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnRegister_and_Next_Wordlist_Addword_Click(object sender, EventArgs e)
        {
            if (Registerword())
            {
                Intent intent = new Intent(this, typeof(Wordlist_Addword));
                StartActivity(intent);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rlMain_Wordlist_Addword_Touch(object sender, TouchEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void etxtWord_Wordlist_Addword_FocusChange(object sender, FocusChangeEventArgs e)
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
        public void etxtMeaning_Wordlist_Addword_FocusChange(object sender, FocusChangeEventArgs e)
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
        #endregion

        #region Method
        /// <summary>
        /// Register new word
        /// </summary>
        /// <returns>true:success false: failure</returns>
        private bool Registerword()
        {
            string etxtWord = this.etxtWord.Text;
            string etxtMeaning = this.etxtMeaning.Text;
            if (string.IsNullOrEmpty(etxtWord))
            {
                var dlg = new Android.Support.V7.App.AlertDialog.Builder(this);
                dlg.SetTitle(Message.Enterword[Utility.language]);
                dlg.SetPositiveButton("OK", (sender, e) => { return; });
                dlg.Show();
                return false;
            }
            else
            {
                var xelm = XDocument.Load(Utility.WordListPath);
                var xmlcd = Utility.GetXElement(Utility.cd, xelm);
                xmlcd.Add(new XElement("Word", new XElement("Wordname", XmlConvert.EncodeLocalName(etxtWord)), new XElement("Wordmeaning", XmlConvert.EncodeLocalName(etxtMeaning)), new XElement("Tag", "00000"), new XElement("Memo", XmlConvert.EncodeLocalName(string.Empty))));
                xelm.Save(Utility.WordListPath);
                Finish();
                return true;
            }
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