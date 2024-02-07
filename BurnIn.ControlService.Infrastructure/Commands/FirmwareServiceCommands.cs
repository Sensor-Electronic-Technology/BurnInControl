using BurnIn.Shared;
using BurnIn.Shared.Models;
using System.Data;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public record UpdateResponse(string Version, string UploadText);

public class GetLatestVersionCommand : IRequest { }

public class CheckIfNewerVersion : IRequest {
    public string? ControllerVersion { get; set; }
}

public class UpdateCommand : IRequest<UpdateResponse> { }

public class ControllerVersionReceived : INotification {
    public string? ControllerVersion { get; set; }
}



