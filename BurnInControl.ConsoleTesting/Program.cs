// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using BurnInControl.ConsoleTesting.TestWorkflow;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net.Http;
using System.Text.Json;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.FirmwareData;
using MongoDB.Bson;
using Octokit;
using Octokit.Internal;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services;

//await TestWorkFlow();

//TestStateMachine();

//await CheckLatest();
//await CreateRelease();
//await GetVersionFromString();

/*await TestRunProcess();

async Task TestRunProcess() {
    using Process process = new Process();
    ProcessStartInfo startInfo = new ProcessStartInfo(" C:\\Program Files\\Arduino CLI\\arduino-cli.exe");
    startInfo.Arguments="upload -p COM3 -i \"C:\\Users\\aelmendo\\Documents\\Arduino\\burn-build\\BurnInFirmwareV3.ino.hex\" -b arduino:avr:mega -v --log";
    startInfo.RedirectStandardOutput = true;
    startInfo.UseShellExecute = false;
    process.StartInfo = startInfo;
    process.Start();
    var output=await process.StandardOutput.ReadToEndAsync();
    process.StandardOutput.
    Console.WriteLine(output);
}*/
/*await CreateStationDatabase();
await CreateTrackerDatabase();*/

var objectId=ObjectId.GenerateNewId().ToString();
Console.WriteLine(objectId);
//await CloneDatabase();

async Task CloneDatabase() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var piClient = new MongoClient("mongodb://192.168.68.112:27017");
    var database = client.GetDatabase("burn_in_db");
    var piDatabase = piClient.GetDatabase("burn_in_db");
    
    var stationCollection=database.GetCollection<Station>("stations");
    var piStationCollection=piDatabase.GetCollection<Station>("stations");
    
    var stations=await stationCollection.Find(Builders<Station>.Filter.Empty).ToListAsync();
    foreach(var station in stations) {
        await piStationCollection.InsertOneAsync(station);
    }
    
    var trackerCollection=database.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var piTrackerCollection=piDatabase.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var trackers=await trackerCollection.Find(Builders<StationFirmwareTracker>.Filter.Empty).ToListAsync();
    foreach(var tracker in trackers) {
        await piTrackerCollection.InsertOneAsync(tracker);
    }
    
    var versionCollection=database.GetCollection<VersionLog>("version_log");
    var piVersionCollection=piDatabase.GetCollection<VersionLog>("version_log");
    var versions=await versionCollection.Find(Builders<VersionLog>.Filter.Empty).ToListAsync();
    foreach(var version in versions) {
        await piVersionCollection.InsertOneAsync(version);
    }
    
}


async Task CreateTrackerDatabase() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var trackerCollection=database.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var stationCollection=database.GetCollection<Station>("stations");
    var stations=await stationCollection.Find(Builders<Station>.Filter.Empty).ToListAsync();
    foreach(var station in stations) {
        StationFirmwareTracker tracker = new StationFirmwareTracker();
        tracker._id=ObjectId.GenerateNewId();
        tracker.StationId=station.StationId;
        tracker.CurrentVersion="V0.0.0";
        tracker.AvailableVersion="";
        tracker.UpdateAvailable = false;
        Console.WriteLine($"StationId {tracker.StationId}");
        await trackerCollection.InsertOneAsync(tracker);
    }
}

async Task UpdateFirmware() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    var firmwarePath = "/source/ControlUpload/";
    var firmwareFileName = "BurnInFirmwareV3.ino.hex";
    var command = ""; 
    var program = "arduino-cli";
    var firmwareFullPath = firmwarePath + firmwareFileName;
}


async Task CheckLatest() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    GitHubClient github = new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"));
    var release=await github.Repository.Release.GetLatest(org, repo);
    Console.WriteLine($"Release: {release.Name}");
}

async Task CreateRelease() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    GitHubClient github = new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"),
        new InMemoryCredentialStore(new Credentials("aelmendorf","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")));

    
    /*var commit=(await github.Repository.Commit.GetAll(org, repo)).First();*/
    var refs = await github.Git.Reference.Get(org, repo, "heads/main");
    Console.WriteLine($"Sha: {refs.Object.Sha}");
    var commit=await github.Git.Commit.Get(org, repo, refs.Object.Sha);
    Console.WriteLine($"Sha: {commit.Sha}");
    Console.ReadKey();
    var tag = new NewTag {
        Message = "New Release tag",
        Tag = "V0.1.0",
        Type = TaggedType.Commit, // TODO: what are the defaults when nothing specified?
        Object = commit.Sha,
        Tagger = new Committer("aelmendorf","aelmendorf234@gmail.com",DateTimeOffset.Now)
    };
    
    var tadResult=await github.Git.Tag.Create(org,repo,tag);
    Console.WriteLine($"Tag: {tadResult.Tag}");
    var newRelease = new NewRelease(tag.Tag);
    newRelease.MakeLatest = MakeLatestQualifier.True;
    newRelease.Name = "Version: "+tag.Tag;
    newRelease.Body = "Release: "+tag.Tag;
    newRelease.Draft = false;
    newRelease.Prerelease = false;
    
    var result = await github.Repository.Release.Create(org, repo, newRelease);
    await using var archiveContents = File.OpenRead(@"C:\Users\aelmendo\Documents\Arduino\burn-build\BurnInFirmwareV3.ino.hex");
        
    var assetUpload = new ReleaseAssetUpload() 
    {
        FileName = "BurnInFirmwareV3.ino.hex",
        ContentType = "application/file",
        RawData = archiveContents
    };
    
    var release = await github.Repository.Release.Get(org,repo,result.Id);
    var asset = await github.Repository.Release.UploadAsset(release, assetUpload);
    Console.WriteLine($"Uploaded asset: {asset.Name}, {asset.Id}");
}

async Task GetVersionFromString() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<VersionLog>("version_log");
    var latest = "V0.0.0";
    var split=latest.Split('.');
    var startidx=latest.IndexOf('V');
    var endidx=latest.IndexOf('.');
    var majorStr = latest.Substring(startidx + 1, (endidx - startidx) - 1);
    var minorstr = split[1];
    var patchstr = split[2];

    VersionLog versionLogEntry = new VersionLog();
    versionLogEntry._id= ObjectId.GenerateNewId();
    versionLogEntry.Version = latest;
    versionLogEntry.Major = int.Parse(majorStr);
    versionLogEntry.Minor = int.Parse(minorstr);
    versionLogEntry.Patch = int.Parse(patchstr);
    versionLogEntry.Latest = true;

    await collection.InsertOneAsync(versionLogEntry);
    Console.WriteLine("Check Database");

    Console.WriteLine($"Version: {versionLogEntry.Version}, Major: {versionLogEntry.Major}, Minor: {versionLogEntry.Minor}, Patch: {versionLogEntry.Patch}");

    foreach(var str in split) {
        Console.WriteLine(str);
    }
}

/*HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Add("Authorization", "Bearer mytoken");
//client.BaseAddress = new Uri("192.168.68.112:8080");
var response=await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,"http://192.168.68.112:8080/v1/update"));
if (response.StatusCode == HttpStatusCode.OK) {
    Console.WriteLine("Updates ran");
} else {
    Console.WriteLine("Updates Failed");
}*/


/*string text = "AutoTune";
bool sw=false;
sw = true;
for (int i = 0; i < 10; i++) {
    sw = !sw;
    text=sw ? "AutoTune":"Heating";
    Console.WriteLine(text);
}*/




/*MessagePacket<StationCommand> packet = new MessagePacket<StationCommand>() {
    Prefix = StationMsgPrefix.CommandPrefix,
    Packet = StationCommand.Reset
};*/
/*var jsonString=await File.ReadAllTextAsync(@"C:\temp\jsonTest.txt");
try {
    var doc=JsonSerializer.Deserialize<JsonDocument>(jsonString);
    var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
    var prefix=StationMsgPrefix.FromValue(prefixValue);
    Console.WriteLine($"Prefix: {prefix.Value}");
    var packetElem=doc.RootElement.GetProperty("Packet");

    var serialData = packetElem.Deserialize<StationSerialData>();
    Console.WriteLine($"Object: CurrentSetPoint: {serialData.CurrentSetPoint}");
}catch(Exception e) {
    Console.WriteLine(e.Message);
}*/

//var output=JsonSerializer.Serialize<MessagePacket<StationCommand>>(packet);
/*Console.WriteLine("Input: ");
Console.WriteLine(jsonString);*/




void PrintMenu() {
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("1. Startup");
    Console.WriteLine("2. Connect");
    Console.WriteLine("3. Start");
    Console.WriteLine("4. Pause");
    Console.WriteLine("5. Continue");
    Console.WriteLine("6. Disconnect");
    Console.WriteLine("7. Exit");
    Console.WriteLine();
}

async Task TestWorkFlow() {
    /*var serviceProvider = ConfigureServices();
    var host=serviceProvider.GetService<IWorkflowHost>();*/
    //var workflowHost=new WorkflowHost();
    /*host.RegisterWorkflow<DataWorkflow,WorkflowData>();
    host.Start();
    host.StartWorkflow(nameof(DataWorkflow),new WorkflowData() {
        Message = "Message",
        Success = false
    });*/

}

static IServiceProvider ConfigureServices()
{
    //setup dependency injection
    IServiceCollection services = new ServiceCollection();
    //services.AddLogging();
    /*services.AddWorkflow();*/
    
    services.AddWorkflow(x => x.UseMongoDB(@"mongodb://172.20.3.41:27017", "burn_test_workflow"));
    var serviceProvider = services.BuildServiceProvider();

    return serviceProvider;
}


async Task CreateStationDatabase() {
    var client = new MongoClient("mongodb://192.168.68.112:27017");
    StationDataService stationService = new StationDataService(client);
    Station station = new Station();
}



