package com.cubeSuite.fragment.midiPort;

import android.app.ProgressDialog;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.Communication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.fragment.ControlBaseFragment;

/* JADX INFO: loaded from: classes.dex */
public class MidiPortFragment extends ControlBaseFragment {
    ControlAndPdfActivity activity;
    private Button deviceOnlyMode;
    private Button hostOnlyMode;
    private Button otgMode;
    private ProgressDialog progressDialog;
    private View thisView;

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.midi_portb_fragment, viewGroup, false);
        this.activity = (ControlAndPdfActivity) getActivity();
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(this.thisView.getContext());
            this.progressDialog = progressDialog;
            progressDialog.setMessage(getString(R.string.bt_connecting));
        }
        this.otgMode = (Button) this.thisView.findViewById(R.id.otgMode);
        this.hostOnlyMode = (Button) this.thisView.findViewById(R.id.hostOnlyMode);
        this.deviceOnlyMode = (Button) this.thisView.findViewById(R.id.deviceOnlyMode);
        this.otgMode.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m265x65afc011(view);
            }
        });
        this.deviceOnlyMode.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m266x57596630(view);
            }
        });
        this.hostOnlyMode.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m267x49030c4f(view);
            }
        });
        return this.thisView;
    }

    /* JADX INFO: renamed from: lambda$onCreateView$0$com-cubeSuite-fragment-midiPort-MidiPortFragment, reason: not valid java name */
    /* synthetic */ void m265x65afc011(View view) {
        AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setContent(R.string.otgModeSwitching).showDialog();
        communicationUtil.writeDataToBle(Communication.makeWritePacketByType((byte) 5, new byte[]{0}, 0L), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment.1
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchSuccess).showDialog();
                } else {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchFail).showDialog();
                }
            }
        });
    }

    /* JADX INFO: renamed from: lambda$onCreateView$1$com-cubeSuite-fragment-midiPort-MidiPortFragment, reason: not valid java name */
    /* synthetic */ void m266x57596630(View view) {
        AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setContent(R.string.otgModeSwitching).showDialog();
        communicationUtil.writeDataToBle(Communication.makeWritePacketByType((byte) 5, new byte[]{2}, 0L), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchSuccess).showDialog();
                } else {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchFail).showDialog();
                }
            }
        });
    }

    /* JADX INFO: renamed from: lambda$onCreateView$2$com-cubeSuite-fragment-midiPort-MidiPortFragment, reason: not valid java name */
    /* synthetic */ void m267x49030c4f(View view) {
        AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setContent(R.string.hostOnlyModeSwitching).showDialog();
        communicationUtil.writeDataToBle(Communication.makeWritePacketByType((byte) 5, new byte[]{1}, 0L), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment.3
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchSuccess).showDialog();
                } else {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).setContent(R.string.switchFail).showDialog();
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        communicationUtil.splitReadData((byte) 5, 0L, 1, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midiPort.MidiPortFragment.4
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    AlertDialogUtil.getInstance(MidiPortFragment.this.activity).closeDialog();
                    byte b = bArr[0];
                    if (b == 0) {
                        MidiPortFragment.this.otgMode.setEnabled(false);
                        MidiPortFragment.this.hostOnlyMode.setEnabled(true);
                        MidiPortFragment.this.deviceOnlyMode.setEnabled(true);
                    } else if (b == 1) {
                        MidiPortFragment.this.hostOnlyMode.setEnabled(false);
                        MidiPortFragment.this.otgMode.setEnabled(true);
                        MidiPortFragment.this.deviceOnlyMode.setEnabled(true);
                    } else if (b == 2) {
                        MidiPortFragment.this.deviceOnlyMode.setEnabled(false);
                        MidiPortFragment.this.hostOnlyMode.setEnabled(true);
                        MidiPortFragment.this.otgMode.setEnabled(true);
                    }
                }
            }
        });
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
