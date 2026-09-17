using System.Windows;
using System.Windows.Media;

namespace DnsChanger
{
    public enum DialogType { Success, Error, Warning, Question }

    public partial class CustomDialog : Window
    {
        public bool Result { get; private set; }

        private CustomDialog(string title, string message, DialogType type, bool showCancel)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;

            // انتخاب آیکون و رنگ بر اساس نوع پیام
            Brush circleColor;
            switch (type)
            {
                case DialogType.Success:
                    SuccessIcon.Visibility = Visibility.Visible;
                    circleColor = (Brush)FindResource("Success");
                    break;
                case DialogType.Error:
                    ErrorIcon.Visibility = Visibility.Visible;
                    circleColor = (Brush)FindResource("Danger");
                    break;
                case DialogType.Warning:
                    WarningIcon.Visibility = Visibility.Visible;
                    circleColor = new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)); // نارنجی
                    break;
                default: // Question
                    QuestionIcon.Visibility = Visibility.Visible;
                    circleColor = (Brush)FindResource("AccentGradient");
                    break;
            }
            IconCircle.Background = circleColor;

            if (showCancel)
            {
                SecondaryButton.Visibility = Visibility.Visible;
                PrimaryButton.Content = "بله";
                SecondaryButton.Content = "خیر";
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
        }

        // --- متدهای استاتیک ساده برای استفاده‌ی راحت، شبیه MessageBox.Show ---

        public static void ShowInfo(string message, string title = "توجه")
        {
            var dialog = new CustomDialog(title, message, DialogType.Success, showCancel: false);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        public static void ShowError(string message, string title = "خطا")
        {
            var dialog = new CustomDialog(title, message, DialogType.Error, showCancel: false);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        public static void ShowWarning(string message, string title = "هشدار")
        {
            var dialog = new CustomDialog(title, message, DialogType.Warning, showCancel: false);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        public static bool Confirm(string message, string title = "تایید")
        {
            var dialog = new CustomDialog(title, message, DialogType.Question, showCancel: true);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return dialog.Result;
        }

        public static string PromptPassword(string ssid)
        {
            var dialog = new CustomDialog($"اتصال به {ssid}","رمز عبور شبکه را وارد کن", DialogType.Question, true);
            dialog.PasswordInput.Visibility = Visibility.Visible;
            dialog.PrimaryButton.Content = "اتصال";
            dialog.SecondaryButton.Content = "انصراف";
            dialog.Owner = Application.Current.MainWindow;
            
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.PasswordInput.Password : null;
        }
    }
}