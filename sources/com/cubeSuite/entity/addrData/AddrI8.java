package com.cubeSuite.entity.addrData;

import androidx.databinding.Bindable;

/* JADX INFO: loaded from: classes.dex */
public class AddrI8 extends BaseDataAddr {
    private byte data;
    private int max;
    private int min;

    public AddrI8(byte b, long j) {
        this.min = -128;
        this.max = 127;
        setData(b);
        this.addr = j;
    }

    public AddrI8(byte b, long j, int i, int i2) {
        this.min = i;
        this.max = i2;
        setData(b);
        this.addr = j;
    }

    @Bindable
    public byte getData() {
        return this.data;
    }

    public boolean setData(byte b) {
        byte bMin = (byte) Math.min(Math.max((int) b, this.min), this.max);
        if (this.data == bMin) {
            return false;
        }
        this.data = bMin;
        notifyChange();
        return true;
    }
}
