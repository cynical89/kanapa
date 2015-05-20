using System.Collections.Generic;

namespace Kanapa
{
  public interface ICouchRawResponse
  {
    string Body { get; }

    IEnumerable<ICouchHeader> ResponseHeaders { get; }
  }
}