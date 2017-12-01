using System.Security.Cryptography.X509Certificates;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Users;
using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System.Configuration;

namespace eMatch.Data.Mongo
{
    public class MongoUserRepo : IUserRepository
    {
        readonly MongoDatabase db;

        public MongoUserRepo()
        {
            var client = new MongoClient(ConfigurationManager.ConnectionStrings["Mongo"].ToString());
            MongoServer server = client.GetServer();
            db = server.GetDatabase("ematch");
        }

        public IQueryable<User> Users
        {
            get { return db.GetCollection<User>("users").AsQueryable<User>(); }
        }

        public User GetUser(string userid)
        {
            return db.GetCollection<User>("users").FindOneById(ObjectId.Parse(userid));
        }

        public User GetUserByEmail(string userEmailAddress, string userPassword)
        {
            var users = db.GetCollection<User>("users");

            var query = Query.And(
                Query.EQ("Email", userEmailAddress),
                Query.EQ("Password", userPassword)
                );

            return users.FindOne(query);
        }

        public User CreateUser(User user)
        {
            user.Email = user.Email.ToLower();

            var userExists = GetUserByEmail(user.Email, user.Password);

            if (userExists != null)
            {
                throw new Exception("User already exists!");
            }

            return SaveUser(user);
        }

        public User SaveUser(User user)
        {
            user.Email = user.Email.ToLower();
            var users = db.GetCollection<User>("users");
            users.Save(user);
            return user;
        }

        public void DeleteUser(string id)
        {
            var users = db.GetCollection<User>("users");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            users.Remove(query);
        }

        public void DeleteUser(string userEmailAddress, string userPassword)
        {
            userEmailAddress = userEmailAddress.ToLower();

            var users = db.GetCollection<User>("users");

            var query = Query.And(
                Query.EQ("Email", userEmailAddress),
                Query.EQ("Password", userPassword)
                );

            users.Remove(query);
        }

        public bool DoesAccountExist(string userEmailAddress, string userPassword)
        {
            userEmailAddress = userEmailAddress.ToLower();

            var users = db.GetCollection<User>("users");

            var query = Query.And(
                Query.EQ("Email", userEmailAddress),
                Query.EQ("Password", userPassword)
                );

            var user = users.FindOne(query);

            if (user != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool DoesUserNameExist(string userEmailAddress)
        {
            userEmailAddress = userEmailAddress.ToLower();

            var users = db.GetCollection<User>("users");
            var user = users.FindOne(Query.EQ("Email", userEmailAddress));

            return !Object.Equals(null, user);

        }

        public IQueryable<Profile> Profiles
        {
            get { return db.GetCollection<Profile>("profiles").AsQueryable<Profile>(); }
        }

        public Profile SaveProfile(Profile profile)
        {
            var profiles = db.GetCollection<Profile>("profiles");
            profiles.Save(profile);
            return profile;
        }

        public void DeleteProfile(string id)
        {
            var profiles = db.GetCollection<Profile>("profiles");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            profiles.Remove(query);
        }
    }
}
