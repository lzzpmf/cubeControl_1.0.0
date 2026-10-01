package com.cubeSuite.fragment.midikeyboard;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.communication.MidikeyboardCommunication;
import com.cubeSuite.databinding.ScChFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Shift;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardScChFragment extends Fragment {
    ScChFragmentBinding binding;
    private final MidikeyboardCommunication mc;
    private final Shift shift;

    public MidikeyboardScChFragment(Shift shift, MidikeyboardCommunication midikeyboardCommunication) {
        this.shift = shift;
        this.mc = midikeyboardCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        ScChFragmentBinding scChFragmentBinding = (ScChFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.sc_ch_fragment, viewGroup, false);
        this.binding = scChFragmentBinding;
        scChFragmentBinding.setData(this.shift);
        this.binding.setListener(this);
        return this.binding.getRoot();
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }
}
