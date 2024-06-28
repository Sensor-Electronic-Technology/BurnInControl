using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.QuickTest;
using BurnInControl.UI.Data;
using Radzen;

namespace BurnInControl.UI.Services;

public class TestSetupService {
    private QuickTestDataService _qtDataService;
    public List<PocketWaferSetup> WaferSetups { get; set; }=new List<PocketWaferSetup>();
    public List<SetupAlert> SetupAlerts { get; set; }=new List<SetupAlert>();
    public int SetupCount => this.WaferSetups.Count;
    public int AlertCount => this.SetupAlerts.Count;
    public bool Verified { get; set; }
    public bool SetupError { get; set; }
    public bool Saved { get; set; }
    
    public int LoadedCount=>this.WaferSetups.Count(e=>e.Loaded);

    public TestSetupService(QuickTestDataService qtDataService) {
        this._qtDataService = qtDataService;
        for (int i = 0; i < 3; i++) {
            this.SetupAlerts.Add(new SetupAlert());
            this.SetupAlerts[i].Pocket = GetPocketLabel(i);
            this.WaferSetups.Add(GenerateWaferSetup(i));
        }
    }

    public void SetTestSetup(List<PocketWaferSetup> testSetup) {
        this.WaferSetups = testSetup;
    }

    public async Task Load() {
        this.SetupError = false;
        this.Verified = false;
        this.Saved = false;
        this.WaferSetups.Clear();
        this.SetupAlerts.Clear();
        for (int i = 0; i < 3; i++) {
            this.SetupAlerts.Add(new SetupAlert());
            this.SetupAlerts[i].Pocket = GetPocketLabel(i);
            this.WaferSetups.Add(GenerateWaferSetup(i));
        }
    }
    public bool PocketsLoaded() {
        return this.WaferSetups.All(e=>!e.Loaded);
    }
    public PocketWaferSetup? GetWaferSetup(int index) {
        if(this.WaferSetups.Any() && index < this.WaferSetups.Count) {
            return this.WaferSetups[index];
        }
        return null;
    }
    
    public SetupAlert GetSetupAlert(int index) {
        return this.SetupAlerts[index];
    }
    public Task<IEnumerable<string>> GetQtList(TimeSpan history) {
        return this._qtDataService.GetQuickTestList(DateTime.Now-history);
    }
    public void Reset() {
        this.SetupError = false;
        this.Verified = false;
        this.Saved = false;
        this.WaferSetups.Clear();
        this.SetupAlerts.Clear();
        for (int i = 0; i < 3; i++) {
            this.SetupAlerts.Add(new SetupAlert());
            this.SetupAlerts[i].Pocket = GetPocketLabel(i);
            this.WaferSetups.Add(GenerateWaferSetup(i));
        }
    }
    public async Task VerifyHandler() {
        for (int i = 0; i < 3; i++) {
            var setup = this.WaferSetups[i];
            if (setup.Loaded) {
                if(string.IsNullOrWhiteSpace(setup.WaferId)) {
                    ItemAlert waferIdAlert = new ItemAlert();
                    waferIdAlert.Style = AlertStyle.Danger;
                    waferIdAlert.Message = "Wafer Id is empty. Please enter a wafer id or " +
                                                   "test designation before continuing";
                    waferIdAlert.Okay = false;
                    this.SetupAlerts[i].WaferIdAlert= waferIdAlert;
                    bool p1Set = !string.IsNullOrEmpty(setup.Probe1Pad);
                    bool p2Set = !string.IsNullOrEmpty(setup.Probe2Pad);
                    if(p1Set && p2Set) {
                        var probePadAlert = new ItemAlert() {
                            Style = AlertStyle.Success, 
                            Message = "Pad Selection Okay", 
                            Okay = true
                        };
                        this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                    } else  {
                        if (p1Set == false && p2Set == false) {
                            var probePadAlert = new ItemAlert() {
                                Style = AlertStyle.Danger,
                                Message = "Neither pad is set, please set one or both pads to continue",  
                                Okay = false
                            };
                            this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                        } else {
                            var probePadAlert = new ItemAlert() {
                                Style = AlertStyle.Warning,
                                Message = "One pad is not set, If this was intentional press save to continue", 
                                Okay = true
                            };
                            this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                        }
                    }
                    this.SetupAlerts[i].Okay = false;
                    continue;
                }
                var result=await this._qtDataService.QuickTestExists(setup.WaferId);
                if (!result.IsError) {
                    this.SetupAlerts[i].Okay = result.Value;
                    bool p1Set = !string.IsNullOrEmpty(setup.Probe1Pad);
                    bool p2Set = !string.IsNullOrEmpty(setup.Probe2Pad);
                    if (!result.Value) {
                        ItemAlert waferIdAlert = new ItemAlert();
                        waferIdAlert.Style = AlertStyle.Secondary;
                        waferIdAlert.Message = "Wafer not found in database!! " +
                                               "If you would like to continue anyways press save";
                        waferIdAlert.Okay = true;
                        this.SetupAlerts[i].WaferIdAlert= waferIdAlert;
                    } else {
                        ItemAlert waferIdAlert = new ItemAlert();
                        waferIdAlert.Style = AlertStyle.Success;
                        waferIdAlert.Message = "Wafer Found";
                        waferIdAlert.Okay = true;
                        this.SetupAlerts[i].WaferIdAlert= waferIdAlert;
                    }
                    if(p1Set && p2Set) {
                        var probePadAlert = new ItemAlert() {
                            Style = AlertStyle.Success, 
                            Message = "Pad Selection Okay", 
                            Okay = true
                        };
                        this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                    } else  {
                        if (p1Set == false && p2Set == false) {
                            var probePadAlert = new ItemAlert() {
                                Style = AlertStyle.Danger,
                                Message = "Neither pad is set, please set one or both pads to continue",  
                                Okay = false
                            };
                            this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                        } else {
                            var probePadAlert = new ItemAlert() {
                                Style = AlertStyle.Warning,
                                Message = "One pad is not set, If this was intentional press save to continue", 
                                Okay = true
                            };
                            this.SetupAlerts[i].ProbePadAlert = probePadAlert;
                        }
                    }
                    this.SetupAlerts[i].Okay=(this.SetupAlerts[i].WaferIdAlert.Okay==true && this.SetupAlerts[i].ProbePadAlert.Okay==true);
                } else {
                    ItemAlert waferIdAlert = new ItemAlert();
                    waferIdAlert.Style = AlertStyle.Danger;
                    waferIdAlert.Message = $"Error: {result.FirstError.Description}. " +
                                           $"\n check network connection";
                    waferIdAlert.Okay = false;
                    this.SetupAlerts[i].WaferIdAlert= waferIdAlert;
                    this.SetupAlerts[i].Okay = false;
                }
            } else {
                ItemAlert waferIdAlert = new ItemAlert();
                waferIdAlert.Style = AlertStyle.Info;
                waferIdAlert.Message = "Pocket not loaded";
                waferIdAlert.Okay = true;
                this.SetupAlerts[i].WaferIdAlert= waferIdAlert;
                this.SetupAlerts[i].Okay = true;
            }
        }
        this.SetupError=this.SetupAlerts.Any(e => !e.Okay) || this.LoadedCount==0;
    }
    private PocketWaferSetup GenerateWaferSetup(int index) {
        switch (index) {
            case 0: {
                return new PocketWaferSetup() {
                    WaferId = string.Empty,
                    BurnNumber = 1,
                    WaferSize = 2,
                    StationPocket = StationPocket.LeftPocket,
                    Probe1 = StationProbe.Probe1,
                    Probe2 = StationProbe.Probe2,
                    Loaded = true
                };
            }
            case 1: {
                return new PocketWaferSetup() {
                    WaferId = string.Empty,
                    BurnNumber = 1,
                    WaferSize = 2,
                    StationPocket = StationPocket.MiddlePocket,
                    Probe1 = StationProbe.Probe3,
                    Probe2 = StationProbe.Probe4,
                    Loaded = true
                };
            }
            case 2: {
                return new PocketWaferSetup() {
                    WaferId = string.Empty,
                    BurnNumber = 1,
                    WaferSize = 2,
                    StationPocket = StationPocket.RightPocket,
                    Probe1 = StationProbe.Probe5,
                    Probe2 = StationProbe.Probe6,
                    Loaded = true
                };
            }
            default: {
                return new PocketWaferSetup();
            }
        }
    }
    private string GetPocketLabel(int index) {
        return index switch {
            0 => "Left Pocket(P1)",
            1 => "Middle Pocket(P2)",
            2 => "Right Pocket(P3)",
            _ => "Unknown"
        };
    }
}