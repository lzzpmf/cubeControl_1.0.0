package com.cubeSuite.customControl;

import android.app.AlertDialog;
import android.content.Context;
import android.view.View;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.ImageView;
import android.widget.Switch;
import android.widget.TextView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.recyclerview.widget.RecyclerView.Adapter;
import com.cubeSuite.R;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class ListAlertDialog<H extends RecyclerView.Adapter<?>> {
    private final Button close;
    private Context context;
    private final AlertDialog dialog;
    private final RecyclerView list_alert_recycler;
    private final TextIconView title;
    private final Switch toggle;

    public interface BtnClick {
        default void closeBtnClick() {
        }
    }

    public interface ToggleClick {
        void toggleClick(CompoundButton compoundButton, boolean z);
    }

    public static class ListAlertHolder extends RecyclerView.ViewHolder {
        private String deviceName;
        private int id;
        private ImageView image;
        View itemView;
        private TextView name;

        public String getDeviceName() {
            return this.deviceName;
        }

        public void setInfo(String str, int i) {
            this.deviceName = str;
            this.name.setText(str);
            this.image.setImageResource(i);
        }

        public ListAlertHolder(View view) {
            super(view);
            this.itemView = view;
            this.image = (ImageView) view.findViewById(R.id.effect_unit_img);
            this.name = (TextView) view.findViewById(R.id.effect_unit_text);
        }
    }

    public ListAlertDialog(Context context) {
        this.context = context;
        AlertDialog.Builder builder = new AlertDialog.Builder(context);
        View viewInflate = View.inflate(context, R.layout.list_alert_dialog, null);
        this.title = (TextIconView) viewInflate.findViewById(R.id.dialog_title);
        this.list_alert_recycler = (RecyclerView) viewInflate.findViewById(R.id.list_alert_recycler);
        Button button = (Button) viewInflate.findViewById(R.id.close);
        this.close = button;
        this.toggle = (Switch) viewInflate.findViewById(R.id.toggle);
        builder.setCancelable(false);
        builder.setView(viewInflate);
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.ListAlertDialog$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m100lambda$new$0$comcubeSuitecustomControlListAlertDialog(view);
            }
        });
        this.dialog = builder.create();
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-customControl-ListAlertDialog, reason: not valid java name */
    /* synthetic */ void m100lambda$new$0$comcubeSuitecustomControlListAlertDialog(View view) {
        closeDialog();
    }

    public ListAlertDialog setTitle(String str) {
        this.title.setText(str);
        return this;
    }

    public ListAlertDialog setTitle(int i) {
        this.title.setText(i);
        return this;
    }

    public ListAlertDialog setContent(H h) {
        h.setHasStableIds(true);
        LinearLayoutManager linearLayoutManager = new LinearLayoutManager(this.context);
        linearLayoutManager.setOrientation(1);
        this.list_alert_recycler.setLayoutManager(linearLayoutManager);
        this.list_alert_recycler.setAdapter(h);
        return this;
    }

    public ListAlertDialog setBtnClick(final BtnClick btnClick) {
        this.close.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.ListAlertDialog$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m101lambda$setBtnClick$1$comcubeSuitecustomControlListAlertDialog(btnClick, view);
            }
        });
        return this;
    }

    /* JADX INFO: renamed from: lambda$setBtnClick$1$com-cubeSuite-customControl-ListAlertDialog, reason: not valid java name */
    /* synthetic */ void m101lambda$setBtnClick$1$comcubeSuitecustomControlListAlertDialog(BtnClick btnClick, View view) {
        btnClick.closeBtnClick();
        closeDialog();
    }

    public ListAlertDialog setToggleClick(final ToggleClick toggleClick) {
        this.toggle.setVisibility(0);
        Switch r0 = this.toggle;
        Objects.requireNonNull(toggleClick);
        r0.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.customControl.ListAlertDialog$$ExternalSyntheticLambda1
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public final void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                toggleClick.toggleClick(compoundButton, z);
            }
        });
        return this;
    }

    public void setToggleChecked(boolean z) {
        this.toggle.setChecked(z);
    }

    public void closeDialog() {
        if (this.context == null) {
            return;
        }
        this.dialog.dismiss();
    }

    public ListAlertDialog showDialog() {
        if (this.context == null) {
            return this;
        }
        this.dialog.show();
        return this;
    }
}
