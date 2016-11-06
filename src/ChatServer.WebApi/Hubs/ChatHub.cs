using System.Collections.Generic;
using System.Linq;
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
        private readonly ISupportUsersRegistry supportUsersRegistry;
        private string SupportCommonGroupName = "SupportCommongGroup";
        private string SupportClientsThatNeedHelpGroupName = "SupportClientsThatNeedHelpGroup";

        public ChatHub(ILogger<ChatHub> log, IUserConnectionsRepository userConnectionsRepository, ISupportUsersRegistry supportUsersRegistry)
        {
            this.log = log;
            this.userConnectionsRepository = userConnectionsRepository;
            this.supportUsersRegistry = supportUsersRegistry;
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

        private async Task SendOnlineUsersToSupportUser()
        {
            var users = await GetOnlineUserIds();
            var supportUsers = await supportUsersRegistry.GetSupportUserIds();
            var onlineSupportUsers = users.Intersect(supportUsers).ToList();

            Clients.Group(SupportCommonGroupName).setOnlineUsersForSupport(onlineSupportUsers);
        }

        public async override Task OnConnected()
        {
            this.log.LogInformation($"On connected: {Context.ConnectionId} {Context.User.Identity.Name}");
            var userId = GetUserId();
            if (userId == null)
                return;

            Clients.Client(Context.ConnectionId).setConnectionId(Context.ConnectionId);
            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);

            if (await supportUsersRegistry.IsSupportUser(userId))
            {
                await Groups.Add(Context.ConnectionId, SupportCommonGroupName);
                await SendOnlineUsersToSupportUser();
            }
        }

        public async Task<List<string>> GetOnlineUserIds()
        {
            return await userConnectionsRepository.GetAllActiveUserIds();
        }

        public async override Task OnDisconnected(bool stopCalled)
        {
            this.log.LogInformation($"On disconnected: {Context.ConnectionId}");
            var userId = GetUserId();
            if (userId != null)
            {
                await userConnectionsRepository.RemoveConnectionForUser(userId, Context.ConnectionId);
            }
            await SendOnlineUsersToSupportUser();

            await base.OnDisconnected(stopCalled);
        }

        public async override Task OnReconnected()
        {
            this.log.LogInformation($"On reconnected: {Context.ConnectionId}");

            var userId = GetUserId();
            if (userId == null)
                return;

            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);
            await SendOnlineUsersToSupportUser();

            await base.OnReconnected();
        }
    }
}