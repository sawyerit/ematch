using System.Collections.Generic;
using System.Linq;
using eMatch.Data.Mongo;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services;
using eMatch.Engine.Services.Interfaces;
using Lucene.Net.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Moq;

namespace eMatch.Tests.ServiceTests
{
    [TestClass]
    public class MatchServiceTests
    {
        private IUserRepository _userRepo;
        private IOfferRepository _offerRepo;
        private MatchService _match;
     

        [TestInitialize]
        public void SetUp()
        {
            _userRepo = new Mock<MongoUserRepo>(MockBehavior.Strict).Object;
            _offerRepo = new Mock<MongoOfferRepo>(MockBehavior.Strict).Object;

            var mockUserService = new Mock<UserService>(_userRepo);

            mockUserService.Setup(x => x.GetProfile(It.IsAny<string>())).Returns(It.IsAny<Profile>);

            var mockOfferService = new Mock<OfferService>(_offerRepo);
            
            _match = new MatchService(mockUserService.Object, mockOfferService.Object);
        }


        //[TestMethod, TestCategory("Unit Test"), TestCategory("Service")]
        public void First_Match_Service_Test()
        {
            //TODO: Working on this!

            //Arrange
            //Act
            var matchingOffers = _match.GetActiveMatchesForUser(ObjectId.GenerateNewId().ToString());

            //Assert
            Assert.IsTrue(matchingOffers.Count == 1, "We should only have found 1 match offer but the offer count is wrong.");
        }


    }
}
