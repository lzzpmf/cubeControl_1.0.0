package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import androidx.viewpager.widget.ViewPager;
import com.cubeSuite.R;
import com.google.android.material.tabs.TabLayout;

/* JADX INFO: loaded from: classes.dex */
public abstract class MidikeyboardControlFragmentBinding extends ViewDataBinding {
    public final TabLayout tabGlob;
    public final ViewPager viewPagerGlob;

    protected MidikeyboardControlFragmentBinding(Object obj, View view, int i, TabLayout tabLayout, ViewPager viewPager) {
        super(obj, view, i);
        this.tabGlob = tabLayout;
        this.viewPagerGlob = viewPager;
    }

    public static MidikeyboardControlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static MidikeyboardControlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (MidikeyboardControlFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.midikeyboard_control_fragment, viewGroup, z, obj);
    }

    public static MidikeyboardControlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static MidikeyboardControlFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (MidikeyboardControlFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.midikeyboard_control_fragment, null, false, obj);
    }

    public static MidikeyboardControlFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static MidikeyboardControlFragmentBinding bind(View view, Object obj) {
        return (MidikeyboardControlFragmentBinding) bind(obj, view, R.layout.midikeyboard_control_fragment);
    }
}
