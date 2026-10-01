package com.cubeSuite.fragment.midikeyboard;

import android.graphics.Color;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TextView;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.communication.MidikeyboardCommunication;
import com.cubeSuite.databinding.UsrFragmentBinding;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Sys;
import com.cubeSuite.entity.midikeyboard.Usr;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.utils.NumEditFilter;
import com.cubeSuite.utils.StringUtils;
import top.defaults.colorpicker.ColorPickerPopup;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardUsrFragment extends Fragment {
    private UsrFragmentBinding binding;
    private final MidikeyboardCommunication mc;
    private final Usr usr;
    private UsrBinding usrBinding = new UsrBinding();

    public MidikeyboardUsrFragment(Usr usr, MidikeyboardCommunication midikeyboardCommunication) {
        this.usr = usr;
        this.mc = midikeyboardCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        UsrFragmentBinding usrFragmentBinding = (UsrFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.usr_fragment, viewGroup, false);
        this.binding = usrFragmentBinding;
        usrFragmentBinding.setUsrBinding(this.usrBinding);
        this.binding.setListener(this);
        this.binding.setData(this.usr);
        NumEditFilter[] numEditFilterArr = {new NumEditFilter(0, 127)};
        this.binding.minVel.setFilters(numEditFilterArr);
        this.binding.maxVel.setFilters(numEditFilterArr);
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
        AddrU8[] color = this.usr.getPadPara()[0].getColor();
        this.binding.selectColorBtn.setBackgroundColor(Color.rgb(color[0].getData(), color[1].getData(), color[2].getData()));
        createKeybed(layoutInflater, viewGroup);
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
        this.binding.pads.setVisibility(8);
        this.binding.knobs.setVisibility(8);
        this.binding.keybed.setVisibility(8);
        this.binding.mps.setVisibility(8);
        this.binding.ppr.setVisibility(8);
        if (string.equals(getString(R.string.pad))) {
            this.binding.pads.setVisibility(0);
            return;
        }
        if (string.equals(getString(R.string.keybed))) {
            this.binding.keybed.setVisibility(0);
            return;
        }
        if (string.equals(getString(R.string.knob))) {
            this.binding.knobs.setVisibility(0);
        } else if (string.equals(getString(R.string.mps))) {
            this.binding.mps.setVisibility(0);
        } else if (string.equals(getString(R.string.ppr))) {
            this.binding.ppr.setVisibility(0);
        }
    }

    public void showColorDialog(final AddrU8[] addrU8Arr) {
        new ColorPickerPopup.Builder(getActivity()).initialColor(Color.rgb(addrU8Arr[0].getData(), addrU8Arr[1].getData(), addrU8Arr[2].getData())).enableAlpha(false).okTitle(getString(R.string.confirm)).cancelTitle(getString(R.string.close)).showIndicator(false).showValue(false).build().show(new ColorPickerPopup.ColorPickerObserver() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardUsrFragment.1
            @Override // top.defaults.colorpicker.ColorPickerPopup.ColorPickerObserver
            public void onColorPicked(int i) {
                int i2 = (16711680 & i) >> 16;
                int i3 = (65280 & i) >> 8;
                int i4 = i & 255;
                addrU8Arr[0].setData(i2);
                addrU8Arr[1].setData(i3);
                addrU8Arr[2].setData(i4);
                MidikeyboardUsrFragment.this.binding.selectColorBtn.setBackgroundColor(Color.rgb(i2, i3, i4));
                MidikeyboardUsrFragment.this.mc.sendColor(addrU8Arr);
            }
        });
    }

    public void setNum(CharSequence charSequence, AddrU8 addrU8, int i, int i2) {
        U8DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)), addrU8);
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendUsrU8(addrU8);
        }
    }

    public void I8DataChange(int i, AddrI8 addrI8) {
        if (addrI8.setData((byte) i)) {
            this.mc.sendUsrI8(addrI8);
        }
    }

    public void padChange(int i) {
        this.usrBinding.setPadIndex(i);
    }

    public void padTypeChange(int i) {
        U8DataChange(i, this.usr.getPadPara()[this.usrBinding.getPadIndex()].getType());
        this.binding.minValueLayout.setVisibility(0);
        this.binding.maxValueLayout.setVisibility(0);
        this.binding.channelLayout.setVisibility(0);
        this.binding.padNoteStr.setVisibility(0);
        if (i == 0) {
            this.usrBinding.setPadNoteText(getString(R.string.note));
            this.usrBinding.setPadMinText(getString(R.string.minVel));
            this.usrBinding.setPadMaxText(getString(R.string.maxVel));
            this.usrBinding.setPadNoteStr(Global.BluetoothPedalCustomCommand.OCTAVE);
            return;
        }
        if (i == 1) {
            this.usrBinding.setPadNoteText(getString(R.string.cc));
            this.usrBinding.setPadMinText(getString(R.string.value1));
            this.usrBinding.setPadMaxText(getString(R.string.value2));
            this.usrBinding.setPadNoteStr(Global.BluetoothPedalCustomCommand.MIDI_STR);
            return;
        }
        if (i == 2) {
            this.usrBinding.setPadNoteText(getString(R.string.cc));
            this.usrBinding.setPadMinText(getString(R.string.upStr));
            this.usrBinding.setPadMaxText(getString(R.string.downStr));
            this.usrBinding.setPadNoteStr(Global.BluetoothPedalCustomCommand.MIDI_STR);
            return;
        }
        if (i == 3) {
            this.usrBinding.setPadNoteText(getString(R.string.lsb));
            this.usrBinding.setPadMinText(getString(R.string.msb));
            this.usrBinding.setPadMaxText(getString(R.string.program));
            this.binding.padNoteStr.setVisibility(8);
            return;
        }
        if (i != 4) {
            return;
        }
        this.binding.minValueLayout.setVisibility(8);
        this.binding.maxValueLayout.setVisibility(8);
        this.binding.channelLayout.setVisibility(8);
        this.usrBinding.setPadNoteStr(Global.BluetoothPedalCustomCommand.OCTAVE);
    }

    int[] bitToData(int i) {
        return new int[]{i >> 7, i & 127};
    }

    public void setMackie(CharSequence charSequence, AddrU8 addrU8, int i, int i2) {
        U8DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)) | (addrU8.getData() & 128), addrU8);
    }

    public int getMackie(AddrU8 addrU8) {
        return bitToData(addrU8.getData())[1];
    }

    public boolean getMackieOnOff(AddrU8 addrU8) {
        return bitToData(addrU8.getData())[0] >= 1;
    }

    public void setMackieOnOff(boolean z, AddrU8 addrU8) {
        U8DataChange((z ? 128 : 0) | (addrU8.getData() & 127), addrU8);
    }

    public void createKeybed(LayoutInflater layoutInflater, ViewGroup viewGroup) {
        final int i = 0;
        while (i < 25) {
            View viewInflate = LayoutInflater.from(getActivity()).inflate(R.layout.keybed_key_layout, (ViewGroup) null);
            this.binding.keybed.addView(viewInflate);
            int i2 = i + 1;
            ((TextView) viewInflate.findViewById(R.id.index)).setText(i2 + "");
            EditText editText = (EditText) viewInflate.findViewById(R.id.mpcValue);
            editText.setText(String.valueOf(getMackie(this.usr.getKeyPara().getMackie()[i])));
            final TextView textView = (TextView) viewInflate.findViewById(R.id.mpcText);
            textView.setText(this.usrBinding.getKeyBedMcpStr()[getMackie(this.usr.getKeyPara().getMackie()[i])]);
            editText.setFilters(new NumEditFilter[]{new NumEditFilter(0, 127)});
            editText.addTextChangedListener(new TextWatcher() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardUsrFragment.2
                @Override // android.text.TextWatcher
                public void beforeTextChanged(CharSequence charSequence, int i3, int i4, int i5) {
                }

                @Override // android.text.TextWatcher
                public void onTextChanged(CharSequence charSequence, int i3, int i4, int i5) {
                }

                @Override // android.text.TextWatcher
                public void afterTextChanged(Editable editable) {
                    MidikeyboardUsrFragment midikeyboardUsrFragment = MidikeyboardUsrFragment.this;
                    midikeyboardUsrFragment.setMackie(editable, midikeyboardUsrFragment.usr.getKeyPara().getMackie()[i], 0, 127);
                    textView.setText(MidikeyboardUsrFragment.this.usrBinding.getKeyBedMcpStr()[MidikeyboardUsrFragment.this.usr.getKeyPara().getMackie()[i].getData()]);
                }
            });
            SwitchCompat switchCompat = (SwitchCompat) viewInflate.findViewById(R.id.mpcSwitch);
            switchCompat.setChecked(getMackieOnOff(this.usr.getKeyPara().getMackie()[i]));
            switchCompat.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardUsrFragment$$ExternalSyntheticLambda0
                @Override // android.widget.CompoundButton.OnCheckedChangeListener
                public final void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                    this.f$0.m268x70091ac1(i, compoundButton, z);
                }
            });
            i = i2;
        }
    }

    /* JADX INFO: renamed from: lambda$createKeybed$0$com-cubeSuite-fragment-midikeyboard-MidikeyboardUsrFragment, reason: not valid java name */
    /* synthetic */ void m268x70091ac1(int i, CompoundButton compoundButton, boolean z) {
        setMackieOnOff(z, this.usr.getKeyPara().getMackie()[i]);
    }

    public void knobChange(int i) {
        this.usrBinding.setKnobIndex(i);
    }

    public void knobTypeChange(int i) {
        U8DataChange(i, this.usr.getEncoderPara()[this.usrBinding.getPadIndex()].getType());
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

    public void psrModeChange(int i) {
        U8DataChange(i, this.usr.getBtnPara()[this.usrBinding.getPsrIndex()].getType());
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
