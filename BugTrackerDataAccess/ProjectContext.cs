using BugTrackerDataAccess.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BugTrackerDataAccess
{
    public class ProjectContext : IProjectContext
    {
        private readonly IMongoDatabase mongoDatabase;


        public ProjectContext(IOptions<Settings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            mongoDatabase = client.GetDatabase(options.Value.Database);
        }

        public IMongoCollection<Project> Projects => mongoDatabase.GetCollection<Project>("Projects");


    }

    public interface IProjectContext
    {
        IMongoCollection<Project> Projects { get; }
    }
}