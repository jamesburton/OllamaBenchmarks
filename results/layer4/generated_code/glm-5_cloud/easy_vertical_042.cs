using System.Threading.Tasks;

public record Document(string Id, string Title, string Content, string AuthorId);

public interface IDocumentStore
{
    Task SaveAsync(Document document);
    Task<Document?> GetAsync(string id);
    Task DeleteAsync(string id);
}

public class DocumentEditor
{
    private readonly IDocumentStore _store;

    public DocumentEditor(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<bool> UpdateTitleAsync(string id, string newTitle)
    {
        var document = await _store.GetAsync(id);

        if (document is null)
        {
            return false;
        }

        var updatedDocument = document with { Title = newTitle };
        await _store.SaveAsync(updatedDocument);

        return true;
    }
}