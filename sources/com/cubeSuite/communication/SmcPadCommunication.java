package com.cubeSuite.communication;

import com.cubeSuite.callback.Callback;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.smcPad.SmcPadEntry;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadCommunication {
    private CommunicationUtil cu;

    public SmcPadCommunication(CommunicationUtil communicationUtil) {
        this.cu = communicationUtil;
    }

    public void sendUsrU8(AddrU8 addrU8) {
        sendDataToDevice((byte) 5, addrU8.getAddr(), new byte[]{(byte) addrU8.getData()});
    }

    public void sendGlobU8(AddrU8 addrU8) {
        sendDataToDevice((byte) 4, addrU8.getAddr(), new byte[]{(byte) addrU8.getData()});
    }

    public void sendGlobI8(AddrI8 addrI8) {
        sendDataToDevice((byte) 4, addrI8.getAddr(), new byte[]{addrI8.getData()});
    }

    public void sendGlobU16(AddrU16 addrU16) {
        sendDataToDevice((byte) 4, addrU16.getAddr(), Communication.u16AddU8Arr(addrU16.getData()));
    }

    public void sendColor(AddrU8[] addrU8Arr) {
        sendDataToDevice((byte) 5, addrU8Arr[0].getAddr(), new byte[]{(byte) addrU8Arr[0].getData(), (byte) addrU8Arr[1].getData(), (byte) addrU8Arr[2].getData()});
    }

    public void sendDataToDevice(byte b, long j, byte[] bArr) {
        this.cu.splitWriteData(b, bArr, j);
    }

    public void sendSysEx(SmcPadEntry.SmcPadUser.SmcPadSys smcPadSys) {
        int data = smcPadSys.len.getData() + 1;
        byte[] bArr = new byte[data];
        for (int i = 1; i < data; i++) {
            bArr[i] = (byte) smcPadSys.val[i - 1].getData();
        }
        bArr[0] = (byte) smcPadSys.len.getData();
        sendDataToDevice((byte) 5, smcPadSys.len.getAddr(), bArr);
    }

    public void resetFactory(byte[] bArr, final byte[] bArr2, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 4, bArr, 0L, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.SmcPadCommunication.1
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    SmcPadCommunication.this.cu.splitWriteData((byte) 5, bArr2, 0L, bleCommunicationCallback);
                } else {
                    bleCommunicationCallback.writeDataCallback(false);
                }
            }
        });
    }

    public void resetFactory(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, bArr, 0L, bleCommunicationCallback);
    }

    public void saveConfig(int i, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, new byte[0], i, bleCommunicationCallback);
    }
}
