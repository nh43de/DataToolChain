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
    }
}
