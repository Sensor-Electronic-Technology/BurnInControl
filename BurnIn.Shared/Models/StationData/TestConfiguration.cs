using MongoDB.Bson;
namespace BurnIn.Shared.Models.StationData;

public class TestConfiguration {
    public ObjectId _id { get; set; }
    public string TestName { get; set; }
    public StationCurrent SetCurrent { get; set; }
    public long RunTime { get; set; }
}