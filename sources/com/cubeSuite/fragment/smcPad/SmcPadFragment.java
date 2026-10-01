package com.cubeSuite.fragment.smcPad;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.communication.SmcPadCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.entity.smcPad.SmcPadEntry;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.utils.FileUtils;
import com.google.android.material.tabs.TabLayout;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadFragment extends ControlBaseFragment {
    private SmcPadEntry entry;
    private TabLayout mTabLayout;
    private View m_mainView;
    SmcPadButDialog smcPadButDialog;
    private SmcPadButton smcPadButton;
    public SmcPadCommunication smcPadCommunication;
    SmcPadGlobDialog smcPadGlobDialog;
    private SmcPadKnob smcPadKnob;
    SmcPadKnobDialog smcPadKnobDialog;
    SmcPadNoteDialog smcPadNoteDialog;
    private SmcPadPad smcPadPad;
    SmcPadPadDialog smcPadPadDialog;
    int currentPreset = 0;
    int currentOctive = 0;
    int currentTranspose = 0;
    int currentPadbank = 0;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            if (SmcPadFragment.this.smcPadCommunication == null) {
                SmcPadFragment.this.smcPadCommunication = new SmcPadCommunication(ControlBaseFragment.communicationUtil);
                SmcPadFragment.this.smcPadGlobDialog.smcPadCommunication = SmcPadFragment.this.smcPadCommunication;
                SmcPadFragment.this.smcPadPadDialog.smcPadCommunication = SmcPadFragment.this.smcPadCommunication;
                SmcPadFragment.this.smcPadKnobDialog.smcPadCommunication = SmcPadFragment.this.smcPadCommunication;
                SmcPadFragment.this.smcPadButDialog.smcPadCommunication = SmcPadFragment.this.smcPadCommunication;
                SmcPadFragment.this.smcPadNoteDialog.smcPadCommunication = SmcPadFragment.this.smcPadCommunication;
            }
            AlertDialogUtil.getInstance(SmcPadFragment.this.activity).closeDialog();
            return false;
        }
    });
    private TabLayout.OnTabSelectedListener listener = new TabLayout.OnTabSelectedListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.2
        @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
        public void onTabReselected(TabLayout.Tab tab) {
        }

        @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
        public void onTabUnselected(TabLayout.Tab tab) {
        }

        @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
        public void onTabSelected(TabLayout.Tab tab) {
            int position = tab.getPosition();
            if (position == 0) {
                SmcPadFragment.this.smcPadKnob.setVisibility(0);
                SmcPadFragment.this.smcPadPad.setVisibility(4);
                SmcPadFragment.this.smcPadButton.setVisibility(4);
            } else if (position == 1) {
                SmcPadFragment.this.smcPadKnob.setVisibility(4);
                SmcPadFragment.this.smcPadPad.setVisibility(0);
                SmcPadFragment.this.smcPadButton.setVisibility(4);
            } else {
                if (position != 2) {
                    return;
                }
                SmcPadFragment.this.smcPadKnob.setVisibility(4);
                SmcPadFragment.this.smcPadPad.setVisibility(4);
                SmcPadFragment.this.smcPadButton.setVisibility(0);
            }
        }
    };

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        this.activity = (ControlAndPdfActivity) getActivity();
        this.m_mainView = layoutInflater.inflate(R.layout.fragment_smc_pad, viewGroup, false);
        initView();
        return this.m_mainView;
    }

    private void initView() {
        SmcPadGlobDialog smcPadGlobDialog = new SmcPadGlobDialog(getActivity(), "Glob");
        this.smcPadGlobDialog = smcPadGlobDialog;
        smcPadGlobDialog.smcPadGlobDialogCallback = new SmcPadGlobDialogCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.3
            @Override // com.cubeSuite.fragment.smcPad.SmcPadGlobDialogCallback
            public void onSelectPreset(int i) {
                SmcPadFragment.this.currentPreset = i;
            }

            @Override // com.cubeSuite.fragment.smcPad.SmcPadGlobDialogCallback
            public void onSelectOctive(int i) {
                SmcPadFragment.this.currentOctive = i;
            }

            @Override // com.cubeSuite.fragment.smcPad.SmcPadGlobDialogCallback
            public void onSelectTranspose(int i) {
                SmcPadFragment.this.currentTranspose = i;
            }
        };
        this.smcPadPadDialog = new SmcPadPadDialog(getActivity(), "");
        this.smcPadKnobDialog = new SmcPadKnobDialog(getActivity(), "");
        this.smcPadButDialog = new SmcPadButDialog(getActivity(), "");
        this.smcPadNoteDialog = new SmcPadNoteDialog(getActivity(), "note repeat");
        this.mTabLayout = (TabLayout) this.m_mainView.findViewById(R.id.tab_layout);
        ((Button) this.m_mainView.findViewById(R.id.glob)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.4
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadFragment.this.smcPadGlobDialog.updateData(SmcPadFragment.this.entry.smcPadGlob);
                SmcPadFragment.this.smcPadGlobDialog.show();
            }
        });
        this.mTabLayout.addOnTabSelectedListener(this.listener);
        ((Button) this.m_mainView.findViewById(R.id.reset)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.5
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadFragment.this.resetConfig();
            }
        });
        ((Button) this.m_mainView.findViewById(R.id.save)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.6
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadFragment.this.saveConfig();
            }
        });
        ((Button) this.m_mainView.findViewById(R.id.note_repeat)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.7
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadFragment.this.smcPadNoteDialog.updateData(SmcPadFragment.this.entry.smcPadGlob);
                SmcPadFragment.this.smcPadNoteDialog.show();
            }
        });
        SmcPadKnob smcPadKnob = (SmcPadKnob) this.m_mainView.findViewById(R.id.smc_pad_knob);
        this.smcPadKnob = smcPadKnob;
        smcPadKnob.gridView.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.8
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                Integer num = (Integer) SmcPadFragment.this.smcPadKnob.myBaseAdapter.getItem(i);
                SmcPadFragment.this.smcPadKnobDialog.updateData(SmcPadFragment.this.entry.usr[SmcPadFragment.this.currentPreset].smcPadKnobs[num.intValue()], "Knob " + (num.intValue() + 1));
                SmcPadFragment.this.smcPadKnobDialog.show();
            }
        });
        this.smcPadKnob.setVisibility(0);
        SmcPadPad smcPadPad = (SmcPadPad) this.m_mainView.findViewById(R.id.smc_pad_pad);
        this.smcPadPad = smcPadPad;
        smcPadPad.smcPadPadCallback = new SmcPadPad.SmcPadPadCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.9
            @Override // com.cubeSuite.fragment.smcPad.SmcPadPad.SmcPadPadCallback
            public void onItemClick(int i) {
                SmcPadFragment.this.entry.smcPadGlob.padbank.setData(i);
                if (SmcPadFragment.this.smcPadCommunication != null) {
                    SmcPadFragment.this.smcPadCommunication.sendGlobU8(SmcPadFragment.this.entry.smcPadGlob.padbank);
                }
                SmcPadFragment.this.currentPadbank = i;
            }
        };
        this.smcPadPad.gridView.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.10
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                Object item = SmcPadFragment.this.smcPadPad.myBaseAdapter.getItem(i);
                SmcPadFragment.this.smcPadPadDialog.updateData(SmcPadFragment.this.entry.usr[SmcPadFragment.this.currentPreset].smcPadPads[SmcPadFragment.this.currentPadbank == 0 ? (SmcPadFragment.this.currentOctive * 16) + ((Integer) item).intValue() + SmcPadFragment.this.currentTranspose : ((Integer) item).intValue() + 112], (SmcPadFragment.this.currentPadbank == 0 ? new StringBuilder("Pad ").append((SmcPadFragment.this.currentOctive * 16) + ((Integer) item).intValue() + 1 + SmcPadFragment.this.currentTranspose) : new StringBuilder("Pad ").append(((Integer) item).intValue() + 113)).toString());
                SmcPadFragment.this.smcPadPadDialog.show();
            }
        });
        this.smcPadPad.setVisibility(4);
        SmcPadButton smcPadButton = (SmcPadButton) this.m_mainView.findViewById(R.id.smc_pad_button);
        this.smcPadButton = smcPadButton;
        smcPadButton.callback = new SmcPadButtonCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.11
            @Override // com.cubeSuite.fragment.smcPad.SmcPadButtonCallback
            public void onButtonClick(int i) {
                String str;
                if (i == 0) {
                    str = "Left Button";
                } else if (i == 1) {
                    str = "Right Button";
                } else if (i == 2) {
                    str = "Play Button";
                } else if (i == 3) {
                    str = "Stop Button";
                } else if (i != 4) {
                    str = "";
                } else {
                    str = "Record Button";
                }
                SmcPadFragment.this.smcPadButDialog.updateData(SmcPadFragment.this.entry.usr[SmcPadFragment.this.currentPreset].smcPadButtons[i], str);
                SmcPadFragment.this.smcPadButDialog.show();
            }
        };
        this.smcPadButton.setVisibility(4);
    }

    public void saveConfig() {
        this.smcPadCommunication.saveConfig(0, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.12
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance(SmcPadFragment.this.getActivity()).showDialog().setTitle(R.string.success).setContent(R.string.saveConfigSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                } else {
                    AlertDialogUtil.getInstance(SmcPadFragment.this.getActivity()).showDialog().setTitle(R.string.failure).setContent(R.string.saveConfigFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                }
            }
        });
    }

    public void resetConfig() {
        AlertDialogUtil.getInstance(getActivity()).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.13
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                AlertDialogUtil.getInstance(SmcPadFragment.this.getActivity()).setTitle(R.string.hint).setContent(R.string.recoveringData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
                byte[] file = FileUtils.readFile(SmcPadFragment.this.getResources().openRawResource(R.raw.spb));
                Objects.requireNonNull(SmcPadFragment.this.entry);
                final byte[] bArr = new byte[28312];
                Objects.requireNonNull(SmcPadFragment.this.entry);
                System.arraycopy(file, 0, bArr, 0, 28312);
                SmcPadFragment.this.smcPadCommunication.resetFactory(bArr, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.13.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            SmcPadFragment.this.entry.setUsrData(bArr);
                            SmcPadFragment.this.handler.sendEmptyMessageDelayed(0, 2000L);
                        } else {
                            AlertDialogUtil.getInstance(SmcPadFragment.this.getActivity()).showDialog().setTitle(R.string.error).setContent(R.string.resetFailure).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.13.1.1
                            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
                        }
                    }
                });
            }
        }, false).setTitle(R.string.warning).setContent(R.string.hintResetFactory).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        this.entry = new SmcPadEntry();
        CommunicationUtil communicationUtil = communicationUtil;
        Objects.requireNonNull(this.entry);
        communicationUtil.splitReadData((byte) 4, 0L, 12, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.14
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    SmcPadFragment.this.entry.setGlobData(bArr);
                    SmcPadFragment smcPadFragment = SmcPadFragment.this;
                    smcPadFragment.currentPreset = smcPadFragment.entry.smcPadGlob.preset.getData();
                    SmcPadFragment smcPadFragment2 = SmcPadFragment.this;
                    smcPadFragment2.currentOctive = smcPadFragment2.entry.smcPadGlob.octave.getData();
                    SmcPadFragment smcPadFragment3 = SmcPadFragment.this;
                    smcPadFragment3.currentTranspose = smcPadFragment3.entry.smcPadGlob.transpose.getData();
                    SmcPadFragment smcPadFragment4 = SmcPadFragment.this;
                    smcPadFragment4.currentPadbank = smcPadFragment4.entry.smcPadGlob.padbank.getData();
                    SmcPadFragment.this.smcPadPad.updateData(SmcPadFragment.this.entry.smcPadGlob);
                    CommunicationUtil communicationUtil2 = ControlBaseFragment.communicationUtil;
                    Objects.requireNonNull(SmcPadFragment.this.entry);
                    communicationUtil2.splitReadData((byte) 5, 0L, 28312, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smcPad.SmcPadFragment.14.1
                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                        public void readDataCallback(boolean z2, byte[] bArr2) {
                            if (z2) {
                                SmcPadFragment.this.entry.setUsrData(bArr2);
                                SmcPadFragment.this.handler.sendEmptyMessageDelayed(0, 1000L);
                            } else {
                                AlertDialogUtil.getInstance(SmcPadFragment.this.activity).showDialog().setTitle(R.string.error).setContent(R.string.connect_fail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
                            }
                        }
                    });
                    return;
                }
                AlertDialogUtil.getInstance(SmcPadFragment.this.activity).showDialog().setTitle(R.string.error).setContent(R.string.connect_fail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }
}
