package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniGlobFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25MiniGlobFragmentBinding extends ViewDataBinding {
    public final AppCompatSpinner keyCurve;

    @Bindable
    protected Smk25MiniGlob mData;

    @Bindable
    protected Smk25MiniGlobFragment mListener;
    public final AppCompatSpinner port;

    public abstract void setData(Smk25MiniGlob smk25MiniGlob);

    public abstract void setListener(Smk25MiniGlobFragment smk25MiniGlobFragment);

    protected Smk25MiniGlobFragmentBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, AppCompatSpinner appCompatSpinner2) {
        super(obj, view, i);
        this.keyCurve = appCompatSpinner;
        this.port = appCompatSpinner2;
    }

    public Smk25MiniGlob getData() {
        return this.mData;
    }

    public Smk25MiniGlobFragment getListener() {
        return this.mListener;
    }

    public static Smk25MiniGlobFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25MiniGlobFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25MiniGlobFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25_mini_glob_fragment, viewGroup, z, obj);
    }

    public static Smk25MiniGlobFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25MiniGlobFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25MiniGlobFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25_mini_glob_fragment, null, false, obj);
    }

    public static Smk25MiniGlobFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25MiniGlobFragmentBinding bind(View view, Object obj) {
        return (Smk25MiniGlobFragmentBinding) bind(obj, view, R.layout.smk25_mini_glob_fragment);
    }
}
