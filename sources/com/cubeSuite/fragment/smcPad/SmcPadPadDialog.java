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
import com.cubeSuite.customControl.ColorPicker;
import com.cubeSuite.customControl.MidiComBox;
import com.cubeSuite.customControl.SmcInputBox;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.smcPad.SmcPadEntry;
import java.util.ArrayList;
import java.util.Arrays;
import top.defaults.colorpicker.ColorPickerPopup;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadPadDialog extends Dialog {
    private final int LINER_PADDING;
    private MidiComBox channel;
    ColorPicker colorPicker;
    LinearLayout contentLayout;
    private MidiComBox led;
    private TextView m_title;
    private SmcInputBox smcInputBox;
    public SmcPadCommunication smcPadCommunication;
    private SmcPadEntry.SmcPadUser.SmcPadPad smcPadPad;
    private MidiComBox type;
    private MidiComBox value1;
    private MidiComBox value2;
    private MidiComBox value3;

    public SmcPadPadDialog(Context context, String str) {
        super(context);
        this.LINER_PADDING = dpToPx(16);
        init(context, Color.parseColor("#273c75"), str);
    }

    private void init(Context context, int i, String str) {
        requestWindowFeature(1);
        FrameLayout frameLayout = new FrameLayout(context);
        frameLayout.setLayoutParams(new ViewGroup.LayoutParams(-1, -2));
        LinearLayout linearLayout = new LinearLayout(context);
        this.contentLayout = linearLayout;
        linearLayout.setLayoutParams(new FrameLayout.LayoutParams(-1, -2));
        this.contentLayout.setOrientation(1);
        LinearLayout linearLayout2 = this.contentLayout;
        int i2 = this.LINER_PADDING;
        linearLayout2.setPadding(i2, i2, i2, i2);
        TextView textView = new TextView(context);
        this.m_title = textView;
        textView.setText(str);
        this.m_title.setTextSize(24.0f);
        this.m_title.setTextColor(R.color.accent);
        this.m_title.setGravity(17);
        this.contentLayout.addView(this.m_title);
        MidiComBox midiComBox = new MidiComBox(context, "Type", Arrays.asList("Note", "CC Toggle", "Momentary", "Program", "MCP", "Custom", "Comb MCP"), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m291lambda$init$0$comcubeSuitefragmentsmcPadSmcPadPadDialog(i3);
            }
        });
        this.type = midiComBox;
        this.contentLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 1; i3 < 17; i3++) {
            arrayList.add("Channel " + i3);
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Channel", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m292lambda$init$1$comcubeSuitefragmentsmcPadSmcPadPadDialog(i4);
            }
        });
        this.channel = midiComBox2;
        this.contentLayout.addView(midiComBox2);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 0; i4 < 128; i4++) {
            arrayList2.add("   " + Integer.toString(i4) + "   ");
        }
        MidiComBox midiComBox3 = new MidiComBox(context, "Note", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m293lambda$init$2$comcubeSuitefragmentsmcPadSmcPadPadDialog(i5);
            }
        });
        this.value1 = midiComBox3;
        this.contentLayout.addView(midiComBox3);
        MidiComBox midiComBox4 = new MidiComBox(context, "Min", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m294lambda$init$3$comcubeSuitefragmentsmcPadSmcPadPadDialog(i5);
            }
        });
        this.value2 = midiComBox4;
        this.contentLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Max", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m295lambda$init$4$comcubeSuitefragmentsmcPadSmcPadPadDialog(i5);
            }
        });
        this.value3 = midiComBox5;
        this.contentLayout.addView(midiComBox5);
        ArrayList arrayList3 = new ArrayList();
        for (int i5 = 0; i5 < 256; i5++) {
            arrayList3.add("   " + Integer.toString(i5) + "   ");
        }
        MidiComBox midiComBox6 = new MidiComBox(context, "Led", arrayList3, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i6) {
                this.f$0.m296lambda$init$5$comcubeSuitefragmentsmcPadSmcPadPadDialog(i6);
            }
        });
        this.led = midiComBox6;
        this.contentLayout.addView(midiComBox6);
        SmcInputBox smcInputBox = new SmcInputBox(context);
        this.smcInputBox = smcInputBox;
        smcInputBox.sender.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadPadDialog smcPadPadDialog = SmcPadPadDialog.this;
                smcPadPadDialog.setSysEx(smcPadPadDialog.smcInputBox.editTex.getText().toString(), SmcPadPadDialog.this.smcPadPad.sys);
            }
        });
        this.contentLayout.addView(this.smcInputBox);
        ColorPicker colorPicker = new ColorPicker(context);
        this.colorPicker = colorPicker;
        colorPicker.colorPicker.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcPadPadDialog smcPadPadDialog = SmcPadPadDialog.this;
                smcPadPadDialog.showColorDialog(smcPadPadDialog.smcPadPad.color);
            }
        });
        this.contentLayout.addView(this.colorPicker);
        frameLayout.addView(this.contentLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m291lambda$init$0$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.type.setData(i);
        switchType(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.type);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m292lambda$init$1$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.ch.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.ch);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m293lambda$init$2$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.dat[0].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.dat[0]);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m294lambda$init$3$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.dat[1].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.dat[1]);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m295lambda$init$4$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.dat[2].setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.dat[2]);
    }

    /* JADX INFO: renamed from: lambda$init$5$com-cubeSuite-fragment-smcPad-SmcPadPadDialog, reason: not valid java name */
    /* synthetic */ void m296lambda$init$5$comcubeSuitefragmentsmcPadSmcPadPadDialog(int i) {
        this.smcPadPad.softLed.setData(i);
        this.smcPadCommunication.sendUsrU8(this.smcPadPad.softLed);
    }

    public void showColorDialog(AddrU8[] addrU8Arr) {
        new ColorPickerPopup.Builder(getContext()).initialColor(Color.rgb(addrU8Arr[0].getData(), addrU8Arr[1].getData(), addrU8Arr[2].getData())).enableAlpha(false).okTitle("confirm").cancelTitle("close").showIndicator(false).showValue(false).build().show(this.contentLayout, new ColorPickerPopup.ColorPickerObserver() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPadDialog.3
            @Override // top.defaults.colorpicker.ColorPickerPopup.ColorPickerObserver
            public void onColorPicked(int i) {
                SmcPadPadDialog.this.smcPadPad.color[0].setData((16711680 & i) >> 16);
                SmcPadPadDialog.this.smcPadPad.color[1].setData((65280 & i) >> 8);
                SmcPadPadDialog.this.smcPadPad.color[2].setData(i & 255);
                SmcPadPadDialog.this.smcPadCommunication.sendColor(SmcPadPadDialog.this.smcPadPad.color);
                SmcPadPadDialog.this.colorPicker.colorPicker.setBackgroundColor(Color.rgb(SmcPadPadDialog.this.smcPadPad.color[0].getData(), SmcPadPadDialog.this.smcPadPad.color[1].getData(), SmcPadPadDialog.this.smcPadPad.color[2].getData()));
            }
        });
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
        switch (i) {
            case 0:
                this.value1.updateName("Note");
                this.value2.updateName("Min");
                this.value3.updateName("Max");
                this.channel.setVisibility(0);
                this.smcInputBox.setVisibility(8);
                this.value2.setVisibility(0);
                this.value3.setVisibility(0);
                this.value1.setVisibility(0);
                break;
            case 1:
                this.value1.updateName("cc");
                this.value2.updateName("Value1");
                this.value3.updateName("Value2");
                this.channel.setVisibility(0);
                this.smcInputBox.setVisibility(8);
                this.value2.setVisibility(0);
                this.value3.setVisibility(0);
                this.value1.setVisibility(0);
                break;
            case 2:
                this.value1.updateName("cc");
                this.value2.updateName("Up");
                this.value3.updateName("Down");
                this.channel.setVisibility(0);
                this.smcInputBox.setVisibility(8);
                this.value2.setVisibility(0);
                this.value3.setVisibility(0);
                this.value1.setVisibility(0);
                break;
            case 3:
                this.value1.updateName("LSB");
                this.value2.updateName("MSB");
                this.value3.updateName("Program");
                this.channel.setVisibility(0);
                this.smcInputBox.setVisibility(8);
                this.value2.setVisibility(0);
                this.value3.setVisibility(0);
                this.value1.setVisibility(0);
                break;
            case 4:
                this.value1.updateName("Note");
                this.value2.updateName("MSB");
                this.value3.updateName("Program");
                this.channel.setVisibility(8);
                this.smcInputBox.setVisibility(8);
                this.value2.setVisibility(8);
                this.value3.setVisibility(8);
                this.value1.setVisibility(0);
                break;
            case 5:
            case 6:
                this.value1.updateName("Note");
                this.value2.updateName("MSB");
                this.value3.updateName("Program");
                this.channel.setVisibility(0);
                this.smcInputBox.setVisibility(0);
                this.value2.setVisibility(8);
                this.value3.setVisibility(8);
                this.value1.setVisibility(8);
                break;
        }
    }

    public void updateData(SmcPadEntry.SmcPadUser.SmcPadPad smcPadPad, String str) {
        this.smcPadPad = smcPadPad;
        this.m_title.setText(str);
        switchType(smcPadPad.type.getData());
        this.colorPicker.colorPicker.setBackgroundColor(Color.rgb(smcPadPad.color[0].getData(), smcPadPad.color[1].getData(), smcPadPad.color[2].getData()));
        this.type.spinner.setSelection(smcPadPad.type.getData());
        this.channel.spinner.setSelection(smcPadPad.ch.getData());
        this.value1.spinner.setSelection(smcPadPad.dat[0].getData());
        this.value2.spinner.setSelection(smcPadPad.dat[1].getData());
        this.value3.spinner.setSelection(smcPadPad.dat[2].getData());
        this.led.spinner.setSelection(smcPadPad.softLed.getData());
        this.smcInputBox.editTex.setText(byteToX16String(smcPadPad.sys.val, smcPadPad.sys.len.getData()));
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
