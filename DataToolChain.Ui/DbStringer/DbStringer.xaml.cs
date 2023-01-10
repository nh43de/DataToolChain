using System.Windows;

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
            Clipboard.SetText(_viewModel.OutputText);
        }
    }
}
