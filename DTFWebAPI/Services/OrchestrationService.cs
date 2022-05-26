public class OrchestrationService : IOrchestrationService
{
    public AzureTableInstanceStore InstanceStore { get; set; }
    public ServiceBusOrchestrationService ServiceBusOrchestrationService { get; set; }
    public TaskHubClient TaskHubClient { get; set; }
    public TaskHubWorker TaskHubWorker { get; set; }

    public OrchestrationService()
    {
        var serviceBusConnectionString = ConfigurationManager.AppSetting["ServiceBusConnectionString"];
        var storageConnectionString = ConfigurationManager.AppSetting["StorageConnectionString"];
        var taskHubName = ConfigurationManager.AppSetting["taskHubName"];

        InstanceStore = new AzureTableInstanceStore(taskHubName, storageConnectionString);
        ServiceBusOrchestrationService = new ServiceBusOrchestrationService(serviceBusConnectionString, taskHubName, InstanceStore, null, null);

        TaskHubClient = new TaskHubClient(ServiceBusOrchestrationService);
        TaskHubWorker = new TaskHubWorker(ServiceBusOrchestrationService);
    }

    public async Task StartAsync()
    {
        await ServiceBusOrchestrationService.CreateIfNotExistsAsync();

        TaskHubWorker.StartAsync().Wait();
    }

    public string GetInstanceId()
    {
        return Guid.NewGuid().ToString();
    }
}