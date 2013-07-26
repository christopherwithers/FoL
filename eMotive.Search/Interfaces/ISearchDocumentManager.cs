namespace eMotive.Search.Interfaces
{
    public interface ISearchDocumentManager
    {
        bool Add(ISearchDocument _document);
        bool Update(ISearchDocument _document);
        bool Delete(ISearchDocument _document);
    }
}
