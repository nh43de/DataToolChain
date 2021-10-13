using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataPowerTools.Extensions;
using DataToolChain.Ui.Extensions;
using JustTransformPlayground.Ui;

namespace DataToolChain
{
    /// <summary>
    /// Interaction logic for RegexMatcher.xaml
    /// </summary>
    public partial class JsonFormatter : Window
    {
        public JsonFormatterViewModel _viewModel { get; set; } = new JsonFormatterViewModel();

        public JsonFormatter()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    public class JsonFormatterViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"{ 'some': { 'sort': 'of', 'json': true } }";

        public event PropertyChangedEventHandler PropertyChanged;
        

        public string StringInput
        {
            get { return _stringInput; }
            set
            {
                _stringInput = value;
                UpdateOutput();
            }
        }

        public string StringOutput
        {
            get { return _stringOutput.JoinStr(); }
            set
            {
                _stringOutput = value;
                OnPropertyChanged();
            }
        }

        public JsonFormatterViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            try
            {
                var o = StringInput.ToObject<object>();
                
                StringOutput = o.ToJson(true);
            }
            catch (Exception)
            {
                StringOutput = "Error in Json.";
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

