using Steamworks;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace IASWorkshopLegacy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The number that identifies the game on the Steam Workshop.
        /// </summary>
        public static AppId_t AppId { get; private set; }

        private const string AppIdFile = "AppID.txt";
        private const string FileError = "Make sure AppID.txt contains a valid number.";
        private const string ConnectionError = "Make sure Steam is running and your App ID is correct.";

        private static bool _running = true;

        /// <summary>
        /// WPF calls this method when the app starts up, but before the first window appears.
        /// </summary>
        /// <param name="e">Contains the program's command-line arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
                        AppId = new AppId_t(appId);
                        // Steam will look for the App ID in env vars during initialization
                        Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());
                        Environment.SetEnvironmentVariable("SteamGameId", appId.ToString());
                        break;
                    }
                }

                // Did we find a number?
                if (AppId.m_AppId == 0)
                {
                    error = FileError;
                }
            }
            catch (Exception ex)
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
                Shutdown();
            }
        }

        /// <summary>
        /// Runs Steam callbacks repeatedly until the program exits.
        /// </summary>
        private static void RunCallbacks(object state)
        {
            while (_running)
            {
                Current.Dispatcher.Invoke(new Action(SteamAPI.RunCallbacks));
                Thread.Sleep(100); // Every 100 ms, or 10 times per second
            }
        }

        /// <summary>
        /// WPF calls this when the app is about to exit.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _running = false;
            SteamAPI.Shutdown();
            base.OnExit(e);
        }
    }
}
