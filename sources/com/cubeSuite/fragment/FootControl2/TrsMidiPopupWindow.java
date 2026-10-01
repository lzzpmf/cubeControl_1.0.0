package com.cubeSuite.fragment.FootControl2;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.ImageView;
import android.widget.PopupWindow;
import androidx.appcompat.widget.SwitchCompat;
import com.cubeSuite.R;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.fc2.FC2Struct;

/* JADX INFO: loaded from: classes.dex */
public class TrsMidiPopupWindow extends PopupWindow {
    CommunicationUtil communicationUtil;
    ImageView expressionPedalImage;
    FC2Struct fcEntry;
    SwitchCompat polarityBut;
    ImageView trsMidiImage;

    public TrsMidiPopupWindow(Context context, CommunicationUtil communicationUtil) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_trs_switch_layout, (ViewGroup) null);
        this.communicationUtil = communicationUtil;
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        this.expressionPedalImage = (ImageView) viewInflate.findViewById(R.id.expressionPedal);
        this.trsMidiImage = (ImageView) viewInflate.findViewById(R.id.trsMidi);
        Button button = (Button) viewInflate.findViewById(R.id.back);
        this.polarityBut = (SwitchCompat) viewInflate.findViewById(R.id.polarity_but);
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.TrsMidiPopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m187x2cc2d4be(view);
            }
        });
        this.expressionPedalImage.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.TrsMidiPopupWindow$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m188x2df9279d(view);
            }
        });
        this.trsMidiImage.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.TrsMidiPopupWindow$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m189x2f2f7a7c(view);
            }
        });
        this.polarityBut.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.FootControl2.TrsMidiPopupWindow.1
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                TrsMidiPopupWindow.this.fcEntry.polar.setData(z ? 1 : 0);
                TrsMidiPopupWindow trsMidiPopupWindow = TrsMidiPopupWindow.this;
                trsMidiPopupWindow.sendData(trsMidiPopupWindow.fcEntry.polar);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-TrsMidiPopupWindow, reason: not valid java name */
    /* synthetic */ void m187x2cc2d4be(View view) {
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-TrsMidiPopupWindow, reason: not valid java name */
    /* synthetic */ void m188x2df9279d(View view) {
        this.expressionPedalImage.setBackgroundResource(R.drawable.bg_accent);
        this.trsMidiImage.setBackgroundResource(R.drawable.bg_click_effect);
        this.fcEntry.trs.setData(0);
        sendData(this.fcEntry.trs);
    }

    /* JADX INFO: renamed from: lambda$new$2$com-cubeSuite-fragment-FootControl2-TrsMidiPopupWindow, reason: not valid java name */
    /* synthetic */ void m189x2f2f7a7c(View view) {
        this.trsMidiImage.setBackgroundResource(R.drawable.bg_accent);
        this.expressionPedalImage.setBackgroundResource(R.drawable.bg_click_effect);
        this.fcEntry.trs.setData(1);
        sendData(this.fcEntry.trs);
    }

    void updateView(FC2Struct fC2Struct) {
        this.fcEntry = fC2Struct;
        ImageView imageView = this.expressionPedalImage;
        int data = fC2Struct.trs.getData();
        int i = R.drawable.bg_accent;
        imageView.setBackgroundResource(data == 0 ? R.drawable.bg_accent : R.drawable.bg_click_effect);
        ImageView imageView2 = this.trsMidiImage;
        if (fC2Struct.trs.getData() != 1) {
            i = R.drawable.bg_click_effect;
        }
        imageView2.setBackgroundResource(i);
        this.polarityBut.setChecked(fC2Struct.polar.getData() != 0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendData(AddrU8 addrU8) {
        this.communicationUtil.splitWriteData((byte) 4, new byte[]{addrU8.getByteData()}, addrU8.getAddr());
    }
}
