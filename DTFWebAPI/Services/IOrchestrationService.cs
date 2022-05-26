public interface IOrchestrationService
{
    AzureTableInstanceStore InstanceStore { get; set; }
    ServiceBusOrchestrationService ServiceBusOrchestrationService { get; set; }
    TaskHubClient TaskHubClient { get; set; }
    TaskHubWorker TaskHubWorker { get; set; }

    Task StartAsync();

    string GetInstanceId();
}