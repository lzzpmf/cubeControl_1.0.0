package com.cubeSuite.entity.other;

import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.fastble.data.BleDevice;

/* JADX INFO: loaded from: classes.dex */
public class DeviceInfo {
    private BleDevice bleDevice;
    private ControlBaseFragment controlFragment;
    private String firmwareUrl;
    private int firmwareVersion;
    private String name;
    private String pdfUrl;
    private int pdfVersion;

    public void clear() {
        this.name = "";
        this.pdfVersion = 0;
        this.pdfUrl = "";
        this.firmwareVersion = 0;
        this.firmwareUrl = "";
        this.bleDevice = null;
    }

    public ControlBaseFragment getControlFragment() {
        return this.controlFragment;
    }

    public int getPdfVersion() {
        return this.pdfVersion;
    }

    public void setPdfVersion(Integer num) {
        this.pdfVersion = num == null ? 0 : num.intValue();
    }

    public String getPdfUrl() {
        return this.pdfUrl;
    }

    public void setPdfUrl(String str) {
        this.pdfUrl = str;
    }

    public String getFirmwareUrl() {
        return this.firmwareUrl;
    }

    public void setFirmwareUrl(String str) {
        this.firmwareUrl = str;
    }

    public int getFirmwareVersion() {
        return this.firmwareVersion;
    }

    public void setFirmwareVersion(Integer num) {
        this.firmwareVersion = num == null ? 0 : num.intValue();
    }

    public String getName() {
        return this.name;
    }

    public BleDevice getBleDevice() {
        return this.bleDevice;
    }

    public void setBleDevice(BleDevice bleDevice) {
        this.bleDevice = bleDevice;
        this.name = bleDevice.getName();
        setName(bleDevice.getName());
        this.controlFragment = BleHandle.getFragmentByName(this.name);
    }

    public void setName(String str) {
        this.name = str;
        this.controlFragment = BleHandle.getFragmentByName(str);
    }
}
