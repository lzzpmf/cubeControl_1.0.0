package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class ColorPicker extends LinearLayout {
    public View colorPicker;

    public ColorPicker(Context context) {
        super(context);
        initView(context);
    }

    public ColorPicker(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    public ColorPicker(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        initView(context);
    }

    void initView(Context context) {
        this.colorPicker = LayoutInflater.from(context).inflate(R.layout.color_picker, (ViewGroup) this, true).findViewById(R.id.color_picker);
    }
}
