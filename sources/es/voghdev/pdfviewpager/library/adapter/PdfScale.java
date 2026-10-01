package es.voghdev.pdfviewpager.library.adapter;

/* JADX INFO: loaded from: classes.dex */
public class PdfScale {
    public static final float DEFAULT_SCALE = 1.0f;
    float scale = 1.0f;
    float centerX = 0.0f;
    float centerY = 0.0f;

    public float getScale() {
        return this.scale;
    }

    public void setScale(float f) {
        this.scale = f;
    }

    public float getCenterX() {
        return this.centerX;
    }

    public void setCenterX(float f) {
        this.centerX = f;
    }

    public float getCenterY() {
        return this.centerY;
    }

    public void setCenterY(float f) {
        this.centerY = f;
    }
}
