using eMotive.Search.Objects;

namespace eMotive.Search.Interfaces
{
    public interface ISearchManager
    {
        SearchResult DoSearch(Objects.Search _search);

        bool Add(ISearchDocument _document);
        bool Update(ISearchDocument _document);
        bool Delete(ISearchDocument _document);

        void DeleteAll();

        int NumberOfDocuments();
    }
}
