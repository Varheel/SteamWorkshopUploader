using Steamworks;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SWUploaderNet35
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        private const string AppIdFile = "AppID.txt";
        private const string FileError = "Make sure AppID.txt contains a valid number.";
        private const string ConnectionError = "Make sure Steam is running and your App ID is correct.";

        private readonly NavigationWindow _window;

        public StartPage()
        {
            InitializeComponent();

            _window = (NavigationWindow)Application.Current.MainWindow;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string error = null;

            // Read the game's App ID from the included text file
            try
            {
                foreach (string line in File.ReadAllLines(AppIdFile))
                {
                    // Split on whitespace (the default for Split) and take the first element (Split never returns an empty array)
                    string first = line.Split()[0];

                    if (uint.TryParse(first, out uint appId))
                    {
                        App.AppId = new AppId_t(appId);
                        // Steam will look for the App ID in env vars during initialization
                        Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());
                        Environment.SetEnvironmentVariable("SteamGameId", appId.ToString());
                        break;
                    }
                }

                // Did we find a number?
                if (App.AppId.m_AppId == 0)
                {
                    error = FileError;
                }
            }
            catch (Exception)
            {
                error = FileError;
            }

            // Initialize the Steam API
            if (error == null)
            {
                try
                {
                    if (SteamAPI.Init())
                    {
                        App.Running = true;
                        ThreadPool.QueueUserWorkItem(RunCallbacks);
                    }
                    else
                    {
                        error = ConnectionError;
                    }
                }
                catch
                {
                    error = ConnectionError;
                }
            }

            // If there was an error, show a message and quit
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            else
            {
                _window.Navigate(new MainPage());
            }
        }

        /// <summary>
        /// Runs Steam callbacks repeatedly until the program exits.
        /// </summary>
        private static void RunCallbacks(object state)
        {
            while (App.Running)
            {
                Application.Current.Dispatcher.Invoke(new Action(SteamAPI.RunCallbacks));
                Thread.Sleep(100); // Every 100 ms, or 10 times per second
            }
        }
    }
}
