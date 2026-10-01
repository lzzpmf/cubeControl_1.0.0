package com.cubeSuite.entity.fc2;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class CustomDataStruct {
    public MidiCodeStruct[] midiCodeA = new MidiCodeStruct[16];
    public MidiCodeStruct[] midiCodeB = new MidiCodeStruct[16];
    public AddrU8 mode;

    public CustomDataStruct() {
        int i = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeA;
            if (i >= midiCodeStructArr.length) {
                return;
            }
            midiCodeStructArr[i] = new MidiCodeStruct();
            this.midiCodeB[i] = new MidiCodeStruct();
            i++;
        }
    }
}
