package com.cubeSuite.entity.smk25Mini;

import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Arp;
import com.cubeSuite.entity.midikeyboard.Shift;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniGlob {
    private Arp arp;
    private AddrU8 inputMode;
    private AddrU8 keyCurve;
    private Shift shift;

    public Smk25MiniGlob() {
        this.shift = new Shift();
        this.arp = new Arp();
    }

    public Smk25MiniGlob(AddrU8 addrU8, Shift shift, Arp arp, AddrU8 addrU9) {
        this.shift = new Shift();
        new Arp();
        this.keyCurve = addrU8;
        this.shift = shift;
        this.arp = arp;
        this.inputMode = addrU9;
    }

    public AddrU8 getKeyCurve() {
        return this.keyCurve;
    }

    public void setKeyCurve(AddrU8 addrU8) {
        this.keyCurve = addrU8;
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
