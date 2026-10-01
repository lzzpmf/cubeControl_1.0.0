package com.cubeSuite.generated.callback;

import android.widget.RadioGroup;

/* JADX INFO: loaded from: classes.dex */
public final class OnCheckedChangeListener implements RadioGroup.OnCheckedChangeListener {
    final Listener mListener;
    final int mSourceId;

    public interface Listener {
        void _internalCallbackOnCheckedChanged(int i, RadioGroup radioGroup, int i2);
    }

    public OnCheckedChangeListener(Listener listener, int i) {
        this.mListener = listener;
        this.mSourceId = i;
    }

    @Override // android.widget.RadioGroup.OnCheckedChangeListener
    public void onCheckedChanged(RadioGroup radioGroup, int i) {
        this.mListener._internalCallbackOnCheckedChanged(this.mSourceId, radioGroup, i);
    }
}
