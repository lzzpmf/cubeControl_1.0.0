package com.cubeSuite.entity.fc2;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class MidiCodeStruct {
    public AddrU8 channel;
    public AddrU8 data1;
    public AddrU8 data2;
    public AddrU8 isEnable;
    public AddrU8 type;

    public void reset() {
        this.isEnable.setData(0);
        this.channel.setData(0);
        this.type.setData(0);
        this.data1.setData(0);
        this.data2.setData(0);
    }

    public void copy(MidiCodeStruct midiCodeStruct) {
        this.isEnable.setData(midiCodeStruct.isEnable.getData());
        this.channel.setData(midiCodeStruct.channel.getData());
        this.type.setData(midiCodeStruct.type.getData());
        this.data1.setData(midiCodeStruct.data1.getData());
        this.data2.setData(midiCodeStruct.data2.getData());
    }
}
