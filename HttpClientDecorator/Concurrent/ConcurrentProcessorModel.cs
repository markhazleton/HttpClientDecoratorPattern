using System.Diagnostics;

namespace HttpClientDecorator.Concurrent;

public class ConcurrentProcessorModel
{
    public ConcurrentProcessorModel(int taskId)
    {
        TaskId = taskId;
    }
    public int TaskId { get; set; }
    public int TaskCount { get; set; }
    public long DurationMS { get; set; }
    public int SemaphoreCount { get; set; }
    public long SemaphoreWaitTicks { get; set; }
    public override string? ToString()
    {
        return $"Task:{TaskId:D4} Duration:{DurationMS:D5} TaskCount:{TaskCount:D2} SemaphoreCount:{SemaphoreCount:D2} SemaphoreWaitTicks:{SemaphoreWaitTicks:D4}";
    }

    public ConcurrentProcessorModel CreateInitialTaskData(int taskId)
    {
        return new ConcurrentProcessorModel(taskId);
    }

    public ConcurrentProcessorModel CloneWithNewTaskId(int newTaskId)
    {
        return new ConcurrentProcessorModel(newTaskId)
        {
            TaskId = newTaskId,
            TaskCount = TaskCount + 1,
            DurationMS = DurationMS,
            SemaphoreCount = SemaphoreCount,
            SemaphoreWaitTicks = SemaphoreWaitTicks
        };
    }
}
