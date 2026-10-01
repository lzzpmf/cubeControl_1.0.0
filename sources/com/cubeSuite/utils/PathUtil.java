package com.cubeSuite.utils;

import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

/* JADX INFO: loaded from: classes.dex */
public class PathUtil {
    private static String getFilePathForN(Context context, Uri uri) {
        try {
            Cursor cursorQuery = context.getContentResolver().query(uri, null, null, null, null);
            int columnIndex = cursorQuery.getColumnIndex("_display_name");
            cursorQuery.moveToFirst();
            File file = new File(context.getFilesDir(), cursorQuery.getString(columnIndex));
            InputStream inputStreamOpenInputStream = context.getContentResolver().openInputStream(uri);
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            byte[] bArr = new byte[inputStreamOpenInputStream.available()];
            while (true) {
                int i = inputStreamOpenInputStream.read(bArr);
                if (i != -1) {
                    fileOutputStream.write(bArr, 0, i);
                } else {
                    cursorQuery.close();
                    inputStreamOpenInputStream.close();
                    fileOutputStream.close();
                    return file.getPath();
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public static String getPath(Context context, Uri uri) {
        return getFilePathForN(context, uri);
    }

    public static String getFileExtension(String str) {
        int iLastIndexOf;
        if (str == null || str.isEmpty() || (iLastIndexOf = str.lastIndexOf(46)) < 0 || iLastIndexOf == str.length() - 1) {
            return "";
        }
        return str.substring(iLastIndexOf + 1);
    }
}
