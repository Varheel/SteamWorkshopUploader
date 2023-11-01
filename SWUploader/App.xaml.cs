using Steamworks;
using System;
using System.Threading;
using System.Windows;

namespace SWUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The AppId for Twilight Town FPS
        /// </summary>
        public static readonly AppId_t AppId = new AppId_t(2477140);

        private static bool _running = true;

        /// <summary>
        /// WPF calls this method when the app starts up, but before the first window appears.
        /// </summary>
        /// <param name="e">Contains the program's command-line arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool success;

            // Steam will look for the App ID during initialization
            Environment.SetEnvironmentVariable("SteamAppId", AppId.ToString());
            Environment.SetEnvironmentVariable("SteamGameId", AppId.ToString());

            try
            {
                success = SteamAPI.Init();
            }
            catch (Exception)
            {
                success = false;
            }

            if (success)
            {
                ThreadPool.QueueUserWorkItem(RunCallbacks);
            }
            else
            {
                MessageBox.Show("Could not connect to Steam. Make sure Steam is running and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
