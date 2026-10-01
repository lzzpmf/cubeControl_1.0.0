package com.cubeSuite.customControl;

import android.app.AlertDialog;
import android.content.Context;
import android.view.View;
import android.widget.Button;
import android.widget.GridLayout;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class SelectKeyDialog {
    public final String[] aztab = new String[34];
    private GridLayout contentList;
    private final Context context;
    private final AlertDialog dialog;
    private Listening listening;
    private final TextIconView title;

    public interface Listening {
        default void itemClick(int i) {
        }
    }

    public SelectKeyDialog(Context context) {
        for (int i = 0; i < 26; i++) {
            this.aztab[i] = "" + ((char) (i + 65));
        }
        String[] strArr = this.aztab;
        strArr[26] = "↑";
        strArr[27] = "↓";
        strArr[28] = "←";
        strArr[29] = "→";
        strArr[30] = "PAGE UP";
        strArr[31] = "PAGE DOWN";
        strArr[32] = "SPACE";
        strArr[33] = "Enter";
        this.context = context;
        AlertDialog.Builder builder = new AlertDialog.Builder(context);
        View viewInflate = View.inflate(context, R.layout.select_key_dialog, null);
        this.title = (TextIconView) viewInflate.findViewById(R.id.title);
        this.contentList = (GridLayout) viewInflate.findViewById(R.id.contentList);
        Button button = (Button) viewInflate.findViewById(R.id.close);
        builder.setCancelable(false);
        builder.setView(viewInflate);
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SelectKeyDialog$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m107lambda$new$0$comcubeSuitecustomControlSelectKeyDialog(view);
            }
        });
        for (final int i2 = 0; i2 < this.aztab.length; i2++) {
            Button button2 = new Button(context);
            button2.setText(this.aztab[i2]);
            this.contentList.addView(button2, getGridLayout((i2 / 2) + 1, i2 % 2));
            button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SelectKeyDialog$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m108lambda$new$1$comcubeSuitecustomControlSelectKeyDialog(i2, view);
                }
            });
        }
        this.dialog = builder.create();
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-customControl-SelectKeyDialog, reason: not valid java name */
    /* synthetic */ void m107lambda$new$0$comcubeSuitecustomControlSelectKeyDialog(View view) {
        closeDialog();
    }

    /* JADX INFO: renamed from: lambda$new$1$com-cubeSuite-customControl-SelectKeyDialog, reason: not valid java name */
    /* synthetic */ void m108lambda$new$1$comcubeSuitecustomControlSelectKeyDialog(int i, View view) {
        Listening listening = this.listening;
        if (listening != null) {
            listening.itemClick(i);
        }
        closeDialog();
    }

    private GridLayout.LayoutParams getGridLayout(int i, int i2) {
        return new GridLayout.LayoutParams(GridLayout.spec(i, 1.0f), GridLayout.spec(i2, 1.0f));
    }

    public SelectKeyDialog setTitle(String str) {
        this.title.setText(str);
        return this;
    }

    public SelectKeyDialog setTitle(int i) {
        this.title.setText(i);
        return this;
    }

    public void setListening(Listening listening) {
        this.listening = listening;
    }

    public void closeDialog() {
        if (this.context == null) {
            return;
        }
        this.dialog.dismiss();
    }

    public SelectKeyDialog showDialog() {
        if (this.context == null) {
            return this;
        }
        this.dialog.show();
        return this;
    }
}
