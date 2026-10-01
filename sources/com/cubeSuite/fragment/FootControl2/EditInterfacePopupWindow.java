package com.cubeSuite.fragment.FootControl2;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.PopupWindow;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.NumKeyPadView;
import com.cubeSuite.entity.fc2.MidiCodeStruct;

/* JADX INFO: loaded from: classes.dex */
public class EditInterfacePopupWindow extends PopupWindow {
    Button channel;
    Context context;
    int currentEditInput = -1;
    int currentMidiCodeIndex;
    Button data1;
    Button data2;
    NumKeyPadView numKeyPad;
    Button numType;
    TextView tvMidiInfo;

    public EditInterfacePopupWindow(Context context, int i, final MidiCodeStruct midiCodeStruct, final EditInterfacePopupWindowCallback editInterfacePopupWindowCallback) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_edit_interface_midi_info_layout, (ViewGroup) null);
        this.currentMidiCodeIndex = i;
        this.context = context;
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        Button button = (Button) viewInflate.findViewById(R.id.back);
        this.channel = (Button) viewInflate.findViewById(R.id.channel);
        this.numType = (Button) viewInflate.findViewById(R.id.numType);
        this.data1 = (Button) viewInflate.findViewById(R.id.data1);
        this.data2 = (Button) viewInflate.findViewById(R.id.data2);
        this.numKeyPad = (NumKeyPadView) viewInflate.findViewById(R.id.numKeyPad);
        this.tvMidiInfo = (TextView) viewInflate.findViewById(R.id.tvMidiInfo);
        Button button2 = (Button) viewInflate.findViewById(R.id.cancel);
        Button button3 = (Button) viewInflate.findViewById(R.id.save);
        this.channel.setText(String.valueOf(midiCodeStruct.channel.getData() + 1));
        this.numType.setText(String.valueOf(midiCodeStruct.type.getData()));
        this.data1.setText(String.valueOf(midiCodeStruct.data1.getData()));
        this.data2.setText(String.valueOf(midiCodeStruct.data2.getData()));
        updateCurrentMidiCodeText();
        button3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m148xbb8fed45(midiCodeStruct, editInterfacePopupWindowCallback, view);
            }
        });
        button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m149x4fce5ce4(view);
            }
        });
        this.numKeyPad.setKeyPadListener(new NumKeyPadView.KeypadListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.NumKeyPadView.KeypadListener
            public final void numChange(int i2) {
                this.f$0.m150xe40ccc83(i2);
            }
        });
        this.channel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m151x784b3c22(view);
            }
        });
        this.numType.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m152xc89abc1(view);
            }
        });
        this.data1.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda5
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m153xa0c81b60(view);
            }
        });
        this.data2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda6
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m154x35068aff(view);
            }
        });
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.EditInterfacePopupWindow$$ExternalSyntheticLambda7
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m155xc944fa9e(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m148xbb8fed45(MidiCodeStruct midiCodeStruct, EditInterfacePopupWindowCallback editInterfacePopupWindowCallback, View view) {
        midiCodeStruct.channel.setData(Integer.parseInt(String.valueOf(this.channel.getText())) - 1);
        midiCodeStruct.type.setData(Integer.parseInt(String.valueOf(this.numType.getText())));
        int i = Integer.parseInt(String.valueOf(this.data1.getText()));
        int i2 = Integer.parseInt(String.valueOf(this.data2.getText()));
        midiCodeStruct.data1.setData(i);
        midiCodeStruct.data2.setData(i2);
        editInterfacePopupWindowCallback.update();
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m149x4fce5ce4(View view) {
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m150xe40ccc83(int i) {
        int i2 = this.currentEditInput;
        if (i2 == 0) {
            this.channel.setText(String.valueOf(i + 1));
        } else if (i2 == 1) {
            this.numType.setText(String.valueOf(i));
        } else if (i2 == 2) {
            this.data1.setText(String.valueOf(i));
        } else if (i2 == 3) {
            this.data2.setText(String.valueOf(i));
        }
        updateCurrentMidiCodeText();
    }

    /* JADX INFO: renamed from: lambda$new$3$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m151x784b3c22(View view) {
        editMidiCodeFocus(0);
    }

    /* JADX INFO: renamed from: lambda$new$4$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m152xc89abc1(View view) {
        editMidiCodeFocus(1);
    }

    /* JADX INFO: renamed from: lambda$new$5$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m153xa0c81b60(View view) {
        editMidiCodeFocus(2);
    }

    /* JADX INFO: renamed from: lambda$new$6$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m154x35068aff(View view) {
        editMidiCodeFocus(3);
    }

    /* JADX INFO: renamed from: lambda$new$7$com-cubeSuite-fragment-FootControl2-EditInterfacePopupWindow, reason: not valid java name */
    /* synthetic */ void m155xc944fa9e(View view) {
        dismiss();
    }

    public void updateCurrentMidiCodeText() {
        this.tvMidiInfo.setText("[" + (this.currentMidiCodeIndex + 1) + "]   " + ((String) this.channel.getText()) + "   CC   " + ((String) this.numType.getText()) + "   " + ((String) this.data1.getText()) + "~" + ((String) this.data2.getText()));
    }

    private void editMidiCodeFocus(int i) {
        this.currentEditInput = i;
        this.channel.setTextColor(this.context.getResources().getColor(R.color.dominantHue));
        this.numType.setTextColor(this.context.getResources().getColor(R.color.dominantHue));
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
            this.numKeyPad.setValue(String.valueOf(this.numType.getText()));
            this.numType.setTextColor(this.context.getResources().getColor(R.color.accent));
        } else if (i == 2) {
            this.numKeyPad.setValue(String.valueOf(this.data1.getText()));
            this.data1.setTextColor(this.context.getResources().getColor(R.color.accent));
        } else if (i == 3) {
            this.numKeyPad.setValue(String.valueOf(this.data2.getText()));
            this.data2.setTextColor(this.context.getResources().getColor(R.color.accent));
        }
        this.numKeyPad.show();
    }
}
