using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kanapa.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kanapa
{
  public class CouchClient : ICouchClient
  {
    private readonly Uri _host;
    private readonly ICouchAuthorizationInterceptor _couchAuthorizationInterceptor;
    private readonly ICouchMiddleware _middleware;

    public CouchClient(Uri host, ICouchMiddleware middleware, ICouchAuthorizationInterceptor couchAuthorizationInterceptor = null)
    {
      if (middleware == null)
      {
        throw new ArgumentNullException(nameof(middleware));
      }
      _host = new Uri(host, "/");
      _couchAuthorizationInterceptor = couchAuthorizationInterceptor;
      _middleware = middleware;
    }

    public async Task<IEnumerable<string>> GetDatabaseNames()
    {
      try
      {
        return JsonConvert.DeserializeObject<string[]>(await RequestDatabase(new Uri(_host,"_all_dbs"), "GET"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t read database names: {e.Message}", e);
      }
    }

    public async Task<CouchDatabaseMetadata> GetDatabaseMetadata(string db)
    {
      try
      {
        return JsonConvert.DeserializeObject<CouchDatabaseMetadata>(await RequestDatabase(new Uri(_host, db), "GET"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get database metadata: {e.Message}", e);
      }
    }

    public async Task<IEnumerable<CouchDocumentMetadata>> GetAllDocuments(string db, string fromKey = null, string toKey = null)
    {
      try
      {
        var result = await RequestDatabase(new Uri(_host,$"/{db}/_all_docs" + GetKeysPart(fromKey, toKey)), "GET");

        var d = JObject.Parse(result);

        return d["rows"].Select(row => new CouchDocumentMetadata
        {
          Id = row["id"].Value<string>(),
          Revision = row["value"]["rev"].Value<string>()
        }).ToArray();
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get documents: {e.Message}", e);
      }
    }

    public async Task<CouchEntityInfo> CreateDesign(string db, string name, IEnumerable<CouchViewDefinition> views)
    {
      Response response;
      try
      {
        var design = new CouchDesignDocument
        {
          Id = $"_design/{name}",
          Language = "javascript",
          Views = views
        };

        response =
          JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host,db), "POST",
            JsonConvert.SerializeObject(design), "application/json"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Cant create design document {db}/_design/{name}: {e.Message}", e);
      }

      if (response.Ok == false)
      {
        throw new CouchException($"Cant create design document {db}/_design/{name}: {response.Reason}");
      }

      return new CouchEntityInfo { ETag = response.ETag, Id = response.EntityId };
    }

    public async Task<CouchEntityInfo> DeleteDesign(string db, string name, string etag)
    {
      Response response;
      try
      {
        response = JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host,$"{db}/_design/{name}?rev={etag}"), "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t delete design document {db}/_design/{name}: {e.Message}");
      }

      if (response.Ok == false)
      {
        throw new CouchException($"Can`t delete design document {db}/_design/{name}: {response.Reason}");
      }

      return null;
    }

    public async Task<CouchDesignDocument> GetDesign(string db, string name)
    {
      try
      {
        return JsonConvert.DeserializeObject<CouchDesignDocument>(await RequestDatabase(new Uri(_host, $"{db}/_design/{name}"), "GET"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get view {name} : {e.Message}", e);
      }
    }

    public async Task<CouchEntityInfo> PutDesign(string db, CouchDesignDocument couchDesign)
    {
      Response response;
      try
      {
        response =
          JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host, $"{db}/_design/{couchDesign.Name}"), "PUT",
            JsonConvert.SerializeObject(couchDesign)));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t create view {db}/_design/{couchDesign.Name} : {e.Message}", e);
      }

      if (response.Ok == false)
      {
        throw new CouchException($"Can`t create view {db}/_design/{couchDesign.Name} : {response.Reason}");
      }

      return new CouchEntityInfo { ETag = response.ETag, Id = response.EntityId };
    }

    public async Task<CouchDesignDocument> CreateView(string db, string designName, CouchViewDefinition couchView)
    {
      if (couchView == null)
      {
        throw new ArgumentNullException(nameof(couchView));
      }
      var design = await GetDesign(db, designName);
      if (design.Views.Any(p => p.Name == couchView.Name))
      {
        throw new CouchException($"Can`t create view {db}/_design/{designName}/{couchView.Name} : View with the same name already exists in document.");
      }

      design.Views = new List<CouchViewDefinition>(design.Views) { couchView };
      await PutDesign(db, design);

      return design;
    }

    public async Task<CouchEntityInfo> DeleteView(string db, string designName, string viewName)
    {
      var design = await GetDesign(db, designName);
      if (design.Views.Any(p => p.Name == viewName) == false)
      {
        throw new CouchException($"Can`t delete view {db}/_design/{designName}/{viewName} : View not found.");
      }
      design.Views = design.Views.Where(l => l.Name != viewName).ToArray();
      await PutDesign(db, design);

      return null;
    }

    public async Task<CouchEntityInfo> PutView(string db, string designName, CouchViewDefinition couchView)
    {
      if (couchView == null)
      {
        throw new ArgumentNullException(nameof(couchView));
      }
      var design = await GetDesign(db, designName);
      var index = design.Views.FirstOrDefault(p => p.Name == couchView.Name);
      if (index == null)
      {
        throw new CouchException($"Can`t update view {db}/_design/{designName}/{couchView.Name} : View not found.");
      }
      index.Mapping = couchView.Mapping;
      return await PutDesign(db, design);
    }

    public async Task<CouchEntityInfo> Put<T>(string db, string documentId, T item)
    {
      Response response;
      try
      {
        response =
          JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host, $"{db}/{documentId}"), "PUT", JsonConvert.SerializeObject(item)));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t update document {documentId} : {e.Message}", e);
      }
      if (response.Ok == false)
      {
        throw new CouchException($"Can`t update document {documentId} : {response.Reason}");
      }

      return new CouchEntityInfo { ETag = response.ETag, Id = response.EntityId };
    }

    public async Task<CouchEntityInfo> CreateDatabase(string db)
    {
      Response result;
      try
      {
        result = JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host,db), "PUT"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Failed to create database {db} : {e.Message}", e);
      }
      if (result.Ok == false)
      {
        throw new CouchException($"Failed to create database {db} : {result.Reason}");
      }

      return null;
    }

    public async Task<CouchEntityInfo> DeleteDatabase(string db)
    {
      Response result;
      try
      {
        result = JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host, db), "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Failed to delete database {db}", e);
      }
      if (result.Ok == false)
      {
        throw new CouchException($"Failed to delete database {db}: {result.Reason}");
      }

      return null;
    }

    public async Task<CouchView<T>> CreateAndQueryTemporaryView<T>(string db, CouchMapReduce couchMapReduce, string fromKey = null, string toKey = null)
    {
      try
      {
        var result = await RequestDatabase(new Uri(_host, $"{db}/_temp_view" + GetKeysPart(fromKey, toKey)), "POST", JsonConvert.SerializeObject(couchMapReduce), "application/json");
        return JsonConvert.DeserializeObject<CouchView<T>>(result);
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get temporary view details: {e.Message}", e);
      }
    }

    public async Task<CouchView<T>> QueryView<T>(string db, string designName, string viewName, string fromKey = null, string toKey = null)
    {
      try
      {
        var result = await RequestDatabase(new Uri(_host,$"{db}/_design/{designName}/_view/{viewName}" + GetKeysPart(fromKey, toKey)), "GET");
        return JsonConvert.DeserializeObject<CouchView<T>>(result);
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t query view {db}/_design/{designName}/_view/{viewName} : {e.Message}");
      }
    }

    public async Task<CouchEntityInfo> Create<T>(string db, T content)
    {
      Response response;
      try
      {
        response =
          JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host,db), "POST", JsonConvert.SerializeObject(content), "application/json"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t create entity: {e.Message}", e);
      }
      if (response.Ok == false)
      {
        throw new CouchException($"Can`t create entity: {response.Reason}");
      }

      return new CouchEntityInfo { ETag = response.ETag, Id = response.EntityId };
    }

    public async Task<T> Get<T>(string db, string documentId)
    {
      try
      {
        return JsonConvert.DeserializeObject<T>(await RequestDatabase(new Uri(_host,$"{db}/{documentId}"), "GET"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get entity: {e.Message}", e);
      }
    }

    public async Task<CouchEntityInfo> Delete(string db, string docid, string etag)
    {
      Response response;
      try
      {
        response = JsonConvert.DeserializeObject<Response>(await RequestDatabase(new Uri(_host,$"{db}/{docid}?rev={etag}"), "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t delete {db}/{docid}: {e.Message}", e);
      }
      if (response.Ok == false)
      {
        throw new CouchException($"Can`t delete {db}/{docid}: {response.Reason}");
      }
      return null;
    }

    private static string GetKeysPart(string fromKey, string toKey)
    {
      var urlPart = string.Empty;
      var fromKeyIsNull = string.IsNullOrEmpty(fromKey);
      if (fromKeyIsNull == false)
      {
        urlPart += "?startkey=" + fromKey;
      }

      if (string.IsNullOrEmpty(toKey) == false)
      {
        urlPart += (fromKeyIsNull ? "?" : "&") + "endkey" + toKey;
      }

      return urlPart;
    }

    private async Task<string> RequestDatabase(Uri url, string method, string data = null, string contentType = null) =>
      (await _middleware.RequestDatabase(url, method, _couchAuthorizationInterceptor, data, contentType)).Body;
  }
}
