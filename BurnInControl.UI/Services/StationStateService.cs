namespace BurnInControl.UI.Services;

public class StationStateService {
    public delegate Task AsyncEventHandler<in TArg>(TArg arg);
    
    public event AsyncEventHandler<bool>? OnServiceConnectionChanged;
    public event AsyncEventHandler<bool>? OnUsbConnectionChanged;
    public event AsyncEventHandler<bool>? OnTestStartAccepted;
    
    public bool ServiceConnected { get; set; }
    public bool UsbConnected { get; set; }
    
    public StationStateService() {
        this.ServiceConnected = false;
        this.UsbConnected = false;
    }
    
    public void SetServiceConnection(bool connected) {
        this.ServiceConnected = connected;
        this.OnServiceConnectionChanged?.Invoke(connected);
    }
    
    public void SetUsbConnection(bool connected) {
        this.UsbConnected = connected;
        this.OnUsbConnectionChanged?.Invoke(connected);
    }
    
    public void UpdateTestStartAccepted(bool accepted) {
        this.OnTestStartAccepted?.Invoke(accepted);
    }
       
}