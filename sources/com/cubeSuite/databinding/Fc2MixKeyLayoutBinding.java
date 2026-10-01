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
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class Fc2MixKeyLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final AppCompatSpinner comb1;
    public final AppCompatSpinner comb2;
    public final AppCompatSpinner comb3;
    public final ButtonIconView deleteList;
    public final TextView footSwitchText;
    public final LinearLayout main;
    public final RadioButton midiA;
    public final RadioButton midiB;
    public final RadioGroup midiGroup;
    public final LinearLayout midiInfoListA;
    public final LinearLayout midiInfoListB;
    public final ScrollView midiInfoListScrollA;
    public final ScrollView midiInfoListScrollB;
    public final LinearLayout mixKeyKeyTuner;
    public final LinearLayout mixKeyKeyboard;
    public final LinearLayout mixKeyMidi;
    public final AppCompatSpinner mixKeyTypeSpinner;
    public final Button modeSwitch;
    public final ButtonIconView plusMidiCode;
    private final LinearLayout rootView;
    public final AppCompatSpinner tunerSpinner;

    private Fc2MixKeyLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2, AppCompatSpinner appCompatSpinner3, ButtonIconView buttonIconView2, TextView textView, LinearLayout linearLayout2, RadioButton radioButton, RadioButton radioButton2, RadioGroup radioGroup, LinearLayout linearLayout3, LinearLayout linearLayout4, ScrollView scrollView, ScrollView scrollView2, LinearLayout linearLayout5, LinearLayout linearLayout6, LinearLayout linearLayout7, AppCompatSpinner appCompatSpinner4, Button button, ButtonIconView buttonIconView3, AppCompatSpinner appCompatSpinner5) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.comb1 = appCompatSpinner;
        this.comb2 = appCompatSpinner2;
        this.comb3 = appCompatSpinner3;
        this.deleteList = buttonIconView2;
        this.footSwitchText = textView;
        this.main = linearLayout2;
        this.midiA = radioButton;
        this.midiB = radioButton2;
        this.midiGroup = radioGroup;
        this.midiInfoListA = linearLayout3;
        this.midiInfoListB = linearLayout4;
        this.midiInfoListScrollA = scrollView;
        this.midiInfoListScrollB = scrollView2;
        this.mixKeyKeyTuner = linearLayout5;
        this.mixKeyKeyboard = linearLayout6;
        this.mixKeyMidi = linearLayout7;
        this.mixKeyTypeSpinner = appCompatSpinner4;
        this.modeSwitch = button;
        this.plusMidiCode = buttonIconView3;
        this.tunerSpinner = appCompatSpinner5;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2MixKeyLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2MixKeyLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_mix_key_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2MixKeyLayoutBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.comb1;
            AppCompatSpinner appCompatSpinner = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb1);
            if (appCompatSpinner != null) {
                i = R.id.comb2;
                AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb2);
                if (appCompatSpinner2 != null) {
                    i = R.id.comb3;
                    AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.comb3);
                    if (appCompatSpinner3 != null) {
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
                                            i = R.id.midiInfoListA;
                                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.midiInfoListA);
                                            if (linearLayout2 != null) {
                                                i = R.id.midiInfoListB;
                                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.midiInfoListB);
                                                if (linearLayout3 != null) {
                                                    i = R.id.midiInfoListScrollA;
                                                    ScrollView scrollView = (ScrollView) ViewBindings.findChildViewById(view, R.id.midiInfoListScrollA);
                                                    if (scrollView != null) {
                                                        i = R.id.midiInfoListScrollB;
                                                        ScrollView scrollView2 = (ScrollView) ViewBindings.findChildViewById(view, R.id.midiInfoListScrollB);
                                                        if (scrollView2 != null) {
                                                            i = R.id.mixKeyKeyTuner;
                                                            LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.mixKeyKeyTuner);
                                                            if (linearLayout4 != null) {
                                                                i = R.id.mixKeyKeyboard;
                                                                LinearLayout linearLayout5 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.mixKeyKeyboard);
                                                                if (linearLayout5 != null) {
                                                                    i = R.id.mixKeyMidi;
                                                                    LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.mixKeyMidi);
                                                                    if (linearLayout6 != null) {
                                                                        i = R.id.mixKeyTypeSpinner;
                                                                        AppCompatSpinner appCompatSpinner4 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.mixKeyTypeSpinner);
                                                                        if (appCompatSpinner4 != null) {
                                                                            i = R.id.modeSwitch;
                                                                            Button button = (Button) ViewBindings.findChildViewById(view, R.id.modeSwitch);
                                                                            if (button != null) {
                                                                                i = R.id.plusMidiCode;
                                                                                ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.plusMidiCode);
                                                                                if (buttonIconView3 != null) {
                                                                                    i = R.id.tunerSpinner;
                                                                                    AppCompatSpinner appCompatSpinner5 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.tunerSpinner);
                                                                                    if (appCompatSpinner5 != null) {
                                                                                        return new Fc2MixKeyLayoutBinding(linearLayout, buttonIconView, appCompatSpinner, appCompatSpinner2, appCompatSpinner3, buttonIconView2, textView, linearLayout, radioButton, radioButton2, radioGroup, linearLayout2, linearLayout3, scrollView, scrollView2, linearLayout4, linearLayout5, linearLayout6, appCompatSpinner4, button, buttonIconView3, appCompatSpinner5);
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
