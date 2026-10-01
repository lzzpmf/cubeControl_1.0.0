package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.GridView;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class SmcPadPadBinding implements ViewBinding {
    public final RadioGroup padBankGroup;
    public final RadioButton padBankGroup1;
    public final RadioButton padBankGroup2;
    public final GridView padGridView;
    private final LinearLayout rootView;

    private SmcPadPadBinding(LinearLayout linearLayout, RadioGroup radioGroup, RadioButton radioButton, RadioButton radioButton2, GridView gridView) {
        this.rootView = linearLayout;
        this.padBankGroup = radioGroup;
        this.padBankGroup1 = radioButton;
        this.padBankGroup2 = radioButton2;
        this.padGridView = gridView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SmcPadPadBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SmcPadPadBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.smc_pad_pad, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SmcPadPadBinding bind(View view) {
        int i = R.id.pad_bank_group;
        RadioGroup radioGroup = (RadioGroup) ViewBindings.findChildViewById(view, R.id.pad_bank_group);
        if (radioGroup != null) {
            i = R.id.pad_bank_group1;
            RadioButton radioButton = (RadioButton) ViewBindings.findChildViewById(view, R.id.pad_bank_group1);
            if (radioButton != null) {
                i = R.id.pad_bank_group2;
                RadioButton radioButton2 = (RadioButton) ViewBindings.findChildViewById(view, R.id.pad_bank_group2);
                if (radioButton2 != null) {
                    i = R.id.pad_grid_view;
                    GridView gridView = (GridView) ViewBindings.findChildViewById(view, R.id.pad_grid_view);
                    if (gridView != null) {
                        return new SmcPadPadBinding((LinearLayout) view, radioGroup, radioButton, radioButton2, gridView);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
