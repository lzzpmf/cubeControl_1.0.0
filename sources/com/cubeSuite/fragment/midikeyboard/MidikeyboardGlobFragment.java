package com.cubeSuite.fragment.midikeyboard;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.communication.MidikeyboardCommunication;
import com.cubeSuite.databinding.GlobFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.utils.NumEditFilter;
import com.cubeSuite.utils.StringUtils;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardGlobFragment extends Fragment {
    private final Glob glob;
    private final MidikeyboardCommunication mc;

    public MidikeyboardGlobFragment(Glob glob, MidikeyboardCommunication midikeyboardCommunication) {
        this.glob = glob;
        this.mc = midikeyboardCommunication;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        GlobFragmentBinding globFragmentBinding = (GlobFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.glob_fragment, viewGroup, false);
        globFragmentBinding.setData(this.glob);
        globFragmentBinding.setListener(this);
        globFragmentBinding.gate.setFilters(new NumEditFilter[]{new NumEditFilter(0, 255)});
        return globFragmentBinding.getRoot();
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
