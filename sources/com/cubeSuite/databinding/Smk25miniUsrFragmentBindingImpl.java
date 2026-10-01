package com.cubeSuite.databinding;

import android.text.Editable;
import android.util.SparseIntArray;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.AdapterViewBindingAdapter;
import androidx.databinding.adapters.RadioGroupBindingAdapter;
import androidx.databinding.adapters.TextViewBindingAdapter;
import com.cubeSuite.R;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.BtnPara;
import com.cubeSuite.entity.midikeyboard.EncoderPara;
import com.cubeSuite.entity.midikeyboard.PedalPara;
import com.cubeSuite.entity.midikeyboard.Sys;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.entity.smk25Mini.Smk25MiniKeyPara;
import com.cubeSuite.entity.smk25Mini.Smk25MiniMod;
import com.cubeSuite.entity.smk25Mini.Smk25MiniPitch;
import com.cubeSuite.entity.smk25Mini.Smk25MiniUsr;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniUsrFragment;
import com.cubeSuite.generated.callback.AfterTextChanged;
import com.cubeSuite.generated.callback.OnCheckedChangeListener;
import com.cubeSuite.generated.callback.OnClickListener;
import com.cubeSuite.generated.callback.OnItemSelected;
import kotlinx.coroutines.internal.LockFreeTaskQueueCore;
import okhttp3.internal.http2.Http2Stream;
import okio.SegmentPool;

/* JADX INFO: loaded from: classes.dex */
public class Smk25miniUsrFragmentBindingImpl extends Smk25miniUsrFragmentBinding implements OnItemSelected.Listener, AfterTextChanged.Listener, OnCheckedChangeListener.Listener, OnClickListener.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private final RadioGroup.OnCheckedChangeListener mCallback48;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback49;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback50;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback51;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback52;
    private final TextViewBindingAdapter.AfterTextChanged mCallback53;
    private final TextViewBindingAdapter.AfterTextChanged mCallback54;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback55;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback56;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback57;
    private final TextViewBindingAdapter.AfterTextChanged mCallback58;
    private final TextViewBindingAdapter.AfterTextChanged mCallback59;
    private final TextViewBindingAdapter.AfterTextChanged mCallback60;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback61;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback62;
    private final TextViewBindingAdapter.AfterTextChanged mCallback63;
    private final TextViewBindingAdapter.AfterTextChanged mCallback64;
    private final TextViewBindingAdapter.AfterTextChanged mCallback65;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback66;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback67;
    private final View.OnClickListener mCallback68;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback69;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback70;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback71;
    private final TextViewBindingAdapter.AfterTextChanged mCallback72;
    private final TextViewBindingAdapter.AfterTextChanged mCallback73;
    private final TextViewBindingAdapter.AfterTextChanged mCallback74;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback75;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback76;
    private final TextViewBindingAdapter.AfterTextChanged mCallback77;
    private final TextViewBindingAdapter.AfterTextChanged mCallback78;
    private final TextViewBindingAdapter.AfterTextChanged mCallback79;
    private final TextViewBindingAdapter.AfterTextChanged mCallback80;
    private long mDirtyFlags;
    private long mDirtyFlags_1;
    private final ScrollView mboundView0;
    private final AppCompatSpinner mboundView10;
    private final EditText mboundView11;
    private final AppCompatSpinner mboundView17;
    private final TextView mboundView20;
    private final AppCompatSpinner mboundView23;
    private final Button mboundView25;
    private final AppCompatSpinner mboundView27;
    private final AppCompatSpinner mboundView28;
    private final AppCompatSpinner mboundView33;
    private final TextView mboundView34;
    private final EditText mboundView35;
    private final TextView mboundView37;
    private final TextView mboundView39;
    private final AppCompatSpinner mboundView9;

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.keybed, 42);
        sparseIntArray.put(R.id.knobs, 43);
        sparseIntArray.put(R.id.knobCCLayout, 44);
        sparseIntArray.put(R.id.mps, 45);
        sparseIntArray.put(R.id.pedalType, 46);
        sparseIntArray.put(R.id.pedalCh, 47);
        sparseIntArray.put(R.id.pedalCC, 48);
        sparseIntArray.put(R.id.pedalUp, 49);
        sparseIntArray.put(R.id.pedalDown, 50);
        sparseIntArray.put(R.id.ppr, 51);
        sparseIntArray.put(R.id.psrChannelLayout, 52);
        sparseIntArray.put(R.id.psrCCLayout, 53);
        sparseIntArray.put(R.id.psrValue1Layout, 54);
        sparseIntArray.put(R.id.psrValue2Layout, 55);
        sparseIntArray.put(R.id.sysExLayout, 56);
    }

    public Smk25miniUsrFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 57, sIncludes, sViewsWithIds));
    }

    private Smk25miniUsrFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 31, (AppCompatSpinner) objArr[3], (LinearLayout) objArr[42], (AppCompatSpinner) objArr[2], (EditText) objArr[7], (EditText) objArr[6], (LinearLayout) objArr[44], (TextView) objArr[12], (TextView) objArr[15], (EditText) objArr[16], (TextView) objArr[13], (EditText) objArr[14], (AppCompatSpinner) objArr[8], (LinearLayout) objArr[43], (EditText) objArr[19], (EditText) objArr[22], (EditText) objArr[21], (AppCompatSpinner) objArr[18], (LinearLayout) objArr[45], (AppCompatSpinner) objArr[4], (LinearLayout) objArr[48], (LinearLayout) objArr[47], (LinearLayout) objArr[50], (LinearLayout) objArr[46], (LinearLayout) objArr[49], (AppCompatSpinner) objArr[24], (AppCompatSpinner) objArr[26], (LinearLayout) objArr[51], (LinearLayout) objArr[53], (TextView) objArr[36], (LinearLayout) objArr[52], (AppCompatSpinner) objArr[32], (EditText) objArr[38], (LinearLayout) objArr[54], (EditText) objArr[40], (LinearLayout) objArr[55], (EditText) objArr[29], (EditText) objArr[31], (EditText) objArr[30], (LinearLayout) objArr[56], (EditText) objArr[41], (AppCompatSpinner) objArr[5], (RadioGroup) objArr[1]);
        this.mDirtyFlags = -1L;
        this.mDirtyFlags_1 = -1L;
        this.keyCurve.setTag(null);
        this.keybedChannel.setTag(null);
        this.keybedMaxVel.setTag(null);
        this.keybedMinVel.setTag(null);
        this.knobCCStr.setTag(null);
        this.knobMaxText.setTag(null);
        this.knobMaxVal.setTag(null);
        this.knobMinText.setTag(null);
        this.knobMinVal.setTag(null);
        this.knobType.setTag(null);
        ScrollView scrollView = (ScrollView) objArr[0];
        this.mboundView0 = scrollView;
        scrollView.setTag(null);
        AppCompatSpinner appCompatSpinner = (AppCompatSpinner) objArr[10];
        this.mboundView10 = appCompatSpinner;
        appCompatSpinner.setTag(null);
        EditText editText = (EditText) objArr[11];
        this.mboundView11 = editText;
        editText.setTag(null);
        AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) objArr[17];
        this.mboundView17 = appCompatSpinner2;
        appCompatSpinner2.setTag(null);
        TextView textView = (TextView) objArr[20];
        this.mboundView20 = textView;
        textView.setTag(null);
        AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) objArr[23];
        this.mboundView23 = appCompatSpinner3;
        appCompatSpinner3.setTag(null);
        Button button = (Button) objArr[25];
        this.mboundView25 = button;
        button.setTag(null);
        AppCompatSpinner appCompatSpinner4 = (AppCompatSpinner) objArr[27];
        this.mboundView27 = appCompatSpinner4;
        appCompatSpinner4.setTag(null);
        AppCompatSpinner appCompatSpinner5 = (AppCompatSpinner) objArr[28];
        this.mboundView28 = appCompatSpinner5;
        appCompatSpinner5.setTag(null);
        AppCompatSpinner appCompatSpinner6 = (AppCompatSpinner) objArr[33];
        this.mboundView33 = appCompatSpinner6;
        appCompatSpinner6.setTag(null);
        TextView textView2 = (TextView) objArr[34];
        this.mboundView34 = textView2;
        textView2.setTag(null);
        EditText editText2 = (EditText) objArr[35];
        this.mboundView35 = editText2;
        editText2.setTag(null);
        TextView textView3 = (TextView) objArr[37];
        this.mboundView37 = textView3;
        textView3.setTag(null);
        TextView textView4 = (TextView) objArr[39];
        this.mboundView39 = textView4;
        textView4.setTag(null);
        AppCompatSpinner appCompatSpinner7 = (AppCompatSpinner) objArr[9];
        this.mboundView9 = appCompatSpinner7;
        appCompatSpinner7.setTag(null);
        this.modCCVal.setTag(null);
        this.modMaxVal.setTag(null);
        this.modMinVal.setTag(null);
        this.modSpeed.setTag(null);
        this.octive.setTag(null);
        this.pitchSpeed.setTag(null);
        this.port.setTag(null);
        this.psrCCStr.setTag(null);
        this.psrMode.setTag(null);
        this.psrValue1.setTag(null);
        this.psrValue2.setTag(null);
        this.sustainCCVal.setTag(null);
        this.sustainDownVal.setTag(null);
        this.sustainUpVal.setTag(null);
        this.sysExValue.setTag(null);
        this.transpose.setTag(null);
        this.typeRadio.setTag(null);
        setRootTag(view);
        this.mCallback70 = new OnItemSelected(this, 23);
        this.mCallback71 = new OnItemSelected(this, 24);
        this.mCallback49 = new OnItemSelected(this, 2);
        this.mCallback80 = new AfterTextChanged(this, 33);
        this.mCallback59 = new AfterTextChanged(this, 12);
        this.mCallback48 = new OnCheckedChangeListener(this, 1);
        this.mCallback69 = new OnItemSelected(this, 22);
        this.mCallback57 = new OnItemSelected(this, 10);
        this.mCallback58 = new AfterTextChanged(this, 11);
        this.mCallback67 = new OnItemSelected(this, 20);
        this.mCallback79 = new AfterTextChanged(this, 32);
        this.mCallback55 = new OnItemSelected(this, 8);
        this.mCallback56 = new OnItemSelected(this, 9);
        this.mCallback68 = new OnClickListener(this, 21);
        this.mCallback65 = new AfterTextChanged(this, 18);
        this.mCallback77 = new AfterTextChanged(this, 30);
        this.mCallback52 = new OnItemSelected(this, 5);
        this.mCallback64 = new AfterTextChanged(this, 17);
        this.mCallback78 = new AfterTextChanged(this, 31);
        this.mCallback54 = new AfterTextChanged(this, 7);
        this.mCallback66 = new OnItemSelected(this, 19);
        this.mCallback53 = new AfterTextChanged(this, 6);
        this.mCallback74 = new AfterTextChanged(this, 27);
        this.mCallback50 = new OnItemSelected(this, 3);
        this.mCallback62 = new OnItemSelected(this, 15);
        this.mCallback76 = new OnItemSelected(this, 29);
        this.mCallback63 = new AfterTextChanged(this, 16);
        this.mCallback75 = new OnItemSelected(this, 28);
        this.mCallback51 = new OnItemSelected(this, 4);
        this.mCallback72 = new AfterTextChanged(this, 25);
        this.mCallback60 = new AfterTextChanged(this, 13);
        this.mCallback61 = new OnItemSelected(this, 14);
        this.mCallback73 = new AfterTextChanged(this, 26);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 0L;
            this.mDirtyFlags_1 = 256L;
        }
        requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean hasPendingBindings() {
        synchronized (this) {
            if (this.mDirtyFlags == 0 && this.mDirtyFlags_1 == 0) {
                return false;
            }
            return true;
        }
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean setVariable(int i, Object obj) {
        if (9 == i) {
            setListener((Smk25MiniUsrFragment) obj);
            return true;
        }
        if (3 == i) {
            setGlobData((Smk25MiniGlob) obj);
            return true;
        }
        if (2 == i) {
            setData((Smk25MiniUsr) obj);
            return true;
        }
        if (21 != i) {
            return false;
        }
        setUsrBinding((UsrBinding) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25miniUsrFragmentBinding
    public void setListener(Smk25MiniUsrFragment smk25MiniUsrFragment) {
        this.mListener = smk25MiniUsrFragment;
        synchronized (this) {
            this.mDirtyFlags |= 2147483648L;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniUsrFragmentBinding
    public void setGlobData(Smk25MiniGlob smk25MiniGlob) {
        this.mGlobData = smk25MiniGlob;
        synchronized (this) {
            this.mDirtyFlags |= 4294967296L;
        }
        notifyPropertyChanged(3);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniUsrFragmentBinding
    public void setData(Smk25MiniUsr smk25MiniUsr) {
        this.mData = smk25MiniUsr;
        synchronized (this) {
            this.mDirtyFlags |= 8589934592L;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniUsrFragmentBinding
    public void setUsrBinding(UsrBinding usrBinding) {
        updateRegistration(30, usrBinding);
        this.mUsrBinding = usrBinding;
        synchronized (this) {
            this.mDirtyFlags |= 1073741824;
        }
        notifyPropertyChanged(21);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        switch (i) {
            case 0:
                return onChangeDataModParaDat2((AddrU8) obj, i2);
            case 1:
                return onChangeDataModParaSpeed((AddrU8) obj, i2);
            case 2:
                return onChangeDataKeyParaMax((AddrU8) obj, i2);
            case 3:
                return onChangeDataEncoderParaDat0((AddrU8) obj, i2);
            case 4:
                return onChangeDataEncoderParaCh((AddrU8) obj, i2);
            case 5:
                return onChangeDataEncoderParaDat1((AddrU8) obj, i2);
            case 6:
                return onChangeDataBtnParaDat1((AddrU8) obj, i2);
            case 7:
                return onChangeDataEncoderParaCurve((AddrU8) obj, i2);
            case 8:
                return onChangeDataPedalParaDat2((AddrU8) obj, i2);
            case 9:
                return onChangeDataKeyParaMin((AddrU8) obj, i2);
            case 10:
                return onChangeGlobDataInputMode((AddrU8) obj, i2);
            case 11:
                return onChangeDataKeyParaCh((AddrU8) obj, i2);
            case 12:
                return onChangeGlobDataKeyCurve((AddrU8) obj, i2);
            case 13:
                return onChangeDataModParaDat1((AddrU8) obj, i2);
            case 14:
                return onChangeDataBtnParaCh((AddrU8) obj, i2);
            case 15:
                return onChangeDataEncoderParaDat2((AddrU8) obj, i2);
            case 16:
                return onChangeDataBtnParaDat2((AddrU8) obj, i2);
            case 17:
                return onChangeDataEncoderParaType((AddrU8) obj, i2);
            case 18:
                return onChangeDataKeyParaOctave((AddrI8) obj, i2);
            case 19:
                return onChangeDataPedalParaDat1((AddrU8) obj, i2);
            case 20:
                return onChangeDataModParaDat0((AddrU8) obj, i2);
            case 21:
                return onChangeDataBtnParaType((AddrU8) obj, i2);
            case 22:
                return onChangeDataModParaCh((AddrU8) obj, i2);
            case 23:
                return onChangeDataPitchParaSpeed((AddrU8) obj, i2);
            case 24:
                return onChangeDataPedalParaType((AddrU8) obj, i2);
            case 25:
                return onChangeDataKeyParaTranspose((AddrI8) obj, i2);
            case 26:
                return onChangeDataBtnParaDat0((AddrU8) obj, i2);
            case 27:
                return onChangeDataPedalParaDat0((AddrU8) obj, i2);
            case 28:
                return onChangeDataPitchParaCh((AddrU8) obj, i2);
            case 29:
                return onChangeDataPedalParaCh((AddrU8) obj, i2);
            case 30:
                return onChangeUsrBinding((UsrBinding) obj, i2);
            default:
                return false;
        }
    }

    private boolean onChangeDataModParaDat2(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 17179869184L;
        }
        return true;
    }

    private boolean onChangeDataModParaSpeed(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 2;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 34359738368L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaMax(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 4;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 68719476736L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaDat0(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 8;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 137438953472L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 16;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 274877906944L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaDat1(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 32;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 549755813888L;
        }
        return true;
    }

    private boolean onChangeDataBtnParaDat1(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 64;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 1099511627776L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaCurve(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 128;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 2199023255552L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat2(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 256;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 4398046511104L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaMin(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 512;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 8796093022208L;
        }
        return true;
    }

    private boolean onChangeGlobDataInputMode(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1024;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 17592186044416L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 2048;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 35184372088832L;
        }
        return true;
    }

    private boolean onChangeGlobDataKeyCurve(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 4096;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 70368744177664L;
        }
        return true;
    }

    private boolean onChangeDataModParaDat1(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 8192;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 140737488355328L;
        }
        return true;
    }

    private boolean onChangeDataBtnParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= Http2Stream.EMIT_BUFFER_SIZE;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 281474976710656L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaDat2(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 32768;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 562949953421312L;
        }
        return true;
    }

    private boolean onChangeDataBtnParaDat2(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= SegmentPool.MAX_SIZE;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 1125899906842624L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaType(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 131072;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 2251799813685248L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaOctave(AddrI8 addrI8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 262144;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 4503599627370496L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat1(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 524288;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 9007199254740992L;
        }
        return true;
    }

    private boolean onChangeDataModParaDat0(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1048576;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 18014398509481984L;
        }
        return true;
    }

    private boolean onChangeDataBtnParaType(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 2097152;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 36028797018963968L;
        }
        return true;
    }

    private boolean onChangeDataModParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 4194304;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 72057594037927936L;
        }
        return true;
    }

    private boolean onChangeDataPitchParaSpeed(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 8388608;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 144115188075855872L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaType(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 16777216;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 288230376151711744L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaTranspose(AddrI8 addrI8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 33554432;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 576460752303423488L;
        }
        return true;
    }

    private boolean onChangeDataBtnParaDat0(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 67108864;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= LockFreeTaskQueueCore.FROZEN_MASK;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat0(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 134217728;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= LockFreeTaskQueueCore.CLOSED_MASK;
        }
        return true;
    }

    private boolean onChangeDataPitchParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 268435456;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 4611686018427387904L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 536870912;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= Long.MIN_VALUE;
        }
        return true;
    }

    private boolean onChangeUsrBinding(UsrBinding usrBinding, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1073741824;
            }
            return true;
        }
        if (i == 5) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 1;
            }
            return true;
        }
        if (i == 8) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 2;
            }
            return true;
        }
        if (i == 7) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 4;
            }
            return true;
        }
        if (i == 10) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 8;
            }
            return true;
        }
        if (i == 17) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 16;
            }
            return true;
        }
        if (i == 16) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 32;
            }
            return true;
        }
        if (i == 19) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 64;
            }
            return true;
        }
        if (i != 20) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags_1 |= 128;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:145:0x0263 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:146:0x0265  */
    /* JADX WARN: Code duplicated, block: B:147:0x026a  */
    /* JADX WARN: Code duplicated, block: B:150:0x0273  */
    /* JADX WARN: Code duplicated, block: B:153:0x0284  */
    /* JADX WARN: Code duplicated, block: B:158:0x029b A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:159:0x029d  */
    /* JADX WARN: Code duplicated, block: B:160:0x02a2  */
    /* JADX WARN: Code duplicated, block: B:163:0x02af  */
    /* JADX WARN: Code duplicated, block: B:168:0x02c2 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:169:0x02c4  */
    /* JADX WARN: Code duplicated, block: B:170:0x02cb  */
    /* JADX WARN: Code duplicated, block: B:173:0x02d8  */
    /* JADX WARN: Code duplicated, block: B:177:0x02e6  */
    /* JADX WARN: Code duplicated, block: B:179:0x02ea  */
    /* JADX WARN: Code duplicated, block: B:180:0x02f4  */
    /* JADX WARN: Code duplicated, block: B:183:0x02fe  */
    /* JADX WARN: Code duplicated, block: B:184:0x0303  */
    /* JADX WARN: Code duplicated, block: B:188:0x0313  */
    /* JADX WARN: Code duplicated, block: B:192:0x031f A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:193:0x0321  */
    /* JADX WARN: Code duplicated, block: B:194:0x032b  */
    /* JADX WARN: Code duplicated, block: B:197:0x0336  */
    /* JADX WARN: Code duplicated, block: B:198:0x033b  */
    /* JADX WARN: Code duplicated, block: B:202:0x0346  */
    /* JADX WARN: Code duplicated, block: B:207:0x0356  */
    /* JADX WARN: Code duplicated, block: B:208:0x035e  */
    /* JADX WARN: Code duplicated, block: B:211:0x0367  */
    /* JADX WARN: Code duplicated, block: B:212:0x036c  */
    /* JADX WARN: Code duplicated, block: B:215:0x0378  */
    /* JADX WARN: Code duplicated, block: B:219:0x0388  */
    /* JADX WARN: Code duplicated, block: B:222:0x039e  */
    /* JADX WARN: Code duplicated, block: B:226:0x03aa A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:227:0x03ac  */
    /* JADX WARN: Code duplicated, block: B:228:0x03b7  */
    /* JADX WARN: Code duplicated, block: B:231:0x03c4  */
    /* JADX WARN: Code duplicated, block: B:232:0x03c9 A[PHI: r55 r56
  0x03c9: PHI (r55v14 java.lang.String) = (r2v27 java.lang.String), (r55v17 java.lang.String) binds: [B:230:0x03c2, B:225:0x03a5] A[DONT_GENERATE, DONT_INLINE]
  0x03c9: PHI (r56v3 java.lang.String) = (r56v0 java.lang.String), (r56v4 java.lang.String) binds: [B:230:0x03c2, B:225:0x03a5] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:234:0x03cc  */
    /* JADX WARN: Code duplicated, block: B:236:0x03d2 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:237:0x03d4  */
    /* JADX WARN: Code duplicated, block: B:238:0x03d9  */
    /* JADX WARN: Code duplicated, block: B:240:0x03dd  */
    /* JADX WARN: Code duplicated, block: B:241:0x03e2  */
    /* JADX WARN: Code duplicated, block: B:244:0x03ef  */
    /* JADX WARN: Code duplicated, block: B:246:0x03f5 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:247:0x03f7  */
    /* JADX WARN: Code duplicated, block: B:248:0x03fc  */
    /* JADX WARN: Code duplicated, block: B:251:0x0405  */
    /* JADX WARN: Code duplicated, block: B:252:0x0416  */
    /* JADX WARN: Code duplicated, block: B:255:0x042d  */
    /* JADX WARN: Code duplicated, block: B:259:0x0448 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:260:0x044a  */
    /* JADX WARN: Code duplicated, block: B:261:0x044f  */
    /* JADX WARN: Code duplicated, block: B:264:0x045c  */
    /* JADX WARN: Code duplicated, block: B:268:0x0468 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:269:0x046a  */
    /* JADX WARN: Code duplicated, block: B:270:0x0475  */
    /* JADX WARN: Code duplicated, block: B:273:0x0481  */
    /* JADX WARN: Code duplicated, block: B:274:0x0486 A[PHI: r57 r58
  0x0486: PHI (r57v12 java.lang.String) = (r0v5 java.lang.String), (r57v15 java.lang.String) binds: [B:272:0x047f, B:267:0x0463] A[DONT_GENERATE, DONT_INLINE]
  0x0486: PHI (r58v3 java.lang.String) = (r58v0 java.lang.String), (r58v4 java.lang.String) binds: [B:272:0x047f, B:267:0x0463] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:277:0x0492  */
    /* JADX WARN: Code duplicated, block: B:281:0x049c A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:282:0x049e  */
    /* JADX WARN: Code duplicated, block: B:283:0x04a3  */
    /* JADX WARN: Code duplicated, block: B:286:0x04ad  */
    /* JADX WARN: Code duplicated, block: B:287:0x04b2 A[PHI: r59
  0x04b2: PHI (r59v4 int) = (r59v2 int), (r59v6 int) binds: [B:285:0x04ab, B:280:0x0499] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:290:0x04be  */
    /* JADX WARN: Code duplicated, block: B:294:0x04d1 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:295:0x04d3  */
    /* JADX WARN: Code duplicated, block: B:296:0x04d8  */
    /* JADX WARN: Code duplicated, block: B:299:0x04e0  */
    /* JADX WARN: Code duplicated, block: B:29:0x007b A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:303:0x04ef  */
    /* JADX WARN: Code duplicated, block: B:305:0x04f3  */
    /* JADX WARN: Code duplicated, block: B:306:0x04fd  */
    /* JADX WARN: Code duplicated, block: B:309:0x0507  */
    /* JADX WARN: Code duplicated, block: B:30:0x007d  */
    /* JADX WARN: Code duplicated, block: B:310:0x050c  */
    /* JADX WARN: Code duplicated, block: B:313:0x0518  */
    /* JADX WARN: Code duplicated, block: B:317:0x0522  */
    /* JADX WARN: Code duplicated, block: B:31:0x0082  */
    /* JADX WARN: Code duplicated, block: B:320:0x0531  */
    /* JADX WARN: Code duplicated, block: B:324:0x053f  */
    /* JADX WARN: Code duplicated, block: B:326:0x0543  */
    /* JADX WARN: Code duplicated, block: B:327:0x054d  */
    /* JADX WARN: Code duplicated, block: B:330:0x0557  */
    /* JADX WARN: Code duplicated, block: B:331:0x055c  */
    /* JADX WARN: Code duplicated, block: B:335:0x056c  */
    /* JADX WARN: Code duplicated, block: B:340:0x0578 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:341:0x057a  */
    /* JADX WARN: Code duplicated, block: B:342:0x0582  */
    /* JADX WARN: Code duplicated, block: B:345:0x058b  */
    /* JADX WARN: Code duplicated, block: B:346:0x0590  */
    /* JADX WARN: Code duplicated, block: B:34:0x0088  */
    /* JADX WARN: Code duplicated, block: B:350:0x05a1  */
    /* JADX WARN: Code duplicated, block: B:354:0x05ad A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:355:0x05af  */
    /* JADX WARN: Code duplicated, block: B:356:0x05ba  */
    /* JADX WARN: Code duplicated, block: B:359:0x05c7  */
    /* JADX WARN: Code duplicated, block: B:35:0x008d  */
    /* JADX WARN: Code duplicated, block: B:360:0x05d3 A[PHI: r56 r63
  0x05d3: PHI (r56v15 java.lang.String) = (r0v33 java.lang.String), (r56v17 java.lang.String) binds: [B:358:0x05c5, B:353:0x05a8] A[DONT_GENERATE, DONT_INLINE]
  0x05d3: PHI (r63v10 java.lang.String) = (r63v8 java.lang.String), (r63v13 java.lang.String) binds: [B:358:0x05c5, B:353:0x05a8] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:363:0x05e4  */
    /* JADX WARN: Code duplicated, block: B:367:0x05fb A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:368:0x05fd  */
    /* JADX WARN: Code duplicated, block: B:369:0x0602  */
    /* JADX WARN: Code duplicated, block: B:372:0x060f  */
    /* JADX WARN: Code duplicated, block: B:376:0x061b A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:377:0x061d  */
    /* JADX WARN: Code duplicated, block: B:378:0x0628  */
    /* JADX WARN: Code duplicated, block: B:381:0x0635  */
    /* JADX WARN: Code duplicated, block: B:382:0x063a A[PHI: r64 r65
  0x063a: PHI (r64v11 java.lang.String) = (r0v43 java.lang.String), (r64v14 java.lang.String) binds: [B:380:0x0633, B:375:0x0616] A[DONT_GENERATE, DONT_INLINE]
  0x063a: PHI (r65v3 java.lang.String) = (r65v0 java.lang.String), (r65v4 java.lang.String) binds: [B:380:0x0633, B:375:0x0616] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:385:0x0646  */
    /* JADX WARN: Code duplicated, block: B:389:0x0657 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:390:0x0659  */
    /* JADX WARN: Code duplicated, block: B:391:0x065e  */
    /* JADX WARN: Code duplicated, block: B:394:0x066b  */
    /* JADX WARN: Code duplicated, block: B:398:0x0679  */
    /* JADX WARN: Code duplicated, block: B:400:0x067d  */
    /* JADX WARN: Code duplicated, block: B:401:0x0687  */
    /* JADX WARN: Code duplicated, block: B:404:0x0692  */
    /* JADX WARN: Code duplicated, block: B:405:0x0697  */
    /* JADX WARN: Code duplicated, block: B:409:0x06a7  */
    /* JADX WARN: Code duplicated, block: B:413:0x06b3 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:414:0x06b5  */
    /* JADX WARN: Code duplicated, block: B:415:0x06bf  */
    /* JADX WARN: Code duplicated, block: B:418:0x06ca  */
    /* JADX WARN: Code duplicated, block: B:419:0x06cf  */
    /* JADX WARN: Code duplicated, block: B:423:0x06df  */
    /* JADX WARN: Code duplicated, block: B:428:0x06ec A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:429:0x06ee  */
    /* JADX WARN: Code duplicated, block: B:430:0x06f6  */
    /* JADX WARN: Code duplicated, block: B:433:0x06ff  */
    /* JADX WARN: Code duplicated, block: B:434:0x0704  */
    /* JADX WARN: Code duplicated, block: B:438:0x0717  */
    /* JADX WARN: Code duplicated, block: B:442:0x0723 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:443:0x0725  */
    /* JADX WARN: Code duplicated, block: B:444:0x0730  */
    /* JADX WARN: Code duplicated, block: B:447:0x073d  */
    /* JADX WARN: Code duplicated, block: B:448:0x0749 A[PHI: r63 r68
  0x0749: PHI (r63v24 java.lang.String) = (r0v67 java.lang.String), (r63v26 java.lang.String) binds: [B:446:0x073b, B:441:0x071e] A[DONT_GENERATE, DONT_INLINE]
  0x0749: PHI (r68v13 java.lang.String) = (r68v11 java.lang.String), (r68v16 java.lang.String) binds: [B:446:0x073b, B:441:0x071e] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:451:0x075a  */
    /* JADX WARN: Code duplicated, block: B:455:0x07a5 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:456:0x07a7  */
    /* JADX WARN: Code duplicated, block: B:457:0x07ac  */
    /* JADX WARN: Code duplicated, block: B:460:0x07b9  */
    /* JADX WARN: Code duplicated, block: B:464:0x07c5 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:465:0x07c7  */
    /* JADX WARN: Code duplicated, block: B:466:0x07d2  */
    /* JADX WARN: Code duplicated, block: B:469:0x07de  */
    /* JADX WARN: Code duplicated, block: B:470:0x07e3 A[PHI: r68 r69
  0x07e3: PHI (r68v28 java.lang.String) = (r0v76 java.lang.String), (r68v31 java.lang.String) binds: [B:468:0x07dc, B:463:0x07c0] A[DONT_GENERATE, DONT_INLINE]
  0x07e3: PHI (r69v6 java.lang.String) = (r69v3 java.lang.String), (r69v7 java.lang.String) binds: [B:468:0x07dc, B:463:0x07c0] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:473:0x07ef  */
    /* JADX WARN: Code duplicated, block: B:478:0x0802 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:479:0x0804  */
    /* JADX WARN: Code duplicated, block: B:480:0x0809  */
    /* JADX WARN: Code duplicated, block: B:483:0x0816  */
    /* JADX WARN: Code duplicated, block: B:487:0x0824  */
    /* JADX WARN: Code duplicated, block: B:489:0x0828  */
    /* JADX WARN: Code duplicated, block: B:490:0x0830  */
    /* JADX WARN: Code duplicated, block: B:493:0x083a  */
    /* JADX WARN: Code duplicated, block: B:494:0x083f  */
    /* JADX WARN: Code duplicated, block: B:498:0x084f  */
    /* JADX WARN: Code duplicated, block: B:502:0x085b A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:503:0x085d  */
    /* JADX WARN: Code duplicated, block: B:504:0x0865  */
    /* JADX WARN: Code duplicated, block: B:507:0x0870  */
    /* JADX WARN: Code duplicated, block: B:508:0x0875  */
    /* JADX WARN: Code duplicated, block: B:512:0x0880  */
    /* JADX WARN: Code duplicated, block: B:517:0x0890  */
    /* JADX WARN: Code duplicated, block: B:518:0x0898  */
    /* JADX WARN: Code duplicated, block: B:521:0x08a1  */
    /* JADX WARN: Code duplicated, block: B:522:0x08a6  */
    /* JADX WARN: Code duplicated, block: B:525:0x08b2  */
    /* JADX WARN: Code duplicated, block: B:529:0x08c2  */
    /* JADX WARN: Code duplicated, block: B:532:0x08d8  */
    /* JADX WARN: Code duplicated, block: B:536:0x08e3 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:537:0x08e5  */
    /* JADX WARN: Code duplicated, block: B:538:0x08ea  */
    /* JADX WARN: Code duplicated, block: B:541:0x08f5  */
    /* JADX WARN: Code duplicated, block: B:542:0x0939 A[PHI: r71
  0x0939: PHI (r71v13 java.lang.String) = (r71v11 java.lang.String), (r71v16 java.lang.String) binds: [B:540:0x08f3, B:535:0x08df] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:591:0x0a2c A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:592:0x0a2e  */
    /* JADX WARN: Code duplicated, block: B:593:0x0a33  */
    /* JADX WARN: Code duplicated, block: B:595:0x0a37  */
    /* JADX WARN: Code duplicated, block: B:596:0x0a3e  */
    /* JADX WARN: Code duplicated, block: B:601:0x0a4c A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:602:0x0a4e  */
    /* JADX WARN: Code duplicated, block: B:603:0x0a53  */
    /* JADX WARN: Code duplicated, block: B:605:0x0a57  */
    /* JADX WARN: Code duplicated, block: B:606:0x0a5e  */
    /* JADX WARN: Code duplicated, block: B:608:0x0a62  */
    /* JADX WARN: Code duplicated, block: B:610:0x0a6a A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:611:0x0a6c  */
    /* JADX WARN: Code duplicated, block: B:612:0x0a75  */
    /* JADX WARN: Code duplicated, block: B:89:0x01ad  */
    /* JADX WARN: Instruction removed from duplicated block: B:232:0x03c9, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:274:0x0486, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:287:0x04b2, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:382:0x063a, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:470:0x07e3, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:542:0x0939, please report this as an issue */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r11v24, types: [androidx.databinding.InverseBindingListener, androidx.databinding.adapters.AdapterViewBindingAdapter$OnNothingSelected, androidx.databinding.adapters.TextViewBindingAdapter$BeforeTextChanged, androidx.databinding.adapters.TextViewBindingAdapter$OnTextChanged] */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        long j3;
        int data;
        AddrU8 keyCurve;
        int data2;
        String strValueOf;
        String str;
        String strValueOf2;
        int data3;
        int data4;
        int data5;
        int i;
        int data6;
        AddrU8 ch;
        int data7;
        int i2;
        BtnPara btnPara;
        Object[] dat;
        AddrU8 addrU8;
        int data8;
        String strValueOf3;
        AddrU8 addrU9;
        int data9;
        String strValueOf4;
        AddrU8 addrU10;
        int data10;
        String strValueOf5;
        int i3;
        String str2;
        String str3;
        AddrU8 ch2;
        String str4;
        int data11;
        Sys sys;
        String sysEx;
        AddrU8 type;
        String str5;
        int i4;
        int i5;
        String str6;
        int data12;
        String str7;
        EncoderPara encoderPara;
        AddrU8 ch3;
        String str8;
        int data13;
        AddrU8 curve;
        int i6;
        int data14;
        Object[] dat2;
        int i7;
        AddrU8 addrU11;
        String str9;
        int data15;
        String strValueOf6;
        AddrU8 addrU12;
        int data16;
        String strValueOf7;
        AddrU8 addrU13;
        int data17;
        String strValueOf8;
        String str10;
        AddrU8 type2;
        String str11;
        int data18;
        String str12;
        String str13;
        PedalPara pedalPara;
        AddrU8 type3;
        String str14;
        int data19;
        Object[] dat3;
        AddrU8 addrU14;
        int data20;
        String strValueOf9;
        AddrU8 addrU15;
        int data21;
        String strValueOf10;
        AddrU8 addrU16;
        int data22;
        String strValueOf11;
        String str15;
        AddrU8 ch4;
        String str16;
        int data23;
        String str17;
        String str18;
        Smk25MiniMod modPara;
        AddrU8 speed;
        String str19;
        int data24;
        Object[] dat4;
        AddrU8 addrU17;
        int data25;
        String strValueOf12;
        AddrU8 addrU18;
        int data26;
        String strValueOf13;
        AddrU8 addrU19;
        int data27;
        String strValueOf14;
        String str20;
        String str21;
        int i8;
        AddrU8 ch5;
        String str22;
        int data28;
        int i9;
        String str23;
        String str24;
        Object[] objArr;
        String str25;
        String[] psrCCStr;
        Object[] objArr2;
        String str26;
        String[] modCCStr;
        String psrValue2Text;
        String str27;
        String str28;
        String str29;
        String str30;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
            j2 = this.mDirtyFlags_1;
            this.mDirtyFlags_1 = 0L;
        }
        Smk25MiniUsrFragment smk25MiniUsrFragment = this.mListener;
        Smk25MiniGlob smk25MiniGlob = this.mGlobData;
        Smk25MiniUsr smk25MiniUsr = this.mData;
        UsrBinding usrBinding = this.mUsrBinding;
        int i10 = ((10737418240L & j) > 0L ? 1 : ((10737418240L & j) == 0L ? 0 : -1));
        if ((87965225194496L & j) == 0 && (j2 & 256) == 0) {
            data2 = 0;
            data = 0;
            j3 = 70373039149056L;
        } else {
            if ((j & 17596481012736L) == 0 && (j2 & 256) == 0) {
                j3 = 70373039149056L;
            } else {
                AddrU8 inputMode = smk25MiniGlob != null ? smk25MiniGlob.getInputMode() : null;
                j3 = 70373039149056L;
                updateRegistration(10, inputMode);
                data = inputMode != null ? inputMode.getData() : 0;
                if ((j & j3) == 0 || (j2 & 256) != 0) {
                    if (smk25MiniGlob != null) {
                        keyCurve = smk25MiniGlob.getKeyCurve();
                    } else {
                        keyCurve = null;
                    }
                    updateRegistration(12, keyCurve);
                    if (keyCurve != null) {
                        data2 = keyCurve.getData();
                    } else {
                        data2 = 0;
                    }
                } else {
                    data2 = 0;
                }
            }
            if ((j & j3) == 0) {
                if (smk25MiniGlob != null) {
                    keyCurve = smk25MiniGlob.getKeyCurve();
                } else {
                    keyCurve = null;
                }
                updateRegistration(12, keyCurve);
                if (keyCurve != null) {
                    data2 = keyCurve.getData();
                } else {
                    data2 = 0;
                }
            } else {
                if (smk25MiniGlob != null) {
                    keyCurve = smk25MiniGlob.getKeyCurve();
                } else {
                    keyCurve = null;
                }
                updateRegistration(12, keyCurve);
                if (keyCurve != null) {
                    data2 = keyCurve.getData();
                } else {
                    data2 = 0;
                }
            }
        }
        if (((-87965225194497L) & j) == 0 && (297 & j2) == 0) {
            j = j;
            j2 = j2;
            usrBinding = usrBinding;
            i10 = i10;
            data = data;
            str12 = null;
            strValueOf2 = null;
            i4 = 0;
            str14 = null;
            i9 = 0;
            strValueOf = null;
            i6 = 0;
            data7 = 0;
            data3 = 0;
            data15 = 0;
            data28 = 0;
            data18 = 0;
            i8 = 0;
            str10 = null;
            str = null;
            sysEx = null;
            str19 = null;
            str17 = null;
            strValueOf11 = null;
            str8 = null;
            str9 = null;
            str20 = null;
            str23 = null;
            strValueOf14 = null;
            data23 = 0;
            i5 = 0;
            i7 = 0;
            data24 = 0;
            i = 0;
            i2 = 0;
            data12 = 0;
            data5 = 0;
            str5 = null;
        } else {
            if ((581008409739135492L & j) == 0 && (j2 & 256) == 0) {
                j = j;
                data4 = 0;
                data5 = 0;
                strValueOf2 = null;
                data3 = 0;
                strValueOf = null;
                str = null;
            } else {
                Smk25MiniKeyPara keyPara = smk25MiniUsr != null ? smk25MiniUsr.getKeyPara() : null;
                if ((j & 77309411332L) == 0 && (j2 & 256) == 0) {
                    strValueOf = null;
                } else {
                    AddrU8 max = keyPara != null ? keyPara.getMax() : null;
                    updateRegistration(2, max);
                    strValueOf = String.valueOf(max != null ? max.getData() : 0);
                }
                if ((j & 8804682957312L) == 0 && (j2 & 256) == 0) {
                    strValueOf2 = null;
                    str = null;
                } else {
                    AddrU8 min = keyPara != null ? keyPara.getMin() : null;
                    str = null;
                    updateRegistration(9, min);
                    strValueOf2 = String.valueOf(min != null ? min.getData() : 0);
                }
                if ((j & 35192962025472L) == 0 && (j2 & 256) == 0) {
                    data3 = 0;
                } else {
                    AddrU8 ch6 = keyPara != null ? keyPara.getCh() : str;
                    updateRegistration(11, ch6);
                    if (ch6 != 0) {
                        data3 = ch6.getData();
                    } else {
                        data3 = 0;
                    }
                }
                if ((j & 4503608217567232L) == 0 && (j2 & 256) == 0) {
                    data4 = 0;
                } else {
                    AddrI8 octave = keyPara != null ? keyPara.getOctave() : str;
                    updateRegistration(18, octave);
                    data4 = (octave != 0 ? octave.getData() : (byte) 0) + 3;
                }
                if ((j & 576460760926912512L) == 0 && (j2 & 256) == 0) {
                    data5 = 0;
                } else {
                    AddrI8 transpose = keyPara != null ? keyPara.getTranspose() : str;
                    updateRegistration(25, transpose);
                    data5 = (transpose != 0 ? transpose.getData() : (byte) 0) + 12;
                }
            }
            if ((j & 4755801215370002432L) == 0 && (j2 & 256) == 0) {
                i = data4;
                data6 = 0;
            } else {
                Smk25MiniPitch pitchPara = smk25MiniUsr != null ? smk25MiniUsr.getPitchPara() : str;
                if ((j & 144115196674179072L) == 0 && (j2 & 256) == 0) {
                    i = data4;
                } else {
                    AddrU8 speed2 = pitchPara != 0 ? pitchPara.getSpeed() : str;
                    i = data4;
                    updateRegistration(23, speed2);
                    data6 = speed2 != 0 ? speed2.getData() : 0;
                    if ((j & 4611686027285757952L) == 0 || (j2 & 256) != 0) {
                        if (pitchPara != 0) {
                            ch = pitchPara.getCh();
                        } else {
                            ch = str;
                        }
                        updateRegistration(28, ch);
                        data7 = ch != 0 ? ch.getData() : 0;
                    }
                    i2 = data6;
                    if ((j & 1190358787901440064L) == 0 || (j2 & 288) != 0) {
                        if (smk25MiniUsr != null) {
                            btnPara = smk25MiniUsr.getBtnPara();
                        } else {
                            btnPara = str;
                        }
                        if ((j & 1154048513756168256L) == 0 || (j2 & 288) != 0) {
                            if (btnPara != 0) {
                                dat = btnPara.getDat();
                            } else {
                                dat = str;
                            }
                            if ((j & 1108101562432L) == 0 || (j2 & 256) != 0) {
                                if (dat != 0) {
                                    addrU8 = (AddrU8) getFromArray(dat, 1);
                                } else {
                                    addrU8 = str;
                                }
                                updateRegistration(6, addrU8);
                                if (addrU8 != 0) {
                                    data8 = addrU8.getData();
                                } else {
                                    data8 = 0;
                                }
                                strValueOf3 = String.valueOf(data8);
                            } else {
                                strValueOf3 = str;
                            }
                            if ((j & 1125908496842752L) == 0 || (j2 & 256) != 0) {
                                if (dat != 0) {
                                    addrU9 = (AddrU8) getFromArray(dat, 2);
                                } else {
                                    addrU9 = str;
                                }
                                updateRegistration(16, addrU9);
                                if (addrU9 != 0) {
                                    data9 = addrU9.getData();
                                } else {
                                    data9 = 0;
                                }
                                strValueOf4 = String.valueOf(data9);
                            } else {
                                strValueOf4 = str;
                            }
                            if ((j & 1152921514337632256L) == 0 || (j2 & 288) != 0) {
                                if (dat != 0) {
                                    addrU10 = (AddrU8) getFromArray(dat, 0);
                                } else {
                                    addrU10 = str;
                                }
                                updateRegistration(26, addrU10);
                                if (addrU10 != 0) {
                                    data10 = addrU10.getData();
                                } else {
                                    data10 = 0;
                                }
                                if ((j & 1152921513263890432L) == 0 || (j2 & 256) != 0) {
                                    strValueOf5 = String.valueOf(data10);
                                    i3 = data10;
                                    str2 = strValueOf4;
                                    str3 = strValueOf3;
                                } else {
                                    i3 = data10;
                                    str2 = strValueOf4;
                                    str3 = strValueOf3;
                                    strValueOf5 = str;
                                }
                            } else {
                                str2 = strValueOf4;
                                strValueOf5 = str;
                                str3 = strValueOf3;
                            }
                            if ((j & 281483566661632L) == 0 || (j2 & 256) != 0) {
                                if (btnPara != 0) {
                                    ch2 = btnPara.getCh();
                                } else {
                                    ch2 = str;
                                }
                                str4 = str3;
                                updateRegistration(14, ch2);
                                data11 = ch2 != 0 ? ch2.getData() : 0;
                                if (i10 == 0 || (j2 & 256) != 0) {
                                    if (btnPara != 0) {
                                        sys = btnPara.getSys();
                                    } else {
                                        sys = str;
                                    }
                                    if (smk25MiniUsrFragment != null) {
                                        sysEx = smk25MiniUsrFragment.getSysEx(sys);
                                    } else {
                                        sysEx = str;
                                    }
                                } else {
                                    sysEx = str;
                                }
                                if ((j & 36028805610995712L) == 0 || (j2 & 256) != 0) {
                                    if (btnPara != 0) {
                                        type = btnPara.getType();
                                    } else {
                                        type = str;
                                    }
                                    updateRegistration(21, type);
                                    if (type != 0) {
                                        int i11 = i3;
                                        str5 = strValueOf5;
                                        i4 = i11;
                                        i5 = data11;
                                        str6 = str2;
                                        data12 = type.getData();
                                        str7 = str4;
                                    }
                                    if ((j & 2817920526876856L) == 0 || (j2 & 257) != 0) {
                                        if (smk25MiniUsr != null) {
                                            encoderPara = smk25MiniUsr.getEncoderPara();
                                        } else {
                                            encoderPara = str;
                                        }
                                        if ((j & 283467841552L) == 0 || (j2 & 256) != 0) {
                                            if (encoderPara != 0) {
                                                ch3 = encoderPara.getCh();
                                            } else {
                                                ch3 = str;
                                            }
                                            str8 = str6;
                                            updateRegistration(4, ch3);
                                            data13 = ch3 != 0 ? ch3.getData() : 0;
                                            if ((j & 2207613190272L) == 0 || (j2 & 256) != 0) {
                                                if (encoderPara != 0) {
                                                    curve = encoderPara.getCurve();
                                                } else {
                                                    curve = str;
                                                }
                                                i6 = data13;
                                                updateRegistration(7, curve);
                                                data14 = curve != 0 ? curve.getData() : 0;
                                                if ((j & 563646811897896L) == 0 || (j2 & 257) != 0) {
                                                    if (encoderPara != 0) {
                                                        dat2 = encoderPara.getDat();
                                                    } else {
                                                        dat2 = str;
                                                    }
                                                    if ((j & 147102629896L) == 0 || (j2 & 257) != 0) {
                                                        i7 = data14;
                                                        if (dat2 != 0) {
                                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                        } else {
                                                            addrU11 = str;
                                                        }
                                                        str9 = str7;
                                                        updateRegistration(3, addrU11);
                                                        if (addrU11 != 0) {
                                                            data15 = addrU11.getData();
                                                        } else {
                                                            data15 = 0;
                                                        }
                                                        if ((j & 146028888072L) == 0 || (j2 & 256) != 0) {
                                                            strValueOf6 = String.valueOf(data15);
                                                        } else {
                                                            strValueOf6 = str;
                                                        }
                                                    } else {
                                                        i7 = data14;
                                                        str9 = str7;
                                                        strValueOf6 = str;
                                                        data15 = 0;
                                                    }
                                                    if ((j & 558345748512L) == 0 || (j2 & 256) != 0) {
                                                        if (dat2 != 0) {
                                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                        } else {
                                                            addrU12 = str;
                                                        }
                                                        updateRegistration(5, addrU12);
                                                        if (addrU12 != 0) {
                                                            data16 = addrU12.getData();
                                                        } else {
                                                            data16 = 0;
                                                        }
                                                        strValueOf7 = String.valueOf(data16);
                                                    } else {
                                                        strValueOf7 = str;
                                                    }
                                                    if ((j & 562958543388672L) == 0 || (j2 & 256) != 0) {
                                                        if (dat2 != 0) {
                                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                        } else {
                                                            addrU13 = str;
                                                        }
                                                        updateRegistration(15, addrU13);
                                                        if (addrU13 != 0) {
                                                            data17 = addrU13.getData();
                                                        } else {
                                                            data17 = 0;
                                                        }
                                                        strValueOf8 = String.valueOf(data17);
                                                    } else {
                                                        strValueOf8 = str;
                                                    }
                                                    str10 = strValueOf6;
                                                } else {
                                                    i7 = data14;
                                                    str9 = str7;
                                                    strValueOf7 = str;
                                                    strValueOf8 = strValueOf7;
                                                    str10 = strValueOf8;
                                                    data15 = 0;
                                                }
                                                if ((j & 2251808403750912L) == 0 || (j2 & 256) != 0) {
                                                    if (encoderPara != 0) {
                                                        type2 = encoderPara.getType();
                                                    } else {
                                                        type2 = str;
                                                    }
                                                    str11 = strValueOf8;
                                                    updateRegistration(17, type2);
                                                    if (type2 != 0) {
                                                        data18 = type2.getData();
                                                        str12 = strValueOf7;
                                                        str13 = str11;
                                                    }
                                                } else {
                                                    strValueOf7 = strValueOf7;
                                                    str11 = strValueOf8;
                                                }
                                                str12 = strValueOf7;
                                                str13 = str11;
                                                data18 = 0;
                                            } else {
                                                i6 = data13;
                                            }
                                            if ((j & 563646811897896L) == 0) {
                                                if (encoderPara != 0) {
                                                    dat2 = encoderPara.getDat();
                                                } else {
                                                    dat2 = str;
                                                }
                                                if ((j & 147102629896L) == 0) {
                                                    i7 = data14;
                                                    if (dat2 != 0) {
                                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                    } else {
                                                        addrU11 = str;
                                                    }
                                                    str9 = str7;
                                                    updateRegistration(3, addrU11);
                                                    if (addrU11 != 0) {
                                                        data15 = addrU11.getData();
                                                    } else {
                                                        data15 = 0;
                                                    }
                                                    if ((j & 146028888072L) == 0) {
                                                        strValueOf6 = String.valueOf(data15);
                                                    } else {
                                                        strValueOf6 = String.valueOf(data15);
                                                    }
                                                } else {
                                                    i7 = data14;
                                                    if (dat2 != 0) {
                                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                    } else {
                                                        addrU11 = str;
                                                    }
                                                    str9 = str7;
                                                    updateRegistration(3, addrU11);
                                                    if (addrU11 != 0) {
                                                        data15 = addrU11.getData();
                                                    } else {
                                                        data15 = 0;
                                                    }
                                                    if ((j & 146028888072L) == 0) {
                                                        strValueOf6 = String.valueOf(data15);
                                                    } else {
                                                        strValueOf6 = String.valueOf(data15);
                                                    }
                                                }
                                                if ((j & 558345748512L) == 0) {
                                                    if (dat2 != 0) {
                                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                    } else {
                                                        addrU12 = str;
                                                    }
                                                    updateRegistration(5, addrU12);
                                                    if (addrU12 != 0) {
                                                        data16 = addrU12.getData();
                                                    } else {
                                                        data16 = 0;
                                                    }
                                                    strValueOf7 = String.valueOf(data16);
                                                } else {
                                                    if (dat2 != 0) {
                                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                    } else {
                                                        addrU12 = str;
                                                    }
                                                    updateRegistration(5, addrU12);
                                                    if (addrU12 != 0) {
                                                        data16 = addrU12.getData();
                                                    } else {
                                                        data16 = 0;
                                                    }
                                                    strValueOf7 = String.valueOf(data16);
                                                }
                                                if ((j & 562958543388672L) == 0) {
                                                    if (dat2 != 0) {
                                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                    } else {
                                                        addrU13 = str;
                                                    }
                                                    updateRegistration(15, addrU13);
                                                    if (addrU13 != 0) {
                                                        data17 = addrU13.getData();
                                                    } else {
                                                        data17 = 0;
                                                    }
                                                    strValueOf8 = String.valueOf(data17);
                                                } else {
                                                    if (dat2 != 0) {
                                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                    } else {
                                                        addrU13 = str;
                                                    }
                                                    updateRegistration(15, addrU13);
                                                    if (addrU13 != 0) {
                                                        data17 = addrU13.getData();
                                                    } else {
                                                        data17 = 0;
                                                    }
                                                    strValueOf8 = String.valueOf(data17);
                                                }
                                                str10 = strValueOf6;
                                            } else {
                                                if (encoderPara != 0) {
                                                    dat2 = encoderPara.getDat();
                                                } else {
                                                    dat2 = str;
                                                }
                                                if ((j & 147102629896L) == 0) {
                                                    i7 = data14;
                                                    if (dat2 != 0) {
                                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                    } else {
                                                        addrU11 = str;
                                                    }
                                                    str9 = str7;
                                                    updateRegistration(3, addrU11);
                                                    if (addrU11 != 0) {
                                                        data15 = addrU11.getData();
                                                    } else {
                                                        data15 = 0;
                                                    }
                                                    if ((j & 146028888072L) == 0) {
                                                        strValueOf6 = String.valueOf(data15);
                                                    } else {
                                                        strValueOf6 = String.valueOf(data15);
                                                    }
                                                } else {
                                                    i7 = data14;
                                                    if (dat2 != 0) {
                                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                    } else {
                                                        addrU11 = str;
                                                    }
                                                    str9 = str7;
                                                    updateRegistration(3, addrU11);
                                                    if (addrU11 != 0) {
                                                        data15 = addrU11.getData();
                                                    } else {
                                                        data15 = 0;
                                                    }
                                                    if ((j & 146028888072L) == 0) {
                                                        strValueOf6 = String.valueOf(data15);
                                                    } else {
                                                        strValueOf6 = String.valueOf(data15);
                                                    }
                                                }
                                                if ((j & 558345748512L) == 0) {
                                                    if (dat2 != 0) {
                                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                    } else {
                                                        addrU12 = str;
                                                    }
                                                    updateRegistration(5, addrU12);
                                                    if (addrU12 != 0) {
                                                        data16 = addrU12.getData();
                                                    } else {
                                                        data16 = 0;
                                                    }
                                                    strValueOf7 = String.valueOf(data16);
                                                } else {
                                                    if (dat2 != 0) {
                                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                    } else {
                                                        addrU12 = str;
                                                    }
                                                    updateRegistration(5, addrU12);
                                                    if (addrU12 != 0) {
                                                        data16 = addrU12.getData();
                                                    } else {
                                                        data16 = 0;
                                                    }
                                                    strValueOf7 = String.valueOf(data16);
                                                }
                                                if ((j & 562958543388672L) == 0) {
                                                    if (dat2 != 0) {
                                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                    } else {
                                                        addrU13 = str;
                                                    }
                                                    updateRegistration(15, addrU13);
                                                    if (addrU13 != 0) {
                                                        data17 = addrU13.getData();
                                                    } else {
                                                        data17 = 0;
                                                    }
                                                    strValueOf8 = String.valueOf(data17);
                                                } else {
                                                    if (dat2 != 0) {
                                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                    } else {
                                                        addrU13 = str;
                                                    }
                                                    updateRegistration(15, addrU13);
                                                    if (addrU13 != 0) {
                                                        data17 = addrU13.getData();
                                                    } else {
                                                        data17 = 0;
                                                    }
                                                    strValueOf8 = String.valueOf(data17);
                                                }
                                                str10 = strValueOf6;
                                            }
                                            if ((j & 2251808403750912L) == 0) {
                                                if (encoderPara != 0) {
                                                    type2 = encoderPara.getType();
                                                } else {
                                                    type2 = str;
                                                }
                                                str11 = strValueOf8;
                                                updateRegistration(17, type2);
                                                if (type2 != 0) {
                                                    data18 = type2.getData();
                                                    str12 = strValueOf7;
                                                    str13 = str11;
                                                } else {
                                                    str12 = strValueOf7;
                                                    str13 = str11;
                                                    data18 = 0;
                                                }
                                            } else {
                                                if (encoderPara != 0) {
                                                    type2 = encoderPara.getType();
                                                } else {
                                                    type2 = str;
                                                }
                                                str11 = strValueOf8;
                                                updateRegistration(17, type2);
                                                if (type2 != 0) {
                                                    data18 = type2.getData();
                                                    str12 = strValueOf7;
                                                    str13 = str11;
                                                } else {
                                                    str12 = strValueOf7;
                                                    str13 = str11;
                                                    data18 = 0;
                                                }
                                            }
                                        } else {
                                            sysEx = sysEx;
                                            str8 = str6;
                                        }
                                        if ((j & 2207613190272L) == 0) {
                                            if (encoderPara != 0) {
                                                curve = encoderPara.getCurve();
                                            } else {
                                                curve = str;
                                            }
                                            i6 = data13;
                                            updateRegistration(7, curve);
                                            if (curve != 0) {
                                            }
                                        } else {
                                            if (encoderPara != 0) {
                                                curve = encoderPara.getCurve();
                                            } else {
                                                curve = str;
                                            }
                                            i6 = data13;
                                            updateRegistration(7, curve);
                                            if (curve != 0) {
                                            }
                                        }
                                        if ((j & 563646811897896L) == 0) {
                                            if (encoderPara != 0) {
                                                dat2 = encoderPara.getDat();
                                            } else {
                                                dat2 = str;
                                            }
                                            if ((j & 147102629896L) == 0) {
                                                i7 = data14;
                                                if (dat2 != 0) {
                                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                } else {
                                                    addrU11 = str;
                                                }
                                                str9 = str7;
                                                updateRegistration(3, addrU11);
                                                if (addrU11 != 0) {
                                                    data15 = addrU11.getData();
                                                } else {
                                                    data15 = 0;
                                                }
                                                if ((j & 146028888072L) == 0) {
                                                    strValueOf6 = String.valueOf(data15);
                                                } else {
                                                    strValueOf6 = String.valueOf(data15);
                                                }
                                            } else {
                                                i7 = data14;
                                                if (dat2 != 0) {
                                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                } else {
                                                    addrU11 = str;
                                                }
                                                str9 = str7;
                                                updateRegistration(3, addrU11);
                                                if (addrU11 != 0) {
                                                    data15 = addrU11.getData();
                                                } else {
                                                    data15 = 0;
                                                }
                                                if ((j & 146028888072L) == 0) {
                                                    strValueOf6 = String.valueOf(data15);
                                                } else {
                                                    strValueOf6 = String.valueOf(data15);
                                                }
                                            }
                                            if ((j & 558345748512L) == 0) {
                                                if (dat2 != 0) {
                                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                } else {
                                                    addrU12 = str;
                                                }
                                                updateRegistration(5, addrU12);
                                                if (addrU12 != 0) {
                                                    data16 = addrU12.getData();
                                                } else {
                                                    data16 = 0;
                                                }
                                                strValueOf7 = String.valueOf(data16);
                                            } else {
                                                if (dat2 != 0) {
                                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                } else {
                                                    addrU12 = str;
                                                }
                                                updateRegistration(5, addrU12);
                                                if (addrU12 != 0) {
                                                    data16 = addrU12.getData();
                                                } else {
                                                    data16 = 0;
                                                }
                                                strValueOf7 = String.valueOf(data16);
                                            }
                                            if ((j & 562958543388672L) == 0) {
                                                if (dat2 != 0) {
                                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                } else {
                                                    addrU13 = str;
                                                }
                                                updateRegistration(15, addrU13);
                                                if (addrU13 != 0) {
                                                    data17 = addrU13.getData();
                                                } else {
                                                    data17 = 0;
                                                }
                                                strValueOf8 = String.valueOf(data17);
                                            } else {
                                                if (dat2 != 0) {
                                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                } else {
                                                    addrU13 = str;
                                                }
                                                updateRegistration(15, addrU13);
                                                if (addrU13 != 0) {
                                                    data17 = addrU13.getData();
                                                } else {
                                                    data17 = 0;
                                                }
                                                strValueOf8 = String.valueOf(data17);
                                            }
                                            str10 = strValueOf6;
                                        } else {
                                            if (encoderPara != 0) {
                                                dat2 = encoderPara.getDat();
                                            } else {
                                                dat2 = str;
                                            }
                                            if ((j & 147102629896L) == 0) {
                                                i7 = data14;
                                                if (dat2 != 0) {
                                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                } else {
                                                    addrU11 = str;
                                                }
                                                str9 = str7;
                                                updateRegistration(3, addrU11);
                                                if (addrU11 != 0) {
                                                    data15 = addrU11.getData();
                                                } else {
                                                    data15 = 0;
                                                }
                                                if ((j & 146028888072L) == 0) {
                                                    strValueOf6 = String.valueOf(data15);
                                                } else {
                                                    strValueOf6 = String.valueOf(data15);
                                                }
                                            } else {
                                                i7 = data14;
                                                if (dat2 != 0) {
                                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                                } else {
                                                    addrU11 = str;
                                                }
                                                str9 = str7;
                                                updateRegistration(3, addrU11);
                                                if (addrU11 != 0) {
                                                    data15 = addrU11.getData();
                                                } else {
                                                    data15 = 0;
                                                }
                                                if ((j & 146028888072L) == 0) {
                                                    strValueOf6 = String.valueOf(data15);
                                                } else {
                                                    strValueOf6 = String.valueOf(data15);
                                                }
                                            }
                                            if ((j & 558345748512L) == 0) {
                                                if (dat2 != 0) {
                                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                } else {
                                                    addrU12 = str;
                                                }
                                                updateRegistration(5, addrU12);
                                                if (addrU12 != 0) {
                                                    data16 = addrU12.getData();
                                                } else {
                                                    data16 = 0;
                                                }
                                                strValueOf7 = String.valueOf(data16);
                                            } else {
                                                if (dat2 != 0) {
                                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                                } else {
                                                    addrU12 = str;
                                                }
                                                updateRegistration(5, addrU12);
                                                if (addrU12 != 0) {
                                                    data16 = addrU12.getData();
                                                } else {
                                                    data16 = 0;
                                                }
                                                strValueOf7 = String.valueOf(data16);
                                            }
                                            if ((j & 562958543388672L) == 0) {
                                                if (dat2 != 0) {
                                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                } else {
                                                    addrU13 = str;
                                                }
                                                updateRegistration(15, addrU13);
                                                if (addrU13 != 0) {
                                                    data17 = addrU13.getData();
                                                } else {
                                                    data17 = 0;
                                                }
                                                strValueOf8 = String.valueOf(data17);
                                            } else {
                                                if (dat2 != 0) {
                                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                                } else {
                                                    addrU13 = str;
                                                }
                                                updateRegistration(15, addrU13);
                                                if (addrU13 != 0) {
                                                    data17 = addrU13.getData();
                                                } else {
                                                    data17 = 0;
                                                }
                                                strValueOf8 = String.valueOf(data17);
                                            }
                                            str10 = strValueOf6;
                                        }
                                        if ((j & 2251808403750912L) == 0) {
                                            if (encoderPara != 0) {
                                                type2 = encoderPara.getType();
                                            } else {
                                                type2 = str;
                                            }
                                            str11 = strValueOf8;
                                            updateRegistration(17, type2);
                                            if (type2 != 0) {
                                                data18 = type2.getData();
                                                str12 = strValueOf7;
                                                str13 = str11;
                                            } else {
                                                str12 = strValueOf7;
                                                str13 = str11;
                                                data18 = 0;
                                            }
                                        } else {
                                            if (encoderPara != 0) {
                                                type2 = encoderPara.getType();
                                            } else {
                                                type2 = str;
                                            }
                                            str11 = strValueOf8;
                                            updateRegistration(17, type2);
                                            if (type2 != 0) {
                                                data18 = type2.getData();
                                                str12 = strValueOf7;
                                                str13 = str11;
                                            } else {
                                                str12 = strValueOf7;
                                                str13 = str11;
                                                data18 = 0;
                                            }
                                        }
                                    } else {
                                        sysEx = sysEx;
                                        str8 = str6;
                                        str9 = str7;
                                        str12 = str;
                                        str13 = str12;
                                        str10 = str13;
                                        data18 = 0;
                                        i6 = 0;
                                        i7 = 0;
                                        data15 = 0;
                                    }
                                    if ((j & (-6620287044909793024L)) == 0 || (j2 & 256) != 0) {
                                        if (smk25MiniUsr != null) {
                                            pedalPara = smk25MiniUsr.getPedalPara();
                                        } else {
                                            pedalPara = str;
                                        }
                                        if ((j & 288230384758423552L) == 0 || (j2 & 256) != 0) {
                                            if (pedalPara != 0) {
                                                type3 = pedalPara.getType();
                                            } else {
                                                type3 = str;
                                            }
                                            str14 = str13;
                                            updateRegistration(24, type3);
                                            data19 = type3 != 0 ? type3.getData() : 0;
                                            if ((j & 2314854615239622912L) == 0 || (j2 & 256) != 0) {
                                                if (pedalPara != 0) {
                                                    dat3 = pedalPara.getDat();
                                                } else {
                                                    dat3 = str;
                                                }
                                                if ((j & 4406636445952L) == 0 || (j2 & 256) != 0) {
                                                    if (dat3 != 0) {
                                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                                    } else {
                                                        addrU14 = str;
                                                    }
                                                    updateRegistration(8, addrU14);
                                                    if (addrU14 != 0) {
                                                        data20 = addrU14.getData();
                                                    } else {
                                                        data20 = 0;
                                                    }
                                                    strValueOf9 = String.valueOf(data20);
                                                } else {
                                                    strValueOf9 = str;
                                                }
                                                if ((j & 9007207845199872L) == 0 || (j2 & 256) != 0) {
                                                    if (dat3 != 0) {
                                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                                    } else {
                                                        addrU15 = str;
                                                    }
                                                    updateRegistration(19, addrU15);
                                                    if (addrU15 != 0) {
                                                        data21 = addrU15.getData();
                                                    } else {
                                                        data21 = 0;
                                                    }
                                                    strValueOf10 = String.valueOf(data21);
                                                } else {
                                                    strValueOf10 = str;
                                                }
                                                if ((j & 2305843017937846272L) == 0 || (j2 & 256) != 0) {
                                                    if (dat3 != 0) {
                                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                                    } else {
                                                        addrU16 = str;
                                                    }
                                                    updateRegistration(27, addrU16);
                                                    if (addrU16 != 0) {
                                                        data22 = addrU16.getData();
                                                    } else {
                                                        data22 = 0;
                                                    }
                                                    strValueOf11 = String.valueOf(data22);
                                                } else {
                                                    strValueOf11 = str;
                                                }
                                                str15 = strValueOf9;
                                            } else {
                                                data19 = data19;
                                                str10 = str10;
                                                str15 = str;
                                                strValueOf10 = str15;
                                                strValueOf11 = strValueOf10;
                                            }
                                            if ((j & (-9223372027727970304L)) == 0 || (j2 & 256) != 0) {
                                                if (pedalPara != 0) {
                                                    ch4 = pedalPara.getCh();
                                                } else {
                                                    ch4 = str;
                                                }
                                                str16 = strValueOf10;
                                                updateRegistration(29, ch4);
                                                if (ch4 != 0) {
                                                    data23 = ch4.getData();
                                                    str17 = str15;
                                                    str18 = str16;
                                                }
                                            } else {
                                                str15 = str15;
                                                str16 = strValueOf10;
                                            }
                                            str17 = str15;
                                            str18 = str16;
                                            data23 = 0;
                                        } else {
                                            str12 = str12;
                                            str14 = str13;
                                        }
                                        if ((j & 2314854615239622912L) == 0) {
                                            if (pedalPara != 0) {
                                                dat3 = pedalPara.getDat();
                                            } else {
                                                dat3 = str;
                                            }
                                            if ((j & 4406636445952L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                                } else {
                                                    addrU14 = str;
                                                }
                                                updateRegistration(8, addrU14);
                                                if (addrU14 != 0) {
                                                    data20 = addrU14.getData();
                                                } else {
                                                    data20 = 0;
                                                }
                                                strValueOf9 = String.valueOf(data20);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                                } else {
                                                    addrU14 = str;
                                                }
                                                updateRegistration(8, addrU14);
                                                if (addrU14 != 0) {
                                                    data20 = addrU14.getData();
                                                } else {
                                                    data20 = 0;
                                                }
                                                strValueOf9 = String.valueOf(data20);
                                            }
                                            if ((j & 9007207845199872L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                                } else {
                                                    addrU15 = str;
                                                }
                                                updateRegistration(19, addrU15);
                                                if (addrU15 != 0) {
                                                    data21 = addrU15.getData();
                                                } else {
                                                    data21 = 0;
                                                }
                                                strValueOf10 = String.valueOf(data21);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                                } else {
                                                    addrU15 = str;
                                                }
                                                updateRegistration(19, addrU15);
                                                if (addrU15 != 0) {
                                                    data21 = addrU15.getData();
                                                } else {
                                                    data21 = 0;
                                                }
                                                strValueOf10 = String.valueOf(data21);
                                            }
                                            if ((j & 2305843017937846272L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                                } else {
                                                    addrU16 = str;
                                                }
                                                updateRegistration(27, addrU16);
                                                if (addrU16 != 0) {
                                                    data22 = addrU16.getData();
                                                } else {
                                                    data22 = 0;
                                                }
                                                strValueOf11 = String.valueOf(data22);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                                } else {
                                                    addrU16 = str;
                                                }
                                                updateRegistration(27, addrU16);
                                                if (addrU16 != 0) {
                                                    data22 = addrU16.getData();
                                                } else {
                                                    data22 = 0;
                                                }
                                                strValueOf11 = String.valueOf(data22);
                                            }
                                            str15 = strValueOf9;
                                        } else {
                                            if (pedalPara != 0) {
                                                dat3 = pedalPara.getDat();
                                            } else {
                                                dat3 = str;
                                            }
                                            if ((j & 4406636445952L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                                } else {
                                                    addrU14 = str;
                                                }
                                                updateRegistration(8, addrU14);
                                                if (addrU14 != 0) {
                                                    data20 = addrU14.getData();
                                                } else {
                                                    data20 = 0;
                                                }
                                                strValueOf9 = String.valueOf(data20);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                                } else {
                                                    addrU14 = str;
                                                }
                                                updateRegistration(8, addrU14);
                                                if (addrU14 != 0) {
                                                    data20 = addrU14.getData();
                                                } else {
                                                    data20 = 0;
                                                }
                                                strValueOf9 = String.valueOf(data20);
                                            }
                                            if ((j & 9007207845199872L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                                } else {
                                                    addrU15 = str;
                                                }
                                                updateRegistration(19, addrU15);
                                                if (addrU15 != 0) {
                                                    data21 = addrU15.getData();
                                                } else {
                                                    data21 = 0;
                                                }
                                                strValueOf10 = String.valueOf(data21);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                                } else {
                                                    addrU15 = str;
                                                }
                                                updateRegistration(19, addrU15);
                                                if (addrU15 != 0) {
                                                    data21 = addrU15.getData();
                                                } else {
                                                    data21 = 0;
                                                }
                                                strValueOf10 = String.valueOf(data21);
                                            }
                                            if ((j & 2305843017937846272L) == 0) {
                                                if (dat3 != 0) {
                                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                                } else {
                                                    addrU16 = str;
                                                }
                                                updateRegistration(27, addrU16);
                                                if (addrU16 != 0) {
                                                    data22 = addrU16.getData();
                                                } else {
                                                    data22 = 0;
                                                }
                                                strValueOf11 = String.valueOf(data22);
                                            } else {
                                                if (dat3 != 0) {
                                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                                } else {
                                                    addrU16 = str;
                                                }
                                                updateRegistration(27, addrU16);
                                                if (addrU16 != 0) {
                                                    data22 = addrU16.getData();
                                                } else {
                                                    data22 = 0;
                                                }
                                                strValueOf11 = String.valueOf(data22);
                                            }
                                            str15 = strValueOf9;
                                        }
                                        if ((j & (-9223372027727970304L)) == 0) {
                                            if (pedalPara != 0) {
                                                ch4 = pedalPara.getCh();
                                            } else {
                                                ch4 = str;
                                            }
                                            str16 = strValueOf10;
                                            updateRegistration(29, ch4);
                                            if (ch4 != 0) {
                                                data23 = ch4.getData();
                                                str17 = str15;
                                                str18 = str16;
                                            } else {
                                                str17 = str15;
                                                str18 = str16;
                                                data23 = 0;
                                            }
                                        } else {
                                            if (pedalPara != 0) {
                                                ch4 = pedalPara.getCh();
                                            } else {
                                                ch4 = str;
                                            }
                                            str16 = strValueOf10;
                                            updateRegistration(29, ch4);
                                            if (ch4 != 0) {
                                                data23 = ch4.getData();
                                                str17 = str15;
                                                str18 = str16;
                                            } else {
                                                str17 = str15;
                                                str18 = str16;
                                                data23 = 0;
                                            }
                                        }
                                    } else {
                                        str12 = str12;
                                        str14 = str13;
                                        str10 = str10;
                                        str17 = str;
                                        str18 = str17;
                                        strValueOf11 = str18;
                                        data23 = 0;
                                        data19 = 0;
                                    }
                                    if ((j & 90212791244300291L) == 0 || (j2 & 264) != 0) {
                                        if (smk25MiniUsr != null) {
                                            modPara = smk25MiniUsr.getModPara();
                                        } else {
                                            modPara = str;
                                        }
                                        if ((j & 42949672962L) == 0 || (j2 & 256) != 0) {
                                            if (modPara != 0) {
                                                speed = modPara.getSpeed();
                                            } else {
                                                speed = str;
                                            }
                                            str19 = str18;
                                            updateRegistration(1, speed);
                                            data24 = speed != 0 ? speed.getData() : 0;
                                            if ((j & 18155162842439681L) == 0 || (j2 & 264) != 0) {
                                                if (modPara != 0) {
                                                    dat4 = modPara.getDat();
                                                } else {
                                                    dat4 = str;
                                                }
                                                if ((j & 25769803777L) == 0 || (j2 & 256) != 0) {
                                                    if (dat4 != 0) {
                                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                                    } else {
                                                        addrU17 = str;
                                                    }
                                                    updateRegistration(0, addrU17);
                                                    if (addrU17 != 0) {
                                                        data25 = addrU17.getData();
                                                    } else {
                                                        data25 = 0;
                                                    }
                                                    strValueOf12 = String.valueOf(data25);
                                                } else {
                                                    strValueOf12 = str;
                                                }
                                                if ((j & 140746078298112L) == 0 || (j2 & 256) != 0) {
                                                    if (dat4 != 0) {
                                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                                    } else {
                                                        addrU18 = str;
                                                    }
                                                    updateRegistration(13, addrU18);
                                                    if (addrU18 != 0) {
                                                        data26 = addrU18.getData();
                                                    } else {
                                                        data26 = 0;
                                                    }
                                                    strValueOf13 = String.valueOf(data26);
                                                } else {
                                                    strValueOf13 = str;
                                                }
                                                if ((j & 18014408174206976L) == 0 || (j2 & 264) != 0) {
                                                    if (dat4 != 0) {
                                                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                                                    } else {
                                                        addrU19 = str;
                                                    }
                                                    updateRegistration(20, addrU19);
                                                    if (addrU19 != 0) {
                                                        data27 = addrU19.getData();
                                                    } else {
                                                        data27 = 0;
                                                    }
                                                    if ((j & 18014407100465152L) == 0 || (j2 & 256) != 0) {
                                                        strValueOf14 = String.valueOf(data27);
                                                        int i12 = data27;
                                                        str20 = strValueOf13;
                                                        str21 = strValueOf12;
                                                        i8 = i12;
                                                    } else {
                                                        int i13 = data27;
                                                        str20 = strValueOf13;
                                                        str21 = strValueOf12;
                                                        i8 = i13;
                                                        strValueOf14 = str;
                                                    }
                                                } else {
                                                    str20 = strValueOf13;
                                                    strValueOf14 = str;
                                                    str21 = strValueOf12;
                                                }
                                                if ((j & 72057602632056832L) == 0 || (j2 & 256) != 0) {
                                                    if (modPara != 0) {
                                                        ch5 = modPara.getCh();
                                                    } else {
                                                        ch5 = str;
                                                    }
                                                    str22 = str21;
                                                    updateRegistration(22, ch5);
                                                    data28 = ch5 != 0 ? ch5.getData() : 0;
                                                    i9 = data19;
                                                    str23 = str22;
                                                } else {
                                                    str22 = str21;
                                                }
                                                i9 = data19;
                                                str23 = str22;
                                            } else {
                                                data24 = data24;
                                                strValueOf11 = strValueOf11;
                                                str21 = str;
                                                str20 = str21;
                                                strValueOf14 = str20;
                                            }
                                            i8 = 0;
                                            if ((j & 72057602632056832L) == 0) {
                                                if (modPara != 0) {
                                                    ch5 = modPara.getCh();
                                                } else {
                                                    ch5 = str;
                                                }
                                                str22 = str21;
                                                updateRegistration(22, ch5);
                                                if (ch5 != 0) {
                                                }
                                            } else {
                                                if (modPara != 0) {
                                                    ch5 = modPara.getCh();
                                                } else {
                                                    ch5 = str;
                                                }
                                                str22 = str21;
                                                updateRegistration(22, ch5);
                                                if (ch5 != 0) {
                                                }
                                            }
                                            i9 = data19;
                                            str23 = str22;
                                        } else {
                                            str17 = str17;
                                            str19 = str18;
                                        }
                                        if ((j & 18155162842439681L) == 0) {
                                            if (modPara != 0) {
                                                dat4 = modPara.getDat();
                                            } else {
                                                dat4 = str;
                                            }
                                            if ((j & 25769803777L) == 0) {
                                                if (dat4 != 0) {
                                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                                } else {
                                                    addrU17 = str;
                                                }
                                                updateRegistration(0, addrU17);
                                                if (addrU17 != 0) {
                                                    data25 = addrU17.getData();
                                                } else {
                                                    data25 = 0;
                                                }
                                                strValueOf12 = String.valueOf(data25);
                                            } else {
                                                if (dat4 != 0) {
                                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                                } else {
                                                    addrU17 = str;
                                                }
                                                updateRegistration(0, addrU17);
                                                if (addrU17 != 0) {
                                                    data25 = addrU17.getData();
                                                } else {
                                                    data25 = 0;
                                                }
                                                strValueOf12 = String.valueOf(data25);
                                            }
                                            if ((j & 140746078298112L) == 0) {
                                                if (dat4 != 0) {
                                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                                } else {
                                                    addrU18 = str;
                                                }
                                                updateRegistration(13, addrU18);
                                                if (addrU18 != 0) {
                                                    data26 = addrU18.getData();
                                                } else {
                                                    data26 = 0;
                                                }
                                                strValueOf13 = String.valueOf(data26);
                                            } else {
                                                if (dat4 != 0) {
                                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                                } else {
                                                    addrU18 = str;
                                                }
                                                updateRegistration(13, addrU18);
                                                if (addrU18 != 0) {
                                                    data26 = addrU18.getData();
                                                } else {
                                                    data26 = 0;
                                                }
                                                strValueOf13 = String.valueOf(data26);
                                            }
                                            if ((j & 18014408174206976L) == 0) {
                                            }
                                            if (dat4 != 0) {
                                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                                            } else {
                                                addrU19 = str;
                                            }
                                            updateRegistration(20, addrU19);
                                            if (addrU19 != 0) {
                                                data27 = addrU19.getData();
                                            } else {
                                                data27 = 0;
                                            }
                                            if ((j & 18014407100465152L) == 0) {
                                                strValueOf14 = String.valueOf(data27);
                                                int i14 = data27;
                                                str20 = strValueOf13;
                                                str21 = strValueOf12;
                                                i8 = i14;
                                            } else {
                                                strValueOf14 = String.valueOf(data27);
                                                int i15 = data27;
                                                str20 = strValueOf13;
                                                str21 = strValueOf12;
                                                i8 = i15;
                                            }
                                        } else {
                                            if (modPara != 0) {
                                                dat4 = modPara.getDat();
                                            } else {
                                                dat4 = str;
                                            }
                                            if ((j & 25769803777L) == 0) {
                                                if (dat4 != 0) {
                                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                                } else {
                                                    addrU17 = str;
                                                }
                                                updateRegistration(0, addrU17);
                                                if (addrU17 != 0) {
                                                    data25 = addrU17.getData();
                                                } else {
                                                    data25 = 0;
                                                }
                                                strValueOf12 = String.valueOf(data25);
                                            } else {
                                                if (dat4 != 0) {
                                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                                } else {
                                                    addrU17 = str;
                                                }
                                                updateRegistration(0, addrU17);
                                                if (addrU17 != 0) {
                                                    data25 = addrU17.getData();
                                                } else {
                                                    data25 = 0;
                                                }
                                                strValueOf12 = String.valueOf(data25);
                                            }
                                            if ((j & 140746078298112L) == 0) {
                                                if (dat4 != 0) {
                                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                                } else {
                                                    addrU18 = str;
                                                }
                                                updateRegistration(13, addrU18);
                                                if (addrU18 != 0) {
                                                    data26 = addrU18.getData();
                                                } else {
                                                    data26 = 0;
                                                }
                                                strValueOf13 = String.valueOf(data26);
                                            } else {
                                                if (dat4 != 0) {
                                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                                } else {
                                                    addrU18 = str;
                                                }
                                                updateRegistration(13, addrU18);
                                                if (addrU18 != 0) {
                                                    data26 = addrU18.getData();
                                                } else {
                                                    data26 = 0;
                                                }
                                                strValueOf13 = String.valueOf(data26);
                                            }
                                            if ((j & 18014408174206976L) == 0) {
                                            }
                                            if (dat4 != 0) {
                                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                                            } else {
                                                addrU19 = str;
                                            }
                                            updateRegistration(20, addrU19);
                                            if (addrU19 != 0) {
                                                data27 = addrU19.getData();
                                            } else {
                                                data27 = 0;
                                            }
                                            if ((j & 18014407100465152L) == 0) {
                                                strValueOf14 = String.valueOf(data27);
                                                int i16 = data27;
                                                str20 = strValueOf13;
                                                str21 = strValueOf12;
                                                i8 = i16;
                                            } else {
                                                strValueOf14 = String.valueOf(data27);
                                                int i17 = data27;
                                                str20 = strValueOf13;
                                                str21 = strValueOf12;
                                                i8 = i17;
                                            }
                                        }
                                        if ((j & 72057602632056832L) == 0) {
                                            if (modPara != 0) {
                                                ch5 = modPara.getCh();
                                            } else {
                                                ch5 = str;
                                            }
                                            str22 = str21;
                                            updateRegistration(22, ch5);
                                            if (ch5 != 0) {
                                            }
                                        } else {
                                            if (modPara != 0) {
                                                ch5 = modPara.getCh();
                                            } else {
                                                ch5 = str;
                                            }
                                            str22 = str21;
                                            updateRegistration(22, ch5);
                                            if (ch5 != 0) {
                                            }
                                        }
                                        i9 = data19;
                                        str23 = str22;
                                    } else {
                                        j2 = j2;
                                        strValueOf = strValueOf;
                                        data7 = data7;
                                        i5 = i5;
                                        i = i;
                                        i2 = i2;
                                        data5 = data5;
                                        str5 = str5;
                                        data12 = data12;
                                        data18 = data18;
                                        sysEx = sysEx;
                                        str8 = str8;
                                        i6 = i6;
                                        i7 = i7;
                                        str9 = str9;
                                        data23 = data23;
                                        str10 = str10;
                                        i8 = 0;
                                        data24 = 0;
                                        str19 = str18;
                                        strValueOf11 = strValueOf11;
                                        i4 = i4;
                                        usrBinding = usrBinding;
                                        i10 = i10;
                                        data = data;
                                        str20 = str;
                                        str23 = str20;
                                        strValueOf14 = str23;
                                        strValueOf2 = strValueOf2;
                                        data15 = data15;
                                        str14 = str14;
                                        data28 = 0;
                                        str17 = str17;
                                        data3 = data3;
                                        str12 = str12;
                                        i9 = data19;
                                    }
                                }
                                int i18 = i3;
                                str5 = strValueOf5;
                                i4 = i18;
                                i5 = data11;
                                str6 = str2;
                                str7 = str4;
                            } else {
                                str2 = str2;
                                str4 = str3;
                            }
                            if (i10 == 0) {
                                if (btnPara != 0) {
                                    sys = btnPara.getSys();
                                } else {
                                    sys = str;
                                }
                                if (smk25MiniUsrFragment != null) {
                                    sysEx = smk25MiniUsrFragment.getSysEx(sys);
                                } else {
                                    sysEx = str;
                                }
                            } else {
                                if (btnPara != 0) {
                                    sys = btnPara.getSys();
                                } else {
                                    sys = str;
                                }
                                if (smk25MiniUsrFragment != null) {
                                    sysEx = smk25MiniUsrFragment.getSysEx(sys);
                                } else {
                                    sysEx = str;
                                }
                            }
                            if ((j & 36028805610995712L) == 0) {
                                if (btnPara != 0) {
                                    type = btnPara.getType();
                                } else {
                                    type = str;
                                }
                                updateRegistration(21, type);
                                if (type != 0) {
                                    int i19 = i3;
                                    str5 = strValueOf5;
                                    i4 = i19;
                                    i5 = data11;
                                    str6 = str2;
                                    data12 = type.getData();
                                    str7 = str4;
                                } else {
                                    int i110 = i3;
                                    str5 = strValueOf5;
                                    i4 = i110;
                                    i5 = data11;
                                    str6 = str2;
                                    str7 = str4;
                                }
                            } else {
                                if (btnPara != 0) {
                                    type = btnPara.getType();
                                } else {
                                    type = str;
                                }
                                updateRegistration(21, type);
                                if (type != 0) {
                                    int i111 = i3;
                                    str5 = strValueOf5;
                                    i4 = i111;
                                    i5 = data11;
                                    str6 = str2;
                                    data12 = type.getData();
                                    str7 = str4;
                                } else {
                                    int i112 = i3;
                                    str5 = strValueOf5;
                                    i4 = i112;
                                    i5 = data11;
                                    str6 = str2;
                                    str7 = str4;
                                }
                            }
                            if ((j & 2817920526876856L) == 0) {
                                if (smk25MiniUsr != null) {
                                    encoderPara = smk25MiniUsr.getEncoderPara();
                                } else {
                                    encoderPara = str;
                                }
                                if ((j & 283467841552L) == 0) {
                                    if (encoderPara != 0) {
                                        ch3 = encoderPara.getCh();
                                    } else {
                                        ch3 = str;
                                    }
                                    str8 = str6;
                                    updateRegistration(4, ch3);
                                    if (ch3 != 0) {
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        ch3 = encoderPara.getCh();
                                    } else {
                                        ch3 = str;
                                    }
                                    str8 = str6;
                                    updateRegistration(4, ch3);
                                    if (ch3 != 0) {
                                    }
                                }
                                if ((j & 2207613190272L) == 0) {
                                    if (encoderPara != 0) {
                                        curve = encoderPara.getCurve();
                                    } else {
                                        curve = str;
                                    }
                                    i6 = data13;
                                    updateRegistration(7, curve);
                                    if (curve != 0) {
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        curve = encoderPara.getCurve();
                                    } else {
                                        curve = str;
                                    }
                                    i6 = data13;
                                    updateRegistration(7, curve);
                                    if (curve != 0) {
                                    }
                                }
                                if ((j & 563646811897896L) == 0) {
                                    if (encoderPara != 0) {
                                        dat2 = encoderPara.getDat();
                                    } else {
                                        dat2 = str;
                                    }
                                    if ((j & 147102629896L) == 0) {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    } else {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    }
                                    if ((j & 558345748512L) == 0) {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    }
                                    if ((j & 562958543388672L) == 0) {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    }
                                    str10 = strValueOf6;
                                } else {
                                    if (encoderPara != 0) {
                                        dat2 = encoderPara.getDat();
                                    } else {
                                        dat2 = str;
                                    }
                                    if ((j & 147102629896L) == 0) {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    } else {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    }
                                    if ((j & 558345748512L) == 0) {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    }
                                    if ((j & 562958543388672L) == 0) {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    }
                                    str10 = strValueOf6;
                                }
                                if ((j & 2251808403750912L) == 0) {
                                    if (encoderPara != 0) {
                                        type2 = encoderPara.getType();
                                    } else {
                                        type2 = str;
                                    }
                                    str11 = strValueOf8;
                                    updateRegistration(17, type2);
                                    if (type2 != 0) {
                                        data18 = type2.getData();
                                        str12 = strValueOf7;
                                        str13 = str11;
                                    } else {
                                        str12 = strValueOf7;
                                        str13 = str11;
                                        data18 = 0;
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        type2 = encoderPara.getType();
                                    } else {
                                        type2 = str;
                                    }
                                    str11 = strValueOf8;
                                    updateRegistration(17, type2);
                                    if (type2 != 0) {
                                        data18 = type2.getData();
                                        str12 = strValueOf7;
                                        str13 = str11;
                                    } else {
                                        str12 = strValueOf7;
                                        str13 = str11;
                                        data18 = 0;
                                    }
                                }
                            } else {
                                if (smk25MiniUsr != null) {
                                    encoderPara = smk25MiniUsr.getEncoderPara();
                                } else {
                                    encoderPara = str;
                                }
                                if ((j & 283467841552L) == 0) {
                                    if (encoderPara != 0) {
                                        ch3 = encoderPara.getCh();
                                    } else {
                                        ch3 = str;
                                    }
                                    str8 = str6;
                                    updateRegistration(4, ch3);
                                    if (ch3 != 0) {
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        ch3 = encoderPara.getCh();
                                    } else {
                                        ch3 = str;
                                    }
                                    str8 = str6;
                                    updateRegistration(4, ch3);
                                    if (ch3 != 0) {
                                    }
                                }
                                if ((j & 2207613190272L) == 0) {
                                    if (encoderPara != 0) {
                                        curve = encoderPara.getCurve();
                                    } else {
                                        curve = str;
                                    }
                                    i6 = data13;
                                    updateRegistration(7, curve);
                                    if (curve != 0) {
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        curve = encoderPara.getCurve();
                                    } else {
                                        curve = str;
                                    }
                                    i6 = data13;
                                    updateRegistration(7, curve);
                                    if (curve != 0) {
                                    }
                                }
                                if ((j & 563646811897896L) == 0) {
                                    if (encoderPara != 0) {
                                        dat2 = encoderPara.getDat();
                                    } else {
                                        dat2 = str;
                                    }
                                    if ((j & 147102629896L) == 0) {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    } else {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    }
                                    if ((j & 558345748512L) == 0) {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    }
                                    if ((j & 562958543388672L) == 0) {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    }
                                    str10 = strValueOf6;
                                } else {
                                    if (encoderPara != 0) {
                                        dat2 = encoderPara.getDat();
                                    } else {
                                        dat2 = str;
                                    }
                                    if ((j & 147102629896L) == 0) {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    } else {
                                        i7 = data14;
                                        if (dat2 != 0) {
                                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                                        } else {
                                            addrU11 = str;
                                        }
                                        str9 = str7;
                                        updateRegistration(3, addrU11);
                                        if (addrU11 != 0) {
                                            data15 = addrU11.getData();
                                        } else {
                                            data15 = 0;
                                        }
                                        if ((j & 146028888072L) == 0) {
                                            strValueOf6 = String.valueOf(data15);
                                        } else {
                                            strValueOf6 = String.valueOf(data15);
                                        }
                                    }
                                    if ((j & 558345748512L) == 0) {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                                        } else {
                                            addrU12 = str;
                                        }
                                        updateRegistration(5, addrU12);
                                        if (addrU12 != 0) {
                                            data16 = addrU12.getData();
                                        } else {
                                            data16 = 0;
                                        }
                                        strValueOf7 = String.valueOf(data16);
                                    }
                                    if ((j & 562958543388672L) == 0) {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    } else {
                                        if (dat2 != 0) {
                                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                                        } else {
                                            addrU13 = str;
                                        }
                                        updateRegistration(15, addrU13);
                                        if (addrU13 != 0) {
                                            data17 = addrU13.getData();
                                        } else {
                                            data17 = 0;
                                        }
                                        strValueOf8 = String.valueOf(data17);
                                    }
                                    str10 = strValueOf6;
                                }
                                if ((j & 2251808403750912L) == 0) {
                                    if (encoderPara != 0) {
                                        type2 = encoderPara.getType();
                                    } else {
                                        type2 = str;
                                    }
                                    str11 = strValueOf8;
                                    updateRegistration(17, type2);
                                    if (type2 != 0) {
                                        data18 = type2.getData();
                                        str12 = strValueOf7;
                                        str13 = str11;
                                    } else {
                                        str12 = strValueOf7;
                                        str13 = str11;
                                        data18 = 0;
                                    }
                                } else {
                                    if (encoderPara != 0) {
                                        type2 = encoderPara.getType();
                                    } else {
                                        type2 = str;
                                    }
                                    str11 = strValueOf8;
                                    updateRegistration(17, type2);
                                    if (type2 != 0) {
                                        data18 = type2.getData();
                                        str12 = strValueOf7;
                                        str13 = str11;
                                    } else {
                                        str12 = strValueOf7;
                                        str13 = str11;
                                        data18 = 0;
                                    }
                                }
                            }
                            if ((j & (-6620287044909793024L)) == 0) {
                                if (smk25MiniUsr != null) {
                                    pedalPara = smk25MiniUsr.getPedalPara();
                                } else {
                                    pedalPara = str;
                                }
                                if ((j & 288230384758423552L) == 0) {
                                    if (pedalPara != 0) {
                                        type3 = pedalPara.getType();
                                    } else {
                                        type3 = str;
                                    }
                                    str14 = str13;
                                    updateRegistration(24, type3);
                                    if (type3 != 0) {
                                    }
                                } else {
                                    if (pedalPara != 0) {
                                        type3 = pedalPara.getType();
                                    } else {
                                        type3 = str;
                                    }
                                    str14 = str13;
                                    updateRegistration(24, type3);
                                    if (type3 != 0) {
                                    }
                                }
                                if ((j & 2314854615239622912L) == 0) {
                                    if (pedalPara != 0) {
                                        dat3 = pedalPara.getDat();
                                    } else {
                                        dat3 = str;
                                    }
                                    if ((j & 4406636445952L) == 0) {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    }
                                    if ((j & 9007207845199872L) == 0) {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    }
                                    if ((j & 2305843017937846272L) == 0) {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    }
                                    str15 = strValueOf9;
                                } else {
                                    if (pedalPara != 0) {
                                        dat3 = pedalPara.getDat();
                                    } else {
                                        dat3 = str;
                                    }
                                    if ((j & 4406636445952L) == 0) {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    }
                                    if ((j & 9007207845199872L) == 0) {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    }
                                    if ((j & 2305843017937846272L) == 0) {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    }
                                    str15 = strValueOf9;
                                }
                                if ((j & (-9223372027727970304L)) == 0) {
                                    if (pedalPara != 0) {
                                        ch4 = pedalPara.getCh();
                                    } else {
                                        ch4 = str;
                                    }
                                    str16 = strValueOf10;
                                    updateRegistration(29, ch4);
                                    if (ch4 != 0) {
                                        data23 = ch4.getData();
                                        str17 = str15;
                                        str18 = str16;
                                    } else {
                                        str17 = str15;
                                        str18 = str16;
                                        data23 = 0;
                                    }
                                } else {
                                    if (pedalPara != 0) {
                                        ch4 = pedalPara.getCh();
                                    } else {
                                        ch4 = str;
                                    }
                                    str16 = strValueOf10;
                                    updateRegistration(29, ch4);
                                    if (ch4 != 0) {
                                        data23 = ch4.getData();
                                        str17 = str15;
                                        str18 = str16;
                                    } else {
                                        str17 = str15;
                                        str18 = str16;
                                        data23 = 0;
                                    }
                                }
                            } else {
                                if (smk25MiniUsr != null) {
                                    pedalPara = smk25MiniUsr.getPedalPara();
                                } else {
                                    pedalPara = str;
                                }
                                if ((j & 288230384758423552L) == 0) {
                                    if (pedalPara != 0) {
                                        type3 = pedalPara.getType();
                                    } else {
                                        type3 = str;
                                    }
                                    str14 = str13;
                                    updateRegistration(24, type3);
                                    if (type3 != 0) {
                                    }
                                } else {
                                    if (pedalPara != 0) {
                                        type3 = pedalPara.getType();
                                    } else {
                                        type3 = str;
                                    }
                                    str14 = str13;
                                    updateRegistration(24, type3);
                                    if (type3 != 0) {
                                    }
                                }
                                if ((j & 2314854615239622912L) == 0) {
                                    if (pedalPara != 0) {
                                        dat3 = pedalPara.getDat();
                                    } else {
                                        dat3 = str;
                                    }
                                    if ((j & 4406636445952L) == 0) {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    }
                                    if ((j & 9007207845199872L) == 0) {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    }
                                    if ((j & 2305843017937846272L) == 0) {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    }
                                    str15 = strValueOf9;
                                } else {
                                    if (pedalPara != 0) {
                                        dat3 = pedalPara.getDat();
                                    } else {
                                        dat3 = str;
                                    }
                                    if ((j & 4406636445952L) == 0) {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                                        } else {
                                            addrU14 = str;
                                        }
                                        updateRegistration(8, addrU14);
                                        if (addrU14 != 0) {
                                            data20 = addrU14.getData();
                                        } else {
                                            data20 = 0;
                                        }
                                        strValueOf9 = String.valueOf(data20);
                                    }
                                    if ((j & 9007207845199872L) == 0) {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                                        } else {
                                            addrU15 = str;
                                        }
                                        updateRegistration(19, addrU15);
                                        if (addrU15 != 0) {
                                            data21 = addrU15.getData();
                                        } else {
                                            data21 = 0;
                                        }
                                        strValueOf10 = String.valueOf(data21);
                                    }
                                    if ((j & 2305843017937846272L) == 0) {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    } else {
                                        if (dat3 != 0) {
                                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                                        } else {
                                            addrU16 = str;
                                        }
                                        updateRegistration(27, addrU16);
                                        if (addrU16 != 0) {
                                            data22 = addrU16.getData();
                                        } else {
                                            data22 = 0;
                                        }
                                        strValueOf11 = String.valueOf(data22);
                                    }
                                    str15 = strValueOf9;
                                }
                                if ((j & (-9223372027727970304L)) == 0) {
                                    if (pedalPara != 0) {
                                        ch4 = pedalPara.getCh();
                                    } else {
                                        ch4 = str;
                                    }
                                    str16 = strValueOf10;
                                    updateRegistration(29, ch4);
                                    if (ch4 != 0) {
                                        data23 = ch4.getData();
                                        str17 = str15;
                                        str18 = str16;
                                    } else {
                                        str17 = str15;
                                        str18 = str16;
                                        data23 = 0;
                                    }
                                } else {
                                    if (pedalPara != 0) {
                                        ch4 = pedalPara.getCh();
                                    } else {
                                        ch4 = str;
                                    }
                                    str16 = strValueOf10;
                                    updateRegistration(29, ch4);
                                    if (ch4 != 0) {
                                        data23 = ch4.getData();
                                        str17 = str15;
                                        str18 = str16;
                                    } else {
                                        str17 = str15;
                                        str18 = str16;
                                        data23 = 0;
                                    }
                                }
                            }
                            if ((j & 90212791244300291L) == 0) {
                                if (smk25MiniUsr != null) {
                                    modPara = smk25MiniUsr.getModPara();
                                } else {
                                    modPara = str;
                                }
                                if ((j & 42949672962L) == 0) {
                                    if (modPara != 0) {
                                        speed = modPara.getSpeed();
                                    } else {
                                        speed = str;
                                    }
                                    str19 = str18;
                                    updateRegistration(1, speed);
                                    if (speed != 0) {
                                    }
                                } else {
                                    if (modPara != 0) {
                                        speed = modPara.getSpeed();
                                    } else {
                                        speed = str;
                                    }
                                    str19 = str18;
                                    updateRegistration(1, speed);
                                    if (speed != 0) {
                                    }
                                }
                                if ((j & 18155162842439681L) == 0) {
                                    if (modPara != 0) {
                                        dat4 = modPara.getDat();
                                    } else {
                                        dat4 = str;
                                    }
                                    if ((j & 25769803777L) == 0) {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    }
                                    if ((j & 140746078298112L) == 0) {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    }
                                    if ((j & 18014408174206976L) == 0) {
                                    }
                                    if (dat4 != 0) {
                                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                                    } else {
                                        addrU19 = str;
                                    }
                                    updateRegistration(20, addrU19);
                                    if (addrU19 != 0) {
                                        data27 = addrU19.getData();
                                    } else {
                                        data27 = 0;
                                    }
                                    if ((j & 18014407100465152L) == 0) {
                                        strValueOf14 = String.valueOf(data27);
                                        int i113 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i113;
                                    } else {
                                        strValueOf14 = String.valueOf(data27);
                                        int i114 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i114;
                                    }
                                } else {
                                    if (modPara != 0) {
                                        dat4 = modPara.getDat();
                                    } else {
                                        dat4 = str;
                                    }
                                    if ((j & 25769803777L) == 0) {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    }
                                    if ((j & 140746078298112L) == 0) {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    }
                                    if ((j & 18014408174206976L) == 0) {
                                    }
                                    if (dat4 != 0) {
                                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                                    } else {
                                        addrU19 = str;
                                    }
                                    updateRegistration(20, addrU19);
                                    if (addrU19 != 0) {
                                        data27 = addrU19.getData();
                                    } else {
                                        data27 = 0;
                                    }
                                    if ((j & 18014407100465152L) == 0) {
                                        strValueOf14 = String.valueOf(data27);
                                        int i115 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i115;
                                    } else {
                                        strValueOf14 = String.valueOf(data27);
                                        int i116 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i116;
                                    }
                                }
                                if ((j & 72057602632056832L) == 0) {
                                    if (modPara != 0) {
                                        ch5 = modPara.getCh();
                                    } else {
                                        ch5 = str;
                                    }
                                    str22 = str21;
                                    updateRegistration(22, ch5);
                                    if (ch5 != 0) {
                                    }
                                } else {
                                    if (modPara != 0) {
                                        ch5 = modPara.getCh();
                                    } else {
                                        ch5 = str;
                                    }
                                    str22 = str21;
                                    updateRegistration(22, ch5);
                                    if (ch5 != 0) {
                                    }
                                }
                                i9 = data19;
                                str23 = str22;
                            } else {
                                if (smk25MiniUsr != null) {
                                    modPara = smk25MiniUsr.getModPara();
                                } else {
                                    modPara = str;
                                }
                                if ((j & 42949672962L) == 0) {
                                    if (modPara != 0) {
                                        speed = modPara.getSpeed();
                                    } else {
                                        speed = str;
                                    }
                                    str19 = str18;
                                    updateRegistration(1, speed);
                                    if (speed != 0) {
                                    }
                                } else {
                                    if (modPara != 0) {
                                        speed = modPara.getSpeed();
                                    } else {
                                        speed = str;
                                    }
                                    str19 = str18;
                                    updateRegistration(1, speed);
                                    if (speed != 0) {
                                    }
                                }
                                if ((j & 18155162842439681L) == 0) {
                                    if (modPara != 0) {
                                        dat4 = modPara.getDat();
                                    } else {
                                        dat4 = str;
                                    }
                                    if ((j & 25769803777L) == 0) {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    }
                                    if ((j & 140746078298112L) == 0) {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    }
                                    if ((j & 18014408174206976L) == 0) {
                                    }
                                    if (dat4 != 0) {
                                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                                    } else {
                                        addrU19 = str;
                                    }
                                    updateRegistration(20, addrU19);
                                    if (addrU19 != 0) {
                                        data27 = addrU19.getData();
                                    } else {
                                        data27 = 0;
                                    }
                                    if ((j & 18014407100465152L) == 0) {
                                        strValueOf14 = String.valueOf(data27);
                                        int i117 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i117;
                                    } else {
                                        strValueOf14 = String.valueOf(data27);
                                        int i118 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i118;
                                    }
                                } else {
                                    if (modPara != 0) {
                                        dat4 = modPara.getDat();
                                    } else {
                                        dat4 = str;
                                    }
                                    if ((j & 25769803777L) == 0) {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                                        } else {
                                            addrU17 = str;
                                        }
                                        updateRegistration(0, addrU17);
                                        if (addrU17 != 0) {
                                            data25 = addrU17.getData();
                                        } else {
                                            data25 = 0;
                                        }
                                        strValueOf12 = String.valueOf(data25);
                                    }
                                    if ((j & 140746078298112L) == 0) {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    } else {
                                        if (dat4 != 0) {
                                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                                        } else {
                                            addrU18 = str;
                                        }
                                        updateRegistration(13, addrU18);
                                        if (addrU18 != 0) {
                                            data26 = addrU18.getData();
                                        } else {
                                            data26 = 0;
                                        }
                                        strValueOf13 = String.valueOf(data26);
                                    }
                                    if ((j & 18014408174206976L) == 0) {
                                    }
                                    if (dat4 != 0) {
                                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                                    } else {
                                        addrU19 = str;
                                    }
                                    updateRegistration(20, addrU19);
                                    if (addrU19 != 0) {
                                        data27 = addrU19.getData();
                                    } else {
                                        data27 = 0;
                                    }
                                    if ((j & 18014407100465152L) == 0) {
                                        strValueOf14 = String.valueOf(data27);
                                        int i119 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i119;
                                    } else {
                                        strValueOf14 = String.valueOf(data27);
                                        int i1110 = data27;
                                        str20 = strValueOf13;
                                        str21 = strValueOf12;
                                        i8 = i1110;
                                    }
                                }
                                if ((j & 72057602632056832L) == 0) {
                                    if (modPara != 0) {
                                        ch5 = modPara.getCh();
                                    } else {
                                        ch5 = str;
                                    }
                                    str22 = str21;
                                    updateRegistration(22, ch5);
                                    if (ch5 != 0) {
                                    }
                                } else {
                                    if (modPara != 0) {
                                        ch5 = modPara.getCh();
                                    } else {
                                        ch5 = str;
                                    }
                                    str22 = str21;
                                    updateRegistration(22, ch5);
                                    if (ch5 != 0) {
                                    }
                                }
                                i9 = data19;
                                str23 = str22;
                            }
                        } else {
                            data5 = data5;
                            strValueOf2 = strValueOf2;
                            str2 = str;
                            str3 = str2;
                            strValueOf5 = str3;
                        }
                        i3 = 0;
                        if ((j & 281483566661632L) == 0) {
                            if (btnPara != 0) {
                                ch2 = btnPara.getCh();
                            } else {
                                ch2 = str;
                            }
                            str4 = str3;
                            updateRegistration(14, ch2);
                            if (ch2 != 0) {
                            }
                        } else {
                            if (btnPara != 0) {
                                ch2 = btnPara.getCh();
                            } else {
                                ch2 = str;
                            }
                            str4 = str3;
                            updateRegistration(14, ch2);
                            if (ch2 != 0) {
                            }
                        }
                        if (i10 == 0) {
                            if (btnPara != 0) {
                                sys = btnPara.getSys();
                            } else {
                                sys = str;
                            }
                            if (smk25MiniUsrFragment != null) {
                                sysEx = smk25MiniUsrFragment.getSysEx(sys);
                            } else {
                                sysEx = str;
                            }
                        } else {
                            if (btnPara != 0) {
                                sys = btnPara.getSys();
                            } else {
                                sys = str;
                            }
                            if (smk25MiniUsrFragment != null) {
                                sysEx = smk25MiniUsrFragment.getSysEx(sys);
                            } else {
                                sysEx = str;
                            }
                        }
                        if ((j & 36028805610995712L) == 0) {
                            if (btnPara != 0) {
                                type = btnPara.getType();
                            } else {
                                type = str;
                            }
                            updateRegistration(21, type);
                            if (type != 0) {
                                int i1111 = i3;
                                str5 = strValueOf5;
                                i4 = i1111;
                                i5 = data11;
                                str6 = str2;
                                data12 = type.getData();
                                str7 = str4;
                            } else {
                                int i1112 = i3;
                                str5 = strValueOf5;
                                i4 = i1112;
                                i5 = data11;
                                str6 = str2;
                                str7 = str4;
                            }
                        } else {
                            if (btnPara != 0) {
                                type = btnPara.getType();
                            } else {
                                type = str;
                            }
                            updateRegistration(21, type);
                            if (type != 0) {
                                int i1113 = i3;
                                str5 = strValueOf5;
                                i4 = i1113;
                                i5 = data11;
                                str6 = str2;
                                data12 = type.getData();
                                str7 = str4;
                            } else {
                                int i1114 = i3;
                                str5 = strValueOf5;
                                i4 = i1114;
                                i5 = data11;
                                str6 = str2;
                                str7 = str4;
                            }
                        }
                        if ((j & 2817920526876856L) == 0) {
                            if (smk25MiniUsr != null) {
                                encoderPara = smk25MiniUsr.getEncoderPara();
                            } else {
                                encoderPara = str;
                            }
                            if ((j & 283467841552L) == 0) {
                                if (encoderPara != 0) {
                                    ch3 = encoderPara.getCh();
                                } else {
                                    ch3 = str;
                                }
                                str8 = str6;
                                updateRegistration(4, ch3);
                                if (ch3 != 0) {
                                }
                            } else {
                                if (encoderPara != 0) {
                                    ch3 = encoderPara.getCh();
                                } else {
                                    ch3 = str;
                                }
                                str8 = str6;
                                updateRegistration(4, ch3);
                                if (ch3 != 0) {
                                }
                            }
                            if ((j & 2207613190272L) == 0) {
                                if (encoderPara != 0) {
                                    curve = encoderPara.getCurve();
                                } else {
                                    curve = str;
                                }
                                i6 = data13;
                                updateRegistration(7, curve);
                                if (curve != 0) {
                                }
                            } else {
                                if (encoderPara != 0) {
                                    curve = encoderPara.getCurve();
                                } else {
                                    curve = str;
                                }
                                i6 = data13;
                                updateRegistration(7, curve);
                                if (curve != 0) {
                                }
                            }
                            if ((j & 563646811897896L) == 0) {
                                if (encoderPara != 0) {
                                    dat2 = encoderPara.getDat();
                                } else {
                                    dat2 = str;
                                }
                                if ((j & 147102629896L) == 0) {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                } else {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                }
                                if ((j & 558345748512L) == 0) {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                } else {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                }
                                if ((j & 562958543388672L) == 0) {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                } else {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                }
                                str10 = strValueOf6;
                            } else {
                                if (encoderPara != 0) {
                                    dat2 = encoderPara.getDat();
                                } else {
                                    dat2 = str;
                                }
                                if ((j & 147102629896L) == 0) {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                } else {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                }
                                if ((j & 558345748512L) == 0) {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                } else {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                }
                                if ((j & 562958543388672L) == 0) {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                } else {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                }
                                str10 = strValueOf6;
                            }
                            if ((j & 2251808403750912L) == 0) {
                                if (encoderPara != 0) {
                                    type2 = encoderPara.getType();
                                } else {
                                    type2 = str;
                                }
                                str11 = strValueOf8;
                                updateRegistration(17, type2);
                                if (type2 != 0) {
                                    data18 = type2.getData();
                                    str12 = strValueOf7;
                                    str13 = str11;
                                } else {
                                    str12 = strValueOf7;
                                    str13 = str11;
                                    data18 = 0;
                                }
                            } else {
                                if (encoderPara != 0) {
                                    type2 = encoderPara.getType();
                                } else {
                                    type2 = str;
                                }
                                str11 = strValueOf8;
                                updateRegistration(17, type2);
                                if (type2 != 0) {
                                    data18 = type2.getData();
                                    str12 = strValueOf7;
                                    str13 = str11;
                                } else {
                                    str12 = strValueOf7;
                                    str13 = str11;
                                    data18 = 0;
                                }
                            }
                        } else {
                            if (smk25MiniUsr != null) {
                                encoderPara = smk25MiniUsr.getEncoderPara();
                            } else {
                                encoderPara = str;
                            }
                            if ((j & 283467841552L) == 0) {
                                if (encoderPara != 0) {
                                    ch3 = encoderPara.getCh();
                                } else {
                                    ch3 = str;
                                }
                                str8 = str6;
                                updateRegistration(4, ch3);
                                if (ch3 != 0) {
                                }
                            } else {
                                if (encoderPara != 0) {
                                    ch3 = encoderPara.getCh();
                                } else {
                                    ch3 = str;
                                }
                                str8 = str6;
                                updateRegistration(4, ch3);
                                if (ch3 != 0) {
                                }
                            }
                            if ((j & 2207613190272L) == 0) {
                                if (encoderPara != 0) {
                                    curve = encoderPara.getCurve();
                                } else {
                                    curve = str;
                                }
                                i6 = data13;
                                updateRegistration(7, curve);
                                if (curve != 0) {
                                }
                            } else {
                                if (encoderPara != 0) {
                                    curve = encoderPara.getCurve();
                                } else {
                                    curve = str;
                                }
                                i6 = data13;
                                updateRegistration(7, curve);
                                if (curve != 0) {
                                }
                            }
                            if ((j & 563646811897896L) == 0) {
                                if (encoderPara != 0) {
                                    dat2 = encoderPara.getDat();
                                } else {
                                    dat2 = str;
                                }
                                if ((j & 147102629896L) == 0) {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                } else {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                }
                                if ((j & 558345748512L) == 0) {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                } else {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                }
                                if ((j & 562958543388672L) == 0) {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                } else {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                }
                                str10 = strValueOf6;
                            } else {
                                if (encoderPara != 0) {
                                    dat2 = encoderPara.getDat();
                                } else {
                                    dat2 = str;
                                }
                                if ((j & 147102629896L) == 0) {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                } else {
                                    i7 = data14;
                                    if (dat2 != 0) {
                                        addrU11 = (AddrU8) getFromArray(dat2, 0);
                                    } else {
                                        addrU11 = str;
                                    }
                                    str9 = str7;
                                    updateRegistration(3, addrU11);
                                    if (addrU11 != 0) {
                                        data15 = addrU11.getData();
                                    } else {
                                        data15 = 0;
                                    }
                                    if ((j & 146028888072L) == 0) {
                                        strValueOf6 = String.valueOf(data15);
                                    } else {
                                        strValueOf6 = String.valueOf(data15);
                                    }
                                }
                                if ((j & 558345748512L) == 0) {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                } else {
                                    if (dat2 != 0) {
                                        addrU12 = (AddrU8) getFromArray(dat2, 1);
                                    } else {
                                        addrU12 = str;
                                    }
                                    updateRegistration(5, addrU12);
                                    if (addrU12 != 0) {
                                        data16 = addrU12.getData();
                                    } else {
                                        data16 = 0;
                                    }
                                    strValueOf7 = String.valueOf(data16);
                                }
                                if ((j & 562958543388672L) == 0) {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                } else {
                                    if (dat2 != 0) {
                                        addrU13 = (AddrU8) getFromArray(dat2, 2);
                                    } else {
                                        addrU13 = str;
                                    }
                                    updateRegistration(15, addrU13);
                                    if (addrU13 != 0) {
                                        data17 = addrU13.getData();
                                    } else {
                                        data17 = 0;
                                    }
                                    strValueOf8 = String.valueOf(data17);
                                }
                                str10 = strValueOf6;
                            }
                            if ((j & 2251808403750912L) == 0) {
                                if (encoderPara != 0) {
                                    type2 = encoderPara.getType();
                                } else {
                                    type2 = str;
                                }
                                str11 = strValueOf8;
                                updateRegistration(17, type2);
                                if (type2 != 0) {
                                    data18 = type2.getData();
                                    str12 = strValueOf7;
                                    str13 = str11;
                                } else {
                                    str12 = strValueOf7;
                                    str13 = str11;
                                    data18 = 0;
                                }
                            } else {
                                if (encoderPara != 0) {
                                    type2 = encoderPara.getType();
                                } else {
                                    type2 = str;
                                }
                                str11 = strValueOf8;
                                updateRegistration(17, type2);
                                if (type2 != 0) {
                                    data18 = type2.getData();
                                    str12 = strValueOf7;
                                    str13 = str11;
                                } else {
                                    str12 = strValueOf7;
                                    str13 = str11;
                                    data18 = 0;
                                }
                            }
                        }
                        if ((j & (-6620287044909793024L)) == 0) {
                            if (smk25MiniUsr != null) {
                                pedalPara = smk25MiniUsr.getPedalPara();
                            } else {
                                pedalPara = str;
                            }
                            if ((j & 288230384758423552L) == 0) {
                                if (pedalPara != 0) {
                                    type3 = pedalPara.getType();
                                } else {
                                    type3 = str;
                                }
                                str14 = str13;
                                updateRegistration(24, type3);
                                if (type3 != 0) {
                                }
                            } else {
                                if (pedalPara != 0) {
                                    type3 = pedalPara.getType();
                                } else {
                                    type3 = str;
                                }
                                str14 = str13;
                                updateRegistration(24, type3);
                                if (type3 != 0) {
                                }
                            }
                            if ((j & 2314854615239622912L) == 0) {
                                if (pedalPara != 0) {
                                    dat3 = pedalPara.getDat();
                                } else {
                                    dat3 = str;
                                }
                                if ((j & 4406636445952L) == 0) {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                } else {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                }
                                if ((j & 9007207845199872L) == 0) {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                } else {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                }
                                if ((j & 2305843017937846272L) == 0) {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                } else {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                }
                                str15 = strValueOf9;
                            } else {
                                if (pedalPara != 0) {
                                    dat3 = pedalPara.getDat();
                                } else {
                                    dat3 = str;
                                }
                                if ((j & 4406636445952L) == 0) {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                } else {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                }
                                if ((j & 9007207845199872L) == 0) {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                } else {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                }
                                if ((j & 2305843017937846272L) == 0) {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                } else {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                }
                                str15 = strValueOf9;
                            }
                            if ((j & (-9223372027727970304L)) == 0) {
                                if (pedalPara != 0) {
                                    ch4 = pedalPara.getCh();
                                } else {
                                    ch4 = str;
                                }
                                str16 = strValueOf10;
                                updateRegistration(29, ch4);
                                if (ch4 != 0) {
                                    data23 = ch4.getData();
                                    str17 = str15;
                                    str18 = str16;
                                } else {
                                    str17 = str15;
                                    str18 = str16;
                                    data23 = 0;
                                }
                            } else {
                                if (pedalPara != 0) {
                                    ch4 = pedalPara.getCh();
                                } else {
                                    ch4 = str;
                                }
                                str16 = strValueOf10;
                                updateRegistration(29, ch4);
                                if (ch4 != 0) {
                                    data23 = ch4.getData();
                                    str17 = str15;
                                    str18 = str16;
                                } else {
                                    str17 = str15;
                                    str18 = str16;
                                    data23 = 0;
                                }
                            }
                        } else {
                            if (smk25MiniUsr != null) {
                                pedalPara = smk25MiniUsr.getPedalPara();
                            } else {
                                pedalPara = str;
                            }
                            if ((j & 288230384758423552L) == 0) {
                                if (pedalPara != 0) {
                                    type3 = pedalPara.getType();
                                } else {
                                    type3 = str;
                                }
                                str14 = str13;
                                updateRegistration(24, type3);
                                if (type3 != 0) {
                                }
                            } else {
                                if (pedalPara != 0) {
                                    type3 = pedalPara.getType();
                                } else {
                                    type3 = str;
                                }
                                str14 = str13;
                                updateRegistration(24, type3);
                                if (type3 != 0) {
                                }
                            }
                            if ((j & 2314854615239622912L) == 0) {
                                if (pedalPara != 0) {
                                    dat3 = pedalPara.getDat();
                                } else {
                                    dat3 = str;
                                }
                                if ((j & 4406636445952L) == 0) {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                } else {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                }
                                if ((j & 9007207845199872L) == 0) {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                } else {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                }
                                if ((j & 2305843017937846272L) == 0) {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                } else {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                }
                                str15 = strValueOf9;
                            } else {
                                if (pedalPara != 0) {
                                    dat3 = pedalPara.getDat();
                                } else {
                                    dat3 = str;
                                }
                                if ((j & 4406636445952L) == 0) {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                } else {
                                    if (dat3 != 0) {
                                        addrU14 = (AddrU8) getFromArray(dat3, 2);
                                    } else {
                                        addrU14 = str;
                                    }
                                    updateRegistration(8, addrU14);
                                    if (addrU14 != 0) {
                                        data20 = addrU14.getData();
                                    } else {
                                        data20 = 0;
                                    }
                                    strValueOf9 = String.valueOf(data20);
                                }
                                if ((j & 9007207845199872L) == 0) {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                } else {
                                    if (dat3 != 0) {
                                        addrU15 = (AddrU8) getFromArray(dat3, 1);
                                    } else {
                                        addrU15 = str;
                                    }
                                    updateRegistration(19, addrU15);
                                    if (addrU15 != 0) {
                                        data21 = addrU15.getData();
                                    } else {
                                        data21 = 0;
                                    }
                                    strValueOf10 = String.valueOf(data21);
                                }
                                if ((j & 2305843017937846272L) == 0) {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                } else {
                                    if (dat3 != 0) {
                                        addrU16 = (AddrU8) getFromArray(dat3, 0);
                                    } else {
                                        addrU16 = str;
                                    }
                                    updateRegistration(27, addrU16);
                                    if (addrU16 != 0) {
                                        data22 = addrU16.getData();
                                    } else {
                                        data22 = 0;
                                    }
                                    strValueOf11 = String.valueOf(data22);
                                }
                                str15 = strValueOf9;
                            }
                            if ((j & (-9223372027727970304L)) == 0) {
                                if (pedalPara != 0) {
                                    ch4 = pedalPara.getCh();
                                } else {
                                    ch4 = str;
                                }
                                str16 = strValueOf10;
                                updateRegistration(29, ch4);
                                if (ch4 != 0) {
                                    data23 = ch4.getData();
                                    str17 = str15;
                                    str18 = str16;
                                } else {
                                    str17 = str15;
                                    str18 = str16;
                                    data23 = 0;
                                }
                            } else {
                                if (pedalPara != 0) {
                                    ch4 = pedalPara.getCh();
                                } else {
                                    ch4 = str;
                                }
                                str16 = strValueOf10;
                                updateRegistration(29, ch4);
                                if (ch4 != 0) {
                                    data23 = ch4.getData();
                                    str17 = str15;
                                    str18 = str16;
                                } else {
                                    str17 = str15;
                                    str18 = str16;
                                    data23 = 0;
                                }
                            }
                        }
                        if ((j & 90212791244300291L) == 0) {
                            if (smk25MiniUsr != null) {
                                modPara = smk25MiniUsr.getModPara();
                            } else {
                                modPara = str;
                            }
                            if ((j & 42949672962L) == 0) {
                                if (modPara != 0) {
                                    speed = modPara.getSpeed();
                                } else {
                                    speed = str;
                                }
                                str19 = str18;
                                updateRegistration(1, speed);
                                if (speed != 0) {
                                }
                            } else {
                                if (modPara != 0) {
                                    speed = modPara.getSpeed();
                                } else {
                                    speed = str;
                                }
                                str19 = str18;
                                updateRegistration(1, speed);
                                if (speed != 0) {
                                }
                            }
                            if ((j & 18155162842439681L) == 0) {
                                if (modPara != 0) {
                                    dat4 = modPara.getDat();
                                } else {
                                    dat4 = str;
                                }
                                if ((j & 25769803777L) == 0) {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                } else {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                }
                                if ((j & 140746078298112L) == 0) {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                } else {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                }
                                if ((j & 18014408174206976L) == 0) {
                                }
                                if (dat4 != 0) {
                                    addrU19 = (AddrU8) getFromArray(dat4, 0);
                                } else {
                                    addrU19 = str;
                                }
                                updateRegistration(20, addrU19);
                                if (addrU19 != 0) {
                                    data27 = addrU19.getData();
                                } else {
                                    data27 = 0;
                                }
                                if ((j & 18014407100465152L) == 0) {
                                    strValueOf14 = String.valueOf(data27);
                                    int i1115 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i1115;
                                } else {
                                    strValueOf14 = String.valueOf(data27);
                                    int i1116 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i1116;
                                }
                            } else {
                                if (modPara != 0) {
                                    dat4 = modPara.getDat();
                                } else {
                                    dat4 = str;
                                }
                                if ((j & 25769803777L) == 0) {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                } else {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                }
                                if ((j & 140746078298112L) == 0) {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                } else {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                }
                                if ((j & 18014408174206976L) == 0) {
                                }
                                if (dat4 != 0) {
                                    addrU19 = (AddrU8) getFromArray(dat4, 0);
                                } else {
                                    addrU19 = str;
                                }
                                updateRegistration(20, addrU19);
                                if (addrU19 != 0) {
                                    data27 = addrU19.getData();
                                } else {
                                    data27 = 0;
                                }
                                if ((j & 18014407100465152L) == 0) {
                                    strValueOf14 = String.valueOf(data27);
                                    int i1117 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i1117;
                                } else {
                                    strValueOf14 = String.valueOf(data27);
                                    int i1118 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i1118;
                                }
                            }
                            if ((j & 72057602632056832L) == 0) {
                                if (modPara != 0) {
                                    ch5 = modPara.getCh();
                                } else {
                                    ch5 = str;
                                }
                                str22 = str21;
                                updateRegistration(22, ch5);
                                if (ch5 != 0) {
                                }
                            } else {
                                if (modPara != 0) {
                                    ch5 = modPara.getCh();
                                } else {
                                    ch5 = str;
                                }
                                str22 = str21;
                                updateRegistration(22, ch5);
                                if (ch5 != 0) {
                                }
                            }
                            i9 = data19;
                            str23 = str22;
                        } else {
                            if (smk25MiniUsr != null) {
                                modPara = smk25MiniUsr.getModPara();
                            } else {
                                modPara = str;
                            }
                            if ((j & 42949672962L) == 0) {
                                if (modPara != 0) {
                                    speed = modPara.getSpeed();
                                } else {
                                    speed = str;
                                }
                                str19 = str18;
                                updateRegistration(1, speed);
                                if (speed != 0) {
                                }
                            } else {
                                if (modPara != 0) {
                                    speed = modPara.getSpeed();
                                } else {
                                    speed = str;
                                }
                                str19 = str18;
                                updateRegistration(1, speed);
                                if (speed != 0) {
                                }
                            }
                            if ((j & 18155162842439681L) == 0) {
                                if (modPara != 0) {
                                    dat4 = modPara.getDat();
                                } else {
                                    dat4 = str;
                                }
                                if ((j & 25769803777L) == 0) {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                } else {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                }
                                if ((j & 140746078298112L) == 0) {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                } else {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                }
                                if ((j & 18014408174206976L) == 0) {
                                }
                                if (dat4 != 0) {
                                    addrU19 = (AddrU8) getFromArray(dat4, 0);
                                } else {
                                    addrU19 = str;
                                }
                                updateRegistration(20, addrU19);
                                if (addrU19 != 0) {
                                    data27 = addrU19.getData();
                                } else {
                                    data27 = 0;
                                }
                                if ((j & 18014407100465152L) == 0) {
                                    strValueOf14 = String.valueOf(data27);
                                    int i1119 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i1119;
                                } else {
                                    strValueOf14 = String.valueOf(data27);
                                    int i11110 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i11110;
                                }
                            } else {
                                if (modPara != 0) {
                                    dat4 = modPara.getDat();
                                } else {
                                    dat4 = str;
                                }
                                if ((j & 25769803777L) == 0) {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                } else {
                                    if (dat4 != 0) {
                                        addrU17 = (AddrU8) getFromArray(dat4, 2);
                                    } else {
                                        addrU17 = str;
                                    }
                                    updateRegistration(0, addrU17);
                                    if (addrU17 != 0) {
                                        data25 = addrU17.getData();
                                    } else {
                                        data25 = 0;
                                    }
                                    strValueOf12 = String.valueOf(data25);
                                }
                                if ((j & 140746078298112L) == 0) {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                } else {
                                    if (dat4 != 0) {
                                        addrU18 = (AddrU8) getFromArray(dat4, 1);
                                    } else {
                                        addrU18 = str;
                                    }
                                    updateRegistration(13, addrU18);
                                    if (addrU18 != 0) {
                                        data26 = addrU18.getData();
                                    } else {
                                        data26 = 0;
                                    }
                                    strValueOf13 = String.valueOf(data26);
                                }
                                if ((j & 18014408174206976L) == 0) {
                                }
                                if (dat4 != 0) {
                                    addrU19 = (AddrU8) getFromArray(dat4, 0);
                                } else {
                                    addrU19 = str;
                                }
                                updateRegistration(20, addrU19);
                                if (addrU19 != 0) {
                                    data27 = addrU19.getData();
                                } else {
                                    data27 = 0;
                                }
                                if ((j & 18014407100465152L) == 0) {
                                    strValueOf14 = String.valueOf(data27);
                                    int i11111 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i11111;
                                } else {
                                    strValueOf14 = String.valueOf(data27);
                                    int i11112 = data27;
                                    str20 = strValueOf13;
                                    str21 = strValueOf12;
                                    i8 = i11112;
                                }
                            }
                            if ((j & 72057602632056832L) == 0) {
                                if (modPara != 0) {
                                    ch5 = modPara.getCh();
                                } else {
                                    ch5 = str;
                                }
                                str22 = str21;
                                updateRegistration(22, ch5);
                                if (ch5 != 0) {
                                }
                            } else {
                                if (modPara != 0) {
                                    ch5 = modPara.getCh();
                                } else {
                                    ch5 = str;
                                }
                                str22 = str21;
                                updateRegistration(22, ch5);
                                if (ch5 != 0) {
                                }
                            }
                            i9 = data19;
                            str23 = str22;
                        }
                    } else {
                        data5 = data5;
                        strValueOf2 = strValueOf2;
                        sysEx = str;
                        str6 = sysEx;
                        str7 = str6;
                        str5 = str7;
                        i4 = 0;
                        i5 = 0;
                    }
                    data12 = 0;
                    if ((j & 2817920526876856L) == 0) {
                        if (smk25MiniUsr != null) {
                            encoderPara = smk25MiniUsr.getEncoderPara();
                        } else {
                            encoderPara = str;
                        }
                        if ((j & 283467841552L) == 0) {
                            if (encoderPara != 0) {
                                ch3 = encoderPara.getCh();
                            } else {
                                ch3 = str;
                            }
                            str8 = str6;
                            updateRegistration(4, ch3);
                            if (ch3 != 0) {
                            }
                        } else {
                            if (encoderPara != 0) {
                                ch3 = encoderPara.getCh();
                            } else {
                                ch3 = str;
                            }
                            str8 = str6;
                            updateRegistration(4, ch3);
                            if (ch3 != 0) {
                            }
                        }
                        if ((j & 2207613190272L) == 0) {
                            if (encoderPara != 0) {
                                curve = encoderPara.getCurve();
                            } else {
                                curve = str;
                            }
                            i6 = data13;
                            updateRegistration(7, curve);
                            if (curve != 0) {
                            }
                        } else {
                            if (encoderPara != 0) {
                                curve = encoderPara.getCurve();
                            } else {
                                curve = str;
                            }
                            i6 = data13;
                            updateRegistration(7, curve);
                            if (curve != 0) {
                            }
                        }
                        if ((j & 563646811897896L) == 0) {
                            if (encoderPara != 0) {
                                dat2 = encoderPara.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j & 147102629896L) == 0) {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            } else {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            }
                            if ((j & 558345748512L) == 0) {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            } else {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            }
                            if ((j & 562958543388672L) == 0) {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            } else {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            }
                            str10 = strValueOf6;
                        } else {
                            if (encoderPara != 0) {
                                dat2 = encoderPara.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j & 147102629896L) == 0) {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            } else {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            }
                            if ((j & 558345748512L) == 0) {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            } else {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            }
                            if ((j & 562958543388672L) == 0) {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            } else {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            }
                            str10 = strValueOf6;
                        }
                        if ((j & 2251808403750912L) == 0) {
                            if (encoderPara != 0) {
                                type2 = encoderPara.getType();
                            } else {
                                type2 = str;
                            }
                            str11 = strValueOf8;
                            updateRegistration(17, type2);
                            if (type2 != 0) {
                                data18 = type2.getData();
                                str12 = strValueOf7;
                                str13 = str11;
                            } else {
                                str12 = strValueOf7;
                                str13 = str11;
                                data18 = 0;
                            }
                        } else {
                            if (encoderPara != 0) {
                                type2 = encoderPara.getType();
                            } else {
                                type2 = str;
                            }
                            str11 = strValueOf8;
                            updateRegistration(17, type2);
                            if (type2 != 0) {
                                data18 = type2.getData();
                                str12 = strValueOf7;
                                str13 = str11;
                            } else {
                                str12 = strValueOf7;
                                str13 = str11;
                                data18 = 0;
                            }
                        }
                    } else {
                        if (smk25MiniUsr != null) {
                            encoderPara = smk25MiniUsr.getEncoderPara();
                        } else {
                            encoderPara = str;
                        }
                        if ((j & 283467841552L) == 0) {
                            if (encoderPara != 0) {
                                ch3 = encoderPara.getCh();
                            } else {
                                ch3 = str;
                            }
                            str8 = str6;
                            updateRegistration(4, ch3);
                            if (ch3 != 0) {
                            }
                        } else {
                            if (encoderPara != 0) {
                                ch3 = encoderPara.getCh();
                            } else {
                                ch3 = str;
                            }
                            str8 = str6;
                            updateRegistration(4, ch3);
                            if (ch3 != 0) {
                            }
                        }
                        if ((j & 2207613190272L) == 0) {
                            if (encoderPara != 0) {
                                curve = encoderPara.getCurve();
                            } else {
                                curve = str;
                            }
                            i6 = data13;
                            updateRegistration(7, curve);
                            if (curve != 0) {
                            }
                        } else {
                            if (encoderPara != 0) {
                                curve = encoderPara.getCurve();
                            } else {
                                curve = str;
                            }
                            i6 = data13;
                            updateRegistration(7, curve);
                            if (curve != 0) {
                            }
                        }
                        if ((j & 563646811897896L) == 0) {
                            if (encoderPara != 0) {
                                dat2 = encoderPara.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j & 147102629896L) == 0) {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            } else {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            }
                            if ((j & 558345748512L) == 0) {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            } else {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            }
                            if ((j & 562958543388672L) == 0) {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            } else {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            }
                            str10 = strValueOf6;
                        } else {
                            if (encoderPara != 0) {
                                dat2 = encoderPara.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j & 147102629896L) == 0) {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            } else {
                                i7 = data14;
                                if (dat2 != 0) {
                                    addrU11 = (AddrU8) getFromArray(dat2, 0);
                                } else {
                                    addrU11 = str;
                                }
                                str9 = str7;
                                updateRegistration(3, addrU11);
                                if (addrU11 != 0) {
                                    data15 = addrU11.getData();
                                } else {
                                    data15 = 0;
                                }
                                if ((j & 146028888072L) == 0) {
                                    strValueOf6 = String.valueOf(data15);
                                } else {
                                    strValueOf6 = String.valueOf(data15);
                                }
                            }
                            if ((j & 558345748512L) == 0) {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            } else {
                                if (dat2 != 0) {
                                    addrU12 = (AddrU8) getFromArray(dat2, 1);
                                } else {
                                    addrU12 = str;
                                }
                                updateRegistration(5, addrU12);
                                if (addrU12 != 0) {
                                    data16 = addrU12.getData();
                                } else {
                                    data16 = 0;
                                }
                                strValueOf7 = String.valueOf(data16);
                            }
                            if ((j & 562958543388672L) == 0) {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            } else {
                                if (dat2 != 0) {
                                    addrU13 = (AddrU8) getFromArray(dat2, 2);
                                } else {
                                    addrU13 = str;
                                }
                                updateRegistration(15, addrU13);
                                if (addrU13 != 0) {
                                    data17 = addrU13.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf8 = String.valueOf(data17);
                            }
                            str10 = strValueOf6;
                        }
                        if ((j & 2251808403750912L) == 0) {
                            if (encoderPara != 0) {
                                type2 = encoderPara.getType();
                            } else {
                                type2 = str;
                            }
                            str11 = strValueOf8;
                            updateRegistration(17, type2);
                            if (type2 != 0) {
                                data18 = type2.getData();
                                str12 = strValueOf7;
                                str13 = str11;
                            } else {
                                str12 = strValueOf7;
                                str13 = str11;
                                data18 = 0;
                            }
                        } else {
                            if (encoderPara != 0) {
                                type2 = encoderPara.getType();
                            } else {
                                type2 = str;
                            }
                            str11 = strValueOf8;
                            updateRegistration(17, type2);
                            if (type2 != 0) {
                                data18 = type2.getData();
                                str12 = strValueOf7;
                                str13 = str11;
                            } else {
                                str12 = strValueOf7;
                                str13 = str11;
                                data18 = 0;
                            }
                        }
                    }
                    if ((j & (-6620287044909793024L)) == 0) {
                        if (smk25MiniUsr != null) {
                            pedalPara = smk25MiniUsr.getPedalPara();
                        } else {
                            pedalPara = str;
                        }
                        if ((j & 288230384758423552L) == 0) {
                            if (pedalPara != 0) {
                                type3 = pedalPara.getType();
                            } else {
                                type3 = str;
                            }
                            str14 = str13;
                            updateRegistration(24, type3);
                            if (type3 != 0) {
                            }
                        } else {
                            if (pedalPara != 0) {
                                type3 = pedalPara.getType();
                            } else {
                                type3 = str;
                            }
                            str14 = str13;
                            updateRegistration(24, type3);
                            if (type3 != 0) {
                            }
                        }
                        if ((j & 2314854615239622912L) == 0) {
                            if (pedalPara != 0) {
                                dat3 = pedalPara.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j & 4406636445952L) == 0) {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            } else {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            }
                            if ((j & 9007207845199872L) == 0) {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            } else {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            }
                            if ((j & 2305843017937846272L) == 0) {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            } else {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            }
                            str15 = strValueOf9;
                        } else {
                            if (pedalPara != 0) {
                                dat3 = pedalPara.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j & 4406636445952L) == 0) {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            } else {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            }
                            if ((j & 9007207845199872L) == 0) {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            } else {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            }
                            if ((j & 2305843017937846272L) == 0) {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            } else {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            }
                            str15 = strValueOf9;
                        }
                        if ((j & (-9223372027727970304L)) == 0) {
                            if (pedalPara != 0) {
                                ch4 = pedalPara.getCh();
                            } else {
                                ch4 = str;
                            }
                            str16 = strValueOf10;
                            updateRegistration(29, ch4);
                            if (ch4 != 0) {
                                data23 = ch4.getData();
                                str17 = str15;
                                str18 = str16;
                            } else {
                                str17 = str15;
                                str18 = str16;
                                data23 = 0;
                            }
                        } else {
                            if (pedalPara != 0) {
                                ch4 = pedalPara.getCh();
                            } else {
                                ch4 = str;
                            }
                            str16 = strValueOf10;
                            updateRegistration(29, ch4);
                            if (ch4 != 0) {
                                data23 = ch4.getData();
                                str17 = str15;
                                str18 = str16;
                            } else {
                                str17 = str15;
                                str18 = str16;
                                data23 = 0;
                            }
                        }
                    } else {
                        if (smk25MiniUsr != null) {
                            pedalPara = smk25MiniUsr.getPedalPara();
                        } else {
                            pedalPara = str;
                        }
                        if ((j & 288230384758423552L) == 0) {
                            if (pedalPara != 0) {
                                type3 = pedalPara.getType();
                            } else {
                                type3 = str;
                            }
                            str14 = str13;
                            updateRegistration(24, type3);
                            if (type3 != 0) {
                            }
                        } else {
                            if (pedalPara != 0) {
                                type3 = pedalPara.getType();
                            } else {
                                type3 = str;
                            }
                            str14 = str13;
                            updateRegistration(24, type3);
                            if (type3 != 0) {
                            }
                        }
                        if ((j & 2314854615239622912L) == 0) {
                            if (pedalPara != 0) {
                                dat3 = pedalPara.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j & 4406636445952L) == 0) {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            } else {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            }
                            if ((j & 9007207845199872L) == 0) {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            } else {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            }
                            if ((j & 2305843017937846272L) == 0) {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            } else {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            }
                            str15 = strValueOf9;
                        } else {
                            if (pedalPara != 0) {
                                dat3 = pedalPara.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j & 4406636445952L) == 0) {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            } else {
                                if (dat3 != 0) {
                                    addrU14 = (AddrU8) getFromArray(dat3, 2);
                                } else {
                                    addrU14 = str;
                                }
                                updateRegistration(8, addrU14);
                                if (addrU14 != 0) {
                                    data20 = addrU14.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf9 = String.valueOf(data20);
                            }
                            if ((j & 9007207845199872L) == 0) {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            } else {
                                if (dat3 != 0) {
                                    addrU15 = (AddrU8) getFromArray(dat3, 1);
                                } else {
                                    addrU15 = str;
                                }
                                updateRegistration(19, addrU15);
                                if (addrU15 != 0) {
                                    data21 = addrU15.getData();
                                } else {
                                    data21 = 0;
                                }
                                strValueOf10 = String.valueOf(data21);
                            }
                            if ((j & 2305843017937846272L) == 0) {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            } else {
                                if (dat3 != 0) {
                                    addrU16 = (AddrU8) getFromArray(dat3, 0);
                                } else {
                                    addrU16 = str;
                                }
                                updateRegistration(27, addrU16);
                                if (addrU16 != 0) {
                                    data22 = addrU16.getData();
                                } else {
                                    data22 = 0;
                                }
                                strValueOf11 = String.valueOf(data22);
                            }
                            str15 = strValueOf9;
                        }
                        if ((j & (-9223372027727970304L)) == 0) {
                            if (pedalPara != 0) {
                                ch4 = pedalPara.getCh();
                            } else {
                                ch4 = str;
                            }
                            str16 = strValueOf10;
                            updateRegistration(29, ch4);
                            if (ch4 != 0) {
                                data23 = ch4.getData();
                                str17 = str15;
                                str18 = str16;
                            } else {
                                str17 = str15;
                                str18 = str16;
                                data23 = 0;
                            }
                        } else {
                            if (pedalPara != 0) {
                                ch4 = pedalPara.getCh();
                            } else {
                                ch4 = str;
                            }
                            str16 = strValueOf10;
                            updateRegistration(29, ch4);
                            if (ch4 != 0) {
                                data23 = ch4.getData();
                                str17 = str15;
                                str18 = str16;
                            } else {
                                str17 = str15;
                                str18 = str16;
                                data23 = 0;
                            }
                        }
                    }
                    if ((j & 90212791244300291L) == 0) {
                        if (smk25MiniUsr != null) {
                            modPara = smk25MiniUsr.getModPara();
                        } else {
                            modPara = str;
                        }
                        if ((j & 42949672962L) == 0) {
                            if (modPara != 0) {
                                speed = modPara.getSpeed();
                            } else {
                                speed = str;
                            }
                            str19 = str18;
                            updateRegistration(1, speed);
                            if (speed != 0) {
                            }
                        } else {
                            if (modPara != 0) {
                                speed = modPara.getSpeed();
                            } else {
                                speed = str;
                            }
                            str19 = str18;
                            updateRegistration(1, speed);
                            if (speed != 0) {
                            }
                        }
                        if ((j & 18155162842439681L) == 0) {
                            if (modPara != 0) {
                                dat4 = modPara.getDat();
                            } else {
                                dat4 = str;
                            }
                            if ((j & 25769803777L) == 0) {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            } else {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            }
                            if ((j & 140746078298112L) == 0) {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            } else {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            }
                            if ((j & 18014408174206976L) == 0) {
                            }
                            if (dat4 != 0) {
                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                            } else {
                                addrU19 = str;
                            }
                            updateRegistration(20, addrU19);
                            if (addrU19 != 0) {
                                data27 = addrU19.getData();
                            } else {
                                data27 = 0;
                            }
                            if ((j & 18014407100465152L) == 0) {
                                strValueOf14 = String.valueOf(data27);
                                int i11113 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11113;
                            } else {
                                strValueOf14 = String.valueOf(data27);
                                int i11114 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11114;
                            }
                        } else {
                            if (modPara != 0) {
                                dat4 = modPara.getDat();
                            } else {
                                dat4 = str;
                            }
                            if ((j & 25769803777L) == 0) {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            } else {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            }
                            if ((j & 140746078298112L) == 0) {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            } else {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            }
                            if ((j & 18014408174206976L) == 0) {
                            }
                            if (dat4 != 0) {
                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                            } else {
                                addrU19 = str;
                            }
                            updateRegistration(20, addrU19);
                            if (addrU19 != 0) {
                                data27 = addrU19.getData();
                            } else {
                                data27 = 0;
                            }
                            if ((j & 18014407100465152L) == 0) {
                                strValueOf14 = String.valueOf(data27);
                                int i11115 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11115;
                            } else {
                                strValueOf14 = String.valueOf(data27);
                                int i11116 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11116;
                            }
                        }
                        if ((j & 72057602632056832L) == 0) {
                            if (modPara != 0) {
                                ch5 = modPara.getCh();
                            } else {
                                ch5 = str;
                            }
                            str22 = str21;
                            updateRegistration(22, ch5);
                            if (ch5 != 0) {
                            }
                        } else {
                            if (modPara != 0) {
                                ch5 = modPara.getCh();
                            } else {
                                ch5 = str;
                            }
                            str22 = str21;
                            updateRegistration(22, ch5);
                            if (ch5 != 0) {
                            }
                        }
                        i9 = data19;
                        str23 = str22;
                    } else {
                        if (smk25MiniUsr != null) {
                            modPara = smk25MiniUsr.getModPara();
                        } else {
                            modPara = str;
                        }
                        if ((j & 42949672962L) == 0) {
                            if (modPara != 0) {
                                speed = modPara.getSpeed();
                            } else {
                                speed = str;
                            }
                            str19 = str18;
                            updateRegistration(1, speed);
                            if (speed != 0) {
                            }
                        } else {
                            if (modPara != 0) {
                                speed = modPara.getSpeed();
                            } else {
                                speed = str;
                            }
                            str19 = str18;
                            updateRegistration(1, speed);
                            if (speed != 0) {
                            }
                        }
                        if ((j & 18155162842439681L) == 0) {
                            if (modPara != 0) {
                                dat4 = modPara.getDat();
                            } else {
                                dat4 = str;
                            }
                            if ((j & 25769803777L) == 0) {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            } else {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            }
                            if ((j & 140746078298112L) == 0) {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            } else {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            }
                            if ((j & 18014408174206976L) == 0) {
                            }
                            if (dat4 != 0) {
                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                            } else {
                                addrU19 = str;
                            }
                            updateRegistration(20, addrU19);
                            if (addrU19 != 0) {
                                data27 = addrU19.getData();
                            } else {
                                data27 = 0;
                            }
                            if ((j & 18014407100465152L) == 0) {
                                strValueOf14 = String.valueOf(data27);
                                int i11117 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11117;
                            } else {
                                strValueOf14 = String.valueOf(data27);
                                int i11118 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11118;
                            }
                        } else {
                            if (modPara != 0) {
                                dat4 = modPara.getDat();
                            } else {
                                dat4 = str;
                            }
                            if ((j & 25769803777L) == 0) {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            } else {
                                if (dat4 != 0) {
                                    addrU17 = (AddrU8) getFromArray(dat4, 2);
                                } else {
                                    addrU17 = str;
                                }
                                updateRegistration(0, addrU17);
                                if (addrU17 != 0) {
                                    data25 = addrU17.getData();
                                } else {
                                    data25 = 0;
                                }
                                strValueOf12 = String.valueOf(data25);
                            }
                            if ((j & 140746078298112L) == 0) {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            } else {
                                if (dat4 != 0) {
                                    addrU18 = (AddrU8) getFromArray(dat4, 1);
                                } else {
                                    addrU18 = str;
                                }
                                updateRegistration(13, addrU18);
                                if (addrU18 != 0) {
                                    data26 = addrU18.getData();
                                } else {
                                    data26 = 0;
                                }
                                strValueOf13 = String.valueOf(data26);
                            }
                            if ((j & 18014408174206976L) == 0) {
                            }
                            if (dat4 != 0) {
                                addrU19 = (AddrU8) getFromArray(dat4, 0);
                            } else {
                                addrU19 = str;
                            }
                            updateRegistration(20, addrU19);
                            if (addrU19 != 0) {
                                data27 = addrU19.getData();
                            } else {
                                data27 = 0;
                            }
                            if ((j & 18014407100465152L) == 0) {
                                strValueOf14 = String.valueOf(data27);
                                int i11119 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i11119;
                            } else {
                                strValueOf14 = String.valueOf(data27);
                                int i111110 = data27;
                                str20 = strValueOf13;
                                str21 = strValueOf12;
                                i8 = i111110;
                            }
                        }
                        if ((j & 72057602632056832L) == 0) {
                            if (modPara != 0) {
                                ch5 = modPara.getCh();
                            } else {
                                ch5 = str;
                            }
                            str22 = str21;
                            updateRegistration(22, ch5);
                            if (ch5 != 0) {
                            }
                        } else {
                            if (modPara != 0) {
                                ch5 = modPara.getCh();
                            } else {
                                ch5 = str;
                            }
                            str22 = str21;
                            updateRegistration(22, ch5);
                            if (ch5 != 0) {
                            }
                        }
                        i9 = data19;
                        str23 = str22;
                    }
                }
                if ((j & 4611686027285757952L) == 0) {
                    if (pitchPara != 0) {
                        ch = pitchPara.getCh();
                    } else {
                        ch = str;
                    }
                    updateRegistration(28, ch);
                    if (ch != 0) {
                    }
                } else {
                    if (pitchPara != 0) {
                        ch = pitchPara.getCh();
                    } else {
                        ch = str;
                    }
                    updateRegistration(28, ch);
                    if (ch != 0) {
                    }
                }
                i2 = data6;
                if ((j & 1190358787901440064L) == 0) {
                    if (smk25MiniUsr != null) {
                        btnPara = smk25MiniUsr.getBtnPara();
                    } else {
                        btnPara = str;
                    }
                    if ((j & 1154048513756168256L) == 0) {
                        if (btnPara != 0) {
                            dat = btnPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j & 1108101562432L) == 0) {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        } else {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        }
                        if ((j & 1125908496842752L) == 0) {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        } else {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        }
                        if ((j & 1152921514337632256L) == 0) {
                        }
                        if (dat != 0) {
                            addrU10 = (AddrU8) getFromArray(dat, 0);
                        } else {
                            addrU10 = str;
                        }
                        updateRegistration(26, addrU10);
                        if (addrU10 != 0) {
                            data10 = addrU10.getData();
                        } else {
                            data10 = 0;
                        }
                        if ((j & 1152921513263890432L) == 0) {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        } else {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        }
                    } else {
                        if (btnPara != 0) {
                            dat = btnPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j & 1108101562432L) == 0) {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        } else {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        }
                        if ((j & 1125908496842752L) == 0) {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        } else {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        }
                        if ((j & 1152921514337632256L) == 0) {
                        }
                        if (dat != 0) {
                            addrU10 = (AddrU8) getFromArray(dat, 0);
                        } else {
                            addrU10 = str;
                        }
                        updateRegistration(26, addrU10);
                        if (addrU10 != 0) {
                            data10 = addrU10.getData();
                        } else {
                            data10 = 0;
                        }
                        if ((j & 1152921513263890432L) == 0) {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        } else {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        }
                    }
                    if ((j & 281483566661632L) == 0) {
                        if (btnPara != 0) {
                            ch2 = btnPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str4 = str3;
                        updateRegistration(14, ch2);
                        if (ch2 != 0) {
                        }
                    } else {
                        if (btnPara != 0) {
                            ch2 = btnPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str4 = str3;
                        updateRegistration(14, ch2);
                        if (ch2 != 0) {
                        }
                    }
                    if (i10 == 0) {
                        if (btnPara != 0) {
                            sys = btnPara.getSys();
                        } else {
                            sys = str;
                        }
                        if (smk25MiniUsrFragment != null) {
                            sysEx = smk25MiniUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    } else {
                        if (btnPara != 0) {
                            sys = btnPara.getSys();
                        } else {
                            sys = str;
                        }
                        if (smk25MiniUsrFragment != null) {
                            sysEx = smk25MiniUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    }
                    if ((j & 36028805610995712L) == 0) {
                        if (btnPara != 0) {
                            type = btnPara.getType();
                        } else {
                            type = str;
                        }
                        updateRegistration(21, type);
                        if (type != 0) {
                            int i11120 = i3;
                            str5 = strValueOf5;
                            i4 = i11120;
                            i5 = data11;
                            str6 = str2;
                            data12 = type.getData();
                            str7 = str4;
                        } else {
                            int i11121 = i3;
                            str5 = strValueOf5;
                            i4 = i11121;
                            i5 = data11;
                            str6 = str2;
                            str7 = str4;
                            data12 = 0;
                        }
                    } else {
                        if (btnPara != 0) {
                            type = btnPara.getType();
                        } else {
                            type = str;
                        }
                        updateRegistration(21, type);
                        if (type != 0) {
                            int i11122 = i3;
                            str5 = strValueOf5;
                            i4 = i11122;
                            i5 = data11;
                            str6 = str2;
                            data12 = type.getData();
                            str7 = str4;
                        } else {
                            int i11123 = i3;
                            str5 = strValueOf5;
                            i4 = i11123;
                            i5 = data11;
                            str6 = str2;
                            str7 = str4;
                            data12 = 0;
                        }
                    }
                } else {
                    if (smk25MiniUsr != null) {
                        btnPara = smk25MiniUsr.getBtnPara();
                    } else {
                        btnPara = str;
                    }
                    if ((j & 1154048513756168256L) == 0) {
                        if (btnPara != 0) {
                            dat = btnPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j & 1108101562432L) == 0) {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        } else {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        }
                        if ((j & 1125908496842752L) == 0) {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        } else {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        }
                        if ((j & 1152921514337632256L) == 0) {
                        }
                        if (dat != 0) {
                            addrU10 = (AddrU8) getFromArray(dat, 0);
                        } else {
                            addrU10 = str;
                        }
                        updateRegistration(26, addrU10);
                        if (addrU10 != 0) {
                            data10 = addrU10.getData();
                        } else {
                            data10 = 0;
                        }
                        if ((j & 1152921513263890432L) == 0) {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        } else {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        }
                    } else {
                        if (btnPara != 0) {
                            dat = btnPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j & 1108101562432L) == 0) {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        } else {
                            if (dat != 0) {
                                addrU8 = (AddrU8) getFromArray(dat, 1);
                            } else {
                                addrU8 = str;
                            }
                            updateRegistration(6, addrU8);
                            if (addrU8 != 0) {
                                data8 = addrU8.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf3 = String.valueOf(data8);
                        }
                        if ((j & 1125908496842752L) == 0) {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        } else {
                            if (dat != 0) {
                                addrU9 = (AddrU8) getFromArray(dat, 2);
                            } else {
                                addrU9 = str;
                            }
                            updateRegistration(16, addrU9);
                            if (addrU9 != 0) {
                                data9 = addrU9.getData();
                            } else {
                                data9 = 0;
                            }
                            strValueOf4 = String.valueOf(data9);
                        }
                        if ((j & 1152921514337632256L) == 0) {
                        }
                        if (dat != 0) {
                            addrU10 = (AddrU8) getFromArray(dat, 0);
                        } else {
                            addrU10 = str;
                        }
                        updateRegistration(26, addrU10);
                        if (addrU10 != 0) {
                            data10 = addrU10.getData();
                        } else {
                            data10 = 0;
                        }
                        if ((j & 1152921513263890432L) == 0) {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        } else {
                            strValueOf5 = String.valueOf(data10);
                            i3 = data10;
                            str2 = strValueOf4;
                            str3 = strValueOf3;
                        }
                    }
                    if ((j & 281483566661632L) == 0) {
                        if (btnPara != 0) {
                            ch2 = btnPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str4 = str3;
                        updateRegistration(14, ch2);
                        if (ch2 != 0) {
                        }
                    } else {
                        if (btnPara != 0) {
                            ch2 = btnPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str4 = str3;
                        updateRegistration(14, ch2);
                        if (ch2 != 0) {
                        }
                    }
                    if (i10 == 0) {
                        if (btnPara != 0) {
                            sys = btnPara.getSys();
                        } else {
                            sys = str;
                        }
                        if (smk25MiniUsrFragment != null) {
                            sysEx = smk25MiniUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    } else {
                        if (btnPara != 0) {
                            sys = btnPara.getSys();
                        } else {
                            sys = str;
                        }
                        if (smk25MiniUsrFragment != null) {
                            sysEx = smk25MiniUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    }
                    if ((j & 36028805610995712L) == 0) {
                        if (btnPara != 0) {
                            type = btnPara.getType();
                        } else {
                            type = str;
                        }
                        updateRegistration(21, type);
                        if (type != 0) {
                            int i11124 = i3;
                            str5 = strValueOf5;
                            i4 = i11124;
                            i5 = data11;
                            str6 = str2;
                            data12 = type.getData();
                            str7 = str4;
                        } else {
                            int i11125 = i3;
                            str5 = strValueOf5;
                            i4 = i11125;
                            i5 = data11;
                            str6 = str2;
                            str7 = str4;
                            data12 = 0;
                        }
                    } else {
                        if (btnPara != 0) {
                            type = btnPara.getType();
                        } else {
                            type = str;
                        }
                        updateRegistration(21, type);
                        if (type != 0) {
                            int i11126 = i3;
                            str5 = strValueOf5;
                            i4 = i11126;
                            i5 = data11;
                            str6 = str2;
                            data12 = type.getData();
                            str7 = str4;
                        } else {
                            int i11127 = i3;
                            str5 = strValueOf5;
                            i4 = i11127;
                            i5 = data11;
                            str6 = str2;
                            str7 = str4;
                            data12 = 0;
                        }
                    }
                }
                if ((j & 2817920526876856L) == 0) {
                    if (smk25MiniUsr != null) {
                        encoderPara = smk25MiniUsr.getEncoderPara();
                    } else {
                        encoderPara = str;
                    }
                    if ((j & 283467841552L) == 0) {
                        if (encoderPara != 0) {
                            ch3 = encoderPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(4, ch3);
                        if (ch3 != 0) {
                        }
                    } else {
                        if (encoderPara != 0) {
                            ch3 = encoderPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(4, ch3);
                        if (ch3 != 0) {
                        }
                    }
                    if ((j & 2207613190272L) == 0) {
                        if (encoderPara != 0) {
                            curve = encoderPara.getCurve();
                        } else {
                            curve = str;
                        }
                        i6 = data13;
                        updateRegistration(7, curve);
                        if (curve != 0) {
                        }
                    } else {
                        if (encoderPara != 0) {
                            curve = encoderPara.getCurve();
                        } else {
                            curve = str;
                        }
                        i6 = data13;
                        updateRegistration(7, curve);
                        if (curve != 0) {
                        }
                    }
                    if ((j & 563646811897896L) == 0) {
                        if (encoderPara != 0) {
                            dat2 = encoderPara.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j & 147102629896L) == 0) {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        } else {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        }
                        if ((j & 558345748512L) == 0) {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        } else {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        }
                        if ((j & 562958543388672L) == 0) {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        } else {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        }
                        str10 = strValueOf6;
                    } else {
                        if (encoderPara != 0) {
                            dat2 = encoderPara.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j & 147102629896L) == 0) {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        } else {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        }
                        if ((j & 558345748512L) == 0) {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        } else {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        }
                        if ((j & 562958543388672L) == 0) {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        } else {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        }
                        str10 = strValueOf6;
                    }
                    if ((j & 2251808403750912L) == 0) {
                        if (encoderPara != 0) {
                            type2 = encoderPara.getType();
                        } else {
                            type2 = str;
                        }
                        str11 = strValueOf8;
                        updateRegistration(17, type2);
                        if (type2 != 0) {
                            data18 = type2.getData();
                            str12 = strValueOf7;
                            str13 = str11;
                        } else {
                            str12 = strValueOf7;
                            str13 = str11;
                            data18 = 0;
                        }
                    } else {
                        if (encoderPara != 0) {
                            type2 = encoderPara.getType();
                        } else {
                            type2 = str;
                        }
                        str11 = strValueOf8;
                        updateRegistration(17, type2);
                        if (type2 != 0) {
                            data18 = type2.getData();
                            str12 = strValueOf7;
                            str13 = str11;
                        } else {
                            str12 = strValueOf7;
                            str13 = str11;
                            data18 = 0;
                        }
                    }
                } else {
                    if (smk25MiniUsr != null) {
                        encoderPara = smk25MiniUsr.getEncoderPara();
                    } else {
                        encoderPara = str;
                    }
                    if ((j & 283467841552L) == 0) {
                        if (encoderPara != 0) {
                            ch3 = encoderPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(4, ch3);
                        if (ch3 != 0) {
                        }
                    } else {
                        if (encoderPara != 0) {
                            ch3 = encoderPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(4, ch3);
                        if (ch3 != 0) {
                        }
                    }
                    if ((j & 2207613190272L) == 0) {
                        if (encoderPara != 0) {
                            curve = encoderPara.getCurve();
                        } else {
                            curve = str;
                        }
                        i6 = data13;
                        updateRegistration(7, curve);
                        if (curve != 0) {
                        }
                    } else {
                        if (encoderPara != 0) {
                            curve = encoderPara.getCurve();
                        } else {
                            curve = str;
                        }
                        i6 = data13;
                        updateRegistration(7, curve);
                        if (curve != 0) {
                        }
                    }
                    if ((j & 563646811897896L) == 0) {
                        if (encoderPara != 0) {
                            dat2 = encoderPara.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j & 147102629896L) == 0) {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        } else {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        }
                        if ((j & 558345748512L) == 0) {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        } else {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        }
                        if ((j & 562958543388672L) == 0) {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        } else {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        }
                        str10 = strValueOf6;
                    } else {
                        if (encoderPara != 0) {
                            dat2 = encoderPara.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j & 147102629896L) == 0) {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        } else {
                            i7 = data14;
                            if (dat2 != 0) {
                                addrU11 = (AddrU8) getFromArray(dat2, 0);
                            } else {
                                addrU11 = str;
                            }
                            str9 = str7;
                            updateRegistration(3, addrU11);
                            if (addrU11 != 0) {
                                data15 = addrU11.getData();
                            } else {
                                data15 = 0;
                            }
                            if ((j & 146028888072L) == 0) {
                                strValueOf6 = String.valueOf(data15);
                            } else {
                                strValueOf6 = String.valueOf(data15);
                            }
                        }
                        if ((j & 558345748512L) == 0) {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        } else {
                            if (dat2 != 0) {
                                addrU12 = (AddrU8) getFromArray(dat2, 1);
                            } else {
                                addrU12 = str;
                            }
                            updateRegistration(5, addrU12);
                            if (addrU12 != 0) {
                                data16 = addrU12.getData();
                            } else {
                                data16 = 0;
                            }
                            strValueOf7 = String.valueOf(data16);
                        }
                        if ((j & 562958543388672L) == 0) {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        } else {
                            if (dat2 != 0) {
                                addrU13 = (AddrU8) getFromArray(dat2, 2);
                            } else {
                                addrU13 = str;
                            }
                            updateRegistration(15, addrU13);
                            if (addrU13 != 0) {
                                data17 = addrU13.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf8 = String.valueOf(data17);
                        }
                        str10 = strValueOf6;
                    }
                    if ((j & 2251808403750912L) == 0) {
                        if (encoderPara != 0) {
                            type2 = encoderPara.getType();
                        } else {
                            type2 = str;
                        }
                        str11 = strValueOf8;
                        updateRegistration(17, type2);
                        if (type2 != 0) {
                            data18 = type2.getData();
                            str12 = strValueOf7;
                            str13 = str11;
                        } else {
                            str12 = strValueOf7;
                            str13 = str11;
                            data18 = 0;
                        }
                    } else {
                        if (encoderPara != 0) {
                            type2 = encoderPara.getType();
                        } else {
                            type2 = str;
                        }
                        str11 = strValueOf8;
                        updateRegistration(17, type2);
                        if (type2 != 0) {
                            data18 = type2.getData();
                            str12 = strValueOf7;
                            str13 = str11;
                        } else {
                            str12 = strValueOf7;
                            str13 = str11;
                            data18 = 0;
                        }
                    }
                }
                if ((j & (-6620287044909793024L)) == 0) {
                    if (smk25MiniUsr != null) {
                        pedalPara = smk25MiniUsr.getPedalPara();
                    } else {
                        pedalPara = str;
                    }
                    if ((j & 288230384758423552L) == 0) {
                        if (pedalPara != 0) {
                            type3 = pedalPara.getType();
                        } else {
                            type3 = str;
                        }
                        str14 = str13;
                        updateRegistration(24, type3);
                        if (type3 != 0) {
                        }
                    } else {
                        if (pedalPara != 0) {
                            type3 = pedalPara.getType();
                        } else {
                            type3 = str;
                        }
                        str14 = str13;
                        updateRegistration(24, type3);
                        if (type3 != 0) {
                        }
                    }
                    if ((j & 2314854615239622912L) == 0) {
                        if (pedalPara != 0) {
                            dat3 = pedalPara.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j & 4406636445952L) == 0) {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        }
                        if ((j & 9007207845199872L) == 0) {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        } else {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        }
                        if ((j & 2305843017937846272L) == 0) {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        } else {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        }
                        str15 = strValueOf9;
                    } else {
                        if (pedalPara != 0) {
                            dat3 = pedalPara.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j & 4406636445952L) == 0) {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        }
                        if ((j & 9007207845199872L) == 0) {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        } else {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        }
                        if ((j & 2305843017937846272L) == 0) {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        } else {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        }
                        str15 = strValueOf9;
                    }
                    if ((j & (-9223372027727970304L)) == 0) {
                        if (pedalPara != 0) {
                            ch4 = pedalPara.getCh();
                        } else {
                            ch4 = str;
                        }
                        str16 = strValueOf10;
                        updateRegistration(29, ch4);
                        if (ch4 != 0) {
                            data23 = ch4.getData();
                            str17 = str15;
                            str18 = str16;
                        } else {
                            str17 = str15;
                            str18 = str16;
                            data23 = 0;
                        }
                    } else {
                        if (pedalPara != 0) {
                            ch4 = pedalPara.getCh();
                        } else {
                            ch4 = str;
                        }
                        str16 = strValueOf10;
                        updateRegistration(29, ch4);
                        if (ch4 != 0) {
                            data23 = ch4.getData();
                            str17 = str15;
                            str18 = str16;
                        } else {
                            str17 = str15;
                            str18 = str16;
                            data23 = 0;
                        }
                    }
                } else {
                    if (smk25MiniUsr != null) {
                        pedalPara = smk25MiniUsr.getPedalPara();
                    } else {
                        pedalPara = str;
                    }
                    if ((j & 288230384758423552L) == 0) {
                        if (pedalPara != 0) {
                            type3 = pedalPara.getType();
                        } else {
                            type3 = str;
                        }
                        str14 = str13;
                        updateRegistration(24, type3);
                        if (type3 != 0) {
                        }
                    } else {
                        if (pedalPara != 0) {
                            type3 = pedalPara.getType();
                        } else {
                            type3 = str;
                        }
                        str14 = str13;
                        updateRegistration(24, type3);
                        if (type3 != 0) {
                        }
                    }
                    if ((j & 2314854615239622912L) == 0) {
                        if (pedalPara != 0) {
                            dat3 = pedalPara.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j & 4406636445952L) == 0) {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        }
                        if ((j & 9007207845199872L) == 0) {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        } else {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        }
                        if ((j & 2305843017937846272L) == 0) {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        } else {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        }
                        str15 = strValueOf9;
                    } else {
                        if (pedalPara != 0) {
                            dat3 = pedalPara.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j & 4406636445952L) == 0) {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                addrU14 = (AddrU8) getFromArray(dat3, 2);
                            } else {
                                addrU14 = str;
                            }
                            updateRegistration(8, addrU14);
                            if (addrU14 != 0) {
                                data20 = addrU14.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf9 = String.valueOf(data20);
                        }
                        if ((j & 9007207845199872L) == 0) {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        } else {
                            if (dat3 != 0) {
                                addrU15 = (AddrU8) getFromArray(dat3, 1);
                            } else {
                                addrU15 = str;
                            }
                            updateRegistration(19, addrU15);
                            if (addrU15 != 0) {
                                data21 = addrU15.getData();
                            } else {
                                data21 = 0;
                            }
                            strValueOf10 = String.valueOf(data21);
                        }
                        if ((j & 2305843017937846272L) == 0) {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        } else {
                            if (dat3 != 0) {
                                addrU16 = (AddrU8) getFromArray(dat3, 0);
                            } else {
                                addrU16 = str;
                            }
                            updateRegistration(27, addrU16);
                            if (addrU16 != 0) {
                                data22 = addrU16.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf11 = String.valueOf(data22);
                        }
                        str15 = strValueOf9;
                    }
                    if ((j & (-9223372027727970304L)) == 0) {
                        if (pedalPara != 0) {
                            ch4 = pedalPara.getCh();
                        } else {
                            ch4 = str;
                        }
                        str16 = strValueOf10;
                        updateRegistration(29, ch4);
                        if (ch4 != 0) {
                            data23 = ch4.getData();
                            str17 = str15;
                            str18 = str16;
                        } else {
                            str17 = str15;
                            str18 = str16;
                            data23 = 0;
                        }
                    } else {
                        if (pedalPara != 0) {
                            ch4 = pedalPara.getCh();
                        } else {
                            ch4 = str;
                        }
                        str16 = strValueOf10;
                        updateRegistration(29, ch4);
                        if (ch4 != 0) {
                            data23 = ch4.getData();
                            str17 = str15;
                            str18 = str16;
                        } else {
                            str17 = str15;
                            str18 = str16;
                            data23 = 0;
                        }
                    }
                }
                if ((j & 90212791244300291L) == 0) {
                    if (smk25MiniUsr != null) {
                        modPara = smk25MiniUsr.getModPara();
                    } else {
                        modPara = str;
                    }
                    if ((j & 42949672962L) == 0) {
                        if (modPara != 0) {
                            speed = modPara.getSpeed();
                        } else {
                            speed = str;
                        }
                        str19 = str18;
                        updateRegistration(1, speed);
                        if (speed != 0) {
                        }
                    } else {
                        if (modPara != 0) {
                            speed = modPara.getSpeed();
                        } else {
                            speed = str;
                        }
                        str19 = str18;
                        updateRegistration(1, speed);
                        if (speed != 0) {
                        }
                    }
                    if ((j & 18155162842439681L) == 0) {
                        if (modPara != 0) {
                            dat4 = modPara.getDat();
                        } else {
                            dat4 = str;
                        }
                        if ((j & 25769803777L) == 0) {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        } else {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        }
                        if ((j & 140746078298112L) == 0) {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        } else {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        }
                        if ((j & 18014408174206976L) == 0) {
                        }
                        if (dat4 != 0) {
                            addrU19 = (AddrU8) getFromArray(dat4, 0);
                        } else {
                            addrU19 = str;
                        }
                        updateRegistration(20, addrU19);
                        if (addrU19 != 0) {
                            data27 = addrU19.getData();
                        } else {
                            data27 = 0;
                        }
                        if ((j & 18014407100465152L) == 0) {
                            strValueOf14 = String.valueOf(data27);
                            int i111111 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111111;
                        } else {
                            strValueOf14 = String.valueOf(data27);
                            int i111112 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111112;
                        }
                    } else {
                        if (modPara != 0) {
                            dat4 = modPara.getDat();
                        } else {
                            dat4 = str;
                        }
                        if ((j & 25769803777L) == 0) {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        } else {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        }
                        if ((j & 140746078298112L) == 0) {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        } else {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        }
                        if ((j & 18014408174206976L) == 0) {
                        }
                        if (dat4 != 0) {
                            addrU19 = (AddrU8) getFromArray(dat4, 0);
                        } else {
                            addrU19 = str;
                        }
                        updateRegistration(20, addrU19);
                        if (addrU19 != 0) {
                            data27 = addrU19.getData();
                        } else {
                            data27 = 0;
                        }
                        if ((j & 18014407100465152L) == 0) {
                            strValueOf14 = String.valueOf(data27);
                            int i111113 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111113;
                        } else {
                            strValueOf14 = String.valueOf(data27);
                            int i111114 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111114;
                        }
                    }
                    if ((j & 72057602632056832L) == 0) {
                        if (modPara != 0) {
                            ch5 = modPara.getCh();
                        } else {
                            ch5 = str;
                        }
                        str22 = str21;
                        updateRegistration(22, ch5);
                        if (ch5 != 0) {
                        }
                    } else {
                        if (modPara != 0) {
                            ch5 = modPara.getCh();
                        } else {
                            ch5 = str;
                        }
                        str22 = str21;
                        updateRegistration(22, ch5);
                        if (ch5 != 0) {
                        }
                    }
                    i9 = data19;
                    str23 = str22;
                } else {
                    if (smk25MiniUsr != null) {
                        modPara = smk25MiniUsr.getModPara();
                    } else {
                        modPara = str;
                    }
                    if ((j & 42949672962L) == 0) {
                        if (modPara != 0) {
                            speed = modPara.getSpeed();
                        } else {
                            speed = str;
                        }
                        str19 = str18;
                        updateRegistration(1, speed);
                        if (speed != 0) {
                        }
                    } else {
                        if (modPara != 0) {
                            speed = modPara.getSpeed();
                        } else {
                            speed = str;
                        }
                        str19 = str18;
                        updateRegistration(1, speed);
                        if (speed != 0) {
                        }
                    }
                    if ((j & 18155162842439681L) == 0) {
                        if (modPara != 0) {
                            dat4 = modPara.getDat();
                        } else {
                            dat4 = str;
                        }
                        if ((j & 25769803777L) == 0) {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        } else {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        }
                        if ((j & 140746078298112L) == 0) {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        } else {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        }
                        if ((j & 18014408174206976L) == 0) {
                        }
                        if (dat4 != 0) {
                            addrU19 = (AddrU8) getFromArray(dat4, 0);
                        } else {
                            addrU19 = str;
                        }
                        updateRegistration(20, addrU19);
                        if (addrU19 != 0) {
                            data27 = addrU19.getData();
                        } else {
                            data27 = 0;
                        }
                        if ((j & 18014407100465152L) == 0) {
                            strValueOf14 = String.valueOf(data27);
                            int i111115 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111115;
                        } else {
                            strValueOf14 = String.valueOf(data27);
                            int i111116 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111116;
                        }
                    } else {
                        if (modPara != 0) {
                            dat4 = modPara.getDat();
                        } else {
                            dat4 = str;
                        }
                        if ((j & 25769803777L) == 0) {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        } else {
                            if (dat4 != 0) {
                                addrU17 = (AddrU8) getFromArray(dat4, 2);
                            } else {
                                addrU17 = str;
                            }
                            updateRegistration(0, addrU17);
                            if (addrU17 != 0) {
                                data25 = addrU17.getData();
                            } else {
                                data25 = 0;
                            }
                            strValueOf12 = String.valueOf(data25);
                        }
                        if ((j & 140746078298112L) == 0) {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        } else {
                            if (dat4 != 0) {
                                addrU18 = (AddrU8) getFromArray(dat4, 1);
                            } else {
                                addrU18 = str;
                            }
                            updateRegistration(13, addrU18);
                            if (addrU18 != 0) {
                                data26 = addrU18.getData();
                            } else {
                                data26 = 0;
                            }
                            strValueOf13 = String.valueOf(data26);
                        }
                        if ((j & 18014408174206976L) == 0) {
                        }
                        if (dat4 != 0) {
                            addrU19 = (AddrU8) getFromArray(dat4, 0);
                        } else {
                            addrU19 = str;
                        }
                        updateRegistration(20, addrU19);
                        if (addrU19 != 0) {
                            data27 = addrU19.getData();
                        } else {
                            data27 = 0;
                        }
                        if ((j & 18014407100465152L) == 0) {
                            strValueOf14 = String.valueOf(data27);
                            int i111117 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111117;
                        } else {
                            strValueOf14 = String.valueOf(data27);
                            int i111118 = data27;
                            str20 = strValueOf13;
                            str21 = strValueOf12;
                            i8 = i111118;
                        }
                    }
                    if ((j & 72057602632056832L) == 0) {
                        if (modPara != 0) {
                            ch5 = modPara.getCh();
                        } else {
                            ch5 = str;
                        }
                        str22 = str21;
                        updateRegistration(22, ch5);
                        if (ch5 != 0) {
                        }
                    } else {
                        if (modPara != 0) {
                            ch5 = modPara.getCh();
                        } else {
                            ch5 = str;
                        }
                        str22 = str21;
                        updateRegistration(22, ch5);
                        if (ch5 != 0) {
                        }
                    }
                    i9 = data19;
                    str23 = str22;
                }
            }
            i2 = data6;
            if ((j & 1190358787901440064L) == 0) {
                if (smk25MiniUsr != null) {
                    btnPara = smk25MiniUsr.getBtnPara();
                } else {
                    btnPara = str;
                }
                if ((j & 1154048513756168256L) == 0) {
                    if (btnPara != 0) {
                        dat = btnPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j & 1108101562432L) == 0) {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    }
                    if ((j & 1125908496842752L) == 0) {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    } else {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    }
                    if ((j & 1152921514337632256L) == 0) {
                    }
                    if (dat != 0) {
                        addrU10 = (AddrU8) getFromArray(dat, 0);
                    } else {
                        addrU10 = str;
                    }
                    updateRegistration(26, addrU10);
                    if (addrU10 != 0) {
                        data10 = addrU10.getData();
                    } else {
                        data10 = 0;
                    }
                    if ((j & 1152921513263890432L) == 0) {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    } else {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    }
                } else {
                    if (btnPara != 0) {
                        dat = btnPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j & 1108101562432L) == 0) {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    }
                    if ((j & 1125908496842752L) == 0) {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    } else {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    }
                    if ((j & 1152921514337632256L) == 0) {
                    }
                    if (dat != 0) {
                        addrU10 = (AddrU8) getFromArray(dat, 0);
                    } else {
                        addrU10 = str;
                    }
                    updateRegistration(26, addrU10);
                    if (addrU10 != 0) {
                        data10 = addrU10.getData();
                    } else {
                        data10 = 0;
                    }
                    if ((j & 1152921513263890432L) == 0) {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    } else {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    }
                }
                if ((j & 281483566661632L) == 0) {
                    if (btnPara != 0) {
                        ch2 = btnPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str4 = str3;
                    updateRegistration(14, ch2);
                    if (ch2 != 0) {
                    }
                } else {
                    if (btnPara != 0) {
                        ch2 = btnPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str4 = str3;
                    updateRegistration(14, ch2);
                    if (ch2 != 0) {
                    }
                }
                if (i10 == 0) {
                    if (btnPara != 0) {
                        sys = btnPara.getSys();
                    } else {
                        sys = str;
                    }
                    if (smk25MiniUsrFragment != null) {
                        sysEx = smk25MiniUsrFragment.getSysEx(sys);
                    } else {
                        sysEx = str;
                    }
                } else {
                    if (btnPara != 0) {
                        sys = btnPara.getSys();
                    } else {
                        sys = str;
                    }
                    if (smk25MiniUsrFragment != null) {
                        sysEx = smk25MiniUsrFragment.getSysEx(sys);
                    } else {
                        sysEx = str;
                    }
                }
                if ((j & 36028805610995712L) == 0) {
                    if (btnPara != 0) {
                        type = btnPara.getType();
                    } else {
                        type = str;
                    }
                    updateRegistration(21, type);
                    if (type != 0) {
                        int i11128 = i3;
                        str5 = strValueOf5;
                        i4 = i11128;
                        i5 = data11;
                        str6 = str2;
                        data12 = type.getData();
                        str7 = str4;
                    } else {
                        int i11129 = i3;
                        str5 = strValueOf5;
                        i4 = i11129;
                        i5 = data11;
                        str6 = str2;
                        str7 = str4;
                        data12 = 0;
                    }
                } else {
                    if (btnPara != 0) {
                        type = btnPara.getType();
                    } else {
                        type = str;
                    }
                    updateRegistration(21, type);
                    if (type != 0) {
                        int i111210 = i3;
                        str5 = strValueOf5;
                        i4 = i111210;
                        i5 = data11;
                        str6 = str2;
                        data12 = type.getData();
                        str7 = str4;
                    } else {
                        int i111211 = i3;
                        str5 = strValueOf5;
                        i4 = i111211;
                        i5 = data11;
                        str6 = str2;
                        str7 = str4;
                        data12 = 0;
                    }
                }
            } else {
                if (smk25MiniUsr != null) {
                    btnPara = smk25MiniUsr.getBtnPara();
                } else {
                    btnPara = str;
                }
                if ((j & 1154048513756168256L) == 0) {
                    if (btnPara != 0) {
                        dat = btnPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j & 1108101562432L) == 0) {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    }
                    if ((j & 1125908496842752L) == 0) {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    } else {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    }
                    if ((j & 1152921514337632256L) == 0) {
                    }
                    if (dat != 0) {
                        addrU10 = (AddrU8) getFromArray(dat, 0);
                    } else {
                        addrU10 = str;
                    }
                    updateRegistration(26, addrU10);
                    if (addrU10 != 0) {
                        data10 = addrU10.getData();
                    } else {
                        data10 = 0;
                    }
                    if ((j & 1152921513263890432L) == 0) {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    } else {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    }
                } else {
                    if (btnPara != 0) {
                        dat = btnPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j & 1108101562432L) == 0) {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            addrU8 = (AddrU8) getFromArray(dat, 1);
                        } else {
                            addrU8 = str;
                        }
                        updateRegistration(6, addrU8);
                        if (addrU8 != 0) {
                            data8 = addrU8.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf3 = String.valueOf(data8);
                    }
                    if ((j & 1125908496842752L) == 0) {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    } else {
                        if (dat != 0) {
                            addrU9 = (AddrU8) getFromArray(dat, 2);
                        } else {
                            addrU9 = str;
                        }
                        updateRegistration(16, addrU9);
                        if (addrU9 != 0) {
                            data9 = addrU9.getData();
                        } else {
                            data9 = 0;
                        }
                        strValueOf4 = String.valueOf(data9);
                    }
                    if ((j & 1152921514337632256L) == 0) {
                    }
                    if (dat != 0) {
                        addrU10 = (AddrU8) getFromArray(dat, 0);
                    } else {
                        addrU10 = str;
                    }
                    updateRegistration(26, addrU10);
                    if (addrU10 != 0) {
                        data10 = addrU10.getData();
                    } else {
                        data10 = 0;
                    }
                    if ((j & 1152921513263890432L) == 0) {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    } else {
                        strValueOf5 = String.valueOf(data10);
                        i3 = data10;
                        str2 = strValueOf4;
                        str3 = strValueOf3;
                    }
                }
                if ((j & 281483566661632L) == 0) {
                    if (btnPara != 0) {
                        ch2 = btnPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str4 = str3;
                    updateRegistration(14, ch2);
                    if (ch2 != 0) {
                    }
                } else {
                    if (btnPara != 0) {
                        ch2 = btnPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str4 = str3;
                    updateRegistration(14, ch2);
                    if (ch2 != 0) {
                    }
                }
                if (i10 == 0) {
                    if (btnPara != 0) {
                        sys = btnPara.getSys();
                    } else {
                        sys = str;
                    }
                    if (smk25MiniUsrFragment != null) {
                        sysEx = smk25MiniUsrFragment.getSysEx(sys);
                    } else {
                        sysEx = str;
                    }
                } else {
                    if (btnPara != 0) {
                        sys = btnPara.getSys();
                    } else {
                        sys = str;
                    }
                    if (smk25MiniUsrFragment != null) {
                        sysEx = smk25MiniUsrFragment.getSysEx(sys);
                    } else {
                        sysEx = str;
                    }
                }
                if ((j & 36028805610995712L) == 0) {
                    if (btnPara != 0) {
                        type = btnPara.getType();
                    } else {
                        type = str;
                    }
                    updateRegistration(21, type);
                    if (type != 0) {
                        int i111212 = i3;
                        str5 = strValueOf5;
                        i4 = i111212;
                        i5 = data11;
                        str6 = str2;
                        data12 = type.getData();
                        str7 = str4;
                    } else {
                        int i111213 = i3;
                        str5 = strValueOf5;
                        i4 = i111213;
                        i5 = data11;
                        str6 = str2;
                        str7 = str4;
                        data12 = 0;
                    }
                } else {
                    if (btnPara != 0) {
                        type = btnPara.getType();
                    } else {
                        type = str;
                    }
                    updateRegistration(21, type);
                    if (type != 0) {
                        int i111214 = i3;
                        str5 = strValueOf5;
                        i4 = i111214;
                        i5 = data11;
                        str6 = str2;
                        data12 = type.getData();
                        str7 = str4;
                    } else {
                        int i111215 = i3;
                        str5 = strValueOf5;
                        i4 = i111215;
                        i5 = data11;
                        str6 = str2;
                        str7 = str4;
                        data12 = 0;
                    }
                }
            }
            if ((j & 2817920526876856L) == 0) {
                if (smk25MiniUsr != null) {
                    encoderPara = smk25MiniUsr.getEncoderPara();
                } else {
                    encoderPara = str;
                }
                if ((j & 283467841552L) == 0) {
                    if (encoderPara != 0) {
                        ch3 = encoderPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(4, ch3);
                    if (ch3 != 0) {
                    }
                } else {
                    if (encoderPara != 0) {
                        ch3 = encoderPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(4, ch3);
                    if (ch3 != 0) {
                    }
                }
                if ((j & 2207613190272L) == 0) {
                    if (encoderPara != 0) {
                        curve = encoderPara.getCurve();
                    } else {
                        curve = str;
                    }
                    i6 = data13;
                    updateRegistration(7, curve);
                    if (curve != 0) {
                    }
                } else {
                    if (encoderPara != 0) {
                        curve = encoderPara.getCurve();
                    } else {
                        curve = str;
                    }
                    i6 = data13;
                    updateRegistration(7, curve);
                    if (curve != 0) {
                    }
                }
                if ((j & 563646811897896L) == 0) {
                    if (encoderPara != 0) {
                        dat2 = encoderPara.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j & 147102629896L) == 0) {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    } else {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    }
                    if ((j & 558345748512L) == 0) {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    } else {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    }
                    if ((j & 562958543388672L) == 0) {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    } else {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    }
                    str10 = strValueOf6;
                } else {
                    if (encoderPara != 0) {
                        dat2 = encoderPara.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j & 147102629896L) == 0) {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    } else {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    }
                    if ((j & 558345748512L) == 0) {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    } else {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    }
                    if ((j & 562958543388672L) == 0) {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    } else {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    }
                    str10 = strValueOf6;
                }
                if ((j & 2251808403750912L) == 0) {
                    if (encoderPara != 0) {
                        type2 = encoderPara.getType();
                    } else {
                        type2 = str;
                    }
                    str11 = strValueOf8;
                    updateRegistration(17, type2);
                    if (type2 != 0) {
                        data18 = type2.getData();
                        str12 = strValueOf7;
                        str13 = str11;
                    } else {
                        str12 = strValueOf7;
                        str13 = str11;
                        data18 = 0;
                    }
                } else {
                    if (encoderPara != 0) {
                        type2 = encoderPara.getType();
                    } else {
                        type2 = str;
                    }
                    str11 = strValueOf8;
                    updateRegistration(17, type2);
                    if (type2 != 0) {
                        data18 = type2.getData();
                        str12 = strValueOf7;
                        str13 = str11;
                    } else {
                        str12 = strValueOf7;
                        str13 = str11;
                        data18 = 0;
                    }
                }
            } else {
                if (smk25MiniUsr != null) {
                    encoderPara = smk25MiniUsr.getEncoderPara();
                } else {
                    encoderPara = str;
                }
                if ((j & 283467841552L) == 0) {
                    if (encoderPara != 0) {
                        ch3 = encoderPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(4, ch3);
                    if (ch3 != 0) {
                    }
                } else {
                    if (encoderPara != 0) {
                        ch3 = encoderPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(4, ch3);
                    if (ch3 != 0) {
                    }
                }
                if ((j & 2207613190272L) == 0) {
                    if (encoderPara != 0) {
                        curve = encoderPara.getCurve();
                    } else {
                        curve = str;
                    }
                    i6 = data13;
                    updateRegistration(7, curve);
                    if (curve != 0) {
                    }
                } else {
                    if (encoderPara != 0) {
                        curve = encoderPara.getCurve();
                    } else {
                        curve = str;
                    }
                    i6 = data13;
                    updateRegistration(7, curve);
                    if (curve != 0) {
                    }
                }
                if ((j & 563646811897896L) == 0) {
                    if (encoderPara != 0) {
                        dat2 = encoderPara.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j & 147102629896L) == 0) {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    } else {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    }
                    if ((j & 558345748512L) == 0) {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    } else {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    }
                    if ((j & 562958543388672L) == 0) {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    } else {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    }
                    str10 = strValueOf6;
                } else {
                    if (encoderPara != 0) {
                        dat2 = encoderPara.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j & 147102629896L) == 0) {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    } else {
                        i7 = data14;
                        if (dat2 != 0) {
                            addrU11 = (AddrU8) getFromArray(dat2, 0);
                        } else {
                            addrU11 = str;
                        }
                        str9 = str7;
                        updateRegistration(3, addrU11);
                        if (addrU11 != 0) {
                            data15 = addrU11.getData();
                        } else {
                            data15 = 0;
                        }
                        if ((j & 146028888072L) == 0) {
                            strValueOf6 = String.valueOf(data15);
                        } else {
                            strValueOf6 = String.valueOf(data15);
                        }
                    }
                    if ((j & 558345748512L) == 0) {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    } else {
                        if (dat2 != 0) {
                            addrU12 = (AddrU8) getFromArray(dat2, 1);
                        } else {
                            addrU12 = str;
                        }
                        updateRegistration(5, addrU12);
                        if (addrU12 != 0) {
                            data16 = addrU12.getData();
                        } else {
                            data16 = 0;
                        }
                        strValueOf7 = String.valueOf(data16);
                    }
                    if ((j & 562958543388672L) == 0) {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    } else {
                        if (dat2 != 0) {
                            addrU13 = (AddrU8) getFromArray(dat2, 2);
                        } else {
                            addrU13 = str;
                        }
                        updateRegistration(15, addrU13);
                        if (addrU13 != 0) {
                            data17 = addrU13.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf8 = String.valueOf(data17);
                    }
                    str10 = strValueOf6;
                }
                if ((j & 2251808403750912L) == 0) {
                    if (encoderPara != 0) {
                        type2 = encoderPara.getType();
                    } else {
                        type2 = str;
                    }
                    str11 = strValueOf8;
                    updateRegistration(17, type2);
                    if (type2 != 0) {
                        data18 = type2.getData();
                        str12 = strValueOf7;
                        str13 = str11;
                    } else {
                        str12 = strValueOf7;
                        str13 = str11;
                        data18 = 0;
                    }
                } else {
                    if (encoderPara != 0) {
                        type2 = encoderPara.getType();
                    } else {
                        type2 = str;
                    }
                    str11 = strValueOf8;
                    updateRegistration(17, type2);
                    if (type2 != 0) {
                        data18 = type2.getData();
                        str12 = strValueOf7;
                        str13 = str11;
                    } else {
                        str12 = strValueOf7;
                        str13 = str11;
                        data18 = 0;
                    }
                }
            }
            if ((j & (-6620287044909793024L)) == 0) {
                if (smk25MiniUsr != null) {
                    pedalPara = smk25MiniUsr.getPedalPara();
                } else {
                    pedalPara = str;
                }
                if ((j & 288230384758423552L) == 0) {
                    if (pedalPara != 0) {
                        type3 = pedalPara.getType();
                    } else {
                        type3 = str;
                    }
                    str14 = str13;
                    updateRegistration(24, type3);
                    if (type3 != 0) {
                    }
                } else {
                    if (pedalPara != 0) {
                        type3 = pedalPara.getType();
                    } else {
                        type3 = str;
                    }
                    str14 = str13;
                    updateRegistration(24, type3);
                    if (type3 != 0) {
                    }
                }
                if ((j & 2314854615239622912L) == 0) {
                    if (pedalPara != 0) {
                        dat3 = pedalPara.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j & 4406636445952L) == 0) {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    }
                    if ((j & 9007207845199872L) == 0) {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    } else {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    }
                    if ((j & 2305843017937846272L) == 0) {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    } else {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    }
                    str15 = strValueOf9;
                } else {
                    if (pedalPara != 0) {
                        dat3 = pedalPara.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j & 4406636445952L) == 0) {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    }
                    if ((j & 9007207845199872L) == 0) {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    } else {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    }
                    if ((j & 2305843017937846272L) == 0) {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    } else {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    }
                    str15 = strValueOf9;
                }
                if ((j & (-9223372027727970304L)) == 0) {
                    if (pedalPara != 0) {
                        ch4 = pedalPara.getCh();
                    } else {
                        ch4 = str;
                    }
                    str16 = strValueOf10;
                    updateRegistration(29, ch4);
                    if (ch4 != 0) {
                        data23 = ch4.getData();
                        str17 = str15;
                        str18 = str16;
                    } else {
                        str17 = str15;
                        str18 = str16;
                        data23 = 0;
                    }
                } else {
                    if (pedalPara != 0) {
                        ch4 = pedalPara.getCh();
                    } else {
                        ch4 = str;
                    }
                    str16 = strValueOf10;
                    updateRegistration(29, ch4);
                    if (ch4 != 0) {
                        data23 = ch4.getData();
                        str17 = str15;
                        str18 = str16;
                    } else {
                        str17 = str15;
                        str18 = str16;
                        data23 = 0;
                    }
                }
            } else {
                if (smk25MiniUsr != null) {
                    pedalPara = smk25MiniUsr.getPedalPara();
                } else {
                    pedalPara = str;
                }
                if ((j & 288230384758423552L) == 0) {
                    if (pedalPara != 0) {
                        type3 = pedalPara.getType();
                    } else {
                        type3 = str;
                    }
                    str14 = str13;
                    updateRegistration(24, type3);
                    if (type3 != 0) {
                    }
                } else {
                    if (pedalPara != 0) {
                        type3 = pedalPara.getType();
                    } else {
                        type3 = str;
                    }
                    str14 = str13;
                    updateRegistration(24, type3);
                    if (type3 != 0) {
                    }
                }
                if ((j & 2314854615239622912L) == 0) {
                    if (pedalPara != 0) {
                        dat3 = pedalPara.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j & 4406636445952L) == 0) {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    }
                    if ((j & 9007207845199872L) == 0) {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    } else {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    }
                    if ((j & 2305843017937846272L) == 0) {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    } else {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    }
                    str15 = strValueOf9;
                } else {
                    if (pedalPara != 0) {
                        dat3 = pedalPara.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j & 4406636445952L) == 0) {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            addrU14 = (AddrU8) getFromArray(dat3, 2);
                        } else {
                            addrU14 = str;
                        }
                        updateRegistration(8, addrU14);
                        if (addrU14 != 0) {
                            data20 = addrU14.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf9 = String.valueOf(data20);
                    }
                    if ((j & 9007207845199872L) == 0) {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    } else {
                        if (dat3 != 0) {
                            addrU15 = (AddrU8) getFromArray(dat3, 1);
                        } else {
                            addrU15 = str;
                        }
                        updateRegistration(19, addrU15);
                        if (addrU15 != 0) {
                            data21 = addrU15.getData();
                        } else {
                            data21 = 0;
                        }
                        strValueOf10 = String.valueOf(data21);
                    }
                    if ((j & 2305843017937846272L) == 0) {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    } else {
                        if (dat3 != 0) {
                            addrU16 = (AddrU8) getFromArray(dat3, 0);
                        } else {
                            addrU16 = str;
                        }
                        updateRegistration(27, addrU16);
                        if (addrU16 != 0) {
                            data22 = addrU16.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf11 = String.valueOf(data22);
                    }
                    str15 = strValueOf9;
                }
                if ((j & (-9223372027727970304L)) == 0) {
                    if (pedalPara != 0) {
                        ch4 = pedalPara.getCh();
                    } else {
                        ch4 = str;
                    }
                    str16 = strValueOf10;
                    updateRegistration(29, ch4);
                    if (ch4 != 0) {
                        data23 = ch4.getData();
                        str17 = str15;
                        str18 = str16;
                    } else {
                        str17 = str15;
                        str18 = str16;
                        data23 = 0;
                    }
                } else {
                    if (pedalPara != 0) {
                        ch4 = pedalPara.getCh();
                    } else {
                        ch4 = str;
                    }
                    str16 = strValueOf10;
                    updateRegistration(29, ch4);
                    if (ch4 != 0) {
                        data23 = ch4.getData();
                        str17 = str15;
                        str18 = str16;
                    } else {
                        str17 = str15;
                        str18 = str16;
                        data23 = 0;
                    }
                }
            }
            if ((j & 90212791244300291L) == 0) {
                if (smk25MiniUsr != null) {
                    modPara = smk25MiniUsr.getModPara();
                } else {
                    modPara = str;
                }
                if ((j & 42949672962L) == 0) {
                    if (modPara != 0) {
                        speed = modPara.getSpeed();
                    } else {
                        speed = str;
                    }
                    str19 = str18;
                    updateRegistration(1, speed);
                    if (speed != 0) {
                    }
                } else {
                    if (modPara != 0) {
                        speed = modPara.getSpeed();
                    } else {
                        speed = str;
                    }
                    str19 = str18;
                    updateRegistration(1, speed);
                    if (speed != 0) {
                    }
                }
                if ((j & 18155162842439681L) == 0) {
                    if (modPara != 0) {
                        dat4 = modPara.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j & 25769803777L) == 0) {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    } else {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    }
                    if ((j & 140746078298112L) == 0) {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    } else {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    }
                    if ((j & 18014408174206976L) == 0) {
                    }
                    if (dat4 != 0) {
                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                    } else {
                        addrU19 = str;
                    }
                    updateRegistration(20, addrU19);
                    if (addrU19 != 0) {
                        data27 = addrU19.getData();
                    } else {
                        data27 = 0;
                    }
                    if ((j & 18014407100465152L) == 0) {
                        strValueOf14 = String.valueOf(data27);
                        int i111119 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i111119;
                    } else {
                        strValueOf14 = String.valueOf(data27);
                        int i1111110 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111110;
                    }
                } else {
                    if (modPara != 0) {
                        dat4 = modPara.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j & 25769803777L) == 0) {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    } else {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    }
                    if ((j & 140746078298112L) == 0) {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    } else {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    }
                    if ((j & 18014408174206976L) == 0) {
                    }
                    if (dat4 != 0) {
                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                    } else {
                        addrU19 = str;
                    }
                    updateRegistration(20, addrU19);
                    if (addrU19 != 0) {
                        data27 = addrU19.getData();
                    } else {
                        data27 = 0;
                    }
                    if ((j & 18014407100465152L) == 0) {
                        strValueOf14 = String.valueOf(data27);
                        int i1111111 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111111;
                    } else {
                        strValueOf14 = String.valueOf(data27);
                        int i1111112 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111112;
                    }
                }
                if ((j & 72057602632056832L) == 0) {
                    if (modPara != 0) {
                        ch5 = modPara.getCh();
                    } else {
                        ch5 = str;
                    }
                    str22 = str21;
                    updateRegistration(22, ch5);
                    if (ch5 != 0) {
                    }
                } else {
                    if (modPara != 0) {
                        ch5 = modPara.getCh();
                    } else {
                        ch5 = str;
                    }
                    str22 = str21;
                    updateRegistration(22, ch5);
                    if (ch5 != 0) {
                    }
                }
                i9 = data19;
                str23 = str22;
            } else {
                if (smk25MiniUsr != null) {
                    modPara = smk25MiniUsr.getModPara();
                } else {
                    modPara = str;
                }
                if ((j & 42949672962L) == 0) {
                    if (modPara != 0) {
                        speed = modPara.getSpeed();
                    } else {
                        speed = str;
                    }
                    str19 = str18;
                    updateRegistration(1, speed);
                    if (speed != 0) {
                    }
                } else {
                    if (modPara != 0) {
                        speed = modPara.getSpeed();
                    } else {
                        speed = str;
                    }
                    str19 = str18;
                    updateRegistration(1, speed);
                    if (speed != 0) {
                    }
                }
                if ((j & 18155162842439681L) == 0) {
                    if (modPara != 0) {
                        dat4 = modPara.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j & 25769803777L) == 0) {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    } else {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    }
                    if ((j & 140746078298112L) == 0) {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    } else {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    }
                    if ((j & 18014408174206976L) == 0) {
                    }
                    if (dat4 != 0) {
                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                    } else {
                        addrU19 = str;
                    }
                    updateRegistration(20, addrU19);
                    if (addrU19 != 0) {
                        data27 = addrU19.getData();
                    } else {
                        data27 = 0;
                    }
                    if ((j & 18014407100465152L) == 0) {
                        strValueOf14 = String.valueOf(data27);
                        int i1111113 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111113;
                    } else {
                        strValueOf14 = String.valueOf(data27);
                        int i1111114 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111114;
                    }
                } else {
                    if (modPara != 0) {
                        dat4 = modPara.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j & 25769803777L) == 0) {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    } else {
                        if (dat4 != 0) {
                            addrU17 = (AddrU8) getFromArray(dat4, 2);
                        } else {
                            addrU17 = str;
                        }
                        updateRegistration(0, addrU17);
                        if (addrU17 != 0) {
                            data25 = addrU17.getData();
                        } else {
                            data25 = 0;
                        }
                        strValueOf12 = String.valueOf(data25);
                    }
                    if ((j & 140746078298112L) == 0) {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    } else {
                        if (dat4 != 0) {
                            addrU18 = (AddrU8) getFromArray(dat4, 1);
                        } else {
                            addrU18 = str;
                        }
                        updateRegistration(13, addrU18);
                        if (addrU18 != 0) {
                            data26 = addrU18.getData();
                        } else {
                            data26 = 0;
                        }
                        strValueOf13 = String.valueOf(data26);
                    }
                    if ((j & 18014408174206976L) == 0) {
                    }
                    if (dat4 != 0) {
                        addrU19 = (AddrU8) getFromArray(dat4, 0);
                    } else {
                        addrU19 = str;
                    }
                    updateRegistration(20, addrU19);
                    if (addrU19 != 0) {
                        data27 = addrU19.getData();
                    } else {
                        data27 = 0;
                    }
                    if ((j & 18014407100465152L) == 0) {
                        strValueOf14 = String.valueOf(data27);
                        int i1111115 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111115;
                    } else {
                        strValueOf14 = String.valueOf(data27);
                        int i1111116 = data27;
                        str20 = strValueOf13;
                        str21 = strValueOf12;
                        i8 = i1111116;
                    }
                }
                if ((j & 72057602632056832L) == 0) {
                    if (modPara != 0) {
                        ch5 = modPara.getCh();
                    } else {
                        ch5 = str;
                    }
                    str22 = str21;
                    updateRegistration(22, ch5);
                    if (ch5 != 0) {
                    }
                } else {
                    if (modPara != 0) {
                        ch5 = modPara.getCh();
                    } else {
                        ch5 = str;
                    }
                    str22 = str21;
                    updateRegistration(22, ch5);
                    if (ch5 != 0) {
                    }
                }
                i9 = data19;
                str23 = str22;
            }
        }
        if ((j & 1170936050287116296L) == 0 && (j2 & 511) == 0) {
            i9 = i9;
            str27 = str;
            str24 = str27;
            str26 = str24;
            str28 = str26;
            str25 = str28;
            psrValue2Text = str25;
            str29 = psrValue2Text;
            str30 = str29;
        } else {
            long j4 = j & 1073741824;
            String knobMinText = ((j4 == 0 && (j2 & 258) == 0) || usrBinding == null) ? str : usrBinding.getKnobMinText();
            String knobMaxText = ((j4 == 0 && (j2 & 260) == 0) || usrBinding == null) ? str : usrBinding.getKnobMaxText();
            String psrCCText = ((j4 == 0 && (j2 & 272) == 0) || usrBinding == null) ? str : usrBinding.getPsrCCText();
            String psrValue1Text = ((j4 == 0 && (j2 & 320) == 0) || usrBinding == null) ? str : usrBinding.getPsrValue1Text();
            if ((j & 147102629896L) == 0 && (j2 & 257) == 0) {
                i9 = i9;
            } else {
                Object[] knobCCStr = usrBinding != null ? usrBinding.getKnobCCStr() : str;
                if (knobCCStr != 0) {
                    str24 = (String) getFromArray(knobCCStr, data15);
                }
                if ((j & 1152921514337632256L) == 0 || (j2 & 288) != 0) {
                    if (usrBinding != null) {
                        psrCCStr = usrBinding.getPsrCCStr();
                    } else {
                        objArr = str;
                    }
                    if (objArr != 0) {
                        objArr = psrCCStr;
                        str25 = (String) getFromArray(objArr, i4);
                    } else {
                        objArr = psrCCStr;
                        str25 = str;
                    }
                } else {
                    objArr = psrCCStr;
                    str25 = str;
                }
                if ((j & 18014408174206976L) == 0 || (j2 & 264) != 0) {
                    if (usrBinding != null) {
                        modCCStr = usrBinding.getModCCStr();
                    } else {
                        objArr2 = str;
                    }
                    if (objArr2 != 0) {
                        str26 = (String) getFromArray(objArr2, i8);
                    } else {
                        objArr2 = modCCStr;
                        str26 = str;
                    }
                } else {
                    objArr2 = modCCStr;
                    str26 = str;
                }
                if (j4 == 0) {
                    objArr2 = modCCStr;
                    if ((j2 & 384) != 0) {
                        psrValue2Text = str;
                    } else if (usrBinding != null) {
                        psrValue2Text = usrBinding.getPsrValue2Text();
                    } else {
                        psrValue2Text = str;
                    }
                } else if (usrBinding != null) {
                    psrValue2Text = usrBinding.getPsrValue2Text();
                } else {
                    psrValue2Text = str;
                }
                str27 = knobMinText;
                str28 = knobMaxText;
                str29 = psrCCText;
                str30 = psrValue1Text;
            }
            str24 = str;
            if ((j & 1152921514337632256L) == 0) {
                if (usrBinding != null) {
                    psrCCStr = usrBinding.getPsrCCStr();
                } else {
                    objArr = str;
                }
                if (objArr != 0) {
                    objArr = psrCCStr;
                    str25 = (String) getFromArray(objArr, i4);
                } else {
                    objArr = psrCCStr;
                    str25 = str;
                }
            } else {
                if (usrBinding != null) {
                    psrCCStr = usrBinding.getPsrCCStr();
                } else {
                    objArr = str;
                }
                if (objArr != 0) {
                    objArr = psrCCStr;
                    str25 = (String) getFromArray(objArr, i4);
                } else {
                    objArr = psrCCStr;
                    str25 = str;
                }
            }
            if ((j & 18014408174206976L) == 0) {
                if (usrBinding != null) {
                    modCCStr = usrBinding.getModCCStr();
                } else {
                    objArr2 = str;
                }
                if (objArr2 != 0) {
                    str26 = (String) getFromArray(objArr2, i8);
                } else {
                    objArr2 = modCCStr;
                    str26 = str;
                }
            } else {
                if (usrBinding != null) {
                    modCCStr = usrBinding.getModCCStr();
                } else {
                    objArr2 = str;
                }
                if (objArr2 != 0) {
                    str26 = (String) getFromArray(objArr2, i8);
                } else {
                    objArr2 = modCCStr;
                    str26 = str;
                }
            }
            if (j4 == 0) {
                objArr2 = modCCStr;
                if ((j2 & 384) != 0) {
                    psrValue2Text = str;
                } else if (usrBinding != null) {
                    psrValue2Text = usrBinding.getPsrValue2Text();
                } else {
                    psrValue2Text = str;
                }
            } else if (usrBinding != null) {
                psrValue2Text = usrBinding.getPsrValue2Text();
            } else {
                psrValue2Text = str;
            }
            str27 = knobMinText;
            str28 = knobMaxText;
            str29 = psrCCText;
            str30 = psrValue1Text;
        }
        if ((j & j3) != 0 || (j2 & 256) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keyCurve, data2);
        }
        long j5 = j2 & 256;
        if (j5 != 0) {
            ?? r11 = str;
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keyCurve, this.mCallback50, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keybedChannel, this.mCallback49, r11, r11);
            TextViewBindingAdapter.setTextWatcher(this.keybedMaxVel, r11, r11, this.mCallback54, r11);
            TextViewBindingAdapter.setTextWatcher(this.keybedMinVel, r11, r11, this.mCallback53, r11);
            TextViewBindingAdapter.setTextWatcher(this.knobMaxVal, r11, r11, this.mCallback60, r11);
            TextViewBindingAdapter.setTextWatcher(this.knobMinVal, r11, r11, this.mCallback59, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.knobType, this.mCallback55, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView10, this.mCallback57, r11, r11);
            TextViewBindingAdapter.setTextWatcher(this.mboundView11, r11, r11, this.mCallback58, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView17, this.mCallback61, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView23, this.mCallback66, r11, r11);
            this.mboundView25.setOnClickListener(this.mCallback68);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView27, this.mCallback70, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView28, this.mCallback71, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView33, this.mCallback76, r11, r11);
            TextViewBindingAdapter.setTextWatcher(this.mboundView35, r11, r11, this.mCallback77, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView9, this.mCallback56, r11, r11);
            TextViewBindingAdapter.setTextWatcher(this.modCCVal, r11, r11, this.mCallback63, r11);
            TextViewBindingAdapter.setTextWatcher(this.modMaxVal, r11, r11, this.mCallback65, r11);
            TextViewBindingAdapter.setTextWatcher(this.modMinVal, r11, r11, this.mCallback64, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.modSpeed, this.mCallback62, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.octive, this.mCallback51, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.pitchSpeed, this.mCallback67, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.port, this.mCallback69, r11, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.psrMode, this.mCallback75, r11, r11);
            TextViewBindingAdapter.setTextWatcher(this.psrValue1, r11, r11, this.mCallback78, r11);
            TextViewBindingAdapter.setTextWatcher(this.psrValue2, r11, r11, this.mCallback79, r11);
            TextViewBindingAdapter.setTextWatcher(this.sustainCCVal, r11, r11, this.mCallback72, r11);
            TextViewBindingAdapter.setTextWatcher(this.sustainDownVal, r11, r11, this.mCallback74, r11);
            TextViewBindingAdapter.setTextWatcher(this.sustainUpVal, r11, r11, this.mCallback73, r11);
            TextViewBindingAdapter.setTextWatcher(this.sysExValue, r11, r11, this.mCallback80, r11);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.transpose, this.mCallback52, r11, r11);
            RadioGroupBindingAdapter.setListeners(this.typeRadio, this.mCallback48, r11);
        }
        if ((j & 35192962025472L) != 0 || j5 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keybedChannel, data3);
        }
        if ((j & 77309411332L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.keybedMaxVel, strValueOf);
        }
        if ((j & 8804682957312L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.keybedMinVel, strValueOf2);
        }
        if ((j & 147102629896L) != 0 || (j2 & 257) != 0) {
            TextViewBindingAdapter.setText(this.knobCCStr, str24);
        }
        long j6 = j & 1073741824;
        if (j6 != 0 || (j2 & 260) != 0) {
            TextViewBindingAdapter.setText(this.knobMaxText, str28);
        }
        if ((j & 562958543388672L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.knobMaxVal, str14);
        }
        if (j6 != 0 || (j2 & 258) != 0) {
            TextViewBindingAdapter.setText(this.knobMinText, str27);
        }
        if ((j & 558345748512L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.knobMinVal, str12);
        }
        if ((j & 2251808403750912L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.knobType, data18);
        }
        if ((j & 283467841552L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView10, i6);
        }
        if ((j & 146028888072L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.mboundView11, str10);
        }
        if ((j & 72057602632056832L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView17, data28);
        }
        if ((j & 18014408174206976L) != 0 || (j2 & 264) != 0) {
            TextViewBindingAdapter.setText(this.mboundView20, str26);
        }
        if ((j & 4611686027285757952L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView23, data7);
        }
        if ((j & 288230384758423552L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView27, i9);
        }
        if ((j & (-9223372027727970304L)) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView28, data23);
        }
        if ((j & 281483566661632L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView33, i5);
        }
        if (j6 != 0 || (j2 & 272) != 0) {
            TextViewBindingAdapter.setText(this.mboundView34, str29);
        }
        if ((j & 1152921513263890432L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.mboundView35, str5);
        }
        if (j6 != 0 || (j2 & 320) != 0) {
            TextViewBindingAdapter.setText(this.mboundView37, str30);
        }
        if (j6 != 0 || (j2 & 384) != 0) {
            TextViewBindingAdapter.setText(this.mboundView39, psrValue2Text);
        }
        if ((j & 2207613190272L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView9, i7);
        }
        if ((j & 18014407100465152L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.modCCVal, strValueOf14);
        }
        if ((j & 25769803777L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.modMaxVal, str23);
        }
        if ((j & 140746078298112L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.modMinVal, str20);
        }
        if ((j & 42949672962L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.modSpeed, data24);
        }
        if ((j & 4503608217567232L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.octive, i);
        }
        if ((j & 144115196674179072L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.pitchSpeed, i2);
        }
        if ((j & 17596481012736L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.port, data);
        }
        if ((j & 1152921514337632256L) != 0 || (j2 & 288) != 0) {
            TextViewBindingAdapter.setText(this.psrCCStr, str25);
        }
        if ((j & 36028805610995712L) != 0 || r8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.psrMode, data12);
        }
        if ((j & 1108101562432L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.psrValue1, str9);
        }
        if ((j & 1125908496842752L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.psrValue2, str8);
        }
        if ((j & 2305843017937846272L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.sustainCCVal, strValueOf11);
        }
        if ((j & 4406636445952L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.sustainDownVal, str17);
        }
        if ((j & 9007207845199872L) != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.sustainUpVal, str19);
        }
        if (i10 != 0 || r8 != 0) {
            TextViewBindingAdapter.setText(this.sysExValue, sysEx);
        }
        if ((j & 576460760926912512L) == 0 && r8 == 0) {
            return;
        }
        AdapterViewBindingAdapter.setSelectedItemPosition(this.transpose, data5);
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        Smk25MiniKeyPara keyPara;
        Smk25MiniKeyPara keyPara2;
        Smk25MiniKeyPara keyPara3;
        EncoderPara encoderPara;
        EncoderPara encoderPara2;
        Smk25MiniMod modPara;
        Smk25MiniMod modPara2;
        Smk25MiniPitch pitchPara;
        Smk25MiniPitch pitchPara2;
        PedalPara pedalPara;
        PedalPara pedalPara2;
        BtnPara btnPara;
        switch (i) {
            case 2:
                Smk25MiniUsrFragment smk25MiniUsrFragment = this.mListener;
                Smk25MiniUsr smk25MiniUsr = this.mData;
                if (smk25MiniUsrFragment != null && smk25MiniUsr != null && (keyPara = smk25MiniUsr.getKeyPara()) != null) {
                    smk25MiniUsrFragment.U8DataChange(i2, keyPara.getCh());
                    break;
                }
                break;
            case 3:
                Smk25MiniUsrFragment smk25MiniUsrFragment2 = this.mListener;
                Smk25MiniGlob smk25MiniGlob = this.mGlobData;
                if (smk25MiniUsrFragment2 != null && smk25MiniGlob != null) {
                    smk25MiniUsrFragment2.GlobU8DataChange(i2, smk25MiniGlob.getKeyCurve());
                    break;
                }
                break;
            case 4:
                Smk25MiniUsrFragment smk25MiniUsrFragment3 = this.mListener;
                Smk25MiniUsr smk25MiniUsr2 = this.mData;
                if (smk25MiniUsrFragment3 != null && smk25MiniUsr2 != null && (keyPara2 = smk25MiniUsr2.getKeyPara()) != null) {
                    smk25MiniUsrFragment3.I8DataChange(i2 - 3, keyPara2.getOctave());
                    break;
                }
                break;
            case 5:
                Smk25MiniUsrFragment smk25MiniUsrFragment4 = this.mListener;
                Smk25MiniUsr smk25MiniUsr3 = this.mData;
                if (smk25MiniUsrFragment4 != null && smk25MiniUsr3 != null && (keyPara3 = smk25MiniUsr3.getKeyPara()) != null) {
                    smk25MiniUsrFragment4.I8DataChange(i2 - 12, keyPara3.getTranspose());
                    break;
                }
                break;
            case 8:
                Smk25MiniUsrFragment smk25MiniUsrFragment5 = this.mListener;
                if (smk25MiniUsrFragment5 != null) {
                    smk25MiniUsrFragment5.knobTypeChange(i2);
                }
                break;
            case 9:
                Smk25MiniUsrFragment smk25MiniUsrFragment6 = this.mListener;
                Smk25MiniUsr smk25MiniUsr4 = this.mData;
                if (smk25MiniUsrFragment6 != null && smk25MiniUsr4 != null && (encoderPara = smk25MiniUsr4.getEncoderPara()) != null) {
                    smk25MiniUsrFragment6.U8DataChange(i2, encoderPara.getCurve());
                    break;
                }
                break;
            case 10:
                Smk25MiniUsrFragment smk25MiniUsrFragment7 = this.mListener;
                Smk25MiniUsr smk25MiniUsr5 = this.mData;
                if (smk25MiniUsrFragment7 != null && smk25MiniUsr5 != null && (encoderPara2 = smk25MiniUsr5.getEncoderPara()) != null) {
                    smk25MiniUsrFragment7.U8DataChange(i2, encoderPara2.getCh());
                    break;
                }
                break;
            case 14:
                Smk25MiniUsrFragment smk25MiniUsrFragment8 = this.mListener;
                Smk25MiniUsr smk25MiniUsr6 = this.mData;
                if (smk25MiniUsrFragment8 != null && smk25MiniUsr6 != null && (modPara = smk25MiniUsr6.getModPara()) != null) {
                    smk25MiniUsrFragment8.U8DataChange(i2, modPara.getCh());
                    break;
                }
                break;
            case 15:
                Smk25MiniUsrFragment smk25MiniUsrFragment9 = this.mListener;
                Smk25MiniUsr smk25MiniUsr7 = this.mData;
                if (smk25MiniUsrFragment9 != null && smk25MiniUsr7 != null && (modPara2 = smk25MiniUsr7.getModPara()) != null) {
                    smk25MiniUsrFragment9.U8DataChange(i2, modPara2.getSpeed());
                    break;
                }
                break;
            case 19:
                Smk25MiniUsrFragment smk25MiniUsrFragment10 = this.mListener;
                Smk25MiniUsr smk25MiniUsr8 = this.mData;
                if (smk25MiniUsrFragment10 != null && smk25MiniUsr8 != null && (pitchPara = smk25MiniUsr8.getPitchPara()) != null) {
                    smk25MiniUsrFragment10.U8DataChange(i2, pitchPara.getCh());
                    break;
                }
                break;
            case 20:
                Smk25MiniUsrFragment smk25MiniUsrFragment11 = this.mListener;
                Smk25MiniUsr smk25MiniUsr9 = this.mData;
                if (smk25MiniUsrFragment11 != null && smk25MiniUsr9 != null && (pitchPara2 = smk25MiniUsr9.getPitchPara()) != null) {
                    smk25MiniUsrFragment11.U8DataChange(i2, pitchPara2.getSpeed());
                    break;
                }
                break;
            case 22:
                Smk25MiniUsrFragment smk25MiniUsrFragment12 = this.mListener;
                Smk25MiniGlob smk25MiniGlob2 = this.mGlobData;
                if (smk25MiniUsrFragment12 != null && smk25MiniGlob2 != null) {
                    smk25MiniUsrFragment12.pedalPortChange(i2, smk25MiniGlob2.getInputMode());
                    break;
                }
                break;
            case 23:
                Smk25MiniUsrFragment smk25MiniUsrFragment13 = this.mListener;
                Smk25MiniUsr smk25MiniUsr10 = this.mData;
                if (smk25MiniUsrFragment13 != null && smk25MiniUsr10 != null && (pedalPara = smk25MiniUsr10.getPedalPara()) != null) {
                    smk25MiniUsrFragment13.U8DataChange(i2, pedalPara.getType());
                    break;
                }
                break;
            case 24:
                Smk25MiniUsrFragment smk25MiniUsrFragment14 = this.mListener;
                Smk25MiniUsr smk25MiniUsr11 = this.mData;
                if (smk25MiniUsrFragment14 != null && smk25MiniUsr11 != null && (pedalPara2 = smk25MiniUsr11.getPedalPara()) != null) {
                    smk25MiniUsrFragment14.U8DataChange(i2, pedalPara2.getCh());
                    break;
                }
                break;
            case 28:
                Smk25MiniUsrFragment smk25MiniUsrFragment15 = this.mListener;
                if (smk25MiniUsrFragment15 != null) {
                    smk25MiniUsrFragment15.psrModeChange(i2);
                }
                break;
            case 29:
                Smk25MiniUsrFragment smk25MiniUsrFragment16 = this.mListener;
                Smk25MiniUsr smk25MiniUsr12 = this.mData;
                if (smk25MiniUsrFragment16 != null && smk25MiniUsr12 != null && (btnPara = smk25MiniUsr12.getBtnPara()) != null) {
                    smk25MiniUsrFragment16.U8DataChange(i2, btnPara.getCh());
                    break;
                }
                break;
        }
    }

    @Override // com.cubeSuite.generated.callback.AfterTextChanged.Listener
    public final void _internalCallbackAfterTextChanged(int i, Editable editable) {
        Smk25MiniKeyPara keyPara;
        Smk25MiniKeyPara keyPara2;
        EncoderPara encoderPara;
        AddrU8[] dat;
        EncoderPara encoderPara2;
        AddrU8[] dat2;
        EncoderPara encoderPara3;
        AddrU8[] dat3;
        Smk25MiniMod modPara;
        AddrU8[] dat4;
        Smk25MiniMod modPara2;
        AddrU8[] dat5;
        Smk25MiniMod modPara3;
        AddrU8[] dat6;
        PedalPara pedalPara;
        AddrU8[] dat7;
        PedalPara pedalPara2;
        AddrU8[] dat8;
        PedalPara pedalPara3;
        AddrU8[] dat9;
        BtnPara btnPara;
        AddrU8[] dat10;
        BtnPara btnPara2;
        AddrU8[] dat11;
        BtnPara btnPara3;
        AddrU8[] dat12;
        BtnPara btnPara4;
        if (i == 6) {
            Smk25MiniUsrFragment smk25MiniUsrFragment = this.mListener;
            Smk25MiniUsr smk25MiniUsr = this.mData;
            if (smk25MiniUsrFragment == null || smk25MiniUsr == null || (keyPara = smk25MiniUsr.getKeyPara()) == null) {
                return;
            }
            smk25MiniUsrFragment.setNum(editable, keyPara.getMin(), 0, 127);
            return;
        }
        if (i != 7) {
            switch (i) {
                case 11:
                    Smk25MiniUsrFragment smk25MiniUsrFragment2 = this.mListener;
                    Smk25MiniUsr smk25MiniUsr2 = this.mData;
                    if (smk25MiniUsrFragment2 != null && smk25MiniUsr2 != null && (encoderPara = smk25MiniUsr2.getEncoderPara()) != null && (dat = encoderPara.getDat()) != null) {
                        smk25MiniUsrFragment2.setNum(editable, (AddrU8) getFromArray(dat, 0), 0, 127);
                        break;
                    }
                    break;
                case 12:
                    Smk25MiniUsrFragment smk25MiniUsrFragment3 = this.mListener;
                    Smk25MiniUsr smk25MiniUsr3 = this.mData;
                    if (smk25MiniUsrFragment3 != null && smk25MiniUsr3 != null && (encoderPara2 = smk25MiniUsr3.getEncoderPara()) != null && (dat2 = encoderPara2.getDat()) != null) {
                        smk25MiniUsrFragment3.setNum(editable, (AddrU8) getFromArray(dat2, 1), 0, 127);
                        break;
                    }
                    break;
                case 13:
                    Smk25MiniUsrFragment smk25MiniUsrFragment4 = this.mListener;
                    Smk25MiniUsr smk25MiniUsr4 = this.mData;
                    if (smk25MiniUsrFragment4 != null && smk25MiniUsr4 != null && (encoderPara3 = smk25MiniUsr4.getEncoderPara()) != null && (dat3 = encoderPara3.getDat()) != null) {
                        smk25MiniUsrFragment4.setNum(editable, (AddrU8) getFromArray(dat3, 2), 0, 127);
                        break;
                    }
                    break;
                default:
                    switch (i) {
                        case 16:
                            Smk25MiniUsrFragment smk25MiniUsrFragment5 = this.mListener;
                            Smk25MiniUsr smk25MiniUsr5 = this.mData;
                            if (smk25MiniUsrFragment5 != null && smk25MiniUsr5 != null && (modPara = smk25MiniUsr5.getModPara()) != null && (dat4 = modPara.getDat()) != null) {
                                smk25MiniUsrFragment5.setNum(editable, (AddrU8) getFromArray(dat4, 0), 0, 127);
                                break;
                            }
                            break;
                        case 17:
                            Smk25MiniUsrFragment smk25MiniUsrFragment6 = this.mListener;
                            Smk25MiniUsr smk25MiniUsr6 = this.mData;
                            if (smk25MiniUsrFragment6 != null && smk25MiniUsr6 != null && (modPara2 = smk25MiniUsr6.getModPara()) != null && (dat5 = modPara2.getDat()) != null) {
                                smk25MiniUsrFragment6.setNum(editable, (AddrU8) getFromArray(dat5, 1), 0, 127);
                                break;
                            }
                            break;
                        case 18:
                            Smk25MiniUsrFragment smk25MiniUsrFragment7 = this.mListener;
                            Smk25MiniUsr smk25MiniUsr7 = this.mData;
                            if (smk25MiniUsrFragment7 != null && smk25MiniUsr7 != null && (modPara3 = smk25MiniUsr7.getModPara()) != null && (dat6 = modPara3.getDat()) != null) {
                                smk25MiniUsrFragment7.setNum(editable, (AddrU8) getFromArray(dat6, 2), 0, 127);
                                break;
                            }
                            break;
                        default:
                            switch (i) {
                                case 25:
                                    Smk25MiniUsrFragment smk25MiniUsrFragment8 = this.mListener;
                                    Smk25MiniUsr smk25MiniUsr8 = this.mData;
                                    if (smk25MiniUsrFragment8 != null && smk25MiniUsr8 != null && (pedalPara = smk25MiniUsr8.getPedalPara()) != null && (dat7 = pedalPara.getDat()) != null) {
                                        smk25MiniUsrFragment8.setNum(editable, (AddrU8) getFromArray(dat7, 0), 0, 127);
                                        break;
                                    }
                                    break;
                                case 26:
                                    Smk25MiniUsrFragment smk25MiniUsrFragment9 = this.mListener;
                                    Smk25MiniUsr smk25MiniUsr9 = this.mData;
                                    if (smk25MiniUsrFragment9 != null && smk25MiniUsr9 != null && (pedalPara2 = smk25MiniUsr9.getPedalPara()) != null && (dat8 = pedalPara2.getDat()) != null) {
                                        smk25MiniUsrFragment9.setNum(editable, (AddrU8) getFromArray(dat8, 1), 0, 127);
                                        break;
                                    }
                                    break;
                                case 27:
                                    Smk25MiniUsrFragment smk25MiniUsrFragment10 = this.mListener;
                                    Smk25MiniUsr smk25MiniUsr10 = this.mData;
                                    if (smk25MiniUsrFragment10 != null && smk25MiniUsr10 != null && (pedalPara3 = smk25MiniUsr10.getPedalPara()) != null && (dat9 = pedalPara3.getDat()) != null) {
                                        smk25MiniUsrFragment10.setNum(editable, (AddrU8) getFromArray(dat9, 2), 0, 127);
                                        break;
                                    }
                                    break;
                                default:
                                    switch (i) {
                                        case 30:
                                            Smk25MiniUsrFragment smk25MiniUsrFragment11 = this.mListener;
                                            Smk25MiniUsr smk25MiniUsr11 = this.mData;
                                            if (smk25MiniUsrFragment11 != null && smk25MiniUsr11 != null && (btnPara = smk25MiniUsr11.getBtnPara()) != null && (dat10 = btnPara.getDat()) != null) {
                                                smk25MiniUsrFragment11.setNum(editable, (AddrU8) getFromArray(dat10, 0), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 31:
                                            Smk25MiniUsrFragment smk25MiniUsrFragment12 = this.mListener;
                                            Smk25MiniUsr smk25MiniUsr12 = this.mData;
                                            if (smk25MiniUsrFragment12 != null && smk25MiniUsr12 != null && (btnPara2 = smk25MiniUsr12.getBtnPara()) != null && (dat11 = btnPara2.getDat()) != null) {
                                                smk25MiniUsrFragment12.setNum(editable, (AddrU8) getFromArray(dat11, 1), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 32:
                                            Smk25MiniUsrFragment smk25MiniUsrFragment13 = this.mListener;
                                            Smk25MiniUsr smk25MiniUsr13 = this.mData;
                                            if (smk25MiniUsrFragment13 != null && smk25MiniUsr13 != null && (btnPara3 = smk25MiniUsr13.getBtnPara()) != null && (dat12 = btnPara3.getDat()) != null) {
                                                smk25MiniUsrFragment13.setNum(editable, (AddrU8) getFromArray(dat12, 2), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 33:
                                            Smk25MiniUsrFragment smk25MiniUsrFragment14 = this.mListener;
                                            Smk25MiniUsr smk25MiniUsr14 = this.mData;
                                            if (smk25MiniUsrFragment14 != null && smk25MiniUsr14 != null && (btnPara4 = smk25MiniUsr14.getBtnPara()) != null) {
                                                smk25MiniUsrFragment14.setSysEx(editable, btnPara4.getSys());
                                                break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
            return;
        }
        Smk25MiniUsrFragment smk25MiniUsrFragment15 = this.mListener;
        Smk25MiniUsr smk25MiniUsr15 = this.mData;
        if (smk25MiniUsrFragment15 == null || smk25MiniUsr15 == null || (keyPara2 = smk25MiniUsr15.getKeyPara()) == null) {
            return;
        }
        smk25MiniUsrFragment15.setNum(editable, keyPara2.getMax(), 0, 127);
    }

    @Override // com.cubeSuite.generated.callback.OnCheckedChangeListener.Listener
    public final void _internalCallbackOnCheckedChanged(int i, RadioGroup radioGroup, int i2) {
        Smk25MiniUsrFragment smk25MiniUsrFragment = this.mListener;
        if (smk25MiniUsrFragment != null) {
            smk25MiniUsrFragment.itemSelectChange(radioGroup);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnClickListener.Listener
    public final void _internalCallbackOnClick(int i, View view) {
        Smk25MiniUsrFragment smk25MiniUsrFragment = this.mListener;
        if (smk25MiniUsrFragment != null) {
            smk25MiniUsrFragment.setSustainPedalMode();
        }
    }
}
