package com.cubeSuite.entity.fc2;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class AdvCustomStruct1 {
    public AddrU8 mode;
    public MidiCodeStruct[] midiCodeA = new MidiCodeStruct[16];
    public MidiCodeStruct[] midiCodeB = new MidiCodeStruct[16];
    public byte[] sysExA = new byte[128];
    public byte[] sysExB = new byte[128];

    public AdvCustomStruct1() {
        int i = 0;
        int i2 = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeA;
            if (i2 >= midiCodeStructArr.length) {
                break;
            }
            midiCodeStructArr[i2] = new MidiCodeStruct();
            i2++;
        }
        while (true) {
            MidiCodeStruct[] midiCodeStructArr2 = this.midiCodeB;
            if (i >= midiCodeStructArr2.length) {
                return;
            }
            midiCodeStructArr2[i] = new MidiCodeStruct();
            i++;
        }
    }
}
