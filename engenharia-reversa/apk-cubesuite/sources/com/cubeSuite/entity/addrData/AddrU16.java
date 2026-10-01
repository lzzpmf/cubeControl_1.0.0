package com.cubeSuite.entity.addrData;

/* JADX INFO: loaded from: classes.dex */
public class AddrU16 extends BaseDataAddr {
    private int data;
    private int max;
    private int min;

    public AddrU16(int i, long j) {
        this.min = 0;
        this.max = 65535;
        setData(i);
        this.addr = j;
    }

    public int getData() {
        return this.data;
    }

    public AddrU16(int i, long j, int i2, int i3) {
        this.min = i2;
        this.max = i3;
        setData(i);
        this.addr = j;
    }

    public boolean setData(int i) {
        int iMin = Math.min(Math.max(i & 65535, this.min), this.max);
        if (this.data == iMin) {
            return false;
        }
        this.data = iMin;
        notifyChange();
        return true;
    }
}
