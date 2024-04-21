using MediatR;

namespace BurnInControl.Application.FirmwareUpdate.Messages;

public class TryUpdateFirmwareCommand:IRequest<bool> { }