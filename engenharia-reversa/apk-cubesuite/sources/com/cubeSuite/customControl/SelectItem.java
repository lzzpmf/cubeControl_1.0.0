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
public class SelectItem extends LinearLayout {
    private Button btnInfo;
    private LinearLayout clickContent;
    private String modeText;
    private TextView tvInfo;
    private TextView tvTitle;

    public String getModeText() {
        return this.modeText;
    }

    public SelectItem(Context context) {
        super(context);
    }

    public SelectItem(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        LayoutInflater.from(context).inflate(R.layout.select_item_layout, (ViewGroup) this, true);
        this.clickContent = (LinearLayout) findViewById(R.id.clickContent);
        this.tvTitle = (TextView) findViewById(R.id.tvTitle);
        this.btnInfo = (Button) findViewById(R.id.btnInfo);
        this.tvInfo = (TextView) findViewById(R.id.tvInfo);
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.SelectItem);
        String string = typedArrayObtainStyledAttributes.getString(1);
        this.modeText = string;
        this.tvTitle.setText(string);
        this.tvInfo.setText(typedArrayObtainStyledAttributes.getString(0));
        typedArrayObtainStyledAttributes.recycle();
        this.btnInfo.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SelectItem$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m106lambda$new$0$comcubeSuitecustomControlSelectItem(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-customControl-SelectItem, reason: not valid java name */
    /* synthetic */ void m106lambda$new$0$comcubeSuitecustomControlSelectItem(View view) {
        TextView textView = this.tvInfo;
        textView.setVisibility(textView.getVisibility() == 8 ? 0 : 8);
    }

    public void setItemClick(View.OnClickListener onClickListener) {
        if (onClickListener != null) {
            this.clickContent.setOnClickListener(onClickListener);
        }
    }
}
