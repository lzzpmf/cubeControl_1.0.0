package com.cubeSuite.entity.footCtrl;

/* JADX INFO: loaded from: classes.dex */
public class CustomData {
    private MidiCode[] midiCodeA;
    private MidiCode[] midiCodeB;
    private byte mode;

    public CustomData(int i, MidiCode[] midiCodeArr, MidiCode[] midiCodeArr2) {
        this.mode = (byte) 0;
        this.midiCodeA = new MidiCode[16];
        this.midiCodeB = new MidiCode[16];
        this.mode = (byte) Math.max(Math.min(4, i), 0);
        this.midiCodeA = midiCodeArr;
        this.midiCodeB = midiCodeArr2;
    }

    public CustomData() {
        int i = 0;
        this.mode = (byte) 0;
        this.midiCodeA = new MidiCode[16];
        this.midiCodeB = new MidiCode[16];
        int i2 = 0;
        while (true) {
            MidiCode[] midiCodeArr = this.midiCodeA;
            if (i2 >= midiCodeArr.length) {
                break;
            }
            midiCodeArr[i2] = new MidiCode();
            i2++;
        }
        while (true) {
            MidiCode[] midiCodeArr2 = this.midiCodeB;
            if (i >= midiCodeArr2.length) {
                return;
            }
            midiCodeArr2[i] = new MidiCode();
            i++;
        }
    }

    public byte getMode() {
        return this.mode;
    }

    public void setMode(int i) {
        this.mode = (byte) Math.max(Math.min(4, i), 0);
    }

    public MidiCode[] getMidiCodeA() {
        return this.midiCodeA;
    }

    public void setMidiCodeA(MidiCode[] midiCodeArr) {
        this.midiCodeA = midiCodeArr;
    }

    public MidiCode[] getMidiCodeB() {
        return this.midiCodeB;
    }

    public void setMidiCodeB(MidiCode[] midiCodeArr) {
        this.midiCodeB = midiCodeArr;
    }
}
