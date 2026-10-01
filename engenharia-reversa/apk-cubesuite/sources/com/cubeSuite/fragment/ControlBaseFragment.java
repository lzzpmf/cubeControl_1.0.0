package com.cubeSuite.fragment;

import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;

/* JADX INFO: loaded from: classes.dex */
public abstract class ControlBaseFragment extends Fragment {
    public static CommunicationUtil communicationUtil;
    protected BaseDeviceActivity activity;

    public abstract BaseDeviceActivity getBaseActivity();

    public abstract void initData();

    public void initialize() {
        this.activity = getBaseActivity();
        if (communicationUtil == null) {
            communicationUtil = new CommunicationUtil();
        }
        communicationUtil.connect(new Callback.OpenNotify() { // from class: com.cubeSuite.fragment.ControlBaseFragment$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.callback.Callback.OpenNotify
            public final void openStatus(boolean z) {
                this.f$0.m127lambda$initialize$0$comcubeSuitefragmentControlBaseFragment(z);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$initialize$0$com-cubeSuite-fragment-ControlBaseFragment, reason: not valid java name */
    /* synthetic */ void m127lambda$initialize$0$comcubeSuitefragmentControlBaseFragment(boolean z) {
        if (!z) {
            AlertDialogUtil.getInstance(this.activity).showDialog().setTitle(R.string.error).setContent(R.string.connect_fail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
            return;
        }
        try {
            Thread.sleep(3000L);
            initData();
        } catch (Exception unused) {
        }
    }
}
