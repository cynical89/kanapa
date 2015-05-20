using System;
using System.Threading.Tasks;

namespace Kanapa
{
  public interface ICouchMiddleware
  {
    Task<ICouchRawResponse> RequestDatabase(
      Uri requestUri,
      string method,
      ICouchAuthorizationInterceptor authInterceptor, 
      string data = null, 
      string contentType = null);
  }
}