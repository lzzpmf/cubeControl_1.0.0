package es.voghdev.pdfviewpager.library.adapter;

import android.graphics.Bitmap;

/* JADX INFO: loaded from: classes.dex */
public interface BitmapContainer {
    void clear();

    Bitmap get(int i);

    void remove(int i);
}
