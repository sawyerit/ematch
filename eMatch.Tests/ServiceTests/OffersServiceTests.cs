using System.Collections.Generic;
using System.Linq;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Services;
using eMatch.Engine.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace eMatch.Tests.ServiceTests
{
    [TestClass]
    public class OffersServiceTests
    {
        Mock<IOfferRepository> _offerRepo;
        IOfferService _offerService;
        List<Offer> _offers;

         [TestInitialize]
        public void SetUp()
        {
            _offerRepo = new Mock<IOfferRepository>(MockBehavior.Strict);
            _offers = new List<Offer>();
            _offers.Add(MongoDbTestUtil.CreateOffer(null, null, 10, Offer.StatusType.Active));
            _offerRepo.Setup(x => x.Offers).Returns(_offers.AsQueryable);
            _offerService = new OfferService(_offerRepo.Object);
        }


        [TestMethod, TestCategory("Unit Test"), TestCategory("Service")]
        public void Can_Match_An_Offer_By_Category_And_Keywords()
        {
            //Arrange
            //Act
            var matchingOffers = _offerService.GetOffers("discounts", new List<string>() { "coffee", "gofastjuice" });

            //Assert
            Assert.IsTrue(matchingOffers.Count == 1, "We should only have found 1 offer but the offer count is wrong.");
        }


    }
}
