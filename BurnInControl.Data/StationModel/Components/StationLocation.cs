using MongoDB.Bson;
namespace BurnIn.Data.StationModel.Components;

public class StationPosition {
    public ObjectId _id { get; set; }
    public int Position { get; set; }
    public string? Identifier { get; set; }
    public string? Description { get; set; }
}

