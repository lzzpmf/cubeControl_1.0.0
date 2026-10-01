package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class TrsSwitchLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final ImageView expressionPedal;
    public final LinearLayout main;
    private final LinearLayout rootView;
    public final ImageView trsInterface;
    public final ImageView trsMidi;

    private TrsSwitchLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, ImageView imageView, LinearLayout linearLayout2, ImageView imageView2, ImageView imageView3) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.expressionPedal = imageView;
        this.main = linearLayout2;
        this.trsInterface = imageView2;
        this.trsMidi = imageView3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static TrsSwitchLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static TrsSwitchLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.trs_switch_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static TrsSwitchLayoutBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.expressionPedal;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.expressionPedal);
            if (imageView != null) {
                LinearLayout linearLayout = (LinearLayout) view;
                i = R.id.trs_interface;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, R.id.trs_interface);
                if (imageView2 != null) {
                    i = R.id.trsMidi;
                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, R.id.trsMidi);
                    if (imageView3 != null) {
                        return new TrsSwitchLayoutBinding(linearLayout, buttonIconView, imageView, linearLayout, imageView2, imageView3);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
