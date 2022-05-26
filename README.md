# Durable Task Framework with ASP.NET Core API

Setting up DTF with ASP.NET Core API (.NET 6.0) by creating generic orchestration controller and services.

## Run the Project

1. With Default Setting:

- Build the project - it will resolve all the Nuget dependencies.
- Run the project [F5]

2. Update the Azure Service Bus and Storage Connection Strings:

If you want to use your own Azure subscription's Service Bus and Storage then go to `appsettings.json` and replace the following properties with yours:
```
{
  "StorageConnectionString": "YOURS_STORAGE_KEY",
  "ServiceBusConnectionString": "YOURS_SERVICE_BUS_CONNECTION_STRING",
}
```

3. Use Storage Emulator (VS 2022):

If you are having Visual Studio 2022 then follow below steps:

- Open CMD as Administrator and go to above directory
- Go to "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator"
- Run "azurite.exe"
- Use `UseDevelopmentStorage=true` as `StorageConnectionString` within `appsettings.json`

Commands:
```
C:\Windows\system32>cd C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator

C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator>azurite.exe
```

If you are using older version of Visual Studio then follow this link: https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio

## Calling API from Swagger

Once you run the project you will get your browser open and in that browser Swagger will be loaded from where you can make API calls to test your endpoints.

- POST - runs the orchestrator
- GET - returns the orchestrator instance based on instanceId

We have 2 orchestrators right now `CalculateCharges` and `ProfileChange`. The `ProfileChange` accepts a user name and return a greeting message. While the `CalculateCharges` accepts a base amount and returns the calculated charge after applying discount, shipping, tax etc.

Steps to make an POST API call from Swagger page:

- Expand the POST call
- Click on "Try it Out" button
- Pass the below object as body for `ProfileChange`:
    ```
    {
      "eventName": "ProfileChangeEvent",
      "eventData": "John Doe",
      "orchestrationName": "ProfileChange"
    }
    ```
- Pass the below object as body for `CalculateCharges`
    ```
    {
      "eventName": "CalculateChargesEvent",
      "eventData": "1000",
      "orchestrationName": "CalculateCharges"
    }
    ```

Steps to make an GET API call from Swagger page:

- Expand the GET call
- Click on "Try it Out" button
- Pass the below the instanceId obtained from POST call

## Creating a New Orchestrator

Follow the below steps to create your own Orchestrator:

- Create a new folder in `Orchestrators` folder with the name of your orchestrator.
- Create `Activities` sub-folder in your new orchestrator folder.
- Create `<YOUR_NAME>Orchestration.cs` orchestrator CS file inside orchestrator folder.
- Create activities CS files inside `Activities` sub-folder.
- Follow the existing orchestrator code to create your own orchestrator.
- Follow the existing activities code to create your own activities.
- Add your orchestrator and activities to `OrchestratorAndActivities.cs` file.
    ```
    // adding orchestrator
    public static readonly Dictionary<OrchestrationName, Type> Orchestrations = new() 
    {
        { OrchestrationName.ProfileChange, typeof(ProfileChangeOrchestration) },
        { OrchestrationName.CalculateCharges, typeof(CalculateChargesOrchestration) },
    };

    // adding activities
    public static readonly List<TaskActivity> Activities = new()
    {
        new ChangeNameTask(),

        new ApplyDiscountTask(),
        new ApplyFeesTask(),
        new ApplyShippingTask(),
        new ApplyTaxTask(),
    };
    ```

## Taling about components

OrchestrationService interface and class setup is shown below for API project.

Service Inteface:
```
public interface IOrchestrationService
{
    AzureTableInstanceStore InstanceStore { get; set; }
    ServiceBusOrchestrationService ServiceBusOrchestrationService { get; set; }
    TaskHubClient TaskHubClient { get; set; }
    TaskHubWorker TaskHubWorker { get; set; }

    Task StartAsync();

    string GetInstanceId();
}
```

Service Class:
```
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
```

Add Service to Scope:
```
builder.Services.AddSingleton<IOrchestrationService, OrchestrationService>(serviceProvider =>
{
    var orchestrationService = new OrchestrationService();

    orchestrationService.TaskHubWorker
        .AddTaskOrchestrations(OrchestratorAndActivities.Orchestrations.Values.ToArray())
        .AddTaskActivities(OrchestratorAndActivities.Activities.ToArray());

    orchestrationService.StartAsync().Wait();

    return orchestrationService;
});
```

Orchestrator Models - OrchestratorAndActivities:
```
public static class OrchestratorAndActivities
{
    public static readonly Dictionary<OrchestrationName, Type> Orchestrations = new() 
    {
        { OrchestrationName.ProfileChange, typeof(ProfileChangeOrchestration) },
        { OrchestrationName.CalculateCharges, typeof(CalculateChargesOrchestration) },
    };

    public static readonly List<TaskActivity> Activities = new()
    {
        new ChangeNameTask(),

        new ApplyDiscountTask(),
        new ApplyFeesTask(),
        new ApplyShippingTask(),
        new ApplyTaxTask(),
    };
}

public enum OrchestrationName
{
    ProfileChange,
    CalculateCharges
}
```

Orchestrator Models - OrchestrationData:
```
public class OrchestrationData
{
    public string EventName { get; set; }
    public string OrchestrationInput { get; set; }
    public string EventData { get; set; }
}
```

Access Service within Controller:
```
private readonly IOrchestrationService _orchestrationService;

public OrchestrationController(IOrchestrationService profileChangeOrchestrationService)
{
    _orchestrationService = profileChangeOrchestrationService;
}
```

POST Method to Create Task Instance:
```
[HttpPost]
public object? Post(OrchestrationData data)
{
    try
    {
        if (!Enum.TryParse(data.OrchestrationInput, out OrchestrationName orchestrationName))
            throw new Exception("Invalid Orchestrations");

        var instance = _orchestrationService.TaskHubClient.CreateOrchestrationInstanceWithRaisedEventAsync
            (
                orchestrationType: OrchestratorAndActivities.Orchestrations[orchestrationName], 
                instanceId: _orchestrationService.GetInstanceId(), 
                orchestrationInput: data.OrchestrationInput, 
                eventName: data.EventName, 
                eventData: data.EventData
            ).Result;

        var state = _orchestrationService.TaskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(60), CancellationToken.None).Result;

        return new
        {
            instance.InstanceId,
            state.OrchestrationInstance,
            state.Status,
            state.Input,
            state.Output
        };
    }
    catch (Exception ex)
    {
        return ex;
    }
}
```

GET Method to Obtain Task Instance State:
```
[HttpGet("{instanceId}")]
public object? Get(string instanceId)
{
    try
    {
        var state = _orchestrationService.TaskHubClient.GetOrchestrationStateAsync(instanceId).Result;

        return new
        {
            state.OrchestrationInstance,
            state.OrchestrationStatus,
            state.Input,
            state.Output
        };
    }
    catch (Exception ex)
    {
        return ex;
    }
}
```