package com.cubeSuite.customControl;

import android.app.Activity;
import android.app.AlertDialog;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class AlertDialogUtil {
    private static AlertDialogUtil alertDialogUtil;
    private Activity activity;
    private final Button cancel;
    private final Button confirm;
    private final TextView content;
    private final AlertDialog dialog;
    private final LinearLayout linear;
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

    private AlertDialogUtil(Activity activity) {
        this.activity = activity;
        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
        View viewInflate = View.inflate(activity, R.layout.custorm_alert_dialog, null);
        this.title = (TextView) viewInflate.findViewById(R.id.dialog_title);
        this.content = (TextView) viewInflate.findViewById(R.id.dialog_content);
        this.confirm = (Button) viewInflate.findViewById(R.id.dialog_confirm);
        this.cancel = (Button) viewInflate.findViewById(R.id.dialog_cancel);
        this.linear = (LinearLayout) viewInflate.findViewById(R.id.linear);
        builder.setCancelable(false);
        builder.setView(viewInflate);
        this.dialog = builder.create();
    }

    public AlertDialogUtil setState(boolean z) {
        if (z) {
            this.linear.setBackgroundResource(R.drawable.bg_click_effect_ban);
            this.title.setTextColor(this.activity.getColor(R.color.disable));
            return this;
        }
        this.linear.setBackgroundResource(R.drawable.bg_hollow_fillet_dominant_hue);
        this.title.setTextColor(this.activity.getColor(R.color.dominantHue));
        return this;
    }

    public AlertDialogUtil setBtnVisible(SelectBtn selectBtn) {
        this.cancel.setVisibility(0);
        this.confirm.setVisibility(0);
        int iOrdinal = selectBtn.ordinal();
        if (iOrdinal == 0) {
            this.cancel.setVisibility(8);
        } else if (iOrdinal == 1) {
            this.confirm.setVisibility(8);
        } else if (iOrdinal == 3) {
            this.confirm.setVisibility(8);
            this.cancel.setVisibility(8);
        }
        return alertDialogUtil;
    }

    public AlertDialogUtil setTitle(String str) {
        alertDialogUtil.title.setText(str);
        return alertDialogUtil;
    }

    public AlertDialogUtil setTitle(int i) {
        alertDialogUtil.title.setText(i);
        return alertDialogUtil;
    }

    public AlertDialogUtil setContent(String str) {
        alertDialogUtil.content.setText(str);
        return alertDialogUtil;
    }

    public AlertDialogUtil setContent(int i) {
        alertDialogUtil.content.setText(i);
        return alertDialogUtil;
    }

    public AlertDialogUtil setConfirmBtnText(int i) {
        alertDialogUtil.confirm.setText(i);
        return alertDialogUtil;
    }

    public AlertDialogUtil setConfirmBtnText(String str) {
        alertDialogUtil.confirm.setText(str);
        return alertDialogUtil;
    }

    public AlertDialogUtil setCancelBtnText(int i) {
        alertDialogUtil.cancel.setText(i);
        return alertDialogUtil;
    }

    public AlertDialogUtil setCancelBtnText(String str) {
        alertDialogUtil.cancel.setText(str);
        return alertDialogUtil;
    }

    public AlertDialogUtil setCancelColor(int i, int i2) {
        alertDialogUtil.cancel.setBackgroundResource(i);
        alertDialogUtil.cancel.setTextColor(this.activity.getResources().getColor(i2));
        return alertDialogUtil;
    }

    public AlertDialogUtil setBtnClick(BtnClick btnClick) {
        return setBtnClick(btnClick, true);
    }

    public AlertDialogUtil setBtnClick(final BtnClick btnClick, final boolean z) {
        this.confirm.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.AlertDialogUtil$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                AlertDialogUtil.lambda$setBtnClick$0(btnClick, z, view);
            }
        });
        this.cancel.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.AlertDialogUtil$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                AlertDialogUtil.lambda$setBtnClick$1(btnClick, view);
            }
        });
        return alertDialogUtil;
    }

    static /* synthetic */ void lambda$setBtnClick$0(BtnClick btnClick, boolean z, View view) {
        btnClick.confirmBtnClick();
        if (z) {
            alertDialogUtil.closeDialog();
        }
    }

    static /* synthetic */ void lambda$setBtnClick$1(BtnClick btnClick, View view) {
        btnClick.cancelBtnClick();
        alertDialogUtil.closeDialog();
    }

    public AlertDialogUtil closeDialog() {
        if (this.activity.isFinishing()) {
            return alertDialogUtil;
        }
        this.dialog.dismiss();
        return alertDialogUtil;
    }

    public AlertDialogUtil showErrorDialog() {
        if (this.activity.isFinishing() || this.dialog.isShowing()) {
            return alertDialogUtil;
        }
        setState(true);
        this.dialog.show();
        return alertDialogUtil;
    }

    public AlertDialogUtil showDialog() {
        if (this.activity.isFinishing() || this.dialog.isShowing()) {
            return alertDialogUtil;
        }
        setState(false);
        this.dialog.show();
        return alertDialogUtil;
    }

    public static AlertDialogUtil getInstance(Activity activity) {
        if (activity == null) {
            return alertDialogUtil;
        }
        AlertDialogUtil alertDialogUtil2 = alertDialogUtil;
        if (alertDialogUtil2 == null) {
            alertDialogUtil = new AlertDialogUtil(activity);
        } else {
            Activity activity2 = alertDialogUtil2.activity;
            if (activity2 == null) {
                alertDialogUtil2.activity = activity;
            } else if (activity2 != activity) {
                alertDialogUtil2.activity = activity;
                alertDialogUtil = new AlertDialogUtil(activity);
            }
        }
        alertDialogUtil.setBtnClick(new BtnClick() { // from class: com.cubeSuite.customControl.AlertDialogUtil.1
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
            }
        });
        return alertDialogUtil;
    }
}
