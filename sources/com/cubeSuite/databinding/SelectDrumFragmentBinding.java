package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.BackHeadView;
import com.cubeSuite.customControl.GridRadioGroup;

/* JADX INFO: loaded from: classes.dex */
public final class SelectDrumFragmentBinding implements ViewBinding {
    public final BackHeadView backHead;
    public final GridRadioGroup btnSelectDrum;
    private final LinearLayout rootView;

    private SelectDrumFragmentBinding(LinearLayout linearLayout, BackHeadView backHeadView, GridRadioGroup gridRadioGroup) {
        this.rootView = linearLayout;
        this.backHead = backHeadView;
        this.btnSelectDrum = gridRadioGroup;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SelectDrumFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SelectDrumFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.select_drum_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SelectDrumFragmentBinding bind(View view) {
        int i = R.id.backHead;
        BackHeadView backHeadView = (BackHeadView) ViewBindings.findChildViewById(view, R.id.backHead);
        if (backHeadView != null) {
            i = R.id.btnSelectDrum;
            GridRadioGroup gridRadioGroup = (GridRadioGroup) ViewBindings.findChildViewById(view, R.id.btnSelectDrum);
            if (gridRadioGroup != null) {
                return new SelectDrumFragmentBinding((LinearLayout) view, backHeadView, gridRadioGroup);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
