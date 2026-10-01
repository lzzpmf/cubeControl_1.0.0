package com.cubeSuite.utils;

import android.app.Activity;
import android.app.ActivityManager;
import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Matrix;
import android.view.inputmethod.InputMethodManager;
import androidx.core.app.ActivityCompat;
import com.cubeSuite.activitys.ActivityStackUtil;
import java.io.File;
import java.util.Date;
import java.util.Locale;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class AppUtil {
    private static final long DOUBLE_CLICK_INTERVAL = 2000;
    public static final boolean IF_DEBUG = false;
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    private static long lastClickTime;
    private static final String[] PERMISSIONS_STORAGE = {"android.permission.READ_EXTERNAL_STORAGE", "android.permission.WRITE_EXTERNAL_STORAGE"};
    private static final char[] mChars = "0123456789ABCDEF".toCharArray();

    public static boolean RangeCheck2D(float f, float f2, int i, int i2, int i3, int i4) {
        return f >= ((float) i) && f <= ((float) i2) && f2 >= ((float) i3) && f2 <= ((float) i4);
    }

    public static boolean isHasLocationPermission(Context context) {
        return isHasPermission(context, "android.permission.ACCESS_COARSE_LOCATION");
    }

    public static boolean isHasStoragePermission(Context context) {
        return isHasPermission(context, "android.permission.WRITE_EXTERNAL_STORAGE") && isHasPermission(context, "android.permission.READ_EXTERNAL_STORAGE");
    }

    public static boolean isHasPermission(Context context, String str) {
        return context != null && ActivityCompat.checkSelfPermission(context, str) == 0;
    }

    public static boolean isFastDoubleClick() {
        return isFastDoubleClick(DOUBLE_CLICK_INTERVAL);
    }

    public static boolean isFastDoubleClick(long j) {
        long time = new Date().getTime();
        boolean z = time - lastClickTime <= j;
        lastClickTime = time;
        return z;
    }

    public static boolean isAppInBackground(Context context) {
        boolean z = true;
        for (ActivityManager.RunningAppProcessInfo runningAppProcessInfo : ((ActivityManager) context.getSystemService("activity")).getRunningAppProcesses()) {
            if (runningAppProcessInfo.importance == 100) {
                for (String str : runningAppProcessInfo.pkgList) {
                    if (str.equals(context.getPackageName())) {
                        z = false;
                    }
                }
            }
        }
        return z;
    }

    public static void closeKeybord(Activity activity) {
        InputMethodManager inputMethodManager = (InputMethodManager) activity.getSystemService("input_method");
        if (inputMethodManager != null) {
            inputMethodManager.hideSoftInputFromWindow(activity.getWindow().getDecorView().getWindowToken(), 0);
        }
    }

    public static int dp2px(float f) {
        return (int) ((f * ActivityStackUtil.getInstance().getActivity().getResources().getDisplayMetrics().density) + 0.5f);
    }

    public static void sleep(int i) {
        try {
            Thread.sleep(i);
        } catch (InterruptedException unused) {
        }
    }

    public static boolean fileIsExists(String str) {
        try {
            return new File(str).exists();
        } catch (Exception unused) {
            return false;
        }
    }

    public static Bitmap getScaleBitmap(Bitmap bitmap, double d, double d2) {
        Matrix matrix = new Matrix();
        matrix.postScale(((float) d) / bitmap.getWidth(), ((float) d2) / bitmap.getHeight());
        return Bitmap.createBitmap(bitmap, 0, 0, bitmap.getWidth(), bitmap.getHeight(), matrix, false);
    }

    public static void verifyStoragePermissions(Activity activity) {
        try {
            String[] strArr = PERMISSIONS_STORAGE;
            if (ActivityCompat.checkSelfPermission(activity, strArr[1]) != 0) {
                ActivityCompat.requestPermissions(activity, strArr, 1);
            }
        } catch (Exception unused) {
        }
    }

    public static String byte2HexStr(byte[] bArr) {
        return bArr == null ? "" : byte2HexStr(bArr, bArr.length);
    }

    public static String byte2HexStr(byte[] bArr, int i) {
        if (bArr == null) {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        for (int i2 = 0; i2 < i; i2++) {
            char[] cArr = mChars;
            sb.append(cArr[(bArr[i2] & UByte.MAX_VALUE) >> 4]);
            sb.append(cArr[bArr[i2] & 15]);
        }
        return sb.toString().trim().toUpperCase(Locale.US);
    }
}
