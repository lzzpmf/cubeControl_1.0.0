package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.midikeyboard.Arp;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniArpFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25miniArpFragmentBinding extends ViewDataBinding {
    public final EditText gate;
    public final SwitchCompat latch;

    @Bindable
    protected Arp mData;

    @Bindable
    protected Smk25MiniArpFragment mListener;
    public final AppCompatSpinner mode;
    public final AppCompatSpinner octive;
    public final EditText swing;
    public final SwitchCompat sync;
    public final EditText tempo;
    public final AppCompatSpinner time;

    public abstract void setData(Arp arp);

    public abstract void setListener(Smk25MiniArpFragment smk25MiniArpFragment);

    protected Smk25miniArpFragmentBinding(Object obj, View view, int i, EditText editText, SwitchCompat switchCompat, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2, EditText editText2, SwitchCompat switchCompat2, EditText editText3, AppCompatSpinner appCompatSpinner3) {
        super(obj, view, i);
        this.gate = editText;
        this.latch = switchCompat;
        this.mode = appCompatSpinner;
        this.octive = appCompatSpinner2;
        this.swing = editText2;
        this.sync = switchCompat2;
        this.tempo = editText3;
        this.time = appCompatSpinner3;
    }

    public Arp getData() {
        return this.mData;
    }

    public Smk25MiniArpFragment getListener() {
        return this.mListener;
    }

    public static Smk25miniArpFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniArpFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25miniArpFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_arp_fragment, viewGroup, z, obj);
    }

    public static Smk25miniArpFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniArpFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25miniArpFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_arp_fragment, null, false, obj);
    }

    public static Smk25miniArpFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniArpFragmentBinding bind(View view, Object obj) {
        return (Smk25miniArpFragmentBinding) bind(obj, view, R.layout.smk25mini_arp_fragment);
    }
}
