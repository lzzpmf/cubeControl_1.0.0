package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class PitchPara {
    private AddrU8 ch;

    public PitchPara() {
    }

    public PitchPara(AddrU8 addrU8) {
        this.ch = addrU8;
    }

    public AddrU8 getCh() {
        return this.ch;
    }

    public void setCh(AddrU8 addrU8) {
        this.ch = addrU8;
    }
}
