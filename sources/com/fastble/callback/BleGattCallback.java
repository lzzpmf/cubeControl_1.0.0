package com.fastble.callback;

import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import com.fastble.data.BleDevice;
import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public abstract class BleGattCallback extends BluetoothGattCallback {
    public abstract void onConnectFail(BleDevice bleDevice, BleException bleException);

    public abstract void onConnectSuccess(BleDevice bleDevice, BluetoothGatt bluetoothGatt, int i);

    public abstract void onDisConnected(boolean z, BleDevice bleDevice, BluetoothGatt bluetoothGatt, int i);

    public abstract void onStartConnect();
}
