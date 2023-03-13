using System;
using System.Collections.Generic;
using System.Linq;
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
using DataToolChain.RegexMaker;

namespace DataToolChain
{
    /// <summary>
    /// Interaction logic for RegexReplacer.xaml
    /// </summary>
    public partial class RegexReplacer : Window
    {
        private readonly RegexReplacerViewModel _viewModel = new RegexReplacerViewModel();

        public RegexReplacer()
        {
            InitializeComponent();

            DataContext = _viewModel;
        }
    }
}
