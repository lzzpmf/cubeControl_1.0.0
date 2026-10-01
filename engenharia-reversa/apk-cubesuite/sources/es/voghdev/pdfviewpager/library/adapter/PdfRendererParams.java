package es.voghdev.pdfviewpager.library.adapter;

import android.graphics.Bitmap;

/* JADX INFO: loaded from: classes.dex */
public class PdfRendererParams {
    private static final Bitmap.Config DEFAULT_CONFIG = Bitmap.Config.ARGB_8888;
    Bitmap.Config config = DEFAULT_CONFIG;
    int height;
    int offScreenSize;
    float renderQuality;
    int width;

    public int getWidth() {
        return this.width;
    }

    public void setWidth(int i) {
        this.width = i;
    }

    public int getHeight() {
        return this.height;
    }

    public void setHeight(int i) {
        this.height = i;
    }

    public float getRenderQuality() {
        return this.renderQuality;
    }

    public void setRenderQuality(float f) {
        this.renderQuality = f;
    }

    public int getOffScreenSize() {
        return this.offScreenSize;
    }

    public void setOffScreenSize(int i) {
        this.offScreenSize = i;
    }

    public Bitmap.Config getConfig() {
        return this.config;
    }

    public void setConfig(Bitmap.Config config) {
        this.config = config;
    }
}
