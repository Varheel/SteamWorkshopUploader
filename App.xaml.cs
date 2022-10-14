using System;
using System.Diagnostics;
using System.Windows;

namespace IASWorkshop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The path from this program's folder to the main game executable
        /// </summary>
        private const string GamePath = "..\\Game\\gzdoom.exe";

        /// <summary>
        /// WPF calls this method when the app starts up, but before the first window appears.
        /// </summary>
        /// <param name="e">Contains the program's command-line arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Did we receive "publish" as a command-line arg?
            if (e.Args.Length > 0 && "publish".Equals(e.Args[0]))
            {
                // Run the WPF app normally
                base.OnStartup(e);
            }
            else
            {
                try
                {
                    // Start the main game
                    Process.Start(GamePath);
                }
                catch (Exception)
                {
                    // Most likely something is installed in the wrong location
                    MessageBox.Show("Could not start the game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Close this program because we are playing IAS instead
                Shutdown();
            }
        }
    }
}
