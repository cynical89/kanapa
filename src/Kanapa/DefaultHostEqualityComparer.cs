using System;
using System.Collections.Generic;

namespace Kanapa
{
  public sealed class DefaultHostEqualityComparer :IEqualityComparer<string>
  {
    public bool Equals(string x, string y)
    {
      return Uri.Compare(new Uri(x), new Uri(y), UriComponents.Host, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) ==  0;
    }

    public int GetHashCode(string obj)
    {
      return obj.GetHashCode();
    }
  }
}