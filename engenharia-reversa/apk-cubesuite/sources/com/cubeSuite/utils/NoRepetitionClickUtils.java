package com.cubeSuite.utils;

/* JADX INFO: loaded from: classes.dex */
public class NoRepetitionClickUtils {
    private static final int SPACE_TIME = 3000;
    private static long lastClickTime;

    public static void initLastClickTime() {
        lastClickTime = 0L;
    }

    public static synchronized boolean isRepetitionClick() {
        long jCurrentTimeMillis = System.currentTimeMillis();
        if (jCurrentTimeMillis - lastClickTime <= 3000) {
            return true;
        }
        lastClickTime = jCurrentTimeMillis;
        return false;
    }
}
