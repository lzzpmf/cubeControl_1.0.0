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
public class SelectMode1PopupWindow extends PopupWindow {
    private SelectItem[] selectModelItem = new SelectItem[5];

    public SelectMode1PopupWindow(Context context, final AdvCustomStruct1 advCustomStruct1, final SelectMode1PopupWindowCallback selectMode1PopupWindowCallback) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.fc2_select_foot_ctrl_mode1_layout, (ViewGroup) null);
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
        buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.SelectMode1PopupWindow$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m185x9fffea62(view);
            }
        });
        while (true) {
            SelectItem[] selectItemArr = this.selectModelItem;
            if (i >= selectItemArr.length) {
                return;
            }
            selectItemArr[i].setItemClick(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.SelectMode1PopupWindow$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m186xa603b5c1(advCustomStruct1, i, selectMode1PopupWindowCallback, view);
                }
            });
            i++;
        }
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-fragment-FootControl2-SelectMode1PopupWindow, reason: not valid java name */
    /* synthetic */ void m185x9fffea62(View view) {
        dismiss();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-fragment-FootControl2-SelectMode1PopupWindow, reason: not valid java name */
    /* synthetic */ void m186xa603b5c1(AdvCustomStruct1 advCustomStruct1, int i, SelectMode1PopupWindowCallback selectMode1PopupWindowCallback, View view) {
        advCustomStruct1.mode.setData(i);
        selectMode1PopupWindowCallback.onSelect(i);
        dismiss();
    }
}
