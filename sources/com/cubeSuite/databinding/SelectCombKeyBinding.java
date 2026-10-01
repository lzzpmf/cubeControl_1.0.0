package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.TextIconView;

/* JADX INFO: loaded from: classes.dex */
public final class SelectCombKeyBinding implements ViewBinding {
    public final Button close;
    public final AppCompatSpinner comb1;
    public final AppCompatSpinner comb2;
    public final AppCompatSpinner comb3;
    private final LinearLayout rootView;
    public final TextIconView title;

    private SelectCombKeyBinding(LinearLayout linearLayout, Button button, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2, AppCompatSpinner appCompatSpinner3, TextIconView textIconView) {
        this.rootView = linearLayout;
        this.close = button;
        this.comb1 = appCompatSpinner;
        this.comb2 = appCompatSpinner2;
        this.comb3 = appCompatSpinner3;
        this.title = textIconView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SelectCombKeyBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SelectCombKeyBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.select_comb_key, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SelectCombKeyBinding bind(View view) {
        int i = R.id.close;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.close);
        if (button != null) {
            i = R.id.comb1;
            AppCompatSpinner appCompatSpinner = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb1);
            if (appCompatSpinner != null) {
                i = R.id.comb2;
                AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb2);
                if (appCompatSpinner2 != null) {
                    i = R.id.comb3;
                    AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb3);
                    if (appCompatSpinner3 != null) {
                        i = R.id.title;
                        TextIconView textIconView = (TextIconView) ViewBindings.findChildViewById(view, R.id.title);
                        if (textIconView != null) {
                            return new SelectCombKeyBinding((LinearLayout) view, button, appCompatSpinner, appCompatSpinner2, appCompatSpinner3, textIconView);
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
