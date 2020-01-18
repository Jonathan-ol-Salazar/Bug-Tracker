using BugTrackerDataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTrackerDataAccess.Repositories
{
    public class IssueRepository : IIssueRepository
    {
        private readonly IIssueContext _context;

        public IssueRepository(IIssueContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Issue>> GetAllIssues()
        {
            return await _context.Issues.Find(Builders<Issue>.Filter.Empty).ToListAsync();
        }


        public async Task AddIssues(List<Issue> issue)
        {
            await _context.Issues.InsertManyAsync(issue);
        }


        public async Task<Issue> GetIssue(string id)
        {
            FilterDefinition<Issue> filter = Builders<Issue>.Filter.Eq(x => x.IssueID, id);
            return await _context.Issues.Find(filter).FirstOrDefaultAsync();
        }

        public async Task Create(Issue issue)
        {
            await _context.Issues.InsertOneAsync(issue);
        }

        public async Task<bool> Update(Issue issue)
        {
            ReplaceOneResult updateResult = await _context.Issues.ReplaceOneAsync(filter: b => b.Id == issue.Id, replacement: issue);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> Delete(List<Issue> issue)
        {
            var filter = new BsonDocument("issue_id", new BsonDocument("$in", new BsonArray(issue)));

            DeleteResult deleteResult = await _context.Issues.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<bool> ResetIssues()
        {
            FilterDefinition<Issue> filter = Builders<Issue>.Filter.Empty;
            DeleteResult deleteResult = await _context.Issues.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }

    public interface IIssueRepository
    {
        // LIST OF USERS 
        Task<IEnumerable<Issue>> GetAllIssues();

        // EDIT USERS
        // Selecting Issues 
        Task<Issue> GetIssue(string id);

        Task Create(Issue issue);
        // 'Submit' button
        Task<bool> Update(Issue issue);
        // 'Delete' button
        Task<bool> Delete(List<Issue> issue);

        // Add all Issues from Auth0 to db
        //Task Create(Issue issue);

        Task AddIssues(List<Issue> issue);
        Task<bool> ResetIssues();


    }

}