using AsyncAwaitBestPractices;
using BurnIn.ControlService.Infrastructure.Commands;
using BurnIn.ControlService.Infrastructure.Services;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
namespace BurnIn.Shared.Services;

public class MessageHandler:IRequestHandler<ProcessSerialCommand> {
    private readonly BurnInTestService _testService;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ILogger<MessageHandler> _logger;
    private readonly FirmwareUpdateService _firmwareService;
    private readonly IMediator _mediator;

    public MessageHandler(ILogger<MessageHandler> logger,
        BurnInTestService testService,
        IHubContext<StationHub, IStationHub> hubContext,
        FirmwareUpdateService firmwareService,
        IMediator mediator) {
        this._testService = testService;
        this._logger = logger;
        this._hubContext = hubContext;
        this._firmwareService = firmwareService;
        this._mediator = mediator;
    }
    
    public Task Handle(ProcessSerialCommand request, CancellationToken cancellationToken) {
        try {
            if (!string.IsNullOrEmpty(request.Message)) {
                if (request.Message.Contains("Prefix")) {
                    var doc=JsonSerializer.Deserialize<JsonDocument>(request.Message);
                    var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
                    if (!string.IsNullOrEmpty(prefixValue)) {
                        var prefix=ArduinoMsgPrefix.FromValue(prefixValue);
                        if (prefix != null) {
                            var packetElem=doc.RootElement.GetProperty("Packet");
                            switch (prefix) {
                                case nameof(ArduinoMsgPrefix.DataPrefix): {
                                    return this.HandleData(packetElem);
                                }
                                case nameof(ArduinoMsgPrefix.MessagePrefix): {
                                    return this.HandleMessage(packetElem, false);
                                }
                                case nameof(ArduinoMsgPrefix.InitMessage): {
                                    return this.HandleMessage(packetElem, false);
                                }
                                case nameof(ArduinoMsgPrefix.IdRequest): {
                                    return this.HandleIdChanged(packetElem);
                                }
                                case nameof(ArduinoMsgPrefix.VersionRequest): {
                                    return this.HandleVersionRequest(packetElem);
                                }
                                case nameof(ArduinoMsgPrefix.TestStatus): {
                                    return this.HandleTestStatus(packetElem);
                                }
                                default: {
                                    this._logger.LogWarning($"Prefix value {prefix.Value} not implemented");
                                    return Task.CompletedTask;
                                }
                            }
                        } else {
                            this._logger.LogWarning("ArduinoMsgPrefix.FromValue(prefixValue) was null");
                            return Task.CompletedTask;
                        }
                    } else {
                        this._logger.LogWarning("Prefix value null or empty");
                        return Task.CompletedTask;
                    }
                } else {
                    this._logger.LogWarning("MessagePacket did not contain Prefix");
                    return Task.CompletedTask;
                }
            } else {
                this._logger.LogWarning("Mediator request MessagePacket json text was null or empty");
                return Task.CompletedTask;
            }
        } catch {
            this._logger.LogWarning($"Message had errors.  Message: {request.Message}");
            return Task.CompletedTask;
        }
    }
    
    /*public Task Handle(string message) {
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
    }*/

    private Task HandleData(JsonElement element) {
        try {
            var serialData=element.Deserialize<StationSerialData>();
            if (serialData != null) {
                
                this._testService.Log(serialData);
                this._hubContext.Clients.All.OnSerialCom(serialData).SafeFireAndForget();
            }
        } catch(Exception e) {
            this._logger.LogWarning("Failed to deserialize station data");
        }
        return Task.CompletedTask;
    }

    private Task HandleMessage(JsonElement element,bool isInit) {
        var message=element.GetProperty("Message").ToString();
        return this._hubContext.Clients.All.OnSerialComMessage(message);
    }

    private Task HandleIdChanged(JsonElement element) {
        try {
            var id = element.GetString();
            return this._mediator.Publish(new ControllerIdReceived() {
                ControllerId = id
            });
        } catch {
            this._logger.LogError("Failed to parse Controller Id");
            return Task.CompletedTask;
        }
    }
    
    private async Task HandleVersionRequest(JsonElement element) {
        try {
            var version = element.GetString();
            if (!string.IsNullOrEmpty(version)) {
                await this._mediator.Send(new CheckIfNewerVersion() {
                    ControllerVersion = version
                });
            } else {
                this._logger.LogError("Failed to check firmware version. Version string was null or empty");
            }
        } catch(Exception e) {
            this._logger.LogError("Update check failed Exception: {Error}",e.Message);
        }
    }

    private Task HandleTestStatus(JsonElement element) {
        try {
            var success = element.GetProperty("Status").GetBoolean();
            var message = element.GetProperty("Message").GetString();
            if (success) {
                return this._mediator.Publish(new TestStartedStatus() {
                    Status = ResultFactory.Success(message)
                });
            } else {
                return this._mediator.Publish(new TestStartedStatus() {
                    Status = ResultFactory.Error(message)
                });
            }
        } catch(Exception e) {
            var message = $"Failed to parse Test Status message packet. Exception: {e.Message}";
            this._logger.LogError(message);
            return this._mediator.Publish(new TestStartedStatus() {
                Status = ResultFactory.Error(message)
            });
        }
    }
}