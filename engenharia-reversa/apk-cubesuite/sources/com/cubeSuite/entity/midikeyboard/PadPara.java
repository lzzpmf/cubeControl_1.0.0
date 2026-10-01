package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class PadPara {
    private AddrU8 ch;
    private AddrU8[] color;
    private AddrU8[] dat;
    private AddrU8 type;

    public PadPara() {
        this.dat = new AddrU8[3];
        this.color = new AddrU8[3];
    }

    public PadPara(AddrU8 addrU8, AddrU8 addrU9, AddrU8[] addrU8Arr, AddrU8[] addrU8Arr2) {
        this.dat = new AddrU8[3];
        AddrU8[] addrU8Arr3 = new AddrU8[3];
        this.type = addrU8;
        this.ch = addrU9;
        this.dat = addrU8Arr;
        this.color = addrU8Arr2;
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
