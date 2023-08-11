using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataToolChain.DbStringer
{
    /// <summary>
    /// Interaction logic for DbStringer.xaml
    /// </summary>
    public partial class DbStringer : Window
    {
        DbStringerViewModel _viewModel = new DbStringerViewModel();
       
        
        public DbStringer()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private bool RegexReplacerFilter(object item)
        {
            if (string.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
            {
                return item is SelectableRegexReplacement dd && dd.DisplayText.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase);
            }
        }


        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _viewModel.UpdateFilterText(txtFilter.Text);
        }

        //private void SelectedRegexChanged(object parameter, RoutedEventArgs e)
        //{
        //    //var dd = (RadioButton)parameter;
            
        //    if (parameter is SelectableRegexReplacement selectedReplacer)
        //    {
        //        // Handle the selection change, update the IsChecked property, and apply filtering as needed.
        //        foreach (SelectableRegexReplacement replacer in _viewModel.RegexReplacers.SourceCollection)
        //        {
        //            replacer.IsChecked = replacer == selectedReplacer;
        //        }
        //        //YourCollectionView.Refresh(); // Refresh the CollectionView to apply filtering.
        //    }

        //    _viewModel.UpdateOutputText();
        //}

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.InputText = _viewModel.OutputText;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_viewModel.InputText);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            txtFilter.Text = null;
            _viewModel.UpdateFilterText(null);
        }
    }
}
