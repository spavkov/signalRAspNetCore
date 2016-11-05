using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SignalRT1.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> log;
        private string SupportLobbyGroupName = "SupportLobby";

        public ChatHub(ILogger<ChatHub> log)
        {
            this.log = log;
            this.log.LogInformation("Created chat hub");
        }

        public void Connect(string userName)
        {
            this.log.LogInformation("connecting " + userName);
        }

        protected object GetAuthInfo()
        {
            var user = Context.User;
            return new
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                IsAdmin = user.IsInRole("Admin"),
                UserName = user.Identity.Name
            };
        }

        public override Task OnConnected()
        {
            this.log.LogInformation($"On connected: {Context.ConnectionId} {Context.User.Identity.Name}");
            // Set connection id for just connected client only
            return Clients.Client(Context.ConnectionId).setConnectionId(Context.ConnectionId);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this.log.LogInformation($"On disconnected: {Context.ConnectionId}");
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            this.log.LogInformation($"On reconnected: {Context.ConnectionId}");
            return base.OnReconnected();
        }
    }
}