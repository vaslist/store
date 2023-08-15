namespace Store
{
    public interface IBookRepository
    {
        Book[] GetAllByIsbn(string isbn);
        Book[] GetAllByTitleOrAuthor(string titlePart);
        Book GetById(int id);
        Book[] GetAllByIds(IEnumerable<int> bookIds);
    }
}
