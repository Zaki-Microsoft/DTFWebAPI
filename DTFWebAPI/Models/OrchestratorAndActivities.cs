public static class OrchestratorAndActivities
{
    public static readonly Dictionary<OrchestrationName, Type> Orchestrations = new() 
    {
        { OrchestrationName.ProfileChange, typeof(ProfileChangeOrchestration) },
        { OrchestrationName.CalculateCharges, typeof(CalculateChargesOrchestration) },
        { OrchestrationName.Greetings, typeof(GreetingsOrchestration) },
    };

    public static readonly List<TaskActivity> Activities = new()
    {
        new ChangeNameTask(),

        new ApplyDiscountTask(),
        new ApplyFeesTask(),
        new ApplyShippingTask(),
        new ApplyTaxTask(),

        new GetUserTask(),
        new SendGreetingTask()
    };
}

public enum OrchestrationName
{
    ProfileChange,
    CalculateCharges,
    Greetings
}