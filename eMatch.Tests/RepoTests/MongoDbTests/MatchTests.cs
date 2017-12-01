using System;
using System.Linq;
using eMatch.Data.Mongo;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System.Collections.Generic;

namespace eMatch.Tests.RepoTests.MongoDbTests
{
    [TestClass]
    public class MatchTests
    {
        private IUserRepository _userRepo;
        private IOfferRepository _offerRepo;
        private MatchService _match;
        private OfferService _offerService;
        private UserService _userService;

        [TestInitialize]
        public void SetUp()
        {
            SearchService.ClearAllLuceneOffers();

            _userRepo = new MongoUserRepo();
            _offerRepo = new MongoOfferRepo();

            _userService = new UserService(_userRepo);
            _offerService = new OfferService(_offerRepo);

            _match = new MatchService(_userService, _offerService);
        }

        [TestMethod, TestCategory("Integration Test")]
        public void Get_Active_Matching_Offers_For_User()
        {
            //Arrange
            var offerSaved = _offerService.CreateOffer(MongoDbTestUtil.CreateOffer("Little Jebbies 5 Cent Coffee"));
            _offerService.ActivateOffer(offerSaved.Id);

            var offerSaved2 = _offerService.CreateOffer(MongoDbTestUtil.CreateOffer());

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId);

            var offersFound = matchingOffers.FirstOrDefault().Value;

            //Assert
            Assert.IsTrue(offersFound.Count == 1);

            var matchingOffer = offersFound.FirstOrDefault();

            Assert.IsTrue(matchingOffer.Name == "Little Jebbies 5 Cent Coffee");
            Assert.IsTrue(matchingOffer.Category == "discounts");

            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
            _offerRepo.DeleteOffer(offerSaved2.Id);
        }

        [TestMethod, TestCategory("Integration Test")]
        public void Get_All_Offers_For_User()
        {
            //Arrange
            var offerSaved = _offerService.CreateOffer(MongoDbTestUtil.CreateOffer("Jebbies Discount"));
            _offerService.ActivateOffer(offerSaved.Id);

            var offerSaved2 = _offerService.CreateOffer(MongoDbTestUtil.CreateOffer());

            var offerSaved3 = _offerService.CreateOffer(MongoDbTestUtil.CreateOffer());

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId));

            //Act
            var matchingOffers = _match.GetMatchesForUser(user.UserId);

            var offersFound = matchingOffers.FirstOrDefault().Value;

            //Assert
            Assert.IsTrue(offersFound.Count == 3);

            var matchingOffer = offersFound.FirstOrDefault();

            Assert.IsTrue(matchingOffer.Name == "Jebbies Discount");
            Assert.IsTrue(matchingOffer.Category == "discounts");

            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
            _offerRepo.DeleteOffer(offerSaved2.Id);
            _offerRepo.DeleteOffer(offerSaved3.Id);
        }


        [TestMethod, TestCategory("Integration Test")]
        public void Match_On_Category_Only()
        {
            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString(),
                Category = "Books",
                CreatedBy = ObjectId.GenerateNewId().ToString(),
                Description = null,
                Expires = DateTime.Now.AddDays(40),
                Keywords = new List<string>
                {
                },
                Name = "Sale",
                Status = Offer.StatusType.Pending,
                IsRecurring = false
            };

            var offerSaved = _offerService.CreateOffer(offer);
            _offerService.ActivateOffer(offerSaved.Id);

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId, "Books", null));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId);

            var offersFound = matchingOffers.FirstOrDefault().Value;

            //Assert
            Assert.IsTrue(offersFound.Count == 1);

            var matchingOffer = offersFound.FirstOrDefault();

            Assert.IsTrue(matchingOffer.Name == "Sale");
            Assert.IsTrue(matchingOffer.Category == "Books");

            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
        }

        [TestMethod, TestCategory("Integration Test")]
        public void Matching_On_Partial_Category_Returns_No_Results_As_Expected()
        {
            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString(),
                Category = "Books",
                CreatedBy = ObjectId.GenerateNewId().ToString(),
                Description = null,
                Expires = DateTime.Now.AddDays(40),
                Keywords = new List<string>
                {
                },
                Name = "Sale",
                Status = Offer.StatusType.Pending,
                IsRecurring = false
            };

            var offerSaved = _offerService.CreateOffer(offer);
            _offerService.ActivateOffer(offerSaved.Id);

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId, "ooks", null));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId).FirstOrDefault().Value.Count;
            
            //Assert
            Assert.IsTrue(matchingOffers == 0);
            
            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
        }
        
        [TestMethod, TestCategory("Integration Test")]
        public void Matching_On_Keywords_Only_Returns_No_Results_As_Expected()
        {
            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString(),
                Category = "",
                CreatedBy = ObjectId.GenerateNewId().ToString(),
                Description = null,
                Expires = DateTime.Now.AddDays(40),
                Keywords = new List<string>
                {
                    "Weightlifting"
                },
                Name = "Dumbbell Sale",
                Status = Offer.StatusType.Pending,
                IsRecurring = false
            };

            var offerSaved = _offerService.CreateOffer(offer);
            _offerService.ActivateOffer(offerSaved.Id);

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId, "", new List<string> { "Weightlifting" }));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId).FirstOrDefault().Value.Count;
            Assert.IsTrue(matchingOffers == 0);
            
            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
        }

        [TestMethod, TestCategory("Integration Test")]
        public void Matching_On_Different_Category_But_Same_Keywords_Returns_No_Results()
        {
            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString(),
                Category = "Books",
                CreatedBy = ObjectId.GenerateNewId().ToString(),
                Description = null,
                Expires = DateTime.Now.AddDays(40),
                Keywords = new List<string>
                {
                    "cookies"
                },
                Name = "Dumbbell Sale",
                Status = Offer.StatusType.Pending,
                IsRecurring = false
            };

            var offerSaved = _offerService.CreateOffer(offer);
            _offerService.ActivateOffer(offerSaved.Id);

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId, "Food", new List<string> { "cookies" }));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId).FirstOrDefault().Value.Count;
            Assert.IsTrue(matchingOffers == 0);

            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
        }

        [TestMethod, TestCategory("Integration Test")]
        public void Match_On_Partial_Keyword()
        {
            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString(),
                Category = "Books",
                CreatedBy = ObjectId.GenerateNewId().ToString(),
                Description = null,
                Expires = DateTime.Now.AddDays(40),
                Keywords = new List<string>
                {
                    "weightlifting"
                },
                Name = "Sale",
                Status = Offer.StatusType.Pending,
                IsRecurring = false
            };

            var offerSaved = _offerService.CreateOffer(offer);
            _offerService.ActivateOffer(offerSaved.Id);

            User user1 = MongoDbTestUtil.CreateUserCustomer();

            bool exists = _userRepo.DoesAccountExist(user1.Email, user1.Password);

            if (exists)
            {
                _userRepo.DeleteUser(user1.Email, user1.Password);
            }

            var user = _userService.CreateUser(user1);

            var profileId = _userService.GetProfileID(user.UserId);

            var savedProfile = _userService.SaveProfile(MongoDbTestUtil.CreateProfileWithPreferences(user.UserId, profileId, "Books", new List<string>{"lifting"}));

            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(user.UserId);

            var offersFound = matchingOffers.FirstOrDefault().Value;

            //Assert
            Assert.IsTrue(offersFound.Count == 1);

            var matchingOffer = offersFound.FirstOrDefault();

            Assert.IsTrue(matchingOffer.Name == "Sale");
            Assert.IsTrue(matchingOffer.Category == "Books");

            _userRepo.DeleteProfile(savedProfile.Id);
            var profileDeleted = _userRepo.Profiles.FirstOrDefault(x => x.Id == savedProfile.Id);
            Assert.IsTrue(profileDeleted == null);

            _userRepo.DeleteUser(user1.Email, user1.Password);

            var userGet = _userRepo.GetUser(user.Id);
            Assert.IsNull(userGet);

            //Delete offers
            _offerRepo.DeleteOffer(offerSaved.Id);
        }




    }
}
