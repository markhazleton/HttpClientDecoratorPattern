namespace ObjectGraphDecorator;


/// <summary>
/// Documents and error in loading dashboard data
/// </summary>
public class TelemetryError
{
    public TelemetryError()
    {

    }
    public TelemetryError(string code, string description)
    {
        Code = code;
        Description = description;

    }
    /// <summary>
    /// Error Code
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// Error Description
    /// </summary>
    public string? Description { get; set; }
}
