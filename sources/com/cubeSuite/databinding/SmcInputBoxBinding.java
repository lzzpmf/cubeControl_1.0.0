package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class SmcInputBoxBinding implements ViewBinding {
    private final LinearLayout rootView;
    public final EditText smcInput;
    public final Button smcSender;

    private SmcInputBoxBinding(LinearLayout linearLayout, EditText editText, Button button) {
        this.rootView = linearLayout;
        this.smcInput = editText;
        this.smcSender = button;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SmcInputBoxBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SmcInputBoxBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.smc_input_box, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SmcInputBoxBinding bind(View view) {
        int i = R.id.smc_input;
        EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.smc_input);
        if (editText != null) {
            i = R.id.smc_sender;
            Button button = (Button) ViewBindings.findChildViewById(view, R.id.smc_sender);
            if (button != null) {
                return new SmcInputBoxBinding((LinearLayout) view, editText, button);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
