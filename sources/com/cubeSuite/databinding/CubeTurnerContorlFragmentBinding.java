package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.contrarywind.view.WheelView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.TextIconView;

/* JADX INFO: loaded from: classes.dex */
public final class CubeTurnerContorlFragmentBinding implements ViewBinding {
    public final Button customKeypad1;
    public final Button customKeypad2;
    public final LinearLayout customKeypadContainer;
    public final TextIconView hardware;
    public final ImageView imageView;
    public final ButtonIconView midiChannelMinus;
    public final ButtonIconView midiChannelPlus;
    public final WheelView modeSelect;
    private final ConstraintLayout rootView;
    public final TextView sendMidiChannel;
    public final TextView tvModelExplain;

    private CubeTurnerContorlFragmentBinding(ConstraintLayout constraintLayout, Button button, Button button2, LinearLayout linearLayout, TextIconView textIconView, ImageView imageView, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, WheelView wheelView, TextView textView, TextView textView2) {
        this.rootView = constraintLayout;
        this.customKeypad1 = button;
        this.customKeypad2 = button2;
        this.customKeypadContainer = linearLayout;
        this.hardware = textIconView;
        this.imageView = imageView;
        this.midiChannelMinus = buttonIconView;
        this.midiChannelPlus = buttonIconView2;
        this.modeSelect = wheelView;
        this.sendMidiChannel = textView;
        this.tvModelExplain = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static CubeTurnerContorlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static CubeTurnerContorlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.cube_turner_contorl_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static CubeTurnerContorlFragmentBinding bind(View view) {
        int i = R.id.customKeypad1;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad1);
        if (button != null) {
            i = R.id.customKeypad2;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad2);
            if (button2 != null) {
                i = R.id.customKeypadContainer;
                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeypadContainer);
                if (linearLayout != null) {
                    i = R.id.hardware;
                    TextIconView textIconView = (TextIconView) ViewBindings.findChildViewById(view, R.id.hardware);
                    if (textIconView != null) {
                        i = R.id.imageView;
                        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                        if (imageView != null) {
                            i = R.id.midiChannelMinus;
                            ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelMinus);
                            if (buttonIconView != null) {
                                i = R.id.midiChannelPlus;
                                ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelPlus);
                                if (buttonIconView2 != null) {
                                    i = R.id.modeSelect;
                                    WheelView wheelView = (WheelView) ViewBindings.findChildViewById(view, R.id.modeSelect);
                                    if (wheelView != null) {
                                        i = R.id.sendMidiChannel;
                                        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.sendMidiChannel);
                                        if (textView != null) {
                                            i = R.id.tvModelExplain;
                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvModelExplain);
                                            if (textView2 != null) {
                                                return new CubeTurnerContorlFragmentBinding((ConstraintLayout) view, button, button2, linearLayout, textIconView, imageView, buttonIconView, buttonIconView2, wheelView, textView, textView2);
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
