package com.cubeSuite.customControl;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.widget.TextView;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class LoadView extends Dialog {
    public String title;

    public LoadView(Context context) {
        super(context);
        getWindow().setBackgroundDrawableResource(R.color.transparency);
        setCanceledOnTouchOutside(false);
    }

    public LoadView setTitle(String str) {
        this.title = str;
        return this;
    }

    @Override // android.app.Dialog
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.load_view);
        ((TextView) findViewById(R.id.load_title)).setText(this.title);
    }
}
