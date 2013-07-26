using eMotive.Search.Objects;

namespace eMotive.Search.Interfaces
{
    public interface ISearchManager
    {
        SearchResult DoSearch(Objects.Search _search);
    }
}
