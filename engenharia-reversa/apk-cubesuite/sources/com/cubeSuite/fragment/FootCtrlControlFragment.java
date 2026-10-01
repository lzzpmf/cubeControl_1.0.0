package com.cubeSuite.fragment;

import android.app.ProgressDialog;
import android.graphics.Color;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.ScrollView;
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
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.communication.FootCtrlCommunication;
import com.cubeSuite.communication.JNIUtils;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.ListAlertDialog;
import com.cubeSuite.customControl.NumKeyPadView;
import com.cubeSuite.customControl.SelectItem;
import com.cubeSuite.customControl.SelectKeyDialog;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.footCtrl.CustomData;
import com.cubeSuite.entity.footCtrl.FootCtrlEntry;
import com.cubeSuite.entity.footCtrl.MidiCode;
import com.cubeSuite.utils.AppUtil;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class FootCtrlControlFragment extends ControlBaseFragment {
    public static final String[] MIDI_TYPE = {"PC", "CC", "Note ON", "Note OFF", "SysEx"};
    ControlAndPdfActivity activity;
    private View advancedCustomKeypadContainer;
    private ButtonIconView btnDeleteList;
    private ButtonIconView btnEditMidiBack;
    private Button btnEditMidiCodeBack;
    private Button btnMidiA;
    private Button btnMidiB;
    private Button btnModeSwitch;
    private ButtonIconView btnPlus;
    private ButtonIconView btnSelectModeBack;
    private Button cancel;
    private Button channel;
    private View customKeypadContainer;
    private View customKeypadModeView;
    private Button data1;
    private Button data2;
    private View editMidiCodeView;
    private PopupWindow editMidiCodeWindow;
    private View editMidiContentView;
    private PopupWindow editMidiWindow;
    private EditText etCustomMidiCode;
    private ImageView expressionPedalImage;
    private TextView footSwitchText;
    private PopupWindow mPopWindow;
    private ButtonIconView midiChannelMinus;
    private ButtonIconView midiChannelPlus;
    private LinearLayout midiInfoList;
    private ScrollView midiInfoListScroll;
    private WheelView modeSelect;
    private NumKeyPadView numKeyPad;
    private Button numType;
    private ProgressDialog progressDialog;
    private Button save;
    private SelectKeyDialog selectKeyDialog;
    private View selectModeContentView;
    private PopupWindow selectModeWindow;
    private TextView sendMidiChannel;
    private View thisView;
    private ButtonIconView trsMidiBtn;
    private View trsMidiContentView;
    private ImageView trsMidiImage;
    private PopupWindow trsMidiWindow;
    private TextView tvChannel;
    private TextView tvCustomMidiCode;
    private TextView tvData1;
    private TextView tvData2;
    private TextView tvIData1;
    private TextView tvIData2;
    private TextView tvIType;
    private TextView tvMidiInfo;
    private TextView tvModelExplain;
    private TextView tvType;
    private RadioGroup type;
    private final int SYS_MODE = 4;
    private final int INTERFACE_TYPE = 4;
    private final Button[] footSwitchBtn = new Button[5];
    private final Button[] customKeypadKeyBtn = new Button[4];
    private FootCtrlEntry footCtrlEntry = new FootCtrlEntry();
    private FootCtrlCommunication fcc = new FootCtrlCommunication();
    private final List<String> modeItems = new ArrayList();
    private int currentEditInput = 0;
    int midiChannelValue = 0;
    int trsMidi = 0;
    private int trsMidiChannel = 0;
    private int currentFootSwitch = 4;
    private int groupIndex = 0;
    private final int CHANNEL_NUM = 16;
    private MidiItem[] midiAList = new MidiItem[16];
    private MidiItem[] midiBList = new MidiItem[16];
    private SelectItem[] selectModelItem = new SelectItem[5];
    private final int[] modelsTextExplain = {R.string.programSwitchA1Explain, R.string.programSwitchBExplain, R.string.touchScreenModeExplain, R.string.keyboardModeAExplain, R.string.keyboardModeBExplain, R.string.multimediaKeyModeExplain, R.string.manufacturerEquipmentControlModeExplain, R.string.customModeExplain, R.string.videoModelExplain, R.string.advancedCustomModeExplain, R.string.customKeyboardModeExplain, R.string.programSwitchCExplain};
    private final int[] FOOT_SWITCH_NAME = {R.string.footSwitchA, R.string.footSwitchB, R.string.footSwitchC, R.string.footSwitchD, R.string.padInterface};
    private final Button[] customKeypad = new Button[5];
    private int[] customKeypadSelect = {0, 0, 0, 0, 0};
    private final ListAlertDialog[] customKeypadList = new ListAlertDialog[5];
    private int[] toggleSelect = {0, 0, 0, 0};
    private int currentMidiCode = 0;
    private byte[] modeOrder = {0, 1, 11, 2, 3, 4, 5, 6, 7, 8, 9, 10};
    private byte[] dataToMode = {0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2};

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isInterface() {
        return this.currentFootSwitch == 4;
    }

    public void showButtonCustomFunction(final int i) {
        String[] strArr = Global.BluetoothPedalCustomCommand.MIDICC_STR;
        final String[] strArr2 = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR;
        if (i == 4) {
            strArr = Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB;
            strArr2 = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR_KNOB;
        }
        int length = strArr.length;
        ListAlertDialog listAlertDialog = this.customKeypadList[i];
        if (listAlertDialog == null) {
            int[] iArr = this.customKeypadSelect;
            if (iArr[i] > length) {
                iArr[i] = length;
            }
            BluetoothPedalMIDICCSelectAdapter bluetoothPedalMIDICCSelectAdapter = new BluetoothPedalMIDICCSelectAdapter(this.customKeypadSelect[i], Arrays.asList(strArr), Arrays.asList(strArr2));
            bluetoothPedalMIDICCSelectAdapter.setHasStableIds(true);
            bluetoothPedalMIDICCSelectAdapter.setItemClick(new Callback.HolderItemClick() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda8
                @Override // com.cubeSuite.callback.Callback.HolderItemClick
                public final void click(int i2) {
                    this.f$0.m214x96835630(i, strArr2, i2);
                }
            });
            this.customKeypadList[i] = new ListAlertDialog(this.thisView.getContext()).setTitle(String.valueOf((char) (i + 65))).setContent(bluetoothPedalMIDICCSelectAdapter).showDialog();
            if (i != 4) {
                this.customKeypadList[i].setToggleChecked(this.toggleSelect[i] == 1);
                this.customKeypadList[i].setToggleClick(new ListAlertDialog.ToggleClick() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda9
                    @Override // com.cubeSuite.customControl.ListAlertDialog.ToggleClick
                    public final void toggleClick(CompoundButton compoundButton, boolean z) {
                        this.f$0.m215xd04df80f(i, compoundButton, z);
                    }
                });
                return;
            }
            return;
        }
        listAlertDialog.showDialog();
    }

    /* JADX INFO: renamed from: lambda$showButtonCustomFunction$0$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m214x96835630(int i, String[] strArr, int i2) {
        this.customKeypadSelect[i] = i2;
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteCustomData(i2, i));
        this.customKeypad[i].setText(strArr[i2]);
    }

    /* JADX INFO: renamed from: lambda$showButtonCustomFunction$1$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m215xd04df80f(int i, CompoundButton compoundButton, boolean z) {
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteToggleData(z ? 1 : 0, i));
    }

    private void showKyeCustomDialog(final int i) {
        this.selectKeyDialog.setTitle("FootSwitch[" + (i + 1) + "]").showDialog().setListening(new SelectKeyDialog.Listening() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment.1
            @Override // com.cubeSuite.customControl.SelectKeyDialog.Listening
            public void itemClick(int i2) {
                FootCtrlControlFragment.this.customKeypadKeyBtn[i].setText(FootCtrlControlFragment.this.selectKeyDialog.aztab[i2]);
                ControlBaseFragment.communicationUtil.writeDataToBleNotRes(FootCtrlControlFragment.this.fcc.footCtrlWriteCustomKeyboardMode((byte) i2, i));
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public MidiCode getMidiCode(int i) {
        if (this.groupIndex == 0) {
            return this.footCtrlEntry.getCustomData()[this.currentFootSwitch].getMidiCodeA()[i];
        }
        return this.footCtrlEntry.getCustomData()[this.currentFootSwitch].getMidiCodeB()[i];
    }

    /* JADX INFO: Access modifiers changed from: private */
    public byte[] getSysEx() {
        if (this.groupIndex == 0) {
            return this.footCtrlEntry.getSysExA()[this.currentFootSwitch];
        }
        return this.footCtrlEntry.getSysExB()[this.currentFootSwitch];
    }

    /* JADX INFO: Access modifiers changed from: private */
    class MidiItem {
        private SlidingDeleteItem body;
        private int index;
        private TextView textStr;

        public MidiItem(final int i, final SlidingDeleteItem slidingDeleteItem, final TextView textView) {
            this.index = i;
            this.body = slidingDeleteItem;
            this.textStr = textView;
            FootCtrlControlFragment.this.midiInfoList.addView(slidingDeleteItem);
            textView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$MidiItem$$ExternalSyntheticLambda0
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m222x69292096(i, textView, view);
                }
            });
            slidingDeleteItem.getBtnDelete().setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$MidiItem$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m223x8ebd2997(slidingDeleteItem, i, view);
                }
            });
        }

        /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootCtrlControlFragment$MidiItem, reason: not valid java name */
        /* synthetic */ void m222x69292096(int i, TextView textView, View view) {
            MidiCode midiCode = FootCtrlControlFragment.this.getMidiCode(i);
            FootCtrlControlFragment.this.currentMidiCode = i;
            FootCtrlControlFragment.this.tvMidiInfo.setText(textView.getText());
            if (FootCtrlControlFragment.this.isInterface()) {
                FootCtrlControlFragment.this.numType.setText(((int) midiCode.getType()) + "");
            } else {
                ((RadioButton) FootCtrlControlFragment.this.type.getChildAt(midiCode.getType())).setChecked(true);
            }
            FootCtrlControlFragment.this.setMidiEditViewVisibility();
            FootCtrlControlFragment.this.channel.setText(String.valueOf(midiCode.getChannel() + 1));
            FootCtrlControlFragment.this.data1.setText(String.valueOf((int) midiCode.getData1()));
            FootCtrlControlFragment.this.data2.setText(String.valueOf((int) midiCode.getData2()));
            FootCtrlControlFragment.this.editMidiCodeWindow.showAtLocation(FootCtrlControlFragment.this.thisView, 80, 0, 0);
        }

        /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootCtrlControlFragment$MidiItem, reason: not valid java name */
        /* synthetic */ void m223x8ebd2997(SlidingDeleteItem slidingDeleteItem, int i, View view) {
            slidingDeleteItem.deleteAnimator();
            for (int i2 = 0; i2 < 16; i2++) {
                if (i2 >= i) {
                    MidiCode midiCode = FootCtrlControlFragment.this.getMidiCode(i2);
                    if (i2 != 15) {
                        MidiCode midiCode2 = FootCtrlControlFragment.this.getMidiCode(i2 + 1);
                        if (midiCode2.getIsEnable() == 0) {
                            midiCode.setIsEnable(0);
                            break;
                        }
                        midiCode.setIsEnable(midiCode2.getIsEnable());
                        midiCode.setChannel(midiCode2.getChannel());
                        midiCode.setType(midiCode2.getType());
                        midiCode.setData1(midiCode2.getData1());
                        midiCode.setData2(midiCode2.getData2());
                    } else {
                        midiCode.setIsEnable(0);
                        break;
                    }
                }
            }
            ControlBaseFragment.communicationUtil.writeDataToBle(FootCtrlControlFragment.this.fcc.footCtrlWriteCustomData(FootCtrlControlFragment.this.footCtrlEntry.getCustomData()[FootCtrlControlFragment.this.currentFootSwitch], FootCtrlControlFragment.this.currentFootSwitch));
            FootCtrlControlFragment.this.updateMidiInfo();
        }

        public void updateText() {
            MidiCode midiCode = FootCtrlControlFragment.this.getMidiCode(this.index);
            if (midiCode.getIsEnable() == 0) {
                this.body.setVisibility(8);
            } else {
                this.body.setVisibility(0);
            }
            if (FootCtrlControlFragment.this.isInterface()) {
                this.textStr.setText("[" + (this.index + 1) + "]   " + (midiCode.getChannel() + 1) + "   CC   " + ((int) midiCode.getType()) + "   " + ((int) midiCode.getData1()) + "~" + ((int) midiCode.getData2()));
                return;
            }
            FootCtrlControlFragment footCtrlControlFragment = FootCtrlControlFragment.this;
            if (footCtrlControlFragment.getMidiCode(footCtrlControlFragment.currentMidiCode).getType() == 4) {
                TextView textView = this.textStr;
                StringBuilder sb = new StringBuilder("send:");
                FootCtrlControlFragment footCtrlControlFragment2 = FootCtrlControlFragment.this;
                textView.setText(sb.append(footCtrlControlFragment2.byteToX16String(footCtrlControlFragment2.getSysEx())).toString());
                return;
            }
            this.textStr.setText("[" + (this.index + 1) + "]   " + (midiCode.getChannel() + 1) + "   " + FootCtrlControlFragment.MIDI_TYPE[midiCode.getType()] + "   " + ((int) midiCode.getData1()) + "   " + ((int) midiCode.getData2()));
        }
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.foot_ctrl_contorl_fragment, viewGroup, false);
        this.activity = (ControlAndPdfActivity) getActivity();
        this.selectKeyDialog = new SelectKeyDialog(this.thisView.getContext());
        loadView();
        initScrollPicker();
        setClick();
        initMidiInfo();
        initMidiList();
        initMidiMode();
        initMidiCode();
        updateMidiInfo();
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(this.thisView.getContext());
            this.progressDialog = progressDialog;
            progressDialog.setMessage(getString(R.string.bt_connecting));
        }
        return this.thisView;
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        communicationUtil.splitReadData((byte) 4, 0L, JNIUtils.getFootCtrlSize(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    AlertDialogUtil.getInstance(FootCtrlControlFragment.this.activity).closeDialog();
                    FootCtrlControlFragment.this.setData(bArr);
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }

    private void initScrollPicker() {
        this.modeItems.add(getString(R.string.programSwitchA));
        this.modeItems.add(getString(R.string.programSwitchB));
        this.modeItems.add(getString(R.string.programSwitchC));
        this.modeItems.add(getString(R.string.touchScreenMode));
        this.modeItems.add(getString(R.string.keyboardModeA));
        this.modeItems.add(getString(R.string.keyboardModeB));
        this.modeItems.add(getString(R.string.multimediaKeyMode));
        this.modeItems.add(getString(R.string.manufacturerEquipmentControlMode));
        this.modeItems.add(getString(R.string.customMode));
        this.modeItems.add(getString(R.string.videoModel));
        this.modeItems.add(getString(R.string.advancedCustomMode));
        this.modeItems.add(getString(R.string.customKeyboardMode));
        this.modeSelect = (WheelView) this.thisView.findViewById(R.id.modeSelect);
        setWheelView();
    }

    private void setWheelView() {
        this.modeSelect.setItemsVisibleCount(6);
        this.modeSelect.setTextSize(14.0f);
        this.modeSelect.setCyclic(false);
        this.modeSelect.setTextColorCenter(Color.rgb(255, 255, 255));
        this.modeSelect.setTextColorOut(Color.rgb(51, 234, 184));
        this.modeSelect.setDividerColor(Color.rgb(51, 234, 184));
        this.modeSelect.setAdapter(new ArrayWheelAdapter(this.modeItems));
        this.modeSelect.setOnItemSelectedListener(new OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment.3
            @Override // com.contrarywind.listener.OnItemSelectedListener
            public void onItemSelected(int i) {
                if (FootCtrlControlFragment.this.modeOrder[i] == 7) {
                    FootCtrlControlFragment.this.customKeypadContainer.setVisibility(0);
                } else {
                    FootCtrlControlFragment.this.customKeypadContainer.setVisibility(8);
                }
                if (FootCtrlControlFragment.this.modeOrder[i] == 9) {
                    FootCtrlControlFragment.this.advancedCustomKeypadContainer.setVisibility(0);
                } else {
                    FootCtrlControlFragment.this.advancedCustomKeypadContainer.setVisibility(8);
                }
                if (FootCtrlControlFragment.this.modeOrder[i] == 10) {
                    FootCtrlControlFragment.this.customKeypadModeView.setVisibility(0);
                } else {
                    FootCtrlControlFragment.this.customKeypadModeView.setVisibility(8);
                }
                ControlBaseFragment.communicationUtil.writeDataToBle(FootCtrlControlFragment.this.fcc.bluetoothPedalWriteModel(FootCtrlControlFragment.this.modeOrder[i]));
                FootCtrlControlFragment.this.tvModelExplain.setText(FootCtrlControlFragment.this.modelsTextExplain[FootCtrlControlFragment.this.modeOrder[i]]);
            }
        });
    }

    private void setBankView() {
        int i = 4;
        if (isInterface()) {
            this.btnMidiA.setVisibility(4);
            this.btnMidiB.setVisibility(4);
            this.btnModeSwitch.setVisibility(8);
            return;
        }
        this.btnModeSwitch.setVisibility(0);
        byte mode = this.footCtrlEntry.getCustomData()[this.currentFootSwitch].getMode();
        this.btnModeSwitch.setText(this.selectModelItem[mode].getModeText());
        this.btnMidiA.setVisibility(0);
        Button button = this.btnMidiB;
        if (mode != 3 && mode != 0) {
            i = 0;
        }
        button.setVisibility(i);
    }

    private void setClick() {
        final int i = 0;
        final int i2 = 0;
        while (true) {
            Button[] buttonArr = this.customKeypad;
            if (i2 >= buttonArr.length) {
                break;
            }
            buttonArr[i2].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m207lambda$setClick$2$comcubeSuitefragmentFootCtrlControlFragment(i2, view);
                }
            });
            i2++;
        }
        this.midiChannelPlus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m208lambda$setClick$3$comcubeSuitefragmentFootCtrlControlFragment(view);
            }
        });
        this.midiChannelMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m209lambda$setClick$4$comcubeSuitefragmentFootCtrlControlFragment(view);
            }
        });
        this.trsMidiBtn.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m210lambda$setClick$5$comcubeSuitefragmentFootCtrlControlFragment(view);
            }
        });
        final int i3 = 0;
        while (true) {
            Button[] buttonArr2 = this.footSwitchBtn;
            if (i3 >= buttonArr2.length) {
                break;
            }
            buttonArr2[i3].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda5
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m211lambda$setClick$6$comcubeSuitefragmentFootCtrlControlFragment(i3, view);
                }
            });
            i3++;
        }
        while (true) {
            Button[] buttonArr3 = this.customKeypadKeyBtn;
            if (i < buttonArr3.length) {
                buttonArr3[i].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda6
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m212lambda$setClick$7$comcubeSuitefragmentFootCtrlControlFragment(i, view);
                    }
                });
                i++;
            } else {
                this.trsMidiBtn.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda7
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m213lambda$setClick$8$comcubeSuitefragmentFootCtrlControlFragment(view);
                    }
                });
                return;
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setClick$2$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m207lambda$setClick$2$comcubeSuitefragmentFootCtrlControlFragment(int i, View view) {
        showButtonCustomFunction(i);
    }

    /* JADX INFO: renamed from: lambda$setClick$3$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m208lambda$setClick$3$comcubeSuitefragmentFootCtrlControlFragment(View view) {
        int i = this.midiChannelValue;
        if (i < 15) {
            this.midiChannelValue = i + 1;
            communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteChannelValue((byte) this.midiChannelValue));
            this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        }
    }

    /* JADX INFO: renamed from: lambda$setClick$4$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m209lambda$setClick$4$comcubeSuitefragmentFootCtrlControlFragment(View view) {
        int i = this.midiChannelValue;
        if (i > 0) {
            this.midiChannelValue = i - 1;
            communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteChannelValue((byte) this.midiChannelValue));
            this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        }
    }

    /* JADX INFO: renamed from: lambda$setClick$5$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m210lambda$setClick$5$comcubeSuitefragmentFootCtrlControlFragment(View view) {
        showPopupWindow();
    }

    /* JADX INFO: renamed from: lambda$setClick$6$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m211lambda$setClick$6$comcubeSuitefragmentFootCtrlControlFragment(int i, View view) {
        this.currentFootSwitch = i;
        this.footSwitchText.setText(this.FOOT_SWITCH_NAME[i]);
        this.editMidiWindow.showAtLocation(this.thisView, 80, 0, 0);
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$setClick$7$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m212lambda$setClick$7$comcubeSuitefragmentFootCtrlControlFragment(int i, View view) {
        showKyeCustomDialog(i);
    }

    /* JADX INFO: renamed from: lambda$setClick$8$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m213lambda$setClick$8$comcubeSuitefragmentFootCtrlControlFragment(View view) {
        showTrsMidiSlect();
    }

    private void showPopupWindow() {
        View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.trs_switch_layout, (ViewGroup) null);
        this.mPopWindow = new PopupWindow(viewInflate, -1, -1, true);
        final ImageView imageView = (ImageView) viewInflate.findViewById(R.id.expressionPedal);
        final ImageView imageView2 = (ImageView) viewInflate.findViewById(R.id.trsMidi);
        View viewFindViewById = viewInflate.findViewById(R.id.main);
        int i = this.trsMidi;
        int i2 = R.drawable.bg_accent;
        imageView.setBackgroundResource(i == 0 ? R.drawable.bg_accent : R.drawable.bg_click_effect);
        if (this.trsMidi != 1) {
            i2 = R.drawable.bg_click_effect;
        }
        imageView2.setBackgroundResource(i2);
        imageView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda18
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m218xd00cac1a(imageView, imageView2, view);
            }
        });
        imageView2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda19
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m216x60223830(imageView2, imageView, view);
            }
        });
        viewFindViewById.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda20
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m217x99ecda0f(view);
            }
        });
        this.mPopWindow.showAtLocation(LayoutInflater.from(this.activity).inflate(R.layout.bluetoothe_pedal_contorl_fragment, (ViewGroup) null), 80, 0, 0);
    }

    /* JADX INFO: renamed from: lambda$showPopupWindow$9$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m218xd00cac1a(ImageView imageView, ImageView imageView2, View view) {
        this.trsMidi = 0;
        imageView.setBackgroundResource(R.drawable.bg_accent);
        imageView2.setBackgroundResource(R.drawable.bg_click_effect);
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteTrsMidi((byte) this.trsMidi));
    }

    /* JADX INFO: renamed from: lambda$showPopupWindow$10$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m216x60223830(ImageView imageView, ImageView imageView2, View view) {
        this.trsMidi = 1;
        imageView.setBackgroundResource(R.drawable.bg_accent);
        imageView2.setBackgroundResource(R.drawable.bg_click_effect);
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteTrsMidi((byte) this.trsMidi));
    }

    /* JADX INFO: renamed from: lambda$showPopupWindow$11$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m217x99ecda0f(View view) {
        this.mPopWindow.dismiss();
    }

    private void showTrsMidiSlect() {
        if (this.trsMidiWindow == null) {
            this.trsMidiContentView = LayoutInflater.from(this.activity).inflate(R.layout.trs_switch_layout, (ViewGroup) null);
            this.trsMidiWindow = new PopupWindow(this.trsMidiContentView, -1, -1, true);
            this.expressionPedalImage = (ImageView) this.trsMidiContentView.findViewById(R.id.expressionPedal);
            this.trsMidiImage = (ImageView) this.trsMidiContentView.findViewById(R.id.trsMidi);
            Button button = (Button) this.trsMidiContentView.findViewById(R.id.back);
            ImageView imageView = this.expressionPedalImage;
            int i = this.trsMidi;
            int i2 = R.drawable.bg_accent;
            imageView.setBackgroundResource(i == 0 ? R.drawable.bg_accent : R.drawable.bg_click_effect);
            ImageView imageView2 = this.trsMidiImage;
            if (this.trsMidi != 1) {
                i2 = R.drawable.bg_click_effect;
            }
            imageView2.setBackgroundResource(i2);
            button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda21
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m219x77cc892d(view);
                }
            });
            this.expressionPedalImage.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda23
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m220xb1972b0c(view);
                }
            });
            this.trsMidiImage.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda24
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m221xeb61cceb(view);
                }
            });
        }
        this.trsMidiWindow.showAtLocation(this.thisView, 80, 0, 0);
    }

    /* JADX INFO: renamed from: lambda$showTrsMidiSlect$12$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m219x77cc892d(View view) {
        this.trsMidiWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$showTrsMidiSlect$13$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m220xb1972b0c(View view) {
        this.trsMidi = 0;
        this.expressionPedalImage.setBackgroundResource(R.drawable.bg_accent);
        this.trsMidiImage.setBackgroundResource(R.drawable.bg_click_effect);
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteTrsMidi((byte) this.trsMidi));
    }

    /* JADX INFO: renamed from: lambda$showTrsMidiSlect$14$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m221xeb61cceb(View view) {
        this.trsMidi = 1;
        this.trsMidiImage.setBackgroundResource(R.drawable.bg_accent);
        this.expressionPedalImage.setBackgroundResource(R.drawable.bg_click_effect);
        communicationUtil.writeDataToBle(this.fcc.bluetoothPedalWriteTrsMidi((byte) this.trsMidi));
    }

    private void initMidiInfo() {
        if (this.editMidiWindow == null) {
            this.editMidiContentView = LayoutInflater.from(this.activity).inflate(R.layout.send_midi_edit_layout, (ViewGroup) null);
            this.editMidiWindow = new PopupWindow(this.editMidiContentView, -1, -1, true);
            this.btnEditMidiBack = (ButtonIconView) this.editMidiContentView.findViewById(R.id.back);
            this.btnModeSwitch = (Button) this.editMidiContentView.findViewById(R.id.modeSwitch);
            this.btnMidiA = (Button) this.editMidiContentView.findViewById(R.id.midiA);
            this.btnMidiB = (Button) this.editMidiContentView.findViewById(R.id.midiB);
            this.btnPlus = (ButtonIconView) this.editMidiContentView.findViewById(R.id.plusMidiCode);
            this.btnDeleteList = (ButtonIconView) this.editMidiContentView.findViewById(R.id.deleteList);
            this.midiInfoList = (LinearLayout) this.editMidiContentView.findViewById(R.id.midiInfoList);
            this.midiInfoListScroll = (ScrollView) this.editMidiContentView.findViewById(R.id.midiInfoListScroll);
            this.footSwitchText = (TextView) this.editMidiContentView.findViewById(R.id.footSwitchText);
            this.btnEditMidiBack.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda12
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m198x632f4fc5(view);
                }
            });
            this.btnModeSwitch.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda13
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m199x9cf9f1a4(view);
                }
            });
            this.btnMidiA.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda14
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m200xd6c49383(view);
                }
            });
            this.btnMidiB.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda15
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m201x108f3562(view);
                }
            });
            this.btnPlus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda16
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m202x4a59d741(view);
                }
            });
            this.btnDeleteList.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda17
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m203x41c3c06b(view);
                }
            });
        }
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$15$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m198x632f4fc5(View view) {
        this.editMidiWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$16$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m199x9cf9f1a4(View view) {
        this.selectModeWindow.showAtLocation(this.thisView, 80, 0, 0);
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$17$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m200xd6c49383(View view) {
        if (this.groupIndex == 0) {
            return;
        }
        this.groupIndex = 0;
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$18$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m201x108f3562(View view) {
        if (this.groupIndex == 1) {
            return;
        }
        this.groupIndex = 1;
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$19$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m202x4a59d741(View view) {
        if (isSys() || getMidiCode(15).getIsEnable() == 1) {
            return;
        }
        for (int i = 0; i < 16; i++) {
            if (getMidiCode(i).getIsEnable() == 0) {
                getMidiCode(i).clear();
                getMidiCode(i).setIsEnable(1);
                this.midiBList[i].body.deleteAnimator();
                break;
            }
        }
        CommunicationUtil communicationUtil = communicationUtil;
        FootCtrlCommunication footCtrlCommunication = this.fcc;
        CustomData[] customData = this.footCtrlEntry.getCustomData();
        int i2 = this.currentFootSwitch;
        communicationUtil.writeDataToBle(footCtrlCommunication.footCtrlWriteCustomData(customData[i2], i2));
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$initMidiInfo$20$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m203x41c3c06b(View view) {
        if (isSys()) {
            return;
        }
        AlertDialogUtil.getInstance(this.activity).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment.4
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                for (int i = 0; i < 16; i++) {
                    FootCtrlControlFragment.this.getMidiCode(i).clear();
                    FootCtrlControlFragment.this.getMidiCode(i).setIsEnable(0);
                }
                ControlBaseFragment.communicationUtil.writeDataToBle(FootCtrlControlFragment.this.fcc.footCtrlWriteCustomData(FootCtrlControlFragment.this.footCtrlEntry.getCustomData()[FootCtrlControlFragment.this.currentFootSwitch], FootCtrlControlFragment.this.currentFootSwitch));
                FootCtrlControlFragment.this.updateMidiInfo();
            }
        }).setTitle(R.string.hint).setContent(R.string.removeAllMidiInfo).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void updateMidiInfo() {
        for (int i = 0; i < 16; i++) {
            MidiCode midiCode = getMidiCode(i);
            if (this.groupIndex == 0) {
                if (midiCode.getIsEnable() != 1) {
                    this.midiAList[i].body.setVisibility(8);
                } else {
                    this.midiAList[i].updateText();
                }
                this.midiBList[i].body.setVisibility(8);
            } else {
                if (midiCode.getIsEnable() != 1) {
                    this.midiBList[i].body.setVisibility(8);
                } else {
                    this.midiBList[i].updateText();
                }
                this.midiAList[i].body.setVisibility(8);
            }
        }
        setBankView();
    }

    private void initMidiList() {
        for (int i = 0; i < 16; i++) {
            SlidingDeleteItem slidingDeleteItem = new SlidingDeleteItem(this.activity);
            View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate2 = LayoutInflater.from(this.activity).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem.setScrollView(this.midiInfoListScroll);
            slidingDeleteItem.addContentView(viewInflate);
            slidingDeleteItem.addMenuView(viewInflate2);
            slidingDeleteItem.setBtnDelete((Button) viewInflate2.findViewById(R.id.btnDelete));
            this.midiAList[i] = new MidiItem(i, slidingDeleteItem, (TextView) viewInflate.findViewById(R.id.textStr));
        }
        for (int i2 = 0; i2 < 16; i2++) {
            SlidingDeleteItem slidingDeleteItem2 = new SlidingDeleteItem(this.activity);
            View viewInflate3 = LayoutInflater.from(this.activity).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate4 = LayoutInflater.from(this.activity).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem2.setBtnDelete((Button) viewInflate4.findViewById(R.id.btnDelete));
            slidingDeleteItem2.setScrollView(this.midiInfoListScroll);
            slidingDeleteItem2.addContentView(viewInflate3);
            slidingDeleteItem2.addMenuView(viewInflate4);
            this.midiBList[i2] = new MidiItem(i2, slidingDeleteItem2, (TextView) viewInflate3.findViewById(R.id.textStr));
        }
    }

    private void initMidiMode() {
        if (this.selectModeWindow != null) {
            return;
        }
        this.selectModeContentView = LayoutInflater.from(this.activity).inflate(R.layout.select_foot_ctrl_mode_layout, (ViewGroup) null);
        this.selectModeWindow = new PopupWindow(this.selectModeContentView, -1, -1, true);
        this.btnSelectModeBack = (ButtonIconView) this.selectModeContentView.findViewById(R.id.back);
        final int i = 0;
        this.selectModelItem[0] = (SelectItem) this.selectModeContentView.findViewById(R.id.singleStepOneMode);
        this.selectModelItem[1] = (SelectItem) this.selectModeContentView.findViewById(R.id.singleStepTwoMode);
        this.selectModelItem[2] = (SelectItem) this.selectModeContentView.findViewById(R.id.onTheLooseTwoMode);
        this.selectModelItem[3] = (SelectItem) this.selectModeContentView.findViewById(R.id.longOnMode);
        this.selectModelItem[4] = (SelectItem) this.selectModeContentView.findViewById(R.id.singleOrLongOnMode);
        this.btnSelectModeBack.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda30
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m204x7216afff(view);
            }
        });
        while (true) {
            SelectItem[] selectItemArr = this.selectModelItem;
            if (i >= selectItemArr.length) {
                return;
            }
            selectItemArr[i].setItemClick(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda31
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m205xabe151de(i, view);
                }
            });
            i++;
        }
    }

    /* JADX INFO: renamed from: lambda$initMidiMode$21$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m204x7216afff(View view) {
        this.selectModeWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initMidiMode$22$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m205xabe151de(int i, View view) {
        this.footCtrlEntry.getCustomData()[this.currentFootSwitch].setMode(i);
        setBankView();
        this.btnModeSwitch.setText(this.selectModelItem[i].getModeText());
        CommunicationUtil communicationUtil = communicationUtil;
        FootCtrlCommunication footCtrlCommunication = this.fcc;
        CustomData[] customData = this.footCtrlEntry.getCustomData();
        int i2 = this.currentFootSwitch;
        communicationUtil.writeDataToBle(footCtrlCommunication.footCtrlWriteCustomData(customData[i2], i2));
        setBankView();
        this.selectModeWindow.dismiss();
    }

    public void updateCurrentMidiCodeText() {
        String str;
        String str2 = (String) this.channel.getText();
        String str3 = (String) this.numType.getText();
        int i = 0;
        while (true) {
            if (i >= this.type.getChildCount()) {
                str = "";
                break;
            } else {
                if (((RadioButton) this.type.getChildAt(i)).isChecked()) {
                    str = MIDI_TYPE[i];
                    break;
                }
                i++;
            }
        }
        String str4 = (String) this.data1.getText();
        String str5 = (String) this.data2.getText();
        if (isInterface()) {
            this.tvMidiInfo.setText("[" + (this.currentMidiCode + 1) + "]   " + str2 + "   CC   " + str3 + "   " + str4 + "~" + str5);
        } else if (getMidiCode(this.currentMidiCode).getType() == 4) {
            this.tvMidiInfo.setText("send:" + byteToX16String(getSysEx()));
        } else {
            this.tvMidiInfo.setText("[" + (this.currentMidiCode + 1) + "]   " + str2 + "   " + str + "   " + str4 + "   " + str5);
        }
    }

    private void initMidiCode() {
        if (this.editMidiCodeView == null) {
            this.editMidiCodeView = LayoutInflater.from(this.activity).inflate(R.layout.edit_send_midi_info_layout, (ViewGroup) null);
            this.editMidiCodeWindow = new PopupWindow(this.editMidiCodeView, -1, -1, true);
            this.btnEditMidiCodeBack = (Button) this.editMidiCodeView.findViewById(R.id.back);
            this.tvMidiInfo = (TextView) this.editMidiCodeView.findViewById(R.id.tvMidiInfo);
            this.channel = (Button) this.editMidiCodeView.findViewById(R.id.channel);
            this.type = (RadioGroup) this.editMidiCodeView.findViewById(R.id.type);
            this.data1 = (Button) this.editMidiCodeView.findViewById(R.id.data1);
            this.data2 = (Button) this.editMidiCodeView.findViewById(R.id.data2);
            this.cancel = (Button) this.editMidiCodeView.findViewById(R.id.cancel);
            this.save = (Button) this.editMidiCodeView.findViewById(R.id.save);
            this.numType = (Button) this.editMidiCodeView.findViewById(R.id.numType);
            this.numKeyPad = (NumKeyPadView) this.editMidiCodeView.findViewById(R.id.numKeyPad);
            this.tvIType = (TextView) this.editMidiCodeView.findViewById(R.id.tvIType);
            this.tvType = (TextView) this.editMidiCodeView.findViewById(R.id.tvType);
            this.tvIData1 = (TextView) this.editMidiCodeView.findViewById(R.id.tvIData1);
            this.tvData1 = (TextView) this.editMidiCodeView.findViewById(R.id.tvData1);
            this.tvIData2 = (TextView) this.editMidiCodeView.findViewById(R.id.tvIData2);
            this.tvData2 = (TextView) this.editMidiCodeView.findViewById(R.id.tvData2);
            this.etCustomMidiCode = (EditText) this.editMidiCodeView.findViewById(R.id.etCustomMidiCode);
            this.tvCustomMidiCode = (TextView) this.editMidiCodeView.findViewById(R.id.tvCustomMidiCode);
            this.tvChannel = (TextView) this.editMidiCodeView.findViewById(R.id.tvChannel);
            this.type.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment.5
                @Override // android.widget.RadioGroup.OnCheckedChangeListener
                public void onCheckedChanged(RadioGroup radioGroup, int i) {
                    FootCtrlControlFragment.this.setMidiEditViewVisibility();
                }
            });
            this.numKeyPad.setKeyPadListener(new NumKeyPadView.KeypadListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda0
                @Override // com.cubeSuite.customControl.NumKeyPadView.KeypadListener
                public final void numChange(int i) {
                    this.f$0.m190xe0990187(i);
                }
            });
            this.btnEditMidiCodeBack.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda11
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m191x1a63a366(view);
                }
            });
            this.channel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda22
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m192x542e4545(view);
                }
            });
            this.numType.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda25
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m193x8df8e724(view);
                }
            });
            this.data1.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda26
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m194xc7c38903(view);
                }
            });
            this.data2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda27
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m195x18e2ae2(view);
                }
            });
            this.cancel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda28
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m196x3b58ccc1(view);
                }
            });
            this.save.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda29
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m197x32c2b5eb(view);
                }
            });
        }
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$23$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m190xe0990187(int i) {
        int i2 = this.currentEditInput;
        if (i2 == 0) {
            this.channel.setText(String.valueOf(i + 1));
        } else if (i2 == 1) {
            this.numType.setText(String.valueOf(i));
        } else if (i2 == 2) {
            this.data1.setText(String.valueOf(i));
        } else if (i2 == 3) {
            this.data2.setText(String.valueOf(i));
        }
        updateCurrentMidiCodeText();
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$24$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m191x1a63a366(View view) {
        editMidiCodeFocus(-1);
        this.editMidiCodeWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$25$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m192x542e4545(View view) {
        editMidiCodeFocus(0);
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$26$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m193x8df8e724(View view) {
        editMidiCodeFocus(1);
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$27$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m194xc7c38903(View view) {
        editMidiCodeFocus(2);
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$28$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m195x18e2ae2(View view) {
        editMidiCodeFocus(3);
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$29$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m196x3b58ccc1(View view) {
        editMidiCodeFocus(-1);
        this.editMidiCodeWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initMidiCode$30$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m197x32c2b5eb(View view) {
        MidiCode midiCode = getMidiCode(this.currentMidiCode);
        if (isInterface()) {
            midiCode.setType(Integer.parseInt(String.valueOf(this.numType.getText())));
        } else {
            for (int i = 0; i < this.type.getChildCount(); i++) {
                if (((RadioButton) this.type.getChildAt(i)).isChecked()) {
                    midiCode.setType(i);
                    break;
                }
            }
        }
        if (midiCode.getType() == 4 && !isInterface()) {
            if (midiCode.getType() == 4) {
                if (this.currentMidiCode != 0) {
                    this.currentMidiCode = 0;
                    getMidiCode(0).copy(midiCode);
                    midiCode.setIsEnable(0);
                }
                if (this.groupIndex == 0) {
                    this.footCtrlEntry.getSysExA()[this.currentFootSwitch] = x16StringToByte(this.etCustomMidiCode.getText().toString());
                } else {
                    this.footCtrlEntry.getSysExB()[this.currentFootSwitch] = x16StringToByte(this.etCustomMidiCode.getText().toString());
                }
                for (int i2 = 1; i2 < 16; i2++) {
                    getMidiCode(i2).setIsEnable(0);
                }
            }
            CommunicationUtil communicationUtil = communicationUtil;
            FootCtrlCommunication footCtrlCommunication = this.fcc;
            CustomData[] customData = this.footCtrlEntry.getCustomData();
            int i3 = this.currentFootSwitch;
            communicationUtil.writeDataToBle(footCtrlCommunication.footCtrlWriteCustomData(customData[i3], i3));
            communicationUtil.writeDataToBle(this.fcc.footCtrlWriteCustomMidi(this.groupIndex == 0 ? this.footCtrlEntry.getSysExA()[this.currentFootSwitch] : this.footCtrlEntry.getSysExB()[this.currentFootSwitch], this.currentFootSwitch, this.groupIndex));
            updateMidiInfo();
            AppUtil.closeKeybord(this.activity);
            this.editMidiCodeWindow.dismiss();
            return;
        }
        int i4 = Integer.parseInt(String.valueOf(this.data1.getText()));
        int i5 = Integer.parseInt(String.valueOf(this.data2.getText()));
        midiCode.setChannel(Integer.parseInt(String.valueOf(this.channel.getText())) - 1);
        midiCode.setData1(i4);
        midiCode.setData2(i5);
        editMidiCodeFocus(-1);
        CommunicationUtil communicationUtil2 = communicationUtil;
        FootCtrlCommunication footCtrlCommunication2 = this.fcc;
        CustomData[] customData2 = this.footCtrlEntry.getCustomData();
        int i6 = this.currentFootSwitch;
        communicationUtil2.writeDataToBle(footCtrlCommunication2.footCtrlWriteCustomData(customData2[i6], i6));
        updateMidiInfo();
        this.editMidiCodeWindow.dismiss();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setMidiEditViewVisibility() {
        if (isInterface()) {
            this.tvIType.setVisibility(0);
            this.tvIData1.setVisibility(0);
            this.tvIData2.setVisibility(0);
            this.data1.setVisibility(0);
            this.data2.setVisibility(0);
            this.tvType.setVisibility(8);
            this.tvData1.setVisibility(8);
            this.tvData2.setVisibility(8);
            this.type.setVisibility(8);
            this.numType.setVisibility(0);
            return;
        }
        int i = 0;
        while (true) {
            if (i >= this.type.getChildCount()) {
                i = 0;
                break;
            } else if (((RadioButton) this.type.getChildAt(i)).isChecked()) {
                break;
            } else {
                i++;
            }
        }
        this.tvType.setVisibility(0);
        this.type.setVisibility(0);
        this.numType.setVisibility(8);
        this.tvIData1.setVisibility(8);
        this.tvIData2.setVisibility(8);
        if (i == 4) {
            this.etCustomMidiCode.setText(byteToX16String(getSysEx()));
            this.data1.setVisibility(8);
            this.tvData1.setVisibility(8);
            this.data2.setVisibility(8);
            this.tvData2.setVisibility(8);
            this.channel.setVisibility(8);
            this.tvChannel.setVisibility(8);
            this.numKeyPad.setVisibility(4);
            this.etCustomMidiCode.setVisibility(0);
            this.tvCustomMidiCode.setVisibility(0);
            return;
        }
        if (i == 0) {
            this.data2.setVisibility(8);
            this.tvData2.setVisibility(8);
        } else {
            this.data2.setVisibility(0);
            this.tvData2.setVisibility(0);
        }
        this.data1.setVisibility(0);
        this.tvData1.setVisibility(0);
        this.channel.setVisibility(0);
        this.tvChannel.setVisibility(0);
        this.etCustomMidiCode.setVisibility(8);
        this.tvCustomMidiCode.setVisibility(8);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public String byteToX16String(byte[] bArr) {
        StringBuilder sb = new StringBuilder();
        byte b = bArr[0];
        for (int i = 1; i < b + 1; i++) {
            byte b2 = bArr[i];
            if ((b2 & UByte.MAX_VALUE) < 16) {
                sb.append("0").append(Integer.toHexString(bArr[i] & UByte.MAX_VALUE)).append(" ");
            } else {
                sb.append(Integer.toHexString(b2 & UByte.MAX_VALUE)).append(" ");
            }
        }
        return sb.toString().toUpperCase();
    }

    private byte[] x16StringToByte(String str) {
        boolean z;
        char[] cArr = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        String strReplace = str.toUpperCase().replace("\\s", "").replace("\\n", "");
        byte[] bArr = new byte[128];
        boolean z2 = false;
        int i = 0;
        byte b = 1;
        for (char c : strReplace.substring(0, Math.min(128, strReplace.length())).toCharArray()) {
            byte b2 = 0;
            while (true) {
                if (b2 >= 16) {
                    z = false;
                    break;
                }
                if (cArr[b2] == c) {
                    if (z2) {
                        bArr[b] = (byte) (b2 + i);
                        b = (byte) (b + 1);
                        z2 = false;
                    } else {
                        i = b2 * 16;
                        z2 = true;
                    }
                    z = z2;
                    break;
                }
                b2 = (byte) (b2 + 1);
            }
            if (!z && z2) {
                z2 = false;
                i = 0;
            }
        }
        int i2 = b + 1;
        byte[] bArr2 = new byte[i2];
        System.arraycopy(bArr, 0, bArr2, 0, i2);
        bArr2[0] = (byte) (b - 1);
        return bArr2;
    }

    private boolean isSys() {
        MidiCode midiCode = getMidiCode(0);
        return midiCode.getIsEnable() > 0 && midiCode.getType() == 4;
    }

    private void editMidiCodeFocus(int i) {
        this.currentEditInput = i;
        this.channel.setTextColor(getResources().getColor(R.color.dominantHue));
        this.numType.setTextColor(getResources().getColor(R.color.dominantHue));
        this.data1.setTextColor(getResources().getColor(R.color.dominantHue));
        this.data2.setTextColor(getResources().getColor(R.color.dominantHue));
        this.numKeyPad.setScope(0, 127);
        this.numKeyPad.setOffset(0);
        if (i == 0) {
            this.numKeyPad.setScope(1, 16);
            this.numKeyPad.setValue(String.valueOf(this.channel.getText()));
            this.numKeyPad.setOffset(-1);
            this.channel.setTextColor(getResources().getColor(R.color.accent));
        } else if (i == 1) {
            this.numKeyPad.setValue(String.valueOf(this.numType.getText()));
            this.numType.setTextColor(getResources().getColor(R.color.accent));
        } else if (i == 2) {
            this.numKeyPad.setValue(String.valueOf(this.data1.getText()));
            this.data1.setTextColor(getResources().getColor(R.color.accent));
        } else if (i == 3) {
            this.numKeyPad.setValue(String.valueOf(this.data2.getText()));
            this.data2.setTextColor(getResources().getColor(R.color.accent));
        }
        this.numKeyPad.show();
    }

    private void loadView() {
        this.tvModelExplain = (TextView) this.thisView.findViewById(R.id.tvModelExplain);
        this.customKeypadContainer = this.thisView.findViewById(R.id.customKeypadContainer);
        this.advancedCustomKeypadContainer = this.thisView.findViewById(R.id.advancedCustomKeypadContainer);
        this.customKeypadModeView = this.thisView.findViewById(R.id.customKeypadModeView);
        this.footSwitchBtn[0] = (Button) this.thisView.findViewById(R.id.footSwitchA);
        this.footSwitchBtn[1] = (Button) this.thisView.findViewById(R.id.footSwitchB);
        this.footSwitchBtn[2] = (Button) this.thisView.findViewById(R.id.footSwitchC);
        this.footSwitchBtn[3] = (Button) this.thisView.findViewById(R.id.footSwitchD);
        this.footSwitchBtn[4] = (Button) this.thisView.findViewById(R.id.padInterface);
        this.customKeypadKeyBtn[0] = (Button) this.thisView.findViewById(R.id.customKeypadKey1);
        this.customKeypadKeyBtn[1] = (Button) this.thisView.findViewById(R.id.customKeypadKey2);
        this.customKeypadKeyBtn[2] = (Button) this.thisView.findViewById(R.id.customKeypadKey3);
        this.customKeypadKeyBtn[3] = (Button) this.thisView.findViewById(R.id.customKeypadKey4);
        this.customKeypad[0] = (Button) this.thisView.findViewById(R.id.customKeypad1);
        this.customKeypad[1] = (Button) this.thisView.findViewById(R.id.customKeypad2);
        this.customKeypad[2] = (Button) this.thisView.findViewById(R.id.customKeypad3);
        this.customKeypad[3] = (Button) this.thisView.findViewById(R.id.customKeypad4);
        this.customKeypad[4] = (Button) this.thisView.findViewById(R.id.customKeypad5);
        this.trsMidiBtn = (ButtonIconView) this.thisView.findViewById(R.id.trsMidi);
        this.sendMidiChannel = (TextView) this.thisView.findViewById(R.id.sendMidiChannel);
        this.midiChannelMinus = (ButtonIconView) this.thisView.findViewById(R.id.midiChannelMinus);
        this.midiChannelPlus = (ButtonIconView) this.thisView.findViewById(R.id.midiChannelPlus);
        ((ImageButton) this.thisView.findViewById(R.id.updatePedalParam)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootCtrlControlFragment$$ExternalSyntheticLambda10
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m206xf1c17bd7(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$loadView$31$com-cubeSuite-fragment-FootCtrlControlFragment, reason: not valid java name */
    /* synthetic */ void m206xf1c17bd7(View view) {
        this.currentFootSwitch = 4;
        this.footSwitchText.setText(this.FOOT_SWITCH_NAME[4]);
        this.editMidiWindow.showAtLocation(this.thisView, 80, 0, 0);
        updateMidiInfo();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setData(byte[] bArr) {
        Map<String, Object> footCtrlData = this.fcc.parseFootCtrlData(bArr);
        FootCtrlEntry footCtrlEntry = (FootCtrlEntry) footCtrlData.get("footCtrlEntry");
        this.midiChannelValue = footCtrlEntry.getUsrChan();
        byte state = footCtrlEntry.getState();
        this.trsMidi = footCtrlEntry.getTrsMidi();
        this.modeSelect.setCurrentItem(this.dataToMode[state]);
        this.footCtrlEntry = footCtrlEntry;
        this.tvModelExplain.setText(this.modelsTextExplain[state]);
        if (state == 7) {
            this.customKeypadContainer.setVisibility(0);
        } else {
            this.customKeypadContainer.setVisibility(4);
        }
        if (state == 9) {
            this.advancedCustomKeypadContainer.setVisibility(0);
        } else {
            this.advancedCustomKeypadContainer.setVisibility(8);
        }
        if (state == 10) {
            this.customKeypadModeView.setVisibility(0);
        } else {
            this.customKeypadModeView.setVisibility(8);
        }
        this.sendMidiChannel.setText(String.valueOf(this.midiChannelValue + 1));
        this.customKeypadSelect = (int[]) footCtrlData.get("typeArr");
        this.toggleSelect = (int[]) footCtrlData.get("toggleArr");
        for (int i = 0; i < this.customKeypadKeyBtn.length; i++) {
            this.customKeypadKeyBtn[i].setText(this.selectKeyDialog.aztab[Math.min(Math.max((int) footCtrlEntry.getCustomKeyboard()[i * 4], 0), this.selectKeyDialog.aztab.length - 1)]);
        }
        int i2 = 0;
        while (true) {
            Button[] buttonArr = this.customKeypad;
            if (i2 < buttonArr.length) {
                if (i2 == buttonArr.length - 1) {
                    buttonArr[i2].setText(Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR_KNOB[this.customKeypadSelect[i2]]);
                } else {
                    ListAlertDialog listAlertDialog = this.customKeypadList[i2];
                    if (listAlertDialog != null) {
                        listAlertDialog.setToggleChecked(this.toggleSelect[i2] == 1);
                    }
                    this.customKeypad[i2].setText(Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR[this.customKeypadSelect[i2]]);
                }
                i2++;
            } else {
                updateMidiInfo();
                return;
            }
        }
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        this.progressDialog.dismiss();
        super.onDestroy();
    }
}
