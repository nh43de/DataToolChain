using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using DataToolChain.Ui.Annotations;

namespace DataToolChain
{

    public class BootLoaderViewModel : INotifyPropertyChanged
    {
        private Type _selectedType;

        public List<DropDownDisplay<Type>> Windows { get; }

        public Type SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
            }
        }

        public BootLoaderViewModel()
        {
            Windows =
                Assembly.GetExecutingAssembly()    
                    .GetTypes()
                    .Where(t => t.IsPublic && t.GetConstructors().Any(c => c.GetParameters().Length == 0) && t.BaseType == typeof(Window))
                    .Where(t => t.Name != nameof(Bootloader))
                    .Select(w => new DropDownDisplay<Type>
                    {
                        DisplayText = w.Name,
                        Value = w
                    })
                    .OrderBy(a => a.DisplayText)
                    .ToList();

            SelectedType = Windows[3].Value;
        }

        public void Launch()
        {
            var w = (Window)(SelectedType?.GetConstructor(Type.EmptyTypes)?.Invoke(null)); //.Show();

            w?.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
