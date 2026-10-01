package com.cubeSuite.communication;

import com.cubeSuite.entity.footCtrl.CustomData;
import com.cubeSuite.entity.footCtrl.FootCtrlEntry;

/* JADX INFO: loaded from: classes.dex */
public class JNIUtils {
    public static native byte[] CustomDataToByteArr(CustomData customData);

    public static native FootCtrlEntry dataToFootCtrlEntry(byte[] bArr);

    public static native int getCubeTurnerStructAddr(String str);

    public static native int getCubeTurnerStructSize();

    public static native int getFootCtrlAddr(String str);

    public static native int getFootCtrlSize();

    static {
        System.loadLibrary("GetOffset");
    }
}
