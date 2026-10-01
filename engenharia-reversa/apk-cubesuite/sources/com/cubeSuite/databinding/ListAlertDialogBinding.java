package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.Switch;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.TextIconView;

/* JADX INFO: loaded from: classes.dex */
public final class ListAlertDialogBinding implements ViewBinding {
    public final Button close;
    public final TextIconView dialogTitle;
    public final RecyclerView listAlertRecycler;
    private final LinearLayout rootView;
    public final Switch toggle;

    private ListAlertDialogBinding(LinearLayout linearLayout, Button button, TextIconView textIconView, RecyclerView recyclerView, Switch r5) {
        this.rootView = linearLayout;
        this.close = button;
        this.dialogTitle = textIconView;
        this.listAlertRecycler = recyclerView;
        this.toggle = r5;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ListAlertDialogBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ListAlertDialogBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.list_alert_dialog, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ListAlertDialogBinding bind(View view) {
        int i = R.id.close;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.close);
        if (button != null) {
            i = R.id.dialog_title;
            TextIconView textIconView = (TextIconView) ViewBindings.findChildViewById(view, R.id.dialog_title);
            if (textIconView != null) {
                i = R.id.list_alert_recycler;
                RecyclerView recyclerView = (RecyclerView) ViewBindings.findChildViewById(view, R.id.list_alert_recycler);
                if (recyclerView != null) {
                    i = R.id.toggle;
                    Switch r7 = (Switch) ViewBindings.findChildViewById(view, R.id.toggle);
                    if (r7 != null) {
                        return new ListAlertDialogBinding((LinearLayout) view, button, textIconView, recyclerView, r7);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
