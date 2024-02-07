using BurnIn.Shared.Models;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public enum StationCommand {
    ConnectUsb,
    DisconnectUsb,
    SendCommand,
    RequestId,
    CheckUpdateAvailable,
    SetupTest
}