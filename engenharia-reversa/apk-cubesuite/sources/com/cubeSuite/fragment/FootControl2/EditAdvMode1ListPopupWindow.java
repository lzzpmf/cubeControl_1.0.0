package com.cubeSuite.fragment.FootControl2;

import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.fc2.AdvCustomStruct1;
import com.cubeSuite.entity.fc2.MidiCodeStruct;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class EditAdvMode1ListPopupWindow extends PopupWindow {
    AdvCustomStruct1 advCustomStruct1;
    Button btnModeSwitch;
    CommunicationUtil communicationUtil;
    View containerView;
    Context context;
    LinearLayout midiInfoListA;
    LinearLayout midiInfoListB;
    ScrollView midiInfoListScrollA;
    ScrollView midiInfoListScrollB;
    RadioGroup radioGroup;
    SelectMode1PopupWindow selectMode1PopupWindow;
    private final int CHANNEL_NUM = 16;
    private Fc2AdvMode1MidiItem[] midiListA = new Fc2AdvMode1MidiItem[16];
    private Fc2AdvMode1MidiItem[] midiListB = new Fc2AdvMode1MidiItem[16];
    private int groupIndex = 0;
    private final int SYS_MODE = 4;
    int[] mode = {R.string.singleStepOne, R.string.singleStepTwo, R.string.onTheLooseTwo, R.string.longOn, R.string.singleOrLongOn};

    public EditAdvMode1ListPopupWindow(final Context context, View view, String str, CommunicationUtil communicationUtil) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_adv1_layout, (ViewGroup) null);
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
        ((TextView) viewInflate.findViewById(R.id.footSwitchText)).setText(str);
        this.btnModeSwitch.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m139x376d4901(view2);
            }
        });
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m140x852cc102(view2);
            }
        });
        button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m141xd2ec3903(view2);
            }
        });
        buttonIconView2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m142x20abb104(view2);
            }
        });
        buttonIconView3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m143x6e6b2905(context, view2);
            }
        });
        buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow$$ExternalSyntheticLambda5
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m144xbc2aa106(view2);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m139x376d4901(View view) {
        showSelectModePopupWindow();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m140x852cc102(View view) {
        if (this.groupIndex == 0) {
            return;
        }
        this.groupIndex = 0;
        this.midiInfoListScrollA.setVisibility(0);
        this.midiInfoListScrollB.setVisibility(8);
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m141xd2ec3903(View view) {
        if (this.groupIndex == 1) {
            return;
        }
        this.groupIndex = 1;
        this.midiInfoListScrollA.setVisibility(8);
        this.midiInfoListScrollB.setVisibility(0);
    }

    /* JADX INFO: renamed from: lambda$new$3$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m142x20abb104(View view) {
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

    /* JADX INFO: renamed from: lambda$new$4$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m143x6e6b2905(Context context, View view) {
        if (isSys()) {
            return;
        }
        AlertDialogUtil.getInstance((Activity) context).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.1
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                for (int i = 0; i < 16; i++) {
                    EditAdvMode1ListPopupWindow.this.getMidiCode(i).reset();
                }
                EditAdvMode1ListPopupWindow.this.updateMidiInfo();
            }
        }).setTitle(R.string.hint).setContent(R.string.removeAllMidiInfo).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    /* JADX INFO: renamed from: lambda$new$5$com-cubeSuite-fragment-FootControl2-EditAdvMode1ListPopupWindow, reason: not valid java name */
    /* synthetic */ void m144xbc2aa106(View view) {
        dismiss();
    }

    void updateView(final AdvCustomStruct1 advCustomStruct1) {
        this.advCustomStruct1 = advCustomStruct1;
        this.btnModeSwitch.setText(this.mode[advCustomStruct1.mode.getData()]);
        this.midiInfoListA.removeAllViews();
        this.midiInfoListB.removeAllViews();
        for (int i = 0; i < 16; i++) {
            SlidingDeleteItem slidingDeleteItem = new SlidingDeleteItem(this.context);
            View viewInflate = LayoutInflater.from(this.context).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate2 = LayoutInflater.from(this.context).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem.setScrollView(this.midiInfoListScrollA);
            slidingDeleteItem.addContentView(viewInflate);
            slidingDeleteItem.addMenuView(viewInflate2);
            slidingDeleteItem.setBtnDelete((Button) viewInflate2.findViewById(R.id.btnDelete));
            this.midiListA[i] = new Fc2AdvMode1MidiItem(i, slidingDeleteItem, (TextView) viewInflate.findViewById(R.id.textStr), advCustomStruct1.midiCodeA, advCustomStruct1.sysExA, new Fc2AdvMode1MidiItemCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.2
                @Override // com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItemCallback
                public void onDelete(int i2) {
                    EditAdvMode1ListPopupWindow.this.updateMidiInfo();
                }

                @Override // com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItemCallback
                public void showEditAdvMode1PopupWindow(int i2) {
                    new EditAdvMode1InfoPopupWindow(EditAdvMode1ListPopupWindow.this.context, i2, advCustomStruct1.midiCodeA[i2], advCustomStruct1.sysExA, new EditAdvMode1InfoPopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.2.1
                        @Override // com.cubeSuite.fragment.FootControl2.EditAdvMode1InfoPopupWindowCallback
                        public void update(MidiCodeStruct midiCodeStruct) {
                            if (midiCodeStruct.type.getData() == 4) {
                                EditAdvMode1ListPopupWindow.this.getMidiCode(0).copy(midiCodeStruct);
                                for (int i3 = 1; i3 < 16; i3++) {
                                    EditAdvMode1ListPopupWindow.this.getMidiCode(i3).isEnable.setData(0);
                                }
                            }
                            EditAdvMode1ListPopupWindow.this.updateMidiInfo();
                        }
                    }).showAtLocation(EditAdvMode1ListPopupWindow.this.containerView, 80, 0, 0);
                }
            });
            this.midiInfoListA.addView(slidingDeleteItem);
        }
        for (int i2 = 0; i2 < 16; i2++) {
            SlidingDeleteItem slidingDeleteItem2 = new SlidingDeleteItem(this.context);
            View viewInflate3 = LayoutInflater.from(this.context).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate4 = LayoutInflater.from(this.context).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem2.setScrollView(this.midiInfoListScrollB);
            slidingDeleteItem2.addContentView(viewInflate3);
            slidingDeleteItem2.addMenuView(viewInflate4);
            slidingDeleteItem2.setBtnDelete((Button) viewInflate4.findViewById(R.id.btnDelete));
            this.midiListB[i2] = new Fc2AdvMode1MidiItem(i2, slidingDeleteItem2, (TextView) viewInflate3.findViewById(R.id.textStr), advCustomStruct1.midiCodeB, advCustomStruct1.sysExB, new Fc2AdvMode1MidiItemCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.3
                @Override // com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItemCallback
                public void onDelete(int i3) {
                    EditAdvMode1ListPopupWindow.this.updateMidiInfo();
                }

                @Override // com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItemCallback
                public void showEditAdvMode1PopupWindow(int i3) {
                    new EditAdvMode1InfoPopupWindow(EditAdvMode1ListPopupWindow.this.context, i3, advCustomStruct1.midiCodeB[i3], advCustomStruct1.sysExB, new EditAdvMode1InfoPopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.3.1
                        @Override // com.cubeSuite.fragment.FootControl2.EditAdvMode1InfoPopupWindowCallback
                        public void update(MidiCodeStruct midiCodeStruct) {
                            if (midiCodeStruct.type.getData() == 4) {
                                EditAdvMode1ListPopupWindow.this.getMidiCode(0).copy(midiCodeStruct);
                                for (int i4 = 1; i4 < 16; i4++) {
                                    EditAdvMode1ListPopupWindow.this.getMidiCode(i4).isEnable.setData(0);
                                }
                            }
                            EditAdvMode1ListPopupWindow.this.updateMidiInfo();
                        }
                    }).showAtLocation(EditAdvMode1ListPopupWindow.this.containerView, 80, 0, 0);
                }
            });
            this.midiInfoListB.addView(slidingDeleteItem2);
        }
        if (advCustomStruct1.mode.getData() == 0 || advCustomStruct1.mode.getData() == 3) {
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

    private boolean isSys() {
        MidiCodeStruct midiCode = getMidiCode(0);
        return midiCode.isEnable.getData() > 0 && midiCode.type.getData() == 4;
    }

    void showSelectModePopupWindow() {
        if (this.selectMode1PopupWindow == null) {
            this.selectMode1PopupWindow = new SelectMode1PopupWindow(this.context, this.advCustomStruct1, new SelectMode1PopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditAdvMode1ListPopupWindow.4
                @Override // com.cubeSuite.fragment.FootControl2.SelectMode1PopupWindowCallback
                public void onSelect(int i) {
                    EditAdvMode1ListPopupWindow.this.btnModeSwitch.setText(EditAdvMode1ListPopupWindow.this.mode[i]);
                    if (i == 0 || i == 3) {
                        EditAdvMode1ListPopupWindow.this.groupIndex = 0;
                        EditAdvMode1ListPopupWindow.this.radioGroup.check(R.id.midiA);
                        EditAdvMode1ListPopupWindow.this.radioGroup.setVisibility(8);
                        EditAdvMode1ListPopupWindow.this.midiInfoListScrollA.setVisibility(0);
                        EditAdvMode1ListPopupWindow.this.midiInfoListScrollB.setVisibility(8);
                    } else {
                        EditAdvMode1ListPopupWindow.this.radioGroup.setVisibility(0);
                    }
                    EditAdvMode1ListPopupWindow.this.sendData();
                }
            });
        }
        this.selectMode1PopupWindow.showAtLocation(this.containerView, 80, 0, 0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendData() {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Byte.valueOf(this.advCustomStruct1.mode.getByteData()));
        for (int i = 0; i < this.advCustomStruct1.midiCodeA.length; i++) {
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeA[i].isEnable.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeA[i].channel.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeA[i].type.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeA[i].data1.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeA[i].data2.getByteData()));
        }
        for (int i2 = 0; i2 < this.advCustomStruct1.midiCodeB.length; i2++) {
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeB[i2].isEnable.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeB[i2].channel.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeB[i2].type.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeB[i2].data1.getByteData()));
            arrayList.add(Byte.valueOf(this.advCustomStruct1.midiCodeB[i2].data2.getByteData()));
        }
        for (int i3 = 0; i3 < this.advCustomStruct1.sysExA.length; i3++) {
            arrayList.add(Byte.valueOf(this.advCustomStruct1.sysExA[i3]));
        }
        for (int i4 = 0; i4 < this.advCustomStruct1.sysExB.length; i4++) {
            arrayList.add(Byte.valueOf(this.advCustomStruct1.sysExB[i4]));
        }
        this.communicationUtil.splitWriteData((byte) 4, arrayList, this.advCustomStruct1.mode.getAddr());
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
            return this.advCustomStruct1.midiCodeA[i];
        }
        return this.advCustomStruct1.midiCodeB[i];
    }
}
