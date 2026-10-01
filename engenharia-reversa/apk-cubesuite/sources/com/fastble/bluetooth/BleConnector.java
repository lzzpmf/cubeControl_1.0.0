package com.fastble.bluetooth;

import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import com.fastble.BleManager;
import com.fastble.callback.BleIndicateCallback;
import com.fastble.callback.BleMtuChangedCallback;
import com.fastble.callback.BleNotifyCallback;
import com.fastble.callback.BleReadCallback;
import com.fastble.callback.BleRssiCallback;
import com.fastble.callback.BleWriteCallback;
import com.fastble.data.BleMsg;
import com.fastble.exception.GattException;
import com.fastble.exception.OtherException;
import com.fastble.exception.TimeoutException;
import java.util.UUID;

/* JADX INFO: loaded from: classes.dex */
public class BleConnector {
    private static final String UUID_CLIENT_CHARACTERISTIC_CONFIG_DESCRIPTOR = "00002902-0000-1000-8000-00805f9b34fb";
    private BleBluetooth mBleBluetooth;
    private BluetoothGatt mBluetoothGatt;
    private BluetoothGattCharacteristic mCharacteristic;
    private BluetoothGattService mGattService;
    private Handler mHandler = new Handler(Looper.getMainLooper()) { // from class: com.fastble.bluetooth.BleConnector.1
        @Override // android.os.Handler
        public void handleMessage(Message message) {
            super.handleMessage(message);
            int i = message.what;
            if (i == 49) {
                BleWriteCallback bleWriteCallback = (BleWriteCallback) message.obj;
                if (bleWriteCallback != null) {
                    bleWriteCallback.onWriteFailure(new TimeoutException());
                    return;
                }
                return;
            }
            if (i == 50) {
                BleConnector.this.writeMsgInit();
                BleWriteCallback bleWriteCallback2 = (BleWriteCallback) message.obj;
                Bundle data = message.getData();
                int i2 = data.getInt(BleMsg.KEY_WRITE_BUNDLE_STATUS);
                byte[] byteArray = data.getByteArray(BleMsg.KEY_WRITE_BUNDLE_VALUE);
                if (bleWriteCallback2 != null) {
                    if (i2 == 0) {
                        bleWriteCallback2.onWriteSuccess(1, 1, byteArray);
                        return;
                    } else {
                        bleWriteCallback2.onWriteFailure(new GattException(i2));
                        return;
                    }
                }
                return;
            }
            if (i == 65) {
                BleReadCallback bleReadCallback = (BleReadCallback) message.obj;
                if (bleReadCallback != null) {
                    bleReadCallback.onReadFailure(new TimeoutException());
                    return;
                }
                return;
            }
            if (i == 66) {
                BleConnector.this.readMsgInit();
                BleReadCallback bleReadCallback2 = (BleReadCallback) message.obj;
                Bundle data2 = message.getData();
                int i3 = data2.getInt(BleMsg.KEY_READ_BUNDLE_STATUS);
                byte[] byteArray2 = data2.getByteArray(BleMsg.KEY_READ_BUNDLE_VALUE);
                if (bleReadCallback2 != null) {
                    if (i3 == 0) {
                        bleReadCallback2.onReadSuccess(byteArray2);
                        return;
                    } else {
                        bleReadCallback2.onReadFailure(new GattException(i3));
                        return;
                    }
                }
                return;
            }
            if (i == 81) {
                BleRssiCallback bleRssiCallback = (BleRssiCallback) message.obj;
                if (bleRssiCallback != null) {
                    bleRssiCallback.onRssiFailure(new TimeoutException());
                    return;
                }
                return;
            }
            if (i == 82) {
                BleConnector.this.rssiMsgInit();
                BleRssiCallback bleRssiCallback2 = (BleRssiCallback) message.obj;
                Bundle data3 = message.getData();
                int i4 = data3.getInt(BleMsg.KEY_READ_RSSI_BUNDLE_STATUS);
                int i5 = data3.getInt(BleMsg.KEY_READ_RSSI_BUNDLE_VALUE);
                if (bleRssiCallback2 != null) {
                    if (i4 == 0) {
                        bleRssiCallback2.onRssiSuccess(i5);
                        return;
                    } else {
                        bleRssiCallback2.onRssiFailure(new GattException(i4));
                        return;
                    }
                }
                return;
            }
            if (i == 97) {
                BleMtuChangedCallback bleMtuChangedCallback = (BleMtuChangedCallback) message.obj;
                if (bleMtuChangedCallback != null) {
                    bleMtuChangedCallback.onSetMTUFailure(new TimeoutException());
                    return;
                }
                return;
            }
            if (i != 98) {
                switch (i) {
                    case 17:
                        BleNotifyCallback bleNotifyCallback = (BleNotifyCallback) message.obj;
                        if (bleNotifyCallback != null) {
                            bleNotifyCallback.onNotifyFailure(new TimeoutException());
                        }
                        break;
                    case 18:
                        BleConnector.this.notifyMsgInit();
                        BleNotifyCallback bleNotifyCallback2 = (BleNotifyCallback) message.obj;
                        int i6 = message.getData().getInt(BleMsg.KEY_NOTIFY_BUNDLE_STATUS);
                        if (bleNotifyCallback2 != null) {
                            if (i6 == 0) {
                                bleNotifyCallback2.onNotifySuccess();
                            } else {
                                bleNotifyCallback2.onNotifyFailure(new GattException(i6));
                            }
                        }
                        break;
                    case 19:
                        BleNotifyCallback bleNotifyCallback3 = (BleNotifyCallback) message.obj;
                        byte[] byteArray3 = message.getData().getByteArray(BleMsg.KEY_NOTIFY_BUNDLE_VALUE);
                        if (bleNotifyCallback3 != null) {
                            bleNotifyCallback3.onCharacteristicChanged(byteArray3);
                        }
                        break;
                    default:
                        switch (i) {
                            case 33:
                                BleIndicateCallback bleIndicateCallback = (BleIndicateCallback) message.obj;
                                if (bleIndicateCallback != null) {
                                    bleIndicateCallback.onIndicateFailure(new TimeoutException());
                                }
                                break;
                            case 34:
                                BleConnector.this.indicateMsgInit();
                                BleIndicateCallback bleIndicateCallback2 = (BleIndicateCallback) message.obj;
                                int i7 = message.getData().getInt(BleMsg.KEY_INDICATE_BUNDLE_STATUS);
                                if (bleIndicateCallback2 != null) {
                                    if (i7 == 0) {
                                        bleIndicateCallback2.onIndicateSuccess();
                                    } else {
                                        bleIndicateCallback2.onIndicateFailure(new GattException(i7));
                                    }
                                }
                                break;
                            case 35:
                                BleIndicateCallback bleIndicateCallback3 = (BleIndicateCallback) message.obj;
                                byte[] byteArray4 = message.getData().getByteArray(BleMsg.KEY_INDICATE_BUNDLE_VALUE);
                                if (bleIndicateCallback3 != null) {
                                    bleIndicateCallback3.onCharacteristicChanged(byteArray4);
                                }
                                break;
                        }
                        break;
                }
                return;
            }
            BleConnector.this.mtuChangedMsgInit();
            BleMtuChangedCallback bleMtuChangedCallback2 = (BleMtuChangedCallback) message.obj;
            Bundle data4 = message.getData();
            int i8 = data4.getInt(BleMsg.KEY_SET_MTU_BUNDLE_STATUS);
            int i9 = data4.getInt(BleMsg.KEY_SET_MTU_BUNDLE_VALUE);
            if (bleMtuChangedCallback2 != null) {
                if (i8 == 0) {
                    bleMtuChangedCallback2.onMtuChanged(i9);
                } else {
                    bleMtuChangedCallback2.onSetMTUFailure(new GattException(i8));
                }
            }
        }
    };

    BleConnector(BleBluetooth bleBluetooth) {
        this.mBleBluetooth = bleBluetooth;
        this.mBluetoothGatt = bleBluetooth.getBluetoothGatt();
    }

    private BleConnector withUUID(UUID uuid, UUID uuid2) {
        BluetoothGatt bluetoothGatt;
        if (uuid != null && (bluetoothGatt = this.mBluetoothGatt) != null) {
            this.mGattService = bluetoothGatt.getService(uuid);
        }
        BluetoothGattService bluetoothGattService = this.mGattService;
        if (bluetoothGattService != null && uuid2 != null) {
            this.mCharacteristic = bluetoothGattService.getCharacteristic(uuid2);
        }
        return this;
    }

    public BleConnector withUUIDString(String str, String str2) {
        return withUUID(formUUID(str), formUUID(str2));
    }

    private UUID formUUID(String str) {
        if (str == null) {
            return null;
        }
        return UUID.fromString(str);
    }

    public void enableCharacteristicNotify(BleNotifyCallback bleNotifyCallback, String str, boolean z) {
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic != null && (bluetoothGattCharacteristic.getProperties() | 16) > 0) {
            handleCharacteristicNotifyCallback(bleNotifyCallback, str);
            setCharacteristicNotification(this.mBluetoothGatt, this.mCharacteristic, z, true, bleNotifyCallback);
        } else if (bleNotifyCallback != null) {
            bleNotifyCallback.onNotifyFailure(new OtherException("this characteristic not support notify!"));
        }
    }

    public boolean disableCharacteristicNotify(boolean z) {
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic == null || (bluetoothGattCharacteristic.getProperties() | 16) <= 0) {
            return false;
        }
        return setCharacteristicNotification(this.mBluetoothGatt, this.mCharacteristic, z, false, null);
    }

    private boolean setCharacteristicNotification(BluetoothGatt bluetoothGatt, BluetoothGattCharacteristic bluetoothGattCharacteristic, boolean z, boolean z2, BleNotifyCallback bleNotifyCallback) {
        BluetoothGattDescriptor descriptor;
        byte[] bArr;
        if (bluetoothGatt == null || bluetoothGattCharacteristic == null) {
            notifyMsgInit();
            if (bleNotifyCallback != null) {
                bleNotifyCallback.onNotifyFailure(new OtherException("gatt or characteristic equal null"));
            }
            return false;
        }
        if (!bluetoothGatt.setCharacteristicNotification(bluetoothGattCharacteristic, z2)) {
            notifyMsgInit();
            if (bleNotifyCallback != null) {
                bleNotifyCallback.onNotifyFailure(new OtherException("gatt setCharacteristicNotification fail"));
            }
            return false;
        }
        if (z) {
            descriptor = bluetoothGattCharacteristic.getDescriptor(bluetoothGattCharacteristic.getUuid());
        } else {
            descriptor = bluetoothGattCharacteristic.getDescriptor(formUUID(UUID_CLIENT_CHARACTERISTIC_CONFIG_DESCRIPTOR));
        }
        if (descriptor == null) {
            notifyMsgInit();
            if (bleNotifyCallback != null) {
                bleNotifyCallback.onNotifyFailure(new OtherException("descriptor equals null"));
            }
            return false;
        }
        if (z2) {
            bArr = BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE;
        } else {
            bArr = BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE;
        }
        descriptor.setValue(bArr);
        boolean zWriteDescriptor = bluetoothGatt.writeDescriptor(descriptor);
        if (!zWriteDescriptor) {
            notifyMsgInit();
            if (bleNotifyCallback != null) {
                bleNotifyCallback.onNotifyFailure(new OtherException("gatt writeDescriptor fail"));
            }
        }
        return zWriteDescriptor;
    }

    public void enableCharacteristicIndicate(BleIndicateCallback bleIndicateCallback, String str, boolean z) {
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic != null && (bluetoothGattCharacteristic.getProperties() | 16) > 0) {
            handleCharacteristicIndicateCallback(bleIndicateCallback, str);
            setCharacteristicIndication(this.mBluetoothGatt, this.mCharacteristic, z, true, bleIndicateCallback);
        } else if (bleIndicateCallback != null) {
            bleIndicateCallback.onIndicateFailure(new OtherException("this characteristic not support indicate!"));
        }
    }

    public boolean disableCharacteristicIndicate(boolean z) {
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic == null || (bluetoothGattCharacteristic.getProperties() | 16) <= 0) {
            return false;
        }
        return setCharacteristicIndication(this.mBluetoothGatt, this.mCharacteristic, z, false, null);
    }

    private boolean setCharacteristicIndication(BluetoothGatt bluetoothGatt, BluetoothGattCharacteristic bluetoothGattCharacteristic, boolean z, boolean z2, BleIndicateCallback bleIndicateCallback) {
        BluetoothGattDescriptor descriptor;
        byte[] bArr;
        if (bluetoothGatt == null || bluetoothGattCharacteristic == null) {
            indicateMsgInit();
            if (bleIndicateCallback != null) {
                bleIndicateCallback.onIndicateFailure(new OtherException("gatt or characteristic equal null"));
            }
            return false;
        }
        if (!bluetoothGatt.setCharacteristicNotification(bluetoothGattCharacteristic, z2)) {
            indicateMsgInit();
            if (bleIndicateCallback != null) {
                bleIndicateCallback.onIndicateFailure(new OtherException("gatt setCharacteristicNotification fail"));
            }
            return false;
        }
        if (z) {
            descriptor = bluetoothGattCharacteristic.getDescriptor(bluetoothGattCharacteristic.getUuid());
        } else {
            descriptor = bluetoothGattCharacteristic.getDescriptor(formUUID(UUID_CLIENT_CHARACTERISTIC_CONFIG_DESCRIPTOR));
        }
        if (descriptor == null) {
            indicateMsgInit();
            if (bleIndicateCallback != null) {
                bleIndicateCallback.onIndicateFailure(new OtherException("descriptor equals null"));
            }
            return false;
        }
        if (z2) {
            bArr = BluetoothGattDescriptor.ENABLE_INDICATION_VALUE;
        } else {
            bArr = BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE;
        }
        descriptor.setValue(bArr);
        boolean zWriteDescriptor = bluetoothGatt.writeDescriptor(descriptor);
        if (!zWriteDescriptor) {
            indicateMsgInit();
            if (bleIndicateCallback != null) {
                bleIndicateCallback.onIndicateFailure(new OtherException("gatt writeDescriptor fail"));
            }
        }
        return zWriteDescriptor;
    }

    public void writeCharacteristic(byte[] bArr, BleWriteCallback bleWriteCallback, String str) {
        if (bArr == null || bArr.length <= 0) {
            if (bleWriteCallback != null) {
                bleWriteCallback.onWriteFailure(new OtherException("the data to be written is empty"));
                return;
            }
            return;
        }
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic == null || (bluetoothGattCharacteristic.getProperties() & 12) == 0) {
            if (bleWriteCallback != null) {
                bleWriteCallback.onWriteFailure(new OtherException("this characteristic not support write!"));
            }
        } else {
            if (!this.mCharacteristic.setValue(bArr)) {
                if (bleWriteCallback != null) {
                    bleWriteCallback.onWriteFailure(new OtherException("Updates the locally stored value of this characteristic fail"));
                    return;
                }
                return;
            }
            handleCharacteristicWriteCallback(bleWriteCallback, str);
            if (this.mBluetoothGatt.writeCharacteristic(this.mCharacteristic)) {
                return;
            }
            writeMsgInit();
            if (bleWriteCallback != null) {
                bleWriteCallback.onWriteFailure(new OtherException("gatt writeCharacteristic fail"));
            }
        }
    }

    public void readCharacteristic(BleReadCallback bleReadCallback, String str) {
        BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCharacteristic;
        if (bluetoothGattCharacteristic == null || (bluetoothGattCharacteristic.getProperties() & 2) <= 0) {
            if (bleReadCallback != null) {
                bleReadCallback.onReadFailure(new OtherException("this characteristic not support read!"));
                return;
            }
            return;
        }
        handleCharacteristicReadCallback(bleReadCallback, str);
        if (this.mBluetoothGatt.readCharacteristic(this.mCharacteristic)) {
            return;
        }
        readMsgInit();
        if (bleReadCallback != null) {
            bleReadCallback.onReadFailure(new OtherException("gatt readCharacteristic fail"));
        }
    }

    public void readRemoteRssi(BleRssiCallback bleRssiCallback) {
        handleRSSIReadCallback(bleRssiCallback);
        if (this.mBluetoothGatt.readRemoteRssi()) {
            return;
        }
        rssiMsgInit();
        if (bleRssiCallback != null) {
            bleRssiCallback.onRssiFailure(new OtherException("gatt readRemoteRssi fail"));
        }
    }

    public void setMtu(int i, BleMtuChangedCallback bleMtuChangedCallback) {
        handleSetMtuCallback(bleMtuChangedCallback);
        if (this.mBluetoothGatt.requestMtu(i)) {
            return;
        }
        mtuChangedMsgInit();
        if (bleMtuChangedCallback != null) {
            bleMtuChangedCallback.onSetMTUFailure(new OtherException("gatt requestMtu fail"));
        }
    }

    public boolean requestConnectionPriority(int i) {
        return this.mBluetoothGatt.requestConnectionPriority(i);
    }

    private void handleCharacteristicNotifyCallback(BleNotifyCallback bleNotifyCallback, String str) {
        if (bleNotifyCallback != null) {
            notifyMsgInit();
            bleNotifyCallback.setKey(str);
            bleNotifyCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addNotifyCallback(str, bleNotifyCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(17, bleNotifyCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    private void handleCharacteristicIndicateCallback(BleIndicateCallback bleIndicateCallback, String str) {
        if (bleIndicateCallback != null) {
            indicateMsgInit();
            bleIndicateCallback.setKey(str);
            bleIndicateCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addIndicateCallback(str, bleIndicateCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(33, bleIndicateCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    private void handleCharacteristicWriteCallback(BleWriteCallback bleWriteCallback, String str) {
        if (bleWriteCallback != null) {
            writeMsgInit();
            bleWriteCallback.setKey(str);
            bleWriteCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addWriteCallback(str, bleWriteCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(49, bleWriteCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    private void handleCharacteristicReadCallback(BleReadCallback bleReadCallback, String str) {
        if (bleReadCallback != null) {
            readMsgInit();
            bleReadCallback.setKey(str);
            bleReadCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addReadCallback(str, bleReadCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(65, bleReadCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    private void handleRSSIReadCallback(BleRssiCallback bleRssiCallback) {
        if (bleRssiCallback != null) {
            rssiMsgInit();
            bleRssiCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addRssiCallback(bleRssiCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(81, bleRssiCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    private void handleSetMtuCallback(BleMtuChangedCallback bleMtuChangedCallback) {
        if (bleMtuChangedCallback != null) {
            mtuChangedMsgInit();
            bleMtuChangedCallback.setHandler(this.mHandler);
            this.mBleBluetooth.addMtuChangedCallback(bleMtuChangedCallback);
            Handler handler = this.mHandler;
            handler.sendMessageDelayed(handler.obtainMessage(97, bleMtuChangedCallback), BleManager.getInstance().getOperateTimeout());
        }
    }

    public void notifyMsgInit() {
        this.mHandler.removeMessages(17);
    }

    public void indicateMsgInit() {
        this.mHandler.removeMessages(33);
    }

    public void writeMsgInit() {
        this.mHandler.removeMessages(49);
    }

    public void readMsgInit() {
        this.mHandler.removeMessages(65);
    }

    public void rssiMsgInit() {
        this.mHandler.removeMessages(81);
    }

    public void mtuChangedMsgInit() {
        this.mHandler.removeMessages(97);
    }
}
