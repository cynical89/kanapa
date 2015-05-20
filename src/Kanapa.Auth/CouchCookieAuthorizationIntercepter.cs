using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kanapa.Auth
{
  public sealed class CouchCookieAuthorizationIntercepter : CouchAuthorizationInterceptorBase
  {
    private readonly string _userName;
    private readonly string _password;
    private readonly ICouchMiddleware _middleware;
    private string _currentCookie;
    private int _authInProgress;
    private ManualResetEvent _authComplete;
    private IEnumerable<ICouchHeader> _headers;

    public CouchCookieAuthorizationIntercepter(Uri host, string userName, string password,ICouchMiddleware middleware, IEqualityComparer<Uri> hostEqualityComparer)
      : base(host, hostEqualityComparer)
    {
      _userName = userName;
      _password = password;
      _middleware = middleware;
      _currentCookie = null;
      _authInProgress = 0;
      _authComplete = new ManualResetEvent(false);
      _headers = new ICouchHeader[] { };
    }

    protected override Task<IEnumerable<ICouchHeader>> ProvideHeaders()
    {
      return Task.FromResult(_headers);
    }

    protected async override Task<bool> PerformAuthorization()
    {
      if (Interlocked.CompareExchange(ref _authInProgress, 1, 0) == 1)
      {
        _authComplete.WaitOne();
        return true;
      }
      var sessionUri = new Uri(Host, "_session");
      try
      {
        var response = JsonConvert.DeserializeObject<AuthResponse>(
          (await _middleware.RequestDatabase(sessionUri, "POST", null, JsonConvert.SerializeObject(new AuthRequest
          {
            Name = _userName,
            Password = _password
          }), "application/json")).Body);
        if (response.Ok == false)
        {
          return false;
        }

      }
      catch
      {
        return false;
      }
      finally
      {
        _authComplete.Set();
      }
      return true;
    }
  }
}