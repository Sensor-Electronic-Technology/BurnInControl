using BurnInControl.Dashboard.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BurnInControl.Dashboard.Services;

public class VisitorTrackingService {
    private readonly IMongoCollection<VisitorTracking> _collection;
    private readonly ILogger<VisitorTrackingService> _logger;
    private VisitorTracking _tracking;
    
    public VisitorTrackingService(IMongoClient client,ILogger<VisitorTrackingService> logger) {
        this._collection = client.GetDatabase("burn-dash-tracking")
            .GetCollection<VisitorTracking>("visitor-tracking");
        this._logger = logger;
        this._tracking = this._collection.Find(_ => true).FirstOrDefault();
        if (this._tracking == null) {
            this._tracking = new VisitorTracking();
            this._tracking._id=ObjectId.GenerateNewId();
            this._tracking.CurrentVisitors = new();
            this._tracking.CurrentVisitorCount = 0;
            this._tracking.TotalVisitorCount = 0;
            this._collection.InsertOne(this._tracking);
            this._logger.LogInformation("Visitor Tracking Initialized");
        }
    }

    public async Task AddVisitor(string visitor) {
        if (this._tracking.CurrentVisitors.Contains(visitor)) {
            Console.WriteLine($"Failed to add visitor: {visitor}");
        }
        this._tracking.CurrentVisitors.Add(visitor);
        this._tracking.CurrentVisitorCount++;
        this._tracking.TotalVisitorCount++;
        await this.UpdateTracking();
        Console.WriteLine($"Current Visitors: {this._tracking.CurrentVisitorCount}");
        this._logger.LogInformation("Visitor Added, Current Visitors: " +
                                    "{VisitorCount}",this._tracking.CurrentVisitorCount);
    }
    
    public async Task RemoveVisitor(string visitor) {
        if (!this._tracking.CurrentVisitors.Contains(visitor)) {
            Console.WriteLine($"Failed to remove visitor: {visitor}");
        }
        this._tracking.CurrentVisitors.Remove(visitor);
        this._tracking.CurrentVisitorCount--;
        await this.UpdateTracking();
        this._logger.LogInformation("Visitor Removed, Current Visitors: " +
                                    "{VisitorCount}",this._tracking.CurrentVisitorCount);
    }
    
    private async Task UpdateTracking() {
        await this._collection.ReplaceOneAsync(t => t._id == this._tracking._id, this._tracking);
    }
    
}