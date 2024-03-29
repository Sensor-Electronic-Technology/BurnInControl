﻿using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
using MongoDB.Bson;
namespace BurnInControl.Data.BurnInTests;

public class BurnInTestLog {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public StationCurrent? SetCurrent { get; set; }
    public long RunTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public bool Completed { get; set; }
    
    public long ElapsedTime { get; set; }
    
    public List<WaferSetup> TestSetup { get; set; } = new List<WaferSetup>();
    public List<StationReading> Readings { get; set; } = new List<StationReading>();

    public void StartNew(List<WaferSetup> setup) {
        this.Clear();
        this.TestSetup = setup;
    }

    public void SetStart(DateTime start,StationSerialData data) {
        this.StartTime = start;
        this.Readings.Add(new StationReading() {
            TimeStamp = start,
            Data=data
        });
    }

    public void SetCompleted(DateTime stop) {
        this.StopTime = stop;
    }
    public void AddReading(StationSerialData data) {
        this.Readings.Add(new StationReading() {
            TimeStamp = DateTime.Now,
            Data=data
        });
    }

    public ErrorOr<IEnumerable<WaferResult>> GetReading(string waferId) {
        var waferSetup=this.TestSetup.FirstOrDefault(e => e.WaferId == waferId);
        if (waferSetup != null) {
            var waferResults=this.Readings.Select(e => new WaferResult() {
                TimeStamp = e.TimeStamp,
                Probe1RunTime = e.Data.ProbeRuntimes[waferSetup.Probe1.Value-1],
                Probe2RunTime = e.Data.ProbeRuntimes[waferSetup.Probe2.Value-1],
                Probe1Current = e.Data.Currents[waferSetup.Probe1.Value-1],
                Probe2Current = e.Data.Currents[waferSetup.Probe2.Value-1],
                Probe1Voltage = e.Data.Voltages[waferSetup.Probe1.Value-1],
                Probe2Voltage = e.Data.Voltages[waferSetup.Probe2.Value-1],
                PocketTemperature = e.Data.Temperatures[waferSetup.StationPocket.Value]
            });
            return ErrorOrFactory.From(waferResults);
        }
        return Error.NotFound(description:"Wafer not found");
    }
    public void Clear() {
        this.TestSetup.Clear();
        this.Readings.Clear();
        this._id = default;
        this.StartTime = default;
        this.StopTime = default;
    }
}