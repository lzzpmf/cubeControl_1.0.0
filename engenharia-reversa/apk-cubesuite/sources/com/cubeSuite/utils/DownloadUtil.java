package com.cubeSuite.utils;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.TimeUnit;
import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/* JADX INFO: loaded from: classes.dex */
public class DownloadUtil {
    private static final int DELTA = 262144;
    private static DownloadUtil downloadUtil;
    private int mCurrentDownload;
    private int mTotal;
    File mfile;
    private final OkHttpClient okHttpClient = new OkHttpClient.Builder().callTimeout(50, TimeUnit.SECONDS).connectTimeout(50, TimeUnit.SECONDS).readTimeout(50, TimeUnit.SECONDS).build();

    public interface OnDownloadListener {
        void onDownloadFailed(Exception exc);

        void onDownloadSuccess(File file);

        void onDownloading(int i);
    }

    static /* synthetic */ int access$212(DownloadUtil downloadUtil2, int i) {
        int i2 = downloadUtil2.mCurrentDownload + i;
        downloadUtil2.mCurrentDownload = i2;
        return i2;
    }

    public static DownloadUtil get() {
        if (downloadUtil == null) {
            downloadUtil = new DownloadUtil();
        }
        return downloadUtil;
    }

    private DownloadUtil() {
    }

    public void download(String str, final String str2, final String str3, final OnDownloadListener onDownloadListener) {
        this.okHttpClient.newCall(new Request.Builder().url(str).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.DownloadUtil.1
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                onDownloadListener.onDownloadFailed(iOException);
            }

            /* JADX WARN: Code duplicated, block: B:48:0x0086 A[EXC_TOP_SPLITTER, SYNTHETIC] */
            /* JADX WARN: Code duplicated, block: B:58:0x008b A[EXC_TOP_SPLITTER, SYNTHETIC] */
            /* JADX WARN: Code duplicated, block: B:69:? A[SYNTHETIC] */
            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws Throwable {
                FileOutputStream fileOutputStream;
                byte[] bArr = new byte[2048];
                File file = new File(str2);
                if (!file.exists()) {
                    file.mkdirs();
                }
                File file2 = new File(file, str3);
                InputStream inputStream = null;
                try {
                    InputStream inputStreamByteStream = response.body().byteStream();
                    try {
                        long contentLength = response.body().getContentLength();
                        fileOutputStream = new FileOutputStream(file2);
                        long j = 0;
                        while (true) {
                            try {
                                int i = inputStreamByteStream.read(bArr);
                                if (i == -1) {
                                    break;
                                }
                                fileOutputStream.write(bArr, 0, i);
                                j += (long) i;
                                onDownloadListener.onDownloading((int) (((j * 1.0f) / contentLength) * 100.0f));
                            } catch (Exception e) {
                                e = e;
                                inputStream = inputStreamByteStream;
                                try {
                                    e.printStackTrace();
                                    onDownloadListener.onDownloadFailed(e);
                                    if (inputStream != null) {
                                        try {
                                            inputStream.close();
                                        } catch (IOException unused) {
                                        }
                                    }
                                    if (fileOutputStream == null) {
                                        return;
                                    }
                                } catch (Throwable th) {
                                    th = th;
                                    if (inputStream != null) {
                                        try {
                                            inputStream.close();
                                        } catch (IOException unused2) {
                                        }
                                    }
                                    if (fileOutputStream != null) {
                                        try {
                                            fileOutputStream.close();
                                            throw th;
                                        } catch (IOException unused3) {
                                            throw th;
                                        }
                                    }
                                    throw th;
                                }
                            } catch (Throwable th2) {
                                th = th2;
                                inputStream = inputStreamByteStream;
                                if (inputStream != null) {
                                    inputStream.close();
                                }
                                if (fileOutputStream != null) {
                                    fileOutputStream.close();
                                    throw th;
                                }
                                throw th;
                            }
                        }
                        fileOutputStream.flush();
                        onDownloadListener.onDownloadSuccess(file2);
                        if (inputStreamByteStream != null) {
                            try {
                                inputStreamByteStream.close();
                            } catch (IOException unused4) {
                            }
                        }
                    } catch (Exception e2) {
                        e = e2;
                        fileOutputStream = null;
                    } catch (Throwable th3) {
                        th = th3;
                        fileOutputStream = null;
                    }
                } catch (Exception e3) {
                    e = e3;
                    fileOutputStream = null;
                } catch (Throwable th4) {
                    th = th4;
                    fileOutputStream = null;
                }
                try {
                    fileOutputStream.close();
                } catch (IOException unused5) {
                }
            }
        });
    }

    public void originDownload(final String str, final String str2, final String str3, final OnDownloadListener onDownloadListener) {
        new Thread(new Runnable() { // from class: com.cubeSuite.utils.DownloadUtil.2
            @Override // java.lang.Runnable
            public void run() {
                try {
                    File file = new File(str2);
                    if (!file.exists()) {
                        file.mkdirs();
                    }
                    File file2 = new File(file, str3);
                    HttpURLConnection httpURLConnection = (HttpURLConnection) new URL(str).openConnection();
                    long contentLength = httpURLConnection.getContentLength();
                    InputStream inputStream = httpURLConnection.getInputStream();
                    byte[] bArr = new byte[1024];
                    FileOutputStream fileOutputStream = new FileOutputStream(file2, true);
                    long j = 0;
                    while (true) {
                        int i = inputStream.read(bArr);
                        if (i != -1) {
                            fileOutputStream.write(bArr, 0, i);
                            j += (long) i;
                            onDownloadListener.onDownloading((int) (((j * 1.0f) / contentLength) * 100.0f));
                        } else {
                            onDownloadListener.onDownloadSuccess(file2);
                            fileOutputStream.close();
                            inputStream.close();
                            return;
                        }
                    }
                } catch (IOException e) {
                    onDownloadListener.onDownloadFailed(null);
                    e.printStackTrace();
                }
            }
        }).start();
    }

    private static void isChartPathExist(String str) {
        File file = new File(str);
        if (file.exists()) {
            return;
        }
        file.mkdirs();
    }

    private void getFileLength(String str, Callback callback) {
        this.okHttpClient.newCall(new Request.Builder().url(str).method("HEAD", null).build()).enqueue(callback);
    }

    public void downloadRange(final String str, final String str2, final String str3, final OnDownloadListener onDownloadListener) {
        this.okHttpClient.newCall(new Request.Builder().url(str).method("HEAD", null).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.DownloadUtil.3
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                if (response.isSuccessful()) {
                    DownloadUtil.this.mTotal = Integer.valueOf(response.header("Content-Length")).intValue();
                    File file = new File(str2);
                    if (!file.exists()) {
                        file.mkdirs();
                    }
                    DownloadUtil.this.mfile = new File(file, str3);
                    FileOutputStream fileOutputStream = new FileOutputStream(DownloadUtil.this.mfile, true);
                    DownloadUtil downloadUtil2 = DownloadUtil.this;
                    downloadUtil2.downloadRange(fileOutputStream, downloadUtil2.mTotal, str, onDownloadListener);
                }
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void downloadRange(final FileOutputStream fileOutputStream, int i, final String str, final OnDownloadListener onDownloadListener) {
        this.okHttpClient.newCall(new Request.Builder().url(str).addHeader("range", "bytes=" + i + "-" + Math.min(i + 262144, this.mTotal)).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.DownloadUtil.4
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                DownloadUtil downloadUtil2 = DownloadUtil.this;
                downloadUtil2.downloadRange(fileOutputStream, downloadUtil2.mCurrentDownload, str, onDownloadListener);
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                try {
                    byte[] bArrBytes = response.body().bytes();
                    fileOutputStream.write(bArrBytes);
                    DownloadUtil.access$212(DownloadUtil.this, bArrBytes.length);
                    onDownloadListener.onDownloading((int) (((((double) DownloadUtil.this.mCurrentDownload) * 1.0d) / ((double) DownloadUtil.this.mTotal)) * 100.0d));
                    if (DownloadUtil.this.mCurrentDownload >= DownloadUtil.this.mTotal) {
                        fileOutputStream.flush();
                        fileOutputStream.close();
                        onDownloadListener.onDownloadSuccess(DownloadUtil.this.mfile);
                    } else {
                        DownloadUtil downloadUtil2 = DownloadUtil.this;
                        downloadUtil2.downloadRange(fileOutputStream, downloadUtil2.mCurrentDownload, str, onDownloadListener);
                    }
                } catch (Exception unused) {
                    DownloadUtil downloadUtil3 = DownloadUtil.this;
                    downloadUtil3.downloadRange(fileOutputStream, downloadUtil3.mCurrentDownload, str, onDownloadListener);
                }
            }
        });
    }
}
