package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.contrarywind.view.WheelView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class FootCtrlContorlFragmentBinding implements ViewBinding {
    public final LinearLayout advancedCustomKeypadContainer;
    public final AppCompatSpinner customKeySpinner;
    public final Button customKeypad1;
    public final Button customKeypad2;
    public final Button customKeypad3;
    public final Button customKeypad4;
    public final Button customKeypad5;
    public final LinearLayout customKeypadContainer;
    public final Button customKeypadKey1;
    public final Button customKeypadKey2;
    public final Button customKeypadKey3;
    public final Button customKeypadKey4;
    public final LinearLayout customKeypadModeView;
    public final Button footSwitchA;
    public final Button footSwitchB;
    public final Button footSwitchC;
    public final Button footSwitchD;
    public final ImageView imageView;
    public final Button keyB;
    public final Button keyC;
    public final Button keyD;
    public final LinearLayout linearLayout;
    public final LinearLayout main;
    public final ButtonIconView midiChannelMinus;
    public final ButtonIconView midiChannelPlus;
    public final WheelView modeSelect;
    public final LinearLayout newCustomKeyboardView;
    public final Button padInterface;
    private final LinearLayout rootView;
    public final TextView sendMidiChannel;
    public final ButtonIconView trsMidi;
    public final TextView tvModelExplain;
    public final ImageButton updatePedalParam;

    private FootCtrlContorlFragmentBinding(LinearLayout linearLayout, LinearLayout linearLayout2, AppCompatSpinner appCompatSpinner, Button button, Button button2, Button button3, Button button4, Button button5, LinearLayout linearLayout3, Button button6, Button button7, Button button8, Button button9, LinearLayout linearLayout4, Button button10, Button button11, Button button12, Button button13, ImageView imageView, Button button14, Button button15, Button button16, LinearLayout linearLayout5, LinearLayout linearLayout6, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, WheelView wheelView, LinearLayout linearLayout7, Button button17, TextView textView, ButtonIconView buttonIconView3, TextView textView2, ImageButton imageButton) {
        this.rootView = linearLayout;
        this.advancedCustomKeypadContainer = linearLayout2;
        this.customKeySpinner = appCompatSpinner;
        this.customKeypad1 = button;
        this.customKeypad2 = button2;
        this.customKeypad3 = button3;
        this.customKeypad4 = button4;
        this.customKeypad5 = button5;
        this.customKeypadContainer = linearLayout3;
        this.customKeypadKey1 = button6;
        this.customKeypadKey2 = button7;
        this.customKeypadKey3 = button8;
        this.customKeypadKey4 = button9;
        this.customKeypadModeView = linearLayout4;
        this.footSwitchA = button10;
        this.footSwitchB = button11;
        this.footSwitchC = button12;
        this.footSwitchD = button13;
        this.imageView = imageView;
        this.keyB = button14;
        this.keyC = button15;
        this.keyD = button16;
        this.linearLayout = linearLayout5;
        this.main = linearLayout6;
        this.midiChannelMinus = buttonIconView;
        this.midiChannelPlus = buttonIconView2;
        this.modeSelect = wheelView;
        this.newCustomKeyboardView = linearLayout7;
        this.padInterface = button17;
        this.sendMidiChannel = textView;
        this.trsMidi = buttonIconView3;
        this.tvModelExplain = textView2;
        this.updatePedalParam = imageButton;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static FootCtrlContorlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FootCtrlContorlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.foot_ctrl_contorl_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FootCtrlContorlFragmentBinding bind(View view) {
        int i = R.id.advancedCustomKeypadContainer;
        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.advancedCustomKeypadContainer);
        if (linearLayout != null) {
            i = R.id.custom_key_spinner;
            AppCompatSpinner appCompatSpinner = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.custom_key_spinner);
            if (appCompatSpinner != null) {
                i = R.id.customKeypad1;
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
                                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeypadContainer);
                                    if (linearLayout2 != null) {
                                        i = R.id.customKeypadKey1;
                                        Button button6 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypadKey1);
                                        if (button6 != null) {
                                            i = R.id.customKeypadKey2;
                                            Button button7 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypadKey2);
                                            if (button7 != null) {
                                                i = R.id.customKeypadKey3;
                                                Button button8 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypadKey3);
                                                if (button8 != null) {
                                                    i = R.id.customKeypadKey4;
                                                    Button button9 = (Button) ViewBindings.findChildViewById(view, R.id.customKeypadKey4);
                                                    if (button9 != null) {
                                                        i = R.id.customKeypadModeView;
                                                        LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeypadModeView);
                                                        if (linearLayout3 != null) {
                                                            i = R.id.footSwitchA;
                                                            Button button10 = (Button) ViewBindings.findChildViewById(view, R.id.footSwitchA);
                                                            if (button10 != null) {
                                                                i = R.id.footSwitchB;
                                                                Button button11 = (Button) ViewBindings.findChildViewById(view, R.id.footSwitchB);
                                                                if (button11 != null) {
                                                                    i = R.id.footSwitchC;
                                                                    Button button12 = (Button) ViewBindings.findChildViewById(view, R.id.footSwitchC);
                                                                    if (button12 != null) {
                                                                        i = R.id.footSwitchD;
                                                                        Button button13 = (Button) ViewBindings.findChildViewById(view, R.id.footSwitchD);
                                                                        if (button13 != null) {
                                                                            i = R.id.imageView;
                                                                            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                                                                            if (imageView != null) {
                                                                                i = R.id.keyB;
                                                                                Button button14 = (Button) ViewBindings.findChildViewById(view, R.id.keyB);
                                                                                if (button14 != null) {
                                                                                    i = R.id.keyC;
                                                                                    Button button15 = (Button) ViewBindings.findChildViewById(view, R.id.keyC);
                                                                                    if (button15 != null) {
                                                                                        i = R.id.keyD;
                                                                                        Button button16 = (Button) ViewBindings.findChildViewById(view, R.id.keyD);
                                                                                        if (button16 != null) {
                                                                                            i = R.id.linearLayout;
                                                                                            LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linearLayout);
                                                                                            if (linearLayout4 != null) {
                                                                                                LinearLayout linearLayout5 = (LinearLayout) view;
                                                                                                i = R.id.midiChannelMinus;
                                                                                                ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelMinus);
                                                                                                if (buttonIconView != null) {
                                                                                                    i = R.id.midiChannelPlus;
                                                                                                    ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelPlus);
                                                                                                    if (buttonIconView2 != null) {
                                                                                                        i = R.id.modeSelect;
                                                                                                        WheelView wheelView = (WheelView) ViewBindings.findChildViewById(view, R.id.modeSelect);
                                                                                                        if (wheelView != null) {
                                                                                                            i = R.id.newCustomKeyboardView;
                                                                                                            LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.newCustomKeyboardView);
                                                                                                            if (linearLayout6 != null) {
                                                                                                                i = R.id.padInterface;
                                                                                                                Button button17 = (Button) ViewBindings.findChildViewById(view, R.id.padInterface);
                                                                                                                if (button17 != null) {
                                                                                                                    i = R.id.sendMidiChannel;
                                                                                                                    TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.sendMidiChannel);
                                                                                                                    if (textView != null) {
                                                                                                                        i = R.id.trsMidi;
                                                                                                                        ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.trsMidi);
                                                                                                                        if (buttonIconView3 != null) {
                                                                                                                            i = R.id.tvModelExplain;
                                                                                                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvModelExplain);
                                                                                                                            if (textView2 != null) {
                                                                                                                                i = R.id.updatePedalParam;
                                                                                                                                ImageButton imageButton = (ImageButton) ViewBindings.findChildViewById(view, R.id.updatePedalParam);
                                                                                                                                if (imageButton != null) {
                                                                                                                                    return new FootCtrlContorlFragmentBinding(linearLayout5, linearLayout, appCompatSpinner, button, button2, button3, button4, button5, linearLayout2, button6, button7, button8, button9, linearLayout3, button10, button11, button12, button13, imageView, button14, button15, button16, linearLayout4, linearLayout5, buttonIconView, buttonIconView2, wheelView, linearLayout6, button17, textView, buttonIconView3, textView2, imageButton);
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
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
