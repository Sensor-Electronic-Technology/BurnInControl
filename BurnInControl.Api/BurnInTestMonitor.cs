using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.StationModel.Components;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BurnInControl.Api;

public class BurnInTestMonitor:BackgroundService {
    private readonly IMongoDatabase _database;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<BurnInTestMonitor> _logger;

    public BurnInTestMonitor(IMongoClient client,IHttpClientFactory httpFactory,ILogger<BurnInTestMonitor> logger) {
        this._database=client.GetDatabase("burn_in_db");
        this._httpFactory = httpFactory;
        this._logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            var collection = this._database.GetCollection<BurnInTestLog>("test_logs");

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BurnInTestLog>>()
                .Match(change => change.OperationType == ChangeStreamOperationType.Update);
            var options = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
                FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.WhenAvailable,
            };
            using var changeStream = await collection.WatchAsync(pipeline,options,cancellationToken: stoppingToken);
            foreach (var change in changeStream.ToEnumerable()) {
                Console.WriteLine("Here");
                if (change.FullDocument is { Completed: true }) {
                    var httpClient = this._httpFactory.CreateClient();
                    httpClient.BaseAddress=new Uri("http://localhost:5118/");
                    foreach (var test in change.FullDocument.TestSetup) {
                        await httpClient.GetAsync($"api/process/burn-in/{test.Value.WaferId}", stoppingToken);
                        this._logger.LogInformation("Send burn-in process complete notification for Wafer: {WaferId}",test.Value.WaferId);
                    }
                }
            }
        }
    }
}