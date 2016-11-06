using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatServer.WebApi.Services
{
    public interface ISupportUsersRegistry
    {
        Task<bool> IsSupportUser(string userId);
        Task<List<string>> GetSupportUserIds();
    }

    public class SupportUsersRegistry : ISupportUsersRegistry
    {
        private readonly List<string> supportUsersList = new List<string>()
        {
            "spavkov@gmail.com",
            "support1@gmail.com",
            "support2@gmail.com"
        };

        public Task<bool> IsSupportUser(string userId)
        {
            return Task.FromResult(supportUsersList.Contains(userId));
        }

        public Task<List<string>> GetSupportUserIds()
        {
            return Task.FromResult(supportUsersList);
        }
    }
}