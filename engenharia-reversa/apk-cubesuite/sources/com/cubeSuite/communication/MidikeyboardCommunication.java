package com.cubeSuite.communication;

import com.cubeSuite.callback.Callback;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Sys;
import java.util.Timer;
import java.util.TimerTask;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardCommunication {
    private final CommunicationUtil cu;
    private int sendColorInterval = 0;
    private Timer timer = new Timer();
    private TimerTask updateTask;

    static /* synthetic */ int access$008(MidikeyboardCommunication midikeyboardCommunication) {
        int i = midikeyboardCommunication.sendColorInterval;
        midikeyboardCommunication.sendColorInterval = i + 1;
        return i;
    }

    public MidikeyboardCommunication(CommunicationUtil communicationUtil) {
        this.cu = communicationUtil;
        TimerTask timerTask = new TimerTask() { // from class: com.cubeSuite.communication.MidikeyboardCommunication.1
            @Override // java.util.TimerTask, java.lang.Runnable
            public void run() {
                if (MidikeyboardCommunication.this.sendColorInterval < 10) {
                    MidikeyboardCommunication.access$008(MidikeyboardCommunication.this);
                }
            }
        };
        this.updateTask = timerTask;
        this.timer.schedule(timerTask, 10L, 10L);
    }

    public void sendUsrU8(AddrU8 addrU8) {
        sendDataToDevice((byte) 5, addrU8.getAddr(), new byte[]{(byte) addrU8.getData()});
    }

    public void sendUsrI8(AddrI8 addrI8) {
        sendDataToDevice((byte) 5, addrI8.getAddr(), new byte[]{addrI8.getData()});
    }

    public void sendGlobU8(AddrU8 addrU8) {
        sendDataToDevice((byte) 4, addrU8.getAddr(), new byte[]{(byte) addrU8.getData()});
    }

    public void sendGlobU16(AddrU16 addrU16) {
        sendDataToDevice((byte) 4, addrU16.getAddr(), Communication.u16AddU8Arr(addrU16.getData()));
    }

    public void sendDataToDevice(byte b, long j, byte[] bArr) {
        this.cu.splitWriteData(b, bArr, j);
    }

    public void saveConfig(Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, new byte[0], 0L, bleCommunicationCallback);
    }

    public void sendColor(AddrU8[] addrU8Arr) {
        if (this.sendColorInterval == 10) {
            sendDataToDevice((byte) 5, addrU8Arr[0].getAddr(), new byte[]{(byte) addrU8Arr[0].getData(), (byte) addrU8Arr[1].getData(), (byte) addrU8Arr[2].getData()});
            this.sendColorInterval = 0;
        }
    }

    public void sendSysEx(Sys sys) {
        int data = sys.getLen().getData() + 1;
        byte[] bArr = new byte[data];
        for (int i = 1; i < data; i++) {
            bArr[i] = (byte) sys.getVal()[i - 1].getData();
        }
        bArr[0] = (byte) sys.getLen().getData();
        sendDataToDevice((byte) 5, sys.getLen().getAddr(), bArr);
    }

    public void resetFactory(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.cu.splitWriteData((byte) 5, bArr, 0L, bleCommunicationCallback);
    }
}
