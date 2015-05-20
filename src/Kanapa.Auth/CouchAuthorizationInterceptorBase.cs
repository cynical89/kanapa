using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanapa.Auth
{
  public abstract class CouchAuthorizationInterceptorBase : ICouchAuthorizationInterceptor
  {
    protected Uri Host { get; }
    protected IEqualityComparer<Uri> HostEqualityComparer { get; }

    protected CouchAuthorizationInterceptorBase(Uri host, IEqualityComparer<Uri> hostEqualityComparer)
    {
      if (hostEqualityComparer == null)
      {
        hostEqualityComparer = new CouchDefaultHostEqualityComparer();
      }
      Host = host;
      HostEqualityComparer = hostEqualityComparer;
    }

    protected abstract Task<IEnumerable<ICouchHeader>> ProvideHeaders();
    protected abstract Task<bool> PerformAuthorization();

    protected virtual bool HostEqual(Uri hostToCompare)
    {
      return HostEqualityComparer.Equals(hostToCompare, Host);
    }

    public virtual async Task<IEnumerable<ICouchHeader>> ProvideHeaders(Uri host)
    {
      if (HostEqual(host))
      {
        return await ProvideHeaders();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }

    public async virtual Task<bool> PerformAuthorization(Uri host)
    {
      if (HostEqual(host))
      {
        return await PerformAuthorization();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }
  }
}