using HttpClientUtility.SendService;
using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Processes HTTP client requests concurrently.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpClientConcurrentProcessor"/> class.
/// </remarks>
/// <param name="taskDataFactory">The factory function to create task data.</param>
/// <param name="service">The HTTP client send service.</param>
public class HttpClientConcurrentProcessor(
    Func<int, HttpClientConcurrentModel> taskDataFactory, IHttpClientSendService service) : ConcurrentProcessor<HttpClientConcurrentModel>(taskDataFactory)
{
    private readonly IHttpClientSendService _service = service ?? throw new ArgumentNullException(nameof(service));

    /// <summary>
    /// Gets the next task data based on the current task data.
    /// </summary>
    /// <param name="taskData">The current task data.</param>
    /// <returns>The next task data or null if there are no more tasks.</returns>
    protected override HttpClientConcurrentModel? GetNextTaskData(HttpClientConcurrentModel taskData)
    {
        if (taskData.TaskId < MaxTaskCount)
        {
            return new HttpClientConcurrentModel(taskData.TaskId + 1, taskData.statusCall.RequestPath);
        }
        return null;
    }

    /// <summary>
    /// Processes the task asynchronously.
    /// </summary>
    /// <param name="taskData">The task data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected override async Task<HttpClientConcurrentModel> ProcessAsync(HttpClientConcurrentModel taskData, CancellationToken ct = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var result = await _service.HttpClientSendAsync(taskData.statusCall, ct).ConfigureAwait(true);
            taskData.statusCall = result;
            // add random delay of 3-5 seconds
            // await Task.Delay(new Random().Next(1000, 2000), ct).ConfigureAwait(true);

            sw.Stop();
            taskData.DurationMS = sw.ElapsedMilliseconds;
            return new HttpClientConcurrentModel(taskData, taskData.statusCall.RequestPath);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("An error occurred while processing the task.", ex);
        }
    }
}
