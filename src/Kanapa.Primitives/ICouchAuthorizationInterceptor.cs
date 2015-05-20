using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanapa.Primitives
{
  public interface ICouchAuthorizationInterceptor
  {
    Task<IEnumerable<ICouchHeader>> ProvideHeaders(Uri host);

    Task<bool> PerformAuthorization(Uri host);
  }
}