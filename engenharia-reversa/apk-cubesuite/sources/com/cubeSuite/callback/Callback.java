package com.cubeSuite.callback;

import android.widget.Toast;
import com.cubeSuite.R;
import com.cubeSuite.activitys.ActivityStackUtil;

/* JADX INFO: loaded from: classes.dex */
public class Callback {

    public interface HolderItemClick {
        void click(int i);
    }

    public interface MidikeyboardCallback {
        void usrCreateComp();
    }

    public interface OpenNotify {
        void openStatus(boolean z);
    }

    public interface SpinnerCallback {
        void select(int i);
    }

    public interface BleCommunicationCallback {
        default void eraseCallback(boolean z) {
            if (z || ActivityStackUtil.getInstance().getActivity() == null) {
                return;
            }
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.eraseDataFail, 1).show();
        }

        default void writeDataCallback(boolean z) {
            if (z || ActivityStackUtil.getInstance().getActivity() == null) {
                return;
            }
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.dataSendFail, 1).show();
        }

        default void readDataCallback(boolean z, byte[] bArr) {
            if (z || ActivityStackUtil.getInstance().getActivity() == null) {
                return;
            }
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.readDataFail, 1).show();
        }

        default void queryDataCallback(boolean z, byte[] bArr) {
            if (z || ActivityStackUtil.getInstance().getActivity() == null) {
                return;
            }
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.readDataFail, 1).show();
        }
    }
}
