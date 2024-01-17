using System.Text.Json;
namespace BurnIn.Shared.Models.Configurations;

public interface IConfiguration {
     JsonDocument Serialize();
     void Deserialize(JsonDocument doc);
}