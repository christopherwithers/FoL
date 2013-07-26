using eMotive.Managers.Interfaces;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;

namespace eMotive.Managers.Objects
{
    public class SiteSearchManager : ISiteSearchManager
    {
        private readonly ISearchDocumentManager docManager;
        private readonly ISearchManager searchManager;

        public SiteSearchManager(ISearchDocumentManager _docManager, ISearchManager _searchManager)
        {
            docManager = _docManager;
            searchManager = _searchManager;
        }

        public SearchResult DoSearch(eMotive.Search.Objects.Search _search)
        {
            return searchManager.DoSearch(_search);
        }

        public bool Add(ISearchDocument _document)
        {
            return docManager.Add(_document);
        }

        public bool Update(ISearchDocument _document)
        {
            return docManager.Update(_document);
        }

        public bool Delete(ISearchDocument _document)
        {
            return docManager.Delete(_document);
        }
    }
}
