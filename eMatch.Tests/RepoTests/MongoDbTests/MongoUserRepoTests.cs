using System;
using System.Linq;
using eMatch.Data.Mongo;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System.Collections.Generic;

namespace eMatch.Tests.RepoTests.MongoDbTests
{
    [TestClass]
    public class MongoUserRepoTests
    {
        IUserRepository _repo;

        [TestInitialize]
        public void SetUp()
        {
            _repo = new MongoUserRepo();
        }

        [TestMethod, TestCategory("Unit Test"), TestCategory("MongoDB")]
        public void OfferCtor()
        {
            var repository = new MongoUserRepo();
            Assert.IsNotNull(repository);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Save_User()
        {
            //Arrange
            User user1 = new User
            {
                Address = "1234 West Apple Dr",
                City = "Westminster",
                State = "CO",
                Email = "JEBtester@jeb.com",
                FirstName = "Jeb",
                LastName = "Baz",
                Id = ObjectId.GenerateNewId().ToString(),
                IsMerchant = false,
                Password = "dude",
                Phone = "3035551212"
            };

            bool exists = _repo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _repo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _repo.SaveUser(user1);

            //Email address when saved are converted to lower case.
            Assert.AreEqual(user.Email, user1.Email.ToLower());


            //Act
            var userGet = _repo.GetUser(user.Id);
            var userGetFromEmail = _repo.GetUserByEmail(user.Email, user.Password);

            //Assert
            Assert.AreEqual(user1.Id, userGet.Id);
            Assert.AreEqual(user1.Id, userGetFromEmail.Id);

            _repo.DeleteUser(user.Id);

            userGet = _repo.GetUser(user.Id);
            Assert.IsNull(userGet);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Create_User()
        {
            //Arrange
            User user1 = new User
            {
                Address = "1234 West Apple Dr",
                City = "Westminster",
                State = "CO",
                Email = "JEBtester@jeb.com",
                FirstName = "Jeb",
                LastName = "Baz",
                Id = ObjectId.GenerateNewId().ToString(),
                IsMerchant = false,
                Password = "dude",
                Phone = "3035551212"
            };


            bool exists = _repo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _repo.DeleteUser(user1.Email, user1.Password);
            }
            
            var user = _repo.CreateUser(user1);

            //Email address when saved are converted to lower case.
            Assert.AreEqual(user.Email, user1.Email.ToLower());


            //Act
            var userGet = _repo.GetUser(user.Id);
            var userGetFromEmail = _repo.GetUserByEmail(user.Email, user.Password);

            //Assert
            Assert.AreEqual(user1.Id, userGet.Id);
            Assert.AreEqual(user1.Id, userGetFromEmail.Id);

            _repo.DeleteUser(user.Id);

            userGet = _repo.GetUser(user.Id);
            Assert.IsNull(userGet);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Save_Duplicated_User_And_Delete_User()
        {
            //Arrange
            User user1 = new User
            {
                Address = "1234 West Apple Dr",
                City = "Westminster",
                State = "CO",
                Email = "JEBtester@jeb.com",
                FirstName = "Jeb",
                LastName = "Baz",
                Id = ObjectId.GenerateNewId().ToString(),
                IsMerchant = false,
                Password = "dude",
                Phone = "3035551212"
            };

            var user = _repo.SaveUser(user1);

            bool exists = _repo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _repo.DeleteUser(user1.Email, user1.Password);
            }

            //Act
            var userGet = _repo.GetUser(user.Id);
            var userGetFromEmail = _repo.GetUserByEmail(user.Email, user.Password);

            //Assert
            Assert.AreEqual(userGet, null);
            Assert.AreEqual(userGetFromEmail, null);

            _repo.DeleteUser(user.Id);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Create_Duplicate_User_Throws_Exception()
        {
            var user1 = _repo.CreateUser(MongoDbTestUtil.CreateUserCustomer());

            try
            {
                _repo.CreateUser(MongoDbTestUtil.CreateUserCustomer());
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "User already exists!");
                _repo.DeleteUser(user1.Id);
                throw;
            }
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Get_UserById_AndEmail_And_Delete()
        {
            //Arrange
            User user1 = MongoDbTestUtil.CreateUserCustomer();
            bool exists = _repo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _repo.DeleteUser(user1.Email, user1.Password);
            }
           
            var user = _repo.SaveUser(user1);

            //Act
            var userGet = _repo.GetUser(user.Id);
            var userGetFromEmail = _repo.GetUserByEmail(user.Email, user.Password);

            //Assert
            Assert.AreEqual(user1.Id, userGet.Id);
            Assert.AreEqual(user1.Id, userGetFromEmail.Id);

            _repo.DeleteUser(user.Id);

            userGet = _repo.GetUser(user.Id);
            Assert.IsNull(userGet);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Create_A_Profile_With_Preferences()
        {
            //Arrange
            var profile = MongoDbTestUtil.CreateProfileWithPreferences();

            //Act
            var p = _repo.SaveProfile(profile);

            //Assert
            Assert.IsNotNull(p.Id);
            Assert.AreEqual(p.UserId, profile.UserId);
            Assert.AreEqual(p.Preferences, profile.Preferences);

            _repo.DeleteProfile(p.Id);
            var profileDeleted = _repo.Profiles.FirstOrDefault(x => x.Id == p.Id);
            Assert.IsTrue(profileDeleted == null);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Create_And_Delete_Profile()
        {
            //Arrange
            var profile = MongoDbTestUtil.CreateProfileWithoutPreferences();

            //Act
            var p = _repo.SaveProfile(profile);

            //Assert
            Assert.IsNotNull(p.Id);
            Assert.AreEqual(p.UserId, profile.UserId);

            _repo.DeleteProfile(p.Id);
            var profileDeleted = _repo.Profiles.FirstOrDefault(x => x.Id == p.Id);
            Assert.IsTrue(profileDeleted == null);
        }


        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Match()
        {
            //Arrange
            IOfferRepository offerRepository = new MongoOfferRepo();
            
            var user = _repo.SaveUser(MongoDbTestUtil.CreateUserCustomer());

            var profile = MongoDbTestUtil.CreateProfileWithPreferences(user.Id);

            //Act
            var p = _repo.SaveProfile(profile);

            //Assert
            Assert.IsNotNull(p.Id);
            Assert.AreEqual(p.UserId, profile.UserId);
            Assert.AreEqual(p.Preferences, profile.Preferences);

            _repo.DeleteProfile(p.Id);
            var profileDeleted = _repo.Profiles.FirstOrDefault(x => x.Id == p.Id);
            Assert.IsTrue(profileDeleted == null);

            _repo.DeleteUser(user.Id);

            var userGet = _repo.GetUser(user.Id);
            Assert.IsNull(userGet);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Get_All_Users()
        {
            //Act
            var users = _repo.Users.ToList();
            Assert.IsNotNull(users);
        }




    }
}
