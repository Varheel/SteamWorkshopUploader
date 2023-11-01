using Microsoft.Win32;
using Steamworks;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SWUploader
{
    /// <summary>
    /// Interaction logic for PublishPage.xaml
    /// </summary>
    public partial class PublishPage : Page
    {
        private const uint MaxImageSize = 1024 * 1024;
        private static readonly string[] ImageTypes = new[] { ".jpg", ".png", ".gif" };
        private static readonly string[] ContentTypes = new[] { ".wad", ".7z", ".zip", ".pk3", ".pk7", ".pkz" };
        private const string ContentTypesText = "WAD, .7z, .zip, .pk3, pk7, or .pkz";

        private readonly NavigationWindow _window;
        private readonly CallResult<CreateItemResult_t> _createItemCallResult;
        private readonly CallResult<SubmitItemUpdateResult_t> _submitItemCallResult;
        private PublishedFileId_t _fileId = PublishedFileId_t.Invalid;

        public PublishPage(SteamUGCDetails_t? itemToEdit = null)
        {
            InitializeComponent();

            _window = (NavigationWindow)Application.Current.MainWindow;
            _createItemCallResult = CallResult<CreateItemResult_t>.Create(OnCreateCompleted);
            _submitItemCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitCompleted);

            if (itemToEdit.HasValue)
            {
                _fileId = itemToEdit.Value.m_nPublishedFileId;
                TitleText.Text = itemToEdit.Value.m_rgchTitle;
                DescriptionText.Text = itemToEdit.Value.m_rgchDescription;
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
                string.IsNullOrEmpty(TitleText.Text) ||
                string.IsNullOrEmpty(DescriptionText.Text) ||
                string.IsNullOrEmpty(PreviewImagePath.Text) ||
                string.IsNullOrEmpty(ContentFolder.Text));
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

                    foreach (var entry in new DirectoryInfo(path).GetFileSystemInfos())
                    {
                        // Only files, no subdirectories allowed. All files must be one of the accepted types.
                        if (entry is FileInfo file && ContentTypes.Contains(entry.Extension.ToLower()))
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
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_fileId == PublishedFileId_t.Invalid)
            {
                // First create a new workshop item with no content attached, then the callback will call SubmitItemUpdate
                var call = SteamUGC.CreateItem(App.AppId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                _createItemCallResult.Set(call);
            }
            else
            {
                // We are editing a previously created workshop item, so just update it
                SubmitItemUpdate();
            }
        }

        /// <summary>
        /// "Cancel" button handler. Navigates back to the main page.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e) => _window.Navigate(new MainPage());

        /// <summary>
        /// Submits an item update to Steam. This method assumes that <see cref="_fileId"/> has been set.
        /// </summary>
        private void SubmitItemUpdate()
        {
            var handle = SteamUGC.StartItemUpdate(App.AppId, _fileId);
            SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
            SteamUGC.SetItemTitle(handle, TitleText.Text);
            SteamUGC.SetItemDescription(handle, DescriptionText.Text);
            SteamUGC.SetItemPreview(handle, PreviewImagePath.Text);
            SteamUGC.SetItemContent(handle, ContentFolder.Text);

            var call = SteamUGC.SubmitItemUpdate(handle, null);
            _submitItemCallResult.Set(call);
        }

        /// <summary>
        /// Handles a completed API call to create a new workshop item.
        /// </summary>
        private void OnCreateCompleted(CreateItemResult_t response, bool failure)
        {
            if (failure || response.m_eResult != EResult.k_EResultOK)
            {
                MessageBox.Show(_window, "An error occurred while trying to create a new Steam Workshop item.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                _fileId = response.m_nPublishedFileId;
                SubmitItemUpdate();
            }
        }

        /// <summary>
        /// Handles a completed API call to update the workshop item.
        /// </summary>
        private void OnSubmitCompleted(SubmitItemUpdateResult_t response, bool failure)
        {
            // TODO: There are many possible results other than OK that could be used to show the user specific error messages.
            if (failure || response.m_eResult != EResult.k_EResultOK)
            {
                MessageBox.Show(_window, "An error occurred while trying to submit your content.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Just make a new MainPage so it refreshes (instead of calling NavigationService.GoBack)
                _window.Navigate(new MainPage());
            }
        }
    }
}
