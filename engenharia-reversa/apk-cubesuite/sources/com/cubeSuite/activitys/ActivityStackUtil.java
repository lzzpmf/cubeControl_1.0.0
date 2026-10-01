package com.cubeSuite.activitys;

import android.app.Activity;
import androidx.appcompat.app.AppCompatActivity;
import java.util.Iterator;
import java.util.Stack;

/* JADX INFO: loaded from: classes.dex */
public class ActivityStackUtil extends AppCompatActivity {
    private static ActivityStackUtil instance;
    private Activity activity;
    private Stack<Activity> activityStack = new Stack<>();

    public Activity getActivity() {
        return this.activity;
    }

    private ActivityStackUtil() {
    }

    public static ActivityStackUtil getInstance() {
        if (instance == null) {
            instance = new ActivityStackUtil();
        }
        return instance;
    }

    public void addActivity(Activity activity) {
        this.activity = activity;
        this.activityStack.push(activity);
    }

    public void exit() {
        Iterator<Activity> it = this.activityStack.iterator();
        while (it.hasNext()) {
            it.next().finish();
        }
    }
}
