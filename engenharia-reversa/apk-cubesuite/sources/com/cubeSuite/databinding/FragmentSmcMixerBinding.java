package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.GridLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.core.widget.NestedScrollView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class FragmentSmcMixerBinding implements ViewBinding {
    public final LinearLayout globButtonLayout;
    public final LinearLayout knobLayout;
    private final NestedScrollView rootView;
    public final LinearLayout sliderLayout;
    public final LinearLayout smcHeader;
    public final TextView smcReset;
    public final TextView smcSave;
    public final GridLayout trackButtonLayout;

    private FragmentSmcMixerBinding(NestedScrollView nestedScrollView, LinearLayout linearLayout, LinearLayout linearLayout2, LinearLayout linearLayout3, LinearLayout linearLayout4, TextView textView, TextView textView2, GridLayout gridLayout) {
        this.rootView = nestedScrollView;
        this.globButtonLayout = linearLayout;
        this.knobLayout = linearLayout2;
        this.sliderLayout = linearLayout3;
        this.smcHeader = linearLayout4;
        this.smcReset = textView;
        this.smcSave = textView2;
        this.trackButtonLayout = gridLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public NestedScrollView getRoot() {
        return this.rootView;
    }

    public static FragmentSmcMixerBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FragmentSmcMixerBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fragment_smc_mixer, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FragmentSmcMixerBinding bind(View view) {
        int i = R.id.globButtonLayout;
        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.globButtonLayout);
        if (linearLayout != null) {
            i = R.id.knobLayout;
            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.knobLayout);
            if (linearLayout2 != null) {
                i = R.id.sliderLayout;
                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.sliderLayout);
                if (linearLayout3 != null) {
                    i = R.id.smc_header;
                    LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.smc_header);
                    if (linearLayout4 != null) {
                        i = R.id.smc_reset;
                        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.smc_reset);
                        if (textView != null) {
                            i = R.id.smc_save;
                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.smc_save);
                            if (textView2 != null) {
                                i = R.id.trackButtonLayout;
                                GridLayout gridLayout = (GridLayout) ViewBindings.findChildViewById(view, R.id.trackButtonLayout);
                                if (gridLayout != null) {
                                    return new FragmentSmcMixerBinding((NestedScrollView) view, linearLayout, linearLayout2, linearLayout3, linearLayout4, textView, textView2, gridLayout);
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
