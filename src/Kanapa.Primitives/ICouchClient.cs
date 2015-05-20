using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanapa.Primitives
{
  public interface ICouchClient
  {
    Task<IEnumerable<string>> GetDatabaseNames();
    Task<CouchDatabaseMetadata> GetDatabaseMetadata(string db);
    Task<IEnumerable<CouchDocumentMetadata>> GetAllDocuments(string db, string fromKey = null, string toKey = null);
    Task<CouchEntityInfo> CreateDesign(string db, string name, IEnumerable<CouchViewDefinition> views);
    Task<CouchEntityInfo> DeleteDesign(string db, string name, string etag);
    Task<CouchDesignDocument> GetDesign(string db, string name);
    Task<CouchEntityInfo> PutDesign(string db, CouchDesignDocument couchDesign);
    Task<CouchDesignDocument> CreateView(string db, string designName, CouchViewDefinition couchView);
    Task<CouchEntityInfo> DeleteView(string db, string designName, string viewName);
    Task<CouchEntityInfo> PutView(string db, string designName, CouchViewDefinition couchView);
    Task<CouchEntityInfo> Put<T>(string db, string documentId, T item);
    Task<CouchEntityInfo> CreateDatabase(string db);
    Task<CouchEntityInfo> DeleteDatabase(string db);
    Task<CouchView<T>> CreateAndQueryTemporaryView<T>(string db, CouchMapReduce couchMapReduce, string fromKey = null, string toKey = null);
    Task<CouchView<T>> QueryView<T>(string db, string designName, string viewName, string fromKey = null, string toKey = null);
    Task<CouchEntityInfo> Create<T>(string db, T content);
    Task<T> Get<T>(string db, string documentId);
    Task<CouchEntityInfo> Delete(string db, string docid, string etag);
  }
}