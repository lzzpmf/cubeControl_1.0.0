package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.EqView;

/* JADX INFO: loaded from: classes.dex */
public abstract class IrBoxContorlFragmentBinding extends ViewDataBinding {
    public final LinearLayout editEqView;
    public final AppCompatSpinner effectSpinner;
    public final RadioButton eqRadio;
    public final EqView eqView;
    public final LinearLayout irContainer;
    public final LinearLayout irEdit;
    public final RadioButton irRadio;
    public final RecyclerView irRv;
    public final TextView irVolText;
    public final SeekBar irVolumeSeek;
    public final LinearLayout main;
    public final Button openFileBut;
    public final Button saveBut;
    public final SwitchCompat switchEq;
    public final SwitchCompat switchIr;
    public final Button synBut;
    public final RadioGroup typeRadio;
    public final ImageView volAdd;
    public final ImageView volMinus;

    protected IrBoxContorlFragmentBinding(Object obj, View view, int i, LinearLayout linearLayout, AppCompatSpinner appCompatSpinner, RadioButton radioButton, EqView eqView, LinearLayout linearLayout2, LinearLayout linearLayout3, RadioButton radioButton2, RecyclerView recyclerView, TextView textView, SeekBar seekBar, LinearLayout linearLayout4, Button button, Button button2, SwitchCompat switchCompat, SwitchCompat switchCompat2, Button button3, RadioGroup radioGroup, ImageView imageView, ImageView imageView2) {
        super(obj, view, i);
        this.editEqView = linearLayout;
        this.effectSpinner = appCompatSpinner;
        this.eqRadio = radioButton;
        this.eqView = eqView;
        this.irContainer = linearLayout2;
        this.irEdit = linearLayout3;
        this.irRadio = radioButton2;
        this.irRv = recyclerView;
        this.irVolText = textView;
        this.irVolumeSeek = seekBar;
        this.main = linearLayout4;
        this.openFileBut = button;
        this.saveBut = button2;
        this.switchEq = switchCompat;
        this.switchIr = switchCompat2;
        this.synBut = button3;
        this.typeRadio = radioGroup;
        this.volAdd = imageView;
        this.volMinus = imageView2;
    }

    public static IrBoxContorlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static IrBoxContorlFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (IrBoxContorlFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.ir_box_contorl_fragment, viewGroup, z, obj);
    }

    public static IrBoxContorlFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static IrBoxContorlFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (IrBoxContorlFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.ir_box_contorl_fragment, null, false, obj);
    }

    public static IrBoxContorlFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static IrBoxContorlFragmentBinding bind(View view, Object obj) {
        return (IrBoxContorlFragmentBinding) bind(obj, view, R.layout.ir_box_contorl_fragment);
    }
}
