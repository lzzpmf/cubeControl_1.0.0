package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;

/* JADX INFO: loaded from: classes.dex */
public final class OtaUpgradeFragmentBinding implements ViewBinding {
    public final ButtonIconView detectFirmwareUpgrade;
    public final TextView newVersion;
    private final LinearLayout rootView;

    private OtaUpgradeFragmentBinding(LinearLayout linearLayout, ButtonIconView buttonIconView, TextView textView) {
        this.rootView = linearLayout;
        this.detectFirmwareUpgrade = buttonIconView;
        this.newVersion = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static OtaUpgradeFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static OtaUpgradeFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.ota_upgrade_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static OtaUpgradeFragmentBinding bind(View view) {
        int i = R.id.detect_firmware_upgrade;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.detect_firmware_upgrade);
        if (buttonIconView != null) {
            i = R.id.newVersion;
            TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.newVersion);
            if (textView != null) {
                return new OtaUpgradeFragmentBinding((LinearLayout) view, buttonIconView, textView);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
