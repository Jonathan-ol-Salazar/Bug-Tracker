using BugTrackerDataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTrackerDataAccess.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IRoleContext _context;

        public RoleRepository(IRoleContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            return await _context.Roles.Find(Builders<Role>.Filter.Empty).ToListAsync();
        }


        public async Task AddRoles(List<Role> role)
        {
            await _context.Roles.InsertManyAsync(role);
        }


        public async Task<Role> GetRole(string id)
        {
            FilterDefinition<Role> filter = Builders<Role>.Filter.Eq(x => x.IDCode, id);
            return await _context.Roles.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddRole(Role role)
        {
            await _context.Roles.InsertOneAsync(role);
        }

        public async Task<bool> Update(Role role)
        {
            ReplaceOneResult updateResult = await _context.Roles.ReplaceOneAsync(filter: b => b.Id == role.Id, replacement: role);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> Delete(List<Role> role)
        {
            var filter = new BsonDocument("role_id", new BsonDocument("$in", new BsonArray(role)));

            DeleteResult deleteResult = await _context.Roles.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<bool> ResetRoles()
        {
            FilterDefinition<Role> filter = Builders<Role>.Filter.Empty;
            DeleteResult deleteResult = await _context.Roles.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }

    public interface IRoleRepository
    {
        // LIST OF USERS 
        Task<IEnumerable<Role>> GetAllRoles();

        // EDIT USERS
        // Selecting Roles 
        Task<Role> GetRole(string id);

        Task AddRole(Role role);
        // 'Submit' button
        Task<bool> Update(Role role);
        // 'Delete' button
        Task<bool> Delete(List<Role> role);

        // Add all Roles from Auth0 to db
        //Task Create(Role role);

        Task AddRoles(List<Role> role);
        Task<bool> ResetRoles();


    }

}