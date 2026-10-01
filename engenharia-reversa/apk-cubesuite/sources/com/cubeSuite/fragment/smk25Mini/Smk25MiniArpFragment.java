package com.cubeSuite.fragment.smk25Mini;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.communication.Smk25MiniCommunication;
import com.cubeSuite.databinding.Smk25miniArpFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Arp;
import com.cubeSuite.utils.NumEditFilter;
import com.cubeSuite.utils.StringUtils;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniArpFragment extends Fragment {
    private final Arp arp;
    private final Smk25MiniCommunication mc;

    public Smk25MiniArpFragment(Arp arp, Smk25MiniCommunication smk25MiniCommunication) {
        this.arp = arp;
        this.mc = smk25MiniCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Smk25miniArpFragmentBinding smk25miniArpFragmentBinding = (Smk25miniArpFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25mini_arp_fragment, viewGroup, false);
        smk25miniArpFragmentBinding.setData(this.arp);
        smk25miniArpFragmentBinding.setListener(this);
        smk25miniArpFragmentBinding.gate.setFilters(new NumEditFilter[]{new NumEditFilter(0, 100)});
        smk25miniArpFragmentBinding.swing.setFilters(new NumEditFilter[]{new NumEditFilter(0, 75)});
        smk25miniArpFragmentBinding.tempo.setFilters(new NumEditFilter[]{new NumEditFilter(0, 240)});
        return smk25miniArpFragmentBinding.getRoot();
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }

    public void U16DataChange(int i, AddrU16 addrU16) {
        if (addrU16.setData(i)) {
            this.mc.sendGlobU16(addrU16);
        }
    }

    public void setU16Num(CharSequence charSequence, AddrU16 addrU16, int i, int i2) {
        U16DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)), addrU16);
    }

    public void setNum(CharSequence charSequence, AddrU8 addrU8, int i, int i2) {
        U8DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)), addrU8);
    }
}
