
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Users;
using System.Collections.Generic;

namespace eMatch.Engine.Services.Interfaces
{
    public interface IUserService
    {
        bool DoesEmailExist(string email);

        User GetUser(string id);
        User GetUserByEmailAndPwd(string email, string password);
        User UpdateUser(User user);

        Profile GetProfile(string userId);
        /// <summary>
        /// Creates the user object and inital profile object
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Profile</returns>
        Profile CreateUser(User user);
        Profile CreateProfile(string userId);
        Profile CreateProfile(string userId, List<Preference> preferences);
        Profile SaveProfile(Profile profile);

        void AddPreference(Preference preference, string userId);
        
        string GetProfileID(string userId);
    }
}
