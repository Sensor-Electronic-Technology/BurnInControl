using ErrorOr;
using MediatR;
namespace BurnInControl.Application.FirmwareUpdate.Messages;

public class UpdateFirmwareCommand:IRequest<ErrorOr<(string ver, string avrOutput)>> { }