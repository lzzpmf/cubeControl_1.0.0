package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class BackHeadViewBinding implements ViewBinding {
    public final ButtonIconView btnEnd;
    public final ButtonIconView btnStart;
    private final FrameLayout rootView;
    public final TextView tvTitle;

    private BackHeadViewBinding(FrameLayout frameLayout, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, TextView textView) {
        this.rootView = frameLayout;
        this.btnEnd = buttonIconView;
        this.btnStart = buttonIconView2;
        this.tvTitle = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public FrameLayout getRoot() {
        return this.rootView;
    }

    public static BackHeadViewBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static BackHeadViewBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.back_head_view, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static BackHeadViewBinding bind(View view) {
        int i = R.id.btnEnd;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.btnEnd);
        if (buttonIconView != null) {
            i = R.id.btnStart;
            ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.btnStart);
            if (buttonIconView2 != null) {
                i = R.id.tvTitle;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.tvTitle);
                if (textView != null) {
                    return new BackHeadViewBinding((FrameLayout) view, buttonIconView, buttonIconView2, textView);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
