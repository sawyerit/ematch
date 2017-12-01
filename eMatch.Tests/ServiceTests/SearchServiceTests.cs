using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eMatch.Engine.Services;
using eMatch.Engine.Enitities.Offers;
using System.Linq;

namespace eMatch.Tests.ServiceTests
{
    /// <summary>
    /// Summary description for SearchServiceTests
    /// </summary>
    [TestClass]
    public class SearchServiceTests
    {
        public SearchServiceTests()
        {
            //so far:

            //Find by word in [keyword, description, offer name, category]
            //find by phrase, by multiple keywords in ^
            //find by stem ("drinking" -> finds "drink")
            //returns results with ranking

            //fuzzy query would be nice, may not be doable with multiple word/phrase searches
            //May need to perform multiple searches... normal seach, fuzzysearch, [daterange search,] [proximity search] and then combine
        }

        [TestInitialize]
        public void Setup()
        {
            SearchService.ClearAllLuceneOffers();
        }


        [TestMethod]
        public void Add_Offer_To_Index_FindMultipleKeywordsOR_FinallyRemove()
        {
            Offer offer = MongoDbTestUtil.CreateOffer(null, null, 10, Offer.StatusType.Active);
            SearchService.AddUpdateLuceneIndex(offer);

            List<Offer> results = SearchService.Search("gojuice, coffee").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0]);
            Assert.AreEqual(results[0].Name, "Duck n Coffee discount");

            SearchService.ClearLuceneOfferRecord(offer.Id);

            results = SearchService.Search("gojuice").ToList();

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Add_Offer_To_Index_FindPhrase_FinallyRemove()
        {
            Offer offer = MongoDbTestUtil.CreateOffer(null, null, 10, Offer.StatusType.Active);
            SearchService.AddUpdateLuceneIndex(offer);

            //matches the phrase searched for
            List<Offer> results = SearchService.Search("get some").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0]);
            Assert.AreEqual(results[0].Name, "Duck n Coffee discount");

            //no match for this phrase
            results = SearchService.Search("get more").ToList();
            Assert.AreEqual(0, results.Count);

            //matches on gojuice, but not get more
            results = SearchService.Search("get more, gojuice").ToList();
            Assert.AreEqual(1, results.Count);


            SearchService.ClearLuceneOfferRecord(offer.Id);
            //record is deleted
            results = SearchService.Search("get some").ToList();

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Add_Offer_To_Index_FindByDescription_FinallyRemove()
        {
            Offer offer = MongoDbTestUtil.CreateOffer(null, null, 10, Offer.StatusType.Active);

            SearchService.AddUpdateLuceneIndex(offer);

            List<Offer> results = SearchService.Search("20%").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0]);
            Assert.AreEqual(results[0].Name, "Duck n Coffee discount");

            SearchService.ClearLuceneOfferRecord(offer.Id);

            results = SearchService.Search("20%").ToList();

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Add_Offer_To_Index_FindByStemmingAndRemove()
        {
            //searching for "drinking" will return anything matching "drink"
            Offer offer = MongoDbTestUtil.CreateStemmingOffer(null, null, 10, Offer.StatusType.Active);
            SearchService.AddUpdateLuceneIndex(offer);

            List<Offer> results = SearchService.Search("drinking").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0]);
            Assert.AreEqual(results[0].Name, "Early Bird Drink Special");

            SearchService.ClearLuceneOfferRecord(offer.Id);

            results = SearchService.Search("drinking").ToList();

            Assert.AreEqual(0, results.Count);
        }
    }
}
