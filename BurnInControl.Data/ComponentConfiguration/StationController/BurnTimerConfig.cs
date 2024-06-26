﻿namespace BurnInControl.Data.ComponentConfiguration.StationController;

public class BurnTimerConfig {
    public ulong Time60mASec { get; set; }
    public ulong Time120mASec { get; set; }
    public ulong Time150mASec { get; set; }
    public double TimeOffPercent { get; set; }
    
    public BurnTimerConfig(ulong t60, ulong t120, ulong t150,double offPercent) {
        this.Time60mASec = t60;
        this.Time120mASec = t120;
        this.Time150mASec = t150;
        this.TimeOffPercent= offPercent;
        
    }

    public BurnTimerConfig() {
        this.Time60mASec = 72000;
        this.Time120mASec = 72000;
        this.Time150mASec = 25200;
        this.TimeOffPercent = 95.0;
    }
}