package top.defaults.colorpicker;

import android.view.MotionEvent;

/* JADX INFO: loaded from: classes2.dex */
class ThrottledTouchEventHandler {
    private long lastPassedEventTime;
    private int minInterval;
    private Updatable updatable;

    ThrottledTouchEventHandler(Updatable updatable) {
        this(16, updatable);
    }

    private ThrottledTouchEventHandler(int i, Updatable updatable) {
        this.lastPassedEventTime = 0L;
        this.minInterval = i;
        this.updatable = updatable;
    }

    void onTouchEvent(MotionEvent motionEvent) {
        if (this.updatable == null) {
            return;
        }
        long jCurrentTimeMillis = System.currentTimeMillis();
        if (jCurrentTimeMillis - this.lastPassedEventTime <= this.minInterval) {
            return;
        }
        this.lastPassedEventTime = jCurrentTimeMillis;
        this.updatable.update(motionEvent);
    }
}
