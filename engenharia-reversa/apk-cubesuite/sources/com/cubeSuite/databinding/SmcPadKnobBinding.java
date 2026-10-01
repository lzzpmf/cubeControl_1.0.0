package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.GridView;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.google.android.material.tabs.TabLayout;

/* JADX INFO: loaded from: classes.dex */
public final class SmcPadKnobBinding implements ViewBinding {
    public final GridView knobGridView;
    private final LinearLayout rootView;
    public final TabLayout tabLayoutKnob;

    private SmcPadKnobBinding(LinearLayout linearLayout, GridView gridView, TabLayout tabLayout) {
        this.rootView = linearLayout;
        this.knobGridView = gridView;
        this.tabLayoutKnob = tabLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SmcPadKnobBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SmcPadKnobBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.smc_pad_knob, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SmcPadKnobBinding bind(View view) {
        int i = R.id.knob_grid_view;
        GridView gridView = (GridView) ViewBindings.findChildViewById(view, R.id.knob_grid_view);
        if (gridView != null) {
            i = R.id.tab_layout_knob;
            TabLayout tabLayout = (TabLayout) ViewBindings.findChildViewById(view, R.id.tab_layout_knob);
            if (tabLayout != null) {
                return new SmcPadKnobBinding((LinearLayout) view, gridView, tabLayout);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
