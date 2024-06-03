using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.ProcessSerial.Interfaces;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StationService.Infrastructure.Hub;
using System.Text.Json;
using AsyncAwaitBestPractices;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
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
                //this._logger.LogInformation(message.Message);
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
                        return this.HandleHeaterTuningComplete(packetElem);
                    }
                    case nameof(StationMsgPrefix.HeaterNotifyPrefix): {
                        return this.HandleHeaterTuneStatus(packetElem);
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
                    case nameof(StationMsgPrefix.ProbeTestDonePrefix):{
                        return this._hubContext.Clients.All.OnProbeTestDone();
                    }
                    case nameof(StationMsgPrefix.SendRunningTestPrefix): {
                        //this._logger.LogInformation("Received SendRunningTest");
                        return this.HandleTestStartedFrom(packetElem);
                    }
                    case nameof(StationMsgPrefix.NotifyHeaterModePrefix): {
                        return this.HandleNotifyHeaterMode(packetElem);
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
        StationSerialData? serialData=null;
        try {
            /*Console.WriteLine("In HandleData");
            Console.WriteLine("Received {0}",element.ToString());*/
            serialData=element.Deserialize<StationSerialData>();
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StationSerialData.\n {ErrMessage}", e.ToErrorMessage());
            await _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.DataPrefix,
                $"Error deserializing StationSerialData.\n {e.ToErrorMessage()}");
        }
        
        if (serialData != null) {
            this._hubContext.Clients.All.OnStationData(serialData).SafeFireAndForget();
            await this._mediator.Send(new LogCommand() { Data = serialData });
        }
    }
    private async Task HandleNotifyHeaterMode(JsonElement element) {
        try {
            var mode=element.GetProperty("Mode").GetInt32();
            await this._hubContext.Clients.All.OnSwTuneNotify(mode);
        } catch(Exception e) {
            this._logger.LogError("Error deserializing NotifyHeaterMode.\n {ErrMessage}", e.ToErrorMessage());
            await _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.NotifyHeaterModePrefix,
                $"Error deserializing NotifyHeaterMode.\n {e.ToErrorMessage()}");
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
            this._logger.LogError("Error deserializing TuningSerialData.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TuneComPrefix,
                $"Error deserializing TuningSerialData.\n {e.ToErrorMessage()}");
        }
    }
    private Task HandleHeaterTuneStatus(JsonElement element) {
        try {
            var tuningData=element.Deserialize<HeaterTuneResult>();
            if (tuningData != null) {
                return this._hubContext.Clients.All.OnNotifyHeaterTuningStatus(tuningData);
            }
            return Task.CompletedTask;
        } catch(Exception e) {
            this._logger.LogError("Error deserializing HeaterTuneResult.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TuneComPrefix,
                $"Error deserializing HeaterTuneResult.\n {e.ToErrorMessage()}");
        }
    }
    private Task HandleHeaterTuningComplete(JsonElement element) {
        try {
            var results=element.GetProperty("AutoTuneResults").Deserialize<List<HeaterTuneResult>>();
            if (results != null) {
                return this._hubContext.Clients.All.OnNotifyHeaterTuneComplete(results);
            }
            return Task.CompletedTask;
        } catch(Exception e) {
            this._logger.LogError("Error deserializing HeaterTuneResult.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TuneComPrefix,
                $"Error deserializing HeaterTuneResult.\n {e.ToErrorMessage()}");
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
            return this._hubContext.Clients.All.OnSerialComMessage((int)message.MessageType,message.Message);
        } catch(Exception e) {
            this._logger.LogError("Error deserializing StationMessagePacket.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.MessagePrefix,
                $"Error deserializing StationMessagePacket.\n {e.ToErrorMessage()}");
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
            this._logger.LogError("Error deserializing ConfigSaveStatus.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.SaveConfigStatusPrefix,
                $"Error deserializing ConfigSaveStatus.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleGetConfigResponse(JsonElement element) {
        try {
            Console.WriteLine("Handling GetConfigResponse");
            if (element.GetProperty("ConfigType").TryGetInt32(out var configType)) {
                if (ConfigType.TryFromValue(configType, out var type)) {
                    return this._hubContext.Clients.All.OnRequestConfigHandler(true, 
                        type.Value, 
                        element.GetProperty("Configuration").ToString());
                }
                return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                    $"Error while parsing config type ConfigTypeValue: {configType}");
            }
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                "Error while deserializing config type");
        } catch(Exception e) {
            this._logger.LogError("Error deserializing ConfigType.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.GetConfigPrefix,
                $"Error deserializing ConfigType.\n {e.ToErrorMessage()}");
        }
    }

    private Task HandleTestCompleted(JsonElement element) {
        this._mediator.Send(new TestCompleteCommand());
        return this._hubContext.Clients.All.OnTestCompleted("Test Completed");   
    }
    
    private Task HandleTestStatus(JsonElement element) {
        try {
            Console.WriteLine("Handling TestStatus. Deserializing");
            Console.WriteLine($"Received {element.ToString()}");
            var success = element.GetProperty("Status").GetBoolean();
            var message = element.GetProperty("Message").GetString();
            return this._mediator.Send(new StartTestCommand() {
                Status= success,
                Message = message,
            });
        } catch(Exception e) {
            this._logger.LogError("Error deserializing TestStatus.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartStatusPrefix,
                $"Error deserializing TestStatus.\n {e.ToErrorMessage()}");
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
            this._logger.LogError("Failed to parse ControllerSavedState");
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartFromLoadPrefix,
                "Failed to parse ControllerSavedState");

        } catch(Exception e) {
            this._logger.LogError("Error deserializing ControllerSavedState.\n {ErrMessage}", e.ToErrorMessage());
            return _hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartFromLoadPrefix,
                $"Error deserializing ControllerSavedState.\n {e.ToErrorMessage()}");
        }
    }
}