package com.cubeSuite.fragment.smcPad;

import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.communication.SmcPadCommunication;
import com.cubeSuite.customControl.MidiComBox;
import com.cubeSuite.entity.smcPad.SmcPadEntry;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadKnobDialog extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox channel;
    private TextView m_title;
    public SmcPadCommunication smcPadCommunication;
    private SmcPadEntry.SmcPadUser.SmcPadKnob smcPadKnob;
    private MidiComBox speed;
    private MidiComBox type;
    private MidiComBox value1;
    private MidiComBox value2;
    private MidiComBox value3;

    public SmcPadKnobDialog(Context context, String str) {
        super(context);
        this.LINER_PADDING = dpToPx(16);
        init(context, Color.parseColor("#273c75"), str);
    }

    private void init(Context context, int i, String str) {
        requestWindowFeature(1);
        FrameLayout frameLayout = new FrameLayout(context);
        frameLayout.setLayoutParams(new ViewGroup.LayoutParams(-1, -2));
        LinearLayout linearLayout = new LinearLayout(context);
        linearLayout.setLayoutParams(new FrameLayout.LayoutParams(-1, -2));
        linearLayout.setOrientation(1);
        int i2 = this.LINER_PADDING;
        linearLayout.setPadding(i2, i2, i2, i2);
        TextView textView = new TextView(context);
        this.m_title = textView;
        textView.setText(str);
        this.m_title.setTextSize(24.0f);
        this.m_title.setTextColor(R.color.accent);
        this.m_title.setGravity(17);
        linearLayout.addView(this.m_title);
        MidiComBox midiComBox = new MidiComBox(context, "Type", Arrays.asList("CC", "Aftertouch", "Note", "CW"), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m280lambda$init$0$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i3);
            }
        });
        this.type = midiComBox;
        linearLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m281lambda$init$1$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i4);
            }
        });
        this.channel = midiComBox2;
        linearLayout.addView(midiComBox2);
        MidiComBox midiComBox3 = new MidiComBox(context, "Speed", Arrays.asList("Lowest", "Low", "Normal", "Fast", "Fastest"), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m282lambda$init$2$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i4);
            }
        });
        this.speed = midiComBox3;
        linearLayout.addView(midiComBox3);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + i4 + "   ");
        }
        MidiComBox midiComBox4 = new MidiComBox(context, "CC", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m283lambda$init$3$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i5);
            }
        });
        this.value1 = midiComBox4;
        linearLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Min", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m284lambda$init$4$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i5);
            }
        });
        this.value2 = midiComBox5;
        linearLayout.addView(midiComBox5);
        MidiComBox midiComBox6 = new MidiComBox(context, "Max", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnobDialog$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m285lambda$init$5$comcubeSuitefragmentsmcPadSmcPadKnobDialog(i5);
            }
        });
        this.value3 = midiComBox6;
        linearLayout.addView(midiComBox6);
        frameLayout.addView(linearLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m280lambda$init$0$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.type.setData(i);
        switchType(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m281lambda$init$1$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.ch.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.ch);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m282lambda$init$2$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.curve.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.curve);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m283lambda$init$3$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.dat[0].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m284lambda$init$4$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.dat[1].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$5$com-cubeSuite-fragment-smcPad-SmcPadKnobDialog, reason: not valid java name */
    /* synthetic */ void m285lambda$init$5$comcubeSuitefragmentsmcPadSmcPadKnobDialog(int i) {
        this.smcPadKnob.dat[2].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadKnob.dat[2]);
    }

    public void setSysEx(String str, SmcPadEntry.SmcPadUser.SmcPadSys smcPadSys) {
        byte[] bArrX16StringToByte = x16StringToByte(str);
        smcPadSys.len.setData(bArrX16StringToByte.length);
        for (int i = 0; i < bArrX16StringToByte.length; i++) {
            smcPadSys.val[i].setData(bArrX16StringToByte[i]);
        }
        this.smcPadCommunication.sendSysEx(smcPadSys);
    }

    private void switchType(int i) {
        if (i == 0) {
            this.value1.updateName("CC");
            this.value2.updateName("Min");
            this.value3.updateName("Max");
            this.value1.setVisibility(0);
            return;
        }
        if (i == 1 || i == 2) {
            this.value1.updateName("CC");
            this.value2.updateName("Min");
            this.value3.updateName("Max");
            this.value1.setVisibility(8);
            return;
        }
        if (i != 3) {
            return;
        }
        this.value1.updateName("CC");
        this.value2.updateName("Left");
        this.value3.updateName("Right");
        this.value1.setVisibility(0);
    }

    public void updateData(SmcPadEntry.SmcPadUser.SmcPadKnob smcPadKnob, String str) {
        this.smcPadKnob = smcPadKnob;
        this.m_title.setText(str);
        switchType(smcPadKnob.type.getData());
        this.type.spinner.setSelection(smcPadKnob.type.getData());
        this.channel.spinner.setSelection(smcPadKnob.ch.getData());
        this.value1.spinner.setSelection(smcPadKnob.dat[0].getData());
        this.value2.spinner.setSelection(smcPadKnob.dat[1].getData());
        this.value3.spinner.setSelection(smcPadKnob.dat[2].getData());
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
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
}
