package com.cubeSuite.entity.footCtrl;

/* JADX INFO: loaded from: classes.dex */
public class MidiCode {
    private byte channel;
    private byte data1;
    private byte data2;
    private byte isEnable;
    private byte type;

    public void clear() {
        this.channel = (byte) 0;
        this.type = (byte) 0;
        this.data1 = (byte) 0;
        this.data2 = (byte) 0;
    }

    public MidiCode(int i, int i2, int i3, int i4, int i5) {
        this.isEnable = (byte) 0;
        this.channel = (byte) 0;
        this.type = (byte) 0;
        this.data1 = (byte) 0;
        this.data2 = (byte) 0;
        this.isEnable = (byte) Math.max(Math.min(1, i), 0);
        this.channel = (byte) Math.max(Math.min(15, i2), 0);
        this.type = (byte) Math.max(Math.min(127, i3), 0);
        this.data1 = (byte) Math.max(Math.min(127, i4), 0);
        this.data2 = (byte) Math.max(Math.min(127, i5), 0);
    }

    public void copy(MidiCode midiCode) {
        this.isEnable = midiCode.isEnable;
        this.channel = midiCode.channel;
        this.type = midiCode.type;
        this.data1 = midiCode.data1;
        this.data2 = midiCode.data2;
    }

    public MidiCode() {
        this.isEnable = (byte) 0;
        this.channel = (byte) 0;
        this.type = (byte) 0;
        this.data1 = (byte) 0;
        this.data2 = (byte) 0;
    }

    public byte getIsEnable() {
        return this.isEnable;
    }

    public void setIsEnable(int i) {
        this.isEnable = (byte) Math.max(Math.min(1, i), 0);
    }

    public byte getChannel() {
        return this.channel;
    }

    public void setChannel(int i) {
        this.channel = (byte) Math.max(Math.min(15, i), 0);
    }

    public byte getType() {
        return this.type;
    }

    public void setType(int i) {
        this.type = (byte) Math.max(Math.min(127, i), 0);
    }

    public byte getData1() {
        return this.data1;
    }

    public void setData1(int i) {
        this.data1 = (byte) Math.max(Math.min(127, i), 0);
    }

    public byte getData2() {
        return this.data2;
    }

    public void setData2(int i) {
        this.data2 = (byte) Math.max(Math.min(127, i), 0);
    }
}
