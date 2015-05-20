using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kanapa.Primitives;

namespace Kanapa.Auth
{
  public sealed class CouchOpenAuthAuthorizationInterceptor : CouchAuthorizationInterceptorBase
  {
    public CouchOpenAuthAuthorizationInterceptor(Uri host, IEqualityComparer<Uri> hostEqualityComparer) 
      : base(host, hostEqualityComparer)
    {
    }

    protected override Task<IEnumerable<ICouchHeader>> ProvideHeaders()
    {
      throw new NotImplementedException();
    }

    protected override Task<bool> PerformAuthorization()
    {
      throw new NotImplementedException();
    }
  }
}