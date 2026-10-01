package com.cubeSuite.communication;

import com.cubeSuite.callback.Callback;
import com.cubeSuite.entity.SmcMixerEntry.SmcSys;
import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class SmcCommunication {
    private CommunicationUtil cu;

    public SmcCommunication(CommunicationUtil communicationUtil) {
        this.cu = communicationUtil;
    }

    public void sendUsrU8(AddrU8 addrU8) {
        sendDataToDevice((byte) 5, addrU8.getAddr(), new byte[]{(byte) addrU8.getData()});
    }

    public void sendDataToDevice(byte b, long j, byte[] bArr) {
        this.cu.splitWriteData(b, bArr, j);
    }

    public void sendSysEx(SmcSys smcSys) {
        int data = smcSys.len.getData() + 1;
        byte[] bArr = new byte[data];
        for (int i = 1; i < data; i++) {
            bArr[i] = (byte) smcSys.val[i - 1].getData();
        }
        bArr[0] = (byte) smcSys.len.getData();
        sendDataToDevice((byte) 5, smcSys.len.getAddr(), bArr);
    }

    public void resetFactory(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, bArr, 0L, bleCommunicationCallback);
    }

    public void saveConfig(Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, new byte[0], 0L, bleCommunicationCallback);
    }
}
