using eMatch.Engine.Enitities.Users;
using System.Linq;

namespace eMatch.Engine.Data
{
    public interface IUserRepository
    {
        IQueryable<User> Users { get; }
        IQueryable<Profile> Profiles { get; }

        User SaveUser(User user);
        User GetUser(string userid);
        User GetUserByEmail(string email, string password);
        User CreateUser(User user);

        Profile SaveProfile(Profile profile);
        bool DoesAccountExist(string userEmailAddress, string userPassword);
        bool DoesUserNameExist(string userEmailAddress);

        void DeleteUser(string id);
        void DeleteUser(string userEmailAddress, string userPassword);
        void DeleteProfile(string id);

    }
}
