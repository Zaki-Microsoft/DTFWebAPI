public class SendGreetingTask : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string user)
    {
        return "Welcome, " + user;
    }
}