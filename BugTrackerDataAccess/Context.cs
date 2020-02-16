using BugTrackerDataAccess.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BugTrackerDataAccess
{
    public class Context : IUserContext, IProjectContext, IIssueContext, IRoleContext
    {
        private readonly IMongoDatabase mongoDatabase;


        public Context(IOptions<Settings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            mongoDatabase = client.GetDatabase(options.Value.Database);
        }

        public IMongoCollection<User> Users => mongoDatabase.GetCollection<User>("Users");
        public IMongoCollection<Project> Projects => mongoDatabase.GetCollection<Project>("Projects");
        public IMongoCollection<Issue> Issues => mongoDatabase.GetCollection<Issue>("Issues");
        public IMongoCollection<Role> Roles => mongoDatabase.GetCollection<Role>("Roles");


    }

    public interface IUserContext
    {
        IMongoCollection<User> Users { get; }
    }

    public interface IProjectContext
    {
        IMongoCollection<Project> Projects { get; }
    }

    public interface IIssueContext
    {
        IMongoCollection<Issue> Issues { get; }
    }

    public interface IRoleContext
    {
        IMongoCollection<Role> Roles { get; }
    }
}