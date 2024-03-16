// See https://aka.ms/new-console-template for more information

using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.StationModel;
using MongoDB.Driver;
using System.Text.Json;



async Task CreateStationDatabase() {
    var client = new MongoClient("mongodb://192.168.68.112:27017");
    StationDataService stationService = new StationDataService(client);
    Station station = new Station();
}



