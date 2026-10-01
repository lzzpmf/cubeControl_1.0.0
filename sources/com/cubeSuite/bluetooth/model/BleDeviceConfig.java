package com.cubeSuite.bluetooth.model;

import com.fastble.data.BleDevice;

/* JADX INFO: loaded from: classes.dex */
public class BleDeviceConfig {
    private BleDevice bleDevice;
    public boolean directConnected;
    public final int imageId;
    public final String name;

    public BleDeviceConfig(String str, boolean z, int i) {
        this.name = str;
        this.directConnected = z;
        this.imageId = i;
    }

    public BleDevice getBleDevice() {
        return this.bleDevice;
    }

    public void setBleDevice(BleDevice bleDevice) {
        this.bleDevice = bleDevice;
    }

    public BleDeviceConfig copy(BleDevice bleDevice) {
        BleDeviceConfig bleDeviceConfig = new BleDeviceConfig(this.name, this.directConnected, this.imageId);
        bleDeviceConfig.setBleDevice(bleDevice);
        return bleDeviceConfig;
    }
}
