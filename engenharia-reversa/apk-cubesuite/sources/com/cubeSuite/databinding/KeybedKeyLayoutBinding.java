package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.appcompat.widget.SwitchCompat;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class KeybedKeyLayoutBinding implements ViewBinding {
    public final TextView index;
    public final SwitchCompat mpcSwitch;
    public final TextView mpcText;
    public final EditText mpcValue;
    private final LinearLayout rootView;

    private KeybedKeyLayoutBinding(LinearLayout linearLayout, TextView textView, SwitchCompat switchCompat, TextView textView2, EditText editText) {
        this.rootView = linearLayout;
        this.index = textView;
        this.mpcSwitch = switchCompat;
        this.mpcText = textView2;
        this.mpcValue = editText;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static KeybedKeyLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static KeybedKeyLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.keybed_key_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static KeybedKeyLayoutBinding bind(View view) {
        int i = R.id.index;
        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.index);
        if (textView != null) {
            i = R.id.mpcSwitch;
            SwitchCompat switchCompat = (SwitchCompat) ViewBindings.findChildViewById(view, R.id.mpcSwitch);
            if (switchCompat != null) {
                i = R.id.mpcText;
                TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.mpcText);
                if (textView2 != null) {
                    i = R.id.mpcValue;
                    EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.mpcValue);
                    if (editText != null) {
                        return new KeybedKeyLayoutBinding((LinearLayout) view, textView, switchCompat, textView2, editText);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
