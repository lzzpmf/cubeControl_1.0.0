package com.cubeSuite.communication;

import com.cubeSuite.Global.Global;
import com.cubeSuite.entity.footCtrl.CustomData;
import com.cubeSuite.entity.footCtrl.FootCtrlEntry;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class FootCtrlCommunication {
    long ADDRESS_BASE = 0;

    public byte[] bluetoothPedalWriteModel(byte b) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, this.ADDRESS_BASE);
    }

    public byte[] bluetoothPedalWriteCustomData(int i, int i2) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf((byte) i));
        return Communication.makeWritePacket(arrayList, (((long) i2) * 2) + 2);
    }

    public byte[] bluetoothPedalWriteToggleData(int i, int i2) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf((byte) i));
        return Communication.makeWritePacket(arrayList, (((long) i2) * 2) + 3);
    }

    public byte[] bluetoothPedalReadCurrentModel() {
        return Communication.makeReadPacket(this.ADDRESS_BASE, 13);
    }

    public byte[] footCtrlWriteCustomKeyboardMode(byte b, int i) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, ((long) JNIUtils.getFootCtrlAddr("customKeyboard")) + (((long) i) * 4));
    }

    public Map<String, Object> pasingBluetoothPedalModelData(byte[] bArr) {
        HashMap map = new HashMap();
        int[] iArr = new int[5];
        iArr[0] = 0;
        iArr[1] = 0;
        iArr[2] = 0;
        iArr[3] = 0;
        iArr[4] = 0;
        int[] iArr2 = new int[5];
        iArr2[0] = 0;
        iArr2[1] = 0;
        iArr2[2] = 0;
        iArr2[3] = 0;
        iArr2[4] = 0;
        map.put("typeArr", iArr);
        map.put("toggleArr", iArr2);
        try {
            byte[] bArrParsingResData = Communication.parsingResData(bArr);
            for (int i = 0; i < 5; i++) {
                int i2 = i * 2;
                int length = bArrParsingResData[i2 + 2] & UByte.MAX_VALUE;
                if (i == 4) {
                    if (length >= Global.BluetoothPedalCustomCommand.MIDICC_STR.length) {
                        length = Global.BluetoothPedalCustomCommand.MIDICC_STR.length;
                    }
                } else if (length >= Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB.length) {
                    length = Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB.length;
                }
                iArr[i] = length;
                iArr2[i] = bArrParsingResData[i2 + 3] & UByte.MAX_VALUE;
            }
            map.put("mode", Integer.valueOf(bArrParsingResData[0] & UByte.MAX_VALUE));
            map.put("midiChannel", Integer.valueOf(bArrParsingResData[1] & UByte.MAX_VALUE));
            map.put("trsMidi", Integer.valueOf(bArrParsingResData[12] & UByte.MAX_VALUE));
            return map;
        } catch (Exception e) {
            e.printStackTrace();
            map.put("mode", 0);
            map.put("midiChannel", 0);
            map.put("trsMidi", 0);
            return map;
        }
    }

    public byte[] bluetoothPedalWriteChannelValue(byte b) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, 1L);
    }

    public byte[] bluetoothPedalWriteTrsMidi(byte b) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, 12L);
    }

    public byte[] readFootCtrlData() {
        return Communication.makeReadPacket(this.ADDRESS_BASE, 1000);
    }

    public Map<String, Object> parseFootCtrlData(byte[] bArr) {
        HashMap map = new HashMap();
        FootCtrlEntry footCtrlEntryDataToFootCtrlEntry = JNIUtils.dataToFootCtrlEntry(bArr);
        map.put("footCtrlEntry", footCtrlEntryDataToFootCtrlEntry);
        int[] iArr = new int[5];
        iArr[0] = 0;
        iArr[1] = 0;
        iArr[2] = 0;
        iArr[3] = 0;
        iArr[4] = 0;
        int[] iArr2 = new int[5];
        iArr2[0] = 0;
        iArr2[1] = 0;
        iArr2[2] = 0;
        iArr2[3] = 0;
        iArr2[4] = 0;
        int[] usr = footCtrlEntryDataToFootCtrlEntry.getUsr();
        for (int i = 0; i < 5; i++) {
            int[] iArrU16ToTwoInt = Communication.u16ToTwoInt(usr[i]);
            int length = iArrU16ToTwoInt[0];
            if (i == 4) {
                if (length >= Global.BluetoothPedalCustomCommand.MIDICC_STR.length) {
                    length = Global.BluetoothPedalCustomCommand.MIDICC_STR.length;
                }
            } else if (length >= Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB.length) {
                length = Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB.length;
            }
            iArr[i] = length;
            iArr2[i] = iArrU16ToTwoInt[1];
        }
        map.put("typeArr", iArr);
        map.put("toggleArr", iArr2);
        return map;
    }

    public byte[] footCtrlWriteCustomData(CustomData customData, int i) {
        return Communication.makeWritePacketToByte(JNIUtils.CustomDataToByteArr(customData), JNIUtils.getFootCtrlAddr("customData" + (i + 1)));
    }

    public byte[] footCtrlWriteCustomMidi(byte[] bArr, int i, int i2) {
        String str = "sysExA[" + i + "]";
        if (i2 == 1) {
            str = "sysExB[" + i + "]";
        }
        return Communication.makeWritePacketToByte(bArr, JNIUtils.getFootCtrlAddr(str));
    }
}
