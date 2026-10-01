package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Glob {
    private Arp arp;
    private AddrU8 inputMode;
    private AddrU8 keyCurve;
    private AddrU8 padAft;
    private AddrU8 padCurve;
    private AddrU8 pitchDet;
    private AddrU8 preset;
    private Shift shift;

    public Glob() {
        this.shift = new Shift();
        this.arp = new Arp();
    }

    public Glob(AddrU8 addrU8, AddrU8 addrU9, AddrU8 addrU10, AddrU8 addrU11, AddrU8 addrU12, Shift shift, Arp arp, AddrU8 addrU13) {
        this.shift = new Shift();
        new Arp();
        this.preset = addrU8;
        this.padCurve = addrU9;
        this.keyCurve = addrU10;
        this.pitchDet = addrU11;
        this.padAft = addrU12;
        this.shift = shift;
        this.arp = arp;
        this.inputMode = addrU13;
    }

    public AddrU8 getPreset() {
        return this.preset;
    }

    public void setPreset(AddrU8 addrU8) {
        this.preset = addrU8;
    }

    public AddrU8 getPadCurve() {
        return this.padCurve;
    }

    public void setPadCurve(AddrU8 addrU8) {
        this.padCurve = addrU8;
    }

    public AddrU8 getKeyCurve() {
        return this.keyCurve;
    }

    public void setKeyCurve(AddrU8 addrU8) {
        this.keyCurve = addrU8;
    }

    public AddrU8 getPitchDet() {
        return this.pitchDet;
    }

    public void setPitchDet(AddrU8 addrU8) {
        this.pitchDet = addrU8;
    }

    public AddrU8 getPadAft() {
        return this.padAft;
    }

    public void setPadAft(AddrU8 addrU8) {
        this.padAft = addrU8;
    }

    public Shift getShift() {
        return this.shift;
    }

    public void setShift(Shift shift) {
        this.shift = shift;
    }

    public Arp getArp() {
        return this.arp;
    }

    public void setArp(Arp arp) {
        this.arp = arp;
    }

    public AddrU8 getInputMode() {
        return this.inputMode;
    }

    public void setInputMode(AddrU8 addrU8) {
        this.inputMode = addrU8;
    }
}
