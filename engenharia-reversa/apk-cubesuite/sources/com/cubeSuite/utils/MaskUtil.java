package com.cubeSuite.utils;

import android.app.Activity;
import android.app.ProgressDialog;

/* JADX INFO: loaded from: classes.dex */
public class MaskUtil {
    public static ProgressDialog showProgressDialog(String str, Activity activity) {
        ProgressDialog progressDialog = new ProgressDialog(activity);
        progressDialog.setCanceledOnTouchOutside(false);
        progressDialog.setCancelable(true);
        progressDialog.setMessage(str);
        progressDialog.show();
        return progressDialog;
    }
}
