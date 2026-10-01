package com.cubeSuite.entity.addrData;

import androidx.databinding.Bindable;

/* JADX INFO: loaded from: classes.dex */
public class AddrU8 extends BaseDataAddr {
    private int data;
    private int max;
    private int min;

    public AddrU8(int i, long j) {
        this.min = 0;
        this.max = 255;
        setData(i);
        this.addr = j;
    }

    public AddrU8(int i, long j, int i2, int i3) {
        this.min = i2;
        this.max = i3;
        setData(i);
        this.addr = j;
    }

    @Bindable
    public int getData() {
        return this.data;
    }

    @Bindable
    public byte getByteData() {
        return (byte) this.data;
    }

    public boolean setData(int i) {
        int iMin = Math.min(Math.max(i & 255, this.min), this.max);
        if (this.data == iMin) {
            return false;
        }
        this.data = iMin;
        notifyChange();
        return true;
    }
}
