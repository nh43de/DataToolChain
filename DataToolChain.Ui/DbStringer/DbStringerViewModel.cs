using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace DataToolChain.DbStringer
{
    public class DbStringerViewModel : INotifyPropertyChanged
    {
        private string _inputText = @"List input 1
List input 2
List input 3
List input 4";

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

        public RegexReplacement SelectedRegexReplacement => RegexReplacers?.SourceCollection.Cast<SelectableRegexReplacement>().FirstOrDefault(p => p.IsChecked)?.RegexReplacement;

        public CollectionView RegexReplacers { get; set; }

        public DbStringerViewModel()
        {
            var collection = RegexReplacerCollection.DefaultReplacerCollection;
            collection[2].IsChecked = true;

            RegexReplacers = (CollectionView)CollectionViewSource.GetDefaultView(collection);
            RegexReplacers.Filter = RegexReplacerFilter;

            UpdateOutputText();


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
        
        public string FilterText { get; set; }

        bool RegexReplacerFilter(object item)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;
            else
            {
                return item is SelectableRegexReplacement dd && dd.DisplayText.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            }
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