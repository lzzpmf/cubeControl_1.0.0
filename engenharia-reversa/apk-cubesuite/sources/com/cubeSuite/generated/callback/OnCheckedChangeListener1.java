package com.cubeSuite.generated.callback;

import android.widget.CompoundButton;

/* JADX INFO: loaded from: classes.dex */
public final class OnCheckedChangeListener1 implements CompoundButton.OnCheckedChangeListener {
    final Listener mListener;
    final int mSourceId;

    public interface Listener {
        void _internalCallbackOnCheckedChanged1(int i, CompoundButton compoundButton, boolean z);
    }

    public OnCheckedChangeListener1(Listener listener, int i) {
        this.mListener = listener;
        this.mSourceId = i;
    }

    @Override // android.widget.CompoundButton.OnCheckedChangeListener
    public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
        this.mListener._internalCallbackOnCheckedChanged1(this.mSourceId, compoundButton, z);
    }
}
