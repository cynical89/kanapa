using System.Collections.Generic;
using Kanapa.Primitives;

namespace Kanapa.DefaultMiddleware
{
  public sealed class DefaultCouchRawResponse : ICouchRawResponse
  {
    public DefaultCouchRawResponse(string body, IEnumerable<ICouchHeader> responseHeaders)
    {
      Body = body;
      ResponseHeaders = responseHeaders;
    }

    public string Body { get; }
    public IEnumerable<ICouchHeader> ResponseHeaders { get; }
  }
}