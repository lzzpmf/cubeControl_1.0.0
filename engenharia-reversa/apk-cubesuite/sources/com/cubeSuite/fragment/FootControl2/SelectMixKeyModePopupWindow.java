package com.cubeSuite.fragment.FootControl2;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.PopupWindow;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.SelectItem;
import com.cubeSuite.entity.fc2.AdvCustomStruct1;

/* JADX INFO: loaded from: classes.dex */
public class SelectMixKeyModePopupWindow extends PopupWindow {
    private SelectItem[] selectModelItem = new SelectItem[5];

    public SelectMixKeyModePopupWindow(Context context, final AdvCustomStruct1 advCustomStruct1, final SelectMixKeyModePopupWindowCallback selectMixKeyModePopupWindowCallback) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_select_foot_ctrl_mix_key_mode_layout, (ViewGroup) null);
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-1);
        setFocusable(true);
        ButtonIconView buttonIconView = (ButtonIconView) viewInflate.findViewById(R.id.back);
        final int i = 0;
        this.selectModelItem[0] = (SelectItem) viewInflate.findViewById(R.id.singleStepOneMode);
        this.selectModelItem[1] = (SelectItem) viewInflate.findViewById(R.id.singleStepTwoMode);
        this.selectModelItem[2] = (SelectItem) viewInflate.findViewById(R.id.onTheLooseTwoMode);
        this.selectModelItem[3] = (SelectItem) viewInflate.findViewById(R.id.longOnMode);
        this.selectModelItem[4] = (SelectItem) viewInflate.findViewById(R.id.singleOrLongOnMode);
        buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.SelectMixKeyModePopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m183x6a120742(view);
            }
        });
        while (true) {
            SelectItem[] selectItemArr = this.selectModelItem;
            if (i >= selectItemArr.length) {
                return;
            }
            selectItemArr[i].setItemClick(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.SelectMixKeyModePopupWindow$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m184xb7d17f43(advCustomStruct1, i, selectMixKeyModePopupWindowCallback, view);
                }
            });
            i++;
        }
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-SelectMixKeyModePopupWindow, reason: not valid java name */
    /* synthetic */ void m183x6a120742(View view) {
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-SelectMixKeyModePopupWindow, reason: not valid java name */
    /* synthetic */ void m184xb7d17f43(AdvCustomStruct1 advCustomStruct1, int i, SelectMixKeyModePopupWindowCallback selectMixKeyModePopupWindowCallback, View view) {
        advCustomStruct1.mode.setData(i);
        selectMixKeyModePopupWindowCallback.onSelect(i);
        dismiss();
    }
}
