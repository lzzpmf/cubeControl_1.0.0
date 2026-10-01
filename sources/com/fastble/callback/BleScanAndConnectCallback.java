package com.fastble.callback;

import com.fastble.data.BleDevice;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleScanAndConnectCallback extends BleGattCallback implements BleScanPresenterImp {
    public void onLeScan(BleDevice bleDevice) {
    }

    public abstract void onScanFinished(BleDevice bleDevice);
}
