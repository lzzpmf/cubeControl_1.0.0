package com.cubeSuite.bluetooth.model;

import android.os.Parcel;
import android.os.Parcelable;
import com.cubeSuite.utils.AppUtil;

/* JADX INFO: loaded from: classes.dex */
public class BleScanInfo implements Parcelable {
    public static final Parcelable.Creator<BleScanInfo> CREATOR = new Parcelable.Creator<BleScanInfo>() { // from class: com.cubeSuite.bluetooth.model.BleScanInfo.1
        /* JADX WARN: Can't rename method to resolve collision */
        @Override // android.os.Parcelable.Creator
        public BleScanInfo createFromParcel(Parcel parcel) {
            return new BleScanInfo(parcel);
        }

        /* JADX WARN: Can't rename method to resolve collision */
        @Override // android.os.Parcelable.Creator
        public BleScanInfo[] newArray(int i) {
            return new BleScanInfo[i];
        }
    };
    private boolean isEnableConnect;
    private byte[] rawData;
    private int rssi;

    @Override // android.os.Parcelable
    public int describeContents() {
        return 0;
    }

    public BleScanInfo() {
        this.isEnableConnect = true;
    }

    protected BleScanInfo(Parcel parcel) {
        this.isEnableConnect = true;
        this.rawData = parcel.createByteArray();
        this.rssi = parcel.readInt();
        this.isEnableConnect = parcel.readByte() != 0;
    }

    public byte[] getRawData() {
        return this.rawData;
    }

    public BleScanInfo setRawData(byte[] bArr) {
        this.rawData = bArr;
        return this;
    }

    public int getRssi() {
        return this.rssi;
    }

    public BleScanInfo setRssi(int i) {
        this.rssi = i;
        return this;
    }

    public boolean isEnableConnect() {
        return this.isEnableConnect;
    }

    public BleScanInfo setEnableConnect(boolean z) {
        this.isEnableConnect = z;
        return this;
    }

    @Override // android.os.Parcelable
    public void writeToParcel(Parcel parcel, int i) {
        parcel.writeByteArray(this.rawData);
        parcel.writeInt(this.rssi);
        parcel.writeByte(this.isEnableConnect ? (byte) 1 : (byte) 0);
    }

    public String toString() {
        return "BleScanMessage{rawData=" + AppUtil.byte2HexStr(this.rawData) + ", rssi=" + this.rssi + ", isEnableConnect=" + this.isEnableConnect + '}';
    }
}
