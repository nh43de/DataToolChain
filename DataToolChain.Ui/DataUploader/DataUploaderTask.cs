using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DataToolChain
{
    public class DataUploaderTask : INotifyPropertyChanged
    {
        private string _statusMessage;
        private bool _success;
        private int _rowsCopied;
        public string FilePath { get; set; }

        [JsonIgnore]
        public bool Success
        {
            get { return _success; }
            set
            {
                _success = value; 
                OnPropertyChanged();
            }
        }

        public string DestinationTable { get; set; }

        [JsonIgnore]
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}