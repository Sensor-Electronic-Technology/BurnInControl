using MediatR;
using MongoDB.Bson;

namespace BurnInControl.Application.BurnInTest.Messages;

public class StartLoadStateCommand:IRequest{
    public ObjectId LogId { get; set; }
}