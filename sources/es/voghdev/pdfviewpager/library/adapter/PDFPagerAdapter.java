package es.voghdev.pdfviewpager.library.adapter;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.pdf.PdfRenderer;
import android.view.View;
import android.view.ViewGroup;
import es.voghdev.pdfviewpager.library.R;
import es.voghdev.pdfviewpager.library.subscaleview.ImageSource;
import es.voghdev.pdfviewpager.library.subscaleview.SubsamplingScaleImageView;
import es.voghdev.pdfviewpager.library.util.EmptyClickListener;

/* JADX INFO: loaded from: classes.dex */
public class PDFPagerAdapter extends BasePDFPagerAdapter {
    private static final float DEFAULT_SCALE = 1.0f;
    View.OnClickListener pageClickListener;
    PdfScale scale;

    public PDFPagerAdapter(Context context, String str) {
        super(context, str);
        this.scale = new PdfScale();
        this.pageClickListener = new EmptyClickListener();
    }

    @Override // es.voghdev.pdfviewpager.library.adapter.BasePDFPagerAdapter, androidx.viewpager.widget.PagerAdapter
    public Object instantiateItem(ViewGroup viewGroup, int i) {
        View viewInflate = this.inflater.inflate(R.layout.view_pdf_page, viewGroup, false);
        SubsamplingScaleImageView subsamplingScaleImageView = (SubsamplingScaleImageView) viewInflate.findViewById(R.id.subsamplingImageView);
        if (this.renderer != null && getCount() >= i) {
            PdfRenderer.Page pDFPage = getPDFPage(this.renderer, i);
            Bitmap bitmap = this.bitmapContainer.get(i);
            subsamplingScaleImageView.setImage(ImageSource.bitmap(bitmap));
            subsamplingScaleImageView.setOnClickListener(new View.OnClickListener() { // from class: es.voghdev.pdfviewpager.library.adapter.PDFPagerAdapter.1
                @Override // android.view.View.OnClickListener
                public void onClick(View view) {
                    PDFPagerAdapter.this.pageClickListener.onClick(view);
                }
            });
            pDFPage.render(bitmap, null, null, 1);
            pDFPage.close();
            viewGroup.addView(viewInflate, 0);
        }
        return viewInflate;
    }

    @Override // es.voghdev.pdfviewpager.library.adapter.BasePDFPagerAdapter
    public void close() {
        super.close();
    }

    public static class Builder {
        Context context;
        String pdfPath = "";
        float scale = 1.0f;
        float centerX = 0.0f;
        float centerY = 0.0f;
        int offScreenSize = 1;
        float renderQuality = 2.0f;
        View.OnClickListener pageClickListener = new EmptyClickListener();

        public Builder(Context context) {
            this.context = context;
        }

        public Builder setScale(float f) {
            this.scale = f;
            return this;
        }

        public Builder setScale(PdfScale pdfScale) {
            this.scale = pdfScale.getScale();
            this.centerX = pdfScale.getCenterX();
            this.centerY = pdfScale.getCenterY();
            return this;
        }

        public Builder setCenterX(float f) {
            this.centerX = f;
            return this;
        }

        public Builder setCenterY(float f) {
            this.centerY = f;
            return this;
        }

        public Builder setRenderQuality(float f) {
            this.renderQuality = f;
            return this;
        }

        public Builder setOffScreenSize(int i) {
            this.offScreenSize = i;
            return this;
        }

        public Builder setPdfPath(String str) {
            this.pdfPath = str;
            return this;
        }

        public Builder setOnPageClickListener(View.OnClickListener onClickListener) {
            if (onClickListener != null) {
                this.pageClickListener = onClickListener;
            }
            return this;
        }

        public PDFPagerAdapter create() {
            PDFPagerAdapter pDFPagerAdapter = new PDFPagerAdapter(this.context, this.pdfPath);
            pDFPagerAdapter.scale.setScale(this.scale);
            pDFPagerAdapter.scale.setCenterX(this.centerX);
            pDFPagerAdapter.scale.setCenterY(this.centerY);
            pDFPagerAdapter.offScreenSize = this.offScreenSize;
            pDFPagerAdapter.renderQuality = this.renderQuality;
            pDFPagerAdapter.pageClickListener = this.pageClickListener;
            return pDFPagerAdapter;
        }
    }
}
