package com.cubeSuite.fragment.smk25II;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.appcompat.app.AppCompatActivity;
import androidx.databinding.DataBindingUtil;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentTransaction;
import com.cubeSuite.R;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.MidikeyboardCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.databinding.Smk25iiPresetsBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.entity.smk25II.Smk25IIEntry;
import com.cubeSuite.entity.smk25II.Smk25IIUsr;
import com.cubeSuite.utils.FileUtils;

/* JADX INFO: loaded from: classes.dex */
public class Smk25IIPresetsFragment extends Fragment {
    private Smk25iiPresetsBinding binding;
    private final Smk25IIEntry entry;
    private final Glob glob;
    private final MidikeyboardCommunication mc;
    Smk25IIFragment mf;
    private final Smk25IIUsr[] usrs;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance((AppCompatActivity) Smk25IIPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.hint).setContent(R.string.resetSucceed).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.1.1
            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
            return false;
        }
    });
    private final Smk25IIUsrFragment[] muf = new Smk25IIUsrFragment[8];

    public Smk25IIPresetsFragment(MidikeyboardCommunication midikeyboardCommunication, Smk25IIFragment smk25IIFragment, Smk25IIEntry smk25IIEntry) {
        this.usrs = smk25IIEntry.getUsr();
        this.glob = smk25IIEntry.getGlob();
        this.mc = midikeyboardCommunication;
        this.mf = smk25IIFragment;
        this.entry = smk25IIEntry;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        int i = 0;
        Smk25iiPresetsBinding smk25iiPresetsBinding = (Smk25iiPresetsBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25ii_presets, viewGroup, false);
        this.binding = smk25iiPresetsBinding;
        smk25iiPresetsBinding.setData(this.glob);
        this.binding.setListener(this);
        FragmentTransaction fragmentTransactionBeginTransaction = getParentFragmentManager().beginTransaction();
        while (true) {
            Smk25IIUsrFragment[] smk25IIUsrFragmentArr = this.muf;
            if (i < smk25IIUsrFragmentArr.length) {
                smk25IIUsrFragmentArr[i] = new Smk25IIUsrFragment(this.usrs[i], this.mc);
                fragmentTransactionBeginTransaction.add(R.id.usrFragment, this.muf[i], (String) null);
                i++;
            } else {
                fragmentTransactionBeginTransaction.commit();
                switchUsrConfig(this.glob.getPreset().getData());
                return this.binding.getRoot();
            }
        }
    }

    public void switchUsrConfig(int i) {
        FragmentTransaction fragmentTransactionBeginTransaction = getParentFragmentManager().beginTransaction();
        for (Smk25IIUsrFragment smk25IIUsrFragment : this.muf) {
            fragmentTransactionBeginTransaction.hide(smk25IIUsrFragment);
        }
        fragmentTransactionBeginTransaction.show(this.muf[i]);
        fragmentTransactionBeginTransaction.commit();
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
    }

    public void saveConfig() {
        this.mc.saveConfig(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance((AppCompatActivity) Smk25IIPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.success).setContent(R.string.saveConfigSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                } else {
                    AlertDialogUtil.getInstance((AppCompatActivity) Smk25IIPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.failure).setContent(R.string.saveConfigFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                }
            }
        });
    }

    public void resetConfig() {
        AlertDialogUtil.getInstance((AppCompatActivity) getActivity()).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.3
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                AlertDialogUtil.getInstance((AppCompatActivity) Smk25IIPresetsFragment.this.getActivity()).setTitle(R.string.hint).setContent(R.string.recoveringData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
                byte[] file = FileUtils.readFile(Smk25IIPresetsFragment.this.getResources().openRawResource(R.raw.reset_factory_midikeyboard));
                Smk25IIPresetsFragment.this.entry.setUsrData(file);
                Smk25IIPresetsFragment.this.mc.resetFactory(file, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.3.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            Smk25IIPresetsFragment.this.mf.reLoadUsr();
                            Smk25IIPresetsFragment.this.handler.sendEmptyMessageDelayed(0, 2000L);
                        } else {
                            AlertDialogUtil.getInstance((AppCompatActivity) Smk25IIPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.error).setContent(R.string.resetFailure).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment.3.1.1
                            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
                        }
                    }
                });
            }
        }, false).setTitle(R.string.warning).setContent(R.string.hintResetFactory).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    public void changeConfig(int i, AddrU8 addrU8) {
        U8DataChange(i, addrU8);
        switchUsrConfig(i);
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }
}
