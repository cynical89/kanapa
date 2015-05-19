using System;
using System.Collections.Generic;

namespace Kanapa
{
  public interface ICouchAuthorizationInterceptor
  {
    IEnumerable<ICouchHeader> ProvideHeaders(Uri host);

    bool PerformAuthorization(Uri host);
  }
}