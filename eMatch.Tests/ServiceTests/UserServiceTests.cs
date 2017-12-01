using System.Linq;
using System.Security.Cryptography.X509Certificates;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services;
using eMatch.Engine.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;

namespace eMatch.Tests.ServiceTests
{
    [TestClass]
    public class UserServiceTests
    {
        private User user1;
        Mock<IUserRepository> _userRepo = new Mock<IUserRepository>(MockBehavior.Strict);
        IUserService _userService;

        [TestInitialize]
        public void SetUp()
        {
            #region Setup Opjects
            user1 = new User()
            {
                Address = "123 Baker Street",
                City = "Westminster",
                Email = "joeBob@joebob.com",
                FirstName = "Joe",
                Id = ObjectId.GenerateNewId().ToString(),
                IsMerchant = false,
                LastName = "Bob",
                Password = "1234dude",
                Phone = "3031111122",
                State = "CA"
            };

            List<string> keywordsList = new List<string>();
            keywordsList.Add("joe");
            keywordsList.Add("caffine");

            Preference preference1 = new Preference()
            {
                Category = "Coffee",
                Keywords = keywordsList
            };

            List<Preference> preferences = new List<Preference>();
            preferences.Add(preference1);

            Profile profile1 = new Profile()
            {
                BusinessName = "MyBizzNezz",
                Id = ObjectId.GenerateNewId().ToString(),
                Preferences = preferences,
                UserId = ObjectId.GenerateNewId().ToString(),
                WebsiteURL = null,
            };

            Profile profile2 = new Profile()
            {
                BusinessName = "MyBizzNezz2",
                Id = ObjectId.GenerateNewId().ToString(),
                Preferences = preferences,
                UserId = ObjectId.GenerateNewId().ToString(),
                WebsiteURL = null,
            };

            List<Profile> profiles = new List<Profile>();
            profiles.Add(profile1);
            profiles.Add(profile2);

            #endregion Setup Opjects

            _userRepo.Setup(x => x.GetUser(It.IsAny<string>())).Returns((user1));
            _userRepo.Setup(x => x.SaveProfile(It.IsAny<Profile>())).Returns((Profile p) => p);
            _userRepo.Setup(x => x.SaveUser(It.IsAny<User>())).Returns((user1));
            _userRepo.SetupGet(x => x.Profiles).Returns(profiles.AsQueryable);
            _userRepo.Setup(x => x.CreateUser(user1)).Returns(It.IsAny<User>);
            _userService = new UserService(_userRepo.Object);
        }

        [TestMethod, TestCategory("Unit Test"), TestCategory("Service")]
        public void Can_Create_User_Profile()
        {
            //Arrange

            //Act
            _userService.CreateUser(user1);

            //Assert
            _userRepo.Verify(x => x.CreateUser(user1));
        }

        [TestMethod, TestCategory("Unit Test"), TestCategory("Service")]
        public void Can_Create_User_Profile_EmailAndSMS()
        {
            _userService.CreateUser(user1);
            user1.MyNotifications = Notifications.SMSAndEmail;

            _userRepo.Verify(x => x.CreateUser(It.IsAny<User>()));
            _userRepo.Verify(x => x.SaveProfile(It.IsAny<Profile>()));

            Assert.AreEqual(Notifications.Email, user1.MyNotifications & Notifications.Email);
            Assert.AreEqual(Notifications.SMS, user1.MyNotifications & Notifications.SMS);
            Assert.AreNotEqual(Notifications.MobileApp, user1.MyNotifications & Notifications.MobileApp);
        }


        [TestMethod, TestCategory("Unit Test"), TestCategory("Service")]
        public void Can_Add_Preference_To_Profile()
        {
            //Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var pref = new Preference
            {
                Category = "discounts",
                Keywords = new List<string> { "coffee", "gofastjuice" }
            };

            //Act
            _userService.AddPreference(pref, userId);

            //Assert
            _userRepo.Verify(x => x.SaveProfile(It.IsAny<Profile>()));
        }
    }
}
