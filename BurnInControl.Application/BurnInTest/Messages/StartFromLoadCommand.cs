﻿using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Messages;

public class StartFromLoadCommand:IRequest {
    public ControllerSavedState SavedState { get; set; }
    /*public string? Message { get; set; }
    public string? TestId { get; set; }
    public StationCurrent Current { get; set; }
    public int SetTemperature { get; set; }*/
}