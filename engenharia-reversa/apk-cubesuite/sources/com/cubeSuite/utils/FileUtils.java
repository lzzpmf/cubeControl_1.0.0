package com.cubeSuite.utils;

import android.content.Context;
import android.content.Intent;
import android.content.pm.ResolveInfo;
import android.media.MediaMetadataRetriever;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.text.TextUtils;
import androidx.core.content.FileProvider;
import com.cubeSuite.BuildConfig;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class FileUtils {
    public static List<String> readDirAllFileName(String str, String str2) {
        ArrayList arrayList = new ArrayList();
        File[] fileArrListFiles = new File(str).listFiles();
        for (int i = 0; i < ((File[]) Objects.requireNonNull(fileArrListFiles)).length; i++) {
            if (fileArrListFiles[i].isFile()) {
                System.out.println("文     件：" + fileArrListFiles[i].getName());
                String name = fileArrListFiles[i].getName();
                if (name.endsWith(str2)) {
                    arrayList.add(name);
                }
            }
        }
        return arrayList;
    }

    public static List<Byte> readFile(String str) {
        byte[] bArr = new byte[1024];
        ArrayList arrayList = new ArrayList(1024);
        try {
            FileInputStream fileInputStream = new FileInputStream(str);
            while (true) {
                try {
                    int i = fileInputStream.read(bArr);
                    if (i == -1) {
                        fileInputStream.close();
                        return arrayList;
                    }
                    for (int i2 = 0; i2 < i; i2++) {
                        arrayList.add(Byte.valueOf(bArr[i2]));
                    }
                } catch (Throwable th) {
                    try {
                        fileInputStream.close();
                    } catch (Throwable th2) {
                        th.addSuppressed(th2);
                    }
                    throw th;
                }
                return arrayList;
            }
        } catch (Exception unused) {
            return arrayList;
        }
    }

    public static boolean saveFile(String str, String str2, List<Byte> list) {
        byte[] bArr = new byte[list.size()];
        for (int i = 0; i < list.size(); i++) {
            bArr[i] = list.get(i).byteValue();
        }
        return saveFile(str, str2, bArr);
    }

    public static boolean saveFile(String str, String str2, byte[] bArr) throws Throwable {
        FileOutputStream fileOutputStream = null;
        try {
            File file = new File(str + "/" + str2);
            File parentFile = file.getParentFile();
            if (parentFile != null && !parentFile.exists() && !parentFile.mkdirs()) {
                return false;
            }
            FileOutputStream fileOutputStream2 = new FileOutputStream(file);
            try {
                fileOutputStream2.write(bArr);
                fileOutputStream2.flush();
                try {
                    fileOutputStream2.close();
                } catch (IOException unused) {
                }
                return true;
            } catch (FileNotFoundException unused2) {
                fileOutputStream = fileOutputStream2;
            } catch (IOException unused3) {
                fileOutputStream = fileOutputStream2;
                if (fileOutputStream != null) {
                    try {
                        fileOutputStream.close();
                    } catch (IOException unused4) {
                    }
                }
                return false;
            } catch (Throwable th) {
                fileOutputStream = fileOutputStream2;
                th = th;
                if (fileOutputStream != null) {
                    try {
                        fileOutputStream.close();
                    } catch (IOException unused5) {
                    }
                }
                throw th;
            }
        } catch (FileNotFoundException unused6) {
        } catch (IOException unused7) {
        } catch (Throwable th2) {
            th = th2;
        }
        if (fileOutputStream != null) {
            try {
                fileOutputStream.close();
            } catch (IOException unused8) {
            }
        }
        return false;
    }

    public static void shareFile(Context context, File file) {
        if (file == null || !file.exists()) {
            return;
        }
        Intent intent = new Intent("android.intent.action.SEND");
        intent.putExtra("android.intent.extra.STREAM", Uri.fromFile(file));
        intent.setType(getMimeType(file.getAbsolutePath()));
        intent.setFlags(268435456);
        intent.addFlags(1);
        context.startActivity(Intent.createChooser(intent, "share"));
    }

    public static void shareFile(Context context, String str) {
        File file = new File(str);
        if (file.exists()) {
            Intent intent = new Intent("android.intent.action.SEND");
            Uri uriForFile = FileProvider.getUriForFile(context, BuildConfig.APPLICATION_ID, file);
            intent.putExtra("android.intent.extra.STREAM", uriForFile);
            Iterator<ResolveInfo> it = context.getPackageManager().queryIntentActivities(intent, 65536).iterator();
            while (it.hasNext()) {
                context.grantUriPermission(it.next().activityInfo.packageName, uriForFile, 1);
            }
            intent.setType("application/octet-stream");
            intent.setFlags(268435456);
            intent.addFlags(1);
            context.startActivity(Intent.createChooser(intent, "分享文件"));
        }
    }

    public static String readStream(InputStream inputStream) {
        try {
            byte[] bArr = new byte[inputStream.available()];
            inputStream.read(bArr);
            String str = new String(bArr, "GBK");
            inputStream.close();
            return str;
        } catch (Exception unused) {
            return "";
        }
    }

    public static byte[] readFile(InputStream inputStream) {
        try {
            byte[] bArr = new byte[inputStream.available()];
            inputStream.read(bArr);
            inputStream.close();
            return bArr;
        } catch (Exception unused) {
            return new byte[0];
        }
    }

    private static String getMimeType(String str) {
        MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
        if (str != null) {
            try {
                mediaMetadataRetriever.setDataSource(str);
                return mediaMetadataRetriever.extractMetadata(12);
            } catch (RuntimeException unused) {
                return "*/*";
            }
        }
        return "*/*";
    }

    public static String splicingFilePath(Context context, String str, String str2, String str3, String str4) {
        String rootPath = getRootPath(context);
        if (!TextUtils.isEmpty(str)) {
            if (str.contains(File.separator)) {
                for (String str5 : str.split(File.separator)) {
                    if (!TextUtils.isEmpty(str5)) {
                        rootPath = rootPath + File.separator + str5;
                        File file = new File(rootPath);
                        if (!file.exists()) {
                            file.mkdir();
                        }
                    }
                }
            } else {
                rootPath = rootPath + File.separator + str;
                File file2 = new File(rootPath);
                if (!file2.exists()) {
                    file2.mkdir();
                }
            }
            if (TextUtils.isEmpty(str2)) {
                return rootPath;
            }
            String str6 = rootPath + File.separator + str2;
            File file3 = new File(str6);
            if (!file3.exists()) {
                file3.mkdir();
            }
            if (TextUtils.isEmpty(str3)) {
                return str6;
            }
            String str7 = str6 + File.separator + str3;
            File file4 = new File(str7);
            if (!file4.exists()) {
                file4.mkdir();
            }
            if (TextUtils.isEmpty(str4)) {
                return str7;
            }
            rootPath = str7 + File.separator + str4;
            File file5 = new File(rootPath);
            if (!file5.exists()) {
                file5.mkdir();
            }
        }
        return rootPath;
    }

    private static String getRootPath(Context context) {
        String path = Environment.getExternalStorageDirectory().getPath();
        if (Build.VERSION.SDK_INT >= 29 && context != null) {
            File externalFilesDir = context.getExternalFilesDir(null);
            if (externalFilesDir != null) {
                return externalFilesDir.getPath();
            }
        }
        return path;
    }
}
