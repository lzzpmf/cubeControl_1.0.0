package com.cubeSuite.fragment;

import android.app.Activity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.bluetooth.BleHandle;
import es.voghdev.pdfviewpager.library.RemotePDFViewPager;
import es.voghdev.pdfviewpager.library.adapter.PDFPagerAdapter;
import es.voghdev.pdfviewpager.library.remote.DownloadFile;
import es.voghdev.pdfviewpager.library.util.FileUtil;

/* JADX INFO: loaded from: classes.dex */
public class WebPdfFragment extends Fragment implements DownloadFile.Listener {
    private Activity activity;
    private PDFPagerAdapter adapter;
    private String mUrl;
    private ProgressBar pb_bar;
    private RelativeLayout pdf_root;
    private RemotePDFViewPager remotePDFViewPager;
    private View thisView;

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onProgressUpdate(int i, int i2) {
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        this.thisView = layoutInflater.inflate(R.layout.web_pdf_fragment, viewGroup, false);
        super.onCreate(bundle);
        initView();
        this.activity = getActivity();
        setDownloadListener();
        return this.thisView;
    }

    public WebPdfFragment() {
        this.mUrl = "";
        this.mUrl = BleHandle.connectDeviceInfo.getPdfUrl();
    }

    protected void initView() {
        this.pdf_root = (RelativeLayout) this.thisView.findViewById(R.id.remote_pdf_root);
        this.pb_bar = (ProgressBar) this.thisView.findViewById(R.id.progressbar);
    }

    protected void setDownloadListener() {
        RemotePDFViewPager remotePDFViewPager = new RemotePDFViewPager(this.activity, this.mUrl, this);
        this.remotePDFViewPager = remotePDFViewPager;
        remotePDFViewPager.setId(R.id.pdfViewPager);
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onSuccess(String str, String str2) {
        this.pb_bar.setVisibility(8);
        PDFPagerAdapter pDFPagerAdapter = new PDFPagerAdapter(this.activity, FileUtil.extractFileNameFromURL(str));
        this.adapter = pDFPagerAdapter;
        this.remotePDFViewPager.setAdapter(pDFPagerAdapter);
        updateLayout();
    }

    private void updateLayout() {
        this.pdf_root.removeAllViewsInLayout();
        this.pdf_root.addView(this.remotePDFViewPager, -1, -2);
    }

    @Override // es.voghdev.pdfviewpager.library.remote.DownloadFile.Listener
    public void onFailure(Exception exc) {
        this.pb_bar.setVisibility(8);
    }
}
