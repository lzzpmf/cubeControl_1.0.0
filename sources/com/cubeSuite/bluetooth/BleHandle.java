package com.cubeSuite.bluetooth;

import com.cubeSuite.R;
import com.cubeSuite.bluetooth.model.BleDeviceConfig;
import com.cubeSuite.communication.Communication;
import com.cubeSuite.config.WebConfig;
import com.cubeSuite.entity.other.DeviceInfo;
import com.cubeSuite.enumeration.DrumType;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.fragment.CubeTurnerControlFragment;
import com.cubeSuite.fragment.EmptyFragment;
import com.cubeSuite.fragment.FootControl2.FC2Fragment;
import com.cubeSuite.fragment.FootCtrlControlFragment;
import com.cubeSuite.fragment.LooperMiniControlFragment;
import com.cubeSuite.fragment.LooperProControlFragment;
import com.cubeSuite.fragment.SmcMixerFragment;
import com.cubeSuite.fragment.irBox.IRBoxControlFragment;
import com.cubeSuite.fragment.looperDrum.LooperDrumFragment;
import com.cubeSuite.fragment.midiPort.MidiPortFragment;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardFragment;
import com.cubeSuite.fragment.smcPad.SmcPadFragment;
import com.cubeSuite.fragment.smk25II.Smk25IIFragment;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniFragment;
import com.fastble.data.BleDevice;
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class BleHandle {
    public static String[] supportDeviceNames;
    public static List<BleDeviceConfig> homeDeviceList = new ArrayList();
    public static DeviceInfo connectDeviceInfo = new DeviceInfo();
    public static Map<String, Integer> firmwareListMap = new HashMap();
    public static Map<String, Integer> pdfListMap = new HashMap();
    public static final BleDeviceConfig[] BLE_DEVICE_CONFIGS = {new BleDeviceConfig("FootCtrl", false, R.drawable.ble_pad_vertical), new BleDeviceConfig("CubeTurner", false, R.drawable.cube_turner), new BleDeviceConfig("LooperPro", true, R.drawable.looper_pro), new BleDeviceConfig("LooperMini", true, R.drawable.looper_mini), new BleDeviceConfig("LPCBA", true, R.drawable.looper_mini), new BleDeviceConfig("LooperAuto", true, R.drawable.looper_auto), new BleDeviceConfig("MidiPortA", true, R.drawable.midi_port_a), new BleDeviceConfig("MidiPortB", true, R.drawable.midi_port_b), new BleDeviceConfig("LooperDrum", true, R.drawable.looper_drum), new BleDeviceConfig("LooperDrumX", false, R.drawable.looper_drum), new BleDeviceConfig("LooperDrumX2", false, R.drawable.looper_drum), new BleDeviceConfig("LooperPartner", true, R.drawable.looper_drum), new BleDeviceConfig("DrumBuddy", true, R.drawable.looper_drum), new BleDeviceConfig("DrumBuddyV2", true, R.drawable.looper_drum), new BleDeviceConfig("LooperDrumV2", true, R.drawable.looper_drum), new BleDeviceConfig("SMK25", true, R.drawable.midikeyboard), new BleDeviceConfig("VMK25", true, R.drawable.midikeyboard), new BleDeviceConfig("SMK25v2", true, R.drawable.midikeyboard), new BleDeviceConfig("LooperProX", true, R.drawable.looper_pro), new BleDeviceConfig("LooperAutoX", true, R.drawable.looper_auto), new BleDeviceConfig("IR-BOX", true, R.drawable.ir_box), new BleDeviceConfig("SMK25Mini", true, R.drawable.smk25mini), new BleDeviceConfig("MiniKey25", true, R.drawable.smk25mini), new BleDeviceConfig("SMK25II", true, R.drawable.smk25ii), new BleDeviceConfig("VMK25Pro", true, R.drawable.smk25ii), new BleDeviceConfig("OxyGen-25", true, R.drawable.midikeyboard), new BleDeviceConfig("SMC-Mixer", true, R.drawable.smc_mixer), new BleDeviceConfig("VMX8", true, R.drawable.smc_mixer), new BleDeviceConfig("SMC-PAD", true, R.drawable.smc_pad), new BleDeviceConfig("VMP16", true, R.drawable.smc_pad), new BleDeviceConfig("WPTP-2", false, R.drawable.wptp_2), new BleDeviceConfig("RJMICRO25", true, R.drawable.smk25mini), new BleDeviceConfig("TurnerPro", true, R.drawable.turner_pro), new BleDeviceConfig("FootCtrlPlus", false, R.drawable.foot_ctrl_plus), new BleDeviceConfig("SMARTKEY25", true, R.drawable.midikeyboard), new BleDeviceConfig("IMPULSE-R", true, R.drawable.ir_box), new BleDeviceConfig("SMARTCON", true, R.drawable.smc_mixer), new BleDeviceConfig("SMARTPAD", true, R.drawable.smc_pad), new BleDeviceConfig("SZ-STUDIOPAD16", true, R.drawable.smc_pad), new BleDeviceConfig("SZ-STUDIOKEY25", true, R.drawable.smk25ii), new BleDeviceConfig("SZ-STUDIOKEY25-MINI", true, R.drawable.smk25mini), new BleDeviceConfig("SZ-STUDIOKEY25-M", true, R.drawable.smk25mini), new BleDeviceConfig("SZ-STUDIOFADE8", true, R.drawable.smc_mixer), new BleDeviceConfig("DLP-1", true, R.drawable.looper_drum), new BleDeviceConfig("STARRYCTRL", true, R.drawable.smc_mixer), new BleDeviceConfig("GAM-10", false, R.drawable.foot_ctrl_plus), new BleDeviceConfig("AIRMINI", false, R.drawable.airmini), new BleDeviceConfig("STARRYPAD PRO", true, R.drawable.smc_pad)};

    public static void directAddBleDevice(BleDevice bleDevice) {
        for (BleDeviceConfig bleDeviceConfig : BLE_DEVICE_CONFIGS) {
            if (bleDeviceConfig.name.equalsIgnoreCase(bleDevice.getName())) {
                BleDeviceConfig bleDeviceConfigCopy = bleDeviceConfig.copy(bleDevice);
                bleDeviceConfigCopy.directConnected = true;
                if (isAlreadyAddDevice(bleDevice)) {
                    return;
                }
                homeDeviceList.add(bleDeviceConfigCopy);
                return;
            }
        }
    }

    public static void addBleDevice(BleDevice bleDevice) {
        for (BleDeviceConfig bleDeviceConfig : BLE_DEVICE_CONFIGS) {
            if (bleDeviceConfig.name.equalsIgnoreCase(bleDevice.getName())) {
                BleDeviceConfig bleDeviceConfigCopy = bleDeviceConfig.copy(bleDevice);
                if (isAlreadyAddDevice(bleDevice)) {
                    return;
                }
                homeDeviceList.add(bleDeviceConfigCopy);
                return;
            }
        }
    }

    public static boolean isAlreadyAddDevice(BleDevice bleDevice) {
        Iterator<BleDeviceConfig> it = homeDeviceList.iterator();
        while (it.hasNext()) {
            if (it.next().getBleDevice().getDevice().getAddress().equals(bleDevice.getDevice().getAddress())) {
                return true;
            }
        }
        return false;
    }

    public static ControlBaseFragment getFragmentByName(String str) {
        for (BleDeviceConfig bleDeviceConfig : BLE_DEVICE_CONFIGS) {
            if (bleDeviceConfig.name.equalsIgnoreCase(str)) {
                return getFragment(bleDeviceConfig.name);
            }
        }
        return new EmptyFragment();
    }

    public static String[] getSupportDeviceNames() {
        if (supportDeviceNames == null) {
            supportDeviceNames = new String[BLE_DEVICE_CONFIGS.length];
            int i = 0;
            while (true) {
                BleDeviceConfig[] bleDeviceConfigArr = BLE_DEVICE_CONFIGS;
                if (i >= bleDeviceConfigArr.length) {
                    break;
                }
                supportDeviceNames[i] = bleDeviceConfigArr[i].name;
                i++;
            }
        }
        return supportDeviceNames;
    }

    /* JADX WARN: Failed to restore switch over string. Please report as a decompilation issue */
    public static ControlBaseFragment getFragment(String str) {
        String upperCase = str.toUpperCase();
        upperCase.hashCode();
        byte b = -1;
        switch (upperCase.hashCode()) {
            case -2127903993:
                if (upperCase.equals("IR-BOX")) {
                    b = 0;
                }
                break;
            case -1727768710:
                if (upperCase.equals("WPTP-2")) {
                    b = 1;
                }
                break;
            case -1627119517:
                if (upperCase.equals("TURNERPRO")) {
                    b = 2;
                }
                break;
            case -1614644008:
                if (upperCase.equals("LOOPERAUTOX")) {
                    b = 3;
                }
                break;
            case -1611961919:
                if (upperCase.equals("LOOPERDRUMX")) {
                    b = 4;
                }
                break;
            case -1550690982:
                if (upperCase.equals("IMPULSE-R")) {
                    b = 5;
                }
                break;
            case -1378865713:
                if (upperCase.equals("SMC-PAD")) {
                    b = 6;
                }
                break;
            case -1371354284:
                if (upperCase.equals("SMK25II")) {
                    b = 7;
                }
                break;
            case -1371353904:
                if (upperCase.equals("SMK25V2")) {
                    b = 8;
                }
                break;
            case -1336333001:
                if (upperCase.equals("LOOPERPARTNER")) {
                    b = 9;
                }
                break;
            case -1310123847:
                if (upperCase.equals("SMARTKEY25")) {
                    b = 10;
                }
                break;
            case -1121795580:
                if (upperCase.equals("STARRYCTRL")) {
                    b = 11;
                }
                break;
            case -1074894144:
                if (upperCase.equals("DRUMBUDDY")) {
                    b = 12;
                }
                break;
            case -971951777:
                if (upperCase.equals("MIDIPORTA")) {
                    b = 13;
                }
                break;
            case -971951776:
                if (upperCase.equals("MIDIPORTB")) {
                    b = 14;
                }
                break;
            case -935831969:
                if (upperCase.equals("CUBETURNER")) {
                    b = 15;
                }
                break;
            case -691759413:
                if (upperCase.equals("MINIKEY25")) {
                    b = 16;
                }
                break;
            case -338595194:
                if (upperCase.equals("SZ-STUDIOKEY25-M")) {
                    b = Communication.CMD_QUERY_NAME_VER;
                }
                break;
            case -290800977:
                if (upperCase.equals("RJMICRO25")) {
                    b = 18;
                }
                break;
            case -273779583:
                if (upperCase.equals("AIRMINI")) {
                    b = 19;
                }
                break;
            case -229598564:
                if (upperCase.equals("LOOPERPRO")) {
                    b = 20;
                }
                break;
            case 2638807:
                if (upperCase.equals("VMX8")) {
                    b = 21;
                }
                break;
            case 65141868:
                if (upperCase.equals("DLP-1")) {
                    b = 22;
                }
                break;
            case 72637374:
                if (upperCase.equals("LPCBA")) {
                    b = 23;
                }
                break;
            case 79019828:
                if (upperCase.equals("SMK25")) {
                    b = 24;
                }
                break;
            case 81790391:
                if (upperCase.equals("VMK25")) {
                    b = 25;
                }
                break;
            case 81795166:
                if (upperCase.equals("VMP16")) {
                    b = 26;
                }
                break;
            case 181869337:
                if (upperCase.equals("SMARTCON")) {
                    b = 27;
                }
                break;
            case 181881386:
                if (upperCase.equals("SMARTPAD")) {
                    b = 28;
                }
                break;
            case 683614603:
                if (upperCase.equals("SMK25MINI")) {
                    b = 29;
                }
                break;
            case 838912243:
                if (upperCase.equals("FOOTCTRLPLUS")) {
                    b = 30;
                }
                break;
            case 1177343961:
                if (upperCase.equals("FOOTCTRL")) {
                    b = 31;
                }
                break;
            case 1349079744:
                if (upperCase.equals("SZ-STUDIOFADE8")) {
                    b = 32;
                }
                break;
            case 1353836102:
                if (upperCase.equals("SZ-STUDIOKEY25")) {
                    b = 33;
                }
                break;
            case 1358314332:
                if (upperCase.equals("SZ-STUDIOPAD16")) {
                    b = 34;
                }
                break;
            case 1371160950:
                if (upperCase.equals("VMK25PRO")) {
                    b = Communication.CMD_READ;
                }
                break;
            case 1470682512:
                if (upperCase.equals("OXYGEN-25")) {
                    b = 36;
                }
                break;
            case 1471935360:
                if (upperCase.equals("LOOPERAUTO")) {
                    b = 37;
                }
                break;
            case 1472021879:
                if (upperCase.equals("LOOPERDRUM")) {
                    b = 38;
                }
                break;
            case 1472281128:
                if (upperCase.equals("LOOPERMINI")) {
                    b = 39;
                }
                break;
            case 1472379196:
                if (upperCase.equals("LOOPERPROX")) {
                    b = 40;
                }
                break;
            case 1568788051:
                if (upperCase.equals("LOOPERDRUMV2")) {
                    b = 41;
                }
                break;
            case 1568788113:
                if (upperCase.equals("LOOPERDRUMX2")) {
                    b = 42;
                }
                break;
            case 1788826494:
                if (upperCase.equals("SZ-STUDIOKEY25-MINI")) {
                    b = 43;
                }
                break;
            case 2052433477:
                if (upperCase.equals("SMC-MIXER")) {
                    b = 44;
                }
                break;
            case 2095037305:
                if (upperCase.equals("GAM-10")) {
                    b = 45;
                }
                break;
            case 2113848668:
                if (upperCase.equals("DRUMBUDDYV2")) {
                    b = 46;
                }
                break;
            case 2117905191:
                if (upperCase.equals("STARRYPAD PRO")) {
                    b = 47;
                }
                break;
        }
        switch (b) {
            case 0:
            case 5:
                return new IRBoxControlFragment();
            case 1:
                return new CubeTurnerControlFragment(R.drawable.wptp_2);
            case 2:
            case 13:
                return new EmptyFragment();
            case 3:
            case 20:
            case 37:
            case 40:
                return new LooperProControlFragment();
            case 4:
            case 9:
            case 22:
            case 38:
            case 41:
            case 42:
                return new LooperDrumFragment(DrumType.sinco);
            case 6:
            case 26:
            case 28:
            case 34:
            case 47:
                return new SmcPadFragment();
            case 7:
            case 33:
            case 35:
                return new Smk25IIFragment();
            case 8:
            case 10:
            case 24:
            case 25:
            case 36:
                return new MidikeyboardFragment();
            case 11:
            case 21:
            case 27:
            case 32:
            case 44:
                return new SmcMixerFragment();
            case 12:
            case 46:
                return new LooperDrumFragment(DrumType.drumBuddy);
            case 14:
                return new MidiPortFragment();
            case 15:
                return new CubeTurnerControlFragment(R.drawable.cube_turner);
            case 16:
            case 18:
            case 29:
            case 43:
                return new Smk25MiniFragment();
            case 17:
                return new Smk25MiniFragment();
            case 19:
            case 30:
            case 45:
                return new FC2Fragment();
            case 23:
            case 39:
                return new LooperMiniControlFragment();
            case 31:
                return new FootCtrlControlFragment();
            default:
                return new EmptyFragment();
        }
    }

    public static String getFirmwareFullUrl(String str) {
        int iIntValue = firmwareListMap.get(str.toUpperCase()) == null ? 0 : firmwareListMap.get(str.toUpperCase()).intValue();
        if (iIntValue == 0) {
            return "";
        }
        return WebConfig.FIRMWARE_FILE_URL_LIST + str + "_" + new DecimalFormat("000").format(Integer.valueOf(iIntValue)) + ".ufw";
    }

    public static String getPdfFullUrl(String str) {
        return "";
    }
}
