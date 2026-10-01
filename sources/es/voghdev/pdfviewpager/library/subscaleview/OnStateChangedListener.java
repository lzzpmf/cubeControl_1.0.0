package es.voghdev.pdfviewpager.library.subscaleview;

import android.graphics.PointF;

/* JADX INFO: loaded from: classes.dex */
public interface OnStateChangedListener {
    void onCenterChanged(PointF pointF, int i);

    void onScaleChanged(float f, int i);
}
