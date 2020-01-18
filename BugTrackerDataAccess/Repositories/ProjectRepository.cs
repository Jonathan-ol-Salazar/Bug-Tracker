using BugTrackerDataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTrackerDataAccess.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IProjectContext _context;

        public ProjectRepository(IProjectContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Project>> GetAllProjects()
        {
            return await _context.Projects.Find(Builders<Project>.Filter.Empty).ToListAsync();
        }


        public async Task AddProjects(List<Project> project)
        {
            await _context.Projects.InsertManyAsync(project);
        }


        public async Task<Project> GetProject(string id)
        {
            FilterDefinition<Project> filter = Builders<Project>.Filter.Eq(x => x.ID, id);
            return await _context.Projects.Find(filter).FirstOrDefaultAsync();
        }

        public async Task Create(Project project)
        {
            await _context.Projects.InsertOneAsync(project);
        }

        public async Task<bool> Update(Project project)
        {
            ReplaceOneResult updateResult = await _context.Projects.ReplaceOneAsync(filter: b => b.Id == project.Id, replacement: project);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> Delete(List<Project> project)
        {
            var filter = new BsonDocument("project_id", new BsonDocument("$in", new BsonArray(project)));

            DeleteResult deleteResult = await _context.Projects.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

        public async Task<bool> ResetProjects()
        {
            FilterDefinition<Project> filter = Builders<Project>.Filter.Empty;
            DeleteResult deleteResult = await _context.Projects.DeleteManyAsync(filter);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }

    public interface IProjectRepository
    {
        // LIST OF USERS 
        Task<IEnumerable<Project>> GetAllProjects();

        // EDIT USERS
        // Selecting Projects 
        Task<Project> GetProject(string id);

        Task Create(Project project);
        // 'Submit' button
        Task<bool> Update(Project project);
        // 'Delete' button
        Task<bool> Delete(List<Project> project);

        // Add all Projects from Auth0 to db
        //Task Create(Project project);

        Task AddProjects(List<Project> project);
        Task<bool> ResetProjects();


    }

}