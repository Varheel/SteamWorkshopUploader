using Steamworks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace IASWorkshopLegacy
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly NavigationWindow _window;
        private readonly CallResult<SteamUGCQueryCompleted_t> _queryCallResult;
        private readonly CallResult<DeleteItemResult_t> _deleteItemCallResult;

        public MainPage()
        {
            InitializeComponent();

            _window = (NavigationWindow)Application.Current.MainWindow;
            _queryCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
            _deleteItemCallResult = CallResult<DeleteItemResult_t>.Create(OnDeleteCompleted);

            RefreshList();
        }

        /// <summary>
        /// Gets the user's published Workshop items and updates the list in the UI.
        /// TODO: Disable the ListBox while we are awaiting results.
        /// </summary>
        private void RefreshList()
        {
            var userId = SteamUser.GetSteamID().GetAccountID();

            // Get items published by the current user
            var query = SteamUGC.CreateQueryUserUGCRequest(userId,
                EUserUGCList.k_EUserUGCList_Published,
                EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse,
                EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc,
                App.AppId, App.AppId,
                // Request only the first page of 50 results. If a user ever submits more than
                // 50 mods for this game, then the ModList here will need to implement some kind
                // of paging feature to display them all. That seems unlikely, though.
                1);

            var call = SteamUGC.SendQueryUGCRequest(query);
            _queryCallResult.Set(call);
        }

        /// <summary>
        /// "Refresh" button handler.
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshList();

        /// <summary>
        /// "Edit" button handler. Navigates to the Publish page and passes the selected item for editing.
        /// </summary>
        private void Edit_Click(object sender, RoutedEventArgs e) => _window.Navigate(new PublishPage((SteamUGCDetails_t)ModList.SelectedItem));

        /// <summary>
        /// "Delete" button handler. Asks the user for confirmation, then deletes the selected item and refreshes the list.
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = (SteamUGCDetails_t)ModList.SelectedItem;
            string message = $"\"{item.m_rgchTitle}\" will be removed from the Steam Workshop. Are you sure you want to continue?";
            var result = MessageBox.Show(_window, message, "Confirm Deletion", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var call = SteamUGC.DeleteItem(item.m_nPublishedFileId);
                _deleteItemCallResult.Set(call);
            }
        }

        /// <summary>
        /// "New" button handler. Navigates to the Publish page.
        /// </summary>
        private void New_Click(object sender, RoutedEventArgs e) => _window.Navigate(new PublishPage());

        /// <summary>
        /// Handles a completed API call to get the user's published workshop items.
        /// </summary>
        private void OnQueryCompleted(SteamUGCQueryCompleted_t response, bool failure)
        {
            if (failure || response.m_eResult != EResult.k_EResultOK)
            {
                return;
            }

            var items = new SteamUGCDetails_t[response.m_unNumResultsReturned];

            // Extract the results into the new array
            for (uint i = 0; i < response.m_unNumResultsReturned; i++)
            {
                SteamUGC.GetQueryUGCResult(response.m_handle, i, out items[i]);
            }

            // Update the UI
            ModList.ItemsSource = items;
        }

        /// <summary>
        /// Handles a completed API call to delete a workshop item.
        /// </summary>
        private void OnDeleteCompleted(DeleteItemResult_t result, bool failure)
        {
            // TODO: Should check result and show a success/failure message, but this works just fine
            RefreshList();
        }
    }
}
