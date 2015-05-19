using System;
using System.Collections.Generic;
using System.Text;

namespace Kanapa
{
  public sealed class CouchBasicAuthorizationIntercepter : CouchAuthorizationInterceptorBase
  {
    private readonly ICouchHeader[] _header;

    public CouchBasicAuthorizationIntercepter(string host, string userName, string password, IEqualityComparer<string> hostEqualityComparer)
      :base(host,hostEqualityComparer)
    {
      _header = new ICouchHeader[] {new DefaultCouchHeader("Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)))};
    }

    protected override IEnumerable<ICouchHeader> ProvideHeaders()
    {
      return _header;
    }

    protected override bool PerformAuthorization()
    {
      throw new CouchException($"Unexpected call to perform authorization for http-basic authorization. {Host}");
    }
  }
}