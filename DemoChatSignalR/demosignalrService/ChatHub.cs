using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.WindowsAzure.Mobile.Service;

namespace demosignalrService
{
    [HubName("ChatHub")]
    public class ChatHub : Hub
    {
        public ApiServices Services { get; set; }

        public void Send(string message)
        {
            Clients.All.helloMessage(message);
        }
    }
}