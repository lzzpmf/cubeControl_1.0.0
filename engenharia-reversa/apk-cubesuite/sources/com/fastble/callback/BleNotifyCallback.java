package com.fastble.callback;

import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleNotifyCallback extends BleBaseCallback {
    public abstract void onCharacteristicChanged(byte[] bArr);

    public abstract void onNotifyFailure(BleException bleException);

    public abstract void onNotifySuccess();
}
