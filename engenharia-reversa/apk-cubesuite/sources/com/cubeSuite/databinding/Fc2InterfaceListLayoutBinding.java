package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class Fc2InterfaceListLayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final ButtonIconView deleteList;
    public final TextView footSwitchText;
    public final LinearLayout main;
    public final LinearLayout midiInfoList;
    public final ScrollView midiInfoListScroll;
    public final ButtonIconView plusMidiCode;
    private final LinearLayout rootView;

    private Fc2InterfaceListLayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, TextView textView, LinearLayout linearLayout2, LinearLayout linearLayout3, ScrollView scrollView, ButtonIconView buttonIconView3) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.deleteList = buttonIconView2;
        this.footSwitchText = textView;
        this.main = linearLayout2;
        this.midiInfoList = linearLayout3;
        this.midiInfoListScroll = scrollView;
        this.plusMidiCode = buttonIconView3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2InterfaceListLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2InterfaceListLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_interface_list_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2InterfaceListLayoutBinding bind(View view) {
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
                    i = R.id.midiInfoList;
                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.midiInfoList);
                    if (linearLayout2 != null) {
                        i = R.id.midiInfoListScroll;
                        ScrollView scrollView = (ScrollView) ViewBindings.findChildViewById(view, R.id.midiInfoListScroll);
                        if (scrollView != null) {
                            i = R.id.plusMidiCode;
                            ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.plusMidiCode);
                            if (buttonIconView3 != null) {
                                return new Fc2InterfaceListLayoutBinding(linearLayout, buttonIconView, buttonIconView2, textView, linearLayout, linearLayout2, scrollView, buttonIconView3);
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
