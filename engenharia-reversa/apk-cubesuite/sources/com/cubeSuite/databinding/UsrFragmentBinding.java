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
import com.cubeSuite.entity.midikeyboard.Usr;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardUsrFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class UsrFragmentBinding extends ViewDataBinding {
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

    @Bindable
    protected Usr mData;

    @Bindable
    protected MidikeyboardUsrFragment mListener;

    @Bindable
    protected UsrBinding mUsrBinding;
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
    public final LinearLayout ppr;
    public final LinearLayout psrCCLayout;
    public final TextView psrCCStr;
    public final LinearLayout psrChannelLayout;
    public final AppCompatSpinner psrMode;
    public final EditText psrValue1;
    public final LinearLayout psrValue1Layout;
    public final EditText psrValue2;
    public final LinearLayout psrValue2Layout;
    public final Button selectColorBtn;
    public final EditText sustainCCVal;
    public final EditText sustainDownVal;
    public final EditText sustainUpVal;
    public final LinearLayout sysExLayout;
    public final EditText sysExValue;
    public final AppCompatSpinner transpose;
    public final AppCompatSpinner type;
    public final RadioGroup typeRadio;

    public abstract void setData(Usr usr);

    public abstract void setListener(MidikeyboardUsrFragment midikeyboardUsrFragment);

    public abstract void setUsrBinding(UsrBinding usrBinding);

    protected UsrFragmentBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, LinearLayout linearLayout, LinearLayout linearLayout2, AppCompatSpinner appCompatSpinner2, EditText editText, EditText editText2, LinearLayout linearLayout3, TextView textView, TextView textView2, EditText editText3, TextView textView3, EditText editText4, SeekBar seekBar, AppCompatSpinner appCompatSpinner3, LinearLayout linearLayout4, LinearLayout linearLayout5, EditText editText5, LinearLayout linearLayout6, EditText editText6, EditText editText7, EditText editText8, EditText editText9, LinearLayout linearLayout7, AppCompatSpinner appCompatSpinner4, RadioButton radioButton, TextView textView4, TextView textView5, EditText editText10, LinearLayout linearLayout8, TextView textView6, TextView textView7, SeekBar seekBar2, LinearLayout linearLayout9, LinearLayout linearLayout10, LinearLayout linearLayout11, TextView textView8, LinearLayout linearLayout12, AppCompatSpinner appCompatSpinner5, EditText editText11, LinearLayout linearLayout13, EditText editText12, LinearLayout linearLayout14, Button button, EditText editText13, EditText editText14, EditText editText15, LinearLayout linearLayout15, EditText editText16, AppCompatSpinner appCompatSpinner6, AppCompatSpinner appCompatSpinner7, RadioGroup radioGroup) {
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
        this.maxValueLayout = linearLayout5;
        this.maxVel = editText5;
        this.minValueLayout = linearLayout6;
        this.minVel = editText6;
        this.modCCVal = editText7;
        this.modMaxVal = editText8;
        this.modMinVal = editText9;
        this.mps = linearLayout7;
        this.octive = appCompatSpinner4;
        this.padBtn = radioButton;
        this.padMaxText = textView4;
        this.padMinText = textView5;
        this.padNote = editText10;
        this.padNoteLayout = linearLayout8;
        this.padNoteStr = textView6;
        this.padNoteText = textView7;
        this.padSeekbar = seekBar2;
        this.pads = linearLayout9;
        this.ppr = linearLayout10;
        this.psrCCLayout = linearLayout11;
        this.psrCCStr = textView8;
        this.psrChannelLayout = linearLayout12;
        this.psrMode = appCompatSpinner5;
        this.psrValue1 = editText11;
        this.psrValue1Layout = linearLayout13;
        this.psrValue2 = editText12;
        this.psrValue2Layout = linearLayout14;
        this.selectColorBtn = button;
        this.sustainCCVal = editText13;
        this.sustainDownVal = editText14;
        this.sustainUpVal = editText15;
        this.sysExLayout = linearLayout15;
        this.sysExValue = editText16;
        this.transpose = appCompatSpinner6;
        this.type = appCompatSpinner7;
        this.typeRadio = radioGroup;
    }

    public Usr getData() {
        return this.mData;
    }

    public UsrBinding getUsrBinding() {
        return this.mUsrBinding;
    }

    public MidikeyboardUsrFragment getListener() {
        return this.mListener;
    }

    public static UsrFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static UsrFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (UsrFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.usr_fragment, viewGroup, z, obj);
    }

    public static UsrFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static UsrFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (UsrFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.usr_fragment, null, false, obj);
    }

    public static UsrFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static UsrFragmentBinding bind(View view, Object obj) {
        return (UsrFragmentBinding) bind(obj, view, R.layout.usr_fragment);
    }
}
