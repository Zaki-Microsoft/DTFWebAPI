public class ApplyDiscountTask : TaskActivity<double, double>
{
    protected override double Execute(TaskContext context, double input)
    {
        var discount = 100;
        var output = input - discount;

        return output;
    }
}