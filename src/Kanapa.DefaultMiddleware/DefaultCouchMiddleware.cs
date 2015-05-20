using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if DNXCORE50
using System.Net.Http;
#else

#endif

namespace Kanapa.DefaultMiddleware
{
  public class DefaultCouchMiddleware : ICouchMiddleware
  {
    public virtual async Task<ICouchRawResponse> RequestDatabase
      (Uri requestUri,
      string method, 
      ICouchAuthorizationInterceptor authInterceptor,
      string data = null,
      string contentType = null) =>
#if DNXCORE50
        await DnxCoreImplementation(requestUri, method, data, contentType, authInterceptor);
#else
        await Dnx451Implementation(requestUri, method,data, contentType, authInterceptor);
#endif

#if !DNXCORE50 
    private static async Task<ICouchRawResponse> Dnx451Implementation(Uri url, string method, string data, string contentType, ICouchAuthorizationInterceptor interceptor, int deep = 0)
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

      if (interceptor != null)
      {
        foreach (var header in await interceptor.ProvideHeaders(url))
        {
          req.Headers[header.Name] = header.Value;
        }
      }

      try
      {
        using (var resp = (HttpWebResponse)await req.GetResponseAsync())
        {
          using (var stream = resp.GetResponseStream())
          {
            if (stream == null)
            {
              throw new InvalidOperationException("Response stream contains no data");
            }
            using (var reader = new StreamReader(stream))
            {
              var result = await reader.ReadToEndAsync();
              return new DefaultCouchRawResponse(result,
                resp.Headers.AllKeys.Select(l => new DefaultCouchHeader(l, resp.Headers[l])).ToArray());
            }
          }
          
        }
      }
      catch (WebException e)
      {
        if (deep > 0)
        {
          throw new CouchException("Authentication interceptor failed to authenticate application.");
        }

        if ((e.Status != WebExceptionStatus.ProtocolError) && ((HttpWebResponse)e.Response).StatusCode != HttpStatusCode.Unauthorized)
        {
          throw new CouchException("Response status code does not indicate success or exception occured.", e);
        }

        if (interceptor == null)
        {
          throw new CouchException("Authentication interceptor is not set, but server requires authentication.", e);
        }

        if (await interceptor.PerformAuthorization(url) == false)
        {
          throw new CouchException($"Authentication interceptor failed to authenticate host {url.Host}");
        }

        return await Dnx451Implementation(url, method, data, contentType, interceptor, ++deep);
      }
    }

#else
    private async Task<ICouchRawResponse> DnxCoreImplementation(Uri url, string method, string data, string contentType, ICouchAuthorizationInterceptor interceptor, int deep = 0)
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
          if (interceptor != null)
          {
            foreach (var header in await interceptor.ProvideHeaders(url))
            {
              request.Headers.Add(header.Name, header.Value);
            }
          }
          var result = await client.SendAsync(request);
          if (result.StatusCode == HttpStatusCode.Unauthorized)
          {
            if (deep > 0)
            {
              throw new CouchException("Authentication interceptor failed to authenticate application.");
            }

            if (interceptor == null)
            {
              throw new CouchException("Authentication interceptor is not set, but server requires authentication.");
            }

            if (await interceptor.PerformAuthorization(url) == false)
            {
              throw new CouchException($"Authentication interceptor failed to authenticate host {url.Host}");
            }

            return await DnxCoreImplementation(url, method, data, contentType, interceptor, ++deep);
          }

          if (result.IsSuccessStatusCode == false)
          {
            throw new CouchException($"Response status code does not indicate success or exception occured. {result.StatusCode} : {result.ReasonPhrase}");
          }

          var content = await result.Content.ReadAsStringAsync();
          // return new DefaultCouch
          throw new NotImplementedException();
        }
      }
    }
#endif
  }
}