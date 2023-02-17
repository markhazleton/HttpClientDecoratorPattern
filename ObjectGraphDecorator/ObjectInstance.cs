namespace ObjectGraphDecorator;


public class ObjectInstance
{
    public ObjectInstance()
    {
        Name = "UNKNOWN";
    }
    public ObjectInstance(string name, object? data)
    {
        Name = name;
        Data = data;
    }
    public string Name { get; set; }
    public object? Data { get; set; }
}
