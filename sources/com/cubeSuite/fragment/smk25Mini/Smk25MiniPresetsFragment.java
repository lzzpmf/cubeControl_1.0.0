package com.cubeSuite.fragment.smk25Mini;

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
import com.cubeSuite.communication.Smk25MiniCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.databinding.Smk25miniPresetsFragmentBinding;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.smk25Mini.Smk25MiniEntry;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.entity.smk25Mini.Smk25MiniUsr;
import com.cubeSuite.utils.FileUtils;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniPresetsFragment extends Fragment {
    private Smk25miniPresetsFragmentBinding binding;
    private final Smk25MiniEntry entry;
    private final Smk25MiniGlob glob;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance((AppCompatActivity) Smk25MiniPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.hint).setContent(R.string.resetSucceed).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.1.1
            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
            return false;
        }
    });
    private final Smk25MiniCommunication mc;
    Smk25MiniFragment mf;
    private Smk25MiniUsrFragment muf;
    private Smk25MiniUsr usr;

    public Smk25MiniPresetsFragment(Smk25MiniCommunication smk25MiniCommunication, Smk25MiniFragment smk25MiniFragment, Smk25MiniEntry smk25MiniEntry) {
        this.usr = smk25MiniEntry.getUsr();
        Smk25MiniGlob glob = smk25MiniEntry.getGlob();
        this.glob = glob;
        this.mc = smk25MiniCommunication;
        this.mf = smk25MiniFragment;
        this.entry = smk25MiniEntry;
        this.muf = new Smk25MiniUsrFragment(this.usr, glob, smk25MiniCommunication);
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Smk25miniPresetsFragmentBinding smk25miniPresetsFragmentBinding = (Smk25miniPresetsFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.smk25mini_presets_fragment, viewGroup, false);
        this.binding = smk25miniPresetsFragmentBinding;
        smk25miniPresetsFragmentBinding.setData(this.glob);
        this.binding.setListener(this);
        FragmentTransaction fragmentTransactionBeginTransaction = getParentFragmentManager().beginTransaction();
        Smk25MiniUsrFragment smk25MiniUsrFragment = new Smk25MiniUsrFragment(this.usr, this.glob, this.mc);
        this.muf = smk25MiniUsrFragment;
        fragmentTransactionBeginTransaction.add(R.id.usrFragment, smk25MiniUsrFragment, (String) null);
        fragmentTransactionBeginTransaction.commit();
        switchUsrConfig();
        return this.binding.getRoot();
    }

    public void switchUsrConfig() {
        FragmentTransaction fragmentTransactionBeginTransaction = getParentFragmentManager().beginTransaction();
        fragmentTransactionBeginTransaction.show(this.muf);
        fragmentTransactionBeginTransaction.commit();
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
    }

    public void saveConfig() {
        this.mc.saveConfig(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance((AppCompatActivity) Smk25MiniPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.success).setContent(R.string.saveConfigSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                } else {
                    AlertDialogUtil.getInstance((AppCompatActivity) Smk25MiniPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.failure).setContent(R.string.saveConfigFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                }
            }
        });
    }

    public void resetConfig() {
        AlertDialogUtil.getInstance((AppCompatActivity) getActivity()).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.3
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                AlertDialogUtil.getInstance((AppCompatActivity) Smk25MiniPresetsFragment.this.getActivity()).setTitle(R.string.hint).setContent(R.string.recoveringData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
                byte[] file = FileUtils.readFile(Smk25MiniPresetsFragment.this.getResources().openRawResource(R.raw.reset_factory_smk25mini));
                Objects.requireNonNull(Smk25MiniPresetsFragment.this.entry);
                byte[] bArr = new byte[16];
                Objects.requireNonNull(Smk25MiniPresetsFragment.this.entry);
                byte[] bArr2 = new byte[93];
                Objects.requireNonNull(Smk25MiniPresetsFragment.this.entry);
                System.arraycopy(file, 0, bArr, 0, 16);
                Objects.requireNonNull(Smk25MiniPresetsFragment.this.entry);
                Objects.requireNonNull(Smk25MiniPresetsFragment.this.entry);
                System.arraycopy(file, 16, bArr2, 0, 93);
                Smk25MiniPresetsFragment.this.entry.setGlobData(bArr);
                Smk25MiniPresetsFragment.this.entry.setUsrData(bArr2);
                Smk25MiniPresetsFragment.this.mc.resetFactory(bArr, bArr2, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.3.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            Smk25MiniPresetsFragment.this.mf.reLoad();
                            Smk25MiniPresetsFragment.this.handler.sendEmptyMessageDelayed(0, 2000L);
                        } else {
                            AlertDialogUtil.getInstance((AppCompatActivity) Smk25MiniPresetsFragment.this.getActivity()).showDialog().setTitle(R.string.error).setContent(R.string.resetFailure).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment.3.1.1
                            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
                        }
                    }
                });
            }
        }, false).setTitle(R.string.warning).setContent(R.string.hintResetFactory).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    public void U8DataChange(int i, AddrU8 addrU8) {
        if (addrU8.setData(i)) {
            this.mc.sendGlobU8(addrU8);
        }
    }
}
