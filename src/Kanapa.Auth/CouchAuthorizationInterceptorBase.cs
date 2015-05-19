using System;
using System.Collections.Generic;

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

    protected abstract IEnumerable<ICouchHeader> ProvideHeaders();
    protected abstract bool PerformAuthorization();

    protected virtual bool HostEqual(Uri hostToCompare)
    {
      return HostEqualityComparer.Equals(hostToCompare, Host);
    }

    public virtual IEnumerable<ICouchHeader> ProvideHeaders(Uri host)
    {
      if (HostEqual(host))
      {
        return ProvideHeaders();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }

    public virtual bool PerformAuthorization(Uri host)
    {
      if (HostEqual(host))
      {
        return PerformAuthorization();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }
  }
}