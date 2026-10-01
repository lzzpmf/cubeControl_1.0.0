package com.cubeSuite.entity.addrData;

import androidx.databinding.BaseObservable;

/* JADX INFO: loaded from: classes.dex */
public class BaseDataAddr extends BaseObservable {
    protected long addr;

    public BaseDataAddr() {
    }

    public BaseDataAddr(long j) {
        this.addr = j;
    }

    public long getAddr() {
        return this.addr;
    }
}
