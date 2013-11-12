using System;
using Extensions;

namespace eMotive.Models.Objects.Search
{
    public class BasicSearch
    {
        private int? page;

        public BasicSearch()
        {
            page = 1;
            PageSize = 10;
        }

        public int NumberOfResults { get; set; }
        public int PageSize { get; set; }
        public int? Page
        {
            get { return page; }
            set
            {
                if (!value.HasValue || !value.Value.IsNumeric())
                {
                    page = 1;
                    return;
                }

                if (value <= 0)
                {
                    page = 1;
                    return;
                }

                if (NumberOfResults > 0 && value > TotalPages)
                {
                    page = TotalPages;
                    return;
                }

                page = value;
            }
        }

        public string[] Type { get; set; }

        public string Query { get; set; }

        public string Error { get; set; }

        public string ItemType { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)NumberOfResults / PageSize); }
        }
    }
}
