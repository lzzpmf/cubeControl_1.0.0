package com.cubeSuite.entity.footCtrl;

import java.lang.reflect.Array;

/* JADX INFO: loaded from: classes.dex */
public class FootCtrlEntry {
    private final int MAX_MODE;
    private CustomData[] customData;
    private byte[] customKeyboard;
    private byte state;
    private byte[][] sysExA;
    private byte[][] sysExB;
    private byte trsMidi;
    private int[] usr;
    private byte usrChan;

    public FootCtrlEntry() {
        int i = 0;
        this.state = (byte) 0;
        this.usrChan = (byte) 0;
        this.usr = new int[5];
        this.trsMidi = (byte) 0;
        this.customData = new CustomData[5];
        this.sysExA = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, 4, 128);
        this.sysExB = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, 4, 128);
        this.customKeyboard = new byte[16];
        this.MAX_MODE = 10;
        while (true) {
            CustomData[] customDataArr = this.customData;
            if (i >= customDataArr.length) {
                return;
            }
            customDataArr[i] = new CustomData();
            i++;
        }
    }

    public FootCtrlEntry(byte b, byte b2, int[] iArr, byte b3, CustomData[] customDataArr, byte[][] bArr, byte[][] bArr2, byte[] bArr3) {
        this.state = (byte) 0;
        this.usrChan = (byte) 0;
        this.usr = new int[5];
        this.trsMidi = (byte) 0;
        this.customData = new CustomData[5];
        this.sysExA = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, 4, 128);
        this.sysExB = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, 4, 128);
        this.customKeyboard = new byte[16];
        this.MAX_MODE = 10;
        this.state = (byte) Math.max(Math.min(10, (int) b), 0);
        this.usrChan = b2;
        this.usr = iArr;
        this.trsMidi = b3;
        this.customData = customDataArr;
        this.sysExA = bArr;
        this.sysExB = bArr2;
        this.customKeyboard = bArr3;
    }

    public byte[] getCustomKeyboard() {
        return this.customKeyboard;
    }

    public void setCustomKeyboard(byte[] bArr) {
        this.customKeyboard = bArr;
    }

    public byte[][] getSysExA() {
        return this.sysExA;
    }

    public void setSysExA(byte[][] bArr) {
        this.sysExA = bArr;
    }

    public byte[][] getSysExB() {
        return this.sysExB;
    }

    public void setSysExB(byte[][] bArr) {
        this.sysExB = bArr;
    }

    public byte getUsrChan() {
        return this.usrChan;
    }

    public void setUsrChan(byte b) {
        this.usrChan = b;
    }

    public int[] getUsr() {
        return this.usr;
    }

    public void setUsr(int[] iArr) {
        this.usr = iArr;
    }

    public byte getState() {
        return this.state;
    }

    public void setState(int i) {
        this.state = (byte) Math.max(Math.min(10, i), 0);
    }

    public byte getTrsMidi() {
        return this.trsMidi;
    }

    public void setTrsMidi(byte b) {
        this.trsMidi = b;
    }

    public CustomData[] getCustomData() {
        return this.customData;
    }

    public void setCustomData(CustomData[] customDataArr) {
        this.customData = customDataArr;
    }
}
