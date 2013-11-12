using System.Collections.Generic;
using Lucene.Net.Search;

namespace eMotive.Search.Objects
{
    public class Search
    {
        public class SearchTerm
        {
            public string Field { get; set; }
            public Occur Term { get; set; }
        }

        public Search()
        {
            CurrentPage = 1;
            PageSize = 10;
        }

        public int NumberOfResults { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public string[] Type { get; set; }

        public string Query { get; set; }

        public string Error { get; set; }

        public IDictionary<string, SearchTerm> CustomQuery { get; set; }
    }
}