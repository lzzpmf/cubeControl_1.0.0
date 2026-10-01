package com.cubeSuite.generated.callback;

import android.view.View;
import android.widget.AdapterView;
import androidx.databinding.adapters.AdapterViewBindingAdapter;

/* JADX INFO: loaded from: classes.dex */
public final class OnItemSelected implements AdapterViewBindingAdapter.OnItemSelected {
    final Listener mListener;
    final int mSourceId;

    public interface Listener {
        void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j);
    }

    public OnItemSelected(Listener listener, int i) {
        this.mListener = listener;
        this.mSourceId = i;
    }

    @Override // androidx.databinding.adapters.AdapterViewBindingAdapter.OnItemSelected
    public void onItemSelected(AdapterView adapterView, View view, int i, long j) {
        this.mListener._internalCallbackOnItemSelected(this.mSourceId, adapterView, view, i, j);
    }
}
