package com.cubeSuite.activitys;

import android.bluetooth.BluetoothGatt;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.view.OnApplyWindowInsetsListener;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.core.view.WindowInsetsControllerCompat;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import com.cubeSuite.R;
import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.config.BleEnum;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.fragment.WebPdfFragment;
import com.cubeSuite.fragment.looperDrum.LooperDrumFragment;
import com.fastble.BleManager;
import com.fastble.callback.BleGattCallback;
import com.fastble.callback.BleMtuChangedCallback;
import com.fastble.data.BleDevice;
import com.fastble.exception.BleException;

/* JADX INFO: loaded from: classes.dex */
public class ControlAndPdfActivity extends BaseDeviceActivity {
    private static final int MSG_CONNECTION_DEVICE = 10001;
    private AppCompatActivity activity;
    private Button back;
    ControlBaseFragment controlFragment;
    private Intent intent;
    private WebPdfFragment webPdfFragment;
    FragmentManager manager = getSupportFragmentManager();
    private Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != ControlAndPdfActivity.MSG_CONNECTION_DEVICE) {
                return false;
            }
            BleManager.getInstance().connect(BleHandle.connectDeviceInfo.getBleDevice(), ControlAndPdfActivity.this.bleGattCallback);
            AlertDialogUtil.getInstance(ControlAndPdfActivity.this.activity).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity.1.1
                @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
                public void cancelBtnClick() {
                    ControlAndPdfActivity.this.backHomePage();
                }
            }).setTitle(R.string.hint).setContent(R.string.bt_connecting).setBtnVisible(AlertDialogUtil.SelectBtn.CANCEL);
            return false;
        }
    });
    private BleGattCallback bleGattCallback = new BleGattCallback() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity.2
        @Override // com.fastble.callback.BleGattCallback
        public void onConnectFail(BleDevice bleDevice, BleException bleException) {
        }

        @Override // com.fastble.callback.BleGattCallback
        public void onStartConnect() {
        }

        @Override // com.fastble.callback.BleGattCallback
        public void onConnectSuccess(BleDevice bleDevice, BluetoothGatt bluetoothGatt, int i) {
            ControlAndPdfActivity.this.setMtu();
        }

        @Override // com.fastble.callback.BleGattCallback
        public void onDisConnected(boolean z, BleDevice bleDevice, BluetoothGatt bluetoothGatt, int i) {
            AlertDialogUtil.getInstance(ControlAndPdfActivity.this.activity).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity.2.1
                @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
                public void cancelBtnClick() {
                    ControlAndPdfActivity.this.backHomePage();
                }
            }).setTitle(R.string.hint).setContent(R.string.connect_fail).setBtnVisible(AlertDialogUtil.SelectBtn.CANCEL);
        }
    };

    public ControlAndPdfActivity() {
        this.handler.sendEmptyMessage(MSG_CONNECTION_DEVICE);
        this.webPdfFragment = new WebPdfFragment();
        this.controlFragment = BleHandle.connectDeviceInfo.getControlFragment();
    }

    @Override // androidx.fragment.app.FragmentActivity, android.app.Activity
    public void onResume() {
        if ((this.controlFragment instanceof LooperDrumFragment) && this.activity.getRequestedOrientation() != 0) {
            this.activity.setRequestedOrientation(0);
        }
        super.onResume();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void backHomePage() {
        BleManager.getInstance().disconnectAllDevice();
        this.activity.finish();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setMtu() {
        BleManager.getInstance().setMtu(BleHandle.connectDeviceInfo.getBleDevice(), BleEnum.BLE_MTU_MAX, new BleMtuChangedCallback() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity.3
            @Override // com.fastble.callback.BleMtuChangedCallback
            public void onSetMTUFailure(BleException bleException) {
                Log.e("BLE", "set MTU failure Exception: " + bleException);
            }

            @Override // com.fastble.callback.BleMtuChangedCallback
            public void onMtuChanged(int i) {
                ControlAndPdfActivity.this.controlFragment.initialize();
            }
        });
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.control_and_pdf_activity);
        new WindowInsetsControllerCompat(getWindow(), getWindow().getDecorView()).setAppearanceLightStatusBars(true);
        getWindow().setStatusBarColor(getResources().getColor(R.color.red_FF688C, null));
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.rootLayout), new OnApplyWindowInsetsListener() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity$$ExternalSyntheticLambda0
            @Override // androidx.core.view.OnApplyWindowInsetsListener
            public final WindowInsetsCompat onApplyWindowInsets(View view, WindowInsetsCompat windowInsetsCompat) {
                return ControlAndPdfActivity.lambda$onCreate$0(view, windowInsetsCompat);
            }
        });
        Montior();
        initOnclick();
        this.activity = this;
        ActivityStackUtil.getInstance().addActivity(this);
        if (bundle == null) {
            getSupportFragmentManager().beginTransaction().add(R.id.cuvave_body, this.controlFragment, (String) null).commit();
        }
        changeFragment(0);
    }

    static /* synthetic */ WindowInsetsCompat lambda$onCreate$0(View view, WindowInsetsCompat windowInsetsCompat) {
        view.setPadding(view.getPaddingLeft(), windowInsetsCompat.getInsets(WindowInsetsCompat.Type.statusBars()).f6top, view.getPaddingRight(), view.getPaddingBottom());
        return windowInsetsCompat;
    }

    @Override // android.app.Activity
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.region_right_menu, menu);
        return true;
    }

    @Override // androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onSaveInstanceState(Bundle bundle) {
        super.onSaveInstanceState(bundle);
    }

    private void Montior() {
        this.back = (Button) findViewById(R.id.back);
    }

    private void initOnclick() {
        this.back.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.activitys.ControlAndPdfActivity$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m90x8d62ebed(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$initOnclick$1$com-cubeSuite-activitys-ControlAndPdfActivity, reason: not valid java name */
    /* synthetic */ void m90x8d62ebed(View view) {
        BleManager.getInstance().disconnectAllDevice();
        this.activity.finish();
    }

    private void changeFragment(int i) {
        FragmentTransaction fragmentTransactionBeginTransaction = this.manager.beginTransaction();
        if (i == 0) {
            fragmentTransactionBeginTransaction.show(this.controlFragment);
        }
        fragmentTransactionBeginTransaction.commit();
    }

    @Override // androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    public void onDestroy() {
        super.onDestroy();
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        BleManager.getInstance().disconnectAllDevice();
        overridePendingTransition(0, 0);
        super.onBackPressed();
    }
}
