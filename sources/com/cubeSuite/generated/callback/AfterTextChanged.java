package com.cubeSuite.generated.callback;

import android.text.Editable;
import androidx.databinding.adapters.TextViewBindingAdapter;

/* JADX INFO: loaded from: classes.dex */
public final class AfterTextChanged implements TextViewBindingAdapter.AfterTextChanged {
    final Listener mListener;
    final int mSourceId;

    public interface Listener {
        void _internalCallbackAfterTextChanged(int i, Editable editable);
    }

    public AfterTextChanged(Listener listener, int i) {
        this.mListener = listener;
        this.mSourceId = i;
    }

    @Override // androidx.databinding.adapters.TextViewBindingAdapter.AfterTextChanged
    public void afterTextChanged(Editable editable) {
        this.mListener._internalCallbackAfterTextChanged(this.mSourceId, editable);
    }
}
