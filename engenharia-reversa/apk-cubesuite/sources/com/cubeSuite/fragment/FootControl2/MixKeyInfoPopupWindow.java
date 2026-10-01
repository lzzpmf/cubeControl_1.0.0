package com.cubeSuite.fragment.FootControl2;

import android.content.Context;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.PopupWindow;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.NumKeyPadView;
import com.cubeSuite.entity.fc2.MidiCodeStruct;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class MixKeyInfoPopupWindow extends PopupWindow {
    Button channel;
    View channelContainer;
    Context context;
    int curType;
    int currentMidiCodeIndex;
    Button data1;
    View data1Container;
    Button data2;
    View data2Container;
    EditText etCustomMidiCode;
    NumKeyPadView numKeyPad;
    byte[] sysEx;
    TextView tvCustomMidiCode;
    TextView tvMidiInfo;
    RadioGroup type;
    int currentEditInput = -1;
    public final String[] MIDI_TYPE = {"PC", "CC", "Note ON", "Note OFF", "SysEx"};
    private final int SYS_MODE = 4;
    private final int PC_MODE = 0;
    private final int CHANNEL_NUM = 16;

    public MixKeyInfoPopupWindow(Context context, int i, final MidiCodeStruct midiCodeStruct, final byte[] bArr, final MixKeyInfoPopupWindowCallback mixKeyInfoPopupWindowCallback) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_mix_key_midi_info_layout, (ViewGroup) null);
        this.sysEx = bArr;
        this.currentMidiCodeIndex = i;
        this.context = context;
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        Button button = (Button) viewInflate.findViewById(R.id.back);
        this.channel = (Button) viewInflate.findViewById(R.id.channel);
        this.channelContainer = viewInflate.findViewById(R.id.channelContainer);
        this.data1Container = viewInflate.findViewById(R.id.data1Container);
        this.data2Container = viewInflate.findViewById(R.id.data2Container);
        this.type = (RadioGroup) viewInflate.findViewById(R.id.type);
        this.data1 = (Button) viewInflate.findViewById(R.id.data1);
        this.data2 = (Button) viewInflate.findViewById(R.id.data2);
        this.numKeyPad = (NumKeyPadView) viewInflate.findViewById(R.id.numKeyPad);
        TextView textView = (TextView) viewInflate.findViewById(R.id.tvMidiInfo);
        this.tvMidiInfo = textView;
        textView.setMovementMethod(ScrollingMovementMethod.getInstance());
        Button button2 = (Button) viewInflate.findViewById(R.id.cancel);
        Button button3 = (Button) viewInflate.findViewById(R.id.save);
        this.tvCustomMidiCode = (TextView) viewInflate.findViewById(R.id.tvCustomMidiCode);
        this.etCustomMidiCode = (EditText) viewInflate.findViewById(R.id.etCustomMidiCode);
        int data = midiCodeStruct.type.getData();
        this.curType = data;
        setViewVisibility(data);
        ((RadioButton) this.type.getChildAt(midiCodeStruct.type.getData())).setChecked(true);
        this.channel.setText(String.valueOf(midiCodeStruct.channel.getData() + 1));
        this.data1.setText(String.valueOf(midiCodeStruct.data1.getData()));
        this.data2.setText(String.valueOf(midiCodeStruct.data2.getData()));
        this.etCustomMidiCode.setText(byteToX16String(getSysEx()));
        this.type.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow.1
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup, int i2) {
                int i3 = 0;
                while (true) {
                    if (i3 >= MixKeyInfoPopupWindow.this.type.getChildCount()) {
                        i3 = -1;
                        break;
                    } else if (((RadioButton) MixKeyInfoPopupWindow.this.type.getChildAt(i3)).isChecked()) {
                        break;
                    } else {
                        i3++;
                    }
                }
                if (i3 == -1) {
                    return;
                }
                MixKeyInfoPopupWindow.this.curType = i3;
                MixKeyInfoPopupWindow.this.setViewVisibility(i3);
                MixKeyInfoPopupWindow.this.updateCurrentMidiCodeText();
            }
        });
        button3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m170x26c324f3(bArr, midiCodeStruct, mixKeyInfoPopupWindowCallback, view);
            }
        });
        button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m171x3778f1b4(view);
            }
        });
        this.numKeyPad.setKeyPadListener(new NumKeyPadView.KeypadListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.NumKeyPadView.KeypadListener
            public final void numChange(int i2) {
                this.f$0.m172x482ebe75(i2);
            }
        });
        this.channel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m173x58e48b36(view);
            }
        });
        this.data1.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m174x699a57f7(view);
            }
        });
        this.data2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda5
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m175x7a5024b8(view);
            }
        });
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.MixKeyInfoPopupWindow$$ExternalSyntheticLambda6
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m176x8b05f179(view);
            }
        });
        updateCurrentMidiCodeText();
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m170x26c324f3(byte[] bArr, MidiCodeStruct midiCodeStruct, MixKeyInfoPopupWindowCallback mixKeyInfoPopupWindowCallback, View view) {
        if (this.curType == 4) {
            byte[] bArrX16StringToByte = x16StringToByte(this.etCustomMidiCode.getText().toString());
            for (int i = 0; i < bArr.length; i++) {
                if (i < bArrX16StringToByte.length) {
                    this.sysEx[i] = bArrX16StringToByte[i];
                } else {
                    this.sysEx[i] = 0;
                }
            }
        } else {
            midiCodeStruct.channel.setData(Integer.parseInt(String.valueOf(this.channel.getText())) - 1);
            int i2 = Integer.parseInt(String.valueOf(this.data1.getText()));
            int i3 = Integer.parseInt(String.valueOf(this.data2.getText()));
            midiCodeStruct.data1.setData(i2);
            midiCodeStruct.data2.setData(i3);
        }
        midiCodeStruct.type.setData(this.curType);
        mixKeyInfoPopupWindowCallback.update(midiCodeStruct);
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m171x3778f1b4(View view) {
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m172x482ebe75(int i) {
        int i2 = this.currentEditInput;
        if (i2 == 0) {
            this.channel.setText(String.valueOf(i + 1));
        } else if (i2 == 1) {
            this.data1.setText(String.valueOf(i));
        } else if (i2 == 2) {
            this.data2.setText(String.valueOf(i));
        }
        updateCurrentMidiCodeText();
    }

    /* JADX INFO: renamed from: lambda$new$3$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m173x58e48b36(View view) {
        editMidiCodeFocus(0);
    }

    /* JADX INFO: renamed from: lambda$new$4$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m174x699a57f7(View view) {
        editMidiCodeFocus(1);
    }

    /* JADX INFO: renamed from: lambda$new$5$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m175x7a5024b8(View view) {
        editMidiCodeFocus(2);
    }

    /* JADX INFO: renamed from: lambda$new$6$com-cubeSuite-fragment-FootControl2-MixKeyInfoPopupWindow, reason: not valid java name */
    /* synthetic */ void m176x8b05f179(View view) {
        dismiss();
    }

    void setViewVisibility(int i) {
        if (i == 0) {
            this.channelContainer.setVisibility(0);
            this.data1Container.setVisibility(0);
            this.data2Container.setVisibility(8);
            this.tvCustomMidiCode.setVisibility(8);
            this.etCustomMidiCode.setVisibility(8);
            return;
        }
        if (i == 4) {
            this.channelContainer.setVisibility(8);
            this.data1Container.setVisibility(8);
            this.data2Container.setVisibility(8);
            this.tvCustomMidiCode.setVisibility(0);
            this.etCustomMidiCode.setVisibility(0);
            this.numKeyPad.close();
            return;
        }
        this.channelContainer.setVisibility(0);
        this.data1Container.setVisibility(0);
        this.data2Container.setVisibility(0);
        this.tvCustomMidiCode.setVisibility(8);
        this.etCustomMidiCode.setVisibility(8);
    }

    /* JADX WARN: Code duplicated, block: B:7:0x0024  */
    public void updateCurrentMidiCodeText() {
        String str;
        String str2 = (String) this.channel.getText();
        String str3 = (String) this.data1.getText();
        String str4 = (String) this.data2.getText();
        int i = this.curType;
        if (i >= 0) {
            String[] strArr = this.MIDI_TYPE;
            if (i < strArr.length) {
                str = strArr[i];
            } else {
                str = "";
            }
        } else {
            str = "";
        }
        if (i == 4) {
            this.tvMidiInfo.setText("send:" + byteToX16String(getSysEx()));
        } else {
            this.tvMidiInfo.setText("[" + (this.currentMidiCodeIndex + 1) + "]   " + str2 + "   " + str + "   " + str3 + "   " + str4);
        }
    }

    private byte[] getSysEx() {
        return this.sysEx;
    }

    private byte[] x16StringToByte(String str) {
        boolean z;
        char[] cArr = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        byte[] bArr = new byte[128];
        boolean z2 = false;
        int i = 0;
        int i2 = 1;
        for (char c : str.toUpperCase().replaceAll("\\s", "").replaceAll("\\n", "").toCharArray()) {
            if (i2 - 1 == 127) {
                break;
            }
            byte b = 0;
            while (true) {
                if (b >= 16) {
                    z = false;
                    break;
                }
                if (cArr[b] == c) {
                    if (z2) {
                        bArr[i2] = (byte) (b + i);
                        i2++;
                        z2 = false;
                    } else {
                        i = b * 16;
                        z2 = true;
                    }
                    z = z2;
                    break;
                }
                b = (byte) (b + 1);
            }
            if (!z && z2) {
                z2 = false;
                i = 0;
            }
        }
        bArr[0] = (byte) (i2 - 1);
        return bArr;
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

    private void editMidiCodeFocus(int i) {
        this.currentEditInput = i;
        this.channel.setTextColor(this.context.getResources().getColor(R.color.dominantHue));
        this.data1.setTextColor(this.context.getResources().getColor(R.color.dominantHue));
        this.data2.setTextColor(this.context.getResources().getColor(R.color.dominantHue));
        this.numKeyPad.setScope(0, 127);
        this.numKeyPad.setOffset(0);
        if (i == 0) {
            this.numKeyPad.setScope(1, 16);
            this.numKeyPad.setValue(String.valueOf(this.channel.getText()));
            this.numKeyPad.setOffset(-1);
            this.channel.setTextColor(this.context.getResources().getColor(R.color.accent));
        } else if (i == 1) {
            this.numKeyPad.setValue(String.valueOf(this.data1.getText()));
            this.data1.setTextColor(this.context.getResources().getColor(R.color.accent));
        } else if (i == 2) {
            this.numKeyPad.setValue(String.valueOf(this.data2.getText()));
            this.data2.setTextColor(this.context.getResources().getColor(R.color.accent));
        }
        this.numKeyPad.show();
    }
}
