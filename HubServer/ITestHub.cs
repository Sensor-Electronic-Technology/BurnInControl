using System.Threading.Channels;
namespace HubServer;

public interface ITestHub {
    Task ShowMessage(string message);
    Task OnGetIncrement(int increment);
    
}