using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JUST;

namespace JustTransformPlayground.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class JustTransformPlayground : Window
    {
        public TestJustTransformViewModel _viewModel { get; set; } = new TestJustTransformViewModel();

        public JustTransformPlayground()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    public class TestJustTransformViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _transformerJson = @"{
  ""result"": {
    ""Open"": ""#valueof($.menu.popup.menuitem[?(@.value=='Open')].onclick)"",
    ""Close"": ""#valueof($.menu.popup.menuitem[?(@.value=='Close')].onclick)""
  }
}";

        private string _inputJson = @"{
  ""menu"": {
    ""popup"": {
      ""menuitem"": [{
          ""value"": ""Open"",
          ""onclick"": ""OpenDoc()""
        }, {
          ""value"": ""Close"",
          ""onclick"": ""CloseDoc()""
        }
      ]
    }
  }
}";

        public event PropertyChangedEventHandler PropertyChanged;

        public string TransformerJson
        {
            get => _transformerJson;
            set
            {
                _transformerJson = value;
                UpdateOutput();
            }
        }

        public string InputJson 
        {
            get => _inputJson;
            set
            {
                _inputJson = value;
                UpdateOutput();
            }
        }

        public string StringOutput
        {
            get => _stringOutput;
            set
            {
                _stringOutput = value;
                OnPropertyChanged();
            }
        }

        public TestJustTransformViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            try
            {
                var t = new JsonTransformer();

                StringOutput = t.Transform(_transformerJson, _inputJson).ToObject().ToJson(true);
            }
            catch (Exception e)
            {
                StringOutput = "Error occurred: " + e.Message;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
