package com.cubeSuite.customControl;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class BackHeadView extends LinearLayout {
    private Button btnEnd;
    private Button btnStart;

    public BackHeadView(Context context) {
        super(context);
    }

    public BackHeadView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        LayoutInflater.from(context).inflate(R.layout.back_head_view, (ViewGroup) this, true);
        TextView textView = (TextView) findViewById(R.id.tvTitle);
        this.btnStart = (Button) findViewById(R.id.btnStart);
        this.btnEnd = (Button) findViewById(R.id.btnEnd);
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.BackHeadView);
        textView.setText(typedArrayObtainStyledAttributes.getString(5));
        this.btnStart.setText(typedArrayObtainStyledAttributes.getString(2));
        this.btnEnd.setText(typedArrayObtainStyledAttributes.getString(0));
        this.btnStart.setVisibility(typedArrayObtainStyledAttributes.getInteger(3, 0));
        this.btnEnd.setVisibility(typedArrayObtainStyledAttributes.getInteger(1, 8));
        textView.setVisibility(typedArrayObtainStyledAttributes.getInteger(4, 0));
        typedArrayObtainStyledAttributes.recycle();
    }

    public void setBtnStartText(String str) {
        this.btnStart.setText(str);
    }

    public void setBtnStartText(int i) {
        this.btnStart.setText(i);
    }

    public void setEndStartText(String str) {
        this.btnEnd.setText(str);
    }

    public void setEndStartText(int i) {
        this.btnEnd.setText(i);
    }

    public void setStartClick(View.OnClickListener onClickListener) {
        if (onClickListener != null) {
            this.btnStart.setOnClickListener(onClickListener);
        }
    }

    public void setEndClick(View.OnClickListener onClickListener) {
        if (onClickListener != null) {
            this.btnEnd.setOnClickListener(onClickListener);
        }
    }
}
