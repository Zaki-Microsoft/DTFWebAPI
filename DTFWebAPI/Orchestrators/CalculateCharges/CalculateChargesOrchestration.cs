public class CalculateChargesOrchestration : TaskOrchestration<double, string>
{
    TaskCompletionSource<double>? resumeHandle;

    public override async Task<double> RunTask(OrchestrationContext context, string orchestrationInput)
    {
        var amount = await WaitForSignal();

        var discountResult = await context.ScheduleTask<double>(typeof(ApplyDiscountTask), amount);
        var feesResult = await context.ScheduleTask<double>(typeof(ApplyFeesTask), discountResult);
        var shippingResult = await context.ScheduleTask<double>(typeof(ApplyShippingTask), feesResult);
        var taxResult = await context.ScheduleTask<double>(typeof(ApplyTaxTask), shippingResult);

        return taxResult;
    }

    async Task<double> WaitForSignal()
    {
        resumeHandle = new TaskCompletionSource<double>();
        var data = await resumeHandle.Task;
        resumeHandle = null;
        return data;
    }

    public override void OnEvent(OrchestrationContext context, string eventName, string eventData)
    {
        resumeHandle?.SetResult(double.Parse(eventData));
    }
}