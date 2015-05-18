using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanapa
{
  public interface ICouchClient
  {
    Task<IEnumerable<string>> GetDatabaseNames();
    Task<DatabaseMetadata> GetDatabaseMetadata(string db);
    Task<IEnumerable<DocumentMetadata>> GetAllDocuments(string db, string fromKey = null, string toKey = null);
    Task<EntityInfo> CreateDesign(string db, string name, IEnumerable<ViewDefinition> views);
    Task<CouchClient> DeleteDesign(string db, string name, string etag);
    Task<DesignDocument> GetDesign(string db, string name);
    Task<EntityInfo> PutDesign(string db, DesignDocument design);
    Task<DesignDocument> CreateView(string db, string designName, ViewDefinition view);
    Task<CouchClient> DeleteView(string db, string designName, string viewName);
    Task<EntityInfo> PutView(string db, string designName, ViewDefinition view);
    Task<EntityInfo> Put<T>(string db, string documentId, T item);
    Task<CouchClient> CreateDatabase(string db);
    Task<CouchClient> DeleteDatabase(string db);
    Task<View<T>> CreateAndQueryTemporaryView<T>(string db, MapReduce mapReduce, string fromKey = null, string toKey = null);
    Task<View<T>> QueryView<T>(string db, string designName, string viewName, string fromKey = null, string toKey = null);
    Task<EntityInfo> Create<T>(string db, T content);
    Task<T> Get<T>(string db, string documentId);
    Task<CouchClient> Delete(string db, string docid, string etag);
  }
}