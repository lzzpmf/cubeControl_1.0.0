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
import androidx.appcompat.widget.SwitchCompat;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.contrarywind.view.WheelView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class Fc2LayoutBinding implements ViewBinding {
    public final Button advancedCustomMode1But1;
    public final Button advancedCustomMode1But2;
    public final Button advancedCustomMode1But3;
    public final Button advancedCustomMode1But4;
    public final AppCompatSpinner advancedCustomMode1Spinner;
    public final LinearLayout advancedCustomMode1View;
    public final Button advancedCustomMode2But1;
    public final Button advancedCustomMode2But2;
    public final Button advancedCustomMode2But3;
    public final Button advancedCustomMode2But4;
    public final Button customControlBut1;
    public final Button customControlBut2;
    public final Button customControlBut3;
    public final Button customControlBut4;
    public final Button customControlBut5;
    public final LinearLayout customControlView;
    public final AppCompatSpinner customKeySpinner;
    public final LinearLayout customKeyboardView;
    public final Button footCtrlExport;
    public final Button footCtrlImport;
    public final ImageView imageView;
    public final Button keyA;
    public final Button keyB;
    public final Button keyC;
    public final Button keyD;
    public final LinearLayout linearLayout;
    public final LinearLayout main;
    public final LinearLayout maxGroupCountContainer15;
    public final LinearLayout maxGroupCountContainer31;
    public final AppCompatSpinner maxGroupCountSpinner15;
    public final AppCompatSpinner maxGroupCountSpinner31;
    public final ButtonIconView midiChannelMinus;
    public final ButtonIconView midiChannelPlus;
    public final SwitchCompat midiSwitch;
    public final AppCompatSpinner mixKeyModeSpinner;
    public final LinearLayout mixKeyView;
    public final TextView modeExplainTv;
    public final WheelView modeSelect;
    public final LinearLayout pcdispContainer;
    public final SwitchCompat pcdispSwitch;
    private final LinearLayout rootView;
    public final TextView sendMidiChannel;
    public final ButtonIconView trsMidi;
    public final ImageButton updatePedalParam;

    private Fc2LayoutBinding(LinearLayout linearLayout, Button button, Button button2, Button button3, Button button4, AppCompatSpinner appCompatSpinner, LinearLayout linearLayout2, Button button5, Button button6, Button button7, Button button8, Button button9, Button button10, Button button11, Button button12, Button button13, LinearLayout linearLayout3, AppCompatSpinner appCompatSpinner2, LinearLayout linearLayout4, Button button14, Button button15, ImageView imageView, Button button16, Button button17, Button button18, Button button19, LinearLayout linearLayout5, LinearLayout linearLayout6, LinearLayout linearLayout7, LinearLayout linearLayout8, AppCompatSpinner appCompatSpinner3, AppCompatSpinner appCompatSpinner4, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, SwitchCompat switchCompat, AppCompatSpinner appCompatSpinner5, LinearLayout linearLayout9, TextView textView, WheelView wheelView, LinearLayout linearLayout10, SwitchCompat switchCompat2, TextView textView2, ButtonIconView buttonIconView3, ImageButton imageButton) {
        this.rootView = linearLayout;
        this.advancedCustomMode1But1 = button;
        this.advancedCustomMode1But2 = button2;
        this.advancedCustomMode1But3 = button3;
        this.advancedCustomMode1But4 = button4;
        this.advancedCustomMode1Spinner = appCompatSpinner;
        this.advancedCustomMode1View = linearLayout2;
        this.advancedCustomMode2But1 = button5;
        this.advancedCustomMode2But2 = button6;
        this.advancedCustomMode2But3 = button7;
        this.advancedCustomMode2But4 = button8;
        this.customControlBut1 = button9;
        this.customControlBut2 = button10;
        this.customControlBut3 = button11;
        this.customControlBut4 = button12;
        this.customControlBut5 = button13;
        this.customControlView = linearLayout3;
        this.customKeySpinner = appCompatSpinner2;
        this.customKeyboardView = linearLayout4;
        this.footCtrlExport = button14;
        this.footCtrlImport = button15;
        this.imageView = imageView;
        this.keyA = button16;
        this.keyB = button17;
        this.keyC = button18;
        this.keyD = button19;
        this.linearLayout = linearLayout5;
        this.main = linearLayout6;
        this.maxGroupCountContainer15 = linearLayout7;
        this.maxGroupCountContainer31 = linearLayout8;
        this.maxGroupCountSpinner15 = appCompatSpinner3;
        this.maxGroupCountSpinner31 = appCompatSpinner4;
        this.midiChannelMinus = buttonIconView;
        this.midiChannelPlus = buttonIconView2;
        this.midiSwitch = switchCompat;
        this.mixKeyModeSpinner = appCompatSpinner5;
        this.mixKeyView = linearLayout9;
        this.modeExplainTv = textView;
        this.modeSelect = wheelView;
        this.pcdispContainer = linearLayout10;
        this.pcdispSwitch = switchCompat2;
        this.sendMidiChannel = textView2;
        this.trsMidi = buttonIconView3;
        this.updatePedalParam = imageButton;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2LayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2LayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2LayoutBinding bind(View view) {
        int i = R.id.advancedCustomMode1But1;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1But1);
        if (button != null) {
            i = R.id.advancedCustomMode1But2;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1But2);
            if (button2 != null) {
                i = R.id.advancedCustomMode1But3;
                Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1But3);
                if (button3 != null) {
                    i = R.id.advancedCustomMode1But4;
                    Button button4 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1But4);
                    if (button4 != null) {
                        i = R.id.advancedCustomMode1Spinner;
                        AppCompatSpinner appCompatSpinner = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1Spinner);
                        if (appCompatSpinner != null) {
                            i = R.id.advancedCustomMode1View;
                            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.advancedCustomMode1View);
                            if (linearLayout != null) {
                                i = R.id.advancedCustomMode2But1;
                                Button button5 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode2But1);
                                if (button5 != null) {
                                    i = R.id.advancedCustomMode2But2;
                                    Button button6 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode2But2);
                                    if (button6 != null) {
                                        i = R.id.advancedCustomMode2But3;
                                        Button button7 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode2But3);
                                        if (button7 != null) {
                                            i = R.id.advancedCustomMode2But4;
                                            Button button8 = (Button) ViewBindings.findChildViewById(view, R.id.advancedCustomMode2But4);
                                            if (button8 != null) {
                                                i = R.id.customControlBut1;
                                                Button button9 = (Button) ViewBindings.findChildViewById(view, R.id.customControlBut1);
                                                if (button9 != null) {
                                                    i = R.id.customControlBut2;
                                                    Button button10 = (Button) ViewBindings.findChildViewById(view, R.id.customControlBut2);
                                                    if (button10 != null) {
                                                        i = R.id.customControlBut3;
                                                        Button button11 = (Button) ViewBindings.findChildViewById(view, R.id.customControlBut3);
                                                        if (button11 != null) {
                                                            i = R.id.customControlBut4;
                                                            Button button12 = (Button) ViewBindings.findChildViewById(view, R.id.customControlBut4);
                                                            if (button12 != null) {
                                                                i = R.id.customControlBut5;
                                                                Button button13 = (Button) ViewBindings.findChildViewById(view, R.id.customControlBut5);
                                                                if (button13 != null) {
                                                                    i = R.id.customControlView;
                                                                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customControlView);
                                                                    if (linearLayout2 != null) {
                                                                        i = R.id.custom_key_spinner;
                                                                        AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.custom_key_spinner);
                                                                        if (appCompatSpinner2 != null) {
                                                                            i = R.id.customKeyboardView;
                                                                            LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.customKeyboardView);
                                                                            if (linearLayout3 != null) {
                                                                                i = R.id.foot_ctrl_export;
                                                                                Button button14 = (Button) ViewBindings.findChildViewById(view, R.id.foot_ctrl_export);
                                                                                if (button14 != null) {
                                                                                    i = R.id.foot_ctrl_import;
                                                                                    Button button15 = (Button) ViewBindings.findChildViewById(view, R.id.foot_ctrl_import);
                                                                                    if (button15 != null) {
                                                                                        i = R.id.imageView;
                                                                                        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                                                                                        if (imageView != null) {
                                                                                            i = R.id.keyA;
                                                                                            Button button16 = (Button) ViewBindings.findChildViewById(view, R.id.keyA);
                                                                                            if (button16 != null) {
                                                                                                i = R.id.keyB;
                                                                                                Button button17 = (Button) ViewBindings.findChildViewById(view, R.id.keyB);
                                                                                                if (button17 != null) {
                                                                                                    i = R.id.keyC;
                                                                                                    Button button18 = (Button) ViewBindings.findChildViewById(view, R.id.keyC);
                                                                                                    if (button18 != null) {
                                                                                                        i = R.id.keyD;
                                                                                                        Button button19 = (Button) ViewBindings.findChildViewById(view, R.id.keyD);
                                                                                                        if (button19 != null) {
                                                                                                            i = R.id.linearLayout;
                                                                                                            LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linearLayout);
                                                                                                            if (linearLayout4 != null) {
                                                                                                                LinearLayout linearLayout5 = (LinearLayout) view;
                                                                                                                i = R.id.maxGroupCountContainer15;
                                                                                                                LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.maxGroupCountContainer15);
                                                                                                                if (linearLayout6 != null) {
                                                                                                                    i = R.id.maxGroupCountContainer31;
                                                                                                                    LinearLayout linearLayout7 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.maxGroupCountContainer31);
                                                                                                                    if (linearLayout7 != null) {
                                                                                                                        i = R.id.maxGroupCountSpinner15;
                                                                                                                        AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.maxGroupCountSpinner15);
                                                                                                                        if (appCompatSpinner3 != null) {
                                                                                                                            i = R.id.maxGroupCountSpinner31;
                                                                                                                            AppCompatSpinner appCompatSpinner4 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.maxGroupCountSpinner31);
                                                                                                                            if (appCompatSpinner4 != null) {
                                                                                                                                i = R.id.midiChannelMinus;
                                                                                                                                ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelMinus);
                                                                                                                                if (buttonIconView != null) {
                                                                                                                                    i = R.id.midiChannelPlus;
                                                                                                                                    ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.midiChannelPlus);
                                                                                                                                    if (buttonIconView2 != null) {
                                                                                                                                        i = R.id.midiSwitch;
                                                                                                                                        SwitchCompat switchCompat = (SwitchCompat) ViewBindings.findChildViewById(view, R.id.midiSwitch);
                                                                                                                                        if (switchCompat != null) {
                                                                                                                                            i = R.id.mixKeyModeSpinner;
                                                                                                                                            AppCompatSpinner appCompatSpinner5 = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.mixKeyModeSpinner);
                                                                                                                                            if (appCompatSpinner5 != null) {
                                                                                                                                                i = R.id.mixKeyView;
                                                                                                                                                LinearLayout linearLayout8 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.mixKeyView);
                                                                                                                                                if (linearLayout8 != null) {
                                                                                                                                                    i = R.id.mode_explain_tv;
                                                                                                                                                    TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.mode_explain_tv);
                                                                                                                                                    if (textView != null) {
                                                                                                                                                        i = R.id.modeSelect;
                                                                                                                                                        WheelView wheelView = (WheelView) ViewBindings.findChildViewById(view, R.id.modeSelect);
                                                                                                                                                        if (wheelView != null) {
                                                                                                                                                            i = R.id.pcdispContainer;
                                                                                                                                                            LinearLayout linearLayout9 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.pcdispContainer);
                                                                                                                                                            if (linearLayout9 != null) {
                                                                                                                                                                i = R.id.pcdispSwitch;
                                                                                                                                                                SwitchCompat switchCompat2 = (SwitchCompat) ViewBindings.findChildViewById(view, R.id.pcdispSwitch);
                                                                                                                                                                if (switchCompat2 != null) {
                                                                                                                                                                    i = R.id.sendMidiChannel;
                                                                                                                                                                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.sendMidiChannel);
                                                                                                                                                                    if (textView2 != null) {
                                                                                                                                                                        i = R.id.trsMidi;
                                                                                                                                                                        ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.trsMidi);
                                                                                                                                                                        if (buttonIconView3 != null) {
                                                                                                                                                                            i = R.id.updatePedalParam;
                                                                                                                                                                            ImageButton imageButton = (ImageButton) ViewBindings.findChildViewById(view, R.id.updatePedalParam);
                                                                                                                                                                            if (imageButton != null) {
                                                                                                                                                                                return new Fc2LayoutBinding(linearLayout5, button, button2, button3, button4, appCompatSpinner, linearLayout, button5, button6, button7, button8, button9, button10, button11, button12, button13, linearLayout2, appCompatSpinner2, linearLayout3, button14, button15, imageView, button16, button17, button18, button19, linearLayout4, linearLayout5, linearLayout6, linearLayout7, appCompatSpinner3, appCompatSpinner4, buttonIconView, buttonIconView2, switchCompat, appCompatSpinner5, linearLayout8, textView, wheelView, linearLayout9, switchCompat2, textView2, buttonIconView3, imageButton);
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
