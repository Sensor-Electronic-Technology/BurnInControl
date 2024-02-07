using MongoDB.Bson;
namespace BurnIn.Shared.Models.StationData;

public class StationPosition {
    public ObjectId _id { get; set; }
    public int Position { get; set; }
    public string? Identifier { get; set; }
    public string? Description { get; set; }
}

