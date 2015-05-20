using System.Collections.Generic;

namespace Kanapa.Primitives
{
  public interface ICouchRawResponse
  {
    string Body { get; }

    IEnumerable<ICouchHeader> ResponseHeaders { get; }
  }
}