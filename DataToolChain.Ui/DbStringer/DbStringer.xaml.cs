using System;
using System.Windows;
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
            _viewModel.FilterText  = txtFilter.Text;
            _viewModel.RegexReplacers.Refresh();
        }

        private void SelectedRegexChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateOutputText();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.InputText = _viewModel.OutputText;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_viewModel.InputText);
        }
    }
}
