public class ApplyTaxTask : TaskActivity<double, double>
{
    protected override double Execute(TaskContext context, double input)
    {
        var tax = 100;
        var output = input + tax;

        return output;
    }
}