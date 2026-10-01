package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioGroup;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.Bindable;
import androidx.databinding.DataBindingUtil;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.entity.smk25Mini.Smk25MiniUsr;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniUsrFragment;

/* JADX INFO: loaded from: classes.dex */
public abstract class Smk25miniUsrFragmentBinding extends ViewDataBinding {
    public final AppCompatSpinner keyCurve;
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
    public final AppCompatSpinner knobType;
    public final LinearLayout knobs;

    @Bindable
    protected Smk25MiniUsr mData;

    @Bindable
    protected Smk25MiniGlob mGlobData;

    @Bindable
    protected Smk25MiniUsrFragment mListener;

    @Bindable
    protected UsrBinding mUsrBinding;
    public final EditText modCCVal;
    public final EditText modMaxVal;
    public final EditText modMinVal;
    public final AppCompatSpinner modSpeed;
    public final LinearLayout mps;
    public final AppCompatSpinner octive;
    public final LinearLayout pedalCC;
    public final LinearLayout pedalCh;
    public final LinearLayout pedalDown;
    public final LinearLayout pedalType;
    public final LinearLayout pedalUp;
    public final AppCompatSpinner pitchSpeed;
    public final AppCompatSpinner port;
    public final LinearLayout ppr;
    public final LinearLayout psrCCLayout;
    public final TextView psrCCStr;
    public final LinearLayout psrChannelLayout;
    public final AppCompatSpinner psrMode;
    public final EditText psrValue1;
    public final LinearLayout psrValue1Layout;
    public final EditText psrValue2;
    public final LinearLayout psrValue2Layout;
    public final EditText sustainCCVal;
    public final EditText sustainDownVal;
    public final EditText sustainUpVal;
    public final LinearLayout sysExLayout;
    public final EditText sysExValue;
    public final AppCompatSpinner transpose;
    public final RadioGroup typeRadio;

    public abstract void setData(Smk25MiniUsr smk25MiniUsr);

    public abstract void setGlobData(Smk25MiniGlob smk25MiniGlob);

    public abstract void setListener(Smk25MiniUsrFragment smk25MiniUsrFragment);

    public abstract void setUsrBinding(UsrBinding usrBinding);

    protected Smk25miniUsrFragmentBinding(Object obj, View view, int i, AppCompatSpinner appCompatSpinner, LinearLayout linearLayout, AppCompatSpinner appCompatSpinner2, EditText editText, EditText editText2, LinearLayout linearLayout2, TextView textView, TextView textView2, EditText editText3, TextView textView3, EditText editText4, AppCompatSpinner appCompatSpinner3, LinearLayout linearLayout3, EditText editText5, EditText editText6, EditText editText7, AppCompatSpinner appCompatSpinner4, LinearLayout linearLayout4, AppCompatSpinner appCompatSpinner5, LinearLayout linearLayout5, LinearLayout linearLayout6, LinearLayout linearLayout7, LinearLayout linearLayout8, LinearLayout linearLayout9, AppCompatSpinner appCompatSpinner6, AppCompatSpinner appCompatSpinner7, LinearLayout linearLayout10, LinearLayout linearLayout11, TextView textView4, LinearLayout linearLayout12, AppCompatSpinner appCompatSpinner8, EditText editText8, LinearLayout linearLayout13, EditText editText9, LinearLayout linearLayout14, EditText editText10, EditText editText11, EditText editText12, LinearLayout linearLayout15, EditText editText13, AppCompatSpinner appCompatSpinner9, RadioGroup radioGroup) {
        super(obj, view, i);
        this.keyCurve = appCompatSpinner;
        this.keybed = linearLayout;
        this.keybedChannel = appCompatSpinner2;
        this.keybedMaxVel = editText;
        this.keybedMinVel = editText2;
        this.knobCCLayout = linearLayout2;
        this.knobCCStr = textView;
        this.knobMaxText = textView2;
        this.knobMaxVal = editText3;
        this.knobMinText = textView3;
        this.knobMinVal = editText4;
        this.knobType = appCompatSpinner3;
        this.knobs = linearLayout3;
        this.modCCVal = editText5;
        this.modMaxVal = editText6;
        this.modMinVal = editText7;
        this.modSpeed = appCompatSpinner4;
        this.mps = linearLayout4;
        this.octive = appCompatSpinner5;
        this.pedalCC = linearLayout5;
        this.pedalCh = linearLayout6;
        this.pedalDown = linearLayout7;
        this.pedalType = linearLayout8;
        this.pedalUp = linearLayout9;
        this.pitchSpeed = appCompatSpinner6;
        this.port = appCompatSpinner7;
        this.ppr = linearLayout10;
        this.psrCCLayout = linearLayout11;
        this.psrCCStr = textView4;
        this.psrChannelLayout = linearLayout12;
        this.psrMode = appCompatSpinner8;
        this.psrValue1 = editText8;
        this.psrValue1Layout = linearLayout13;
        this.psrValue2 = editText9;
        this.psrValue2Layout = linearLayout14;
        this.sustainCCVal = editText10;
        this.sustainDownVal = editText11;
        this.sustainUpVal = editText12;
        this.sysExLayout = linearLayout15;
        this.sysExValue = editText13;
        this.transpose = appCompatSpinner9;
        this.typeRadio = radioGroup;
    }

    public Smk25MiniUsr getData() {
        return this.mData;
    }

    public Smk25MiniGlob getGlobData() {
        return this.mGlobData;
    }

    public Smk25MiniUsrFragment getListener() {
        return this.mListener;
    }

    public UsrBinding getUsrBinding() {
        return this.mUsrBinding;
    }

    public static Smk25miniUsrFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        return inflate(layoutInflater, viewGroup, z, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniUsrFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z, Object obj) {
        return (Smk25miniUsrFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_usr_fragment, viewGroup, z, obj);
    }

    public static Smk25miniUsrFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniUsrFragmentBinding inflate(LayoutInflater layoutInflater, Object obj) {
        return (Smk25miniUsrFragmentBinding) ViewDataBinding.inflateInternal(layoutInflater, R.layout.smk25mini_usr_fragment, null, false, obj);
    }

    public static Smk25miniUsrFragmentBinding bind(View view) {
        return bind(view, DataBindingUtil.getDefaultComponent());
    }

    @Deprecated
    public static Smk25miniUsrFragmentBinding bind(View view, Object obj) {
        return (Smk25miniUsrFragmentBinding) bind(obj, view, R.layout.smk25mini_usr_fragment);
    }
}
