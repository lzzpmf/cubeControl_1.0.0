package com.cubeSuite.adapter;

import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.MainActivity;
import com.cubeSuite.R;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.bluetooth.model.BleDeviceConfig;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.utils.NoRepetitionClickUtils;
import com.fastble.BleManager;

/* JADX INFO: loaded from: classes.dex */
public class HomePageAdapter extends RecyclerView.Adapter<InstrumentViewHolder> {
    public static HomePageAdapter homePageAdapter;
    private Intent intent;

    public static HomePageAdapter getInstance() {
        if (homePageAdapter == null) {
            homePageAdapter = new HomePageAdapter();
        }
        return homePageAdapter;
    }

    private HomePageAdapter() {
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public InstrumentViewHolder onCreateViewHolder(ViewGroup viewGroup, int i) {
        return new InstrumentViewHolder(LayoutInflater.from(viewGroup.getContext()).inflate(R.layout.home_list_item, viewGroup, false));
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(final InstrumentViewHolder instrumentViewHolder, int i) {
        final BleDeviceConfig bleDeviceConfig = BleHandle.homeDeviceList.get(i);
        instrumentViewHolder.setInfo(bleDeviceConfig.name, bleDeviceConfig.imageId);
        if (bleDeviceConfig.directConnected) {
            instrumentViewHolder.itemView.setBackgroundResource(R.drawable.bg_click_effect);
            instrumentViewHolder.instrumentView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.adapter.HomePageAdapter$$ExternalSyntheticLambda0
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m91lambda$onBindViewHolder$0$comcubeSuiteadapterHomePageAdapter(bleDeviceConfig, view);
                }
            });
        } else {
            instrumentViewHolder.itemView.setBackgroundResource(R.drawable.bg_click_effect_ban);
            instrumentViewHolder.instrumentView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.adapter.HomePageAdapter$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m92lambda$onBindViewHolder$1$comcubeSuiteadapterHomePageAdapter(instrumentViewHolder, bleDeviceConfig, view);
                }
            });
        }
    }

    /* JADX INFO: renamed from: lambda$onBindViewHolder$0$com-cubeSuite-adapter-HomePageAdapter, reason: not valid java name */
    /* synthetic */ void m91lambda$onBindViewHolder$0$comcubeSuiteadapterHomePageAdapter(BleDeviceConfig bleDeviceConfig, View view) {
        nextPage(bleDeviceConfig);
    }

    /* JADX INFO: renamed from: lambda$onBindViewHolder$1$com-cubeSuite-adapter-HomePageAdapter, reason: not valid java name */
    /* synthetic */ void m92lambda$onBindViewHolder$1$comcubeSuiteadapterHomePageAdapter(InstrumentViewHolder instrumentViewHolder, final BleDeviceConfig bleDeviceConfig, View view) {
        AlertDialogUtil.getInstance(MainActivity.activity).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.adapter.HomePageAdapter.2
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void cancelBtnClick() {
                BleManager.getInstance().disconnectAllDevice();
            }
        }).setTitle(R.string.hint).setCancelColor(R.drawable.btn_hollow_fillet_disable, R.color.disable).setConfirmBtnText(R.string.headFor).setCancelBtnText(R.string.forceConnect).setContent(MainActivity.activity.getString(R.string.pair1) + instrumentViewHolder.deviceName + MainActivity.activity.getString(R.string.pair2)).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.adapter.HomePageAdapter.1
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                MainActivity.activity.startActivity(new Intent("android.settings.BLUETOOTH_SETTINGS"));
            }

            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void cancelBtnClick() {
                HomePageAdapter.this.nextPage(bleDeviceConfig);
            }
        });
    }

    public void nextPage(BleDeviceConfig bleDeviceConfig) {
        if (NoRepetitionClickUtils.isRepetitionClick()) {
            return;
        }
        BleManager.getInstance().cancelScan();
        BleHandle.connectDeviceInfo.setBleDevice(bleDeviceConfig.getBleDevice());
        BleHandle.connectDeviceInfo.setFirmwareUrl(BleHandle.getFirmwareFullUrl(bleDeviceConfig.name));
        BleHandle.connectDeviceInfo.setPdfUrl(BleHandle.getPdfFullUrl(bleDeviceConfig.name));
        BleHandle.connectDeviceInfo.setFirmwareVersion(BleHandle.firmwareListMap.get(bleDeviceConfig.name));
        BleHandle.connectDeviceInfo.setPdfVersion(BleHandle.pdfListMap.get(bleDeviceConfig.name));
        if (this.intent == null) {
            this.intent = new Intent();
        }
        this.intent.setClass(MainActivity.activity, ControlAndPdfActivity.class);
        MainActivity.activity.startActivity(this.intent);
    }

    public static class InstrumentViewHolder extends RecyclerView.ViewHolder {
        private String deviceName;
        private int id;
        private ImageView image;
        View instrumentView;
        private TextView name;
        private TextView needPair;

        public String getDeviceName() {
            return this.deviceName;
        }

        public void setInfo(String str, int i) {
            this.deviceName = str;
            this.name.setText(str);
            this.image.setImageResource(i);
        }

        public InstrumentViewHolder(View view) {
            super(view);
            this.instrumentView = view;
            this.image = (ImageView) view.findViewById(R.id.effect_unit_img);
            this.name = (TextView) view.findViewById(R.id.effect_unit_text);
            this.needPair = (TextView) view.findViewById(R.id.needPair);
        }
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        return BleHandle.homeDeviceList.size();
    }
}
