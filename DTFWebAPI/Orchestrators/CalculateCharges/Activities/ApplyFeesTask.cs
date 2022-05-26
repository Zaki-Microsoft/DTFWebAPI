public class ApplyFeesTask : TaskActivity<double, double>
{
    protected override double Execute(TaskContext context, double input)
    {
        var fees = 100;
        var output = input + fees;

        return output;
    }
}