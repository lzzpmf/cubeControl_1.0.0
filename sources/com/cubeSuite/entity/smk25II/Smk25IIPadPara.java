package com.cubeSuite.entity.smk25II;

import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Sys;

/* JADX INFO: loaded from: classes.dex */
public class Smk25IIPadPara {
    private AddrU8 ch;
    private AddrU8 led;
    private AddrU8 type;
    private AddrU8[] dat = new AddrU8[3];
    private AddrU8[] color = new AddrU8[3];
    private Sys sys = new Sys();

    public AddrU8 getLed() {
        return this.led;
    }

    public void setLed(AddrU8 addrU8) {
        this.led = addrU8;
    }

    public Sys getSys() {
        return this.sys;
    }

    public void setSys(Sys sys) {
        this.sys = sys;
    }

    public AddrU8 getType() {
        return this.type;
    }

    public void setType(AddrU8 addrU8) {
        this.type = addrU8;
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

    public AddrU8[] getColor() {
        return this.color;
    }

    public void setColor(AddrU8[] addrU8Arr) {
        this.color = addrU8Arr;
    }
}
