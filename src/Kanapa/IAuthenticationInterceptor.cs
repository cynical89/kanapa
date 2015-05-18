using System.Collections.Generic;

namespace Kanapa
{
  public interface IAuthenticationInterceptor
  {
    IEnumerable<ICouchHeader> Authenticate(string host);
  }
}