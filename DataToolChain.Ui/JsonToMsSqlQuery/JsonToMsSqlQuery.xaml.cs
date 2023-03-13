using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DataToolChain.Ui.Extensions;
using Newtonsoft.Json.Linq;

namespace DataToolChain
{
    /// <summary>
    /// This turns a JSON object and flattens it into a MS SQL query to use for JSON_VALUE querying.
    /// </summary>
    public partial class JsonToMsSqlQuery : Window
    {
        public JsonToMsSqlQueryViewModel _viewModel { get; set; } = new JsonToMsSqlQueryViewModel();

        public JsonToMsSqlQuery()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class JsonToMsSqlQueryViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"{ 'some': { 'sort': 'of', 'json': true } }";

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _addCasts = true;


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
            get { return _stringOutput; }
            set
            {
                _stringOutput = value;
                OnPropertyChanged();
            }
        }

        public JsonToMsSqlQueryViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            try
            {
                var dd = JObject.Parse(StringInput);

                var o = dd.Flatten()
                    .Select(p =>
                    {
                        var jsonValStr = $"JSON_VALUE([value], '$.{p.Key}')";

                        var colAliasStr = $" AS [{p.Key}]";

                        if (_addCasts)
                        {
                            if (double.TryParse(p.Value.ToString(), out double d))
                                jsonValStr = $"CAST({jsonValStr} AS FLOAT)";
                        }

                        return $"{jsonValStr}{colAliasStr}";
                    });

                var selectStr = "SELECT \r\n" + string.Join(",\r\n", o).Indent();

                StringOutput = selectStr;
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

