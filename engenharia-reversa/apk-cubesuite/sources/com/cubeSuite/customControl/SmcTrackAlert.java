package com.cubeSuite.customControl;

import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.TextView;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.communication.SmcCommunication;
import com.cubeSuite.entity.SmcMixerEntry.SmcBtnPara;
import com.cubeSuite.entity.SmcMixerEntry.SmcSys;
import com.cubeSuite.entity.addrData.AddrU8;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcTrackAlert extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox ccMidiComBox;
    private MidiComBox channel;
    SmcLinearLayout contentLayout;
    private MidiComBox ledMode;
    private MidiComBox lowerMidiComBox;
    private TextView m_title;
    private MidiComBox mode;
    SmcBtnPara para;
    public SmcCommunication smcCommunication;
    private SmcInputBox smcInputBox;
    private MidiComBox upperMidiComBox;

    public SmcTrackAlert(Context context, String str) {
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
        MidiComBox midiComBox = new MidiComBox(context, "Mode", Arrays.asList(context.getResources().getStringArray(R.array.SmcTrackType)), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m120lambda$init$0$comcubeSuitecustomControlSmcTrackAlert(i3);
            }
        });
        this.mode = midiComBox;
        this.contentLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m121lambda$init$1$comcubeSuitecustomControlSmcTrackAlert(i4);
            }
        });
        this.channel = midiComBox2;
        this.contentLayout.addView(midiComBox2);
        MidiComBox midiComBox3 = new MidiComBox(context, "CC", Arrays.asList(Global.BluetoothPedalCustomCommand.MIDI_STR), new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m122lambda$init$2$comcubeSuitecustomControlSmcTrackAlert(i4);
            }
        });
        this.ccMidiComBox = midiComBox3;
        this.contentLayout.addView(midiComBox3);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + Integer.toString(i4) + "   ");
        }
        MidiComBox midiComBox4 = new MidiComBox(context, "Value1", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m123lambda$init$3$comcubeSuitecustomControlSmcTrackAlert(i5);
            }
        });
        this.lowerMidiComBox = midiComBox4;
        this.contentLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Value2", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m124lambda$init$4$comcubeSuitecustomControlSmcTrackAlert(i5);
            }
        });
        this.upperMidiComBox = midiComBox5;
        this.contentLayout.addView(midiComBox5);
        ArrayList arrayList3 = new ArrayList();
        for (int i5 = 0; i5 < 256; i5++) {
            arrayList3.add("   " + Integer.toString(i5) + "   ");
        }
        MidiComBox midiComBox6 = new MidiComBox(context, "Led Mode", arrayList3, new MidiComBox.DataChange() { // from class: com.cubeSuite.customControl.SmcTrackAlert$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i6) {
                this.f$0.m125lambda$init$5$comcubeSuitecustomControlSmcTrackAlert(i6);
            }
        });
        this.ledMode = midiComBox6;
        this.contentLayout.addView(midiComBox6);
        SmcInputBox smcInputBox = new SmcInputBox(context);
        this.smcInputBox = smcInputBox;
        smcInputBox.sender.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SmcTrackAlert.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcTrackAlert smcTrackAlert = SmcTrackAlert.this;
                smcTrackAlert.setSysEx(smcTrackAlert.smcInputBox.editTex.getText().toString(), SmcTrackAlert.this.para.sys);
            }
        });
        this.contentLayout.addView(this.smcInputBox);
        frameLayout.addView(this.contentLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m120lambda$init$0$comcubeSuitecustomControlSmcTrackAlert(int i) {
        switchMode(i);
        this.para.type.setData(i);
        this.smcCommunication.sendUsrU8(this.para.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m121lambda$init$1$comcubeSuitecustomControlSmcTrackAlert(int i) {
        this.para.ch.setData(i);
        this.smcCommunication.sendUsrU8(this.para.ch);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m122lambda$init$2$comcubeSuitecustomControlSmcTrackAlert(int i) {
        this.para.dat[0].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m123lambda$init$3$comcubeSuitecustomControlSmcTrackAlert(int i) {
        this.para.dat[1].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m124lambda$init$4$comcubeSuitecustomControlSmcTrackAlert(int i) {
        this.para.dat[2].setData(i);
        this.smcCommunication.sendUsrU8(this.para.dat[2]);
    }

    /* JADX INFO: renamed from: lambda$init$5$com-cubeSuite-customControl-SmcTrackAlert, reason: not valid java name */
    /* synthetic */ void m125lambda$init$5$comcubeSuitecustomControlSmcTrackAlert(int i) {
        this.para.softled.setData(i);
        this.smcCommunication.sendUsrU8(this.para.softled);
    }

    public void setSysEx(String str, SmcSys smcSys) {
        byte[] bArrX16StringToByte = x16StringToByte(str);
        smcSys.len.setData(bArrX16StringToByte.length);
        for (int i = 0; i < bArrX16StringToByte.length; i++) {
            smcSys.val[i].setData(bArrX16StringToByte[i]);
        }
        this.smcCommunication.sendSysEx(smcSys);
    }

    private byte[] x16StringToByte(String str) {
        int i;
        boolean z;
        boolean z2;
        char[] cArr = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        String strReplace = str.toUpperCase().replace("\\s", "").replace("\\n", "");
        byte[] bArr = new byte[64];
        int i2 = 0;
        boolean z3 = false;
        int i3 = 0;
        for (char c : strReplace.substring(0, Math.min(128, strReplace.length())).toCharArray()) {
            byte b = 0;
            while (true) {
                if (b >= 16) {
                    i = i3;
                    z = z3;
                    z2 = false;
                    break;
                }
                if (cArr[b] == c) {
                    if (z3) {
                        bArr[i2] = (byte) (b + i3);
                        i2 = (byte) (i2 + 1);
                        z2 = false;
                    } else {
                        i3 = b * 16;
                        z2 = true;
                    }
                    i = i3;
                    z = z2;
                    break;
                }
                b = (byte) (b + 1);
            }
            if (z2 || !z) {
                z3 = z;
                i3 = i;
            } else {
                z3 = false;
                i3 = 0;
            }
        }
        byte[] bArr2 = new byte[i2];
        System.arraycopy(bArr, 0, bArr2, 0, i2);
        return bArr2;
    }

    private void switchMode(int i) {
        this.channel.setVisibility(0);
        this.lowerMidiComBox.setVisibility(0);
        this.upperMidiComBox.setVisibility(0);
        this.ccMidiComBox.setVisibility(0);
        this.smcInputBox.setVisibility(8);
        if (i == 0) {
            this.upperMidiComBox.setVisibility(8);
        } else if (i == 5 || i == 4) {
            this.channel.setVisibility(8);
            this.lowerMidiComBox.setVisibility(8);
            this.upperMidiComBox.setVisibility(8);
            if (i == 4) {
                this.smcInputBox.setVisibility(0);
                this.ccMidiComBox.setVisibility(8);
            }
        } else {
            this.channel.setVisibility(0);
            this.lowerMidiComBox.setVisibility(0);
            this.upperMidiComBox.setVisibility(0);
        }
        if (i == 3) {
            this.ccMidiComBox.updateName("LSB");
            this.lowerMidiComBox.updateName("MSB");
            this.upperMidiComBox.updateName("Program");
            ArrayList arrayList = new ArrayList();
            for (int i2 = 0; i2 < Global.BluetoothPedalCustomCommand.MIDI_STR.length; i2++) {
                arrayList.add("   " + Integer.toString(i2) + "   ");
            }
            this.ccMidiComBox.setupAdapter(arrayList);
            return;
        }
        if (i == 5) {
            this.ccMidiComBox.setupAdapter(Arrays.asList(Global.BluetoothPedalCustomCommand.OCTAVE));
            this.ccMidiComBox.updateName("Note");
            return;
        }
        this.ccMidiComBox.setupAdapter(Arrays.asList(Global.BluetoothPedalCustomCommand.MIDI_STR));
        this.ccMidiComBox.updateName("CC");
        this.lowerMidiComBox.updateName("Value1");
        this.upperMidiComBox.updateName("Value2");
        this.ccMidiComBox.setupAdapter(Arrays.asList(Global.BluetoothPedalCustomCommand.MIDI_STR));
    }

    public void updateData(SmcBtnPara smcBtnPara, String str, boolean z) {
        this.para = smcBtnPara;
        this.m_title.setText(str);
        switchMode(smcBtnPara.type.getData());
        this.mode.spinner.setSelection(smcBtnPara.type.getData());
        this.channel.spinner.setSelection(smcBtnPara.ch.getData());
        this.ccMidiComBox.spinner.setSelection(smcBtnPara.dat[0].getData());
        this.lowerMidiComBox.spinner.setSelection(smcBtnPara.dat[1].getData());
        this.upperMidiComBox.spinner.setSelection(smcBtnPara.dat[2].getData());
        this.ledMode.spinner.setSelection(smcBtnPara.softled.getData());
        this.smcInputBox.editTex.setText(byteToX16String(smcBtnPara.sys.val, smcBtnPara.sys.len.getData()));
        this.contentLayout.setChildViewsCanTouch(z);
    }

    private String byteToX16String(AddrU8[] addrU8Arr, int i) {
        int length = addrU8Arr.length;
        int[] iArr = new int[length];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = addrU8Arr[i2].getData();
        }
        StringBuilder sb = new StringBuilder();
        for (int i3 = 0; i3 < i; i3++) {
            int i4 = iArr[i3];
            if ((i4 & 255) < 16) {
                sb.append("0").append(Integer.toHexString(iArr[i3] & 255)).append(" ");
            } else {
                sb.append(Integer.toHexString(i4 & 255)).append(" ");
            }
        }
        return sb.toString().toUpperCase();
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
