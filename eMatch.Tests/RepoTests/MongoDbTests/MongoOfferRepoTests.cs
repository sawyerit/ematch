using System.IO;
using System.Linq;
using eMatch.Data.Mongo;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Accounts;
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Offers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace eMatch.Tests.RepoTests.MongoDbTests
{
    [TestClass]
    public class MongoOfferRepoTests
    {
        List<string> testIds;
        IOfferRepository _offerRepo;
        string _createdBy;

        [TestInitialize]
        public void SetUp()
        {
            testIds = new List<string>();
            _offerRepo = new MongoOfferRepo();
            _createdBy = ObjectId.GenerateNewId().ToString();
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (testIds != null)
            {
                foreach (var item in testIds)
                {
                    _offerRepo.DeleteCampaign(item);
                }
            }
        }

        [TestMethod, TestCategory("Unit Test"), TestCategory("MongoDB")]
        public void OfferCtor()
        {
            var repository = new MongoOfferRepo();
            Assert.IsNotNull(repository);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Create_And_Delete_Offer()
        {
            //Arrange
            var offer = MongoDbTestUtil.CreateOffer(null, _createdBy);
          
            //Act
            var offerSaved = _offerRepo.SaveOffer(offer);

            //Assert
            Assert.IsNotNull(offerSaved.Id);
            Assert.AreEqual(offer.Category, offerSaved.Category);

            //Act
            _offerRepo.DeleteOffer(offerSaved.Id);

            var offerDeleted = _offerRepo.Offers.FirstOrDefault(x => x.Id == offerSaved.Id);

            //Assert
            Assert.IsTrue(offerDeleted == null);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Expire_An_Offer()
        {
            //Arrange
            var offer = MongoDbTestUtil.CreateOffer(null, _createdBy);

            var offerSaved = _offerRepo.SaveOffer(offer);
            var firstTimeToExpire = _offerRepo.SaveOffer(offer).Expires;

            //Act
            _offerRepo.ExpireOffer(offerSaved.Id);

            var secondTimeToExpire = _offerRepo.Offers.FirstOrDefault(x => x.Id == offerSaved.Id).Expires;

            var compare = DateTime.Compare((DateTime)firstTimeToExpire, (DateTime)secondTimeToExpire);
            
            _offerRepo.DeleteOffer(offerSaved.Id);

            //Assert
            Assert.IsTrue(compare == 1);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Change_Offer_Status()
        {
            var offer = MongoDbTestUtil.CreateOffer(null, _createdBy);
            var offerSaved = _offerRepo.SaveOffer(offer);
            Assert.IsTrue(offerSaved.Status == Offer.StatusType.Pending);

            _offerRepo.InactivateOffer(offerSaved.Id);
            var offerInactive = _offerRepo.Offers.FirstOrDefault(x => x.Id == offerSaved.Id);
            Assert.IsTrue(offerInactive.Status == Offer.StatusType.Inactive);

            _offerRepo.ActivateOffer(offerSaved.Id);
            var offerActivated = _offerRepo.Offers.FirstOrDefault(x => x.Id == offerSaved.Id);
            Assert.IsTrue(offerActivated.Status == Offer.StatusType.Active);

            _offerRepo.DeleteOffer(offerSaved.Id);
        }

        [TestMethod, TestCategory("Integration Test"), TestCategory("MongoDB")]
        public void Can_Create_And_Delete_Campaign()
        {
            //Arrange
            var offerActive = MongoDbTestUtil.CreateOffer(null, _createdBy, 10, Offer.StatusType.Active);
            var offerExpired = MongoDbTestUtil.CreateOffer(null, _createdBy, -10, Offer.StatusType.Active);
            var offerPending = MongoDbTestUtil.CreateOffer(null,_createdBy);

            var campaign = new Campaign
            {
                CurrentOffers = new List<Offer>() {offerActive},
                ExpiredOffers = new List<Offer>() {offerExpired},
                PendingOffers = new List<Offer>() {offerPending},
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = ObjectId.GenerateNewId().ToString()
            };

            //Act
            var c = _offerRepo.SaveCampaign(campaign);

            testIds.Add(campaign.Id);

            //Assert
            Assert.IsNotNull(c.Id);
            Assert.AreEqual(campaign.Id, c.Id);

            _offerRepo.DeleteCampaign(campaign.Id);

            var campaignDeleted = _offerRepo.Campaigns.FirstOrDefault(x => x.Id == campaign.Id);
            Assert.IsTrue(campaignDeleted == null);
        }


    }
}
