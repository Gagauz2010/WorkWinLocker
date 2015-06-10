using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace WorkWinLocker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherLocker, dispatcherWorker, dispatcherMessage, dispatcherKiller;
        int total;
        static public List<string> BlockedProcessNames = new List<string>();

        #region WinApi
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            BlockProcess("Диспетчер задач");
        }

        private void BlockProcess(string ProcessName)
        {
            BlockedProcessNames.Add(ProcessName);
        }

        private void startLocker()
        {
            Show();
            Topmost = true;
            WindowState = WindowState.Maximized;
            ShowInTaskbar = false;
            BackGround.RadiusX = 0;
            BackGround.RadiusY = 0;
            this.Closing += Window_Closing;

            Locking(SW_HIDE);

            try
            {
                dispatcherWorker.Stop();
            }
            catch { }
            finally
            {
                dispatcherMessage = new DispatcherTimer();
                dispatcherMessage.Interval = new TimeSpan(0, 0, 1);
                dispatcherMessage.Tick += dispatcherMessage_Tick;
                dispatcherMessage.IsEnabled = true;

                dispatcherKiller = new DispatcherTimer();
                dispatcherKiller.Interval = new TimeSpan(10);
                dispatcherKiller.Tick += dispatcherKiller_Tick;
                dispatcherKiller.IsEnabled = true;

                dispatcherLocker = new DispatcherTimer();
                dispatcherLocker.Interval = new TimeSpan(0, 5, 0);
                dispatcherLocker.Tick += dispatcherLocker_Tick;
                dispatcherLocker.Start();
                dispatcherMessage.Start();
                Message.Text = String.Format("{0}:{1}:{2} until unlocking\nPlease take rest", dispatcherLocker.Interval.Hours.ToString("00"), dispatcherLocker.Interval.Minutes.ToString("00"), dispatcherLocker.Interval.Seconds.ToString("00"));
                total = (int)dispatcherLocker.Interval.TotalSeconds;
            }
        }

        private void dispatcherKiller_Tick(object sender, EventArgs e)
        {
            Process[] Processes = Process.GetProcesses();
            try
            {
                foreach (Process Proc in Processes)
                    if (BlockedProcessNames.IndexOf(Proc.MainWindowTitle) > -1)
                        Proc.Kill();
            }
            catch { }
        }

        private void dispatcherMessage_Tick(object sender, EventArgs e)
        {
            total--;
            if (Message.Text.Contains(":"))
                Message.Text = String.Format("{0}.{1}.{2} until unlocking\nPlease take rest", (total / 3600).ToString("00"), (total / 60).ToString("00"), (total % 60).ToString("00"));
            else
                Message.Text = String.Format("{0}:{1}:{2} until unlocking\nPlease take rest", (total / 3600).ToString("00"), (total / 60).ToString("00"), (total % 60).ToString("00")); 
        }

        private void startWorker()
        {
            Hide();

            Locking(SW_SHOW);

            try
            {
                dispatcherLocker.Stop();
                dispatcherMessage.Stop();
                dispatcherKiller.Stop();
            }
            catch { }
            finally 
            {
                dispatcherWorker = new DispatcherTimer();
                dispatcherWorker.Interval = new TimeSpan(0, 25, 00);
                dispatcherWorker.Tick += dispatcherWorker_Tick;
                dispatcherWorker.Start();
                Message.Text = String.Empty;
            }
        }

        void dispatcherWorker_Tick(object sender, EventArgs e)
        {
            startLocker();
        }

        void dispatcherLocker_Tick(object sender, EventArgs e)
        {
            startWorker();
        }

        private void Locking(int sw_do)
        {
            ShowWindow(FindWindow("Shell_TrayWnd", ""), sw_do);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Locking(SW_SHOW);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                btnExit.Visibility = Visibility.Visible;
                btnContinue.Visibility = Visibility.Visible;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            startWorker();
            //startLocker();
            btnStart.Visibility = Visibility.Hidden;
            Close.Visibility = Visibility.Hidden;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            startWorker();
        }
    }
}
