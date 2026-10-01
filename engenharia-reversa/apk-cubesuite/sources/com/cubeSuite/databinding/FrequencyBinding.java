package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.GainView;

/* JADX INFO: loaded from: classes.dex */
public final class FrequencyBinding implements ViewBinding {
    public final GainView frequencyLayout;
    public final LinearLayout main;
    private final LinearLayout rootView;

    private FrequencyBinding(LinearLayout linearLayout, GainView gainView, LinearLayout linearLayout2) {
        this.rootView = linearLayout;
        this.frequencyLayout = gainView;
        this.main = linearLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static FrequencyBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FrequencyBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.frequency, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FrequencyBinding bind(View view) {
        GainView gainView = (GainView) ViewBindings.findChildViewById(view, R.id.frequencyLayout);
        if (gainView != null) {
            LinearLayout linearLayout = (LinearLayout) view;
            return new FrequencyBinding(linearLayout, gainView, linearLayout);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(R.id.frequencyLayout)));
    }
}
