using BurnInControl.Data.BurnInTests;
using FastEndpoints;
using MongoDB.Driver;
namespace BurnInControl.Api.Endpoints;

public class TestLogsEndpoint:Endpoint<EmptyRequest,BurnInTestLog> {
    private readonly IMongoCollection<BurnInTestLog> _logsCollection;
    public TestLogsEndpoint(IMongoClient client) {
        var database = client.GetDatabase("burn_in_db");
        this._logsCollection= database.GetCollection<BurnInTestLog>("test_logs");
    }
    
    public override void Configure() {
        Get("/api/testlogs");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken) {
        var log = await this._logsCollection.Find(_ => true).FirstOrDefaultAsync(cancellationToken);
        if (log != null) {
            await SendAsync(log,cancellation: cancellationToken);
        }else {
            await SendNotFoundAsync(cancellationToken);
        }
        
    }
    
}