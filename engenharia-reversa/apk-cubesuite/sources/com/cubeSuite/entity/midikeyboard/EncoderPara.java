package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class EncoderPara {
    private AddrU8 ch;
    private AddrU8 curve;
    private AddrU8[] dat;
    private AddrU8 type;

    public EncoderPara() {
        this.dat = new AddrU8[3];
    }

    public EncoderPara(AddrU8 addrU8, AddrU8 addrU9, AddrU8 addrU10, AddrU8[] addrU8Arr) {
        AddrU8[] addrU8Arr2 = new AddrU8[3];
        this.type = addrU8;
        this.curve = addrU9;
        this.ch = addrU10;
        this.dat = addrU8Arr;
    }

    public AddrU8 getType() {
        return this.type;
    }

    public void setType(AddrU8 addrU8) {
        this.type = addrU8;
    }

    public AddrU8 getCurve() {
        return this.curve;
    }

    public void setCurve(AddrU8 addrU8) {
        this.curve = addrU8;
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
