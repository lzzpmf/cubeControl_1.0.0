package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class ControlAndPdfActivityBinding implements ViewBinding {
    public final ButtonIconView back;
    public final FrameLayout cuvaveBody;
    public final LinearLayout linearLayout2;
    public final ConstraintLayout rootLayout;
    private final ConstraintLayout rootView;

    private ControlAndPdfActivityBinding(ConstraintLayout constraintLayout, ButtonIconView buttonIconView, FrameLayout frameLayout, LinearLayout linearLayout, ConstraintLayout constraintLayout2) {
        this.rootView = constraintLayout;
        this.back = buttonIconView;
        this.cuvaveBody = frameLayout;
        this.linearLayout2 = linearLayout;
        this.rootLayout = constraintLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static ControlAndPdfActivityBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ControlAndPdfActivityBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.control_and_pdf_activity, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ControlAndPdfActivityBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.cuvave_body;
            FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, R.id.cuvave_body);
            if (frameLayout != null) {
                i = R.id.linearLayout2;
                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linearLayout2);
                if (linearLayout != null) {
                    ConstraintLayout constraintLayout = (ConstraintLayout) view;
                    return new ControlAndPdfActivityBinding(constraintLayout, buttonIconView, frameLayout, linearLayout, constraintLayout);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
