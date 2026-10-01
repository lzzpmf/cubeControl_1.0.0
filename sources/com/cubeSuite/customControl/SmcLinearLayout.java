package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.widget.LinearLayout;

/* JADX INFO: loaded from: classes.dex */
public class SmcLinearLayout extends LinearLayout {
    private boolean mChildViewsCanTouch;

    public SmcLinearLayout(Context context) {
        super(context);
        this.mChildViewsCanTouch = false;
    }

    public SmcLinearLayout(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mChildViewsCanTouch = false;
    }

    public SmcLinearLayout(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mChildViewsCanTouch = false;
    }

    public void setChildViewsCanTouch(boolean z) {
        this.mChildViewsCanTouch = z;
    }

    @Override // android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        return !this.mChildViewsCanTouch || super.onInterceptTouchEvent(motionEvent);
    }
}
