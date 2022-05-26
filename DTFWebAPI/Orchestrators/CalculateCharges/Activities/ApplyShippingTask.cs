public class ApplyShippingTask : TaskActivity<double, double>
{
    protected override double Execute(TaskContext context, double input)
    {
        var shipping = 100;
        var output = input + shipping;

        return output;
    }
}