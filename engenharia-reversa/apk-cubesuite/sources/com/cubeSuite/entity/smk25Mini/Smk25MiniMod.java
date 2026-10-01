package com.cubeSuite.entity.smk25Mini;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniMod {
    private AddrU8 ch;
    private AddrU8[] dat;
    private AddrU8 speed;

    public Smk25MiniMod() {
        this.dat = new AddrU8[3];
    }

    public Smk25MiniMod(AddrU8 addrU8, AddrU8[] addrU8Arr) {
        AddrU8[] addrU8Arr2 = new AddrU8[3];
        this.ch = addrU8;
        this.dat = addrU8Arr;
    }

    public AddrU8 getSpeed() {
        return this.speed;
    }

    public void setSpeed(AddrU8 addrU8) {
        this.speed = addrU8;
    }

    public AddrU8 getCh() {
        return this.ch;
    }

    public void setCh(AddrU8 addrU8) {
        this.ch = addrU8;
    }

    public AddrU8[] getDat() {
        return this.dat;
    }

    public void setDat(AddrU8[] addrU8Arr) {
        this.dat = addrU8Arr;
    }
}
