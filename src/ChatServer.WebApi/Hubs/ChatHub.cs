using System.Collections.Generic;
using System.Threading.Tasks;
using ChatServer.WebApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ChatServer.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> log;
        private readonly IUserConnectionsRepository userConnectionsRepository;
        private string SupportLobbyGroupName = "SupportLobby";

        public ChatHub(ILogger<ChatHub> log, IUserConnectionsRepository userConnectionsRepository)
        {
            this.log = log;
            this.userConnectionsRepository = userConnectionsRepository;
            this.log.LogInformation("Created chat hub");
        }

        /*
        protected object GetAuthInfo()
        {
            var user = Context.User;
            return new
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                IsAdmin = user.IsInRole("Admin"),
                UserName = user.Identity.Name
            };
        }*/

        protected string GetUserId()
        {
            var user = Context.User;
            if (user != null && user.Identity != null && user.Identity.Name != null)
            {
                return user.Identity.Name;
            }
            return null;
        }

        public async override Task OnConnected()
        {
            this.log.LogInformation($"On connected: {Context.ConnectionId} {Context.User.Identity.Name}");
            var userId = GetUserId();
            if (userId == null)
                return;

            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);

            var users = await GetOnlineUsers();
            // Set connection id for just connected client only
            Clients.Client(Context.ConnectionId).setConnectionId(Context.ConnectionId);
            Clients.All.setOnlineUsers(users);
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            return await userConnectionsRepository.GetAllActiveConnections();
        }

        public async override Task OnDisconnected(bool stopCalled)
        {
            this.log.LogInformation($"On disconnected: {Context.ConnectionId}");
            var userId = GetUserId();
            if (userId != null)
            {
                await userConnectionsRepository.RemoveConnectionForUser(userId, Context.ConnectionId);
            }
            var users = await GetOnlineUsers();
            Clients.All.setOnlineUsers(users);
            await base.OnDisconnected(stopCalled);
        }

        public async override Task OnReconnected()
        {
            this.log.LogInformation($"On reconnected: {Context.ConnectionId}");

            var userId = GetUserId();
            if (userId == null)
                return;

            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);

            await base.OnReconnected();
        }
    }
}