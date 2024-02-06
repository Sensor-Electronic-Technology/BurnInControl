using System.Data;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public class CheckForUpdateCommand : IRequest {
    
}

public class UpdateStatusNotification : INotification {
    public UpdateStatus UpdateStatus { get; set; }
}