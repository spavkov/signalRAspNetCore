using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatServer.WebApi.Services
{
    public interface IUserConnectionsRepository
    {
        Task AddUserConnectionId(string userId, string connectionId);
        Task<List<UserConnection>> GetConnectionsForUser(string userId);
        Task RemoveConnectionForUser(string userId, string connectionId);
        Task<List<string>> GetAllActiveConnections();
    }

    public class UserConnectionsRepository : IUserConnectionsRepository
    {
        readonly ConcurrentDictionary<string, List<UserConnection>> userConnections = new ConcurrentDictionary<string, List<UserConnection>>();

        public Task AddUserConnectionId(string userId, string connectionId)
        {
            userConnections.AddOrUpdate(userId, s =>
            {
                return new List<UserConnection>()
                {
                    new UserConnection(connectionId)
                };
            }, (userId2, list) =>
            {
               list.Add(new UserConnection(connectionId));
               return list;
            });

            return Task.FromResult(true);
        }

        public Task<List<UserConnection>> GetConnectionsForUser(string userId)
        {
            List<UserConnection> conns;
            userConnections.TryGetValue(userId, out conns);
            return Task.FromResult(conns);
        }

        public Task<List<string>> GetAllActiveConnections()
        {
            return Task.FromResult(userConnections.Keys.Distinct().ToList());
        }

        public async Task RemoveConnectionForUser(string userId, string connectionId)
        {
            var conns = await GetConnectionsForUser(userId);
            var found = conns?.FirstOrDefault(a => a.ConnectionId == connectionId);
            if (found != null)
            {
                conns.Remove(found);
            }

            if (conns.Any())
            {
                userConnections.AddOrUpdate(userId, id => conns, (id2, origList) =>
                {
                    var all = origList.Union(conns).Distinct().ToList();
                    return all;
                });
            }
            else
            {
                List<UserConnection> result;
                userConnections.TryRemove(userId, out result);
            }
        }
    }

    public class UserConnection 
    {
        public UserConnection(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public readonly string ConnectionId;
        public DateTime ConnectionDateTimeUtc { get; set; }

        public override int GetHashCode()
        {
            return this.ConnectionId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserConnection);
        }

        public virtual bool Equals(UserConnection other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other)) { return true; }
            return ConnectionId.Equals(other.ConnectionId);
        }
    }
}