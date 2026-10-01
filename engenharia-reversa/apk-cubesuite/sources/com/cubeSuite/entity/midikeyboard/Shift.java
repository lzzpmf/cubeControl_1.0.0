package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Shift {
    private AddrU8 chord;
    private AddrU8 major;
    private AddrU8 scale;

    public Shift() {
    }

    public Shift(AddrU8 addrU8, AddrU8 addrU9, AddrU8 addrU10) {
        this.major = addrU8;
        this.scale = addrU9;
        this.chord = addrU10;
    }

    public AddrU8 getMajor() {
        return this.major;
    }

    public void setMajor(AddrU8 addrU8) {
        this.major = addrU8;
    }

    public AddrU8 getScale() {
        return this.scale;
    }

    public void setScale(AddrU8 addrU8) {
        this.scale = addrU8;
    }

    public AddrU8 getChord() {
        return this.chord;
    }

    public void setChord(AddrU8 addrU8) {
        this.chord = addrU8;
    }
}
