package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class ByAppOpenFileActivityBinding implements ViewBinding {
    public final TextView effectUnitText;
    private final LinearLayout rootView;

    private ByAppOpenFileActivityBinding(LinearLayout linearLayout, TextView textView) {
        this.rootView = linearLayout;
        this.effectUnitText = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ByAppOpenFileActivityBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ByAppOpenFileActivityBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.by_app_open_file_activity, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ByAppOpenFileActivityBinding bind(View view) {
        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.effect_unit_text);
        if (textView != null) {
            return new ByAppOpenFileActivityBinding((LinearLayout) view, textView);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(R.id.effect_unit_text)));
    }
}
