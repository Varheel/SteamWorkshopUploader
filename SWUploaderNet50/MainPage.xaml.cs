using Steamworks.Ugc;
using Steamworks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;

namespace SWUploaderNet50
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly NavigationWindow _window;

        public MainPage()
        {
            InitializeComponent();
            _window = (NavigationWindow)Application.Current.MainWindow;

            // Don't await because we are inside a constructor
            _ = RefreshList();
        }

        /// <summary>
        /// Gets the user's published Workshop items and updates the list in the UI.
        /// TODO: Disable the ListBox while we are awaiting results.
        /// </summary>
        private async Task RefreshList()
        {
            // Get items published by the current user
            var query = Query.ItemsReadyToUse
                .WhereUserPublished(SteamClient.SteamId)
                .SortByCreationDate();

            // GetPageAsync(1) requests the first page of results, which returns up to 50 items according to the Steam API.
            // If a user ever submits more than 50 mods for this game, then the ModList here will need to implement some
            // kind of paging feature in order to display them all. That seems unlikely, though.
            var result = await query.GetPageAsync(1);

            // Update the UI
            ModList.ItemsSource = result?.Entries ?? Array.Empty<Item>();
        }

        /// <summary>
        /// "Refresh" button handler.
        /// </summary>
        private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshList();

        /// <summary>
        /// "Edit" button handler. Navigates to the Publish page and passes the selected item for editing.
        /// </summary>
        private void Edit_Click(object sender, RoutedEventArgs e) => _window.Navigate(new PublishPage((Item)ModList.SelectedItem));

        /// <summary>
        /// "Delete" button handler. Asks the user for confirmation, then deletes the selected item and refreshes the list.
        /// </summary>
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item)ModList.SelectedItem;
            string message = $"\"{item.Title}\" will be removed from the Steam Workshop. Are you sure you want to continue?";
            var result = MessageBox.Show(_window, message, "Confirm Deletion", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Should check Task.Result and show a success/failure message, but this works just fine
                await SteamUGC.DeleteFileAsync(item.Id);
                await RefreshList();
            }
        }

        /// <summary>
        /// "New" button handler. Navigates to the Publish page.
        /// </summary>
        private void New_Click(object sender, RoutedEventArgs e) => _window.Navigate(new PublishPage());
    }
}
