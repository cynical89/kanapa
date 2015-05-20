using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Kanapa.Primitives;
using Newtonsoft.Json;

namespace Kanapa.Auth
{
  public sealed class CouchCookieAuthorizationIntercepter : CouchAuthorizationInterceptorBase
  {
    private readonly string _userName;
    private readonly string _password;
    private readonly ICouchMiddleware _middleware;
    private List<ICouchHeader> _headers;

    public CouchCookieAuthorizationIntercepter(
      Uri host, 
      string userName, 
      string password,
      ICouchMiddleware middleware,
      IEqualityComparer<Uri> hostEqualityComparer)
      : base(host, hostEqualityComparer)
    {
      _userName = userName;
      _password = password;
      _middleware = middleware;
      _headers = new List<ICouchHeader>(4);
    }

    protected override Task<IEnumerable<ICouchHeader>> ProvideHeaders()
    {
      return Task.FromResult((IEnumerable<ICouchHeader>)_headers.ToArray());
    }

    protected async override Task<bool> PerformAuthorization()
    {
      if (Monitor.TryEnter(_headers))
      {
        Monitor.Wait(_headers);
        return true;
      }
      var sessionUri = new Uri(Host, "_session");
      try
      {
        var response =
          await _middleware.RequestDatabase(sessionUri, "POST", null, JsonConvert.SerializeObject(new AuthRequest
          {
            Name = _userName,
            Password = _password
          }), "application/json");

        var deserialized = JsonConvert.DeserializeObject<AuthResponse>(response.Body);
        if (deserialized.Ok == false)
        {
          return false;
        }
        var cookieHeaders =
          response.ResponseHeaders.Where(
            l => string.Compare(l.Name, "Set-Cookie", StringComparison.OrdinalIgnoreCase) == 0)
            .Select(l => (ICouchHeader) new DefaultCouchHeader("Cookie", l.Value.Split(';').First()));
        _headers = cookieHeaders.ToList();
      }
      catch
      {
        return false;
      }
      finally
      {
        Monitor.Exit(_headers);
      }

      return true;
    }
  }
}