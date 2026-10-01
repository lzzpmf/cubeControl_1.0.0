package com.cubeSuite.fragment.FootControl2;

import android.view.View;
import android.widget.TextView;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.fc2.MidiCodeStruct;

/* JADX INFO: loaded from: classes.dex */
public class Fc2InterfaceMidiItem {
    private final int CHANNEL_NUM = 16;
    private SlidingDeleteItem body;
    private int index;
    MidiCodeStruct[] midiCodeTap;
    private TextView textStr;

    public Fc2InterfaceMidiItem(final int i, final SlidingDeleteItem slidingDeleteItem, TextView textView, MidiCodeStruct[] midiCodeStructArr, final Fc2InterfaceMidiItemCallback fc2InterfaceMidiItemCallback) {
        this.index = i;
        this.body = slidingDeleteItem;
        this.textStr = textView;
        this.midiCodeTap = midiCodeStructArr;
        textView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.Fc2InterfaceMidiItem$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                fc2InterfaceMidiItemCallback.showEditInterfacePopupWindow(i);
            }
        });
        slidingDeleteItem.getBtnDelete().setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.Fc2InterfaceMidiItem$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m168x31df060f(slidingDeleteItem, i, fc2InterfaceMidiItemCallback, view);
            }
        });
        updateText();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-Fc2InterfaceMidiItem, reason: not valid java name */
    /* synthetic */ void m168x31df060f(SlidingDeleteItem slidingDeleteItem, int i, Fc2InterfaceMidiItemCallback fc2InterfaceMidiItemCallback, View view) {
        slidingDeleteItem.deleteAnimator();
        for (int i2 = 0; i2 < 16; i2++) {
            if (i2 >= i) {
                MidiCodeStruct midiCode = getMidiCode(i2);
                if (i2 == 15) {
                    midiCode.isEnable.setData(0);
                    break;
                }
                MidiCodeStruct midiCode2 = getMidiCode(i2 + 1);
                if (midiCode2.isEnable.getData() == 0) {
                    midiCode.isEnable.setData(0);
                    break;
                }
                midiCode.isEnable.setData(midiCode2.isEnable.getData());
                midiCode.channel.setData(midiCode2.channel.getData());
                midiCode.type.setData(midiCode2.type.getData());
                midiCode.data1.setData(midiCode2.data1.getData());
                midiCode.data2.setData(midiCode2.data2.getData());
            }
        }
        fc2InterfaceMidiItemCallback.onDelete(i);
    }

    private MidiCodeStruct getMidiCode(int i) {
        return this.midiCodeTap[i];
    }

    public void updateText() {
        MidiCodeStruct midiCode = getMidiCode(this.index);
        if (midiCode.isEnable.getData() == 0) {
            this.body.setVisibility(8);
        } else {
            this.body.setVisibility(0);
        }
        this.textStr.setText("[" + (this.index + 1) + "]   " + (midiCode.channel.getData() + 1) + "   CC   " + midiCode.type.getData() + "   " + midiCode.data1.getData() + "~" + midiCode.data2.getData());
    }
}
