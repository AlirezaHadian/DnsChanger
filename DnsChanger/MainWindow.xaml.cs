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
        private readonly INetworkDiagnosticsService _diagnosticsService;
        private readonly ObservableCollection<CustomDnsEntry> _customDnsEntries = new();
        private DispatcherTimer _messageTimer;
        public MainWindow(IDnsService dnsService, ICustomDnsRepository customDnsRepository, INetworkDiagnosticsService diagnosticsService)
        {
            InitializeComponent();
            _dnsService = dnsService;
            _customDnsRepository = customDnsRepository;
            _diagnosticsService = diagnosticsService;

            CustomDnsItemsControl.ItemsSource = _customDnsEntries;
            LoadCustomDnsEntries();
            ApplySavedSettings();

            _messageTimer = new DispatcherTimer();
            _messageTimer.Interval = TimeSpan.FromSeconds(5);
            _messageTimer.Tick += MessageTimer_Tick;
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
        private void ApplySavedSettings()
        {
            var settings = AppSettings.Load();

            if (settings.IsDarkMode)
                DarkModeButton_Click(this, null);
            else
                LightModeButton_Click(this, null);

            if (FindName(settings.AccentName) is Button savedSwatch)
            {
                AccentSwatch_Click(savedSwatch, null);
            }
        }
        #region DNS 
        /// <summary>
        /// Function
        /// </summary>
        private void LoadCustomDnsEntries()
        {
            _customDnsEntries.Clear();
            foreach (var entry in _customDnsRepository.GetAllDns())
            {
                _customDnsEntries.Add(entry);
            }
        }
        private void ApplyProviderAndNotify(DnsProvider provider)
        {
            _dnsService.SetDns(provider);
            ShowMessage($"{provider.Name} DNS Set!", isSuccess: true);
        }
        private void ShowMessage(string text, bool isSuccess)
        {
            MessageTextBlock.Text = text;
            MessageBorder.Background = new SolidColorBrush(isSuccess
                ? Color.FromRgb(0x22, 0xC5, 0x5E)
                : Color.FromRgb(0xEF, 0x44, 0x44));
            MessageBorder.Visibility = Visibility.Visible;

            _messageTimer.Stop();
            _messageTimer.Start();
        }
        /// <summary>
        /// Events
        /// </summary>
        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            MessageBorder.Visibility = Visibility.Hidden;
            _messageTimer.Stop();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            _dnsService.UnsetDns();

            ShowMessage("DNS Reset!", isSuccess: false);
        }
        private void AddCustomDnsButton_Click(object sender, RoutedEventArgs e)
        {
            string name = CustomNameBox.Text.Trim();
            string primary = CustomPrimaryBox.Text.Trim();
            string secondary = CustomSecondaryBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || !IPAddress.TryParse(primary, out _))
            {
                MessageBox.Show("Please enter a valid name and a valid IP for Primary DNS.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(secondary) && !IPAddress.TryParse(primary, out _))
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
        private void CheckPingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تست Ping واقعی رو قدم بعد با هم می‌زنیم.");
        }
        #endregion
        #region SpeedTest
        private void StartSpeedTestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تست سرعت واقعی رو یه قدم جدا پیاده می‌کنیم.");
        }
        #endregion
        #region Troubleshoot
        private async void RunDiagnosticsButton_Click(object sender, RoutedEventArgs e)
        {
            RunDiagnosticsButton.IsEnabled = false;
            RunDiagnosticsButton.Content = "در حال بررسی...";
            DiagnosticsResultsItemsControl.ItemsSource = null;

            var results = await _diagnosticsService.RunDiagnosticsAsync();
            DiagnosticsResultsItemsControl.ItemsSource = results;

            RunDiagnosticsButton.IsEnabled = true;
            RunDiagnosticsButton.Content = "شروع بررسی";
        }
        private async void FlushDnsButton_Click(object sender, RoutedEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("ipconfig", "/flushdns")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
            ShowMessage("DNS Cache پاک شد!", isSuccess: true);
        }

        private void RestartAdapterButton_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            _dnsService.RestartActiveAdapter();
            ShowMessage("آداپتور ری‌استارت شد!", isSuccess: true);
        }
        #endregion
        #region Wifi
        private void ConnectWifi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("اسکن و اتصال Wi-Fi واقعی رو بعدا با هم می‌سازیم.");
        }
        #endregion
        #region Settings
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        private void NavItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedButton) return;
            string targetPage = clickedButton.Tag?.ToString();

            DnsPagePanel.Visibility = Visibility.Collapsed;
            HistoryPagePanel.Visibility = Visibility.Collapsed;
            SpeedTestPagePanel.Visibility = Visibility.Collapsed;
            TroubleshootPagePanel.Visibility = Visibility.Collapsed;
            WifiPagePanel.Visibility = Visibility.Collapsed;
            SettingsPagePanel.Visibility = Visibility.Collapsed;

            // نمایش فقط صفحه‌ی انتخاب‌شده
            switch (targetPage)
            {
                case "Dns": DnsPagePanel.Visibility = Visibility.Visible; break;
                case "History": HistoryPagePanel.Visibility = Visibility.Visible; break;
                case "SpeedTest": SpeedTestPagePanel.Visibility = Visibility.Visible; break;
                case "Troubleshoot": TroubleshootPagePanel.Visibility = Visibility.Visible; break;
                case "Wifi": WifiPagePanel.Visibility = Visibility.Visible; break;
                case "Settings": SettingsPagePanel.Visibility = Visibility.Visible; break;
            }

            // برگردوندن استایل عادی به همه‌ی دکمه‌های Sidebar
            NavDnsButton.Style = (Style)FindResource("SidebarButtonStyle");
            NavHistoryButton.Style = (Style)FindResource("SidebarButtonStyle");
            NavSpeedTestButton.Style = (Style)FindResource("SidebarButtonStyle");
            NavTroubleshootButton.Style = (Style)FindResource("SidebarButtonStyle");
            NavWifiButton.Style = (Style)FindResource("SidebarButtonStyle");
            NavSettingsButton.Style = (Style)FindResource("SidebarButtonStyle");

            // دادن استایل فعال به دکمه‌ای که کلیک شده
            clickedButton.Style = (Style)FindResource("SidebarButtonActiveStyle");
        }
        private void DarkModeButton_Click(object sender, RoutedEventArgs e)
        {
            Resources["BgDark"] = new SolidColorBrush(Color.FromRgb(0x0F, 0x11, 0x17));
            Resources["BgCard"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1D, 0x29));
            Resources["BgCardHover"] = new SolidColorBrush(Color.FromRgb(0x23, 0x27, 0x39));
            Resources["BgInput"] = new SolidColorBrush(Color.FromRgb(0x12, 0x14, 0x1C));
            Resources["BgChip"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
            Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2E, 0x3F));
            Resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(0x56, 0x5B, 0x6B));
            Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0xF1, 0xF2, 0xF6));
            Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x9E));
            Resources["SidebarBg"] = new SolidColorBrush(Color.FromRgb(0x15, 0x18, 0x22));

            DarkModeButton.Tag = "Selected";
            LightModeButton.Tag = null;

            var settings = AppSettings.Load();
            settings.IsDarkMode = true;
            settings.Save();
        }
        private void LightModeButton_Click(object sender, RoutedEventArgs e)
        {
            Resources["BgDark"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF6, 0xFA));
            Resources["BgCard"] = new SolidColorBrush(Colors.White);
            Resources["BgCardHover"] = new SolidColorBrush(Color.FromRgb(0xEC, 0xED, 0xF2));
            Resources["BgInput"] = new SolidColorBrush(Color.FromRgb(0xF0, 0xF1, 0xF5));
            Resources["BgChip"] = new SolidColorBrush(Color.FromRgb(0xE2, 0xE5, 0xEC));
            Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xE0, 0xE6));
            Resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(0xA0, 0xA5, 0xB0));
            Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1D, 0x29));
            Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x6B, 0x70, 0x80));
            Resources["SidebarBg"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));

            LightModeButton.Tag = "Selected";
            DarkModeButton.Tag = null;

            var settings = AppSettings.Load();
            settings.IsDarkMode = false;
            settings.Save();
        }
        private void AccentSwatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedSwatch) return;


            var gradient = (LinearGradientBrush)clickedSwatch.Background;
            Resources["AccentBlue"] = new SolidColorBrush(gradient.GradientStops[0].Color);
            Resources["AccentGradient"] = gradient.Clone();

            AccentSwatchBluePurple.Tag = null;
            AccentSwatchGreen.Tag = null;
            AccentSwatchOrange.Tag = null;
            AccentSwatchPink.Tag = null;
            AccentSwatchCyan.Tag = null;
            AccentSwatchRed.Tag = null;
            AccentSwatchSlate.Tag = null;
            clickedSwatch.Tag = "Selected";

            var settings = AppSettings.Load();
            settings.AccentName = clickedSwatch.Name;
            settings.Save();
        }
        private static Color Darken(Color c, double factor) =>
            Color.FromRgb((byte)(c.R * factor), (byte)(c.G * factor), (byte)(c.B * factor));
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedLang) return;

            LangFaButton.Tag = null;
            LangEnButton.Tag = null;
            clickedLang.Tag = "Selected";

            MessageBox.Show("ترجمه‌ی کامل رابط کاربری رو قدم بعد با هم پیاده می‌کنیم.");
        }
        #endregion
    }
}