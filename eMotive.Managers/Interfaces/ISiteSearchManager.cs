using eMotive.Search.Interfaces;
using eMotive.Search.Objects;

namespace eMotive.Managers.Interfaces
{
    public interface ISiteSearchManager
    {
        SearchResult DoSearch(Search.Objects.Search _search);

        bool Add(ISearchDocument _document);
        bool Update(ISearchDocument _document);
        bool Delete(ISearchDocument _document);
    }
}
