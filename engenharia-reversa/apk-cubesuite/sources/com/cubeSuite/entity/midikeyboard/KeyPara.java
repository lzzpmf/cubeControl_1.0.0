package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class KeyPara {
    private AddrU8 ch;
    private AddrU8[] mackie;
    private AddrU8 max;
    private AddrU8 min;
    private AddrI8 octave;
    private AddrI8 transpose;

    public KeyPara() {
        this.mackie = new AddrU8[25];
    }

    public KeyPara(AddrU8 addrU8, AddrI8 addrI8, AddrI8 addrI9, AddrU8 addrU9, AddrU8 addrU10, AddrU8[] addrU8Arr) {
        AddrU8[] addrU8Arr2 = new AddrU8[25];
        this.ch = addrU8;
        this.octave = addrI8;
        this.transpose = addrI9;
        this.min = addrU9;
        this.max = addrU10;
        this.mackie = addrU8Arr;
    }

    public AddrU8 getCh() {
        return this.ch;
    }

    public void setCh(AddrU8 addrU8) {
        this.ch = addrU8;
    }

    public AddrI8 getOctave() {
        return this.octave;
    }

    public void setOctave(AddrI8 addrI8) {
        this.octave = addrI8;
    }

    public AddrI8 getTranspose() {
        return this.transpose;
    }

    public void setTranspose(AddrI8 addrI8) {
        this.transpose = addrI8;
    }

    public AddrU8 getMin() {
        return this.min;
    }

    public void setMin(AddrU8 addrU8) {
        this.min = addrU8;
    }

    public AddrU8 getMax() {
        return this.max;
    }

    public void setMax(AddrU8 addrU8) {
        this.max = addrU8;
    }

    public AddrU8[] getMackie() {
        return this.mackie;
    }

    public void setMackie(AddrU8[] addrU8Arr) {
        this.mackie = addrU8Arr;
    }
}
