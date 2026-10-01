package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Typeface;
import android.util.AttributeSet;
import androidx.appcompat.widget.AppCompatButton;

/* JADX INFO: loaded from: classes.dex */
public class ButtonIconView extends AppCompatButton {
    public ButtonIconView(Context context) {
        super(context);
    }

    public ButtonIconView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    private void initView(Context context) {
        setTypeface(Typeface.createFromAsset(context.getAssets(), "fontawesome-webfont.ttf"));
    }
}
