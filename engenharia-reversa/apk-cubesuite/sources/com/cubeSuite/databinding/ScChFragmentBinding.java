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
import com.cubeSuite.fragment.midikeyboard.MidikeyboardScChFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class ScChFragmentBinding extends ViewDataBinding {
    public final AppCompatSpinner chord;
    public final AppCompatSpinner keyCurve;

    @Bindable
    protected Shift mData;

    @Bindable
    protected MidikeyboardScChFragment mListener;
    public final AppCompatSpinner type;

    public abstract void setData(Shift shift);

    public abstract void setListener(MidikeyboardScChFragment midikeyboardScChFragment);

    protected ScChFragmentBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2, AppCompatSpinner appCompatSpinner3) {
        super(obj, view, i);
        this.chord = appCompatSpinner;
        this.keyCurve = appCompatSpinner2;
        this.type = appCompatSpinner3;
    }

    public Shift getData() {
        return this.mData;
    }

    public MidikeyboardScChFragment getListener() {
        return this.mListener;
    }

    public static ScChFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static ScChFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (ScChFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.sc_ch_fragment, viewGroup, z, obj);
    }

    public static ScChFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static ScChFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (ScChFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.sc_ch_fragment, null, false, obj);
    }

    public static ScChFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static ScChFragmentBinding bind(View view, Object obj) {
        return (ScChFragmentBinding) bind(obj, view, R.layout.sc_ch_fragment);
    }
}
