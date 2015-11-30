using System;
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

        private IHubProxy proxy;
        private MobileServiceUser user;
        private HubConnection hubConnection;
        private string message;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            await ConnectToSignalR();
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

            proxy.On<string>("helloMessage", async (msg) =>
             {
                 await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                 {
                     message += " " + msg;
                     Message.Text = message;
                 });

             });
        }

        private async void Button_OnClick(object sender, RoutedEventArgs e)
        {
            await proxy.Invoke<string>("Send", TextBox.Text);
            TextBox.Text = string.Empty;
        }
    }

    
}
