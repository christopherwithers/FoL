using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Web;
using Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using eMotive.Search.Interfaces;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace eMotive.Search.Objects
{
    public class SearchManager : ISearchManager
    {
        private readonly Directory directory;
        private readonly Analyzer analyzer;
        private readonly IndexSearcher searcher;


        public SearchManager(string _indexLocation)
        {
            if (string.IsNullOrEmpty(_indexLocation))
                throw new FileNotFoundException("The lucene index could not be found.");

            var resolvedServerLocation = HttpContext.Current.Server.MapPath(string.Format("~{0}", _indexLocation));

            directory = FSDirectory.Open(new DirectoryInfo(resolvedServerLocation));

            searcher = new IndexSearcher(directory, true);
            //TODO: put this in search result? Check to see if ok for lifetime of singleton.
            analyzer = new StandardAnalyzer(Version.LUCENE_30);
        }

        //TODO: for custom search i.e. user, have searchObj build their own boolean query?
        public SearchResult DoSearch(Search _search)
        {
            //TODO: need to tidy this up, perhaps only initialise parser if _search.Query
            var items = new Collection<ResultItem>();
            try
            {
                var bq = new BooleanQuery();
                var parser = new QueryParser(Version.LUCENE_30, string.Empty, analyzer);
                if (!string.IsNullOrEmpty(_search.Query))
                {
                    
                    var query = parser.Parse(_search.Query);
                    bq = new BooleanQuery
                        {
                            {
                                parser.Parse(string.Format("+Title:{0}", query)), Occur.MUST
                            },
                            {
                                parser.Parse(string.Format("+Description:{0}", query)), Occur.MUST
                            }
                        };
                }
                else
                {
                    if(!_search.CustomQuery.HasContent())
                        throw new ArgumentException("Neither QUery or CustomQUery have been defined.");

                    bq = new BooleanQuery();
                    //TODO: need a way of passing in occur.must and occur.should
                    foreach (var query in _search.CustomQuery)
                    {
                        bq.Add(new BooleanClause(parser.Parse(string.Format("+{0}:{1}", query.Key, query.Value)), Occur.SHOULD));
                    }
                }

                var topDocs = searcher.Search(bq, 10000);
                
                if (topDocs.ScoreDocs.Length > 0)
                {
                    _search.NumberOfResults = topDocs.ScoreDocs.Length;// -1;

                    var page = _search.CurrentPage;// -1;

                    var first = page * _search.PageSize;
                    int last;

                    if (_search.NumberOfResults < first + _search.PageSize)
                    {
                        last = first + _search.PageSize;
                    }
                    else
                    {
                        last = _search.NumberOfResults;
                    }

                    for (var i = first; i < last; i++)
                    {
                        var scoreDoc = topDocs.ScoreDocs[i];

                        var score = scoreDoc.Score;

                        var docId = scoreDoc.Doc;

                        var doc = searcher.Doc(docId);

                        items.Add(new ResultItem
                        {
                            ID = Convert.ToInt32(doc.Get("ID")),
                            Title = doc.Get("name"),
                            Type = doc.Get("type"),
                            Description = doc.Get("description"),
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

}
    }

