package com.cubeSuite.fragment.FootControl2;

import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.SpinnerAdapter;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.widget.AppCompatSpinner;
import com.cubeSuite.R;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.fc2.MidiCodeStruct;
import com.cubeSuite.entity.fc2.MixKeyStruct;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class MixKeyListPopupWindow extends PopupWindow {
    Button btnModeSwitch;
    private AppCompatSpinner comb1;
    private AppCompatSpinner comb2;
    private AppCompatSpinner comb3;
    CommunicationUtil communicationUtil;
    View containerView;
    Context context;
    LinearLayout midiInfoListA;
    LinearLayout midiInfoListB;
    ScrollView midiInfoListScrollA;
    ScrollView midiInfoListScrollB;
    MixKeyStruct mixKey;
    View mixKeyKeyTuner;
    View mixKeyKeyboard;
    View mixKeyMidi;
    private AppCompatSpinner mixKeyTypeSpinner;
    RadioGroup radioGroup;
    SelectMixKeyModePopupWindow selectMixKeyModePopupWindow;
    private AppCompatSpinner tunerSpinner;
    private final int CHANNEL_NUM = 16;
    private Fc2MixKeyMidiItem[] midiListA = new Fc2MixKeyMidiItem[16];
    private Fc2MixKeyMidiItem[] midiListB = new Fc2MixKeyMidiItem[16];
    private int groupIndex = 0;
    private final int SYS_MODE = 4;
    int[] mode = {R.string.singleStepOne, R.string.singleStepTwo, R.string.onTheLooseTwo, R.string.longOn, R.string.singleOrLongOn};
    int com1Index = 0;
    int com2Index = 0;
    int com3Index = 0;

    public MixKeyListPopupWindow(final Context context, View view, String str, final CommunicationUtil communicationUtil) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_mix_key_layout, (ViewGroup) null);
        this.containerView = view;
        this.communicationUtil = communicationUtil;
        this.context = context;
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        ButtonIconView buttonIconView = (ButtonIconView) viewInflate.findViewById(R.id.back);
        this.midiInfoListScrollA = (ScrollView) viewInflate.findViewById(R.id.midiInfoListScrollA);
        this.midiInfoListA = (LinearLayout) viewInflate.findViewById(R.id.midiInfoListA);
        this.midiInfoListScrollB = (ScrollView) viewInflate.findViewById(R.id.midiInfoListScrollB);
        this.midiInfoListB = (LinearLayout) viewInflate.findViewById(R.id.midiInfoListB);
        ButtonIconView buttonIconView2 = (ButtonIconView) viewInflate.findViewById(R.id.plusMidiCode);
        ButtonIconView buttonIconView3 = (ButtonIconView) viewInflate.findViewById(R.id.deleteList);
        this.radioGroup = (RadioGroup) viewInflate.findViewById(R.id.midiGroup);
        Button button = (Button) viewInflate.findViewById(R.id.midiA);
        Button button2 = (Button) viewInflate.findViewById(R.id.midiB);
        this.btnModeSwitch = (Button) viewInflate.findViewById(R.id.modeSwitch);
        TextView textView = (TextView) viewInflate.findViewById(R.id.footSwitchText);
        this.mixKeyKeyTuner = viewInflate.findViewById(R.id.mixKeyKeyTuner);
        this.mixKeyKeyboard = viewInflate.findViewById(R.id.mixKeyKeyboard);
        this.mixKeyMidi = viewInflate.findViewById(R.id.mixKeyMidi);
        this.mixKeyTypeSpinner = (AppCompatSpinner) viewInflate.findViewById(R.id.mixKeyTypeSpinner);
        this.tunerSpinner = (AppCompatSpinner) viewInflate.findViewById(R.id.tunerSpinner);
        this.comb1 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb1);
        this.comb2 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb2);
        this.comb3 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb3);
        ArrayList arrayList = new ArrayList();
        arrayList.add("↑");
        arrayList.add("↓");
        arrayList.add("←");
        arrayList.add("→");
        arrayList.add("Page up");
        arrayList.add("Page down");
        arrayList.add("Last song");
        arrayList.add("Next song");
        arrayList.add("Vol-");
        arrayList.add("Vol+");
        this.tunerSpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, arrayList));
        this.tunerSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.1
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view2, int i, long j) {
                MixKeyListPopupWindow.this.mixKey.tunerdat.setData(i);
                communicationUtil.splitWriteData((byte) 4, new byte[]{MixKeyListPopupWindow.this.mixKey.tunerdat.getByteData()}, MixKeyListPopupWindow.this.mixKey.tunerdat.getAddr());
            }
        });
        this.comb1.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, context.getResources().getStringArray(R.array.comb_csa)));
        this.comb1.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.2
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view2, int i, long j) {
                if (MixKeyListPopupWindow.this.com1Index == i) {
                    return;
                }
                if (i != 0 && i == MixKeyListPopupWindow.this.com2Index) {
                    Toast.makeText(context, R.string.KeyCombinationError, 0).show();
                    MixKeyListPopupWindow.this.comb1.setSelection(MixKeyListPopupWindow.this.com1Index);
                } else {
                    MixKeyListPopupWindow.this.com1Index = i;
                    MixKeyListPopupWindow.this.mixKey.hidkeydat[0].setData(MixKeyListPopupWindow.this.com1Index == 0 ? 0 : MixKeyListPopupWindow.this.com1Index + 62);
                    communicationUtil.splitWriteData((byte) 4, new byte[]{MixKeyListPopupWindow.this.mixKey.hidkeydat[0].getByteData()}, MixKeyListPopupWindow.this.mixKey.hidkeydat[0].getAddr());
                }
            }
        });
        this.comb2.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, context.getResources().getStringArray(R.array.comb_csa)));
        this.comb2.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.3
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view2, int i, long j) {
                if (MixKeyListPopupWindow.this.com2Index == i) {
                    return;
                }
                if (i != 0 && i == MixKeyListPopupWindow.this.com1Index) {
                    Toast.makeText(context, R.string.KeyCombinationError, 0).show();
                    MixKeyListPopupWindow.this.comb2.setSelection(MixKeyListPopupWindow.this.com2Index);
                } else {
                    MixKeyListPopupWindow.this.com2Index = i;
                    MixKeyListPopupWindow.this.mixKey.hidkeydat[1].setData(MixKeyListPopupWindow.this.com2Index == 0 ? 0 : MixKeyListPopupWindow.this.com2Index + 62);
                    communicationUtil.splitWriteData((byte) 4, new byte[]{MixKeyListPopupWindow.this.mixKey.hidkeydat[1].getByteData()}, MixKeyListPopupWindow.this.mixKey.hidkeydat[1].getAddr());
                }
            }
        });
        ArrayList arrayList2 = new ArrayList();
        arrayList2.add(context.getResources().getStringArray(R.array.comb_csa)[0]);
        for (int i = 0; i < 26; i++) {
            arrayList2.add("" + ((char) (i + 65)));
        }
        arrayList2.add("↑");
        arrayList2.add("↓");
        arrayList2.add("←");
        arrayList2.add("→");
        arrayList2.add("PAGE UP");
        arrayList2.add("PAGE DOWN");
        arrayList2.add("SPACE");
        arrayList2.add("Enter");
        arrayList2.add("ESC");
        arrayList2.add("Delete");
        arrayList2.add("Capslock");
        arrayList2.add("Tab");
        for (int i2 = 0; i2 < 10; i2++) {
            arrayList2.add(String.valueOf(i2));
        }
        arrayList2.add("+");
        arrayList2.add("-");
        for (int i3 = 1; i3 < 13; i3++) {
            arrayList2.add("F" + i3);
        }
        this.comb3.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, arrayList2));
        this.comb3.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.4
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view2, int i4, long j) {
                if (MixKeyListPopupWindow.this.com3Index == i4) {
                    return;
                }
                MixKeyListPopupWindow.this.com3Index = i4;
                MixKeyListPopupWindow.this.mixKey.hidkeydat[2].setData(MixKeyListPopupWindow.this.com3Index);
                communicationUtil.splitWriteData((byte) 4, new byte[]{MixKeyListPopupWindow.this.mixKey.hidkeydat[2].getByteData()}, MixKeyListPopupWindow.this.mixKey.hidkeydat[2].getAddr());
            }
        });
        ArrayList arrayList3 = new ArrayList();
        arrayList3.add("Custom Midi");
        arrayList3.add("Custom Keyboard");
        arrayList3.add("Turner");
        this.mixKeyTypeSpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, arrayList3));
        this.mixKeyTypeSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.5
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view2, int i4, long j) {
                MixKeyListPopupWindow.this.mixKey.type.setData(i4);
                if (MixKeyListPopupWindow.this.mixKey.type.getData() == 0) {
                    MixKeyListPopupWindow.this.mixKeyMidi.setVisibility(0);
                    MixKeyListPopupWindow.this.mixKeyKeyboard.setVisibility(8);
                    MixKeyListPopupWindow.this.mixKeyKeyTuner.setVisibility(8);
                } else if (MixKeyListPopupWindow.this.mixKey.type.getData() == 1) {
                    MixKeyListPopupWindow.this.mixKeyMidi.setVisibility(8);
                    MixKeyListPopupWindow.this.mixKeyKeyboard.setVisibility(0);
                    MixKeyListPopupWindow.this.mixKeyKeyTuner.setVisibility(8);
                } else {
                    MixKeyListPopupWindow.this.mixKeyMidi.setVisibility(8);
                    MixKeyListPopupWindow.this.mixKeyKeyboard.setVisibility(8);
                    MixKeyListPopupWindow.this.mixKeyKeyTuner.setVisibility(0);
                }
                communicationUtil.splitWriteData((byte) 4, new byte[]{MixKeyListPopupWindow.this.mixKey.type.getByteData()}, MixKeyListPopupWindow.this.mixKey.type.getAddr());
            }
        });
        textView.setText(str);
        this.btnModeSwitch.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m177xe200303(view2);
            }
        });
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m178x1ed5cfc4(view2);
            }
        });
        button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m179x2f8b9c85(view2);
            }
        });
        buttonIconView2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m180x40416946(view2);
            }
        });
        buttonIconView3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m181x50f73607(context, view2);
            }
        });
        buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow$$ExternalSyntheticLambda5
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m182x61ad02c8(view2);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m177xe200303(View view) {
        showSelectModePopupWindow();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m178x1ed5cfc4(View view) {
        if (this.groupIndex == 0) {
            return;
        }
        this.groupIndex = 0;
        this.midiInfoListScrollA.setVisibility(0);
        this.midiInfoListScrollB.setVisibility(8);
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m179x2f8b9c85(View view) {
        if (this.groupIndex == 1) {
            return;
        }
        this.groupIndex = 1;
        this.midiInfoListScrollA.setVisibility(8);
        this.midiInfoListScrollB.setVisibility(0);
    }

    /* JADX INFO: renamed from: lambda$new$3$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m180x40416946(View view) {
        if (isSys() || getMidiCode(15).isEnable.getData() == 1) {
            return;
        }
        for (int i = 0; i < 16; i++) {
            MidiCodeStruct midiCode = getMidiCode(i);
            if (midiCode.isEnable.getData() == 0) {
                midiCode.reset();
                midiCode.isEnable.setData(1);
                break;
            }
        }
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$new$4$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m181x50f73607(Context context, View view) {
        if (isSys()) {
            return;
        }
        AlertDialogUtil.getInstance((Activity) context).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.6
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                for (int i = 0; i < 16; i++) {
                    MixKeyListPopupWindow.this.getMidiCode(i).reset();
                }
                MixKeyListPopupWindow.this.updateMidiInfo();
            }
        }).setTitle(R.string.hint).setContent(R.string.removeAllMidiInfo).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    /* JADX INFO: renamed from: lambda$new$5$com-cubeSuite-fragment-FootControl2-MixKeyListPopupWindow, reason: not valid java name */
    /* synthetic */ void m182x61ad02c8(View view) {
        dismiss();
    }

    void updateView(final MixKeyStruct mixKeyStruct) {
        int i;
        this.mixKey = mixKeyStruct;
        this.comb1.setSelection(mixKeyStruct.hidkeydat[0].getData() == 0 ? 0 : mixKeyStruct.hidkeydat[0].getData() - 62);
        this.comb2.setSelection(mixKeyStruct.hidkeydat[1].getData() == 0 ? 0 : mixKeyStruct.hidkeydat[1].getData() - 62);
        this.comb3.setSelection(mixKeyStruct.hidkeydat[2].getData());
        this.mixKeyTypeSpinner.setSelection(mixKeyStruct.type.getData());
        this.tunerSpinner.setSelection(mixKeyStruct.tunerdat.getData());
        this.btnModeSwitch.setText(this.mode[mixKeyStruct.midiUsrPlus.mode.getData()]);
        this.midiInfoListA.removeAllViews();
        this.midiInfoListB.removeAllViews();
        if (mixKeyStruct.type.getData() == 0) {
            this.mixKeyMidi.setVisibility(0);
            this.mixKeyKeyboard.setVisibility(8);
            this.mixKeyKeyTuner.setVisibility(8);
        } else if (mixKeyStruct.type.getData() == 1) {
            this.mixKeyMidi.setVisibility(8);
            this.mixKeyKeyboard.setVisibility(0);
            this.mixKeyKeyTuner.setVisibility(8);
        } else {
            this.mixKeyMidi.setVisibility(8);
            this.mixKeyKeyboard.setVisibility(8);
            this.mixKeyKeyTuner.setVisibility(0);
        }
        int i2 = 0;
        while (true) {
            i = R.id.textStr;
            if (i2 >= 16) {
                break;
            }
            SlidingDeleteItem slidingDeleteItem = new SlidingDeleteItem(this.context);
            View viewInflate = LayoutInflater.from(this.context).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate2 = LayoutInflater.from(this.context).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem.setScrollView(this.midiInfoListScrollA);
            slidingDeleteItem.addContentView(viewInflate);
            slidingDeleteItem.addMenuView(viewInflate2);
            slidingDeleteItem.setBtnDelete((Button) viewInflate2.findViewById(R.id.btnDelete));
            this.midiListA[i2] = new Fc2MixKeyMidiItem(i2, slidingDeleteItem, (TextView) viewInflate.findViewById(R.id.textStr), mixKeyStruct.midiUsrPlus.midiCodeA, mixKeyStruct.midiUsrPlus.sysExA, new Fc2MixKeyMidiItemCallback() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.7
                @Override // com.cubeSuite.fragment.FootControl2.Fc2MixKeyMidiItemCallback
                public void onDelete(int i3) {
                    MixKeyListPopupWindow.this.updateMidiInfo();
                }

                @Override // com.cubeSuite.fragment.FootControl2.Fc2MixKeyMidiItemCallback
                public void showEditMixKeyPopupWindow(int i3) {
                    new MixKeyInfoPopupWindow(MixKeyListPopupWindow.this.context, i3, mixKeyStruct.midiUsrPlus.midiCodeA[i3], mixKeyStruct.midiUsrPlus.sysExA, new MixKeyInfoPopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.7.1
                        @Override // com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindowCallback
                        public void update(MidiCodeStruct midiCodeStruct) {
                            if (midiCodeStruct.type.getData() == 4) {
                                MixKeyListPopupWindow.this.getMidiCode(0).copy(midiCodeStruct);
                                for (int i4 = 1; i4 < 16; i4++) {
                                    MixKeyListPopupWindow.this.getMidiCode(i4).isEnable.setData(0);
                                }
                            }
                            MixKeyListPopupWindow.this.updateMidiInfo();
                        }
                    }).showAtLocation(MixKeyListPopupWindow.this.containerView, 80, 0, 0);
                }
            });
            this.midiInfoListA.addView(slidingDeleteItem);
            i2++;
        }
        int i3 = 0;
        while (i3 < 16) {
            SlidingDeleteItem slidingDeleteItem2 = new SlidingDeleteItem(this.context);
            View viewInflate3 = LayoutInflater.from(this.context).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate4 = LayoutInflater.from(this.context).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem2.setScrollView(this.midiInfoListScrollB);
            slidingDeleteItem2.addContentView(viewInflate3);
            slidingDeleteItem2.addMenuView(viewInflate4);
            slidingDeleteItem2.setBtnDelete((Button) viewInflate4.findViewById(R.id.btnDelete));
            this.midiListB[i3] = new Fc2MixKeyMidiItem(i3, slidingDeleteItem2, (TextView) viewInflate3.findViewById(i), mixKeyStruct.midiUsrPlus.midiCodeB, mixKeyStruct.midiUsrPlus.sysExB, new Fc2MixKeyMidiItemCallback() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.8
                @Override // com.cubeSuite.fragment.FootControl2.Fc2MixKeyMidiItemCallback
                public void onDelete(int i4) {
                    MixKeyListPopupWindow.this.updateMidiInfo();
                }

                @Override // com.cubeSuite.fragment.FootControl2.Fc2MixKeyMidiItemCallback
                public void showEditMixKeyPopupWindow(int i4) {
                    new MixKeyInfoPopupWindow(MixKeyListPopupWindow.this.context, i4, mixKeyStruct.midiUsrPlus.midiCodeB[i4], mixKeyStruct.midiUsrPlus.sysExB, new MixKeyInfoPopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.8.1
                        @Override // com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindowCallback
                        public void update(MidiCodeStruct midiCodeStruct) {
                            if (midiCodeStruct.type.getData() == 4) {
                                MixKeyListPopupWindow.this.getMidiCode(0).copy(midiCodeStruct);
                                for (int i5 = 1; i5 < 16; i5++) {
                                    MixKeyListPopupWindow.this.getMidiCode(i5).isEnable.setData(0);
                                }
                            }
                            MixKeyListPopupWindow.this.updateMidiInfo();
                        }
                    }).showAtLocation(MixKeyListPopupWindow.this.containerView, 80, 0, 0);
                }
            });
            this.midiInfoListB.addView(slidingDeleteItem2);
            i3++;
            i = R.id.textStr;
        }
        if (mixKeyStruct.midiUsrPlus.mode.getData() == 0 || mixKeyStruct.midiUsrPlus.mode.getData() == 3) {
            this.radioGroup.setVisibility(8);
        }
        if (this.groupIndex == 0) {
            this.midiInfoListScrollA.setVisibility(0);
            this.midiInfoListScrollB.setVisibility(8);
        } else {
            this.midiInfoListScrollA.setVisibility(8);
            this.midiInfoListScrollB.setVisibility(0);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendData() {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.mode.getByteData()));
        for (int i = 0; i < this.mixKey.midiUsrPlus.midiCodeA.length; i++) {
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeA[i].isEnable.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeA[i].channel.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeA[i].type.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeA[i].data1.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeA[i].data2.getByteData()));
        }
        for (int i2 = 0; i2 < this.mixKey.midiUsrPlus.midiCodeB.length; i2++) {
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeB[i2].isEnable.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeB[i2].channel.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeB[i2].type.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeB[i2].data1.getByteData()));
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.midiCodeB[i2].data2.getByteData()));
        }
        for (int i3 = 0; i3 < this.mixKey.midiUsrPlus.sysExA.length; i3++) {
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.sysExA[i3]));
        }
        for (int i4 = 0; i4 < this.mixKey.midiUsrPlus.sysExB.length; i4++) {
            arrayList.add(Byte.valueOf(this.mixKey.midiUsrPlus.sysExB[i4]));
        }
        this.communicationUtil.splitWriteData((byte) 4, arrayList, this.mixKey.midiUsrPlus.mode.getAddr());
    }

    private boolean isSys() {
        MidiCodeStruct midiCode = getMidiCode(0);
        return midiCode.isEnable.getData() > 0 && midiCode.type.getData() == 4;
    }

    void showSelectModePopupWindow() {
        if (this.selectMixKeyModePopupWindow == null) {
            this.selectMixKeyModePopupWindow = new SelectMixKeyModePopupWindow(this.context, this.mixKey.midiUsrPlus, new SelectMixKeyModePopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyListPopupWindow.9
                @Override // com.cubeSuite.fragment.FootControl2.SelectMixKeyModePopupWindowCallback
                public void onSelect(int i) {
                    MixKeyListPopupWindow.this.btnModeSwitch.setText(MixKeyListPopupWindow.this.mode[i]);
                    if (i == 0 || i == 3) {
                        MixKeyListPopupWindow.this.groupIndex = 0;
                        MixKeyListPopupWindow.this.radioGroup.check(R.id.midiA);
                        MixKeyListPopupWindow.this.radioGroup.setVisibility(8);
                        MixKeyListPopupWindow.this.midiInfoListScrollA.setVisibility(0);
                        MixKeyListPopupWindow.this.midiInfoListScrollB.setVisibility(8);
                    } else {
                        MixKeyListPopupWindow.this.radioGroup.setVisibility(0);
                    }
                    MixKeyListPopupWindow.this.sendData();
                }
            });
        }
        this.selectMixKeyModePopupWindow.showAtLocation(this.containerView, 80, 0, 0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void updateMidiInfo() {
        int i = 0;
        if (this.groupIndex == 0) {
            while (i < 16) {
                this.midiListA[i].updateText();
                i++;
            }
        } else {
            while (i < 16) {
                this.midiListB[i].updateText();
                i++;
            }
        }
        sendData();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public MidiCodeStruct getMidiCode(int i) {
        if (this.groupIndex == 0) {
            return this.mixKey.midiUsrPlus.midiCodeA[i];
        }
        return this.mixKey.midiUsrPlus.midiCodeB[i];
    }
}
