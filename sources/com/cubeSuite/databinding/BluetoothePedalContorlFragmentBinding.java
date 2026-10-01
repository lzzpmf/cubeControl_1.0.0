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

/* JADX INFO: loaded from: classes.dex */
public final class BluetoothePedalContorlFragmentBinding implements ViewBinding {
    public final Button customKeypad1;
    public final Button customKeypad2;
    public final Button customKeypad3;
    public final Button customKeypad4;
    public final Button customKeypad5;
    public final LinearLayout customKeypadContainer;
    public final ImageView imageView;
    public final LinearLayout linearLayout;
    public final ButtonIconView midiChannelMinus;
    public final ButtonIconView midiChannelPlus;
    public final WheelView modeSelect;
    private final ConstraintLayout rootView;
    public final TextView sendMidiChannel;
    public final ButtonIconView trsMidi;
    public final TextView tvModelExplain;

    private BluetoothePedalContorlFragmentBinding(ConstraintLayout constraintLayout, Button button, Button button2, Button button3, Button button4, Button button5, LinearLayout linearLayout, ImageView imageView, LinearLayout linearLayout2, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, WheelView wheelView, TextView textView, ButtonIconView buttonIconView3, TextView textView2) {
        this.rootView = constraintLayout;
        this.customKeypad1 = button;
        this.customKeypad2 = button2;
        this.customKeypad3 = button3;
        this.customKeypad4 = button4;
        this.customKeypad5 = button5;
        this.customKeypadContainer = linearLayout;
        this.imageView = imageView;
        this.linearLayout = linearLayout2;
        this.midiChannelMinus = buttonIconView;
        this.midiChannelPlus = buttonIconView2;
        this.modeSelect = wheelView;
        this.sendMidiChannel = textView;
        this.trsMidi = buttonIconView3;
        this.tvModelExplain = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static BluetoothePedalContorlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static BluetoothePedalContorlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.bluetoothe_pedal_contorl_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static BluetoothePedalContorlFragmentBinding bind(View view) {
        int i = R.id.customKeypad1;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad1);
        if (button != null) {
            i = R.id.customKeypad2;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad2);
            if (button2 != null) {
                i = R.id.customKeypad3;
                Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad3);
                if (button3 != null) {
                    i = R.id.customKeypad4;
                    Button button4 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad4);
                    if (button4 != null) {
                        i = R.id.customKeypad5;
                        Button button5 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypad5);
                        if (button5 != null) {
                            i = R.id.customKeypadContainer;
                            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeypadContainer);
                            if (linearLayout != null) {
                                i = R.id.imageView;
                                ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                                if (imageView != null) {
                                    i = R.id.linearLayout;
                                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linearLayout);
                                    if (linearLayout2 != null) {
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
                                                        i = R.id.trsMidi;
                                                        ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.trsMidi);
                                                        if (buttonIconView3 != null) {
                                                            i = R.id.tvModelExplain;
                                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvModelExplain);
                                                            if (textView2 != null) {
                                                                return new BluetoothePedalContorlFragmentBinding((ConstraintLayout) view, button, button2, button3, button4, button5, linearLayout, imageView, linearLayout2, buttonIconView, buttonIconView2, wheelView, textView, buttonIconView3, textView2);
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
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
