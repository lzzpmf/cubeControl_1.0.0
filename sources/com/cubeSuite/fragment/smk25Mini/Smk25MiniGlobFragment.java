package com.cubeSuite.fragment.smk25Mini;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.communication.Smk25MiniCommunication;
import com.cubeSuite.databinding.Smk25MiniGlobFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.utils.StringUtils;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniGlobFragment extends Fragment {
    private final Smk25MiniGlob glob;
    private final Smk25MiniCommunication mc;

    public Smk25MiniGlobFragment(Smk25MiniGlob smk25MiniGlob, Smk25MiniCommunication smk25MiniCommunication) {
        this.glob = smk25MiniGlob;
        this.mc = smk25MiniCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Smk25MiniGlobFragmentBinding smk25MiniGlobFragmentBinding = (Smk25MiniGlobFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25_mini_glob_fragment, viewGroup, false);
        smk25MiniGlobFragmentBinding.setData(this.glob);
        smk25MiniGlobFragmentBinding.setListener(this);
        return smk25MiniGlobFragmentBinding.getRoot();
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }

    public void setNum(CharSequence charSequence, AddrU8 addrU8, int i, int i2) {
        U8DataChange(Math.min(i2, Math.max(StringUtils.getNum(charSequence.toString()), i)), addrU8);
    }
}
