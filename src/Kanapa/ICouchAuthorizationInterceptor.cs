using System.Collections.Generic;

namespace Kanapa
{
  public interface ICouchAuthorizationInterceptor
  {
    IEnumerable<ICouchHeader> ProvideHeaders(string host);

    bool PerformAuthorization(string host);
  }
}