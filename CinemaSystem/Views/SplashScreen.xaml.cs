// Views/SplashScreen.xaml.cs
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CinemaSystem.Views
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                OpenLoginWindow();
            };
            timer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OpenLoginWindow();
            }
        }

        private void OpenLoginWindow()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}