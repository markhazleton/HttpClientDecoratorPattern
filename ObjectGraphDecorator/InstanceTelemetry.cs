namespace ObjectGraphDecorator;

public class InstanceTelemetry
{
    public InstanceTelemetry(string sectionName, long elapsedTimeMS)
    {
        Name = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        ElapsedTimeMS = elapsedTimeMS;
    }
    public InstanceTelemetry(string sectionName, long elapsedTimeMS, TelemetryError epicError)
    {
        Name = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        ElapsedTimeMS = elapsedTimeMS;
        Error = epicError;
    }
    public string Name { get; set; }
    public long ElapsedTimeMS { get; set; }
    public TelemetryError? Error { get; set; }
}
