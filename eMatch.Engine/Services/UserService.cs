using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services.Interfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eMatch.Engine.Services
{
    public class UserService : IUserService
    {

        IUserRepository _user;

        public UserService(IUserRepository user)
        {
            _user = user;
        }

        public User GetUser(string id)
        {
            return _user.GetUser(id);
        }

        public User GetUserByEmailAndPwd(string email, string password)
        {
            return _user.GetUserByEmail(email, password);
        }

        public bool DoesEmailExist(string email)
        {
            return _user.DoesUserNameExist(email);
        }

        public Profile CreateUser(User user)
        {
            user.MyMemberLevel = MembershipLevel.Basic;
            user.MyNotifications = Notifications.Email;

            _user.CreateUser(user);
            return CreateProfile(user.Id.ToString());
        }

        public User UpdateUser(User user)
        {
            return _user.SaveUser(user);
        }

        public string GetProfileID(string userId)
        {
            return _user.Profiles.Where(x => x.UserId == userId).FirstOrDefault().Id;
        }

        public Profile GetProfile(string userId)
        {
            return _user.Profiles.Where(x => x.UserId == userId).FirstOrDefault();
        }

        public Profile CreateProfile(string userId)
        {
            return CreateProfile(userId, null);
        }

        public Profile CreateProfile(string userId, List<Preference> preferences)
        {
            var user = GetUser(userId);
            if (user == null) throw new Exception("User does not exist");
            var profile = new Profile { UserId = user.Id };
            if (preferences != null) profile.Preferences.AddRange(preferences);
            return _user.SaveProfile(profile);
        }

        public void AddPreference(Preference preference, string userId)
        {
            var profile = GetProfile(userId);
            if (profile == null) profile = CreateProfile(userId);
            profile.Preferences.Add(preference);
            _user.SaveProfile(profile);
        }
        
        public Profile SaveProfile(Profile profile)
        {
            return _user.SaveProfile(profile);
        }
    }
}
