using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
namespace BurnInControl.Application.StationControl.Interfaces;

public interface IStationController {
    public Task<ErrorOr<Success>> ConnectUsb();
    public Task<ErrorOr<Success>> Disconnect();
    public Task<ErrorOr<Success>> Stop();
    public Task<ErrorOr<Success>> Send<TPacket>(StationMsgPrefix prefix, TPacket packet) where TPacket : IPacket;
}