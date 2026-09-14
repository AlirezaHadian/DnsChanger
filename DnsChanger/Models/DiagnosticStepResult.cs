using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DnsChanger.Models
{
    public class DiagnosticStepResult
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    
        public Brush StatusColor => IsSuccess
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
    }
}
