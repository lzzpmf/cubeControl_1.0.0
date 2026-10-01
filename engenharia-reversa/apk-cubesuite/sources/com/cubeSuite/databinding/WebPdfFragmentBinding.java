package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import es.voghdev.pdfviewpager.library.PDFViewPager;

/* JADX INFO: loaded from: classes.dex */
public final class WebPdfFragmentBinding implements ViewBinding {
    public final PDFViewPager pdfViewPager;
    public final ProgressBar progressbar;
    public final RelativeLayout remotePdfRoot;
    private final FrameLayout rootView;

    private WebPdfFragmentBinding(FrameLayout frameLayout, PDFViewPager pDFViewPager, ProgressBar progressBar, RelativeLayout relativeLayout) {
        this.rootView = frameLayout;
        this.pdfViewPager = pDFViewPager;
        this.progressbar = progressBar;
        this.remotePdfRoot = relativeLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public FrameLayout getRoot() {
        return this.rootView;
    }

    public static WebPdfFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static WebPdfFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.web_pdf_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static WebPdfFragmentBinding bind(View view) {
        int i = R.id.pdfViewPager;
        PDFViewPager pDFViewPager = (PDFViewPager) ViewBindings.findChildViewById(view, R.id.pdfViewPager);
        if (pDFViewPager != null) {
            i = R.id.progressbar;
            ProgressBar progressBar = (ProgressBar) ViewBindings.findChildViewById(view, R.id.progressbar);
            if (progressBar != null) {
                i = R.id.remote_pdf_root;
                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, R.id.remote_pdf_root);
                if (relativeLayout != null) {
                    return new WebPdfFragmentBinding((FrameLayout) view, pDFViewPager, progressBar, relativeLayout);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
