using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DataToolChain.DbStringer
{
    public class DbStringerViewModel : INotifyPropertyChanged
    {
        private string _inputText;
        //public const string DefaultFileName = "dbstringerconfig.json";


        public string InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                UpdateOutputText();
                OnPropertyChanged();
            }
        }

        public void UpdateOutputText()
        {
            if (SelectedRegexReplacement != null)
                OutputText = RegexReplacement.RegexReplace(SelectedRegexReplacement, _inputText);

            OnPropertyChanged(nameof(OutputText));
        }

        public string OutputText { get; private set; }

        public RegexReplacement SelectedRegexReplacement => RegexReplacers?.FirstOrDefault(p => p.IsChecked)?.RegexReplacement;

        public RegexReplacerCollection RegexReplacers { get; set; }

        public DbStringerViewModel()
        {
            RegexReplacers = RegexReplacerCollection.DefaultReplacerCollection;

            //if (!File.Exists(DefaultFileName))
            //{
            //    using (var a = new StreamWriter(File.Create(DefaultFileName)))
            //    {
            //        RegexReplacers = RegexReplacerCollection.DefaultReplacerCollection;
            //        a.Write(RegexReplacers.ToJson(true));
            //        a.Close();
            //    }
            //}
            //else
            //{
            //    using (var a = new StreamReader(File.OpenRead(DefaultFileName)))
            //    {
            //        var t = a.ReadToEnd();
            //        try
            //        {
            //            RegexReplacers = t.ToObject<RegexReplacerCollection>();
            //            a.Close();
            //        }
            //        catch (Exception ex)
            //        {
            //            RegexReplacers = RegexReplacerCollection.DefaultReplacerCollection;
            //            RegexReplacers.FailString = t;
            //        }

            //    }
            //}
        }

        public void SaveChanges()
        {
            //File.WriteAllText(DefaultFileName, RegexReplacers.ToJson(true));
        }





        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}