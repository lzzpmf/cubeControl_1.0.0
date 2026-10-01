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
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardGlobFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class GlobFragmentBinding extends ViewDataBinding {
    public final EditText gate;
    public final AppCompatSpinner keyCurve;

    @Bindable
    protected Glob mData;

    @Bindable
    protected MidikeyboardGlobFragment mListener;
    public final SwitchCompat padAfterTouch;
    public final AppCompatSpinner padCurve;
    public final AppCompatSpinner port;

    public abstract void setData(Glob glob);

    public abstract void setListener(MidikeyboardGlobFragment midikeyboardGlobFragment);

    protected GlobFragmentBinding(Object obj, View view, int i, EditText editText, AppCompatSpinner appCompatSpinner, SwitchCompat switchCompat, AppCompatSpinner appCompatSpinner2, AppCompatSpinner appCompatSpinner3) {
        super(obj, view, i);
        this.gate = editText;
        this.keyCurve = appCompatSpinner;
        this.padAfterTouch = switchCompat;
        this.padCurve = appCompatSpinner2;
        this.port = appCompatSpinner3;
    }

    public Glob getData() {
        return this.mData;
    }

    public MidikeyboardGlobFragment getListener() {
        return this.mListener;
    }

    public static GlobFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static GlobFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (GlobFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.glob_fragment, viewGroup, z, obj);
    }

    public static GlobFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static GlobFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (GlobFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.glob_fragment, null, false, obj);
    }

    public static GlobFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static GlobFragmentBinding bind(View view, Object obj) {
        return (GlobFragmentBinding) bind(obj, view, R.layout.glob_fragment);
    }
}
