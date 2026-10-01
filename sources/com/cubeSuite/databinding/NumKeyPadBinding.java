package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class NumKeyPadBinding implements ViewBinding {
    public final ButtonIconView close;
    public final ButtonIconView delete;
    public final Button eight;
    public final Button five;
    public final Button four;
    public final LinearLayout main;
    public final Button nine;
    public final Button one;
    private final LinearLayout rootView;
    public final Button seven;
    public final Button six;
    public final Button three;
    public final Button two;
    public final Button zero;

    private NumKeyPadBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, Button button, Button button2, Button button3, LinearLayout linearLayout2, Button button4, Button button5, Button button6, Button button7, Button button8, Button button9, Button button10) {
        this.rootView = linearLayout;
        this.close = buttonIconView;
        this.delete = buttonIconView2;
        this.eight = button;
        this.five = button2;
        this.four = button3;
        this.main = linearLayout2;
        this.nine = button4;
        this.one = button5;
        this.seven = button6;
        this.six = button7;
        this.three = button8;
        this.two = button9;
        this.zero = button10;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static NumKeyPadBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static NumKeyPadBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.num_key_pad, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static NumKeyPadBinding bind(View view) {
        int i = R.id.close;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.close);
        if (buttonIconView != null) {
            i = R.id.delete;
            ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.delete);
            if (buttonIconView2 != null) {
                i = R.id.eight;
                Button button = (Button) ViewBindings.findChildViewById(view, R.id.eight);
                if (button != null) {
                    i = R.id.five;
                    Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.five);
                    if (button2 != null) {
                        i = R.id.four;
                        Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.four);
                        if (button3 != null) {
                            LinearLayout linearLayout = (LinearLayout) view;
                            i = R.id.nine;
                            Button button4 = (Button) ViewBindings.findChildViewById(view, R.id.nine);
                            if (button4 != null) {
                                i = R.id.one;
                                Button button5 = (Button) ViewBindings.findChildViewById(view, R.id.one);
                                if (button5 != null) {
                                    i = R.id.seven;
                                    Button button6 = (Button) ViewBindings.findChildViewById(view, R.id.seven);
                                    if (button6 != null) {
                                        i = R.id.six;
                                        Button button7 = (Button) ViewBindings.findChildViewById(view, R.id.six);
                                        if (button7 != null) {
                                            i = R.id.three;
                                            Button button8 = (Button) ViewBindings.findChildViewById(view, R.id.three);
                                            if (button8 != null) {
                                                i = R.id.two;
                                                Button button9 = (Button) ViewBindings.findChildViewById(view, R.id.two);
                                                if (button9 != null) {
                                                    i = R.id.zero;
                                                    Button button10 = (Button) ViewBindings.findChildViewById(view, R.id.zero);
                                                    if (button10 != null) {
                                                        return new NumKeyPadBinding(linearLayout, buttonIconView, buttonIconView2, button, button2, button3, linearLayout, button4, button5, button6, button7, button8, button9, button10);
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
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
