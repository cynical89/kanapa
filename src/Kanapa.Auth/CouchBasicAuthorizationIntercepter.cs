using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kanapa.Primitives;

namespace Kanapa.Auth
{
  public sealed class CouchBasicAuthorizationIntercepter : CouchAuthorizationInterceptorBase
  {
    private readonly ICouchHeader[] _header;

    public CouchBasicAuthorizationIntercepter(Uri host, string userName, string password, IEqualityComparer<Uri> hostEqualityComparer)
      :base(host,hostEqualityComparer)
    {
      _header = new ICouchHeader[] {new DefaultCouchHeader("Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)))};
    }

    protected override Task<IEnumerable<ICouchHeader>> ProvideHeaders()
    {
      return Task.FromResult((IEnumerable<ICouchHeader>)_header);
    }

    protected override Task<bool> PerformAuthorization()
    {
      throw new CouchException($"Unexpected call to perform authorization for http-basic authorization. {Host}");
    }
  }
}