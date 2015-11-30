using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Microsoft.WindowsAzure.MobileServices;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

// To add offline sync support, add the NuGet package Microsoft.WindowsAzure.MobileServices.SQLiteStore
// to your project. Then, uncomment the lines marked // offline sync
// For more information, see: http://aka.ms/addofflinesync
//using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
//using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync

namespace demosignalr
{
    sealed partial class MainPage : Page
    {
        private MobileServiceCollection<Message, Message> messages;
        private readonly IMobileServiceTable<Message> messageTable = App.MobileService.GetTable<Message>();
        private IHubProxy proxy;
        //private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>(); // offline sync

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task InsertTodoItem(Message todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await messageTable.InsertAsync(todoItem);
            messages.Add(todoItem);

            //await SyncAsync(); // offline sync
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                messages = await messageTable
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                ListItems.ItemsSource = messages;
                this.ButtonSave.IsEnabled = true;
            }
        }

        private async Task UpdateCheckedTodoItem(Message item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await messageTable.UpdateAsync(item);
            messages.Remove(item);
            ListItems.Focus(Windows.UI.Xaml.FocusState.Unfocused);

            //await SyncAsync(); // offline sync
        }

        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonRefresh.IsEnabled = false;

            //await SyncAsync(); // offline sync
            await RefreshTodoItems();

            ButtonRefresh.IsEnabled = true;
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new Message { Text = TextInput.Text };
            //await InsertTodoItem(todoItem);
            await proxy.Invoke<Message>("Send", todoItem);
        }

        private async void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Message item = cb.DataContext as Message;
            await UpdateCheckedTodoItem(item);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //await InitLocalStoreAsync(); // offline sync
            await ConnectToSignalR();
            await RefreshTodoItems();
        }

        private async Task ConnectToSignalR()
        {
            hubConnection = new HubConnection(App.MobileService.ApplicationUri.AbsoluteUri);

            if (user != null)
            {
                hubConnection.Headers["x-zumo-auth"] = user.MobileServiceAuthenticationToken;
            }
            else
            {
                hubConnection.Headers["x-zumo-application"] = App.MobileService.ApplicationKey;
            }

            proxy = hubConnection.CreateHubProxy("ChatHub");
            await hubConnection.Start();


            var message = new Message()
            {
                Id = "1",
                Text = "Mensaje desde clase"
            };

            proxy.On<Message>("hello", async msg =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var callbackDialog = new MessageDialog(msg.Text);
                    callbackDialog.Commands.Add(new UICommand("OK"));
                    await callbackDialog.ShowAsync();
                });
            });

            proxy.On<string>("helloMessage", async msg =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var callbackDialog = new MessageDialog(msg);
                    callbackDialog.Commands.Add(new UICommand("OK"));
                    await callbackDialog.ShowAsync();
                });
            });

            await proxy.Invoke<Message>("Send", message);
            await proxy.Invoke<string>("JoinRoom", "grupo");



        }

        private MobileServiceUser user;
        private HubConnection hubConnection;

        #region Offline sync

        //private async Task InitLocalStoreAsync()
        //{
        //    if (!App.MobileService.SyncContext.IsInitialized)
        //    {
        //        var store = new MobileServiceSQLiteStore("localstore.db");
        //        store.DefineTable<TodoItem>();
        //        await App.MobileService.SyncContext.InitializeAsync(store);
        //    }
        //
        //    await SyncAsync();
        //}

        //private async Task SyncAsync()
        //{
        //    await App.MobileService.SyncContext.PushAsync();
        //    await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
        //}

        #endregion 
    }
}
