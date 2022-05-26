public class ProfileChangeOrchestration : TaskOrchestration<string, string>
{
    TaskCompletionSource<string>? resumeHandle;

    public override async Task<string> RunTask(OrchestrationContext context, string orchestrationInput)
    {
        var name = await WaitForSignal();

        var result = await context.ScheduleTask<string>(typeof(ChangeNameTask), name);

        return result;
    }

    async Task<string> WaitForSignal()
    {
        this.resumeHandle = new TaskCompletionSource<string>();
        string data = await this.resumeHandle.Task;
        this.resumeHandle = null;
        return data;
    }

    public override void OnEvent(OrchestrationContext context, string eventName, string eventData)
    {
        this.resumeHandle?.SetResult(eventData);
    }
}