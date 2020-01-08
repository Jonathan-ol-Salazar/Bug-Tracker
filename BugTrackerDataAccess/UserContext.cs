using BugTrackerDataAccess.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BugTrackerDataAccess
{
    public class UserContext : IUserContext
    {
        private readonly IMongoDatabase mongoDatabase;


        public UserContext(IOptions<Settings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            mongoDatabase = client.GetDatabase(options.Value.Database);
        }

        public IMongoCollection<User> Users => mongoDatabase.GetCollection<User>("Users");


    }

    public interface IUserContext
    {
        IMongoCollection<User> Users { get; }
    }
}