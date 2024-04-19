using BurnInControl.Application.FirmwareUpdate.Interfaces;
using MediatR;
using ErrorOr;

namespace BurnInControl.Application.FirmwareUpdate.Messages;

public class CheckForUpdateCommand:IRequest{ }