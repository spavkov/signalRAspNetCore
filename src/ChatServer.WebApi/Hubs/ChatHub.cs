using System;
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
        private readonly ISupportRequestRoomsRepository supportRequestRooms;
        private string SupportCommonGroupName = "SupportCommongGroup";
        private string SupportClientsThatNeedHelpGroupName = "SupportClientsThatNeedHelpGroup";

        public ChatHub(ILogger<ChatHub> log, 
            IUserConnectionsRepository userConnectionsRepository, 
            ISupportUsersRegistry supportUsersRegistry,
            ISupportRequestRoomsRepository supportRequestRooms)
        {
            this.log = log;
            this.userConnectionsRepository = userConnectionsRepository;
            this.supportUsersRegistry = supportUsersRegistry;
            this.supportRequestRooms = supportRequestRooms;
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

            Clients.Client(Context.ConnectionId).setConnectionId(Context.ConnectionId);
            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);        
            await NotifyUserIdHasComeOnline(userId);
        }

        private async Task NotifyUserIdHasComeOnline(string userId)
        {
            if (await supportUsersRegistry.IsSupportUser(userId))
            {
                await Groups.Add(Context.ConnectionId, SupportCommonGroupName);
                await Clients.Group(SupportCommonGroupName).addNewOnlineUser(userId);
            }
        }

        public async Task<string> RequestSupport()
        {
            var userId = GetUserId();

            UserSupportRoom room;
            if (await supportRequestRooms.TryGetRoomForUser(userId, out room))
            {
                await Groups.Add(Context.ConnectionId, room.RoomId);
                return room.RoomId;
            }

            var supportRoom = new UserSupportRoom()
            {
                RoomId = Guid.NewGuid().ToString(),
                RequesterClientUserId = userId,
                ParticipantUserIds = new List<string>()
                {
                    userId
                }
            };

            await supportRequestRooms.Add(supportRoom);
            await Groups.Add(Context.ConnectionId, supportRoom.RoomId);

            await Clients.Group(SupportCommonGroupName).supportRoomCreated(supportRoom);
            return supportRoom.RoomId;
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            var users = await userConnectionsRepository.GetAllActiveUserIds();
            var supportUsers = await supportUsersRegistry.GetSupportUserIds();
            var onlineSupportUsers = users.Intersect(supportUsers).ToList();

            return onlineSupportUsers;
        }

        public async Task<List<UserSupportRoom>> GetSupportRooms()
        {
            return await supportRequestRooms.GetAll();
        }

        public async Task<bool> ProvideSupport(string userId)
        {
            var providerUserId = GetUserId();
            var room = await supportRequestRooms.Get(userId);
            await Groups.Add(Context.ConnectionId, room.RoomId);
            room.ParticipantUserIds.Add(providerUserId);

            await supportRequestRooms.Save(room);
            Clients.Group(room.RoomId).receiveRoomMessage(room.RoomId, "PAPIRI", "Please discuss your issue with support");

            return true;
        }

        public async override Task OnDisconnected(bool stopCalled)
        {
            this.log.LogInformation($"On disconnected: {Context.ConnectionId}");
            var userId = GetUserId();
            if (userId != null)
            {
                if (await userConnectionsRepository.RemoveConnectionForUser(userId, Context.ConnectionId))
                {
                    await Clients.Group(SupportCommonGroupName).userDisconnected(userId);
                }
            }

            await base.OnDisconnected(stopCalled);
        }

        public async override Task OnReconnected()
        {
            this.log.LogInformation($"On reconnected: {Context.ConnectionId}");

            var userId = GetUserId();
            if (userId == null)
                return;

            await userConnectionsRepository.AddUserConnectionId(userId, Context.ConnectionId);
            await NotifyUserIdHasComeOnline(userId);

            await base.OnReconnected();
        }
    }

    public class UserSupportRoom
    {
        public string RoomId { get; set; }

        public string RequesterClientUserId { get; set; }

        public List<string> ParticipantUserIds { get; set; }
    }



    public class SupportContext    
    {
        public List<string> SupportUsers { get; set; }

        public List<UserSupportRoom> SupportRooms { get; set; }
    }
}