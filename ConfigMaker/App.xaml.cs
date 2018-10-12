using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        public static void LogException(Exception ex)
        {
            LogText($"{ex.Message}\r\n{ex.StackTrace}\r\n\r\n");
        }

        public static void LogText(string text)
        {
            File.AppendAllText("Log.txt", text);
        }

        public static string GetShoftVersion()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            while (version.LastIndexOf(".0") == version.Length - 2 && version.Length > 1) // 1.2.3.0
                version = version.Substring(0, version.Length - 2);

            return version;
        }
    }
}
