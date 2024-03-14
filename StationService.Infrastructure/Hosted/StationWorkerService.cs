using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StationService.Infrastructure.StationControl;
namespace StationService.Infrastructure.Hosted;

public class StationWorkerService:IHostedService,IDisposable {
    private readonly IStationController _stationController;
    private readonly ILogger<StationWorkerService> _logger;

    public StationWorkerService(IStationController stationController, ILogger<StationWorkerService> logger) {
        this._logger = logger;
        this._stationController = stationController;
    }
    

    public async Task StartAsync(CancellationToken cancellationToken) {
        /*var connectionString=Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        var client = new MongoClient(connectionString);

        var database = client.GetDatabase("burn_in_db");
        var col=database.GetCollection<BurnStationConfiguration>("config");
        await col.InsertOneAsync(new BurnStationConfiguration() {
            HeaterConfig = new HeaterControllerConfig(),
            ProbesConfiguration = new ProbeControllerConfig(),
            StationConfiguration = new StationConfiguration()
        }, cancellationToken: cancellationToken);
        this._logger.LogInformation("Wrote to database, Starting service...");*/
        await this._stationController.Start();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Station Service Started");
        var result=await this._stationController.Stop();
        if (!result.IsError) {
            this._logger.LogInformation("Service Stopped");
        } else {
            this._logger.LogCritical($"Internal Error \n {result.FirstError.Description}");
        }
    }
    public void Dispose() {
        this._stationController.Dispose();
    }

}