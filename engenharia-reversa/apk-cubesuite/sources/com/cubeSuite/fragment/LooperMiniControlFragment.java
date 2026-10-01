package com.cubeSuite.fragment;

import android.app.ProgressDialog;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.communication.LooperCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.fastble.callback.BleWriteCallback;
import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public class LooperMiniControlFragment extends ControlBaseFragment {
    ControlAndPdfActivity activity;
    private SeekBar gate;
    private TextView gateValue;
    private ProgressDialog progressDialog;
    private ButtonIconView stop;
    private ButtonIconView syncGate;
    private View thisView;
    private SeekBar volume;
    private TextView volumeValue;
    private final BleWriteCallback writeCallback = new BleWriteCallback() { // from class: com.cubeSuite.fragment.LooperMiniControlFragment.3
        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteFailure(BleException bleException) {
        }

        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteSuccess(int i, int i2, byte[] bArr) {
        }
    };

    private void setData(byte[] bArr) {
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.looper_mini_control_fragment, viewGroup, false);
        loadView();
        setEvent();
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

    private void setEvent() {
        this.volume.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.fragment.LooperMiniControlFragment.1
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (z) {
                    LooperMiniControlFragment.this.volumeValue.setText(String.valueOf(i));
                    ControlBaseFragment.communicationUtil.writeDataToBleNotRes(LooperCommunication.looperProSetVolume(i));
                }
            }
        });
        this.gate.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.fragment.LooperMiniControlFragment.2
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (z) {
                    LooperMiniControlFragment.this.gateValue.setText(String.valueOf(i));
                    ControlBaseFragment.communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniSetGate(i));
                }
            }
        });
        this.stop.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.LooperMiniControlFragment$$ExternalSyntheticLambda0
            @Override // android.view.View.OnTouchListener
            public final boolean onTouch(View view, MotionEvent motionEvent) {
                return LooperMiniControlFragment.lambda$setEvent$0(view, motionEvent);
            }
        });
        this.syncGate.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.LooperMiniControlFragment$$ExternalSyntheticLambda1
            @Override // android.view.View.OnTouchListener
            public final boolean onTouch(View view, MotionEvent motionEvent) {
                return LooperMiniControlFragment.lambda$setEvent$1(view, motionEvent);
            }
        });
    }

    static /* synthetic */ boolean lambda$setEvent$0(View view, MotionEvent motionEvent) {
        if (motionEvent.getAction() == 0) {
            communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniStop(127));
        }
        if (motionEvent.getAction() == 1) {
            communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniStop(0));
        }
        return false;
    }

    static /* synthetic */ boolean lambda$setEvent$1(View view, MotionEvent motionEvent) {
        if (motionEvent.getAction() == 0) {
            communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniSync(127));
        }
        if (motionEvent.getAction() == 1) {
            communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniSync(0));
        }
        return false;
    }

    private void loadView() {
        this.volume = (SeekBar) this.thisView.findViewById(R.id.volume);
        this.gate = (SeekBar) this.thisView.findViewById(R.id.gate);
        this.volumeValue = (TextView) this.thisView.findViewById(R.id.volumeValue);
        this.gateValue = (TextView) this.thisView.findViewById(R.id.gateValue);
        this.stop = (ButtonIconView) this.thisView.findViewById(R.id.stop);
        this.syncGate = (ButtonIconView) this.thisView.findViewById(R.id.syncGate);
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        this.progressDialog.dismiss();
        super.onDestroy();
    }
}
