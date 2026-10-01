package com.fastble.callback;

import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleIndicateCallback extends BleBaseCallback {
    public abstract void onCharacteristicChanged(byte[] bArr);

    public abstract void onIndicateFailure(BleException bleException);

    public abstract void onIndicateSuccess();
}
