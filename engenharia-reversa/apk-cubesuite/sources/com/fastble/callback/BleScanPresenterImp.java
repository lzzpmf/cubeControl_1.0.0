package com.fastble.callback;

import com.fastble.data.BleDevice;

/* JADX INFO: loaded from: classes.dex */
public interface BleScanPresenterImp {
    void onScanStarted(boolean z);

    void onScanning(BleDevice bleDevice);
}
