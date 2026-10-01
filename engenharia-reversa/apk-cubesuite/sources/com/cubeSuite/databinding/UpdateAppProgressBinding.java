package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class UpdateAppProgressBinding implements ViewBinding {
    public final ProgressBar progress;
    private final LinearLayout rootView;
    public final LinearLayout titleBar;
    public final TextView txtStatus;

    private UpdateAppProgressBinding(LinearLayout linearLayout, ProgressBar progressBar, LinearLayout linearLayout2, TextView textView) {
        this.rootView = linearLayout;
        this.progress = progressBar;
        this.titleBar = linearLayout2;
        this.txtStatus = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static UpdateAppProgressBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static UpdateAppProgressBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.update_app_progress, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static UpdateAppProgressBinding bind(View view) {
        int i = R.id.progress;
        ProgressBar progressBar = (ProgressBar) ViewBindings.findChildViewById(view, R.id.progress);
        if (progressBar != null) {
            i = R.id.titleBar;
            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.titleBar);
            if (linearLayout != null) {
                i = R.id.txtStatus;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.txtStatus);
                if (textView != null) {
                    return new UpdateAppProgressBinding((LinearLayout) view, progressBar, linearLayout, textView);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
