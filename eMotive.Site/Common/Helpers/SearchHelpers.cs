using System;
using System.Web.Mvc;
using Extensions;
using eMotive.Models.Objects.Search;

namespace eMotive.FoL.Common.Helpers
{
    public static class SearchHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper _helper, BasicSearch _paging, Func<int, string> _pageUrl)
        {
            var sb = new System.Text.StringBuilder();
            //   TagBuilder tag;
            if (!_paging.Page.IsNumeric())
                _paging.Page = 1;

            if (_paging.Page < 1)
                _paging.Page = 1;

            if (_paging.Page > _paging.TotalPages)
                _paging.Page = 1;// _paging.TotalPages;

            const int grace = 5;
            const int range = grace * 2;
            var totalPages = _paging.TotalPages;
            var start = (_paging.Page - grace) > 0 ? (_paging.Page - grace) : 1;
            var end = start + range;
            var search = string.Empty;

            if (!string.IsNullOrEmpty(_paging.Query))
                search = string.Concat("&Query=", _paging.Query);

            if (end > totalPages)
            {
                end = totalPages;
                start = (end - range) > 0 ? (end - range) : 1;
            }

            if (start > 1)
            {
              //  sb.Append("<a href='"); sb.Append(_pageUrl(1)); sb.Append(search); sb.Append("'>First</a>&nbsp;&nbsp;");
                sb.Append("<a href='"); sb.Append(_pageUrl(1)); sb.Append(search); sb.Append("'>1</a> ...");
            }

            for (var i = start; i <= end; i++)
            {
                if (i == _paging.Page)
                {
                    sb.Append("<span>"); sb.Append(i); sb.Append("</span>&nbsp;&nbsp;");
                }
                else
                {
                    sb.Append("<a href='"); sb.Append(_pageUrl(i.Value)); sb.Append(search); sb.Append("'>"); sb.Append(i); sb.Append("</a>&nbsp;&nbsp;");
                }
            }

            if (end < totalPages)
            {
                sb.Append("... <a href='"); sb.Append(_pageUrl(totalPages)); sb.Append(search); sb.Append("'>"); sb.Append(totalPages); sb.Append("</a>");
            }

            sb.Append("<div>"); sb.Append(_paging.NumberOfResults); sb.Append(" "); sb.Append(_paging.ItemType); sb.Append(" found. Displaying page ");
            sb.Append(_paging.Page); sb.Append(" of "); sb.Append(totalPages); sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}