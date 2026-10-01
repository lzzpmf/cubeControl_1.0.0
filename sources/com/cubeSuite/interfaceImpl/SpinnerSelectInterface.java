package com.cubeSuite.interfaceImpl;

import android.view.View;
import android.widget.AdapterView;
import com.cubeSuite.callback.Callback;

/* JADX INFO: loaded from: classes.dex */
public class SpinnerSelectInterface implements AdapterView.OnItemSelectedListener {
    private Callback.SpinnerCallback callback;

    @Override // android.widget.AdapterView.OnItemSelectedListener
    public void onNothingSelected(AdapterView<?> adapterView) {
    }

    public SpinnerSelectInterface(Callback.SpinnerCallback spinnerCallback) {
        this.callback = spinnerCallback;
    }

    @Override // android.widget.AdapterView.OnItemSelectedListener
    public void onItemSelected(AdapterView<?> adapterView, View view, int i, long j) {
        this.callback.select(i);
    }
}
