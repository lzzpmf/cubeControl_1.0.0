package es.voghdev.pdfviewpager.library.asset;

import android.content.Context;
import android.os.Handler;
import es.voghdev.pdfviewpager.library.util.FileUtil;
import java.io.IOException;

/* JADX INFO: loaded from: classes.dex */
public class CopyAssetThreadImpl implements CopyAsset {
    Context context;
    CopyAsset.Listener listener;
    Handler uiThread;

    public CopyAssetThreadImpl(Context context, Handler handler, CopyAsset.Listener listener) {
        this.listener = new NullListener();
        this.context = context;
        this.uiThread = handler;
        if (listener != null) {
            this.listener = listener;
        }
    }

    public CopyAssetThreadImpl(Context context, Handler handler) {
        this.listener = new NullListener();
        this.context = context;
        this.uiThread = handler;
    }

    @Override // es.voghdev.pdfviewpager.library.asset.CopyAsset
    public void copy(final String str, final String str2) {
        new Thread(new Runnable() { // from class: es.voghdev.pdfviewpager.library.asset.CopyAssetThreadImpl.1
            @Override // java.lang.Runnable
            public void run() {
                try {
                    FileUtil.copyAsset(CopyAssetThreadImpl.this.context, str, str2);
                    CopyAssetThreadImpl.this.notifySuccess(str, str2);
                } catch (IOException e) {
                    CopyAssetThreadImpl.this.notifyError(e);
                }
            }
        }).start();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void notifySuccess(final String str, final String str2) {
        Handler handler = this.uiThread;
        if (handler == null) {
            return;
        }
        handler.post(new Runnable() { // from class: es.voghdev.pdfviewpager.library.asset.CopyAssetThreadImpl.2
            @Override // java.lang.Runnable
            public void run() {
                CopyAssetThreadImpl.this.listener.success(str, str2);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void notifyError(final IOException iOException) {
        Handler handler = this.uiThread;
        if (handler == null) {
            return;
        }
        handler.post(new Runnable() { // from class: es.voghdev.pdfviewpager.library.asset.CopyAssetThreadImpl.3
            @Override // java.lang.Runnable
            public void run() {
                CopyAssetThreadImpl.this.listener.failure(iOException);
            }
        });
    }

    protected class NullListener implements CopyAsset.Listener {
        @Override // es.voghdev.pdfviewpager.library.asset.CopyAsset.Listener
        public void failure(Exception exc) {
        }

        @Override // es.voghdev.pdfviewpager.library.asset.CopyAsset.Listener
        public void success(String str, String str2) {
        }

        protected NullListener() {
        }
    }
}
