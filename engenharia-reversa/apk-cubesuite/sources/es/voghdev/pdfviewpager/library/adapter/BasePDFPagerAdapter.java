package es.voghdev.pdfviewpager.library.adapter;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.pdf.PdfRenderer;
import android.net.Uri;
import android.os.ParcelFileDescriptor;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import androidx.viewpager.widget.PagerAdapter;
import es.voghdev.pdfviewpager.library.R;
import java.io.File;
import java.io.IOException;
import java.net.URI;

/* JADX INFO: loaded from: classes.dex */
public class BasePDFPagerAdapter extends PagerAdapter {
    protected static final int DEFAULT_OFFSCREENSIZE = 1;
    protected static final float DEFAULT_QUALITY = 2.0f;
    protected static final int FIRST_PAGE = 0;
    protected BitmapContainer bitmapContainer;
    protected Context context;
    protected LayoutInflater inflater;
    protected int offScreenSize;
    protected String pdfPath;
    protected float renderQuality;
    protected PdfRenderer renderer;

    @Override // androidx.viewpager.widget.PagerAdapter
    public void destroyItem(ViewGroup viewGroup, int i, Object obj) {
    }

    public BasePDFPagerAdapter(Context context, String str) {
        this.pdfPath = str;
        this.context = context;
        this.renderQuality = DEFAULT_QUALITY;
        this.offScreenSize = 1;
        init();
    }

    public BasePDFPagerAdapter(Context context, String str, int i) {
        this.pdfPath = str;
        this.context = context;
        this.renderQuality = DEFAULT_QUALITY;
        this.offScreenSize = i;
        init();
    }

    protected void init() {
        try {
            this.renderer = new PdfRenderer(getSeekableFileDescriptor(this.pdfPath));
            this.inflater = (LayoutInflater) this.context.getSystemService("layout_inflater");
            this.bitmapContainer = new SimpleBitmapPool(extractPdfParamsFromFirstPage(this.renderer, this.renderQuality));
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    protected PdfRendererParams extractPdfParamsFromFirstPage(PdfRenderer pdfRenderer, float f) {
        PdfRenderer.Page pDFPage = getPDFPage(pdfRenderer, 0);
        PdfRendererParams pdfRendererParams = new PdfRendererParams();
        pdfRendererParams.setRenderQuality(f);
        pdfRendererParams.setOffScreenSize(this.offScreenSize);
        pdfRendererParams.setWidth((int) (pDFPage.getWidth() * f));
        pdfRendererParams.setHeight((int) (pDFPage.getHeight() * f));
        pDFPage.close();
        return pdfRendererParams;
    }

    protected ParcelFileDescriptor getSeekableFileDescriptor(String str) throws IOException {
        File file = new File(str);
        if (file.exists()) {
            return ParcelFileDescriptor.open(file, 268435456);
        }
        if (isAnAsset(str)) {
            return ParcelFileDescriptor.open(new File(this.context.getCacheDir(), str), 268435456);
        }
        return this.context.getContentResolver().openFileDescriptor(Uri.parse(URI.create(String.format("file://%s", str)).toString()), "rw");
    }

    protected boolean isAnAsset(String str) {
        return !str.startsWith("/");
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public Object instantiateItem(ViewGroup viewGroup, int i) {
        View viewInflate = this.inflater.inflate(R.layout.view_pdf_page, viewGroup, false);
        ImageView imageView = (ImageView) viewInflate.findViewById(R.id.imageView);
        if (this.renderer != null && getCount() >= i) {
            PdfRenderer.Page pDFPage = getPDFPage(this.renderer, i);
            Bitmap bitmap = this.bitmapContainer.get(i);
            pDFPage.render(bitmap, null, null, 1);
            pDFPage.close();
            imageView.setImageBitmap(bitmap);
            viewGroup.addView(viewInflate, 0);
        }
        return viewInflate;
    }

    protected PdfRenderer.Page getPDFPage(PdfRenderer pdfRenderer, int i) {
        return pdfRenderer.openPage(i);
    }

    public void close() {
        releaseAllBitmaps();
        PdfRenderer pdfRenderer = this.renderer;
        if (pdfRenderer != null) {
            pdfRenderer.close();
        }
    }

    protected void releaseAllBitmaps() {
        BitmapContainer bitmapContainer = this.bitmapContainer;
        if (bitmapContainer != null) {
            bitmapContainer.clear();
        }
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public int getCount() {
        PdfRenderer pdfRenderer = this.renderer;
        if (pdfRenderer != null) {
            return pdfRenderer.getPageCount();
        }
        return 0;
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public boolean isViewFromObject(View view, Object obj) {
        return view == ((View) obj);
    }
}
