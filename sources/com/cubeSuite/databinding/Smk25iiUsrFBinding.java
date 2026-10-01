package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.smk25II.Smk25IIUsr;
import com.cubeSuite.entity.smk25II.Smk25IIUsrBinding;
import com.cubeSuite.fragment.smk25II.Smk25IIUsrFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25iiUsrFBinding extends ViewDataBinding {
    public final AppCompatSpinner channel;
    public final LinearLayout channelLayout;
    public final LinearLayout keybed;
    public final AppCompatSpinner keybedChannel;
    public final EditText keybedMaxVel;
    public final EditText keybedMinVel;
    public final LinearLayout knobCCLayout;
    public final TextView knobCCStr;
    public final TextView knobMaxText;
    public final EditText knobMaxVal;
    public final TextView knobMinText;
    public final EditText knobMinVal;
    public final SeekBar knobSeekbar;
    public final AppCompatSpinner knobType;
    public final LinearLayout knobs;
    public final TextView ledText;
    public final EditText ledVel;

    @Bindable
    protected Smk25IIUsr mData;

    @Bindable
    protected Smk25IIUsrFragment mListener;

    @Bindable
    protected Smk25IIUsrBinding mUsrBinding;
    public final LinearLayout maxValueLayout;
    public final EditText maxVel;
    public final LinearLayout minValueLayout;
    public final EditText minVel;
    public final EditText modCCVal;
    public final EditText modMaxVal;
    public final EditText modMinVal;
    public final LinearLayout mps;
    public final AppCompatSpinner octive;
    public final RadioButton padBtn;
    public final TextView padMaxText;
    public final TextView padMinText;
    public final EditText padNote;
    public final LinearLayout padNoteLayout;
    public final TextView padNoteStr;
    public final TextView padNoteText;
    public final SeekBar padSeekbar;
    public final LinearLayout pads;
    public final Button selectColorBtn;
    public final EditText sustainCCVal;
    public final EditText sustainDownVal;
    public final EditText sustainUpVal;
    public final LinearLayout sysExLayout;
    public final EditText sysExValue;
    public final AppCompatSpinner transpose;
    public final AppCompatSpinner type;
    public final RadioGroup typeRadio;

    public abstract void setData(Smk25IIUsr smk25IIUsr);

    public abstract void setListener(Smk25IIUsrFragment smk25IIUsrFragment);

    public abstract void setUsrBinding(Smk25IIUsrBinding smk25IIUsrBinding);

    protected Smk25iiUsrFBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, LinearLayout linearLayout, LinearLayout linearLayout2, AppCompatSpinner appCompatSpinner2, EditText editText, EditText editText2, LinearLayout linearLayout3, TextView textView, TextView textView2, EditText editText3, TextView textView3, EditText editText4, SeekBar seekBar, AppCompatSpinner appCompatSpinner3, LinearLayout linearLayout4, TextView textView4, EditText editText5, LinearLayout linearLayout5, EditText editText6, LinearLayout linearLayout6, EditText editText7, EditText editText8, EditText editText9, EditText editText10, LinearLayout linearLayout7, AppCompatSpinner appCompatSpinner4, RadioButton radioButton, TextView textView5, TextView textView6, EditText editText11, LinearLayout linearLayout8, TextView textView7, TextView textView8, SeekBar seekBar2, LinearLayout linearLayout9, Button button, EditText editText12, EditText editText13, EditText editText14, LinearLayout linearLayout10, EditText editText15, AppCompatSpinner appCompatSpinner5, AppCompatSpinner appCompatSpinner6, RadioGroup radioGroup) {
        super(obj, view, i);
        this.channel = appCompatSpinner;
        this.channelLayout = linearLayout;
        this.keybed = linearLayout2;
        this.keybedChannel = appCompatSpinner2;
        this.keybedMaxVel = editText;
        this.keybedMinVel = editText2;
        this.knobCCLayout = linearLayout3;
        this.knobCCStr = textView;
        this.knobMaxText = textView2;
        this.knobMaxVal = editText3;
        this.knobMinText = textView3;
        this.knobMinVal = editText4;
        this.knobSeekbar = seekBar;
        this.knobType = appCompatSpinner3;
        this.knobs = linearLayout4;
        this.ledText = textView4;
        this.ledVel = editText5;
        this.maxValueLayout = linearLayout5;
        this.maxVel = editText6;
        this.minValueLayout = linearLayout6;
        this.minVel = editText7;
        this.modCCVal = editText8;
        this.modMaxVal = editText9;
        this.modMinVal = editText10;
        this.mps = linearLayout7;
        this.octive = appCompatSpinner4;
        this.padBtn = radioButton;
        this.padMaxText = textView5;
        this.padMinText = textView6;
        this.padNote = editText11;
        this.padNoteLayout = linearLayout8;
        this.padNoteStr = textView7;
        this.padNoteText = textView8;
        this.padSeekbar = seekBar2;
        this.pads = linearLayout9;
        this.selectColorBtn = button;
        this.sustainCCVal = editText12;
        this.sustainDownVal = editText13;
        this.sustainUpVal = editText14;
        this.sysExLayout = linearLayout10;
        this.sysExValue = editText15;
        this.transpose = appCompatSpinner5;
        this.type = appCompatSpinner6;
        this.typeRadio = radioGroup;
    }

    public Smk25IIUsr getData() {
        return this.mData;
    }

    public Smk25IIUsrBinding getUsrBinding() {
        return this.mUsrBinding;
    }

    public Smk25IIUsrFragment getListener() {
        return this.mListener;
    }

    public static Smk25iiUsrFBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiUsrFBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25iiUsrFBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25ii_usr_f, viewGroup, z, obj);
    }

    public static Smk25iiUsrFBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiUsrFBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25iiUsrFBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25ii_usr_f, null, false, obj);
    }

    public static Smk25iiUsrFBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25iiUsrFBinding bind(View view, Object obj) {
        return (Smk25iiUsrFBinding) bind(obj, view, R.layout.smk25ii_usr_f);
    }
}
