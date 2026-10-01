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
import com.cubeSuite.entity.SmcMixerEntry.SmcEncoderPara;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcKnobAlert extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox ccMidiComBox;
    private MidiComBox channel;
    SmcLinearLayout contentLayout;
    private MidiComBox lowerMidiComBox;
    private TextView m_title;
    private SmcEncoderPara para;
    public SmcCommunication smcCommunication;
    private MidiComBox speed;
    private MidiComBox type;
    private MidiComBox upperMidiComBox;

    public SmcKnobAlert(Context context, String str) {
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
        MidiComBox midiComBox = new MidiComBox(context, "Type", Arrays.asList(context.getResources().getStringArray(R.array.SmcKnobType)), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m109lambda$init$0$comcubeSuitecustomControlSmcKnobAlert(i3);
            }
        });
        this.type = midiComBox;
        this.contentLayout.addView(midiComBox);
        MidiComBox midiComBox2 = new MidiComBox(context, "Speed", Arrays.asList(context.getResources().getStringArray(R.array.SmcKnobSpeed)), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m110lambda$init$1$comcubeSuitecustomControlSmcKnobAlert(i3);
            }
        });
        this.speed = midiComBox2;
        this.contentLayout.addView(midiComBox2);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox3 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m111lambda$init$2$comcubeSuitecustomControlSmcKnobAlert(i4);
            }
        });
        this.channel = midiComBox3;
        this.contentLayout.addView(midiComBox3);
        MidiComBox midiComBox4 = new MidiComBox(context, "CC", Arrays.asList(Global.BluetoothPedalCustomCommand.MIDI_STR), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m112lambda$init$3$comcubeSuitecustomControlSmcKnobAlert(i4);
            }
        });
        this.ccMidiComBox = midiComBox4;
        this.contentLayout.addView(midiComBox4);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + Integer.toString(i4) + "   ");
        }
        MidiComBox midiComBox5 = new MidiComBox(context, "Min", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m113lambda$init$4$comcubeSuitecustomControlSmcKnobAlert(i5);
            }
        });
        this.lowerMidiComBox = midiComBox5;
        this.contentLayout.addView(midiComBox5);
        MidiComBox midiComBox6 = new MidiComBox(context, "Max", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcKnobAlert$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m114lambda$init$5$comcubeSuitecustomControlSmcKnobAlert(i5);
            }
        });
        this.upperMidiComBox = midiComBox6;
        this.contentLayout.addView(midiComBox6);
        frameLayout.addView(this.contentLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m109lambda$init$0$comcubeSuitecustomControlSmcKnobAlert(int i) {
        switchKnobType(i);
        this.para.type.setData(i);
        this.smcCommunication.sendUsrU8(this.para.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m110lambda$init$1$comcubeSuitecustomControlSmcKnobAlert(int i) {
        this.para.curve.setData(i);
        this.smcCommunication.sendUsrU8(this.para.curve);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m111lambda$init$2$comcubeSuitecustomControlSmcKnobAlert(int i) {
        this.para.ch.setData(i);
        this.smcCommunication.sendUsrU8(this.para.ch);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m112lambda$init$3$comcubeSuitecustomControlSmcKnobAlert(int i) {
        this.para.dat[0].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m113lambda$init$4$comcubeSuitecustomControlSmcKnobAlert(int i) {
        this.para.dat[1].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$5$com-cubeSuite-customControl-SmcKnobAlert, reason: not valid java name */
    /* synthetic */ void m114lambda$init$5$comcubeSuitecustomControlSmcKnobAlert(int i) {
        this.para.dat[2].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[2]);
    }

    private void switchKnobType(int i) {
        if (i == 0 || i == 3) {
            this.ccMidiComBox.setVisibility(0);
        } else {
            this.ccMidiComBox.setVisibility(8);
        }
        if (i == 3) {
            this.lowerMidiComBox.updateName("Left");
            this.upperMidiComBox.updateName("Right");
        } else {
            this.lowerMidiComBox.updateName("Min");
            this.upperMidiComBox.updateName("Max");
        }
    }

    public void updateData(SmcEncoderPara smcEncoderPara, String str, boolean z) {
        this.para = smcEncoderPara;
        this.m_title.setText(str);
        switchKnobType(smcEncoderPara.type.getData());
        this.type.spinner.setSelection(smcEncoderPara.type.getData());
        this.speed.spinner.setSelection(smcEncoderPara.curve.getData());
        this.channel.spinner.setSelection(smcEncoderPara.ch.getData());
        this.ccMidiComBox.spinner.setSelection(smcEncoderPara.dat[0].getData());
        this.lowerMidiComBox.spinner.setSelection(smcEncoderPara.dat[1].getData());
        this.upperMidiComBox.spinner.setSelection(smcEncoderPara.dat[2].getData());
        this.contentLayout.setChildViewsCanTouch(z);
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
