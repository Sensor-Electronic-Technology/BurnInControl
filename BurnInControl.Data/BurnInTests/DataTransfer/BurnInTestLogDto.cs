using MongoDB.Bson;

namespace BurnInControl.Data.BurnInTests.DataTransfer;

public class BurnInTestLogDto {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public string SetCurrent { get; set; }
    public int SetTemperature { get; set; }
    public long RunTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public bool Completed { get; set; }
    public long ElapsedTime { get; set; }
    public string LeftPocket { get; set; }
    public string MiddlePocket { get; set; }
    public string RightPocket { get; set; }
}