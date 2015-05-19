using System;
using System.Collections.Generic;

namespace Kanapa
{
  public sealed class CouchDefaultHostEqualityComparer :IEqualityComparer<Uri>
  {
    public bool Equals(Uri x, Uri y)
    {
      return Uri.Compare(x, y, UriComponents.Host, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) ==  0;
    }

    public int GetHashCode(Uri obj)
    {
      return obj.GetHashCode();
    }
  }
}