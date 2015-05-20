namespace Kanapa.Primitives
{
  public sealed class DefaultCouchHeader : ICouchHeader
  {
    public DefaultCouchHeader(string name, string value)
    {
      Name = name;
      Value = value;
    }

    public string Name { get; }
    public string Value { get; }
  }
}