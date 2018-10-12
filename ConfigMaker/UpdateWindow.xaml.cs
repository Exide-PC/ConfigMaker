using System;
using System.Diagnostics;
using System.Windows;
using Res = ConfigMaker.Properties.Resources;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        Uri freshVersionUrl = null;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ссылка на файл с информацией о последней версии
            string versionFileUrl = "http://blog.exideprod.com/configmaker-version/";

            System.Net.WebClient web = new System.Net.WebClient();

            web.DownloadStringCompleted += (_, arg) =>
            {
                // Обернем метод на случай, если файл по каким-то причинам не будет доступен
                try
                {
                    // Берем скачанный текст и делим его на 2 части до и после пробела
                    string versionText = arg.Result;
                    string[] parts = versionText.Split(' ');

                    // Часть до пробела - версия в формате 1.2.3.4
                    Version actualVersion = new Version(parts[0]);
                    // После пробела - ссылка на актуальную версию
                    this.freshVersionUrl = new Uri(parts[1], UriKind.Absolute);

                    Version currentVersion = System.Reflection.Assembly
                        .GetExecutingAssembly().GetName().Version;

                    // Если текущая версия не актуальна, то выводим сообщение
                    // и делаем кнопку обновления активной
                    tip1.Text = string.Format(Res.CurrentVersion_Format, currentVersion.ToString());
                    tip2.Text = string.Format(Res.ActualVersion_Format, actualVersion.ToString());

                    installButton.IsEnabled = currentVersion < actualVersion;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    // Если загрузка не удалась - выводим сообщения и пишем в лог
                    tip1.Text = Res.UpdateCheckFailed_Hint1;
                    tip2.Text = Res.UpdateCheckFailed_Hint2;

                    App.LogText($"Update check failed: {ex.InnerException.Message}");
                }
            };

            web.DownloadStringAsync(new Uri(versionFileUrl, UriKind.Absolute));
        }

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            // При нажатии на обновление - запускаем апдейтер с соответствующими параметрами
            //Process.Start("Updater.exe", $"{freshVersionUrl} ConfigMaker.exe");
            //Process.GetCurrentProcess().Kill();
        }
    }
}
