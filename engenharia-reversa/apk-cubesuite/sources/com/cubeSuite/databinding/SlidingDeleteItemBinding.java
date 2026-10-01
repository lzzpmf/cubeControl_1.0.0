package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class SlidingDeleteItemBinding implements ViewBinding {
    public final ButtonIconView btnDelete;
    private final LinearLayout rootView;

    private SlidingDeleteItemBinding(LinearLayout linearLayout, ButtonIconView buttonIconView) {
        this.rootView = linearLayout;
        this.btnDelete = buttonIconView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SlidingDeleteItemBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SlidingDeleteItemBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.sliding_delete_item, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SlidingDeleteItemBinding bind(View view) {
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.btnDelete);
        if (buttonIconView != null) {
            return new SlidingDeleteItemBinding((LinearLayout) view, buttonIconView);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(R.id.btnDelete)));
    }
}
