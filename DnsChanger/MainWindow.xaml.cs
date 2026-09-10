using DnsChanger.Models;
using DnsChanger.Services;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace DnsChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDnsService _dnsService;
        private DispatcherTimer _messageTimer;
        public MainWindow(IDnsService dnsService)
        {
            InitializeComponent();
            _dnsService = dnsService;
            _messageTimer = new DispatcherTimer();
            _messageTimer.Interval = TimeSpan.FromSeconds(5);
            _messageTimer.Tick += MessageTimer_Tick;
        }
        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Hidden;
            _messageTimer.Stop();
        }

        private void Shecan_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            ApplyProviderAndNotify(new DnsProvider { Name = "Shecan", Primary = "178.22.122.100", Secondary = "185.51.200.2" });
        }
        private void Bogzar_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            ApplyProviderAndNotify(new DnsProvider { Name = "Bogzar", Primary = "185.55.226.26", Secondary = "185.55.225.25" });
        }
        private void HostIran_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            ApplyProviderAndNotify(new DnsProvider { Name = "HostIran", Primary = "172.29.0.100", Secondary = "172.29.2.100" });
        }
        private void _403_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            ApplyProviderAndNotify(new DnsProvider { Name = "403", Primary = "10.202.10.202", Secondary = "10.202.10.102" });
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            _dnsService.UnsetDns();

            MessageTextBlock.Content = "DNS Reset!";
            MessageTextBlock.Visibility = Visibility.Visible;
            MessageTextBlock.Background = new SolidColorBrush(Colors.Red);
            _messageTimer.Stop();
            _messageTimer.Start();
        }

        private void ApplyProviderAndNotify(DnsProvider provider)
        {
            _dnsService.SetDns(provider);

            MessageTextBlock.Content = $"{provider.Name} DNS Set!";
            MessageTextBlock.Visibility = Visibility.Visible;
            MessageTextBlock.Background = new SolidColorBrush(Colors.Green);
            _messageTimer.Stop();
            _messageTimer.Start();
        }

        
        public static void AdminPermissionCheck()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                MessageBox.Show("Please run as Administrator");
                return;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}