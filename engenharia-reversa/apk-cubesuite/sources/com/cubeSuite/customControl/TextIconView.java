package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Typeface;
import android.util.AttributeSet;
import androidx.appcompat.widget.AppCompatTextView;

/* JADX INFO: loaded from: classes.dex */
public class TextIconView extends AppCompatTextView {
    public TextIconView(Context context) {
        super(context);
    }

    public TextIconView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    private void initView(Context context) {
        setTypeface(Typeface.createFromAsset(context.getAssets(), "fontawesome-webfont.ttf"));
    }
}
