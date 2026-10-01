package com.cubeSuite.communication;

import androidx.core.view.MotionEventCompat;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class Communication {
    private static final byte CMD_ERASE = 33;
    private static final byte CMD_MIDI_WRITE = 21;
    public static final byte CMD_READ = 35;
    private static final byte CMD_WRITE = 34;
    public static final byte DEV_EXT = 4;
    public static final byte EFF_EXT = 3;
    public static final byte EXT_CNT = 8;
    public static final byte HEAD_ONE = 0;
    public static final byte HEAD_TWO = 89;
    public static final byte NAND_EXT = 1;
    public static final byte NOR_EXT = 0;
    private static final int REQ_HEAD_START = 6;
    public static final byte RSV1_EXT = 6;
    public static final byte RSV2_EXT = 7;
    public static final byte SD_EXT = 2;
    public static final byte USR_EXT = 5;
    public static final byte CMD_QUERY_NAME_VER = 17;
    private static final Byte CMD_Q_NAME_AND_VER = Byte.valueOf(CMD_QUERY_NAME_VER);
    private static final Byte CMD_Q_SHORTE_MSG = (byte) 18;
    private static final Byte CMD_Q_OTHER_INFO = (byte) 18;
    private static final Byte CMD_Q_FIRMWARE_VER = (byte) 19;
    private static final Byte CMD_Q_DEV_TYPE = (byte) 23;
    private static final Byte CMD_Q_SOUND_DATA_ADDR = (byte) 27;
    private static final Byte CMD_Q_CODEC_VAL = (byte) 32;

    public static byte[] parsingResData(byte[] bArr) {
        return bArr;
    }

    public static int threeByteToInt(byte b, byte b2, byte b3) {
        return (b & UByte.MAX_VALUE) | ((b2 & UByte.MAX_VALUE) << 8) | ((b3 & UByte.MAX_VALUE) << 16);
    }

    public static int u8ArrToU16(byte b, byte b2) {
        return (b & UByte.MAX_VALUE) | ((b2 & UByte.MAX_VALUE) << 8);
    }

    private static void addHead(List<Byte> list) {
        list.add((byte) 0);
        list.add(Byte.valueOf(HEAD_TWO));
    }

    private static void addLen(List<Byte> list, int i) {
        list.add(Byte.valueOf((byte) (i & 255)));
        list.add(Byte.valueOf((byte) ((i >> 8) & 255)));
        list.add(Byte.valueOf((byte) ((i >> 16) & 255)));
    }

    public static byte[] makeCustomErasePacket(byte b, long j) {
        ArrayList arrayList = new ArrayList(10);
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_ERASE));
        addLen(arrayList, 5);
        arrayList.add(Byte.valueOf(b));
        addAddr(arrayList, j);
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static void u32AddU8Arr(List<Byte> list, long j) {
        list.add(Byte.valueOf((byte) (j & 255)));
        list.add(Byte.valueOf((byte) ((j >> 8) & 255)));
        list.add(Byte.valueOf((byte) ((j >> 16) & 255)));
        list.add(Byte.valueOf((byte) ((j >> 24) & 255)));
    }

    public static void u24AddU8Arr(List<Byte> list, long j) {
        list.add(Byte.valueOf((byte) (j & 255)));
        list.add(Byte.valueOf((byte) ((j >> 8) & 255)));
        list.add(Byte.valueOf((byte) ((j >> 16) & 255)));
    }

    public static void strAddU8Arr(List<Byte> list, String str) {
        for (byte b : str.getBytes(StandardCharsets.UTF_8)) {
            list.add(Byte.valueOf(b));
        }
    }

    public static String u8ArrToString(List<Byte> list) {
        int size = list.size();
        byte[] bArr = new byte[size];
        for (int i = 0; i < size; i++) {
            bArr[i] = list.get(i).byteValue();
        }
        return new String(bArr);
    }

    public static void u16AddU8Arr(List<Byte> list, int i) {
        list.add(Byte.valueOf((byte) (i & 255)));
        list.add(Byte.valueOf((byte) ((i >> 8) & 255)));
    }

    public static byte[] u16AddU8Arr(int i) {
        return new byte[]{(byte) (i & 255), (byte) (i >> 8)};
    }

    public static long u8ArrToU32(List<Byte> list) {
        return ((list.get(3).byteValue() & UByte.MAX_VALUE) << 24) | (list.get(0).byteValue() & UByte.MAX_VALUE) | ((list.get(1).byteValue() & UByte.MAX_VALUE) << 8) | ((list.get(2).byteValue() & UByte.MAX_VALUE) << 16);
    }

    static int[] u16ToTwoInt(int i) {
        return new int[]{i & 255, i & MotionEventCompat.ACTION_POINTER_INDEX_MASK};
    }

    static void addChecksum(List<Byte> list) {
        Iterator<Byte> it = list.subList(6, list.size()).iterator();
        byte bByteValue = 0;
        while (it.hasNext()) {
            bByteValue = (byte) (bByteValue + it.next().byteValue());
        }
        list.add(Byte.valueOf((byte) (~bByteValue)));
    }

    static void addAddr(List<Byte> list, long j) {
        list.add(Byte.valueOf((byte) (j & 255)));
        list.add(Byte.valueOf((byte) ((j >> 8) & 255)));
        list.add(Byte.valueOf((byte) ((j >> 16) & 255)));
        list.add(Byte.valueOf((byte) ((j >> 24) & 255)));
    }

    public static byte[] makeWritePacket(List<Byte> list, long j) {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_WRITE));
        addLen(arrayList, list.size() + 8);
        arrayList.add((byte) 4);
        addAddr(arrayList, j);
        addLen(arrayList, list.size());
        Iterator<Byte> it = list.iterator();
        while (it.hasNext()) {
            arrayList.add(Byte.valueOf(it.next().byteValue()));
        }
        addChecksum(arrayList);
        byte[] bArr = new byte[arrayList.size()];
        return byteListToByteArr(arrayList);
    }

    public static byte[] makeWritePacketToRAM(byte[] bArr, long j) {
        return makeWritePacketByType((byte) 6, bArr, j);
    }

    public static byte[] makeWritePacketToByte(byte[] bArr, long j) {
        return makeWritePacketByType((byte) 4, bArr, j);
    }

    public static byte[] makeWritePacketByType(byte b, byte[] bArr, long j) {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_WRITE));
        addLen(arrayList, bArr.length + 8);
        arrayList.add(Byte.valueOf(b));
        addAddr(arrayList, j);
        addLen(arrayList, bArr.length);
        for (byte b2 : bArr) {
            arrayList.add(Byte.valueOf(b2));
        }
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static byte[] makeReadPacket(long j, int i) {
        return makeReadPacketByType((byte) 4, j, i);
    }

    public static byte[] makeReadPacketToRAM(long j, int i) {
        return makeReadPacketByType((byte) 6, j, i);
    }

    public static byte[] makeReadPacketByType(byte b, long j, int i) {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_READ));
        addLen(arrayList, 8);
        arrayList.add(Byte.valueOf(b));
        addAddr(arrayList, j);
        addLen(arrayList, i);
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static byte[] makeQueryNameAndVerPacket() {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_QUERY_NAME_VER));
        addLen(arrayList, 0);
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static byte[] makeQueryPacket(byte b) {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(b));
        addLen(arrayList, 0);
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static byte[] makeWriteMidiPacket(byte[] bArr, int i) {
        ArrayList arrayList = new ArrayList();
        addHead(arrayList);
        arrayList.add(Byte.valueOf(CMD_MIDI_WRITE));
        addLen(arrayList, i);
        for (byte b : bArr) {
            arrayList.add(Byte.valueOf(b));
        }
        addChecksum(arrayList);
        return byteListToByteArr(arrayList);
    }

    public static Map<String, String> parsingResNameAndVer(byte[] bArr) {
        HashMap map = new HashMap();
        map.put("name", "");
        map.put("version", "");
        char[] cArr = new char[bArr.length];
        for (int i = 0; i < bArr.length - 1; i++) {
            cArr[i] = (char) bArr[i];
        }
        String[] strArrSplit = String.valueOf(cArr).split("_");
        map.put("name", strArrSplit[0]);
        map.put("version", strArrSplit[1].substring(0, 3));
        return map;
    }

    public static long getAllLen(byte[] bArr) {
        return (((long) bArr[3]) & 255) | ((((long) bArr[4]) & 255) << 8) | ((255 & ((long) bArr[5])) << 16);
    }

    public static long getLen(byte[] bArr) {
        return ((long) (bArr[11] & UByte.MAX_VALUE)) | ((long) ((bArr[12] & UByte.MAX_VALUE) << 8)) | ((long) ((bArr[13] & UByte.MAX_VALUE) << 16));
    }

    public static int getAddr(byte[] bArr) {
        int i = bArr[7] & UByte.MAX_VALUE;
        int i2 = (bArr[8] & UByte.MAX_VALUE) << 8;
        return ((bArr[10] & UByte.MAX_VALUE) << 24) | i | i2 | ((bArr[9] & UByte.MAX_VALUE) << 16);
    }

    public static byte[] byteListToByteArr(List<Byte> list) {
        int size = list.size();
        byte[] bArr = new byte[size];
        for (int i = 0; i < size; i++) {
            bArr[i] = list.get(i).byteValue();
        }
        return bArr;
    }
}
