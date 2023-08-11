using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

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

        public ICommand RadioCheckedCommand { get; } 
        
        // Command handler
        private void OnRadioChecked(object parameter)
        {
            if (parameter is SelectableRegexReplacement selectedReplacer)
            {
                // Handle the selection change, update the IsChecked property, and apply filtering as needed.
                foreach (SelectableRegexReplacement replacer in RegexReplacers.SourceCollection)
                {
                    replacer.IsChecked = replacer == selectedReplacer;
                }

                RegexReplacers.Refresh(); // Refresh the CollectionView to apply filtering.

                UpdateOutputText();
            }
        }

        public DbStringerViewModel()
        {
            RadioCheckedCommand = new RelayCommand(OnRadioChecked);

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

        public void UpdateFilterText(string input)
        {
            FilterText = input;
            RegexReplacers.Refresh();
        }

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

