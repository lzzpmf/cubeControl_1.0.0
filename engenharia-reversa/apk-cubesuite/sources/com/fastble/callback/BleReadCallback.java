package com.fastble.callback;

import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleReadCallback extends BleBaseCallback {
    public abstract void onReadFailure(BleException bleException);

    public abstract void onReadSuccess(byte[] bArr);
}
