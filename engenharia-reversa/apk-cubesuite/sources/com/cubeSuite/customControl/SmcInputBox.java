package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class SmcInputBox extends LinearLayout {
    public EditText editTex;
    public Button sender;

    public SmcInputBox(Context context) {
        super(context);
        initView(context);
    }

    public SmcInputBox(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    public SmcInputBox(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        initView(context);
    }

    void initView(Context context) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.smc_input_box, (ViewGroup) this, true);
        this.editTex = (EditText) viewInflate.findViewById(R.id.smc_input);
        this.sender = (Button) viewInflate.findViewById(R.id.smc_sender);
    }
}
