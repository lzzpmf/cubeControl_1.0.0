package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class ColorPickerBinding implements ViewBinding {
    public final View colorPicker;
    private final LinearLayout rootView;

    private ColorPickerBinding(LinearLayout linearLayout, View view) {
        this.rootView = linearLayout;
        this.colorPicker = view;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ColorPickerBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ColorPickerBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.color_picker, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ColorPickerBinding bind(View view) {
        View viewFindChildViewById = ViewBindings.findChildViewById(view, R.id.color_picker);
        if (viewFindChildViewById != null) {
            return new ColorPickerBinding((LinearLayout) view, viewFindChildViewById);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(R.id.color_picker)));
    }
}
