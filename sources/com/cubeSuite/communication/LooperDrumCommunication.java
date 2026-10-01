package com.cubeSuite.communication;

import com.cubeSuite.callback.Callback;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class LooperDrumCommunication {
    private final int BLOCK_SIZE = 512;
    private CommunicationUtil communicationUtil;

    public interface DrumDataCallback {
        void dataCallback(boolean z, Map<String, Object> map);
    }

    public LooperDrumCommunication(CommunicationUtil communicationUtil) {
        this.communicationUtil = communicationUtil;
    }

    public void playVoice(byte[] bArr) {
        if (this.communicationUtil == null) {
            return;
        }
        int length = bArr.length;
        int i = length + 1;
        byte[] bArr2 = new byte[i];
        bArr2[0] = -103;
        System.arraycopy(bArr, 1, bArr2, 1, length);
        this.communicationUtil.writeDataToBleNotRes(Communication.makeWriteMidiPacket(bArr2, i));
    }

    public void playVoice(List<Byte> list) {
        if (this.communicationUtil == null || list.isEmpty()) {
            return;
        }
        int size = list.size();
        byte[] bArr = new byte[size];
        for (int i = 0; i < size; i++) {
            bArr[i] = list.get(i).byteValue();
        }
        int i2 = size + 1;
        byte[] bArr2 = new byte[i2];
        bArr2[0] = -103;
        System.arraycopy(bArr, 0, bArr2, 1, size);
        this.communicationUtil.writeDataToBleNotRes(Communication.makeWriteMidiPacket(bArr2, i2));
    }

    public void readDrumData(final int i, final DrumDataCallback drumDataCallback) {
        final HashMap map = new HashMap();
        this.communicationUtil.splitReadData((byte) 6, i, 1, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.LooperDrumCommunication.1
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    byte[] bArrParsingResData = Communication.parsingResData(bArr);
                    if (bArrParsingResData.length > 0) {
                        map.put("beat", Byte.valueOf(bArrParsingResData[0]));
                        LooperDrumCommunication.this.communicationUtil.splitReadData((byte) 6, ((long) (i + 1)) * 512, 512, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.LooperDrumCommunication.1.1
                            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                            public void readDataCallback(boolean z2, byte[] bArr2) {
                                if (z2) {
                                    if (bArr2.length == 512) {
                                        map.put("drumData", bArr2);
                                        drumDataCallback.dataCallback(true, map);
                                        return;
                                    } else {
                                        drumDataCallback.dataCallback(false, map);
                                        return;
                                    }
                                }
                                drumDataCallback.dataCallback(false, map);
                            }
                        });
                        return;
                    } else {
                        drumDataCallback.dataCallback(false, map);
                        return;
                    }
                }
                drumDataCallback.dataCallback(false, map);
            }
        });
    }

    public byte[] readDrumBeat(int i) {
        return Communication.makeReadPacketToRAM(i, 1);
    }

    public void sendAllDrumData(final byte[] bArr, int i, final int i2, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.communicationUtil.writeDataToBle(Communication.makeWritePacketToRAM(new byte[]{(byte) i}, i2), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.LooperDrumCommunication.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    LooperDrumCommunication.this.communicationUtil.splitWriteData((byte) 6, bArr, ((long) (i2 + 1)) * 512, bleCommunicationCallback);
                } else {
                    bleCommunicationCallback.writeDataCallback(z);
                }
            }
        });
    }

    public void readAllData(Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.communicationUtil.splitReadData((byte) 6, 0L, 15872, bleCommunicationCallback);
    }

    public void writeAllData(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.communicationUtil.splitWriteData((byte) 6, bArr, 0L, bleCommunicationCallback);
    }

    public void sendDrumData(int i, int i2, int i3) {
        this.communicationUtil.writeDataToBle(Communication.makeWritePacketToRAM(new byte[]{(byte) i}, (((long) (i3 + 1)) * 512) + ((long) i2)));
    }

    public void sendDrumData(int i, int i2, int i3, Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.communicationUtil.writeDataToBle(Communication.makeWritePacketToRAM(new byte[]{(byte) i}, (((long) (i3 + 1)) * 512) + ((long) i2)), bleCommunicationCallback);
    }

    public void sendBeatData(int i, int i2) {
        this.communicationUtil.writeDataToBleNotRes(Communication.makeWritePacketToRAM(new byte[]{(byte) i2}, i));
    }

    public void addBeatData(int i, int i2) {
        this.communicationUtil.writeDataToBleNotRes(Communication.makeWritePacketToRAM(new byte[]{(byte) i2}, i));
        byte[] bArr = new byte[64];
        Arrays.fill(bArr, (byte) 0);
        this.communicationUtil.splitWriteData((byte) 6, bArr, (((long) (i + 1)) * 512) + (((long) (i2 - 1)) * 64));
    }

    public void saveDrumData(Callback.BleCommunicationCallback bleCommunicationCallback) {
        this.communicationUtil.writeDataToBle(Communication.makeWritePacketToRAM(new byte[0], 0L), bleCommunicationCallback);
    }
}
