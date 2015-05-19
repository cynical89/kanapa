using System;
using System.Collections.Generic;
using System.Text;

namespace Kanapa
{
  public sealed class CouchBasicAuthorizationIntercepter : ICouchAuthorizationInterceptor
  {
    private readonly string _host;
    private readonly IEqualityComparer<string> _hostEqualityComparer;
    private readonly ICouchHeader[] _header;

    public CouchBasicAuthorizationIntercepter(string host, string userName, string password, IEqualityComparer<string> hostEqualityComparer)
    {
      if (hostEqualityComparer == null)
      {
        hostEqualityComparer = new CouchDefaultHostEqualityComparer();
      }
      _host = host;
      _hostEqualityComparer = hostEqualityComparer;
      _header = new ICouchHeader[] {new DefaultCouchHeader("Authorization", $"Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)))};
    }

    public IEnumerable<ICouchHeader> ProvideHeaders(string host)
    {
      if (_hostEqualityComparer.Equals(host, _host))
      {
        return _header;
      }

      throw new CouchException($"Unknown host, to authenticate {host}. Only {_host} can be authenticated.");
    }

    public bool PerformAuthorization(string host)
    {
      if (_hostEqualityComparer.Equals(host, host))
      {
        throw new CouchException($"Cannot authenticate host {_host}. Provided credentials are wrong.");
      }

      return false;
    }
  }
}