package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class SelectItemLayoutBinding implements ViewBinding {
    public final ButtonIconView btnInfo;
    public final LinearLayout clickContent;
    public final LinearLayout main;
    private final LinearLayout rootView;
    public final TextView tvInfo;
    public final TextView tvTitle;

    private SelectItemLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, LinearLayout linearLayout2, LinearLayout linearLayout3, TextView textView, TextView textView2) {
        this.rootView = linearLayout;
        this.btnInfo = buttonIconView;
        this.clickContent = linearLayout2;
        this.main = linearLayout3;
        this.tvInfo = textView;
        this.tvTitle = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SelectItemLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SelectItemLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.select_item_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SelectItemLayoutBinding bind(View view) {
        int i = R.id.btnInfo;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.btnInfo);
        if (buttonIconView != null) {
            i = R.id.clickContent;
            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.clickContent);
            if (linearLayout != null) {
                LinearLayout linearLayout2 = (LinearLayout) view;
                i = R.id.tvInfo;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.tvInfo);
                if (textView != null) {
                    i = R.id.tvTitle;
                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvTitle);
                    if (textView2 != null) {
                        return new SelectItemLayoutBinding(linearLayout2, buttonIconView, linearLayout, linearLayout2, textView, textView2);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
