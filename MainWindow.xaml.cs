using Microsoft.Win32;
using Steamworks;
using Steamworks.Ugc;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IASWorkshop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The AppId for I Am Sakuya
        /// </summary>
        private const uint AppId = 111111;

        public MainWindow()
        {
            InitializeComponent();
            //DoSteamStuff();
        }

        /// <summary>
        /// Initializes the Steamworks library and fetches the user's previously submitted mods.
        /// </summary>
        private void DoSteamStuff()
        {
            try
            {
                SteamClient.Init(AppId, true);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not connect to Steam. Make sure Steam is running and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Close the current window
                Close();
                return;
            }

            if (SteamClient.IsValid)
            {
                // Get items published by the current user
                var query = Query.ItemsReadyToUse
                    .WhereUserPublished(SteamClient.SteamId)
                    .SortByCreationDate();

                // Execute the query
                query.GetPageAsync(1).ContinueWith(x =>
                    {
                        foreach (var entry in x.Result?.Entries ?? Array.Empty<Item>())
                        {
                            // TODO: Display these results on screen. Let the user edit or delete them.
                        }
                    });
            }

            // We are done using the Steam API
            SteamClient.Shutdown();
        }

        /// <summary>
        /// Shows a file picker for the Preview Image.
        /// Updates the GUI, displaying the selected file path and the image itself.
        /// </summary>
        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog(this) == true)
            {
                PreviewImagePath.Text = dialog.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        /// <summary>
        /// Submits a Steam Workshop item.
        /// </summary>
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement this method.
            return;

            // File submission will look something like this
            Editor.NewCommunityFile
                .ForAppId(AppId)
                .InLanguage("English")
                .WithPublicVisibility()
                .WithTitle("Title")
                .WithDescription("Description")
                .WithTag("Tag")
                .WithMetaData("MetaData")
                .WithChangeLog("ChangeLog")
                .WithPreviewFile("PreviewFile")
                .WithContent("FolderName")
                .SubmitAsync(new SubmissionProgress());
        }
    }

    internal class SubmissionProgress : IProgress<float>
    {
        private float lastValue = 0;

        public void Report(float value)
        {
            if (value > lastValue)
            {
                lastValue = value;
                // TODO: Update GUI
            }
        }
    }
}
