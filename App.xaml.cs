using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace WorkWinLocker
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        protected override void OnExit(ExitEventArgs e)
        {
            ShowWindow(FindWindow("Shell_TrayWnd", ""), 1);
            base.OnExit(e);
        }

        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            ShowWindow(FindWindow("Shell_TrayWnd", ""), 1);
        }
    }
}
