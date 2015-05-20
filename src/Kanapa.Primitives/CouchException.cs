using System;
using System.Runtime.Serialization;
#if !DNXCORE50

#endif

namespace Kanapa.Primitives
{
#if !DNXCORE50
  [Serializable]
#endif
  public class CouchException : InvalidOperationException
  {
    public CouchException()
    {
    }

    public CouchException(string message)
      : base(message)
    {
    }

    public CouchException(string message, Exception inner) 
      : base(message, inner)
    {
    }

#if !DNXCORE50
    protected CouchException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
#endif

  }
} 