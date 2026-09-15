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
using System.Net.NetworkInformation;

namespace DnsChanger
{
    public partial class MainWindow : Window
    {
        private readonly IDnsService _dnsService;
        private readonly ICustomDnsRepository _customDnsRepository;
        private readonly INetworkDiagnosticsService _diagnosticsService;
        private readonly IActivityLogRepository _activityLog;
        private readonly IPingService _pingService;
        private readonly ISpeedTestService _speedTestService;
        private readonly ObservableCollection<CustomDnsEntry> _customDnsEntries = new();
        private DispatcherTimer _messageTimer;
        public MainWindow(IDnsService dnsService, ICustomDnsRepository customDnsRepository, INetworkDiagnosticsService diagnosticsService,
            IActivityLogRepository activityLog, IPingService pingService, ISpeedTestService speedTestService)
        {
            InitializeComponent();
            _dnsService = dnsService;
            _customDnsRepository = customDnsRepository;
            _diagnosticsService = diagnosticsService;
            _activityLog = activityLog;
            _pingService = pingService;
            _speedTestService = speedTestService;

            CustomDnsItemsControl.ItemsSource = _customDnsEntries;
            LoadCustomDnsEntries();
            ApplySavedSettings();
            RefreshConnectionStatus();

            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

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
                CustomDialog.ShowWarning("برای اعمال تغییرات شبکه، برنامه رو با دسترسی Administrator اجرا کن.");
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
        protected override void OnClosed(EventArgs e)
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
            base.OnClosed(e);
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
            _activityLog.Add($"{provider.Name} DNS ست شد", $"{provider.Primary}, {provider.Secondary}");
            LoadHistory();
            RefreshConnectionStatus();
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
        private async void RefreshConnectionStatus()
        {
            var adapter = _dnsService.GetActiveAdapter();

            if (adapter == null)
            {
                ActiveAdapterText.Text = "—";
                CurrentDnsText.Text = "—";
                PublicIpText.Text = "—";
                ConnectionStatusText.Text = "قطع";
                ConnectionStatusText.Foreground = (Brush)FindResource("Danger");
                ConnectionStatusDot.Fill = (Brush)FindResource("Danger");
                return;
            }

            ActiveAdapterText.Text = adapter.Name;

            var dnsAddresses = adapter.GetIPProperties().DnsAddresses;
            CurrentDnsText.Text = dnsAddresses.Count > 0
                ? string.Join(", ", dnsAddresses)
                : "خودکار (DHCP)";

            ConnectionStatusText.Text = "متصل";
            ConnectionStatusText.Foreground = (Brush)FindResource("Success");
            ConnectionStatusDot.Fill = (Brush)FindResource("Success");

            PublicIpText.Text = "در حال گرفتن...";
            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                PublicIpText.Text = await client.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                PublicIpText.Text = "نامشخص";
            }
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
            _activityLog.Add("DNS به حالت خودکار (DHCP) بازنشانی شد");
            LoadHistory();
            RefreshConnectionStatus();
            ShowMessage("DNS Reset!", isSuccess: false);
        }
        private void AddCustomDnsButton_Click(object sender, RoutedEventArgs e)
        {
            string name = CustomNameBox.Text.Trim();
            string primary = CustomPrimaryBox.Text.Trim();
            string secondary = CustomSecondaryBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || !IPAddress.TryParse(primary, out _))
            {
                CustomDialog.ShowError("لطفاً یک نام و یک IP معتبر برای Primary DNS وارد کن.", "خطا");
                return;
            }
            if (string.IsNullOrWhiteSpace(secondary) && !IPAddress.TryParse(primary, out _))
            {
                CustomDialog.ShowError("Secondary DNS معتبر نیست.", "خطا");
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
            _activityLog.Add($"DNS «{entry.Name}» اضافه شد", $"{entry.Primary}, {entry.Secondary}");
            LoadHistory();
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
                _activityLog.Add($"DNS «{entry.Name}» حذف شد");
                LoadHistory();
            }
        }
        private async void CheckPingButton_Click(object sender, RoutedEventArgs e)
        {
            CheckPingButton.IsEnabled = false;

            var tasks = _customDnsEntries.Select(async entry =>
            {
                entry.PingText = "...";
                var ms = await _pingService.PingAsync(entry.Primary);
                entry.PingText = ms.HasValue ? $"{ms} ms" : "timeout";
            });
            await Task.WhenAll(tasks);

            CheckPingButton.IsEnabled = true;
        }
        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Dispatcher.Invoke(RefreshConnectionStatus);
        }
        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(RefreshConnectionStatus);
        }
        #endregion
        #region SpeedTest
        //private async void StartSpeedTestButton_Click(object sender, RoutedEventArgs e)
        //{
        //    StartSpeedTestButton.IsEnabled = false;
        //    DownloadSpeedText.Text = "—";
        //    UploadSpeedText.Text = "—";
        //    DownloadSpeedMBText.Text = "— MB/s";
        //    UploadSpeedMBText.Text = "— MB/s";
        //    PingResultText.Text = "—";
        //    DataCenterText.Text = "—";

        //    var progress = new Progress<string>(status => SpeedTestStatusText.Text = status);
        //    var result = await _speedTestService.RunTestAsync(progress);

        //    DownloadSpeedText.Text = result.DownloadMbps.ToString("0.0");
        //    UploadSpeedText.Text = result.UploadMbps.ToString("0.0");
        //    DownloadSpeedMBText.Text = $"{(result.DownloadMbps / 8):0.0} MB/s";
        //    UploadSpeedMBText.Text = $"{(result.UploadMbps / 8):0.0} MB/s";
        //    PingResultText.Text = result.PingMs.ToString();
        //    DataCenterText.Text = result.DataCenter;

        //    _activityLog.Add("تست سرعت اجرا شد",
        //$"دانلود: {result.DownloadMbps} Mbps, آپلود: {result.UploadMbps} Mbps, پینگ: {result.PingMs}ms");
        //    LoadHistory();

        //    StartSpeedTestButton.IsEnabled = true;
        //}

        //New: check this
        private async void StartSpeedTestButton_Click(object sender, RoutedEventArgs e)
        {
            StartSpeedTestButton.IsEnabled = false;
            DownloadSpeedText.Text = "—";
            UploadSpeedText.Text = "—";
            DownloadSpeedMBText.Text = "— MB/s";
            UploadSpeedMBText.Text = "— MB/s";
            PingResultText.Text = "—";
            DataCenterText.Text = "—";

            var progress = new Progress<SpeedTestProgress>(p =>
            {
                SpeedTestStatusText.Text = p.Phase;
                SpeedTestLiveNumber.Text = p.CurrentMbps.ToString("0.0");
                UpdateProgressRing(p.PercentComplete);
            });

            var result = await _speedTestService.RunTestAsync(progress);

            DownloadSpeedText.Text = result.DownloadMbps.ToString("0.0");
            UploadSpeedText.Text = result.UploadMbps.ToString("0.0");
            DownloadSpeedMBText.Text = $"{(result.DownloadMbps / 8):0.0} MB/s";
            UploadSpeedMBText.Text = $"{(result.UploadMbps / 8):0.0} MB/s";
            PingResultText.Text = result.PingMs.ToString();
            DataCenterText.Text = result.DataCenter;

            SpeedTestLiveNumber.Text = "0.0";
            SpeedTestStatusText.Text = "آماده";
            UpdateProgressRing(0);

            _activityLog.Add("تست سرعت اجرا شد",
                $"دانلود: {result.DownloadMbps} Mbps, آپلود: {result.UploadMbps} Mbps, پینگ: {result.PingMs}ms");
            LoadHistory();

            StartSpeedTestButton.IsEnabled = true;
        }
        private void UpdateProgressRing(double percent) // and check this
        {
            const double radius = 90;
            const double thickness = 10;
            double circumferenceInUnits = (2 * Math.PI * radius) / thickness;
            double dash = Math.Max(0.001, percent / 100.0 * circumferenceInUnits);
            double gap = Math.Max(0.001, circumferenceInUnits - dash);
            SpeedTestProgressRing.StrokeDashArray = new System.Windows.Media.DoubleCollection { dash, gap };
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

            bool overallOk = results.Count > 0 && results[^1].IsSuccess;
            _activityLog.Add("تشخیص و رفع خودکار اجرا شد", overallOk ? "نتیجه: موفق" : "نتیجه: مشکل حل نشد");
            LoadHistory();

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
            _activityLog.Add("DNS Cache پاک‌سازی شد");
            LoadHistory();
            ShowMessage("DNS Cache پاک شد!", isSuccess: true);
        }
        private void RestartAdapterButton_Click(object sender, RoutedEventArgs e)
        {
            AdminPermissionCheck();
            _dnsService.RestartActiveAdapter();
            _activityLog.Add("آداپتور شبکه ری‌استارت شد");
            LoadHistory();
            ShowMessage("آداپتور ری‌استارت شد!", isSuccess: true);
        }
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            //var confirm = MessageBox.Show("کل تاریخچه پاک بشه؟", "تایید", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (confirm == MessageBoxResult.Yes)
            //{
            //    _activityLog.DeleteAll();
            //    LoadHistory();
            //}
            if (CustomDialog.Confirm("کل تاریخچه پاک بشه؟"))
            {
                _activityLog.DeleteAll();
                LoadHistory();
            }
        }
        #endregion
        #region Wifi
        private void ConnectWifi_Click(object sender, RoutedEventArgs e)
        {
            CustomDialog.ShowInfo("اسکن و اتصال Wi-Fi واقعی رو بعداً با هم می‌سازیم.");
        }
        #endregion
        #region History
        private void LoadHistory()
        {
            HistoryListBox.ItemsSource = _activityLog.GetRecent();
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
            Application.Current.Resources["BgDark"] = new SolidColorBrush(Color.FromRgb(0x0F, 0x11, 0x17));
            Application.Current.Resources["BgCard"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1D, 0x29));
            Application.Current.Resources["BgCardHover"] = new SolidColorBrush(Color.FromRgb(0x23, 0x27, 0x39));
            Application.Current.Resources["BgInput"] = new SolidColorBrush(Color.FromRgb(0x12, 0x14, 0x1C));
            Application.Current.Resources["BgChip"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
            Application.Current.Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2E, 0x3F));
            Application.Current.Resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(0x56, 0x5B, 0x6B));
            Application.Current.Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0xF1, 0xF2, 0xF6));
            Application.Current.Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x9E));
            Application.Current.Resources["SidebarBg"] = new SolidColorBrush(Color.FromRgb(0x15, 0x18, 0x22));

            DarkModeButton.Tag = "Selected";
            LightModeButton.Tag = null;

            var settings = AppSettings.Load();
            settings.IsDarkMode = true;
            settings.Save();
        }
        private void LightModeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources["BgDark"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF6, 0xFA));
            Application.Current.Resources["BgCard"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["BgCardHover"] = new SolidColorBrush(Color.FromRgb(0xEC, 0xED, 0xF2));
            Application.Current.Resources["BgInput"] = new SolidColorBrush(Color.FromRgb(0xF0, 0xF1, 0xF5));
            Application.Current.Resources["BgChip"] = new SolidColorBrush(Color.FromRgb(0xE2, 0xE5, 0xEC));
            Application.Current.Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xE0, 0xE6));
            Application.Current.Resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(0xA0, 0xA5, 0xB0));
            Application.Current.Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1D, 0x29));
            Application.Current.Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x6B, 0x70, 0x80));
            Application.Current.Resources["SidebarBg"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));

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
            Application.Current.Resources["AccentBlue"] = new SolidColorBrush(gradient.GradientStops[0].Color);
            Application.Current.Resources["AccentGradient"] = gradient.Clone();

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
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedLang) return;

            LangFaButton.Tag = null;
            LangEnButton.Tag = null;
            clickedLang.Tag = "Selected";

            CustomDialog.ShowInfo("ترجمه‌ی کامل رابط کاربری رو قدم بعد با هم پیاده می‌کنیم.");
        }
        #endregion
    }
}