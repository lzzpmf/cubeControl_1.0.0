package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Sys {
    private AddrU8 len;
    private AddrU8[] val;

    public Sys() {
        this.val = new AddrU8[64];
    }

    public Sys(AddrU8 addrU8, AddrU8[] addrU8Arr) {
        AddrU8[] addrU8Arr2 = new AddrU8[64];
        this.len = addrU8;
        this.val = addrU8Arr;
    }

    public AddrU8 getLen() {
        return this.len;
    }

    public void setLen(AddrU8 addrU8) {
        this.len = addrU8;
    }

    public AddrU8[] getVal() {
        return this.val;
    }

    public void setVal(AddrU8[] addrU8Arr) {
        this.val = addrU8Arr;
    }
}
