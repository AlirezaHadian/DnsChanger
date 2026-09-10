using DnsChanger.Models;
using DnsChanger.Services;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Net;
using DnsChanger.Repository;

namespace DnsChanger
{
    public partial class MainWindow : Window
    {
        private readonly IDnsService _dnsService;
        private readonly ICustomDnsRepository _customDnsRepository;
        private readonly ObservableCollection<CustomDnsEntry> _customDnsEntries = new ();
        private DispatcherTimer _messageTimer;
        public MainWindow(IDnsService dnsService, ICustomDnsRepository customDnsRepository)
        {
            InitializeComponent();
            
            _dnsService = dnsService;
            _customDnsRepository = customDnsRepository;

            CustomDnsItemsControl.ItemsSource = _customDnsEntries;
            LoadCustomDnsEntries();

            _messageTimer = new DispatcherTimer();
            _messageTimer.Interval = TimeSpan.FromSeconds(5);
            _messageTimer.Tick += MessageTimer_Tick;
        }
        private void LoadCustomDnsEntries()
        {
            _customDnsEntries.Clear();
            foreach (var entry in _customDnsRepository.GetAllDns())
            {
                _customDnsEntries.Add(entry);
            }
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

            MessageTextBlock.Text = "DNS Reset!";
            MessageTextBlock.Visibility = Visibility.Visible;
            MessageTextBlock.Background = new SolidColorBrush(Colors.Red);
            _messageTimer.Stop();
            _messageTimer.Start();
        }
        private void AddCustomDnsButton_Click(object sender, RoutedEventArgs e)
        {
            string name = CustomNameBox.Text.Trim();
            string primary = CustomPrimaryBox.Text.Trim();
            string secondary = CustomSecondaryBox.Text.Trim();

            if(string.IsNullOrEmpty(name) || !IPAddress.TryParse(primary, out _))
            {
                MessageBox.Show("Please enter a valid name and a valid IP for Primary DNS.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(string.IsNullOrWhiteSpace(secondary) && !IPAddress.TryParse(primary, out _))
            {
                MessageBox.Show("Secondary DNS is not valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var entry = new CustomDnsEntry
            {
                Name = name,
                Primary = primary,
                Secondary = string.IsNullOrWhiteSpace(secondary) ? null : secondary,
                CreatedAt = DateTime.Now
            };

            _customDnsRepository.Add(entry);
            LoadCustomDnsEntries();

            CustomNameBox.Clear();
            CustomPrimaryBox.Clear();
            CustomSecondaryBox.Clear();

            AdminPermissionCheck();
            ApplyProviderAndNotify(new DnsProvider { Name = entry.Name, Primary = entry.Primary, Secondary = entry.Secondary });
        }
        private void ApplyCustomDns_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is CustomDnsEntry entry)
            {
                AdminPermissionCheck();
                ApplyProviderAndNotify(new DnsProvider { Name = entry.Name, Primary = entry.Primary, Secondary = entry.Secondary });
            }
        }

        private void DeleteCustomDns_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is CustomDnsEntry entry)
            {
                _customDnsRepository.Delete(entry.Id);
                LoadCustomDnsEntries();
            }
        }

        private void ApplyProviderAndNotify(DnsProvider provider)
        {
            _dnsService.SetDns(provider);

            MessageTextBlock.Text = $"{provider.Name} DNS Set!";
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