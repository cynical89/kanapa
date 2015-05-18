using System;
using System.Collections.Generic;
using System.Text;

namespace Kanapa
{
  public sealed class CouchBasicAuthenticationIntercepter : IAuthenticationInterceptor
  {
    private readonly string _host;
    private readonly IEqualityComparer<string> _hostEqualityComparer;
    private readonly ICouchHeader[] _header;

    public CouchBasicAuthenticationIntercepter(string host, string userName, string password, IEqualityComparer<string> hostEqualityComparer)
    {
      if (hostEqualityComparer == null)
      {
        hostEqualityComparer = new DefaultHostEqualityComparer();
      }
      _host = host;
      _hostEqualityComparer = hostEqualityComparer;
      _header = new ICouchHeader[] {new DefaultCouchHeader("Authorization", $"Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)))};
    }

    public IEnumerable<ICouchHeader> Authenticate(string host)
    {
      if (_hostEqualityComparer.Equals(host, _host))
      {
        return _header;
      }

      throw new CouchException($"Unknown host, to authenticate {host}. Only {_host} can be authenticated.");
    }
  }
}