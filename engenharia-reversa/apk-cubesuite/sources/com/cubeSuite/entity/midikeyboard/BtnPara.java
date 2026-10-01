package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class BtnPara {
    private AddrU8 ch;
    private AddrU8[] dat;
    private Sys sys;
    private AddrU8 type;

    public BtnPara() {
        this.dat = new AddrU8[3];
        this.sys = new Sys();
    }

    public BtnPara(AddrU8 addrU8, AddrU8 addrU9, AddrU8[] addrU8Arr, Sys sys) {
        this.dat = new AddrU8[3];
        new Sys();
        this.type = addrU8;
        this.ch = addrU9;
        this.dat = addrU8Arr;
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

    public Sys getSys() {
        return this.sys;
    }

    public void setSys(Sys sys) {
        this.sys = sys;
    }
}
