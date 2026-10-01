package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.widget.ScrollView;

/* JADX INFO: loaded from: classes.dex */
public class ScrollViewPlus extends ScrollView {
    private ScrollViewListener scrollViewListener;

    public interface ScrollViewListener {
        void onScrollChanged(ScrollViewPlus scrollViewPlus, int i, int i2, int i3, int i4);
    }

    public ScrollViewPlus(Context context) {
        super(context);
        this.scrollViewListener = null;
    }

    public ScrollViewPlus(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.scrollViewListener = null;
    }

    public ScrollViewPlus(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.scrollViewListener = null;
    }

    public ScrollViewPlus(Context context, AttributeSet attributeSet, int i, int i2) {
        super(context, attributeSet, i, i2);
        this.scrollViewListener = null;
    }

    public void setOnScrollViewListener(ScrollViewListener scrollViewListener) {
        this.scrollViewListener = scrollViewListener;
    }

    @Override // android.view.View
    protected void onScrollChanged(int i, int i2, int i3, int i4) {
        super.onScrollChanged(i, i2, i3, i4);
        ScrollViewListener scrollViewListener = this.scrollViewListener;
        if (scrollViewListener != null) {
            scrollViewListener.onScrollChanged(this, i, i2, i3, i4);
        }
    }
}
