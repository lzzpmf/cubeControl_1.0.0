package com.cubeSuite.customControl;

import android.app.AlertDialog;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import com.cubeSuite.R;
import com.cubeSuite.utils.AppUtil;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class UpdateAppInfoDialog {
    private static UpdateAppInfoDialog updateAppInfoDialog;
    private AppCompatActivity activity;
    private final Button cancel;
    private final Button confirm;
    private final LinearLayout content;
    private final AlertDialog dialog;
    private final TextView title;

    public interface BtnClick {
        default void cancelBtnClick() {
        }

        default void confirmBtnClick() {
        }
    }

    public enum SelectBtn {
        CONFIRM,
        CANCEL,
        ALL_VISIBLE,
        ALL_INVISIBLE
    }

    private UpdateAppInfoDialog(AppCompatActivity appCompatActivity) {
        this.activity = appCompatActivity;
        AlertDialog.Builder builder = new AlertDialog.Builder(appCompatActivity);
        View viewInflate = View.inflate(appCompatActivity, R.layout.update_appinfo_dialog, null);
        this.title = (TextView) viewInflate.findViewById(R.id.title);
        this.content = (LinearLayout) viewInflate.findViewById(R.id.content);
        this.confirm = (Button) viewInflate.findViewById(R.id.confirm);
        this.cancel = (Button) viewInflate.findViewById(R.id.cancel);
        builder.setCancelable(false);
        builder.setView(viewInflate);
        this.dialog = builder.create();
    }

    public UpdateAppInfoDialog setTitle(String str) {
        updateAppInfoDialog.title.setText(str);
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog setTitle(int i) {
        updateAppInfoDialog.title.setText(i);
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog addContent(String str) {
        ArrayList arrayList = new ArrayList(1);
        arrayList.add(str);
        return addContent(arrayList);
    }

    public UpdateAppInfoDialog addContent(List<String> list) {
        for (String str : list) {
            TextView textView = new TextView(this.activity);
            textView.setText(str);
            textView.setPadding(AppUtil.dp2px(10.0f), AppUtil.dp2px(5.0f), AppUtil.dp2px(10.0f), AppUtil.dp2px(5.0f));
            textView.setTextColor(this.activity.getColor(R.color.dominantHue));
            this.content.addView(textView);
        }
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog overlayContent(List<String> list) {
        this.content.removeAllViews();
        return addContent(list);
    }

    public UpdateAppInfoDialog overlayContent(String str) {
        ArrayList arrayList = new ArrayList(1);
        arrayList.add(str);
        return overlayContent(arrayList);
    }

    public UpdateAppInfoDialog addContent(int i) {
        return addContent(this.activity.getString(i));
    }

    public UpdateAppInfoDialog setContent(String str) {
        if (this.content.getChildCount() == 1) {
            ((TextView) this.content.getChildAt(0)).setText(str);
            return this;
        }
        return overlayContent(str);
    }

    public UpdateAppInfoDialog setContent(List<String> list) {
        this.content.removeAllViews();
        for (String str : list) {
            TextView textView = new TextView(this.activity);
            textView.setText(str);
            textView.setPadding(AppUtil.dp2px(10.0f), AppUtil.dp2px(5.0f), AppUtil.dp2px(10.0f), AppUtil.dp2px(5.0f));
            textView.setTextColor(this.activity.getColor(R.color.dominantHue));
            this.content.addView(textView);
        }
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog setInstallBtnVis(int i) {
        this.confirm.setVisibility(i);
        return this;
    }

    public UpdateAppInfoDialog setConfirmBtnText(int i) {
        updateAppInfoDialog.confirm.setText(i);
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog setBtnClick(BtnClick btnClick) {
        return setBtnClick(btnClick, true);
    }

    public UpdateAppInfoDialog setBtnClick(final BtnClick btnClick, final boolean z) {
        this.confirm.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.UpdateAppInfoDialog$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                UpdateAppInfoDialog.lambda$setBtnClick$0(btnClick, z, view);
            }
        });
        this.cancel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.UpdateAppInfoDialog$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                UpdateAppInfoDialog.lambda$setBtnClick$1(btnClick, view);
            }
        });
        return updateAppInfoDialog;
    }

    static /* synthetic */ void lambda$setBtnClick$0(BtnClick btnClick, boolean z, View view) {
        btnClick.confirmBtnClick();
        if (z) {
            updateAppInfoDialog.closeDialog();
        }
    }

    static /* synthetic */ void lambda$setBtnClick$1(BtnClick btnClick, View view) {
        btnClick.cancelBtnClick();
        updateAppInfoDialog.closeDialog();
    }

    public UpdateAppInfoDialog closeDialog() {
        if (this.activity.isFinishing()) {
            return updateAppInfoDialog;
        }
        this.dialog.dismiss();
        return updateAppInfoDialog;
    }

    public UpdateAppInfoDialog showDialog() {
        if (this.activity.isFinishing() || this.dialog.isShowing()) {
            return updateAppInfoDialog;
        }
        this.dialog.show();
        return updateAppInfoDialog;
    }

    public static UpdateAppInfoDialog instance(AppCompatActivity appCompatActivity) {
        if (appCompatActivity == null) {
            return updateAppInfoDialog;
        }
        UpdateAppInfoDialog updateAppInfoDialog2 = updateAppInfoDialog;
        if (updateAppInfoDialog2 == null) {
            updateAppInfoDialog = new UpdateAppInfoDialog(appCompatActivity);
        } else {
            AppCompatActivity appCompatActivity2 = updateAppInfoDialog2.activity;
            if (appCompatActivity2 == null) {
                updateAppInfoDialog2.activity = appCompatActivity;
            } else if (appCompatActivity2 != appCompatActivity) {
                updateAppInfoDialog2.activity = appCompatActivity;
                updateAppInfoDialog = new UpdateAppInfoDialog(appCompatActivity);
            }
        }
        return updateAppInfoDialog;
    }
}
