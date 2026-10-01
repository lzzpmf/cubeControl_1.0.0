package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class CustormAlertDialogBinding implements ViewBinding {
    public final Button dialogCancel;
    public final Button dialogConfirm;
    public final TextView dialogContent;
    public final TextView dialogTitle;
    public final LinearLayout linear;
    private final LinearLayout rootView;

    private CustormAlertDialogBinding(LinearLayout linearLayout, Button button, Button button2, TextView textView, TextView textView2, LinearLayout linearLayout2) {
        this.rootView = linearLayout;
        this.dialogCancel = button;
        this.dialogConfirm = button2;
        this.dialogContent = textView;
        this.dialogTitle = textView2;
        this.linear = linearLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static CustormAlertDialogBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static CustormAlertDialogBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.custorm_alert_dialog, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static CustormAlertDialogBinding bind(View view) {
        int i = R.id.dialog_cancel;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.dialog_cancel);
        if (button != null) {
            i = R.id.dialog_confirm;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.dialog_confirm);
            if (button2 != null) {
                i = R.id.dialog_content;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.dialog_content);
                if (textView != null) {
                    i = R.id.dialog_title;
                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.dialog_title);
                    if (textView2 != null) {
                        i = R.id.linear;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.linear);
                        if (linearLayout != null) {
                            return new CustormAlertDialogBinding((LinearLayout) view, button, button2, textView, textView2, linearLayout);
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
