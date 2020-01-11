using BugTrackerDataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.BsonSerializationContext;

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

        //public async Task AddUsersFromAuth0()
        //{
        //    List<User> userDocuments;
        //    // Use API to get all users 

        //    // ACCESS TOKEN FOR AUTH0 MANAGEMENT API

        //    var client = new RestClient("https://wussubininja.au.auth0.com/oauth/token");
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //    request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=LZ1ZnJCpRTSZB4b2iET97KhOajNiPyLk&client_secret=6Actr7Xa1tNRC6370iM6rzD68Wbpq8UCurK3QbtBiRRAUZqheOwFzDspQkZ2-7QJ&audience=https://wussubininja.au.auth0.com/api/v2/", ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);

        //    // Parsing into JSON 
        //    var response2dict = JObject.Parse(response.Content);
        //    // Retrieving Access Token
        //    var Auth0ManagementAPI_AccessToken = response2dict.First.First.ToString();


        //    // GETTING ROLES ASSIGNED TO USER FROM AUTH0
        //    string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/";
        //    string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
        //    // Endpoint to get user role
        //    client = new RestClient(baseURL);
        //    request = new RestRequest(Method.GET);
        //    // Add Auth0 Management API Access Token 
        //    request.AddHeader("authorization", authorizationValue);
        //    response = client.Execute(request);

        //    response2dict = JObject.Parse(response.Content.TrimStart('[').TrimEnd(']'));

        //    var x = 0;
        //    // for-loop to create documents of all users 
        //   // for ()

        //    // index to required attr and assign them to values in documents
        //    // api call to get user role and add to document 
        //    // Insert all documents into database
        //    await _context.Users.InsertManyAsync(userDocuments);
        //}

        public async Task AddUsers(List<User> user)
        {
            await _context.Users.InsertManyAsync(user);
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
        Task<User> GetUser(int id);

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