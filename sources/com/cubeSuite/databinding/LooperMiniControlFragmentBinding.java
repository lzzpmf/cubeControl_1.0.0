package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class LooperMiniControlFragmentBinding implements ViewBinding {
    public final LinearLayout customKeypadContainer;
    public final SeekBar gate;
    public final TextView gateValue;
    public final View halving1;
    public final ImageView imageView;
    public final LinearLayout linearLayout3;
    private final ConstraintLayout rootView;
    public final ButtonIconView stop;
    public final ButtonIconView syncGate;
    public final SeekBar volume;
    public final TextView volumeValue;

    private LooperMiniControlFragmentBinding(ConstraintLayout constraintLayout, LinearLayout linearLayout, SeekBar seekBar, TextView textView, View view, ImageView imageView, LinearLayout linearLayout2, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, SeekBar seekBar2, TextView textView2) {
        this.rootView = constraintLayout;
        this.customKeypadContainer = linearLayout;
        this.gate = seekBar;
        this.gateValue = textView;
        this.halving1 = view;
        this.imageView = imageView;
        this.linearLayout3 = linearLayout2;
        this.stop = buttonIconView;
        this.syncGate = buttonIconView2;
        this.volume = seekBar2;
        this.volumeValue = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static LooperMiniControlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static LooperMiniControlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.looper_mini_control_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static LooperMiniControlFragmentBinding bind(View view) {
        int i = R.id.customKeypadContainer;
        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeypadContainer);
        if (linearLayout != null) {
            i = R.id.gate;
            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, R.id.gate);
            if (seekBar != null) {
                i = R.id.gateValue;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.gateValue);
                if (textView != null) {
                    i = R.id.halving1;
                    View viewFindChildViewById = ViewBindings.findChildViewById(view, R.id.halving1);
                    if (viewFindChildViewById != null) {
                        i = R.id.imageView;
                        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                        if (imageView != null) {
                            i = R.id.linearLayout3;
                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linearLayout3);
                            if (linearLayout2 != null) {
                                i = R.id.stop;
                                ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.stop);
                                if (buttonIconView != null) {
                                    i = R.id.syncGate;
                                    ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.syncGate);
                                    if (buttonIconView2 != null) {
                                        i = R.id.volume;
                                        SeekBar seekBar2 = (SeekBar) ViewBindings.findChildViewById(view, R.id.volume);
                                        if (seekBar2 != null) {
                                            i = R.id.volumeValue;
                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.volumeValue);
                                            if (textView2 != null) {
                                                return new LooperMiniControlFragmentBinding((ConstraintLayout) view, linearLayout, seekBar, textView, viewFindChildViewById, imageView, linearLayout2, buttonIconView, buttonIconView2, seekBar2, textView2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
