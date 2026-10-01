package com.cubeSuite.generated.callback;

import android.widget.SeekBar;
import androidx.databinding.adapters.SeekBarBindingAdapter;

/* JADX INFO: loaded from: classes.dex */
public final class OnProgressChanged implements SeekBarBindingAdapter.OnProgressChanged {
    final Listener mListener;
    final int mSourceId;

    public interface Listener {
        void _internalCallbackOnProgressChanged(int i, SeekBar seekBar, int i2, boolean z);
    }

    public OnProgressChanged(Listener listener, int i) {
        this.mListener = listener;
        this.mSourceId = i;
    }

    @Override // androidx.databinding.adapters.SeekBarBindingAdapter.OnProgressChanged
    public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
        this.mListener._internalCallbackOnProgressChanged(this.mSourceId, seekBar, i, z);
    }
}
