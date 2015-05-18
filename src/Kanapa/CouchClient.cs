using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Net;
using Microsoft.Framework.WebEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if DNXCORE50
using System.Net.Http;
#else
using System.IO;
using System.Threading;
#endif

namespace Kanapa
{
  public class CouchClient : ICouchClient
  {
    private readonly string _host;
    private readonly IUrlEncoder _urlEncoder;
    private readonly ICouchAuthenticationInterceptor _couchAuthenticationInterceptor;

    public CouchClient(string host, IUrlEncoder urlEncoder, ICouchAuthenticationInterceptor couchAuthenticationInterceptor = null)
    {
      _host = host.EndsWith("/") ? host.Substring(0, host.Length - 1) : host;
      _urlEncoder = urlEncoder;
      _couchAuthenticationInterceptor = couchAuthenticationInterceptor;
    }

    public async Task<IEnumerable<string>> GetDatabaseNames()
    {
      try
      {
        return JsonConvert.DeserializeObject<string[]>(await RequestDatabase($"{_host}/_all_dbs", "GET"));
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
        return JsonConvert.DeserializeObject<CouchDatabaseMetadata>(await RequestDatabase($"{_host}/{db}/", "GET"));
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
        var result = await RequestDatabase($"{_host}/{db}/_all_docs" + GetKeysPart(fromKey, toKey), "GET");

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
          JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}", "POST",
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

    public async Task<CouchClient> DeleteDesign(string db, string name, string etag)
    {
      Response response;
      try
      {
        response = JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}/_design/{name}?rev={etag}", "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t delete design document {db}/_design/{name}: {e.Message}");
      }

      if (response.Ok == false)
      {
        throw new CouchException($"Can`t delete design document {db}/_design/{name}: {response.Reason}");
      }

      return this;
    }

    public async Task<CouchDesignDocument> GetDesign(string db, string name)
    {
      try
      {
        return JsonConvert.DeserializeObject<CouchDesignDocument>(await RequestDatabase($"{_host}/{db}/_design/{name}", "GET"));
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
        couchDesign.IgnoreRevisionAndId = true;
        response =
          JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}/_design/{couchDesign.Name}", "PUT",
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

    public async Task<CouchClient> DeleteView(string db, string designName, string viewName)
    {
      var design = await GetDesign(db, designName);
      if (design.Views.Any(p => p.Name == viewName) == false)
      {
        throw new CouchException($"Can`t delete view {db}/_design/{designName}/{viewName} : View not found.");
      }
      design.Views = design.Views.Where(l => l.Name != viewName).ToArray();
      await PutDesign(db, design);

      return this;
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
          JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}/{documentId}", "PUT", JsonConvert.SerializeObject(item)));
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

    public async Task<CouchClient> CreateDatabase(string db)
    {
      Response result;
      try
      {
        result = JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}", "PUT"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Failed to create database {db} : {e.Message}", e);
      }
      if (result.Ok == false)
      {
        throw new CouchException($"Failed to create database {db} : {result.Reason}");
      }

      return this;
    }

    public async Task<CouchClient> DeleteDatabase(string db)
    {
      Response result;
      try
      {
        result = JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}", "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Failed to delete database {db}", e);
      }
      if (result.Ok == false)
      {
        throw new CouchException($"Failed to delete database {db}: {result.Reason}");
      }

      return this;
    }

    public async Task<CouchView<T>> CreateAndQueryTemporaryView<T>(string db, CouchMapReduce couchMapReduce, string fromKey = null, string toKey = null)
    {
      try
      {
        var url = $"{_host}/{db}/_temp_view" + GetKeysPart(fromKey, toKey);
        var result = await RequestDatabase(url, "POST", JsonConvert.SerializeObject(couchMapReduce), "application/json");
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
        var url = $"{_host}/{db}/_design/{designName}/_view/{viewName}" + GetKeysPart(fromKey, toKey);
        var result = await RequestDatabase(url, "GET");
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
          JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}", "POST", JsonConvert.SerializeObject(content), "application/json"));
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
        return JsonConvert.DeserializeObject<T>(await RequestDatabase($"{_host}/{db}/{documentId}", "GET"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t get entity: {e.Message}", e);
      }
    }

    public async Task<CouchClient> Delete(string db, string docid, string etag)
    {
      Response response;
      try
      {
        response = JsonConvert.DeserializeObject<Response>(await RequestDatabase($"{_host}/{db}/{docid}?rev={etag}", "DELETE"));
      }
      catch (Exception e)
      {
        throw new CouchException($"Can`t delete {db}/{docid}: {e.Message}", e);
      }
      if (response.Ok == false)
      {
        throw new CouchException($"Can`t delete {db}/{docid}: {response.Reason}");
      }
      return this;
    }

    private string GetKeysPart(string fromKey, string toKey)
    {
      var urlPart = string.Empty;
      var fromKeyIsNull = string.IsNullOrEmpty(fromKey);
      if (fromKeyIsNull == false)
      {
        urlPart += "?startkey=" + _urlEncoder.UrlEncode(fromKey);
      }

      if (string.IsNullOrEmpty(toKey) == false)
      {
        urlPart += (fromKeyIsNull ? "?" : "&") + "endkey" + _urlEncoder.UrlEncode(toKey);
      }

      return urlPart;
    }

    private async Task<string> RequestDatabase(string url, string method, string data = null, string contentType = null) =>
#if DNXCORE50
        await DnxCoreImplementation(url, method, data, contentType);
#else
        await Dnx451Implementation(url, method, data, contentType);
#endif

#if DNXCORE50
    private async Task<string> DnxCoreImplementation(string url, string method, string data, string contentType, int deep=0)
    {
      using (var client = new HttpClient())
      {
        using (var request = new HttpRequestMessage
        {
          Method = new HttpMethod(method)
        })
        {
          if (string.IsNullOrEmpty(data) == false)
          {
            request.Content = new StringContent(data, Encoding.UTF8, contentType);
          }
          if (_couchAuthenticationInterceptor != null)
          {
            foreach (var header in _couchAuthenticationInterceptor.Authenticate(_host))
            {
              request.Headers.Add(header.Name,header.Value);
            }
          }
          var result = await client.SendAsync(request);
          if(result.StatusCode == HttpStatusCode.Unauthorized)
          {
            if (deep > 0)
            {
              throw new CouchException("Authentication interceptor failed to authenticate application.");
            }

            if (_couchAuthenticationInterceptor == null)
            {
              throw new CouchException("Authentication interceptor is not set, but server requires authentication.");
            }

            return await DnxCoreImplementation(url, method, data, contentType, ++deep);
          }

          if(result.IsSuccessStatusCode == false)
          {
            throw new CouchException($"Response status code does not indicate success or exception occured. {result.StatusCode} : {result.ReasonPhrase}");
          }

          var content = await result.Content.ReadAsStringAsync();
          return content;
        }
      }
    }
#else
    private async Task<string> Dnx451Implementation(string url, string method, string data, string contentType, int deep = 0)
    {
      var req = (HttpWebRequest)WebRequest.Create(url);
      req.Method = method;
      req.Timeout = Timeout.Infinite;

      if (string.IsNullOrEmpty(contentType) == false)
      {
        req.ContentType = contentType;
      }

      if (string.IsNullOrEmpty(data) == false)
      {
        var bytes = Encoding.UTF8.GetBytes(data);
        req.ContentLength = bytes.Length;
        using (var ps = req.GetRequestStream())
        {
          ps.Write(bytes, 0, bytes.Length);
        }
      }
    
      if(_couchAuthenticationInterceptor != null)
      {
        foreach (var header in _couchAuthenticationInterceptor.Authenticate(_host))
        {
          req.Headers[header.Name] = header.Value;
        }
      }

      try
      {
        using (var resp = (HttpWebResponse) await req.GetResponseAsync())
        {
          string result;
          using (var stream = resp.GetResponseStream())
          {
            if (stream == null)
            {
              throw new InvalidOperationException("Response stream contains no data");
            }
            using (var reader = new StreamReader(stream))
            {
              result = await reader.ReadToEndAsync();
            }
          }
          return result;
        }
      }
      catch (WebException e)
      {
        if(deep > 0)
        {
          throw new CouchException("Authentication interceptor failed to authenticate application.");
        }

        if ((e.Status != WebExceptionStatus.ProtocolError) && ((HttpWebResponse)e.Response).StatusCode != HttpStatusCode.Unauthorized)
        {
          throw new CouchException("Response status code does not indicate success or exception occured.", e);
        }
       
        if (_couchAuthenticationInterceptor == null)
        {
          throw new CouchException("Authentication interceptor is not set, but server requires authentication.", e);
        }

        return await Dnx451Implementation(url,method,data,contentType,++deep);
      }
    }
#endif
  }
}
