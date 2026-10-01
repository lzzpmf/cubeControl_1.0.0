package com.cubeSuite.utils;

import java.io.File;
import java.io.InputStream;
import java.io.RandomAccessFile;
import okhttp3.Headers;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/* JADX INFO: loaded from: classes.dex */
public class WGDownloadManager {

    private interface ICheckDownloadFile {
        void onError(String str);

        void onResult(long j, boolean z);
    }

    public interface IDownloadFile {
        void onBegin(long j, boolean z);

        void onError(String str);

        void onFinish(long j);

        void onProgress(int i, long j, long j2);
    }

    public void downloadFile(final String str, final String str2, final String str3, final IDownloadFile iDownloadFile) {
        checkDownloadFile(str, new ICheckDownloadFile() { // from class: com.cubeSuite.utils.WGDownloadManager.1
            @Override // com.cubeSuite.utils.WGDownloadManager.ICheckDownloadFile
            public void onResult(long j, boolean z) {
                boolean z2;
                long j2 = j;
                IDownloadFile iDownloadFile2 = iDownloadFile;
                if (iDownloadFile2 != null) {
                    iDownloadFile2.onBegin(j2, z);
                }
                try {
                    File file = new File(str2);
                    if (file.exists()) {
                        file.mkdirs();
                    }
                    File file2 = new File(file, str3);
                    long j3 = 0;
                    if (file2.exists() && file2.length() == j2) {
                        IDownloadFile iDownloadFile3 = iDownloadFile;
                        if (iDownloadFile3 != null) {
                            iDownloadFile3.onFinish(0L);
                            return;
                        }
                        return;
                    }
                    File file3 = new File(file, str3 + ".tmp");
                    long length = file3.exists() ? file3.length() : 0L;
                    if (file3.exists() && length == j2) {
                        if (file2.exists()) {
                            file2.delete();
                        }
                        file3.renameTo(file2);
                        IDownloadFile iDownloadFile4 = iDownloadFile;
                        if (iDownloadFile4 != null) {
                            iDownloadFile4.onFinish(0L);
                            return;
                        }
                        return;
                    }
                    if (!z && file3.exists()) {
                        file3.delete();
                        length = 0;
                    }
                    RandomAccessFile randomAccessFile = new RandomAccessFile(file3, "rwd");
                    randomAccessFile.seek(length);
                    Response responseExecute = new OkHttpClient().newCall(new Request.Builder().url(str).addHeader("Range", "bytes=" + length + "-").build()).execute();
                    InputStream inputStreamByteStream = responseExecute.body().byteStream();
                    byte[] bArr = new byte[5120];
                    boolean z3 = false;
                    while (true) {
                        if (!z3) {
                            int i = inputStreamByteStream.read(bArr);
                            z2 = z3;
                            if (i == -1) {
                                break;
                            }
                            randomAccessFile.write(bArr, 0, i);
                            long j4 = i;
                            long j5 = length + j4;
                            j3 += j4;
                            int i2 = (int) ((j5 / (j2 * 1.0d)) * 100.0d);
                            boolean z4 = j5 >= j2 ? true : z2;
                            IDownloadFile iDownloadFile5 = iDownloadFile;
                            if (iDownloadFile5 != null) {
                                iDownloadFile5.onProgress(i2, j5, j2);
                            }
                            j2 = j;
                            length = j5;
                            z3 = z4;
                        } else {
                            z2 = z3;
                            break;
                        }
                    }
                    randomAccessFile.close();
                    responseExecute.close();
                    if (z2) {
                        if (file2.exists()) {
                            file2.delete();
                        }
                        file3.renameTo(file2);
                        IDownloadFile iDownloadFile6 = iDownloadFile;
                        if (iDownloadFile6 != null) {
                            iDownloadFile6.onFinish(j3);
                        }
                    }
                } catch (Exception e) {
                    IDownloadFile iDownloadFile7 = iDownloadFile;
                    if (iDownloadFile7 != null) {
                        iDownloadFile7.onError(e.getLocalizedMessage());
                    }
                }
            }

            @Override // com.cubeSuite.utils.WGDownloadManager.ICheckDownloadFile
            public void onError(String str4) {
                IDownloadFile iDownloadFile2 = iDownloadFile;
                if (iDownloadFile2 != null) {
                    iDownloadFile2.onError(str4);
                }
            }
        });
    }

    private void checkDownloadFile(final String str, final ICheckDownloadFile iCheckDownloadFile) {
        new Thread(new Runnable() { // from class: com.cubeSuite.utils.WGDownloadManager.2
            @Override // java.lang.Runnable
            public void run() {
                boolean z;
                long contentLength;
                try {
                    Response responseExecute = new OkHttpClient().newCall(new Request.Builder().url(str).addHeader("Range", "bytes=0-0").build()).execute();
                    Headers headers = responseExecute.headers();
                    int i = 0;
                    while (true) {
                        if (i >= headers.size()) {
                            z = false;
                            contentLength = 0;
                            break;
                        }
                        String strName = headers.name(i);
                        if (strName.equalsIgnoreCase("Content-Range")) {
                            String str2 = "" + headers.get(strName);
                            z = true;
                            contentLength = Long.parseLong(str2.indexOf("/") > 0 ? str2.substring(str2.indexOf("/") + 1) : "0");
                            break;
                        }
                        i++;
                    }
                    responseExecute.close();
                    if (!z) {
                        contentLength = new OkHttpClient().newCall(new Request.Builder().url(str).build()).execute().body().getContentLength();
                        responseExecute.close();
                    }
                    ICheckDownloadFile iCheckDownloadFile2 = iCheckDownloadFile;
                    if (iCheckDownloadFile2 != null) {
                        iCheckDownloadFile2.onResult(contentLength, z);
                    }
                } catch (Exception e) {
                    ICheckDownloadFile iCheckDownloadFile3 = iCheckDownloadFile;
                    if (iCheckDownloadFile3 != null) {
                        iCheckDownloadFile3.onError(e.getLocalizedMessage());
                    }
                }
            }
        }).start();
    }
}
