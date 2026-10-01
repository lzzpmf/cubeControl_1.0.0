package es.voghdev.pdfviewpager.library;

import android.content.Context;
import android.content.res.TypedArray;
import android.os.Handler;
import android.util.AttributeSet;
import android.view.MotionEvent;
import androidx.viewpager.widget.ViewPager;
import es.voghdev.pdfviewpager.library.remote.DownloadFile;
import es.voghdev.pdfviewpager.library.remote.DownloadFileUrlConnectionImpl;
import es.voghdev.pdfviewpager.library.util.FileUtil;
import java.io.File;

/* JADX INFO: loaded from: classes.dex */
public class RemotePDFViewPager extends ViewPager implements DownloadFile.Listener {
    protected Context context;
    protected DownloadFile downloadFile;
    protected DownloadFile.Listener listener;

    public RemotePDFViewPager(Context context, String str, DownloadFile.Listener listener) {
        super(context);
        this.context = context;
        this.listener = listener;
        init(new DownloadFileUrlConnectionImpl(context, new Handler(), this), str);
    }

    public RemotePDFViewPager(Context context, DownloadFile downloadFile, String str, DownloadFile.Listener listener) {
        super(context);
        this.context = context;
        this.listener = listener;
        init(downloadFile, str);
    }

    public RemotePDFViewPager(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.context = context;
        init(attributeSet);
    }

    private void init(DownloadFile downloadFile, String str) {
        setDownloader(downloadFile);
        downloadFile.download(str, new File(this.context.getCacheDir(), FileUtil.extractFileNameFromURL(str)).getAbsolutePath());
    }

    private void init(AttributeSet attributeSet) {
        if (attributeSet != null) {
            TypedArray typedArrayObtainStyledAttributes = this.context.obtainStyledAttributes(attributeSet, R.styleable.PDFViewPager);
            String string = typedArrayObtainStyledAttributes.getString(R.styleable.PDFViewPager_pdfUrl);
            if (string != null && string.length() > 0) {
                init(new DownloadFileUrlConnectionImpl(this.context, new Handler(), this), string);
            }
            typedArrayObtainStyledAttributes.recycle();
        }
    }

    public void setDownloader(DownloadFile downloadFile) {
        this.downloadFile = downloadFile;
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onSuccess(String str, String str2) {
        this.listener.onSuccess(str, str2);
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onFailure(Exception exc) {
        this.listener.onFailure(exc);
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onProgressUpdate(int i, int i2) {
        this.listener.onProgressUpdate(i, i2);
    }

    @Override // androidx.viewpager.widget.ViewPager, android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        try {
            return super.onInterceptTouchEvent(motionEvent);
        } catch (IllegalArgumentException e) {
            e.printStackTrace();
            return false;
        }
    }

    public class NullListener implements DownloadFile.Listener {
        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onFailure(Exception exc) {
        }

        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onProgressUpdate(int i, int i2) {
        }

        @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
        public void onSuccess(String str, String str2) {
        }

        public NullListener() {
        }
    }
}
