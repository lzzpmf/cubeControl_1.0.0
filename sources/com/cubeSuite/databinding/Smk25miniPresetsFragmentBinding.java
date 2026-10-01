package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25miniPresetsFragmentBinding extends ViewDataBinding {

    @Bindable
    protected Smk25MiniGlob mData;

    @Bindable
    protected Smk25MiniPresetsFragment mListener;
    public final FrameLayout usrFragment;

    public abstract void setData(Smk25MiniGlob smk25MiniGlob);

    public abstract void setListener(Smk25MiniPresetsFragment smk25MiniPresetsFragment);

    protected Smk25miniPresetsFragmentBinding(Object obj, View view, int i, FrameLayout frameLayout) {
        super(obj, view, i);
        this.usrFragment = frameLayout;
    }

    public Smk25MiniGlob getData() {
        return this.mData;
    }

    public Smk25MiniPresetsFragment getListener() {
        return this.mListener;
    }

    public static Smk25miniPresetsFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniPresetsFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25miniPresetsFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_presets_fragment, viewGroup, z, obj);
    }

    public static Smk25miniPresetsFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniPresetsFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25miniPresetsFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_presets_fragment, null, false, obj);
    }

    public static Smk25miniPresetsFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniPresetsFragmentBinding bind(View view, Object obj) {
        return (Smk25miniPresetsFragmentBinding) bind(obj, view, R.layout.smk25mini_presets_fragment);
    }
}
