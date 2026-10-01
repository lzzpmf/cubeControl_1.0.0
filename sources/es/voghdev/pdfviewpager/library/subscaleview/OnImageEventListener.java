package es.voghdev.pdfviewpager.library.subscaleview;

/* JADX INFO: loaded from: classes.dex */
public interface OnImageEventListener {
    void onImageLoadError(Exception exc);

    void onImageLoaded();

    void onPreviewLoadError(Exception exc);

    void onPreviewReleased();

    void onReady();

    void onTileLoadError(Exception exc);
}
