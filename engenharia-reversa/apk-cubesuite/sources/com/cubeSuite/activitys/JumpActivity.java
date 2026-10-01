package com.cubeSuite.activitys;

import android.content.Intent;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import com.cubeSuite.R;
import com.cubeSuite.utils.FileUtils;

/* JADX INFO: loaded from: classes.dex */
public class JumpActivity extends AppCompatActivity {
    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.start_activity);
        Intent intent = getIntent();
        String action = intent.getAction();
        String type = intent.getType();
        if ("android.intent.action.VIEW".equals(action) && "application/octet-stream".equals(type)) {
            System.out.println(FileUtils.readFile(intent.getData().toString()));
        }
    }
}
