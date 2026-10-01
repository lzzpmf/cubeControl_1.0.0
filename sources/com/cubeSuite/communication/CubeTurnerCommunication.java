package com.cubeSuite.communication;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class CubeTurnerCommunication {
    public static int hardware;

    public static byte[] cubeTurnerWriteModel(byte b) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, JNIUtils.getCubeTurnerStructAddr("state") + hardware);
    }

    public static byte[] cubeTurnerWriteCustomData(int i, int i2) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf((byte) i));
        return Communication.makeWritePacket(arrayList, JNIUtils.getCubeTurnerStructAddr("usr") + (i2 * 2) + (hardware * 4));
    }

    public static byte[] cubeTurnerWriteToggleData(int i, int i2) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf((byte) i));
        return Communication.makeWritePacket(arrayList, JNIUtils.getCubeTurnerStructAddr("usr") + 1 + (i2 * 2) + (hardware * 4));
    }

    public static byte[] cubeTurnerReadCurrentModel() {
        return Communication.makeReadPacket(0L, JNIUtils.getCubeTurnerStructSize());
    }

    public static Map<String, Object> pasingCubeTurnerModelData(byte[] bArr) {
        HashMap map = new HashMap();
        int[] iArr = new int[2];
        iArr[0] = 0;
        iArr[1] = 0;
        int[] iArr2 = new int[2];
        iArr2[0] = 0;
        iArr2[1] = 0;
        map.put("typeArr", iArr);
        map.put("toggleArr", iArr2);
        try {
            byte[] bArrParsingResData = Communication.parsingResData(bArr);
            hardware = bArrParsingResData[JNIUtils.getCubeTurnerStructAddr("hardware")];
            for (int i = 0; i < 2; i++) {
                int i2 = i * 2;
                iArr[i] = bArrParsingResData[JNIUtils.getCubeTurnerStructAddr("usr") + i2 + (hardware * 4)] & UByte.MAX_VALUE;
                iArr2[i] = bArrParsingResData[JNIUtils.getCubeTurnerStructAddr("usr") + 1 + i2 + (hardware * 4)] & UByte.MAX_VALUE;
            }
            map.put("mode", Integer.valueOf(bArrParsingResData[JNIUtils.getCubeTurnerStructAddr("state") + hardware] & UByte.MAX_VALUE));
            map.put("midiChannel", Integer.valueOf(bArrParsingResData[JNIUtils.getCubeTurnerStructAddr("usrchan") + hardware] & UByte.MAX_VALUE));
            return map;
        } catch (Exception e) {
            e.printStackTrace();
            map.put("mode", 0);
            return map;
        }
    }

    public static byte[] cubeTurnerWriteChannelValue(byte b) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(b));
        return Communication.makeWritePacket(arrayList, JNIUtils.getCubeTurnerStructAddr("usrchan") + hardware);
    }
}
