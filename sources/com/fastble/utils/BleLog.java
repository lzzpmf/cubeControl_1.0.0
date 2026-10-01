package com.fastble.utils;

import android.util.Log;

/* JADX INFO: loaded from: classes.dex */
public final class BleLog {
    private static final String defaultTag = "FastBle";
    public static boolean isPrint = true;

    public static void d(String str) {
        if (!isPrint || str == null) {
            return;
        }
        Log.d(defaultTag, str);
    }

    public static void i(String str) {
        if (!isPrint || str == null) {
            return;
        }
        Log.i(defaultTag, str);
    }

    public static void w(String str) {
        if (!isPrint || str == null) {
            return;
        }
        Log.w(defaultTag, str);
    }

    public static void e(String str) {
        if (!isPrint || str == null) {
            return;
        }
        Log.e(defaultTag, str);
    }
}
