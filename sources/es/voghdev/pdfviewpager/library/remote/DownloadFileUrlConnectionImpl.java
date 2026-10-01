package es.voghdev.pdfviewpager.library.remote;

import android.content.Context;
import android.os.Handler;
import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

/* JADX INFO: loaded from: classes.dex */
public class DownloadFileUrlConnectionImpl implements DownloadFile {
    private static final int BUFFER_LEN = 1024;
    private static final int KILOBYTE = 1024;
    private static final int NOTIFY_PERIOD = 153600;
    Context context;
    DownloadFile.Listener listener;
    Handler uiThread;

    public DownloadFileUrlConnectionImpl(Context context, Handler handler, DownloadFile.Listener listener) {
        new NullListener();
        this.context = context;
        this.uiThread = handler;
        this.listener = listener;
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile
    public void download(final String str, final String str2) {
        new Thread(new Runnable() { // from class: es.voghdev.pdfviewpager.library.remote.DownloadFileUrlConnectionImpl.1
            @Override // java.lang.Runnable
            public void run() {
                try {
                    FileOutputStream fileOutputStream = new FileOutputStream(new File(str2));
                    HttpURLConnection httpURLConnection = (HttpURLConnection) new URL(str).openConnection();
                    int contentLength = httpURLConnection.getContentLength();
                    byte[] bArr = new byte[1024];
                    BufferedInputStream bufferedInputStream = new BufferedInputStream(httpURLConnection.getInputStream());
                    int i = 0;
                    int i2 = 0;
                    while (true) {
                        int i3 = bufferedInputStream.read(bArr);
                        if (i3 > 0) {
                            fileOutputStream.write(bArr, 0, i3);
                            i += i3;
                            i2 += i3;
                            if (DownloadFileUrlConnectionImpl.this.listener != null && i2 > DownloadFileUrlConnectionImpl.NOTIFY_PERIOD) {
                                DownloadFileUrlConnectionImpl.this.notifyProgressOnUiThread(i, contentLength);
                                i2 = 0;
                            }
                        } else {
                            httpURLConnection.disconnect();
                            fileOutputStream.close();
                            DownloadFileUrlConnectionImpl.this.notifySuccessOnUiThread(str, str2);
                            return;
                        }
                    }
                } catch (MalformedURLException e) {
                    DownloadFileUrlConnectionImpl.this.notifyFailureOnUiThread(e);
                } catch (IOException e2) {
                    DownloadFileUrlConnectionImpl.this.notifyFailureOnUiThread(e2);
                }
            }
        }).start();
    }

    protected void notifySuccessOnUiThread(final String str, final String str2) {
        Handler handler = this.uiThread;
        if (handler == null) {
            return;
        }
        handler.post(new Runnable() { // from class: es.voghdev.pdfviewpager.library.remote.DownloadFileUrlConnectionImpl.2
            @Override // java.lang.Runnable
            public void run() {
                DownloadFileUrlConnectionImpl.this.listener.onSuccess(str, str2);
            }
        });
    }

    protected void notifyFailureOnUiThread(final Exception exc) {
        Handler handler = this.uiThread;
        if (handler == null) {
            return;
        }
        handler.post(new Runnable() { // from class: es.voghdev.pdfviewpager.library.remote.DownloadFileUrlConnectionImpl.3
            @Override // java.lang.Runnable
            public void run() {
                DownloadFileUrlConnectionImpl.this.listener.onFailure(exc);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void notifyProgressOnUiThread(final int i, final int i2) {
        Handler handler = this.uiThread;
        if (handler == null) {
            return;
        }
        handler.post(new Runnable() { // from class: es.voghdev.pdfviewpager.library.remote.DownloadFileUrlConnectionImpl.4
            @Override // java.lang.Runnable
            public void run() {
                DownloadFileUrlConnectionImpl.this.listener.onProgressUpdate(i, i2);
            }
        });
    }

    protected class NullListener implements DownloadFile.Listener {
        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onFailure(Exception exc) {
        }

        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onProgressUpdate(int i, int i2) {
        }

        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onSuccess(String str, String str2) {
        }

        protected NullListener() {
        }
    }
}
