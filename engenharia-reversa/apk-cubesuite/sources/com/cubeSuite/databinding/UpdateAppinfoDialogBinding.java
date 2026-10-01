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
public final class UpdateAppinfoDialogBinding implements ViewBinding {
    public final Button cancel;
    public final Button confirm;
    public final LinearLayout content;
    private final LinearLayout rootView;
    public final TextView title;

    private UpdateAppinfoDialogBinding(LinearLayout linearLayout, Button button, Button button2, LinearLayout linearLayout2, TextView textView) {
        this.rootView = linearLayout;
        this.cancel = button;
        this.confirm = button2;
        this.content = linearLayout2;
        this.title = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static UpdateAppinfoDialogBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static UpdateAppinfoDialogBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.update_appinfo_dialog, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static UpdateAppinfoDialogBinding bind(View view) {
        int i = R.id.cancel;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.cancel);
        if (button != null) {
            i = R.id.confirm;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.confirm);
            if (button2 != null) {
                i = R.id.content;
                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.content);
                if (linearLayout != null) {
                    i = R.id.title;
                    TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.title);
                    if (textView != null) {
                        return new UpdateAppinfoDialogBinding((LinearLayout) view, button, button2, linearLayout, textView);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
