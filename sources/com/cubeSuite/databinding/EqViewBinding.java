package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.customControl.EQPic;

/* JADX INFO: loaded from: classes.dex */
public abstract class EqViewBinding extends ViewDataBinding {
    public final SeekBar db;
    public final ImageView dbAdd;
    public final LinearLayout dbContainer;
    public final ImageView dbMinus;
    public final TextView dbText;
    public final SwitchCompat enableSwitch;
    public final TextView enableTv;
    public final EQPic eqPic;
    public final SeekBar hz;
    public final ImageView hzAdd;
    public final ImageView hzMinus;
    public final TextView hzText;
    public final AppCompatSpinner peakSpinner;
    public final SeekBar q;
    public final ImageView qAdd;
    public final LinearLayout qContainer;
    public final ImageView qMinus;
    public final TextView qText;
    public final Button resetBut;

    protected EqViewBinding(Object obj, View view, int i, SeekBar seekBar, ImageView imageView, LinearLayout linearLayout, ImageView imageView2, TextView textView, SwitchCompat switchCompat, TextView textView2, EQPic eQPic, SeekBar seekBar2, ImageView imageView3, ImageView imageView4, TextView textView3, AppCompatSpinner appCompatSpinner, SeekBar seekBar3, ImageView imageView5, LinearLayout linearLayout2, ImageView imageView6, TextView textView4, Button button) {
        super(obj, view, i);
        this.db = seekBar;
        this.dbAdd = imageView;
        this.dbContainer = linearLayout;
        this.dbMinus = imageView2;
        this.dbText = textView;
        this.enableSwitch = switchCompat;
        this.enableTv = textView2;
        this.eqPic = eQPic;
        this.hz = seekBar2;
        this.hzAdd = imageView3;
        this.hzMinus = imageView4;
        this.hzText = textView3;
        this.peakSpinner = appCompatSpinner;
        this.q = seekBar3;
        this.qAdd = imageView5;
        this.qContainer = linearLayout2;
        this.qMinus = imageView6;
        this.qText = textView4;
        this.resetBut = button;
    }

    public static EqViewBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static EqViewBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (EqViewBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.eq_view, viewGroup, z, obj);
    }

    public static EqViewBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static EqViewBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (EqViewBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.eq_view, null, false, obj);
    }

    public static EqViewBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static EqViewBinding bind(View view, Object obj) {
        return (EqViewBinding) bind(obj, view, R.layout.eq_view);
    }
}
