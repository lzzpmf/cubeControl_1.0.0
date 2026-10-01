package com.cubeSuite.fragment.midikeyboard;

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
import com.cubeSuite.databinding.PresetsFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.entity.midikeyboard.MidikeyboardEntry;
import com.cubeSuite.entity.midikeyboard.Usr;
import com.cubeSuite.utils.FileUtils;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardPresetsFragment extends Fragment {
    private PresetsFragmentBinding binding;
    private final Glob glob;
    private final MidikeyboardCommunication mc;
    MidikeyboardFragment mf;
    private final MidikeyboardEntry midikeyboardEntry;
    private final Usr[] usrs;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance((AppCompatActivity) MidikeyboardPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.hint).setContent(R.string.resetSucceed).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.1.1
            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
            return false;
        }
    });
    private final MidikeyboardUsrFragment[] muf = new MidikeyboardUsrFragment[8];

    public MidikeyboardPresetsFragment(MidikeyboardCommunication midikeyboardCommunication, MidikeyboardFragment midikeyboardFragment, MidikeyboardEntry midikeyboardEntry) {
        this.usrs = midikeyboardEntry.getUsr();
        this.glob = midikeyboardEntry.getGlob();
        this.mc = midikeyboardCommunication;
        this.mf = midikeyboardFragment;
        this.midikeyboardEntry = midikeyboardEntry;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        int i = 0;
        PresetsFragmentBinding presetsFragmentBinding = (PresetsFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.presets_fragment, viewGroup, false);
        this.binding = presetsFragmentBinding;
        presetsFragmentBinding.setData(this.glob);
        this.binding.setListener(this);
        FragmentTransaction fragmentTransactionBeginTransaction = getParentFragmentManager().beginTransaction();
        while (true) {
            MidikeyboardUsrFragment[] midikeyboardUsrFragmentArr = this.muf;
            if (i < midikeyboardUsrFragmentArr.length) {
                midikeyboardUsrFragmentArr[i] = new MidikeyboardUsrFragment(this.usrs[i], this.mc);
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
        for (MidikeyboardUsrFragment midikeyboardUsrFragment : this.muf) {
            fragmentTransactionBeginTransaction.hide(midikeyboardUsrFragment);
        }
        fragmentTransactionBeginTransaction.show(this.muf[i]);
        fragmentTransactionBeginTransaction.commit();
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
    }

    public void saveConfig() {
        this.mc.saveConfig(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance((AppCompatActivity) MidikeyboardPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.success).setContent(R.string.saveConfigSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                } else {
                    AlertDialogUtil.getInstance((AppCompatActivity) MidikeyboardPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.failure).setContent(R.string.saveConfigFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                }
            }
        });
    }

    public void resetConfig() {
        AlertDialogUtil.getInstance((AppCompatActivity) getActivity()).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.3
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                AlertDialogUtil.getInstance((AppCompatActivity) MidikeyboardPresetsFragment.this.getActivity()).setTitle(R.string.hint).setContent(R.string.recoveringData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
                byte[] file = FileUtils.readFile(MidikeyboardPresetsFragment.this.getResources().openRawResource(R.raw.reset_factory_midikeyboard));
                MidikeyboardPresetsFragment.this.midikeyboardEntry.setUsrData(file);
                MidikeyboardPresetsFragment.this.mc.resetFactory(file, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.3.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            MidikeyboardPresetsFragment.this.mf.reLoadUsr();
                            MidikeyboardPresetsFragment.this.handler.sendEmptyMessageDelayed(0, 2000L);
                        } else {
                            AlertDialogUtil.getInstance((AppCompatActivity) MidikeyboardPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.error).setContent(R.string.resetFailure).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment.3.1.1
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
