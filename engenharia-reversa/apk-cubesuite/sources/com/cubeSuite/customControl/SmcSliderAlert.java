package com.cubeSuite.customControl;

import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.TextView;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.communication.SmcCommunication;
import com.cubeSuite.entity.SmcMixerEntry.SmcSpPara;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcSliderAlert extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox ccMidiComBox;
    private MidiComBox channel;
    SmcLinearLayout contentLayout;
    private MidiComBox lowerMidiComBox;
    private TextView m_title;
    private SmcSpPara para;
    public SmcCommunication smcCommunication;
    private MidiComBox type;
    private MidiComBox upperMidiComBox;

    public SmcSliderAlert(Context context, String str) {
        super(context);
        this.LINER_PADDING = dpToPx(16);
        init(context, Color.parseColor("#273c75"), str);
    }

    private void init(Context context, int i, String str) {
        requestWindowFeature(1);
        FrameLayout frameLayout = new FrameLayout(context);
        frameLayout.setLayoutParams(new ViewGroup.LayoutParams(-1, -2));
        SmcLinearLayout smcLinearLayout = new SmcLinearLayout(context);
        this.contentLayout = smcLinearLayout;
        smcLinearLayout.setLayoutParams(new FrameLayout.LayoutParams(-1, -2));
        this.contentLayout.setOrientation(1);
        SmcLinearLayout smcLinearLayout2 = this.contentLayout;
        int i2 = this.LINER_PADDING;
        smcLinearLayout2.setPadding(i2, i2, i2, i2);
        TextView textView = new TextView(context);
        this.m_title = textView;
        textView.setText(str);
        this.m_title.setTextSize(24.0f);
        this.m_title.setTextColor(R.color.accent);
        this.m_title.setGravity(17);
        this.contentLayout.addView(this.m_title);
        MidiComBox midiComBox = new MidiComBox(context, "Type", Arrays.asList(context.getResources().getStringArray(R.array.SmcSliderType)), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcSliderAlert$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m115lambda$init$0$comcubeSuitecustomControlSmcSliderAlert(i3);
            }
        });
        this.type = midiComBox;
        this.contentLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcSliderAlert$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m116lambda$init$1$comcubeSuitecustomControlSmcSliderAlert(i4);
            }
        });
        this.channel = midiComBox2;
        this.contentLayout.addView(midiComBox2);
        MidiComBox midiComBox3 = new MidiComBox(context, "CC", Arrays.asList(Global.BluetoothPedalCustomCommand.MIDI_STR), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcSliderAlert$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m117lambda$init$2$comcubeSuitecustomControlSmcSliderAlert(i4);
            }
        });
        this.ccMidiComBox = midiComBox3;
        this.contentLayout.addView(midiComBox3);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + Integer.toString(i4) + "   ");
        }
        MidiComBox midiComBox4 = new MidiComBox(context, "Min", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcSliderAlert$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m118lambda$init$3$comcubeSuitecustomControlSmcSliderAlert(i5);
            }
        });
        this.lowerMidiComBox = midiComBox4;
        this.contentLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Max", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcSliderAlert$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m119lambda$init$4$comcubeSuitecustomControlSmcSliderAlert(i5);
            }
        });
        this.upperMidiComBox = midiComBox5;
        this.contentLayout.addView(midiComBox5);
        frameLayout.addView(this.contentLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-customControl-SmcSliderAlert, reason: not valid java name */
    /* synthetic */ void m115lambda$init$0$comcubeSuitecustomControlSmcSliderAlert(int i) {
        switchSliderType(i);
        this.para.type.setData(i);
        this.smcCommunication.sendUsrU8(this.para.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-customControl-SmcSliderAlert, reason: not valid java name */
    /* synthetic */ void m116lambda$init$1$comcubeSuitecustomControlSmcSliderAlert(int i) {
        this.para.ch.setData(i);
        this.smcCommunication.sendUsrU8(this.para.ch);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-customControl-SmcSliderAlert, reason: not valid java name */
    /* synthetic */ void m117lambda$init$2$comcubeSuitecustomControlSmcSliderAlert(int i) {
        this.para.dat[0].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-customControl-SmcSliderAlert, reason: not valid java name */
    /* synthetic */ void m118lambda$init$3$comcubeSuitecustomControlSmcSliderAlert(int i) {
        this.para.dat[1].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-customControl-SmcSliderAlert, reason: not valid java name */
    /* synthetic */ void m119lambda$init$4$comcubeSuitecustomControlSmcSliderAlert(int i) {
        this.para.dat[2].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[2]);
    }

    private void switchSliderType(int i) {
        if (i == 2) {
            this.ccMidiComBox.setVisibility(8);
            this.lowerMidiComBox.setVisibility(8);
            this.upperMidiComBox.setVisibility(8);
        } else {
            this.ccMidiComBox.setVisibility(0);
            this.lowerMidiComBox.setVisibility(0);
            this.upperMidiComBox.setVisibility(0);
        }
    }

    public void updateData(SmcSpPara smcSpPara, String str, boolean z) {
        this.para = smcSpPara;
        this.m_title.setText(str);
        switchSliderType(smcSpPara.type.getData());
        this.type.spinner.setSelection(smcSpPara.type.getData());
        this.channel.spinner.setSelection(smcSpPara.ch.getData());
        this.ccMidiComBox.spinner.setSelection(smcSpPara.dat[0].getData());
        this.lowerMidiComBox.spinner.setSelection(smcSpPara.dat[1].getData());
        this.upperMidiComBox.spinner.setSelection(smcSpPara.dat[2].getData());
        this.contentLayout.setChildViewsCanTouch(z);
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
