using Steamworks;
using System.Windows;

namespace SWUploaderNet35
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The number that identifies the game on the Steam Workshop.
        /// </summary>
        public static AppId_t AppId { get; set; }

        /// <summary>
        /// True while the Steam API is running.
        /// </summary>
        public static bool Running { get; set; } = false;

        /// <summary>
        /// WPF calls this when the app is about to exit.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            if (Running)
            {
                Running = false;
                SteamAPI.Shutdown();
                base.OnExit(e);
            }
        }
    }
}
