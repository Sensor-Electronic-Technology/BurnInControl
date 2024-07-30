using MongoDB.Bson;

namespace BurnInControl.Dashboard.Data;

public class VisitorTracking {
    public ObjectId _id { get; set; }
    public int CurrentVisitorCount { get; set; }
    public ulong TotalVisitorCount { get; set; }
    public List<string> CurrentVisitors { get; set; } = new();
}