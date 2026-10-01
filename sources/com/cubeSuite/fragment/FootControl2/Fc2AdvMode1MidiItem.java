package com.cubeSuite.fragment.FootControl2;

import android.view.View;
import android.widget.TextView;
import com.cubeSuite.customControl.SlidingDeleteItem;
import com.cubeSuite.entity.fc2.MidiCodeStruct;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class Fc2AdvMode1MidiItem {
    private SlidingDeleteItem body;
    private int index;
    MidiCodeStruct[] midiCodeTap;
    byte[] sysEx;
    private TextView textStr;
    private final int CHANNEL_NUM = 16;
    private final int SYS_MODE = 4;
    public final String[] MIDI_TYPE = {"PC", "CC", "Note ON", "Note OFF", "SysEx"};

    public Fc2AdvMode1MidiItem(final int i, final SlidingDeleteItem slidingDeleteItem, TextView textView, MidiCodeStruct[] midiCodeStructArr, byte[] bArr, final Fc2AdvMode1MidiItemCallback fc2AdvMode1MidiItemCallback) {
        this.sysEx = bArr;
        this.index = i;
        this.body = slidingDeleteItem;
        this.textStr = textView;
        this.midiCodeTap = midiCodeStructArr;
        textView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItem$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                fc2AdvMode1MidiItemCallback.showEditAdvMode1PopupWindow(i);
            }
        });
        slidingDeleteItem.getBtnDelete().setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.Fc2AdvMode1MidiItem$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m167xec36f4cd(slidingDeleteItem, i, fc2AdvMode1MidiItemCallback, view);
            }
        });
        updateText();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-Fc2AdvMode1MidiItem, reason: not valid java name */
    /* synthetic */ void m167xec36f4cd(SlidingDeleteItem slidingDeleteItem, int i, Fc2AdvMode1MidiItemCallback fc2AdvMode1MidiItemCallback, View view) {
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
        fc2AdvMode1MidiItemCallback.onDelete(i);
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
        if (midiCode.type.getData() == 4) {
            this.textStr.setText("send:" + byteToX16String(this.sysEx));
        } else {
            this.textStr.setText("[" + (this.index + 1) + "]   " + (midiCode.channel.getData() + 1) + "   " + this.MIDI_TYPE[midiCode.type.getData()] + "   " + midiCode.data1.getData() + "   " + midiCode.data2.getData());
        }
    }

    private String byteToX16String(byte[] bArr) {
        StringBuilder sb = new StringBuilder();
        int i = bArr[0] & UByte.MAX_VALUE;
        for (int i2 = 1; i2 < i + 1; i2++) {
            byte b = bArr[i2];
            if ((b & UByte.MAX_VALUE) < 16) {
                sb.append("0").append(Integer.toHexString(bArr[i2] & UByte.MAX_VALUE)).append(" ");
            } else {
                sb.append(Integer.toHexString(b & UByte.MAX_VALUE)).append(" ");
            }
        }
        return sb.toString().toUpperCase();
    }
}
