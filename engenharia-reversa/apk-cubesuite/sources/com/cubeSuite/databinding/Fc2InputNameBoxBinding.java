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
public final class Fc2InputNameBoxBinding implements ViewBinding {
    public final EditText editName;
    private final LinearLayout rootView;
    public final Button saveBut;

    private Fc2InputNameBoxBinding(LinearLayout linearLayout, EditText editText, Button button) {
        this.rootView = linearLayout;
        this.editName = editText;
        this.saveBut = button;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static Fc2InputNameBoxBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static Fc2InputNameBoxBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fc2_input_name_box, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static Fc2InputNameBoxBinding bind(View view) {
        int i = R.id.edit_name;
        EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.edit_name);
        if (editText != null) {
            i = R.id.save_but;
            Button button = (Button) ViewBindings.findChildViewById(view, R.id.save_but);
            if (button != null) {
                return new Fc2InputNameBoxBinding((LinearLayout) view, editText, button);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
