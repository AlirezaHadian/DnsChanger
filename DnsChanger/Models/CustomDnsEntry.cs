using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace DnsChanger.Models
{
    public class CustomDnsEntry : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public DateTime CreatedAt { get; set; }

        private string _pingText = "- ms";
        public string PingText
        {
            get => _pingText;
            set
            {
                _pingText = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PingText)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
