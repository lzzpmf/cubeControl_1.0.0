package com.fastble.callback;

import com.fastble.data.BleDevice;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleScanCallback implements BleScanPresenterImp {
    public void onLeScan(BleDevice bleDevice) {
    }

    public abstract void onScanFinished(List<BleDevice> list);
}
