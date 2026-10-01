package com.cubeSuite.fragment;

import android.app.ProgressDialog;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.customControl.AlertDialogUtil;

/* JADX INFO: loaded from: classes.dex */
public class EmptyFragment extends ControlBaseFragment {
    ControlAndPdfActivity activity;
    private ProgressDialog progressDialog;
    private View thisView;

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.empty_fragment, viewGroup, false);
        this.activity = (ControlAndPdfActivity) getActivity();
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(this.thisView.getContext());
            this.progressDialog = progressDialog;
            progressDialog.setMessage(getString(R.string.bt_connecting));
        }
        return this.thisView;
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        AlertDialogUtil.getInstance(this.activity).closeDialog();
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        this.progressDialog.dismiss();
        super.onDestroy();
    }
}
