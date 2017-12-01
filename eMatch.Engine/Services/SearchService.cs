using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using eMatch.Engine.Enitities.Offers;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis;
using Lucene.Net.Search.Spans;

namespace eMatch.Engine.Services
{
    /// <summary>
    /// Currently using the RAMDirectory. If Azure will let us use the filesystem, that would be ideal. Just needs testing.
    /// </summary>
    public static class SearchService
    {
        //Notes: 
        //searching patterns, TODO: proximity searching 
        //http://www.lucenetutorial.com/lucene-query-syntax.html
        //Lucene can "boost" the query, so more weight can be given to the Name than the description
        //This can be expanded to include date range searching as well
        //http://www.leapinggorilla.com/Blog/Read/3/date-range-searches-in-lucene
        //Lucene.net contrib includes faceted searches (showing # results by category for furthur drill down),
        //spell checker auto correct options, and spatial searching if we include lat/long in the offer data.
        //http://www.leapinggorilla.com/Blog/Read/1005/spatial-search-in-lucenenet

        private static string _luceneDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "lucene_index");

        //currently using RAMDirectory.  rename _directory to _ramDirectory and then _fsdirectory to _directory to swtich to filesystem 
        private static FSDirectory _directoryTemp;
        private static FSDirectory _fsdirectory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        private static RAMDirectory _ramDirectoryTemp;
        private static RAMDirectory _directory
        {
            get
            {
                if (_ramDirectoryTemp == null) _ramDirectoryTemp = new RAMDirectory();
                return _ramDirectoryTemp;
            }
        }

        //These are the fields to index and search on. Update this to add or remove fields used in the Lucene document
        //private static string[] _fields = new[] { "Id", "Name", "Category", "Description", "Keywords" }; //expiredate, isrecurring?
        private static Dictionary<string, float> _fields = new Dictionary<string, float> { 
        {"Id", 0F},
        {"Name", 3F},
        {"Category", 2F},
        {"Description", 1F},
        {"Keywords", 1F}
        }; //expiredate, isrecurring?

        #region adds

        /// <summary>
        /// Adds/Updates the Offer object in the lucene index. Contains the mapping information for Offer -> Lucene Doc
        /// If a new field is added to the Offer that should be indexed, it needs to be changed here (as well as in the search methods)
        /// </summary>
        /// <param name="offerData">The Offer to index</param>
        /// <param name="writer">The Lucene index writer to use</param>
        private static void _addToLuceneIndex(Offer offerData, IndexWriter writer)
        {
            writer.UpdateDocument(new Term("Id", offerData.Id.ToString()), MapOfferToLuceneDocument(offerData));
        }

        public static void AddUpdateLuceneIndex(IEnumerable<Offer> offers)
        {
            // init lucene
            //var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var analyzer = new SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "English", StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                foreach (var offer in offers) _addToLuceneIndex(offer, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void AddUpdateLuceneIndex(Offer offer)
        {
            AddUpdateLuceneIndex(new List<Offer> { offer });
        }

        #endregion


        #region deletes

        /// <summary>
        /// Removes and indexed document from Lucene based on the Id. 
        /// The id field MUST be indexed, but does not have to be analyzed
        /// </summary>
        /// <param name="record_id"></param>
        public static void ClearLuceneOfferRecord(string record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        /// <summary>
        /// Clears all indexed items from the Lucene index
        /// </summary>
        /// <returns></returns>
        public static bool ClearAllLuceneOffers()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clears all expired items from the Lucene index
        /// </summary>
        /// <returns></returns>
        public static bool ClearAllExpiredLuceneOffers(List<string> expiredOfferIds)
        {
            try
            {
                foreach (string id in expiredOfferIds)
                {
                    ClearLuceneOfferRecord(id);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion


        #region helpers

        /// <summary>
        /// Maps the retrieved Lucene document to an Offer object
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Engine.Entities.Offers.Offer</returns>
        private static Offer MapLuceneDocumentToOffer(Document doc)
        {
            Offer o = new Offer();

            foreach (KeyValuePair<string, float> field in _fields)
                o.SetVal(field.Key, doc.Get(field.Key));

            return o;
        }

        /// <summary>
        /// Maps the Offer object to Lucene document for indexing
        /// </summary>
        /// <param name="offer"></param>
        /// <returns>Document</returns>
        private static Document MapOfferToLuceneDocument(Offer offerData)
        {
            // create new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields. If the Offer object changes, this has to be updated
            foreach (KeyValuePair<string, float> field in _fields)
            {
                Field newField = new Field(field.Key
                    , offerData.GetVal(field.Key)
                    , Field.Store.YES, field.Key == "Id" ? Field.Index.NOT_ANALYZED : Field.Index.ANALYZED);

                newField.Boost = field.Value;

                doc.Add(newField);
            }

            return doc;
        }

        private static IEnumerable<Offer> _mapLuceneToOfferList(IEnumerable<Document> hits)
        {
            return hits.Select(MapLuceneDocumentToOffer).ToList();
        }

        private static IEnumerable<Offer> _mapLuceneToOfferList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => MapLuceneDocumentToOffer(searcher.Doc(hit.Doc))).ToList();
        }

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        public static string GetVal(this Offer src, string propName)
        {
            var prop = src.GetType().GetProperty(propName);

            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return String.Join(" ", prop.GetValue(src, null) as List<string>);
            }
            else
            {
                object val = prop.GetValue(src);
                return !Object.Equals(null, val) ? val.ToString() : String.Empty;
            }
        }

        public static void SetVal(this Offer src, string propName, string value)
        {
            var prop = src.GetType().GetProperty(propName);

            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                prop.SetValue(src, value.Split(' ').ToList());
            else
                prop.SetValue(src, value);
        }

        #endregion


        #region parsing and querying

        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        /// <summary>
        /// not used yet. would be nice to find a way to implement this AND the multi query builder
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private static FuzzyQuery fuzzyParseQuery(string searchQuery)
        {
            var query = new FuzzyQuery(new Term("Keywords", searchQuery), 0.1f);
            return query;
        }

        private static IEnumerable<Offer> _search(string searchQuery, string searchField = "")
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<Offer>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, true))
            {
                var hits_limit = 1000;
                //var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                var analyzer = new SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "English", StopAnalyzer.ENGLISH_STOP_WORDS_SET);
                searcher.SetDefaultFieldSortScoring(true, true);

                // search by single field
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                    var results = _mapLuceneToOfferList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
                // search by multiple fields (ordered by RELEVANCE)
                else
                {
                    var parser = new MultiFieldQueryParser(Version.LUCENE_30, _fields.Keys.ToArray(), analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, null, hits_limit, Sort.RELEVANCE).ScoreDocs;
                    var results = _mapLuceneToOfferList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
            }
        }

        public static IEnumerable<Offer> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<Offer>();

            var terms = input.Trim()
                            .Replace("-", " ")
                            .Split(',')
                            .Where(x => !string.IsNullOrEmpty(x)).Select(x => "\"" + x.Trim() + "\"");

            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }


        public static IEnumerable<Offer> FuzzySearch(string input)
        {
            if (string.IsNullOrEmpty(input)) return new List<Offer>();

            var terms = input.Trim()
                            .Replace("-", " ")
                            .Split(',')
                            .Where(x => !string.IsNullOrEmpty(x)).Select(x => "\"" + x.Trim() + "\"");

            input = string.Join(" ", terms);

            return _fuzzysearch(input, "");
        }

        private static IEnumerable<Offer> _fuzzysearch(string searchQuery, string searchField)
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<Offer>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, true))
            {
                var hits_limit = 1000;
                searcher.SetDefaultFieldSortScoring(true, true);

                // search by single field for fuzzy
                var query = fuzzyParseQuery(searchQuery);
                var hits = searcher.Search(query, hits_limit).ScoreDocs;
                var results = _mapLuceneToOfferList(hits, searcher);
                searcher.Dispose();
                return results;
            }
        }

        #endregion


        /// <summary>
        /// Used for debugging and learning mostly
        /// </summary>        
        public static IEnumerable<Offer> SearchDefault(string input, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<Offer>() : _search(input, fieldName);
        }

        public static IEnumerable<Offer> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<Offer>();

            // set up lucene searcher
            var searcher = new IndexSearcher(_directory, false);
            var reader = IndexReader.Open(_directory, false);
            var docs = new List<Document>();
            var term = reader.TermDocs();
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToOfferList(docs);
        }

    }

}

