using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataToolChain
{

    public class BootLoaderViewModel
    {
        public List<DropDownDisplay<Type>> Windows { get; } = new List<DropDownDisplay<Type>>();

        public Type SelectedType { get; set; }

        public BootLoaderViewModel()
        {
            Windows =
                Assembly.GetExecutingAssembly()    
                    .GetTypes()
                    .Where(t => t.IsPublic && t.GetConstructors().Any(c => c.GetParameters().Length == 0) && t.BaseType == typeof(Window))
                    .Select(w => new DropDownDisplay<Type>
                    {
                        DisplayText = w.Name,
                        Value = w
                    })
                    .OrderBy(a => a.DisplayText)
                    .ToList();
        }

        public void Launch()
        {
            var w = (Window)(SelectedType?.GetConstructor(Type.EmptyTypes)?.Invoke(null)); //.Show();

            w?.Show();
        }
    }

    /// <summary>
    /// Interaction logic for Bootstrapper.xaml
    /// </summary>
    public partial class Bootloader : Window
    {
        private BootLoaderViewModel _viewModel { get; } = new BootLoaderViewModel();

        public Bootloader()
        {
            InitializeComponent();

            DataContext = _viewModel;
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Launch();
        }
    }
}
