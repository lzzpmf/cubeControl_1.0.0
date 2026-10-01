package com.fastble.callback;

import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleMtuChangedCallback extends BleBaseCallback {
    public abstract void onMtuChanged(int i);

    public abstract void onSetMTUFailure(BleException bleException);
}
