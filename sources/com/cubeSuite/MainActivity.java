package com.cubeSuite;

import android.app.DownloadManager;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import androidx.activity.result.ActivityResult;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.core.view.OnApplyWindowInsetsListener;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.recyclerview.widget.DefaultItemAnimator;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.ItemTouchHelper;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.activitys.ActivityStackUtil;
import com.cubeSuite.adapter.HomePageAdapter;
import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.config.OtherEnum;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.utils.FileUtils;
import com.cubeSuite.utils.WGDownloadManager;
import com.fastble.BleManager;
import com.fastble.callback.BleScanCallback;
import com.fastble.data.BleDevice;
import com.fastble.scan.BleScanRuleConfig;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class MainActivity extends AppCompatActivity {
    public static String APK_NAME = "/CubeSuite.apk";
    public static String APP_FILE_DIR = null;
    public static String EXPORT_WAV_FILE_PATH = null;
    private static final int MY_PERMISSIONS_REQUEST_ACCESS_COARSE_LOCATION = 1;
    public static String OTA_UPDATE_DIR = null;
    public static String OTA_UPDATE_PATH = null;
    private static final int REQUEST_ENABLE_BT = 2;
    public static String SHARE_DRUM_DIR = null;
    private static final int START_SCAN = 10003;
    public static AppCompatActivity activity;
    public static Context context;
    String[] PERMISSIONS;
    private GridLayoutManager gridLayoutManager;
    private HomePageAdapter instrumentAdapter;
    private RecyclerView mInstrumentRecyclerView;
    private long mTaskId;
    private Button searchBleBtn;
    private boolean isScan = false;
    WGDownloadManager downloadManage = new WGDownloadManager();
    private DownloadManager downloadManager = null;
    private final ActivityResultLauncher<Intent> enableBtLauncher = registerForActivityResult(new ActivityResultContracts.StartActivityForResult(), new ActivityResultCallback() { // from class: com.cubeSuite.MainActivity$$ExternalSyntheticLambda2
        @Override // androidx.activity.result.ActivityResultCallback
        public final void onActivityResult(Object obj) {
            MainActivity.lambda$new$0((ActivityResult) obj);
        }
    });

    public static Context getContext() {
        return context;
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        super.onRequestPermissionsResult(i, strArr, iArr);
        if (1001 == i) {
            boolean z = true;
            for (int i2 : iArr) {
                if (i2 == -1) {
                    z = false;
                }
            }
            if (!z || isEnableBluetooth()) {
                return;
            }
            this.enableBtLauncher.launch(new Intent("android.bluetooth.adapter.action.REQUEST_ENABLE"));
        }
    }

    static /* synthetic */ void lambda$new$0(ActivityResult activityResult) {
        if (activityResult.getResultCode() == -1) {
            Log.d("BT", "Bluetooth enabled");
        } else {
            Log.d("BT", "Bluetooth enable request denied");
        }
    }

    private boolean isEnableBluetooth() {
        BluetoothAdapter defaultAdapter = BluetoothAdapter.getDefaultAdapter();
        return defaultAdapter != null && defaultAdapter.isEnabled();
    }

    public boolean checkHasPermissions() {
        for (String str : this.PERMISSIONS) {
            if (ContextCompat.checkSelfPermission(context, str) != 0) {
                return false;
            }
        }
        return true;
    }

    private void startScan() {
        if (!checkHasPermissions()) {
            ActivityCompat.requestPermissions(this, this.PERMISSIONS, 1001);
            return;
        }
        if (!isEnableBluetooth()) {
            this.enableBtLauncher.launch(new Intent("android.bluetooth.adapter.action.REQUEST_ENABLE"));
            return;
        }
        BleHandle.homeDeviceList.clear();
        this.instrumentAdapter.notifyDataSetChanged();
        if (ActivityCompat.checkSelfPermission(this, "android.permission.BLUETOOTH_CONNECT") != 0) {
            return;
        }
        for (BluetoothDevice bluetoothDevice : BluetoothAdapter.getDefaultAdapter().getBondedDevices()) {
            try {
                if (((Boolean) BluetoothDevice.class.getDeclaredMethod("isConnected", null).invoke(bluetoothDevice, null)).booleanValue()) {
                    BleHandle.directAddBleDevice(BleManager.getInstance().convertBleDevice(bluetoothDevice));
                    this.instrumentAdapter.notifyDataSetChanged();
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        BleManager.getInstance().scan(new BleScanCallback() { // from class: com.cubeSuite.MainActivity.1
            @Override // com.fastble.callback.BleScanPresenterImp
            public void onScanStarted(boolean z) {
                BleManager.getInstance().disconnectAllDevice();
                AlertDialogUtil.getInstance(MainActivity.activity).showDialog().setTitle(R.string.hint).setContent(R.string.scaning).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
            }

            @Override // com.fastble.callback.BleScanPresenterImp
            public void onScanning(BleDevice bleDevice) {
                BleHandle.addBleDevice(bleDevice);
                MainActivity.this.instrumentAdapter.notifyDataSetChanged();
            }

            @Override // com.fastble.callback.BleScanCallback
            public void onScanFinished(List<BleDevice> list) {
                AlertDialogUtil.getInstance(MainActivity.activity).closeDialog();
            }
        });
        new Thread(new Runnable() { // from class: com.cubeSuite.MainActivity$$ExternalSyntheticLambda3
            @Override // java.lang.Runnable
            public final void run() {
                MainActivity.lambda$startScan$1();
            }
        }).start();
    }

    static /* synthetic */ void lambda$startScan$1() {
        try {
            Thread.sleep(5000L);
            BleManager.getInstance().cancelScan();
        } catch (Exception unused) {
        }
    }

    @Override // android.app.Activity
    protected void onRestart() {
        super.onRestart();
        BleHandle.homeDeviceList.clear();
        this.instrumentAdapter.notifyDataSetChanged();
        if (ActivityCompat.checkSelfPermission(this, "android.permission.BLUETOOTH_CONNECT") != 0) {
            return;
        }
        for (BluetoothDevice bluetoothDevice : BluetoothAdapter.getDefaultAdapter().getBondedDevices()) {
            try {
                if (((Boolean) BluetoothDevice.class.getDeclaredMethod("isConnected", null).invoke(bluetoothDevice, null)).booleanValue()) {
                    BleHandle.directAddBleDevice(BleManager.getInstance().convertBleDevice(bluetoothDevice));
                    this.instrumentAdapter.notifyDataSetChanged();
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.home_list_activity);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.rootLayout), new OnApplyWindowInsetsListener() { // from class: com.cubeSuite.MainActivity$$ExternalSyntheticLambda0
            @Override // androidx.core.view.OnApplyWindowInsetsListener
            public final WindowInsetsCompat onApplyWindowInsets(View view, WindowInsetsCompat windowInsetsCompat) {
                return MainActivity.lambda$onCreate$2(view, windowInsetsCompat);
            }
        });
        this.downloadManager = (DownloadManager) getSystemService("download");
        ActivityStackUtil.getInstance().addActivity(this);
        activity = this;
        Context applicationContext = getApplicationContext();
        context = applicationContext;
        OTA_UPDATE_DIR = FileUtils.splicingFilePath(applicationContext, OtherEnum.SINCO_ROOT_FOLDER, OtherEnum.SINCO_OTA_UPDATE_FOLDER, null, null);
        EXPORT_WAV_FILE_PATH = FileUtils.splicingFilePath(context, OtherEnum.SINCO_ROOT_FOLDER, OtherEnum.SINCO_WAVE_FILE_FOLDER, null, null);
        SHARE_DRUM_DIR = FileUtils.splicingFilePath(context, OtherEnum.SINCO_ROOT_FOLDER, OtherEnum.SINCO_DRUM_DATA_DIR, null, null);
        OTA_UPDATE_PATH = OTA_UPDATE_DIR + "/OTAFile.ufw";
        this.mInstrumentRecyclerView = (RecyclerView) findViewById(R.id.home_recycler_view);
        APP_FILE_DIR = FileUtils.splicingFilePath(context, OtherEnum.SINCO_ROOT_FOLDER, OtherEnum.SINCO_APP_INSTALL_FOLDER, null, null);
        if (Build.VERSION.SDK_INT >= 33 || Build.VERSION.SDK_INT >= 31) {
            this.PERMISSIONS = new String[]{"android.permission.BLUETOOTH_CONNECT", "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_ADVERTISE", "android.permission.ACCESS_COARSE_LOCATION", "android.permission.ACCESS_FINE_LOCATION"};
        } else if (Build.VERSION.SDK_INT == 30) {
            this.PERMISSIONS = new String[]{"android.permission.ACCESS_COARSE_LOCATION", "android.permission.ACCESS_FINE_LOCATION", "android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.READ_EXTERNAL_STORAGE"};
        } else {
            this.PERMISSIONS = new String[]{"android.permission.ACCESS_COARSE_LOCATION", "android.permission.ACCESS_FINE_LOCATION", "android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.READ_EXTERNAL_STORAGE"};
        }
        ActivityCompat.requestPermissions(this, this.PERMISSIONS, 1001);
        BleManager.getInstance().init(getApplication());
        BleManager.getInstance().setSplitWriteNum(ItemTouchHelper.Callback.DEFAULT_DRAG_ANIMATION_DURATION).setConnectOverTime(10000L).setOperateTimeout(BleManager.DEFAULT_SCAN_TIME);
        BleManager.getInstance().initScanRule(new BleScanRuleConfig.Builder().setDeviceName(false, new String[0]).setScanTimeOut(0L).build());
        this.searchBleBtn = (Button) findViewById(R.id.search_ble_btn);
        this.instrumentAdapter = HomePageAdapter.getInstance();
        GridLayoutManager gridLayoutManager = new GridLayoutManager(activity, 2);
        this.gridLayoutManager = gridLayoutManager;
        gridLayoutManager.setOrientation(1);
        this.mInstrumentRecyclerView.setLayoutManager(this.gridLayoutManager);
        this.mInstrumentRecyclerView.setAdapter(this.instrumentAdapter);
        this.mInstrumentRecyclerView.setItemAnimator(new DefaultItemAnimator());
        this.searchBleBtn.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.MainActivity$$ExternalSyntheticLambda1
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m89lambda$onCreate$3$comcubeSuiteMainActivity(view);
            }
        });
    }

    static /* synthetic */ WindowInsetsCompat lambda$onCreate$2(View view, WindowInsetsCompat windowInsetsCompat) {
        view.setPadding(view.getPaddingLeft(), windowInsetsCompat.getInsets(WindowInsetsCompat.Type.statusBars()).f6top, view.getPaddingRight(), view.getPaddingBottom());
        return windowInsetsCompat;
    }

    /* JADX INFO: renamed from: lambda$onCreate$3$com-cubeSuite-MainActivity, reason: not valid java name */
    /* synthetic */ void m89lambda$onCreate$3$comcubeSuiteMainActivity(View view) {
        startScan();
    }
}
