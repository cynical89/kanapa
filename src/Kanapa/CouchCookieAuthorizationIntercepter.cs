using System;
using System.Collections.Generic;

namespace Kanapa
{
  public sealed class CouchCookieAuthorizationIntercepter : CouchAuthorizationInterceptorBase
  {
    private string _currentCookie;

    public CouchCookieAuthorizationIntercepter(string host, IEqualityComparer<string> hostEqualityComparer) 
      : base(host, hostEqualityComparer)
    {
      _currentCookie = null;
    }

    protected override IEnumerable<ICouchHeader> ProvideHeaders()
    {
      if (_currentCookie != null) return new[]
      {
        new DefaultCouchHeader("Cookie", _currentCookie)
      };
      if (PerformSessionCreate() == false)
      {
        // Not much informative...
        throw new CouchException("Error, when creating new session.");
      }

      return new[] { new DefaultCouchHeader("Cookie", _currentCookie) };
    }

    protected override bool PerformAuthorization()
    {
      return PerformSessionCreate();
    }

    private bool PerformSessionCreate()
    {
      throw new NotImplementedException();
    }
  }
}