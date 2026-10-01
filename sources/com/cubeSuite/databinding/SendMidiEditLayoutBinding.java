package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class SendMidiEditLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final ButtonIconView deleteList;
    public final TextView footSwitchText;
    public final LinearLayout main;
    public final RadioButton midiA;
    public final RadioButton midiB;
    public final RadioGroup midiGroup;
    public final LinearLayout midiInfoList;
    public final ScrollView midiInfoListScroll;
    public final Button modeSwitch;
    public final ButtonIconView plusMidiCode;
    private final LinearLayout rootView;

    private SendMidiEditLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, TextView textView, LinearLayout linearLayout2, RadioButton radioButton, RadioButton radioButton2, RadioGroup radioGroup, LinearLayout linearLayout3, ScrollView scrollView, Button button, ButtonIconView buttonIconView3) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.deleteList = buttonIconView2;
        this.footSwitchText = textView;
        this.main = linearLayout2;
        this.midiA = radioButton;
        this.midiB = radioButton2;
        this.midiGroup = radioGroup;
        this.midiInfoList = linearLayout3;
        this.midiInfoListScroll = scrollView;
        this.modeSwitch = button;
        this.plusMidiCode = buttonIconView3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SendMidiEditLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SendMidiEditLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.send_midi_edit_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SendMidiEditLayoutBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.deleteList;
            ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.deleteList);
            if (buttonIconView2 != null) {
                i = R.id.footSwitchText;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.footSwitchText);
                if (textView != null) {
                    LinearLayout linearLayout = (LinearLayout) view;
                    i = R.id.midiA;
                    RadioButton radioButton = (RadioButton) ViewBindings.findChildViewById(view, R.id.midiA);
                    if (radioButton != null) {
                        i = R.id.midiB;
                        RadioButton radioButton2 = (RadioButton) ViewBindings.findChildViewById(view, R.id.midiB);
                        if (radioButton2 != null) {
                            i = R.id.midiGroup;
                            RadioGroup radioGroup = (RadioGroup) ViewBindings.findChildViewById(view, R.id.midiGroup);
                            if (radioGroup != null) {
                                i = R.id.midiInfoList;
                                LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.midiInfoList);
                                if (linearLayout2 != null) {
                                    i = R.id.midiInfoListScroll;
                                    ScrollView scrollView = (ScrollView) ViewBindings.findChildViewById(view, R.id.midiInfoListScroll);
                                    if (scrollView != null) {
                                        i = R.id.modeSwitch;
                                        Button button = (Button) ViewBindings.findChildViewById(view, R.id.modeSwitch);
                                        if (button != null) {
                                            i = R.id.plusMidiCode;
                                            ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.plusMidiCode);
                                            if (buttonIconView3 != null) {
                                                return new SendMidiEditLayoutBinding(linearLayout, buttonIconView, buttonIconView2, textView, linearLayout, radioButton, radioButton2, radioGroup, linearLayout2, scrollView, button, buttonIconView3);
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
