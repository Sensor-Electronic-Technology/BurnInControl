using Radzen;
namespace BurnInControl.UI.Services;

public class ConsoleWriter {
    public string Message { get; private set; }
    public AlertStyle AlertStyle { get; private set; }
    public event Action<string,AlertStyle>? OnMessageChanged;
    
    public void LogMessage(string message,AlertStyle alertStyle=AlertStyle.Info) {
        this.Message = message;
        this.AlertStyle= alertStyle;
        this.NotifyMessageChanged();
    }
    
    private void NotifyMessageChanged() {
        this.OnMessageChanged?.Invoke(this.Message, this.AlertStyle);
    }
    
}