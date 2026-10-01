package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.SelectItem;

/* JADX INFO: loaded from: classes.dex */
public final class Fc2SelectFootCtrlMode1LayoutBinding implements ViewBinding {
    public final ButtonIconView back;
    public final SelectItem longOnMode;
    public final LinearLayout main;
    public final SelectItem onTheLooseTwoMode;
    private final LinearLayout rootView;
    public final SelectItem singleOrLongOnMode;
    public final SelectItem singleStepOneMode;
    public final SelectItem singleStepTwoMode;

    private Fc2SelectFootCtrlMode1LayoutBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, SelectItem selectItem, LinearLayout linearLayout2, SelectItem selectItem2, SelectItem selectItem3, SelectItem selectItem4, SelectItem selectItem5) {
        this.rootView = linearLayout;
        this.back = buttonIconView;
        this.longOnMode = selectItem;
        this.main = linearLayout2;
        this.onTheLooseTwoMode = selectItem2;
        this.singleOrLongOnMode = selectItem3;
        this.singleStepOneMode = selectItem4;
        this.singleStepTwoMode = selectItem5;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2SelectFootCtrlMode1LayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2SelectFootCtrlMode1LayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_select_foot_ctrl_mode1_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2SelectFootCtrlMode1LayoutBinding bind(View view) {
        int i = R.id.back;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
        if (buttonIconView != null) {
            i = R.id.longOnMode;
            SelectItem selectItem = (SelectItem) ViewBindings.findChildViewById(view, R.id.longOnMode);
            if (selectItem != null) {
                LinearLayout linearLayout = (LinearLayout) view;
                i = R.id.onTheLooseTwoMode;
                SelectItem selectItem2 = (SelectItem) ViewBindings.findChildViewById(view, R.id.onTheLooseTwoMode);
                if (selectItem2 != null) {
                    i = R.id.singleOrLongOnMode;
                    SelectItem selectItem3 = (SelectItem) ViewBindings.findChildViewById(view, R.id.singleOrLongOnMode);
                    if (selectItem3 != null) {
                        i = R.id.singleStepOneMode;
                        SelectItem selectItem4 = (SelectItem) ViewBindings.findChildViewById(view, R.id.singleStepOneMode);
                        if (selectItem4 != null) {
                            i = R.id.singleStepTwoMode;
                            SelectItem selectItem5 = (SelectItem) ViewBindings.findChildViewById(view, R.id.singleStepTwoMode);
                            if (selectItem5 != null) {
                                return new Fc2SelectFootCtrlMode1LayoutBinding(linearLayout, buttonIconView, selectItem, linearLayout, selectItem2, selectItem3, selectItem4, selectItem5);
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
