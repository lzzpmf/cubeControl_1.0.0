package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioGroup;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.NumKeyPadView;

/* JADX INFO: loaded from: classes.dex */
public final class EditSendMidiInfoLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final Button cancel;
    public final Button channel;
    public final Button data1;
    public final Button data2;
    public final EditText etCustomMidiCode;
    public final LinearLayout main;
    public final NumKeyPadView numKeyPad;
    public final Button numType;
    private final LinearLayout rootView;
    public final Button save;
    public final TextView tvChannel;
    public final TextView tvCustomMidiCode;
    public final TextView tvData1;
    public final TextView tvData2;
    public final TextView tvIData1;
    public final TextView tvIData2;
    public final TextView tvIType;
    public final TextView tvMidiInfo;
    public final TextView tvType;
    public final RadioGroup type;

    private EditSendMidiInfoLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, Button button, Button button2, Button button3, Button button4, EditText editText, LinearLayout linearLayout2, NumKeyPadView numKeyPadView, Button button5, Button button6, TextView textView, TextView textView2, TextView textView3, TextView textView4, TextView textView5, TextView textView6, TextView textView7, TextView textView8, TextView textView9, RadioGroup radioGroup) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.cancel = button;
        this.channel = button2;
        this.data1 = button3;
        this.data2 = button4;
        this.etCustomMidiCode = editText;
        this.main = linearLayout2;
        this.numKeyPad = numKeyPadView;
        this.numType = button5;
        this.save = button6;
        this.tvChannel = textView;
        this.tvCustomMidiCode = textView2;
        this.tvData1 = textView3;
        this.tvData2 = textView4;
        this.tvIData1 = textView5;
        this.tvIData2 = textView6;
        this.tvIType = textView7;
        this.tvMidiInfo = textView8;
        this.tvType = textView9;
        this.type = radioGroup;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static EditSendMidiInfoLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static EditSendMidiInfoLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.edit_send_midi_info_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static EditSendMidiInfoLayoutBinding bind(View view) {
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
                            i = R.id.etCustomMidiCode;
                            EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.etCustomMidiCode);
                            if (editText != null) {
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
                                                i = R.id.tvCustomMidiCode;
                                                TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.tvCustomMidiCode);
                                                if (textView2 != null) {
                                                    i = R.id.tvData1;
                                                    TextView textView3 = (TextView) ViewBindings.findChildViewById(view, R.id.tvData1);
                                                    if (textView3 != null) {
                                                        i = R.id.tvData2;
                                                        TextView textView4 = (TextView) ViewBindings.findChildViewById(view, R.id.tvData2);
                                                        if (textView4 != null) {
                                                            i = R.id.tvIData1;
                                                            TextView textView5 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIData1);
                                                            if (textView5 != null) {
                                                                i = R.id.tvIData2;
                                                                TextView textView6 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIData2);
                                                                if (textView6 != null) {
                                                                    i = R.id.tvIType;
                                                                    TextView textView7 = (TextView) ViewBindings.findChildViewById(view, R.id.tvIType);
                                                                    if (textView7 != null) {
                                                                        i = R.id.tvMidiInfo;
                                                                        TextView textView8 = (TextView) ViewBindings.findChildViewById(view, R.id.tvMidiInfo);
                                                                        if (textView8 != null) {
                                                                            i = R.id.tvType;
                                                                            TextView textView9 = (TextView) ViewBindings.findChildViewById(view, R.id.tvType);
                                                                            if (textView9 != null) {
                                                                                i = R.id.type;
                                                                                RadioGroup radioGroup = (RadioGroup) ViewBindings.findChildViewById(view, R.id.type);
                                                                                if (radioGroup != null) {
                                                                                    return new EditSendMidiInfoLayoutBinding(linearLayout, buttonIconView, button, button2, button3, button4, editText, linearLayout, numKeyPadView, button5, button6, textView, textView2, textView3, textView4, textView5, textView6, textView7, textView8, textView9, radioGroup);
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
