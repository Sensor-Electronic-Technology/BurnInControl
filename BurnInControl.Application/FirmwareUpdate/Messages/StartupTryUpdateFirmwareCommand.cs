using ErrorOr;
using MediatR;
namespace BurnInControl.Application.FirmwareUpdate.Messages;

public class StartupTryUpdateFirmwareCommand:IRequest<bool> { }