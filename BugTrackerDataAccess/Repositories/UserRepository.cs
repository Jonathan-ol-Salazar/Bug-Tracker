using BugTrackerDataAccess.Models;
using MongoDB.Bson;
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

        //{
        //    FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.ID, id);
        //    DeleteResult deleteResult = await _context.Users.DeleteOneAsync(filter);

        //    return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        //}
        public async Task<bool> ResetUsers()
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Empty;
            DeleteResult deleteResult = await _context.Users.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }


        public async Task<IEnumerable<User>> GetAllUsers()
        {

            return await _context.Users.Find(Builders<User>.Filter.Empty).ToListAsync();
        }


        public async Task AddUsers(List<User> user)
        {
            await _context.Users.InsertManyAsync(user);
        }



        public async Task<User> GetUser(string id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.ID, id);
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

        public async Task<bool> Delete(List<User> user)
        {

            var filter = new BsonDocument("user_id", new BsonDocument("$in", new BsonArray(user)));
            var x = "";
            DeleteResult deleteResult = await _context.Users.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
        //{
        //    FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.ID, id);
        //    DeleteResult deleteResult = await _context.Users.DeleteManyAsync();

        //    return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        //}
        //{
        //    FilterDefinition<User> filter = Builders<User>.Filter.Eq(x => x.ID, id);
        //    DeleteResult deleteResult = await _context.Users.DeleteOneAsync(filter);

        //    return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        //}
    }


    public interface IUserRepository
    {
        // LIST OF USERS 
        Task<IEnumerable<User>> GetAllUsers();
        
        // EDIT USERS
            // Selecting Users 
        Task<User> GetUser(string id);

        Task Create(User user);
        // 'Submit' button
        Task<bool> Update(User user);
            // 'Delete' button
        Task<bool> Delete(List<User> user);

        // Add all Users from Auth0 to db
        //Task Create(User user);

        Task AddUsers(List<User> user);
        Task<bool> ResetUsers();


    }

}