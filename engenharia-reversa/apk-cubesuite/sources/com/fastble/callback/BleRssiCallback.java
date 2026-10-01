package com.fastble.callback;

import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleRssiCallback extends BleBaseCallback {
    public abstract void onRssiFailure(BleException bleException);

    public abstract void onRssiSuccess(int i);
}
