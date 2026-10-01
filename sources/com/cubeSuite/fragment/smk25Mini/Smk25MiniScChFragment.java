package com.cubeSuite.fragment.smk25Mini;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.communication.Smk25MiniCommunication;
import com.cubeSuite.databinding.Smk25miniScChFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Shift;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniScChFragment extends Fragment {
    Smk25miniScChFragmentBinding binding;
    private final Smk25MiniCommunication mc;
    private final Shift shift;

    public Smk25MiniScChFragment(Shift shift, Smk25MiniCommunication smk25MiniCommunication) {
        this.shift = shift;
        this.mc = smk25MiniCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Smk25miniScChFragmentBinding smk25miniScChFragmentBinding = (Smk25miniScChFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25mini_sc_ch_fragment, viewGroup, false);
        this.binding = smk25miniScChFragmentBinding;
        smk25miniScChFragmentBinding.setData(this.shift);
        this.binding.setListener(this);
        return this.binding.getRoot();
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }
}
