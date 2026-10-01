package com.cubeSuite.fragment.smcPad;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadButton extends LinearLayout {
    ImageButton button1;
    ImageButton button2;
    ImageButton button3;
    ImageButton button4;
    ImageButton button5;
    SmcPadButtonCallback callback;

    public SmcPadButton(Context context) {
        super(context);
        initView(context);
    }

    public SmcPadButton(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    public SmcPadButton(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        initView(context);
    }

    public SmcPadButton(Context context, AttributeSet attributeSet, int i, int i2) {
        super(context, attributeSet, i, i2);
        initView(context);
    }

    protected void initView(Context context) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.smc_pad_button, this);
        ImageButton imageButton = (ImageButton) viewInflate.findViewById(R.id.smc_pad_but_1);
        this.button1 = imageButton;
        imageButton.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButton.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButton.this.callback.onButtonClick(0);
            }
        });
        ImageButton imageButton2 = (ImageButton) viewInflate.findViewById(R.id.smc_pad_but_2);
        this.button2 = imageButton2;
        imageButton2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButton.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButton.this.callback.onButtonClick(1);
            }
        });
        ImageButton imageButton3 = (ImageButton) viewInflate.findViewById(R.id.smc_pad_but_3);
        this.button3 = imageButton3;
        imageButton3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButton.3
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButton.this.callback.onButtonClick(2);
            }
        });
        ImageButton imageButton4 = (ImageButton) viewInflate.findViewById(R.id.smc_pad_but_4);
        this.button4 = imageButton4;
        imageButton4.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButton.4
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButton.this.callback.onButtonClick(3);
            }
        });
        ImageButton imageButton5 = (ImageButton) viewInflate.findViewById(R.id.smc_pad_but_5);
        this.button5 = imageButton5;
        imageButton5.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButton.5
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButton.this.callback.onButtonClick(4);
            }
        });
    }
}
