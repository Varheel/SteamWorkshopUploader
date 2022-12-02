using Microsoft.Win32;
using Steamworks;
using Steamworks.Ugc;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace IASWorkshop
{
    /// <summary>
    /// Interaction logic for Publish.xaml
    /// </summary>
    public partial class PublishPage : Page
    {
        private const uint MaxImageSize = 1024 * 1024;
        private static readonly string[] ImageTypes = new[] { ".jpg", ".png", ".gif" };
        private static readonly string[] ContentTypes = new[] { ".wad", ".7z", ".zip", ".pk3", ".pk7", ".pkz" };
        private const string ContentTypesText = "WAD, .7z, .zip, .pk3, pk7, or .pkz";

        private readonly NavigationWindow _window;
        private readonly Editor _editor;

        public PublishPage(Item? itemToEdit = null)
        {
            InitializeComponent();

            _window = (NavigationWindow)System.Windows.Application.Current.MainWindow;

            if (itemToEdit.HasValue)
            {
                _editor = new Editor(itemToEdit.Value.Id);
                TitleText.Text = itemToEdit.Value.Title;
                DescriptionText.Text = itemToEdit.Value.Description;
            }
            else
            {
                _editor = Editor.NewCommunityFile;
            }

            TitleText.TextChanged += (s, e) => UpdateUi();
            DescriptionText.TextChanged += (s, e) => UpdateUi();

            UpdateUi();
        }

        private void UpdateUi()
        {
            // Enable the Submit button if all fields have values
            // TODO: Use MVVM because that is the preferred way
            SubmitButton.IsEnabled = !(
                string.IsNullOrWhiteSpace(TitleText.Text) ||
                string.IsNullOrWhiteSpace(DescriptionText.Text) ||
                string.IsNullOrWhiteSpace(PreviewImagePath.Text) ||
                string.IsNullOrWhiteSpace(ContentFolder.Text));
        }

        /// <summary>
        /// Shows a folder picker for the Workshop content.
        /// Updates the GUI with the selected folder path.
        /// </summary>
        private void ChooseContent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.SelectedPath;

                if (Directory.Exists(path))
                {
                    bool ok = false;
                    string message = $"No mod files were found. The folder must contain a {ContentTypesText} file.";

                    foreach (var file in new DirectoryInfo(path).EnumerateFiles())
                    {
                        // All files must be one of the accepted types
                        if (ContentTypes.Contains(file.Extension.ToLower()))
                        {
                            // And make sure there's at least one that isn't blank
                            if (file.Length > 0)
                            {
                                ok = true;
                            }
                        }
                        else
                        {
                            message = $"The folder must contain only {ContentTypesText} files.";
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        ContentFolder.Text = path;
                    }
                    else
                    {
                        MessageBox.Show(_window, message);
                    }
                }
                else
                {
                    MessageBox.Show(_window, "Please select a folder.");
                }
            }

            UpdateUi();
        }

        /// <summary>
        /// Shows a file picker for the Preview Image.
        /// Updates the GUI, displaying the selected file path and the image itself.
        /// </summary>
        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = $"Image Files|*{string.Join(";*", ImageTypes)}"
            };

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                bool ok;

                try
                {
                    var file = new FileInfo(path);
                    ok = file.Exists && file.Length > 0 && file.Length < MaxImageSize && ImageTypes.Contains(file.Extension.ToLower());
                }
                catch (Exception)
                {
                    ok = false;
                }

                if (ok)
                {
                    PreviewImagePath.Text = path;
                    PreviewImage.Source = new BitmapImage(new Uri(path));
                }
                else
                {
                    MessageBox.Show(_window, "Please select a valid JPG, PNG, or GIF file that is smaller than 1MB.");
                }
            }

            UpdateUi();
        }

        /// <summary>
        /// "Submit" button handler. Submits a Steam Workshop item.
        /// </summary>
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            var result = await _editor
                .ForAppId(App.AppId)
                .WithPublicVisibility()
                .WithTitle(TitleText.Text)
                .WithDescription(DescriptionText.Text)
                .WithPreviewFile(PreviewImagePath.Text)
                .WithContent(ContentFolder.Text)
                .SubmitAsync(new SubmissionProgress());

            // TODO: There are many possible results other than OK that could be used to show the user specific error messages.
            if (result.Result == Result.OK)
            {
                // Just make a new MainPage so it refreshes (instead of calling NavigationService.GoBack)
                _window.Navigate(new MainPage());
            }
            else
            {
                MessageBox.Show(_window, "An error occurred while trying to submit your content.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// "Cancel" button handler. Navigates back to the main page.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e) => _window.Navigate(new MainPage());
    }
}
