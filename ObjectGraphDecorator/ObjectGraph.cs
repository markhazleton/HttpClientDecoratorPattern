using System.Diagnostics;

namespace ObjectGraphDecorator;

/// <summary>
/// Object Graph Class
/// </summary>
public class ObjectGraph
{
    // The name of the object graph
    public string? Name { get; set; }

    // The instances in the object graph
    public List<ObjectInstance> Instances { get; set; } = new List<ObjectInstance>();

    // The telemetry data associated with the object graph
    public List<InstanceTelemetry> Telemetry { get; internal set; } = new List<InstanceTelemetry>();

    // Gets an instance of type T for a given section name
    internal async Task<T?> GetInstanceAsync<T>(string sectionName, Task<T> task) where T : class
    {
        var sw = new Stopwatch();
        sw.Start();

        // Create a new ObjectInstance with the given section name
        var section = new ObjectInstance { Name = sectionName };

        try
        {
            // Execute the task asynchronously and add the resulting data to the ObjectInstance
            section.Data = await task;

            // Stop the stopwatch and add telemetry data to the Telemetry collection
            sw.Stop();
            Telemetry.Add(new InstanceTelemetry(sectionName, sw.ElapsedMilliseconds));
        }
        catch (Exception ex)
        {
            // Stop the stopwatch and add telemetry data with an error message to the Telemetry collection
            sw.Stop();
            Telemetry.Add(new InstanceTelemetry(sectionName, sw.ElapsedMilliseconds, new TelemetryError("Exception", ex.Message)));

            // Set the Data property of the ObjectInstance to null
            section.Data = null;
        }

        // Add the ObjectInstance to the Instances collection
        Instances.Add(section);

        // Return the data as a nullable type T
        return section.Data as T;
    }
}
