package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.midikeyboard.Shift;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniScChFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25miniScChFragmentBinding extends ViewDataBinding {
    public final AppCompatSpinner chord;
    public final AppCompatSpinner keyCurve;

    @Bindable
    protected Shift mData;

    @Bindable
    protected Smk25MiniScChFragment mListener;
    public final AppCompatSpinner type;

    public abstract void setData(Shift shift);

    public abstract void setListener(Smk25MiniScChFragment smk25MiniScChFragment);

    protected Smk25miniScChFragmentBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2, AppCompatSpinner appCompatSpinner3) {
        super(obj, view, i);
        this.chord = appCompatSpinner;
        this.keyCurve = appCompatSpinner2;
        this.type = appCompatSpinner3;
    }

    public Shift getData() {
        return this.mData;
    }

    public Smk25MiniScChFragment getListener() {
        return this.mListener;
    }

    public static Smk25miniScChFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniScChFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25miniScChFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_sc_ch_fragment, viewGroup, z, obj);
    }

    public static Smk25miniScChFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniScChFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25miniScChFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_sc_ch_fragment, null, false, obj);
    }

    public static Smk25miniScChFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniScChFragmentBinding bind(View view, Object obj) {
        return (Smk25miniScChFragmentBinding) bind(obj, view, R.layout.smk25mini_sc_ch_fragment);
    }
}
