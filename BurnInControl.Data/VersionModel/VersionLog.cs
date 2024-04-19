using MongoDB.Bson;

namespace BurnInControl.Data.VersionModel;

public class VersionLog {
    public ObjectId _id { get; set; }
    public string Version { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public bool Latest { get; set; }
}