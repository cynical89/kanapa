using System.Collections.Generic;

namespace Kanapa
{
  public interface ICouchAuthenticationInterceptor
  {
    IEnumerable<ICouchHeader> Authenticate(string host);
  }
}