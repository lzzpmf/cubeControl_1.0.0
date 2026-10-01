package com.cubeSuite.communication;

/* JADX INFO: loaded from: classes.dex */
public class LooperCommunication {
    public static byte[] looperProSetVolume(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 7, (byte) i}, 3);
    }

    public static byte[] looperProSetGate(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 31, (byte) i}, 3);
    }

    public static byte[] looperProOn(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 20, (byte) i}, 3);
    }

    public static byte[] looperProNext(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 21, (byte) i}, 3);
    }

    public static byte[] looperProStop(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 22, (byte) i}, 3);
    }

    public static byte[] looperProSync(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 23, (byte) i}, 3);
    }

    public static byte[] looperMiniSetVolume(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 7, (byte) i}, 3);
    }

    public static byte[] looperMiniSetGate(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 31, (byte) i}, 3);
    }

    public static byte[] looperMiniStop(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 21, (byte) i}, 3);
    }

    public static byte[] looperMiniSync(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 22, (byte) i}, 3);
    }

    public static byte[] looperAutoSetVolume(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 7, (byte) i}, 3);
    }

    public static byte[] looperAutoSetGate(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 31, (byte) i}, 3);
    }

    public static byte[] looperAutoOn(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 20, (byte) i}, 3);
    }

    public static byte[] looperAutoNext(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 21, (byte) i}, 3);
    }

    public static byte[] looperAutoStop(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 22, (byte) i}, 3);
    }

    public static byte[] looperAutoSync(int i) {
        return Communication.makeWriteMidiPacket(new byte[]{-80, 23, (byte) i}, 3);
    }
}
