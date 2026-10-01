package com.cubeSuite.fragment.smk25Mini;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.communication.Smk25MiniCommunication;
import com.cubeSuite.databinding.Smk25miniUsrFragmentBinding;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Sys;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.entity.smk25Mini.Smk25MiniUsr;
import com.cubeSuite.utils.NumEditFilter;
import com.cubeSuite.utils.StringUtils;
import kotlin.jvm.internal.ByteCompanionObject;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniUsrFragment extends Fragment {
    private Smk25miniUsrFragmentBinding binding;
    private final Smk25MiniGlob glob;
    private final Smk25MiniCommunication mc;
    private final Smk25MiniUsr usr;
    private UsrBinding usrBinding = new UsrBinding();

    public Smk25MiniUsrFragment(Smk25MiniUsr smk25MiniUsr, Smk25MiniGlob smk25MiniGlob, Smk25MiniCommunication smk25MiniCommunication) {
        this.usr = smk25MiniUsr;
        this.glob = smk25MiniGlob;
        this.mc = smk25MiniCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Smk25miniUsrFragmentBinding smk25miniUsrFragmentBinding = (Smk25miniUsrFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25mini_usr_fragment, viewGroup, false);
        this.binding = smk25miniUsrFragmentBinding;
        smk25miniUsrFragmentBinding.setUsrBinding(this.usrBinding);
        this.binding.setListener(this);
        this.binding.setData(this.usr);
        this.binding.setGlobData(this.glob);
        NumEditFilter[] numEditFilterArr = {new NumEditFilter(0, 127)};
        this.binding.keybedMinVel.setFilters(numEditFilterArr);
        this.binding.keybedMaxVel.setFilters(numEditFilterArr);
        this.binding.knobMinVal.setFilters(numEditFilterArr);
        this.binding.knobMaxVal.setFilters(numEditFilterArr);
        this.binding.keybedMaxVel.setFilters(numEditFilterArr);
        this.binding.modCCVal.setFilters(numEditFilterArr);
        this.binding.modMinVal.setFilters(numEditFilterArr);
        this.binding.modMaxVal.setFilters(numEditFilterArr);
        this.binding.sustainCCVal.setFilters(numEditFilterArr);
        this.binding.sustainUpVal.setFilters(numEditFilterArr);
        this.binding.sustainDownVal.setFilters(numEditFilterArr);
        return this.binding.getRoot();
    }

    public void itemSelectChange(RadioGroup radioGroup) {
        String string = "";
        for (int i = 0; i < radioGroup.getChildCount(); i++) {
            RadioButton radioButton = (RadioButton) radioGroup.getChildAt(i);
            if (radioButton.isChecked()) {
                string = radioButton.getText().toString();
            }
        }
        this.binding.knobs.setVisibility(8);
        this.binding.keybed.setVisibility(8);
        this.binding.mps.setVisibility(8);
        this.binding.ppr.setVisibility(8);
        if (string.equals(getString(R.string.keybed))) {
            this.binding.keybed.setVisibility(0);
            return;
        }
        if (string.equals(getString(R.string.knob))) {
            this.binding.knobs.setVisibility(0);
        } else if (string.equals(getString(R.string.utility))) {
            this.binding.mps.setVisibility(0);
        } else if (string.equals(getString(R.string.sus))) {
            this.binding.ppr.setVisibility(0);
        }
    }

    public void setNum(CharSequence charSequence, AddrU8 addrU8, int i, int i2) {
        U8DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)), addrU8);
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendUsrU8(addrU8);
        }
    }

    public void GlobU8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }

    public void I8DataChange(int i, AddrI8 addrI8) {
        if (addrI8.setData((byte) i)) {
            this.mc.sendUsrI8(addrI8);
        }
    }

    public void knobChange(int i) {
        this.usrBinding.setKnobIndex(i);
    }

    public void knobTypeChange(int i) {
        U8DataChange(i, this.usr.getEncoderPara().getType());
        this.usrBinding.setKnobMinText(getString(R.string.min));
        this.usrBinding.setKnobMaxText(getString(R.string.max));
        if (i == 0) {
            this.binding.knobCCLayout.setVisibility(0);
            return;
        }
        if (i == 1 || i == 2) {
            this.binding.knobCCLayout.setVisibility(8);
        } else {
            if (i != 3) {
                return;
            }
            this.binding.knobCCLayout.setVisibility(0);
            this.usrBinding.setKnobMinText(getString(R.string.leftStr));
            this.usrBinding.setKnobMaxText(getString(R.string.rightStr));
        }
    }

    public void psrChange(int i) {
        this.usrBinding.setPsrIndex(i);
    }

    public void setSysEx(CharSequence charSequence, Sys sys) {
        byte[] bArrX16StringToByte = x16StringToByte(charSequence.toString());
        if (sys.getLen().getData() == bArrX16StringToByte.length) {
            for (int i = 0; i < bArrX16StringToByte.length; i++) {
                if (((byte) sys.getVal()[i].getData()) == bArrX16StringToByte[i]) {
                }
            }
            return;
        }
        sys.getLen().setData(bArrX16StringToByte.length);
        for (int i2 = 0; i2 < bArrX16StringToByte.length; i2++) {
            sys.getVal()[i2].setData(bArrX16StringToByte[i2]);
        }
        this.mc.sendSysEx(sys);
    }

    public String getSysEx(Sys sys) {
        return byteToX16String(sys.getVal(), sys.getLen().getData());
    }

    private String byteToX16String(AddrU8[] addrU8Arr, int i) {
        int length = addrU8Arr.length;
        int[] iArr = new int[length];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = addrU8Arr[i2].getData();
        }
        StringBuilder sb = new StringBuilder();
        for (int i3 = 0; i3 < i; i3++) {
            int i4 = iArr[i3];
            if ((i4 & 255) < 16) {
                sb.append("0").append(Integer.toHexString(iArr[i3] & 255)).append(" ");
            } else {
                sb.append(Integer.toHexString(i4 & 255)).append(" ");
            }
        }
        return sb.toString().toUpperCase();
    }

    private byte[] x16StringToByte(String str) {
        int i;
        boolean z;
        boolean z2;
        char[] cArr = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        String strReplace = str.toUpperCase().replace("\\s", "").replace("\\n", "");
        byte[] bArr = new byte[64];
        int i2 = 0;
        boolean z3 = false;
        int i3 = 0;
        for (char c : strReplace.substring(0, Math.min(128, strReplace.length())).toCharArray()) {
            byte b = 0;
            while (true) {
                if (b >= 16) {
                    i = i3;
                    z = z3;
                    z2 = false;
                    break;
                }
                if (cArr[b] == c) {
                    if (z3) {
                        bArr[i2] = (byte) (b + i3);
                        i2 = (byte) (i2 + 1);
                        z2 = false;
                    } else {
                        i3 = b * 16;
                        z2 = true;
                    }
                    i = i3;
                    z = z2;
                    break;
                }
                b = (byte) (b + 1);
            }
            if (z2 || !z) {
                z3 = z;
                i3 = i;
            } else {
                z3 = false;
                i3 = 0;
            }
        }
        byte[] bArr2 = new byte[i2];
        System.arraycopy(bArr, 0, bArr2, 0, i2);
        return bArr2;
    }

    public void pedalPortChange(int i, AddrU8 addrU8) {
        GlobU8DataChange(i, addrU8);
        int i2 = i == 0 ? 0 : 8;
        this.binding.pedalType.setVisibility(i2);
        this.binding.pedalCC.setVisibility(i2);
        this.binding.pedalUp.setVisibility(i2);
        this.binding.pedalDown.setVisibility(i2);
        this.binding.pedalCh.setVisibility(i2);
    }

    public void setSustainPedalMode() {
        this.usr.getPedalPara().getType().setData(0);
        this.usr.getPedalPara().getCh().setData(0);
        this.usr.getPedalPara().getDat()[0].setData(64);
        this.usr.getPedalPara().getDat()[1].setData(0);
        this.usr.getPedalPara().getDat()[2].setData(127);
        this.mc.sendDataToDevice((byte) 5, this.usr.getPedalPara().getType().getAddr(), new byte[]{0, 0, 64, 0, ByteCompanionObject.MAX_VALUE});
    }

    public void psrModeChange(int i) {
        U8DataChange(i, this.usr.getBtnPara().getType());
        this.binding.psrChannelLayout.setVisibility(0);
        this.binding.psrCCLayout.setVisibility(0);
        this.binding.psrValue1.setVisibility(0);
        this.binding.psrValue2.setVisibility(0);
        this.binding.psrValue1Layout.setVisibility(0);
        this.binding.psrValue2Layout.setVisibility(0);
        this.binding.sysExLayout.setVisibility(8);
        this.usrBinding.setPsrCCText(getString(R.string.cc));
        this.usrBinding.setPsrValue1Text(getString(R.string.value1));
        this.usrBinding.setPsrValue2Text(getString(R.string.value2));
        this.usrBinding.setPsrCCStr(Global.BluetoothPedalCustomCommand.MIDI_STR);
        if (i == 0) {
            this.binding.psrValue2Layout.setVisibility(8);
            return;
        }
        if (i == 3) {
            this.usrBinding.setPsrCCText(getString(R.string.lsb));
            this.usrBinding.setPsrValue1Text(getString(R.string.msb));
            this.usrBinding.setPsrValue2Text(getString(R.string.program));
        } else {
            if (i == 4) {
                this.binding.sysExLayout.setVisibility(0);
                this.binding.psrChannelLayout.setVisibility(8);
                this.binding.psrCCLayout.setVisibility(8);
                this.binding.psrValue1Layout.setVisibility(8);
                this.binding.psrValue2Layout.setVisibility(8);
                return;
            }
            if (i != 5) {
                return;
            }
            this.binding.psrChannelLayout.setVisibility(0);
            this.binding.psrValue1Layout.setVisibility(8);
            this.binding.psrValue2Layout.setVisibility(8);
            this.usrBinding.setPsrCCText(getString(R.string.note));
            this.usrBinding.setPsrCCStr(Global.BluetoothPedalCustomCommand.OCTAVE);
        }
    }
}
