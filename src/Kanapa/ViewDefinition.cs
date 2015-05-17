namespace Kanapa
{
  public class ViewDefinition
  {
    public ViewDefinition()
    {
      Mapping = new MapReduce();
    }
    public string Name { get; set; }

    public MapReduce Mapping { get; set; }
  }
}