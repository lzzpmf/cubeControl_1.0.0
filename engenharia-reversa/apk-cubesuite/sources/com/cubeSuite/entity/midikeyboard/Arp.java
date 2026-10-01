package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Arp {
    private AddrU8 gate;
    private AddrU8 latch;
    private AddrU8 mode;
    private AddrU8 oct;
    private AddrU8 swing;
    private AddrU8 sync;
    private AddrU16 tempo;
    private AddrU8 time;

    public Arp() {
    }

    public Arp(AddrU8 addrU8, AddrU8 addrU9, AddrU8 addrU10, AddrU8 addrU11, AddrU8 addrU12, AddrU8 addrU13, AddrU8 addrU14, AddrU16 addrU16) {
        this.mode = addrU8;
        this.oct = addrU9;
        this.gate = addrU10;
        this.swing = addrU11;
        this.latch = addrU12;
        this.time = addrU13;
        this.sync = addrU14;
        this.tempo = addrU16;
    }

    public AddrU8 getMode() {
        return this.mode;
    }

    public void setMode(AddrU8 addrU8) {
        this.mode = addrU8;
    }

    public AddrU8 getOct() {
        return this.oct;
    }

    public void setOct(AddrU8 addrU8) {
        this.oct = addrU8;
    }

    public AddrU8 getGate() {
        return this.gate;
    }

    public void setGate(AddrU8 addrU8) {
        this.gate = addrU8;
    }

    public AddrU8 getSwing() {
        return this.swing;
    }

    public void setSwing(AddrU8 addrU8) {
        this.swing = addrU8;
    }

    public AddrU8 getLatch() {
        return this.latch;
    }

    public void setLatch(AddrU8 addrU8) {
        this.latch = addrU8;
    }

    public AddrU8 getTime() {
        return this.time;
    }

    public void setTime(AddrU8 addrU8) {
        this.time = addrU8;
    }

    public AddrU8 getSync() {
        return this.sync;
    }

    public void setSync(AddrU8 addrU8) {
        this.sync = addrU8;
    }

    public AddrU16 getTempo() {
        return this.tempo;
    }

    public void setTempo(AddrU16 addrU16) {
        this.tempo = addrU16;
    }
}
