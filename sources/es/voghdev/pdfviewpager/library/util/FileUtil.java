package es.voghdev.pdfviewpager.library.util;

import android.content.Context;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;

/* JADX INFO: loaded from: classes.dex */
public class FileUtil {
    public static boolean copyAsset(Context context, String str, String str2) throws IOException {
        InputStream inputStreamOpen = context.getAssets().open(str);
        new File(str2).createNewFile();
        FileOutputStream fileOutputStream = new FileOutputStream(new File(str2));
        byte[] bArr = new byte[1024];
        while (true) {
            int i = inputStreamOpen.read(bArr);
            if (i != -1) {
                fileOutputStream.write(bArr, 0, i);
            } else {
                inputStreamOpen.close();
                fileOutputStream.close();
                return true;
            }
        }
    }

    public static String extractFileNameFromURL(String str) {
        return str.substring(str.lastIndexOf(47) + 1);
    }
}
