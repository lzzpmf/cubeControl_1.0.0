package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class NumKeyPadView extends LinearLayout {
    private Button close;
    private Button delete;
    private boolean flag;
    KeypadListener keypadListener;
    private int max;
    private int min;
    private Button[] nums;
    private int offset;
    private int value;

    public interface KeypadListener {
        void numChange(int i);
    }

    public NumKeyPadView(Context context) {
        super(context);
        this.nums = new Button[10];
        this.value = 0;
        this.flag = false;
        this.offset = 0;
        setVisibility(8);
    }

    public void close() {
        setVisibility(8);
    }

    public void setValue(int i) {
        this.flag = true;
        this.value = i;
    }

    public void setValue(String str) {
        this.flag = true;
        if (str.matches("\\d+") && str.length() < 10) {
            this.value = Integer.parseInt(str);
        } else {
            this.value = 0;
        }
    }

    public void show() {
        setVisibility(0);
    }

    public void setOffset(int i) {
        this.offset = i;
    }

    public NumKeyPadView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.nums = new Button[10];
        this.value = 0;
        this.flag = false;
        this.offset = 0;
        LayoutInflater.from(context).inflate(R.layout.num_key_pad, (ViewGroup) this, true);
        this.nums[0] = (Button) findViewById(R.id.zero);
        this.nums[1] = (Button) findViewById(R.id.one);
        this.nums[2] = (Button) findViewById(R.id.two);
        this.nums[3] = (Button) findViewById(R.id.three);
        this.nums[4] = (Button) findViewById(R.id.four);
        this.nums[5] = (Button) findViewById(R.id.five);
        this.nums[6] = (Button) findViewById(R.id.six);
        this.nums[7] = (Button) findViewById(R.id.seven);
        this.nums[8] = (Button) findViewById(R.id.eight);
        this.nums[9] = (Button) findViewById(R.id.nine);
        this.delete = (Button) findViewById(R.id.delete);
        this.close = (Button) findViewById(R.id.close);
        setItemClick();
    }

    public void setItemClick() {
        this.close.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.NumKeyPadView$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m102lambda$setItemClick$0$comcubeSuitecustomControlNumKeyPadView(view);
            }
        });
        this.delete.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.NumKeyPadView$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m103lambda$setItemClick$1$comcubeSuitecustomControlNumKeyPadView(view);
            }
        });
        final int i = 0;
        while (true) {
            Button[] buttonArr = this.nums;
            if (i >= buttonArr.length) {
                return;
            }
            buttonArr[i].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.NumKeyPadView$$ExternalSyntheticLambda2
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m104lambda$setItemClick$2$comcubeSuitecustomControlNumKeyPadView(i, view);
                }
            });
            i++;
        }
    }

    /* JADX INFO: renamed from: lambda$setItemClick$0$com-cubeSuite-customControl-NumKeyPadView, reason: not valid java name */
    /* synthetic */ void m102lambda$setItemClick$0$comcubeSuitecustomControlNumKeyPadView(View view) {
        setVisibility(8);
    }

    /* JADX INFO: renamed from: lambda$setItemClick$1$com-cubeSuite-customControl-NumKeyPadView, reason: not valid java name */
    /* synthetic */ void m103lambda$setItemClick$1$comcubeSuitecustomControlNumKeyPadView(View view) {
        this.flag = false;
        String strValueOf = String.valueOf(this.value);
        String strSubstring = strValueOf.substring(0, strValueOf.length() - 1);
        int iMin = strSubstring.equals("") ? 0 : Math.min(this.max, Math.max(this.min, Integer.parseInt(strSubstring)));
        this.value = iMin;
        this.keypadListener.numChange(iMin + this.offset);
    }

    /* JADX INFO: renamed from: lambda$setItemClick$2$com-cubeSuite-customControl-NumKeyPadView, reason: not valid java name */
    /* synthetic */ void m104lambda$setItemClick$2$comcubeSuitecustomControlNumKeyPadView(int i, View view) {
        if (this.flag) {
            this.value = 0;
            this.flag = false;
        }
        int iMin = Math.min(this.max, Math.max(Integer.parseInt(String.valueOf(this.value) + i), this.min));
        this.value = iMin;
        KeypadListener keypadListener = this.keypadListener;
        if (keypadListener != null) {
            keypadListener.numChange(iMin + this.offset);
        }
    }

    public void setKeyPadListener(KeypadListener keypadListener) {
        this.keypadListener = keypadListener;
    }

    public void setScope(int i, int i2) {
        this.min = i;
        this.max = i2;
    }
}
