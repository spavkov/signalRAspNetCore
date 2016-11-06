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
            rooms.Add(supportRoom.RoomId, supportRoom);
            return Task.FromResult(true);
        }

        public Task<List<UserSupportRoom>> GetAll()
        {
            return Task.FromResult(rooms.Values.ToList());
        }

        public Task<UserSupportRoom> Get(string roomId)
        {
            return Task.FromResult(rooms[roomId]);
        }

        public Task Save(UserSupportRoom room)
        {
            rooms[room.RoomId] = room;
            return Task.FromResult(true);
        }
    }

    public interface ISupportRequestRoomsRepository
    {
        Task Add(UserSupportRoom supportRoom);

        Task<List<UserSupportRoom>> GetAll();
        Task<UserSupportRoom> Get(string roomId);
        Task Save(UserSupportRoom room);
    }
}