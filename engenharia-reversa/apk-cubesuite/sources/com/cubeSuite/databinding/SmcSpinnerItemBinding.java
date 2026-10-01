package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class SmcSpinnerItemBinding implements ViewBinding {
    private final TextView rootView;

    private SmcSpinnerItemBinding(TextView textView) {
        this.rootView = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public TextView getRoot() {
        return this.rootView;
    }

    public static SmcSpinnerItemBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SmcSpinnerItemBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.smc_spinner_item, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SmcSpinnerItemBinding bind(View view) {
        if (view == null) {
            throw new NullPointerException("rootView");
        }
        return new SmcSpinnerItemBinding((TextView) view);
    }
}
