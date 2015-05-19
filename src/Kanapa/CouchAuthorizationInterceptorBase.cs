using System.Collections.Generic;

namespace Kanapa
{
  public abstract class CouchAuthorizationInterceptorBase : ICouchAuthorizationInterceptor
  {
    protected string Host { get; }
    protected IEqualityComparer<string> HostEqualityComparer { get; }

    protected CouchAuthorizationInterceptorBase(string host, IEqualityComparer<string> hostEqualityComparer)
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

    protected virtual bool HostEqual(string hostToCompare)
    {
      return HostEqualityComparer.Equals(hostToCompare, Host);
    }

    public virtual IEnumerable<ICouchHeader> ProvideHeaders(string host)
    {
      if (HostEqual(host))
      {
        return ProvideHeaders();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }

    public virtual bool PerformAuthorization(string host)
    {
      if (HostEqual(host))
      {
        return PerformAuthorization();
      }

      throw new CouchException($"Unknown host, to authorizate {host}. Only {Host} can be authorized.");
    }
  }
}