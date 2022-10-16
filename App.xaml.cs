using Steamworks;
using System;
using System.Windows;

namespace IASWorkshop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The AppId for I Am Sakuya
        /// </summary>
        public const uint AppId = 1960590;

        /// <summary>
        /// WPF calls this method when the app starts up, but before the first window appears.
        /// </summary>
        /// <param name="e">Contains the program's command-line arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool success;

            try
            {
                SteamClient.Init(AppId, true);
                success = SteamClient.IsValid;
            }
            catch (Exception)
            {
                success = false;
            }

            if (!success)
            {
                MessageBox.Show("Could not connect to Steam. Make sure Steam is running and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// WPF calls this when the app is about to exit.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            SteamClient.Shutdown();
            base.OnExit(e);
        }
    }
}
