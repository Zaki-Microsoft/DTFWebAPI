public class ChangeNameTask : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string input)
    {
        var output = $"Welcome, {input}";

        Console.WriteLine($"{nameof(ChangeNameTask)} - Input: {input}, Output: {output}");

        return output;
    }
}