using BurnIn.Data.StationModel.Components;
using MongoDB.Bson;
namespace BurnIn.Data.StationModel.TestLogs;

public class TestConfiguration {
    public ObjectId _id { get; set; }
    public string TestName { get; set; }
    public StationCurrent SetCurrent { get; set; }
    public long RunTime { get; set; }
}