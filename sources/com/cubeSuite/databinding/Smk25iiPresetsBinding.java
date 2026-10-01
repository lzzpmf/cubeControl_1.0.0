package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.SeekBar;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.fragment.smk25II.Smk25IIPresetsFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25iiPresetsBinding extends ViewDataBinding {

    @Bindable
    protected Glob mData;

    @Bindable
    protected Smk25IIPresetsFragment mListener;
    public final SeekBar preset;
    public final FrameLayout usrFragment;

    public abstract void setData(Glob glob);

    public abstract void setListener(Smk25IIPresetsFragment smk25IIPresetsFragment);

    protected Smk25iiPresetsBinding(Object obj, View view, int i, SeekBar seekBar, FrameLayout frameLayout) {
        super(obj, view, i);
        this.preset = seekBar;
        this.usrFragment = frameLayout;
    }

    public Glob getData() {
        return this.mData;
    }

    public Smk25IIPresetsFragment getListener() {
        return this.mListener;
    }

    public static Smk25iiPresetsBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiPresetsBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25iiPresetsBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25ii_presets, viewGroup, z, obj);
    }

    public static Smk25iiPresetsBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiPresetsBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25iiPresetsBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25ii_presets, null, false, obj);
    }

    public static Smk25iiPresetsBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiPresetsBinding bind(View view, Object obj) {
        return (Smk25iiPresetsBinding) bind(obj, view, R.layout.smk25ii_presets);
    }
}
