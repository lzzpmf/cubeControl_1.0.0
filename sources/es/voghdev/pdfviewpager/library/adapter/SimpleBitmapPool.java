package es.voghdev.pdfviewpager.library.adapter;

import android.graphics.Bitmap;

/* JADX INFO: loaded from: classes.dex */
public class SimpleBitmapPool implements BitmapContainer {
    Bitmap[] bitmaps;
    private Bitmap.Config config;
    private int height;
    private int poolSize;
    private int width;

    private int getPoolSize(int i) {
        return (i * 2) + 1;
    }

    public SimpleBitmapPool(PdfRendererParams pdfRendererParams) {
        this.poolSize = getPoolSize(pdfRendererParams.getOffScreenSize());
        this.width = pdfRendererParams.getWidth();
        this.height = pdfRendererParams.getHeight();
        this.config = pdfRendererParams.getConfig();
        this.bitmaps = new Bitmap[this.poolSize];
    }

    public Bitmap getBitmap(int i) {
        int indexFromPosition = getIndexFromPosition(i);
        if (this.bitmaps[indexFromPosition] == null) {
            createBitmapAtIndex(indexFromPosition);
        }
        this.bitmaps[indexFromPosition].eraseColor(0);
        return this.bitmaps[indexFromPosition];
    }

    @Override // es.voghdev.pdfviewpager.library.adapter.BitmapContainer
    public Bitmap get(int i) {
        return getBitmap(i);
    }

    @Override // es.voghdev.pdfviewpager.library.adapter.BitmapContainer
    public void remove(int i) {
        this.bitmaps[i].recycle();
        this.bitmaps[i] = null;
    }

    @Override // es.voghdev.pdfviewpager.library.adapter.BitmapContainer
    public void clear() {
        recycleAll();
    }

    protected void recycleAll() {
        for (int i = 0; i < this.poolSize; i++) {
            Bitmap bitmap = this.bitmaps[i];
            if (bitmap != null) {
                bitmap.recycle();
                this.bitmaps[i] = null;
            }
        }
    }

    protected void createBitmapAtIndex(int i) {
        this.bitmaps[i] = Bitmap.createBitmap(this.width, this.height, this.config);
    }

    protected int getIndexFromPosition(int i) {
        return i % this.poolSize;
    }
}
