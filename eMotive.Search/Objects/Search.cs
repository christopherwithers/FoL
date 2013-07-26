using System.Collections.Generic;

namespace eMotive.Search.Objects
{
    public class Search
    {
        public int NumberOfResults { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public string Query { get; set; }

        public string Error { get; set; }

        public IDictionary<string, string> CustomQuery { get; set; }
    }
}