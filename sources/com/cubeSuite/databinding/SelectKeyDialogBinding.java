package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.GridLayout;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.TextIconView;

/* JADX INFO: loaded from: classes.dex */
public final class SelectKeyDialogBinding implements ViewBinding {
    public final Button close;
    public final GridLayout contentList;
    private final LinearLayout rootView;
    public final TextIconView title;

    private SelectKeyDialogBinding(LinearLayout linearLayout, Button button, GridLayout gridLayout, TextIconView textIconView) {
        this.rootView = linearLayout;
        this.close = button;
        this.contentList = gridLayout;
        this.title = textIconView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static SelectKeyDialogBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SelectKeyDialogBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.select_key_dialog, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SelectKeyDialogBinding bind(View view) {
        int i = R.id.close;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.close);
        if (button != null) {
            i = R.id.contentList;
            GridLayout gridLayout = (GridLayout) ViewBindings.findChildViewById(view, R.id.contentList);
            if (gridLayout != null) {
                i = R.id.title;
                TextIconView textIconView = (TextIconView) ViewBindings.findChildViewById(view, R.id.title);
                if (textIconView != null) {
                    return new SelectKeyDialogBinding((LinearLayout) view, button, gridLayout, textIconView);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
