using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using eMotive.Search.Interfaces;
using Version = Lucene.Net.Util.Version;

namespace eMotive.Search.Objects
{
    public class SearchManager : ISearchManager, IDisposable
    {
        private readonly FSDirectory directory;
        private static IndexWriter writer;
        private readonly Analyzer analyzer;
        private IndexSearcher searcher;
        private readonly Version luceneVersion;


        public SearchManager(string _indexLocation)
        {
            if(string.IsNullOrEmpty(_indexLocation))
                throw new FileNotFoundException("The lucene index could not be found.");

            luceneVersion = Version.LUCENE_30;

            var resolvedServerLocation = HttpContext.Current.Server.MapPath(string.Format("~{0}", _indexLocation));
            directory = FSDirectory.Open(new DirectoryInfo(resolvedServerLocation));
            writer = new IndexWriter(directory, new StandardAnalyzer(luceneVersion), false, IndexWriter.MaxFieldLength.UNLIMITED);
            analyzer = new PerFieldAnalyzerWrapper(new StandardAnalyzer(luceneVersion));
        }

        public SearchResult DoSearch(Search _search)
        {
            //todo: is this needed? i.e. dispose first

            searcher = new IndexSearcher(writer.GetReader());

            //TODO: need to tidy this up, perhaps only initialise parser if _search.Query
            var items = new Collection<ResultItem>();
            try//todo: do i need to make title DocumentTITLE AND UNALAYZED AGAIN - thenadd an analyzed title in? YESSSSSSSSSSS
            {
                var bq = new BooleanQuery();
                var parser = new QueryParser(luceneVersion, string.Empty, analyzer);
                if (!string.IsNullOrEmpty(_search.Query) && !_search.CustomQuery.HasContent())
                {
                    var query = parser.Parse(_search.Query);
                    bq = new BooleanQuery
                        {
                            {
                                parser.Parse(string.Format("Title:{0}", query)), Occur.MUST
                            },
                            {
                                parser.Parse(string.Format("Description:{0}", query)), Occur.MUST
                            }
                        };
                }
                else
                {
                    if (!_search.CustomQuery.HasContent())
                        throw new ArgumentException("Neither Query or CustomQuery have been defined.");

                    bq = new BooleanQuery();
                    //TODO: need a way of passing in occur.must and occur.should
                    foreach (var query in _search.CustomQuery.Where(n => !string.IsNullOrEmpty(n.Value.Field)))
                    {
                        bq.Add(new BooleanClause(parser.Parse(string.Format("{0}:{1}", query.Key, query.Value.Field)), query.Value.Term));
                    }
                }
                TopDocs docs;
                if (_search.Type.HasContent())
                {
                    var filterBq = new BooleanQuery();
                    foreach (var type in _search.Type)
                    {
                        filterBq.Add(new BooleanClause(parser.Parse(string.Format("Type:{0}", type)), Occur.MUST));
                    }
                    var test = new QueryWrapperFilter(filterBq);
                    docs = searcher.Search(bq, test, 10000);
                }
                else
                {
                    docs = searcher.Search(bq, 10000);
                }



                if (docs.ScoreDocs.Length > 0)
                {
                    _search.NumberOfResults = docs.ScoreDocs.Length;// -1;

                    var page = _search.CurrentPage - 1;

                    var first = page * _search.PageSize;
                    int last;

                    if (_search.NumberOfResults > first + _search.PageSize)
                    {
                        last = first + _search.PageSize;
                    }
                    else
                    {
                        last = _search.NumberOfResults;
                    }

                    for (var i = first; i < last; i++)
                    {
                        var scoreDoc = docs.ScoreDocs[i];

                        var score = scoreDoc.Score;

                        var docId = scoreDoc.Doc;

                        var doc = searcher.Doc(docId);

                        items.Add(new ResultItem
                        {
                            ID = Convert.ToInt32(doc.Get("DatabaseID")),
                            Title = doc.Get("Title"),
                            Type = doc.Get("Type"),
                            Description = doc.Get("Description"),
                            Score = score
                        });
                    }
                }
            }
            catch (ParseException)
            {

                _search.Error = "The search query was malformed. For help with searching, please click the help link.";
            }
            catch
            {
                _search.Error = "An error occured. Please try again.";
            }

            searcher.Dispose();
            searcher = null;
          //  reader.Dispose();
            return new SearchResult
            {
                CurrentPage = _search.CurrentPage,
                Error = _search.Error,
                NumberOfResults = _search.NumberOfResults,
                PageSize = _search.PageSize,
                Query = _search.Query,
                Items = items
            };
        }


        public bool Add(ISearchDocument _document)
        {
            var success = true;
            try
            {
                var doc = _document.BuildRecord();
                writer.AddDocument(doc);
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Update(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.UpdateDocument(new Term("UniqueID", _document.UniqueID), _document.BuildRecord());
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Delete(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.DeleteDocuments(new Term("UniqueID", _document.UniqueID));
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
            
        }

        public void DeleteAll()
        {
            writer.DeleteAll();
            writer.Commit();
        }

        public int NumberOfDocuments()
        { 
            var reader = writer.GetReader();

            var numDocs = reader.NumDocs();

            reader.Dispose();

            return numDocs;
        }

        public void Dispose()
        {
            writer.Commit();

            writer.Dispose();
            directory.Dispose();
        }
    }
}
