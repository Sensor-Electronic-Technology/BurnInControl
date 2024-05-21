using AsyncAwaitBestPractices;
using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.ProcessSerial.Interfaces;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StationService.Infrastructure.Firmware;
using StationService.Infrastructure.Hub;
using StationService.Infrastructure.TestLogs;
using System.Text.Json;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared;
using MediatR;
namespace StationService.Infrastructure.SerialCom;

public class StationMessageHandler : IStationMessageHandler {
    private readonly ILogger<StationMessageHandler> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ISender _mediator;
    
    public StationMessageHandler(ILogger<StationMessageHandler> logger,
        IHubContext<StationHub, IStationHub> hubContext,
        ISender mediator) {
        this._logger = logger;
        this._hubContext = hubContext;
        this._mediator = mediator;
    }

    public Task Handle(StationMessage message,CancellationToken cancellationToken) {
        try {
            if (!string.IsNullOrEmpty(message.Message)) {
                this._logger.LogInformation(message.Message);
                if (message.Message.Contains("Prefix")) {
                    var doc=JsonSerializer.Deserialize<JsonDocument>(message.Message);
                    if (doc != null) { 
                        return this.Parse(doc);
                    } else {
                        this._logger.LogWarning("JsonDocument was null");
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
        } catch(Exception e) {
            var exceptionMessage = $"\nException: {e.Message}";
            if(e.InnerException!=null) {
                exceptionMessage+= $"\n Inner Exception: {e.InnerException.Message}";
            }
            this._logger.LogWarning($"Message had errors.  Message: {message.Message} {exceptionMessage}");
            return Task.CompletedTask;
        }
    }
    
    private Task Parse(JsonDocument doc) {
        var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
        if (!string.IsNullOrEmpty(prefixValue)) {
            var prefix=StationMsgPrefix.FromValue(prefixValue);
            if (prefix != null) {
                var packetElem=doc.RootElement.GetProperty("Packet");
                switch (prefix.Name) {
                    case nameof(StationMsgPrefix.DataPrefix): {
                        //Send to UI and BurnInTestService
                        return this.HandleData(packetElem);
                    }
                    case nameof(StationMsgPrefix.TuneComPrefix): {
                        //Send to UI
                        return this.HandleTuneData(packetElem);
                    }
                    case nameof(StationMsgPrefix.MessagePrefix): {
                        //Send to UI
                        return this.HandleMessage(packetElem, false);
                    }
                    case nameof(StationMsgPrefix.HeaterTuneCompletePrefix): {
                        //TODO: Add handle for received Tuning results
                        //Send to UI and wait for Save or Discard
                        return Task.CompletedTask;
                        //return this.HandleIdChanged(packetElem);
                    }
                    case nameof(StationMsgPrefix.HeaterNotifyPrefix): {
                        //TODO: Add Handle for heater notify that tune is completed
                        // This only notifies the user of progress
                        //Send to UI
                        return Task.CompletedTask;
                    }
                    case nameof(StationMsgPrefix.TestStartStatusPrefix): {
                        //Send to BurnInTestService and start logging
                        return this.HandleTestStatus(packetElem);
                    }
                    case nameof(StationMsgPrefix.TestStartFromLoadPrefix): {
                        //Send to BurnInTestService.  Load and start test
                        return this.HandleTestStartedFrom(packetElem);
                    }
                    case nameof(StationMsgPrefix.TestCompletedPrefix): {
                        //Send to BurnInTestService and complete test
                        return this.HandleTestCompleted(packetElem);
                    }
                    case nameof(StationMsgPrefix.IdRequestPrefix): {
                        //Send to controller and respond with ACK
                        return this._mediator.Send(new SendAckCommand() {
                            AcknowledgeType = AcknowledgeType.IdAck
                        });
                    }
                    case nameof(StationMsgPrefix.VersionRequestPrefix): {
                        //Send to firmware Updated and respond with ACK
                        return this._mediator.Send(new SendAckCommand() {
                            AcknowledgeType = AcknowledgeType.VersionAck
                        });
                    }
                    case nameof(StationMsgPrefix.SaveConfigStatusPrefix): {
                        //Receive status of save config
                        return this.HandleConfigSaveStatus(packetElem);
                    }
                    case nameof(StationMsgPrefix.GetConfigPrefix): {
                        return this.HandleGetConfigResponse(packetElem);
                    }
                    default: {
                        _logger.LogWarning("Prefix value {Value} not implemented",prefix.Value);
                        return Task.CompletedTask;
                    }
                }
            } else {
                _logger.LogWarning("ArduinoMsgPrefix.FromValue(prefixValue) was null");
                return Task.CompletedTask;
            }
        } else {
            _logger.LogWarning("Prefix value null or empty");
            return Task.CompletedTask;
        }
    }
    
    private async Task HandleData(JsonElement element) {
        try {
            var serialData=element.Deserialize<StationSerialData>();
            if (serialData != null) {
                await this._mediator.Send(new LogCommand() { Data = serialData });
                await this._hubContext.Clients.All.OnStationData(serialData);
            }
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            await _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.DataPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }
    
    private Task HandleTuneData(JsonElement element) {
        try {
            var tuningData=element.Deserialize<TuningSerialData>();
            if (tuningData != null) {
                return this._hubContext.Clients.All.OnTuningData(tuningData);
            }
            return Task.CompletedTask;
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TuneComPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleMessage(JsonElement element,bool isInit) {
        try {
            var message = element.Deserialize<StationMessagePacket>();
            if (message == null) {
                this._logger.LogError("Error StationMessagePacket null");
                return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.MessagePrefix,
                    $"Error StationMessagePacket null");
            }
            switch (message.MessageType) {
                case StationMessageType.INIT: {
                    return this._hubContext.Clients.All.OnSerialInitMessage(message.Message);
                }
                case StationMessageType.GENERAL: {
                    return this._hubContext.Clients.All.OnSerialComMessage(message.Message);
                }
                case StationMessageType.NOTIFY: {
                    return this._hubContext.Clients.All.OnSerialNotifyMessage(message.Message);
                }
                case StationMessageType.ERROR: {
                    return this._hubContext.Clients.All.OnSerialErrorMessage(message.Message);
                }
                default: {
                    this._logger.LogError("Error StationMessageType {Type} not implemented",message.MessageType);
                    return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.MessagePrefix,
                        $"Error StationMessageType {message.MessageType} not implemented");
                }
            }
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.MessagePrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }
    
    private Task HandleConfigSaveStatus(JsonElement element) {
        try {
            var configTypeInt=element.GetProperty("Type").GetInt32();
            if (ConfigType.TryFromValue(configTypeInt, out var configType)) {
                var success = element.GetProperty("Status").GetBoolean();
                var message = element.GetProperty("Message").GetString() ?? "Unknown";
                return this._hubContext.Clients.All.OnConfigSaveStatus(configType.Name, success, message);
            } else {
                return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.SaveConfigStatusPrefix,
                    "Error while deserializing config type");
            }

        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.SaveConfigStatusPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleGetConfigResponse(JsonElement element) {
        try {
            if (element.GetProperty("ConfigType").TryGetInt32(out var configType)) {
                if (ConfigType.TryFromValue(configType, out var type)) {
                    switch (type.Name) {
                        case nameof(ConfigType.HeaterControlConfig): {
                            var heaterConfig=element.GetProperty("Configuration").Deserialize<HeaterControllerConfig>();
                            if (heaterConfig != null) {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(true, type, heaterConfig);
                            } else {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(false, type, heaterConfig);
                            }
                        }
                        case nameof(ConfigType.ProbeControlConfig): {
                            var probeConfig=element.GetProperty("Configuration").Deserialize<ProbeControllerConfig>();
                            if (probeConfig != null) {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(true, type, probeConfig);
                            } else {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(false, type, probeConfig);
                            }
                        }
                        case nameof(ConfigType.ControllerConfig): {
                            var stationConfig=element.GetProperty("Configuration").Deserialize<StationConfiguration>();
                            if (stationConfig != null) {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(true, type, stationConfig);
                            } else {
                                return this._hubContext.Clients.All.OnRequestConfigHandler(false, type, stationConfig);
                            }
                        }
                        default: {
                            this._logger.LogError("ConfigType {ConfigType} not implemented", configType);
                            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                                $"ConfigType {configType} not implemented");
                        }
                    }
                }
                return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                    $"Error while parsing config type ConfigTypeValue: {configType}");
            }
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                "Error while deserializing config type");
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleTestCompleted(JsonElement element) {
        this._mediator.Send(new TestCompleteCommand());
        return this._hubContext.Clients.All.OnTestCompleted("Test Completed");   
    }
    
    private Task HandleTestStatus(JsonElement element) {
        try {
            var success = element.GetProperty("Status").GetBoolean();
            var message = element.GetProperty("Message").GetString();
            return this._mediator.Send(new StartTestCommand() {
                Status= success,
                Message = message,
            });
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartStatusPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleTestStartedFrom(JsonElement element) {
        try {
            Console.WriteLine(element.ToString());
            var startFromPacket = element.Deserialize<ControllerSavedState>();
            if (startFromPacket != null) {
                return this._mediator.Send(new StartFromLoadCommand() {
                    SavedState=startFromPacket
                });
            }
            this._logger.LogError("Failed to parse StartTestFromPacket");
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartFromLoadPrefix,
                "Failed to parse StartTestFromPacket");

        } catch(Exception e) {
            this._logger.LogError("Error deserializing StartTestFromPacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartFromLoadPrefix,
                $"Error deserializing StartTestFromPacket.\n {e.ToErrorMessage()}");
        }
    }
}