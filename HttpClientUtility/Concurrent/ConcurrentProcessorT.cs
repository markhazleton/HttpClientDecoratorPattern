using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a concurrent processor that processes tasks of type T.
/// </summary>
/// <typeparam name="T">The type of task data.</typeparam>
public abstract class ConcurrentProcessor<T>(Func<int, T> taskDataFactory) where T : ConcurrentProcessorModel
{
    private readonly List<Task<T>> tasks = [];
    private readonly Func<int, T> taskDataFactory = taskDataFactory ?? throw new ArgumentNullException(nameof(taskDataFactory));
    private SemaphoreSlim semaphore = new(1, 1); // Default initialization

    /// <summary>
    /// Gets or sets the maximum concurrency level.
    /// </summary>
    protected int MaxConcurrency { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of tasks to process.
    /// </summary>
    protected int MaxTaskCount { get; set; } = 1;

    /// <summary>
    /// Asynchronously waits for a semaphore and returns the elapsed ticks.
    /// </summary>
    /// <param name="semaphore">The semaphore to wait on.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The elapsed ticks while waiting for the semaphore.</returns>
    protected async Task<long> AwaitSemaphoreAsync(SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        stopwatch.Stop();
        return stopwatch.ElapsedTicks;
    }

    /// <summary>
    /// Gets the next task data based on the current task data.
    /// </summary>
    /// <param name="taskData">The current task data.</param>
    /// <returns>The next task data or null if there are no more tasks.</returns>
    protected virtual T? GetNextTaskData(T taskData)
    {
        if (taskData.TaskId < MaxTaskCount)
        {
            return taskDataFactory(taskData.TaskId + 1);
        }
        return null;
    }

    /// <summary>
    /// Manages the process of a task asynchronously.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="taskCount">The total number of tasks.</param>
    /// <param name="semaphoreWait">The elapsed ticks while waiting for the semaphore.</param>
    /// <param name="semaphore">The semaphore used for concurrency control.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected async Task<T> ManageProcessAsync(int taskId, int taskCount, long semaphoreWait, SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var taskData = taskDataFactory(taskId);
            taskData.TaskCount = taskCount;
            taskData.SemaphoreCount = semaphore.CurrentCount;
            taskData.SemaphoreWaitTicks = semaphoreWait;

            return await ProcessAsync(taskData, ct).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
            sw.Stop();
        }
    }

    /// <summary>
    /// Processes the task asynchronously.
    /// </summary>
    /// <param name="taskData">The task data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected abstract Task<T> ProcessAsync(T taskData, CancellationToken ct = default);

    /// <summary>
    /// Runs the concurrent processor asynchronously.
    /// </summary>
    /// <param name="maxTaskCount">The maximum number of tasks to process.</param>
    /// <param name="maxConcurrency">The maximum concurrency level.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The list of results from the processed tasks.</returns>
    public async Task<List<T>> RunAsync(int maxTaskCount, int maxConcurrency, CancellationToken ct = default)
    {
        MaxTaskCount = maxTaskCount;
        MaxConcurrency = maxConcurrency;
        semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);

        var taskData = taskDataFactory(1);
        var results = new List<T>();

        while (taskData is not null)
        {
            long semaphoreWait = await AwaitSemaphoreAsync(semaphore, ct).ConfigureAwait(false);
            var task = ManageProcessAsync(taskData.TaskId, tasks.Count, semaphoreWait, semaphore, ct);
            tasks.Add(task);

            taskData = GetNextTaskData(taskData);

            if (tasks.Count >= MaxConcurrency)
            {
                var finishedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
                results.Add(await finishedTask.ConfigureAwait(false));
                tasks.Remove(finishedTask);
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        results.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));

        return results;
    }
}
