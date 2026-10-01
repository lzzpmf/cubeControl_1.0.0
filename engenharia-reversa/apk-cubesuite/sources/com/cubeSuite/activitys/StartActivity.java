package com.cubeSuite.activitys;

import android.content.Intent;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import com.cubeSuite.Global.Global;
import com.cubeSuite.MainActivity;
import com.cubeSuite.R;
import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.config.WebConfig;
import com.cubeSuite.entity.other.AppUpdateInfo;
import com.cubeSuite.utils.HttpUtil;
import com.cubeSuite.utils.instrument.HttpCallback;
import com.google.gson.Gson;
import org.jsoup.Jsoup;
import org.jsoup.nodes.Element;

/* JADX INFO: loaded from: classes.dex */
public class StartActivity extends AppCompatActivity {
    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.start_activity);
        getWindow().addFlags(1024);
        new Thread(new Runnable() { // from class: com.cubeSuite.activitys.StartActivity.1
            @Override // java.lang.Runnable
            public void run() {
                StartActivity.this.getPdfVersion();
                StartActivity.this.getAppVersion();
            }
        }).start();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void getFirmwareVersion() {
        HttpUtil.get(WebConfig.FIRMWARE_FILE_URL_LIST, new HttpCallback() { // from class: com.cubeSuite.activitys.StartActivity.2
            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void successCallBack(String str) {
                for (Element element : Jsoup.parse(str).select("a")) {
                    if (element.text().substring(element.text().length() - 3).equals("ufw")) {
                        String[] strArrSplit = element.text().substring(0, element.text().length() - 4).split("_");
                        BleHandle.firmwareListMap.put(strArrSplit[0].toUpperCase(), Integer.valueOf(strArrSplit[1]));
                    }
                }
                StartActivity.this.startActivity(new Intent(StartActivity.this.getApplicationContext(), (Class<?>) MainActivity.class));
                StartActivity.this.finish();
            }

            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void failureCallBack() {
                StartActivity.this.startActivity(new Intent(StartActivity.this.getApplicationContext(), (Class<?>) MainActivity.class));
                StartActivity.this.finish();
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void getAppVersion() {
        HttpUtil.get(WebConfig.APP_UPDATE_INFO_PATH, new HttpCallback() { // from class: com.cubeSuite.activitys.StartActivity.3
            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void failureCallBack() {
            }

            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void successCallBack(String str) {
                try {
                    Global.g_updateAppInfo = (AppUpdateInfo) new Gson().fromJson(str, AppUpdateInfo.class);
                    Global.g_updateAppInfo.setNeedUpdate(32 < Global.g_updateAppInfo.getAppVer());
                } catch (Exception unused) {
                }
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void getPdfVersion() {
        HttpUtil.get(WebConfig.PDF_URL_LIST, new HttpCallback() { // from class: com.cubeSuite.activitys.StartActivity.4
            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void successCallBack(String str) {
                for (Element element : Jsoup.parse(str).select("a")) {
                    if (element.text().substring(element.text().length() - 3).equals("pdf")) {
                        String[] strArrSplit = element.text().substring(0, element.text().length() - 4).split("_");
                        BleHandle.pdfListMap.put(strArrSplit[0], Integer.valueOf(strArrSplit[1]));
                    }
                }
                StartActivity.this.getFirmwareVersion();
            }

            @Override // com.cubeSuite.utils.instrument.HttpCallback
            public void failureCallBack() {
                StartActivity.this.getFirmwareVersion();
            }
        });
    }
}
