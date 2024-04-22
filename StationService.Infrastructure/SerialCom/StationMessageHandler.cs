﻿using AsyncAwaitBestPractices;
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
using BurnInControl.Data.StationModel.Components;
using MediatR;
namespace StationService.Infrastructure.SerialCom;

public class StationMessageHandler:IStationMessageHandler{
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ILogger<StationMessageHandler> _logger;
    private readonly IMediator _mediator;
    
    public StationMessageHandler(ILogger<StationMessageHandler> logger,
        IHubContext<StationHub, IStationHub> hubContext,
        IMediator mediator) {
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
                        return Task.CompletedTask;
                    }
                    case nameof(StationMsgPrefix.VersionRequestPrefix): {
                        //Send to firmware Updated and respond with ACK
                        return Task.CompletedTask;
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
    }
    
    private Task HandleData(JsonElement element) {
        try {
            var serialData=element.Deserialize<StationSerialData>();
            if (serialData != null) {
                this._mediator.Send(new LogCommand() { Data = serialData });
                return this._hubContext.Clients.All.OnStationData(serialData);
            }
            return Task.CompletedTask;
        } catch(Exception e) {
            this._logger.LogWarning("Failed to deserialize station data");
            return Task.CompletedTask;
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
            this._logger.LogWarning("Failed to deserialize station data");
            return Task.CompletedTask;
        }
    }

    private Task HandleMessage(JsonElement element,bool isInit) {
        try {
            var message = element.Deserialize<StationMessagePacket>();
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
                    var logMessage = "Error: Invalid StationMessagePacket in StationMessageType";
                    this._logger.LogWarning(logMessage);
                    return this._hubContext.Clients.All.OnSerialErrorMessage(logMessage);
                }
            }
        } catch(Exception e) {
            var errorMessage = "Exception: "+e.Message;
            if (e.InnerException != null) {
                errorMessage += "\n InnerException: " + e.InnerException;
            }
            this._logger.LogError($"Error deserializing SystemMessagePacket.\n {errorMessage}");
            return this._hubContext.Clients.All.OnSerialErrorMessage($"Error deserializing SystemMessagePacket.\n {errorMessage}");
        }
    }

    private Task HandleTestCompleted(JsonElement element) {
        this._mediator.Send(new TestCompletedMessage());
        return this._hubContext.Clients.All.OnTestCompleted("Test Completed");   
    }
    
    private Task HandleTestStatus(JsonElement element) {
        try {
            var success = element.GetProperty("Status").GetBoolean();
            var message = element.GetProperty("Message").GetString();
            var testId=element.GetProperty("TestId").GetString();
            return this._mediator.Send(new StartStatusCommand() {
                Status= success,
                Message = message,
                TestId = testId,
            });
        } catch(Exception e) {
            var message = $"Failed to parse test status message packet. Exception: {e.Message}";
            this._logger.LogError(message);
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartStatusPrefix,$"Error: {message}");
        }
    }

    private Task HandleTestStartedFrom(JsonElement element) {
        try {
            var message = element.GetProperty("Message").GetString();
            var testId=element.GetProperty("TestId").GetString();
            var current=element.GetProperty("SetCurrent").GetInt32();
            var setTemp=element.GetProperty("SetTemp").GetInt32();
            return this._mediator.Send(new StartFromLoadCommand() {
                Message=message,
                TestId=testId,
                Current = StationCurrent.FromValue(current),
                SetTemperature=setTemp
            });
        } catch(Exception e) {
            var message = $"Failed to parse test status message packet. Exception: {e.Message}";
            this._logger.LogError(message);
            return this._hubContext.Clients.All.OnSerialComError(StationMsgPrefix.TestStartStatusPrefix,$"Error: {message}");
        }
    }
}