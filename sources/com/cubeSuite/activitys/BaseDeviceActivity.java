package com.cubeSuite.activitys;

import android.content.Intent;
import androidx.appcompat.app.AppCompatActivity;
import com.cubeSuite.MainActivity;
import com.cubeSuite.bluetooth.BleHandle;
import com.fastble.BleManager;

/* JADX INFO: loaded from: classes.dex */
public class BaseDeviceActivity extends AppCompatActivity {
    private Intent intent;

    public void jumpToMain() {
        if (this.intent == null) {
            this.intent = new Intent();
        }
        this.intent.setClass(getApplicationContext(), MainActivity.class);
        BleManager.getInstance().disconnectAllDevice();
        BleHandle.homeDeviceList.clear();
        startActivity(this.intent);
        overridePendingTransition(0, 0);
        ActivityStackUtil.getInstance().exit();
    }
}
