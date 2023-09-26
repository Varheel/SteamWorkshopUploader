using Steamworks;
using System.IO;
using System.Windows;

namespace SteamWorkshopUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The number that identifies the game on the Steam Workshop.
        /// </summary>
        public static uint AppId { get; private set; }

        private const string AppIdFile = "AppID.txt";
        private const string FileError = "Make sure AppID.txt contains a valid number.";
        private const string ConnectionError = "Make sure Steam is running and your App ID is correct.";

        /// <summary>
        /// WPF calls this method when the app starts up, but before the first window appears.
        /// </summary>
        /// <param name="e">Contains the program's command-line arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string? error = null;

            // Read the game's App ID from the included text file
            try
            {
                foreach (string line in File.ReadLines(AppIdFile))
                {
                    // Split on whitespace (the default for Split) and take the first element (Split never returns an empty array)
                    string first = line.Split()[0];

                    if (uint.TryParse(first, out uint appId))
                    {
                        AppId = appId;
                        break;
                    }
                }

                // Did we find a number?
                if (AppId == 0)
                {
                    error = FileError;
                }
            }
            catch
            {
                error = FileError;
            }

            // Initialize the Steam API
            if (error == null)
            {
                try
                {
                    SteamClient.Init(AppId, true);

                    if (!SteamClient.IsValid)
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
        /// WPF calls this when the app is about to exit.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            SteamClient.Shutdown();
            base.OnExit(e);
        }
    }
}
