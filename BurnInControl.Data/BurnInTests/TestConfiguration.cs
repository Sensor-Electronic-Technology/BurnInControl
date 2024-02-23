using BurnInControl.Data.StationModel.Components;
using MongoDB.Bson;
namespace BurnInControl.Data.BurnInTests;

public class TestConfiguration {
    public ObjectId _id { get; set; }
    public string TestName { get; set; }
    public StationCurrent SetCurrent { get; set; }
    public long RunTime { get; set; }
}