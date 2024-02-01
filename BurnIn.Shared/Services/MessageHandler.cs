using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
namespace BurnIn.Shared.Services;

public class MessageHandler {
    private readonly BurnInTestService _testService;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ILogger<MessageHandler> _logger;
    private readonly FirmwareVersionService _firmwareService;

    public MessageHandler(ILogger<MessageHandler> logger,
        BurnInTestService testService,
        IHubContext<StationHub, IStationHub> hubContext,
        FirmwareVersionService firmwareService) {
        this._testService = testService;
        this._logger = logger;
        this._hubContext = hubContext;
        this._firmwareService = firmwareService;
    }
    
    public Task Handle(string message) {
        try {
            if (message.Contains("Prefix")) {
                var doc=JsonSerializer.Deserialize<JsonDocument>(message);
                var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
                if (!string.IsNullOrEmpty(prefixValue)) {
                    var prefix=ArduinoMsgPrefix.FromValue(prefixValue);
                    if (prefix != null) {
                        var packetElem=doc.RootElement.GetProperty("Packet");
                        prefix.When(ArduinoMsgPrefix.DataPrefix).Then(() => this.HandleData(packetElem))
                            .When(ArduinoMsgPrefix.MessagePrefix).Then(() => this.HandleMessage(packetElem, false))
                            .When(ArduinoMsgPrefix.InitMessage).Then(() => this.HandleMessage(packetElem, true))
                            .When(ArduinoMsgPrefix.IdRequest).Then(() => this.HandleIdChanged(packetElem))
                            .When(ArduinoMsgPrefix.VersionRequest).Then(()=>this.HandleVersionRequest(packetElem))
                            .When(ArduinoMsgPrefix.TestStatus).Then(()=>this.HandleTestStatus(packetElem));
                    }
                }
            } else {
                this._hubContext.Clients.All.OnSerialComMessage(message);
            }

        } catch {
            this._logger.LogWarning($"Message had errors.  Message: {message}");
        }
        return Task.CompletedTask;
    }

    private void HandleData(JsonElement element) {
        try {
            var serialData=element.Deserialize<StationSerialData>();
            if (serialData != null) {
                this._testService.Log(serialData);
                this._hubContext.Clients.All.OnSerialCom(serialData).SafeFireAndForget();
            }
        } catch(Exception e) {
            this._logger.LogWarning("Failed to deserialize station data");
        }
    }

    private void HandleMessage(JsonElement element,bool isInit) {
        var message=element.GetProperty("Message").ToString();
        this._hubContext.Clients.All.OnSerialComMessage(message).SafeFireAndForget();
    }

    private void HandleIdChanged(JsonElement element) {
        var id = element.GetString();
        this._hubContext.Clients.All.OnIdChanged(id);
    }
    
    private void HandleVersionRequest(JsonElement element) {
        try {
            var version = element.GetString();
            if (!string.IsNullOrEmpty(version)) {
                FirmwareUpdateStatus status = this._firmwareService.CheckNewerVersion(version);
                this._hubContext.Clients.All.OnUpdateChecked(status);
            } else {
                this._hubContext.Clients.All.OnUpdateChecked(new FirmwareUpdateStatus() {
                    Message = "Failed to check firmware version. Version string was null or empty",
                    UpdateReady = false,
                    Type=UpdateType.None
                });
                this._logger.LogError("Failed to check firmware version. Version string was null or empty");
            }
        } catch(Exception e) {
            this._hubContext.Clients.All.OnUpdateChecked(new FirmwareUpdateStatus() {
                Message =$"Update check failed Exception: {e.Message}",
                UpdateReady = false,
                Type=UpdateType.None
            });
            this._logger.LogError("Update check failed Exception: {Error}",e.Message);
        }
    }

    private void HandleTestStatus(JsonElement element) {
        try {
            var success = element.GetProperty("Status").GetBoolean();
            var message = element.GetProperty("Message").GetString();
            if (success) {
                this._testService.SetSetupComplete();
                this._hubContext.Clients.All.OnTestStarted();
            } else {
                if (!string.IsNullOrEmpty(message)) {
                    this._hubContext.Clients.All.OnTestStartedFailed(message);
                } else {
                    this._hubContext.Clients.All.OnTestStartedFailed("Test failed to start, cause unknown");
                }
            }
        } catch(Exception e) {
            this._hubContext.Clients.All.OnTestStartedFailed("Failed to parse test status message");
            this._logger.LogError("Update check failed Exception: {Error}",e.Message);
        }
    }
    
}