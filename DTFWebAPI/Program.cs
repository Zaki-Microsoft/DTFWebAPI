var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IOrchestrationService, OrchestrationService>(serviceProvider =>
{
    var orchestrationService = new OrchestrationService();

    orchestrationService.TaskHubWorker
        .AddTaskOrchestrations(OrchestratorAndActivities.Orchestrations.Values.ToArray())
        .AddTaskActivities(OrchestratorAndActivities.Activities.ToArray());

    orchestrationService.StartAsync().Wait();

    return orchestrationService;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();