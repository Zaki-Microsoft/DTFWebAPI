public class GetUserTask : TaskActivity<string, string>
{
    protected override string Execute(DurableTask.Core.TaskContext context, string input)
    {
        return "John Doe";
    }
}