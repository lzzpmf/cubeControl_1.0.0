package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class SmcPadButtonBinding implements ViewBinding {
    private final LinearLayout rootView;
    public final ImageButton smcPadBut1;
    public final ImageButton smcPadBut2;
    public final ImageButton smcPadBut3;
    public final ImageButton smcPadBut4;
    public final ImageButton smcPadBut5;

    private SmcPadButtonBinding(LinearLayout linearLayout, ImageButton imageButton, ImageButton imageButton2, ImageButton imageButton3, ImageButton imageButton4, ImageButton imageButton5) {
        this.rootView = linearLayout;
        this.smcPadBut1 = imageButton;
        this.smcPadBut2 = imageButton2;
        this.smcPadBut3 = imageButton3;
        this.smcPadBut4 = imageButton4;
        this.smcPadBut5 = imageButton5;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SmcPadButtonBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SmcPadButtonBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.smc_pad_button, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SmcPadButtonBinding bind(View view) {
        int i = R.id.smc_pad_but_1;
        ImageButton imageButton = (ImageButton) ViewBindings.findChildViewById(view, R.id.smc_pad_but_1);
        if (imageButton != null) {
            i = R.id.smc_pad_but_2;
            ImageButton imageButton2 = (ImageButton) ViewBindings.findChildViewById(view, R.id.smc_pad_but_2);
            if (imageButton2 != null) {
                i = R.id.smc_pad_but_3;
                ImageButton imageButton3 = (ImageButton) ViewBindings.findChildViewById(view, R.id.smc_pad_but_3);
                if (imageButton3 != null) {
                    i = R.id.smc_pad_but_4;
                    ImageButton imageButton4 = (ImageButton) ViewBindings.findChildViewById(view, R.id.smc_pad_but_4);
                    if (imageButton4 != null) {
                        i = R.id.smc_pad_but_5;
                        ImageButton imageButton5 = (ImageButton) ViewBindings.findChildViewById(view, R.id.smc_pad_but_5);
                        if (imageButton5 != null) {
                            return new SmcPadButtonBinding((LinearLayout) view, imageButton, imageButton2, imageButton3, imageButton4, imageButton5);
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
