namespace Kanapa.Primitives
{
  public class CouchViewDefinition
  {
    public CouchViewDefinition()
    {
      Mapping = new CouchMapReduce();
    }
    public string Name { get; set; }

    public CouchMapReduce Mapping { get; set; }
  }
}