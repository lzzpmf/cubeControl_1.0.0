package com.cubeSuite.utils;

/* JADX INFO: loaded from: classes.dex */
public class JNIClass {
    public static native int getCubeTurnerStructAddr(String str);

    static {
        System.loadLibrary("GetOffset");
    }
}
