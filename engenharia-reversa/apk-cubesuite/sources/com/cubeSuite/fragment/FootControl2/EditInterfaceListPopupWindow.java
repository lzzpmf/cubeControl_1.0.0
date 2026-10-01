package com.cubeSuite.fragment.FootControl2;

import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.ScrollView;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.fc2.MidiCodeStruct;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class EditInterfaceListPopupWindow extends PopupWindow {
    CommunicationUtil communicationUtil;
    View containView;
    Context context;
    MidiCodeStruct[] midiCodeTap;
    LinearLayout midiInfoList;
    ScrollView midiInfoListScroll;
    private final int CHANNEL_NUM = 16;
    private Fc2InterfaceMidiItem[] midiList = new Fc2InterfaceMidiItem[16];

    public EditInterfaceListPopupWindow(final Context context, View view, CommunicationUtil communicationUtil) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_interface_list_layout, (ViewGroup) null);
        this.communicationUtil = communicationUtil;
        this.context = context;
        this.containView = view;
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        ButtonIconView buttonIconView = (ButtonIconView) viewInflate.findViewById(R.id.back);
        this.midiInfoListScroll = (ScrollView) viewInflate.findViewById(R.id.midiInfoListScroll);
        this.midiInfoList = (LinearLayout) viewInflate.findViewById(R.id.midiInfoList);
        ButtonIconView buttonIconView2 = (ButtonIconView) viewInflate.findViewById(R.id.plusMidiCode);
        ButtonIconView buttonIconView3 = (ButtonIconView) viewInflate.findViewById(R.id.deleteList);
        buttonIconView2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m145xfd6d0947(view2);
            }
        });
        buttonIconView3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m146x679c9166(context, view2);
            }
        });
        buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                this.f$0.m147xd1cc1985(view2);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-EditInterfaceListPopupWindow, reason: not valid java name */
    /* synthetic */ void m145xfd6d0947(View view) {
        if (this.midiCodeTap[15].isEnable.getData() == 1) {
            return;
        }
        for (int i = 0; i < 16; i++) {
            MidiCodeStruct midiCodeStruct = this.midiCodeTap[i];
            if (midiCodeStruct.isEnable.getData() == 0) {
                midiCodeStruct.reset();
                midiCodeStruct.isEnable.setData(1);
                break;
            }
        }
        updateMidiInfo();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-EditInterfaceListPopupWindow, reason: not valid java name */
    /* synthetic */ void m146x679c9166(Context context, View view) {
        AlertDialogUtil.getInstance((Activity) context).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow.1
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                for (int i = 0; i < 16; i++) {
                    EditInterfaceListPopupWindow.this.midiCodeTap[i].reset();
                }
                EditInterfaceListPopupWindow.this.updateMidiInfo();
            }
        }).setTitle(R.string.hint).setContent(R.string.removeAllMidiInfo).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-EditInterfaceListPopupWindow, reason: not valid java name */
    /* synthetic */ void m147xd1cc1985(View view) {
        dismiss();
    }

    void updateView(final MidiCodeStruct[] midiCodeStructArr) {
        this.midiCodeTap = midiCodeStructArr;
        this.midiInfoList.removeAllViews();
        int i = 0;
        while (i < 16) {
            SlidingDeleteItem slidingDeleteItem = new SlidingDeleteItem(this.context);
            View viewInflate = LayoutInflater.from(this.context).inflate(R.layout.skip_item, (ViewGroup) null);
            View viewInflate2 = LayoutInflater.from(this.context).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
            slidingDeleteItem.setScrollView(this.midiInfoListScroll);
            slidingDeleteItem.addContentView(viewInflate);
            slidingDeleteItem.addMenuView(viewInflate2);
            slidingDeleteItem.setBtnDelete((Button) viewInflate2.findViewById(R.id.btnDelete));
            MidiCodeStruct[] midiCodeStructArr2 = midiCodeStructArr;
            this.midiList[i] = new Fc2InterfaceMidiItem(i, slidingDeleteItem, (TextView) viewInflate.findViewById(R.id.textStr), midiCodeStructArr2, new Fc2InterfaceMidiItemCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow.2
                @Override // com.cubeSuite.fragment.FootControl2.Fc2InterfaceMidiItemCallback
                public void onDelete(int i2) {
                    EditInterfaceListPopupWindow.this.updateMidiInfo();
                }

                @Override // com.cubeSuite.fragment.FootControl2.Fc2InterfaceMidiItemCallback
                public void showEditInterfacePopupWindow(int i2) {
                    new EditInterfacePopupWindow(EditInterfaceListPopupWindow.this.context, i2, midiCodeStructArr[i2], new EditInterfacePopupWindowCallback() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfaceListPopupWindow.2.1
                        @Override // com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindowCallback
                        public void update() {
                            EditInterfaceListPopupWindow.this.updateMidiInfo();
                        }
                    }).showAtLocation(EditInterfaceListPopupWindow.this.containView, 80, 0, 0);
                }
            });
            this.midiInfoList.addView(slidingDeleteItem);
            i++;
            midiCodeStructArr = midiCodeStructArr2;
        }
    }

    private void sendData() {
        ArrayList arrayList = new ArrayList();
        int i = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeTap;
            if (i < midiCodeStructArr.length) {
                arrayList.add(Byte.valueOf((byte) midiCodeStructArr[i].isEnable.getData()));
                arrayList.add(Byte.valueOf((byte) this.midiCodeTap[i].channel.getData()));
                arrayList.add(Byte.valueOf((byte) this.midiCodeTap[i].type.getData()));
                arrayList.add(Byte.valueOf((byte) this.midiCodeTap[i].data1.getData()));
                arrayList.add(Byte.valueOf((byte) this.midiCodeTap[i].data2.getData()));
                i++;
            } else {
                this.communicationUtil.splitWriteData((byte) 4, arrayList, midiCodeStructArr[0].isEnable.getAddr());
                return;
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void updateMidiInfo() {
        for (int i = 0; i < 16; i++) {
            this.midiList[i].updateText();
        }
        sendData();
    }
}
