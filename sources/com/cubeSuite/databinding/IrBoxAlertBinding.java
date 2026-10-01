package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class IrBoxAlertBinding implements ViewBinding {
    public final EditText editName;
    public final AppCompatSpinner nameSpinner;
    private final LinearLayout rootView;
    public final Button saveBut;

    private IrBoxAlertBinding(LinearLayout linearLayout, EditText editText, AppCompatSpinner appCompatSpinner, Button button) {
        this.rootView = linearLayout;
        this.editName = editText;
        this.nameSpinner = appCompatSpinner;
        this.saveBut = button;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static IrBoxAlertBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static IrBoxAlertBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.ir_box_alert, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static IrBoxAlertBinding bind(View view) {
        int i = R.id.edit_name;
        EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.edit_name);
        if (editText != null) {
            i = R.id.name_spinner;
            AppCompatSpinner appCompatSpinner = (AppCompatSpinner) ViewBindings.findChildViewById(view, R.id.name_spinner);
            if (appCompatSpinner != null) {
                i = R.id.save_but;
                Button button = (Button) ViewBindings.findChildViewById(view, R.id.save_but);
                if (button != null) {
                    return new IrBoxAlertBinding((LinearLayout) view, editText, appCompatSpinner, button);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
