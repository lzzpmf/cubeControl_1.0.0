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
import com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class PresetsFragmentBinding extends ViewDataBinding {

    @Bindable
    protected Glob mData;

    @Bindable
    protected MidikeyboardPresetsFragment mListener;
    public final SeekBar preset;
    public final FrameLayout usrFragment;

    public abstract void setData(Glob glob);

    public abstract void setListener(MidikeyboardPresetsFragment midikeyboardPresetsFragment);

    protected PresetsFragmentBinding(Object obj, View view, int i, SeekBar seekBar, FrameLayout frameLayout) {
        super(obj, view, i);
        this.preset = seekBar;
        this.usrFragment = frameLayout;
    }

    public Glob getData() {
        return this.mData;
    }

    public MidikeyboardPresetsFragment getListener() {
        return this.mListener;
    }

    public static PresetsFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static PresetsFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (PresetsFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.presets_fragment, viewGroup, z, obj);
    }

    public static PresetsFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static PresetsFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (PresetsFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.presets_fragment, null, false, obj);
    }

    public static PresetsFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static PresetsFragmentBinding bind(View view, Object obj) {
        return (PresetsFragmentBinding) bind(obj, view, R.layout.presets_fragment);
    }
}
