using BugTrackerDataAccess.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTrackerDataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IUserContext _context;

        public UserRepository(IUserContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {

            return await _context.Users.Find(Builders<User>.Filter.Empty).ToListAsync();
        }

        public async Task<User> GetUser(int id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.UserID, id);
            return await _context.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task Create(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task<bool> Update(User user)
        {
            ReplaceOneResult updateResult = await _context.Users.ReplaceOneAsync(filter: b => b.Id == user.Id, replacement: user);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> Delete(int id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.UserID, id);
            DeleteResult deleteResult = await _context.Users.DeleteOneAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }


    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUser(int id);
        Task Create(User user);
        Task<bool> Update(User user);
        Task<bool> Delete(int id);
    }

}
