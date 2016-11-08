using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.WebApi.Hubs;

namespace ChatServer.WebApi.Services
{
    public class SupportRequestRoomsRepository : ISupportRequestRoomsRepository
    {
        private readonly Dictionary<string, UserSupportRoom> rooms = new Dictionary<string, UserSupportRoom>(); 

        public Task Add(UserSupportRoom supportRoom)
        {
            rooms.Add(supportRoom.RequesterClientUserId, supportRoom);
            return Task.FromResult(true);
        }

        public Task<List<UserSupportRoom>> GetAll()
        {
            return Task.FromResult(rooms.Values.ToList());
        }

        public Task<UserSupportRoom> Get(string userId)
        {
            return Task.FromResult(rooms[userId]);
        }

        public Task Save(UserSupportRoom room)
        {
            rooms[room.RequesterClientUserId] = room;
            return Task.FromResult(true);
        }

        public Task<bool> TryGetRoomForUser(string userId, out UserSupportRoom room)
        {
            if (rooms.ContainsKey(userId))
            {
                room = rooms[userId];
                return Task.FromResult(true);
            }

            room = null;
            return Task.FromResult(false);
        }
    }

    public interface ISupportRequestRoomsRepository
    {
        Task Add(UserSupportRoom supportRoom);

        Task<List<UserSupportRoom>> GetAll();
        Task<UserSupportRoom> Get(string roomId);
        Task Save(UserSupportRoom room);
        Task<bool> TryGetRoomForUser(string userId, out UserSupportRoom room);
    }
}