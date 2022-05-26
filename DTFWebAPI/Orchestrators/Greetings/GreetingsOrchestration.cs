public class GreetingsOrchestration : TaskOrchestration<string, string>
{
    public override async Task<string> RunTask(OrchestrationContext context, string inputId)
    {
        string user = await context.ScheduleTask<string>(typeof(GetUserTask), inputId);
        string greeting = await context.ScheduleTask<string>(typeof(SendGreetingTask), user);
        return greeting;
    }
}