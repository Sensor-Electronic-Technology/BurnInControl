﻿using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.Util;
namespace BurnInControl.Shared.Hubs;

public interface IStationHub {
    Task OnSerialCom(StationSerialData serialData);

    Task OnSerialComMessage(string message);

    Task OnTestStatus(StartTestStatus status);
}