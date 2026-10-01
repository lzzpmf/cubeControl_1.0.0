package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.NumKeyPadView;

/* JADX INFO: loaded from: classes.dex */
public final class Fc2EditInterfaceMidiInfoLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final Button cancel;
    public final Button channel;
    public final Button data1;
    public final Button data2;
    public final LinearLayout main;
    public final NumKeyPadView numKeyPad;
    public final Button numType;
    private final LinearLayout rootView;
    public final Button save;
    public final TextView tvChannel;
    public final TextView tvIData1;
    public final TextView tvIData2;
    public final TextView tvIType;
    public final TextView tvMidiInfo;

    private Fc2EditInterfaceMidiInfoLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, Button button, Button button2, Button button3, Button button4, LinearLayout linearLayout2, NumKeyPadView numKeyPadView, Button button5, Button button6, TextView textView, TextView textView2, TextView textView3, TextView textView4, TextView textView5) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.cancel = button;
        this.channel = button2;
        this.data1 = button3;
        this.data2 = button4;
        this.main = linearLayout2;
        this.numKeyPad = numKeyPadView;
        this.numType = button5;
        this.save = button6;
        this.tvChannel = textView;
        this.tvIData1 = textView2;
        this.tvIData2 = textView3;
        this.tvIType = textView4;
        this.tvMidiInfo = textView5;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2EditInterfaceMidiInfoLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2EditInterfaceMidiInfoLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_edit_interface_midi_info_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2EditInterfaceMidiInfoLayoutBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.cancel;
            Button button = (Button) ViewBindings.findChildViewById(view, R.id.cancel);
            if (button != null) {
                i = R.id.channel;
                Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.channel);
                if (button2 != null) {
                    i = R.id.data1;
                    Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.data1);
                    if (button3 != null) {
                        i = R.id.data2;
                        Button button4 = (Button) ViewBindings.findChildViewById(view, R.id.data2);
                        if (button4 != null) {
                            LinearLayout linearLayout = (LinearLayout) view;
                            i = R.id.numKeyPad;
                            NumKeyPadView numKeyPadView = (NumKeyPadView) ViewBindings.findChildViewById(view, R.id.numKeyPad);
                            if (numKeyPadView != null) {
                                i = R.id.numType;
                                Button button5 = (Button) ViewBindings.findChildViewById(view, R.id.numType);
                                if (button5 != null) {
                                    i = R.id.save;
                                    Button button6 = (Button) ViewBindings.findChildViewById(view, R.id.save);
                                    if (button6 != null) {
                                        i = R.id.tvChannel;
                                        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.tvChannel);
                                        if (textView != null) {
                                            i = R.id.tvIData1;
                                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIData1);
                                            if (textView2 != null) {
                                                i = R.id.tvIData2;
                                                TextView textView3 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIData2);
                                                if (textView3 != null) {
                                                    i = R.id.tvIType;
                                                    TextView textView4 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIType);
                                                    if (textView4 != null) {
                                                        i = R.id.tvMidiInfo;
                                                        TextView textView5 = (TextView) ViewBindings.findChildViewById(view, R.id.tvMidiInfo);
                                                        if (textView5 != null) {
                                                            return new Fc2EditInterfaceMidiInfoLayoutBinding(linearLayout, buttonIconView, button, button2, button3, button4, linearLayout, numKeyPadView, button5, button6, textView, textView2, textView3, textView4, textView5);
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
