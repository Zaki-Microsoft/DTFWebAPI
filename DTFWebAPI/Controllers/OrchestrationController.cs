using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DTFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrchestrationController : ControllerBase
    {
        private readonly IOrchestrationService _orchestrationService;

        public OrchestrationController(IOrchestrationService profileChangeOrchestrationService)
        {
            _orchestrationService = profileChangeOrchestrationService;
        }

        [HttpPost]
        public object? Post(OrchestrationData data)
        {
            try
            {
                if (!Enum.TryParse(data.OrchestrationName, out OrchestrationName orchestrationName))
                    throw new Exception("Invalid Orchestrations");

                var instance = _orchestrationService.TaskHubClient.CreateOrchestrationInstanceWithRaisedEventAsync
                    (
                        orchestrationType: OrchestratorAndActivities.Orchestrations[orchestrationName], 
                        instanceId: _orchestrationService.GetInstanceId(), 
                        orchestrationInput: null, 
                        eventName: data.EventName, 
                        eventData: data.EventData
                    ).Result;

                var state = _orchestrationService.TaskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(60), CancellationToken.None).Result;

                return new
                {
                    instance.InstanceId,
                    state.OrchestrationInstance,
                    state.OrchestrationStatus,
                    state.Input,
                    state.Output
                };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("Async")]
        public object? PostAsync(OrchestrationData data)
        {
            try
            {
                if (!Enum.TryParse(data.OrchestrationName, out OrchestrationName orchestrationName))
                    throw new Exception("Invalid Orchestrations");

                var instance = _orchestrationService.TaskHubClient.CreateOrchestrationInstanceWithRaisedEventAsync
                    (
                        orchestrationType: OrchestratorAndActivities.Orchestrations[orchestrationName],
                        instanceId: _orchestrationService.GetInstanceId(),
                        orchestrationInput: null,
                        eventName: data.EventName,
                        eventData: data.EventData
                    ).Result;

                return instance;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("NoEvent")]
        public object? PostNoEvent(OrchestrationData data)
        {
            try
            {
                if (!Enum.TryParse(data.OrchestrationName, out OrchestrationName orchestrationName))
                    throw new Exception("Invalid Orchestrations");

                var instance = _orchestrationService.TaskHubClient.CreateOrchestrationInstanceAsync
                    (
                        orchestrationType: OrchestratorAndActivities.Orchestrations[orchestrationName],
                        instanceId: _orchestrationService.GetInstanceId(),
                        input: data.EventData
                    ).Result;

                var state = _orchestrationService.TaskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(60), CancellationToken.None).Result;

                return new
                {
                    instance.InstanceId,
                    state.OrchestrationInstance,
                    state.OrchestrationStatus,
                    state.Input,
                    state.Output
                };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("NoEventAsync")]
        public object? PostNoEventAsync(OrchestrationData data)
        {
            try
            {
                if (!Enum.TryParse(data.OrchestrationName, out OrchestrationName orchestrationName))
                    throw new Exception("Invalid Orchestrations");

                var instance = _orchestrationService.TaskHubClient.CreateOrchestrationInstanceAsync
                    (
                        orchestrationType: OrchestratorAndActivities.Orchestrations[orchestrationName],
                        instanceId: _orchestrationService.GetInstanceId(),
                        input: data.EventData
                    ).Result;

                return instance;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

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
                return ex.Message;
            }
        }
    }
}
