package com.cubeSuite.fragment;

import android.app.ProgressDialog;
import android.content.Context;
import android.graphics.Color;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.ImageView;
import android.widget.TextView;
import com.contrarywind.listener.OnItemSelectedListener;
import com.contrarywind.view.WheelView;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.adapter.other.ArrayWheelAdapter;
import com.cubeSuite.adapter.other.BluetoothPedalMIDICCSelectAdapter;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.CubeTurnerCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.ListAlertDialog;
import com.cubeSuite.customControl.TextIconView;
import com.fastble.callback.BleWriteCallback;
import com.fastble.exception.BleException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class CubeTurnerControlFragment extends ControlBaseFragment {
    ControlAndPdfActivity activity;
    private View customKeypadContainer;
    private TextIconView hardwareIcon;
    private ButtonIconView midiChannelMinus;
    private ButtonIconView midiChannelPlus;
    private WheelView modeSelect;
    private ProgressDialog progressDialog;
    private TextView sendMidiChannel;
    private View thisView;
    int topImage;
    private TextView tvModelExplain;
    private final Button[] customKeypad = new Button[2];
    private final List<String> modeItems = new ArrayList();
    private int[] customKeypadSelect = {0, 0};
    private int[] toggleSelect = {0, 0};
    private final ListAlertDialog[] customKeypadList = new ListAlertDialog[2];
    int midiChannelValue = 0;
    private final int[] modelsTextExplain = {R.string.tTouchScreenModeExplain, R.string.tKeyboardModeAExplain, R.string.tKeyboardModeBExplain, R.string.tMultimediaKeyModeExplain, R.string.tManufacturerEquipmentControlModeExplain, R.string.tCustomModeExplain};
    private final BleWriteCallback writeCallback = new BleWriteCallback() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment.3
        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteFailure(BleException bleException) {
        }

        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteSuccess(int i, int i2, byte[] bArr) {
        }
    };

    public CubeTurnerControlFragment(int i) {
        this.topImage = i;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.cube_turner_contorl_fragment, viewGroup, false);
        loadView();
        initScrollPicker();
        setClick();
        this.activity = (ControlAndPdfActivity) getActivity();
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(this.thisView.getContext());
            this.progressDialog = progressDialog;
            progressDialog.setMessage(getString(R.string.bt_connecting));
        }
        ((ImageView) this.thisView.findViewById(R.id.imageView)).setImageDrawable(((Context) Objects.requireNonNull(getContext())).getDrawable(this.topImage));
        return this.thisView;
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        communicationUtil.readData(CubeTurnerCommunication.cubeTurnerReadCurrentModel(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment.1
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    AlertDialogUtil.getInstance(CubeTurnerControlFragment.this.activity).closeDialog();
                    CubeTurnerControlFragment.this.setData(bArr);
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }

    public void showButtonCustomFunction(final int i) {
        String[] strArr = Global.CubeTurnerCustomCommand.MIDICC_STR;
        final String[] strArr2 = Global.CubeTurnerCustomCommand.CUSTOM_COMMAND_STR;
        int length = strArr.length;
        ListAlertDialog listAlertDialog = this.customKeypadList[i];
        if (listAlertDialog == null) {
            int[] iArr = this.customKeypadSelect;
            if (iArr[i] > length) {
                iArr[i] = length;
            }
            BluetoothPedalMIDICCSelectAdapter bluetoothPedalMIDICCSelectAdapter = new BluetoothPedalMIDICCSelectAdapter(this.customKeypadSelect[i], Arrays.asList(strArr), Arrays.asList(strArr2));
            bluetoothPedalMIDICCSelectAdapter.setHasStableIds(true);
            bluetoothPedalMIDICCSelectAdapter.setItemClick(new Callback.HolderItemClick() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment$$ExternalSyntheticLambda0
                @Override // com.cubeSuite.callback.Callback.HolderItemClick
                public final void click(int i2) {
                    this.f$0.m131xa5de87ea(i, strArr2, i2);
                }
            });
            ListAlertDialog listAlertDialog2 = new ListAlertDialog(this.thisView.getContext());
            if (i == 0) {
                this.customKeypadList[i] = listAlertDialog2.setTitle(R.string.left).setContent(bluetoothPedalMIDICCSelectAdapter).showDialog();
            } else if (i == 1) {
                this.customKeypadList[i] = listAlertDialog2.setTitle(R.string.right).setContent(bluetoothPedalMIDICCSelectAdapter).showDialog();
            }
            this.customKeypadList[i].setToggleChecked(this.toggleSelect[i] == 1);
            this.customKeypadList[i].setToggleClick(new ListAlertDialog.ToggleClick() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment$$ExternalSyntheticLambda1
                @Override // com.cubeSuite.customControl.ListAlertDialog.ToggleClick
                public final void toggleClick(CompoundButton compoundButton, boolean z) {
                    CubeTurnerControlFragment.communicationUtil.writeDataToBle(CubeTurnerCommunication.cubeTurnerWriteToggleData(z ? 1 : 0, i));
                }
            });
            return;
        }
        listAlertDialog.showDialog();
    }

    /* JADX INFO: renamed from: lambda$showButtonCustomFunction$0$com-cubeSuite-fragment-CubeTurnerControlFragment, reason: not valid java name */
    /* synthetic */ void m131xa5de87ea(int i, String[] strArr, int i2) {
        this.customKeypadSelect[i] = i2;
        communicationUtil.writeDataToBle(CubeTurnerCommunication.cubeTurnerWriteCustomData(i2, i));
        this.customKeypad[i].setText(strArr[i2]);
    }

    private void initScrollPicker() {
        this.modeItems.add(getString(R.string.touchScreenMode));
        this.modeItems.add(getString(R.string.keyboardModeA));
        this.modeItems.add(getString(R.string.keyboardModeB));
        this.modeItems.add(getString(R.string.multimediaKeyMode));
        this.modeItems.add(getString(R.string.customMode));
        this.modeSelect = (WheelView) this.thisView.findViewById(R.id.modeSelect);
        setWheelView();
    }

    private void setWheelView() {
        this.modeSelect.setItemsVisibleCount(5);
        this.modeSelect.setTextSize(14.0f);
        this.modeSelect.setCyclic(false);
        this.modeSelect.setTextColorCenter(Color.rgb(255, 255, 255));
        this.modeSelect.setTextColorOut(Color.rgb(51, 234, 184));
        this.modeSelect.setDividerColor(Color.rgb(51, 234, 184));
        this.modeSelect.setAdapter(new ArrayWheelAdapter(this.modeItems));
        this.modeSelect.setOnItemSelectedListener(new OnItemSelectedListener() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment.2
            @Override // com.contrarywind.listener.OnItemSelectedListener
            public void onItemSelected(int i) {
                if (i == CubeTurnerControlFragment.this.modeItems.size() - 1) {
                    CubeTurnerControlFragment.this.customKeypadContainer.setVisibility(0);
                } else {
                    CubeTurnerControlFragment.this.customKeypadContainer.setVisibility(4);
                }
                if (i >= CubeTurnerControlFragment.this.modeItems.size() - 1) {
                    i++;
                }
                ControlBaseFragment.communicationUtil.writeDataToBle(CubeTurnerCommunication.cubeTurnerWriteModel((byte) i));
                CubeTurnerControlFragment.this.tvModelExplain.setText(CubeTurnerControlFragment.this.modelsTextExplain[i]);
            }
        });
    }

    private void setClick() {
        final int i = 0;
        while (true) {
            Button[] buttonArr = this.customKeypad;
            if (i < buttonArr.length) {
                buttonArr[i].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment$$ExternalSyntheticLambda2
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m128xa8872dfa(i, view);
                    }
                });
                i++;
            } else {
                this.midiChannelPlus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment$$ExternalSyntheticLambda3
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m129x9a30d419(view);
                    }
                });
                this.midiChannelMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.CubeTurnerControlFragment$$ExternalSyntheticLambda4
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m130x8bda7a38(view);
                    }
                });
                return;
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setClick$2$com-cubeSuite-fragment-CubeTurnerControlFragment, reason: not valid java name */
    /* synthetic */ void m128xa8872dfa(int i, View view) {
        showButtonCustomFunction(i);
    }

    /* JADX INFO: renamed from: lambda$setClick$3$com-cubeSuite-fragment-CubeTurnerControlFragment, reason: not valid java name */
    /* synthetic */ void m129x9a30d419(View view) {
        int i = this.midiChannelValue;
        if (i < 15) {
            this.midiChannelValue = i + 1;
            communicationUtil.writeDataToBle(CubeTurnerCommunication.cubeTurnerWriteChannelValue((byte) this.midiChannelValue));
            this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        }
    }

    /* JADX INFO: renamed from: lambda$setClick$4$com-cubeSuite-fragment-CubeTurnerControlFragment, reason: not valid java name */
    /* synthetic */ void m130x8bda7a38(View view) {
        int i = this.midiChannelValue;
        if (i > 0) {
            this.midiChannelValue = i - 1;
            communicationUtil.writeDataToBle(CubeTurnerCommunication.cubeTurnerWriteChannelValue((byte) this.midiChannelValue));
            this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        }
    }

    private void loadView() {
        this.tvModelExplain = (TextView) this.thisView.findViewById(R.id.tvModelExplain);
        this.sendMidiChannel = (TextView) this.thisView.findViewById(R.id.sendMidiChannel);
        this.midiChannelMinus = (ButtonIconView) this.thisView.findViewById(R.id.midiChannelMinus);
        this.midiChannelPlus = (ButtonIconView) this.thisView.findViewById(R.id.midiChannelPlus);
        this.customKeypadContainer = this.thisView.findViewById(R.id.customKeypadContainer);
        this.hardwareIcon = (TextIconView) this.thisView.findViewById(R.id.hardware);
        this.customKeypad[0] = (Button) this.thisView.findViewById(R.id.customKeypad1);
        this.customKeypad[1] = (Button) this.thisView.findViewById(R.id.customKeypad2);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setData(byte[] bArr) {
        Map<String, Object> mapPasingCubeTurnerModelData = CubeTurnerCommunication.pasingCubeTurnerModelData(bArr);
        this.hardwareIcon.setText(CubeTurnerCommunication.hardware == 0 ? R.string.aLeft : R.string.aRight);
        this.midiChannelValue = ((Integer) mapPasingCubeTurnerModelData.get("midiChannel")).intValue();
        int iIntValue = ((Integer) mapPasingCubeTurnerModelData.get("mode")).intValue();
        this.modeSelect.setCurrentItem(iIntValue >= this.modeItems.size() - 1 ? iIntValue - 1 : iIntValue);
        this.tvModelExplain.setText(this.modelsTextExplain[iIntValue]);
        if (iIntValue == this.modeItems.size()) {
            this.customKeypadContainer.setVisibility(0);
        } else {
            this.customKeypadContainer.setVisibility(4);
        }
        this.customKeypadSelect = (int[]) mapPasingCubeTurnerModelData.get("typeArr");
        this.toggleSelect = (int[]) mapPasingCubeTurnerModelData.get("toggleArr");
        this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        int i = 0;
        while (true) {
            Button[] buttonArr = this.customKeypad;
            if (i >= buttonArr.length) {
                return;
            }
            if (i == buttonArr.length - 1) {
                buttonArr[i].setText(Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR_KNOB[this.customKeypadSelect[i]]);
            } else {
                ListAlertDialog listAlertDialog = this.customKeypadList[i];
                if (listAlertDialog != null) {
                    listAlertDialog.setToggleChecked(this.toggleSelect[i] == 1);
                }
                this.customKeypad[i].setText(Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR[this.customKeypadSelect[i]]);
            }
            i++;
        }
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        this.progressDialog.dismiss();
        super.onDestroy();
    }
}
