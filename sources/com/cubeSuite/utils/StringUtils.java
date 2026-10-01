package com.cubeSuite.utils;

import java.util.regex.Matcher;
import java.util.regex.Pattern;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class StringUtils {
    public static int getNum(String str) {
        Matcher matcher = Pattern.compile("\\d+").matcher(str);
        if (matcher.find()) {
            return Integer.parseInt(matcher.group(0));
        }
        return 0;
    }

    public static String bytesToHexString(byte b) {
        StringBuilder sb = new StringBuilder("");
        String hexString = Integer.toHexString(b & UByte.MAX_VALUE);
        if (hexString.length() < 2) {
            sb.append(0);
        }
        sb.append(hexString);
        return sb.toString();
    }
}
