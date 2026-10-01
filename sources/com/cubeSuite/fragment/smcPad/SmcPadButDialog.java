package com.cubeSuite.fragment.smcPad;

import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.communication.SmcPadCommunication;
import com.cubeSuite.customControl.MidiComBox;
import com.cubeSuite.customControl.SmcInputBox;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.smcPad.SmcPadEntry;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadButDialog extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox channel;
    private MidiComBox led;
    private TextView m_title;
    private SmcInputBox smcInputBox;
    private SmcPadEntry.SmcPadUser.SmcPadButton smcPadButton;
    public SmcPadCommunication smcPadCommunication;
    private MidiComBox type;
    private MidiComBox value1;
    private MidiComBox value2;
    private MidiComBox value3;

    public SmcPadButDialog(Context context, String str) {
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
        MidiComBox midiComBox = new MidiComBox(context, "Mode", Arrays.asList("CC Single", "CC Toggle", "CC Push", "Program", "Custom", "Note"), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m269lambda$init$0$comcubeSuitefragmentsmcPadSmcPadButDialog(i3);
            }
        });
        this.type = midiComBox;
        linearLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m270lambda$init$1$comcubeSuitefragmentsmcPadSmcPadButDialog(i4);
            }
        });
        this.channel = midiComBox2;
        linearLayout.addView(midiComBox2);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + Integer.toString(i4) + "   ");
        }
        MidiComBox midiComBox3 = new MidiComBox(context, "CC", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m271lambda$init$2$comcubeSuitefragmentsmcPadSmcPadButDialog(i5);
            }
        });
        this.value1 = midiComBox3;
        linearLayout.addView(midiComBox3);
        MidiComBox midiComBox4 = new MidiComBox(context, "Value1", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m272lambda$init$3$comcubeSuitefragmentsmcPadSmcPadButDialog(i5);
            }
        });
        this.value2 = midiComBox4;
        linearLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Value2", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m273lambda$init$4$comcubeSuitefragmentsmcPadSmcPadButDialog(i5);
            }
        });
        this.value3 = midiComBox5;
        linearLayout.addView(midiComBox5);
        ArrayList arrayList3 = new ArrayList();
        for (int i5 = 0; i5 < 256; i5++) {
            arrayList3.add("   " + Integer.toString(i5) + "   ");
        }
        MidiComBox midiComBox6 = new MidiComBox(context, "Led", arrayList3, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i6) {
                this.f$0.m274lambda$init$5$comcubeSuitefragmentsmcPadSmcPadButDialog(i6);
            }
        });
        this.led = midiComBox6;
        linearLayout.addView(midiComBox6);
        SmcInputBox smcInputBox = new SmcInputBox(context);
        this.smcInputBox = smcInputBox;
        smcInputBox.sender.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadButDialog.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadButDialog smcPadButDialog = SmcPadButDialog.this;
                smcPadButDialog.setSysEx(smcPadButDialog.smcInputBox.editTex.getText().toString(), SmcPadButDialog.this.smcPadButton.sys);
            }
        });
        linearLayout.addView(this.smcInputBox);
        frameLayout.addView(linearLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m269lambda$init$0$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.type.setData(i);
        switchType(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m270lambda$init$1$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.ch.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.ch);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m271lambda$init$2$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.dat[0].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m272lambda$init$3$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.dat[1].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m273lambda$init$4$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.dat[2].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.dat[2]);
    }

    /* JADX INFO: renamed from: lambda$init$5$com-cubeSuite-fragment-smcPad-SmcPadButDialog, reason: not valid java name */
    /* synthetic */ void m274lambda$init$5$comcubeSuitefragmentsmcPadSmcPadButDialog(int i) {
        this.smcPadButton.ledMode.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadButton.ledMode);
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
            this.value2.updateName("Value1");
            this.value3.updateName("Value2");
            this.value3.setVisibility(8);
            this.smcInputBox.setVisibility(8);
            this.channel.setVisibility(0);
            this.value2.setVisibility(0);
            this.value1.setVisibility(0);
            return;
        }
        if (i == 1 || i == 2) {
            this.value1.updateName("CC");
            this.value2.updateName("Value1");
            this.value3.updateName("Value2");
            this.value3.setVisibility(0);
            this.smcInputBox.setVisibility(8);
            this.channel.setVisibility(0);
            this.value2.setVisibility(0);
            this.value1.setVisibility(0);
            this.value3.setVisibility(0);
            return;
        }
        if (i == 3) {
            this.value1.updateName("LSB");
            this.value2.updateName("MSB");
            this.value3.updateName("Program");
            this.value3.setVisibility(0);
            this.smcInputBox.setVisibility(8);
            this.channel.setVisibility(0);
            this.value2.setVisibility(0);
            this.value1.setVisibility(0);
            this.value3.setVisibility(0);
            return;
        }
        if (i == 4) {
            this.value1.setVisibility(8);
            this.value2.setVisibility(8);
            this.value3.setVisibility(8);
            this.smcInputBox.setVisibility(0);
            this.channel.setVisibility(8);
            return;
        }
        if (i != 5) {
            return;
        }
        this.value1.updateName("Note");
        this.value1.setVisibility(0);
        this.value2.setVisibility(8);
        this.value3.setVisibility(8);
        this.smcInputBox.setVisibility(8);
        this.channel.setVisibility(8);
    }

    public void updateData(SmcPadEntry.SmcPadUser.SmcPadButton smcPadButton, String str) {
        this.smcPadButton = smcPadButton;
        this.m_title.setText(str);
        switchType(smcPadButton.type.getData());
        this.type.spinner.setSelection(smcPadButton.type.getData());
        this.channel.spinner.setSelection(smcPadButton.ch.getData());
        this.value1.spinner.setSelection(smcPadButton.dat[0].getData());
        this.value2.spinner.setSelection(smcPadButton.dat[1].getData());
        this.value3.spinner.setSelection(smcPadButton.dat[2].getData());
        this.led.spinner.setSelection(smcPadButton.ledMode.getData());
        this.smcInputBox.editTex.setText(byteToX16String(smcPadButton.sys.val, smcPadButton.sys.len.getData()));
    }

    private String byteToX16String(AddrU8[] addrU8Arr, int i) {
        int length = addrU8Arr.length;
        int[] iArr = new int[length];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = addrU8Arr[i2].getData();
        }
        StringBuilder sb = new StringBuilder();
        int iMin = Math.min(length, i);
        for (int i3 = 0; i3 < iMin; i3++) {
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
