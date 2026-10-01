package com.cubeSuite.databinding;

import android.text.Editable;
import android.util.SparseIntArray;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.AdapterViewBindingAdapter;
import androidx.databinding.adapters.RadioGroupBindingAdapter;
import androidx.databinding.adapters.SeekBarBindingAdapter;
import androidx.databinding.adapters.TextViewBindingAdapter;
import com.cubeSuite.R;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.EncoderPara;
import com.cubeSuite.entity.midikeyboard.KeyPara;
import com.cubeSuite.entity.midikeyboard.ModPara;
import com.cubeSuite.entity.midikeyboard.PedalPara;
import com.cubeSuite.entity.midikeyboard.PitchPara;
import com.cubeSuite.entity.midikeyboard.Sys;
import com.cubeSuite.entity.smk25II.Smk25IIPadPara;
import com.cubeSuite.entity.smk25II.Smk25IIUsr;
import com.cubeSuite.entity.smk25II.Smk25IIUsrBinding;
import com.cubeSuite.fragment.smk25II.Smk25IIUsrFragment;
import com.cubeSuite.generated.callback.AfterTextChanged;
import com.cubeSuite.generated.callback.OnCheckedChangeListener;
import com.cubeSuite.generated.callback.OnClickListener;
import com.cubeSuite.generated.callback.OnItemSelected;
import com.cubeSuite.generated.callback.OnProgressChanged;
import kotlinx.coroutines.internal.LockFreeTaskQueueCore;
import okhttp3.internal.http2.Http2Stream;
import okio.SegmentPool;

/* JADX INFO: loaded from: classes.dex */
public class Smk25iiUsrFBindingImpl extends Smk25iiUsrFBinding implements AfterTextChanged.Listener, OnItemSelected.Listener, OnClickListener.Listener, OnProgressChanged.Listener, OnCheckedChangeListener.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private final TextViewBindingAdapter.AfterTextChanged mCallback10;
    private final TextViewBindingAdapter.AfterTextChanged mCallback11;
    private final TextViewBindingAdapter.AfterTextChanged mCallback12;
    private final View.OnClickListener mCallback13;
    private final TextViewBindingAdapter.AfterTextChanged mCallback14;
    private final TextViewBindingAdapter.AfterTextChanged mCallback15;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback16;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback17;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback18;
    private final TextViewBindingAdapter.AfterTextChanged mCallback19;
    private final TextViewBindingAdapter.AfterTextChanged mCallback20;
    private final SeekBarBindingAdapter.OnProgressChanged mCallback21;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback22;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback23;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback24;
    private final TextViewBindingAdapter.AfterTextChanged mCallback25;
    private final TextViewBindingAdapter.AfterTextChanged mCallback26;
    private final TextViewBindingAdapter.AfterTextChanged mCallback27;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback28;
    private final TextViewBindingAdapter.AfterTextChanged mCallback29;
    private final TextViewBindingAdapter.AfterTextChanged mCallback30;
    private final TextViewBindingAdapter.AfterTextChanged mCallback31;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback32;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback33;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback34;
    private final TextViewBindingAdapter.AfterTextChanged mCallback35;
    private final TextViewBindingAdapter.AfterTextChanged mCallback36;
    private final TextViewBindingAdapter.AfterTextChanged mCallback37;
    private final RadioGroup.OnCheckedChangeListener mCallback6;
    private final SeekBarBindingAdapter.OnProgressChanged mCallback7;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback8;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback9;
    private long mDirtyFlags;
    private long mDirtyFlags_1;
    private final ScrollView mboundView0;
    private final TextView mboundView2;
    private final TextView mboundView21;
    private final AppCompatSpinner mboundView24;
    private final AppCompatSpinner mboundView25;
    private final EditText mboundView26;
    private final AppCompatSpinner mboundView32;
    private final TextView mboundView34;
    private final AppCompatSpinner mboundView37;
    private final AppCompatSpinner mboundView38;
    private final AppCompatSpinner mboundView39;

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.padBtn, 43);
        sparseIntArray.put(R.id.pads, 44);
        sparseIntArray.put(R.id.channelLayout, 45);
        sparseIntArray.put(R.id.padNoteLayout, 46);
        sparseIntArray.put(R.id.minValueLayout, 47);
        sparseIntArray.put(R.id.maxValueLayout, 48);
        sparseIntArray.put(R.id.ledText, 49);
        sparseIntArray.put(R.id.sysExLayout, 50);
        sparseIntArray.put(R.id.keybed, 51);
        sparseIntArray.put(R.id.knobs, 52);
        sparseIntArray.put(R.id.knobCCLayout, 53);
        sparseIntArray.put(R.id.mps, 54);
    }

    public Smk25iiUsrFBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 55, sIncludes, sViewsWithIds));
    }

    private Smk25iiUsrFBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 28, (AppCompatSpinner) objArr[5], (LinearLayout) objArr[45], (LinearLayout) objArr[51], (AppCompatSpinner) objArr[16], (EditText) objArr[20], (EditText) objArr[19], (LinearLayout) objArr[53], (TextView) objArr[27], (TextView) objArr[30], (EditText) objArr[31], (TextView) objArr[28], (EditText) objArr[29], (SeekBar) objArr[22], (AppCompatSpinner) objArr[23], (LinearLayout) objArr[52], (TextView) objArr[49], (EditText) objArr[14], (LinearLayout) objArr[48], (EditText) objArr[12], (LinearLayout) objArr[47], (EditText) objArr[10], (EditText) objArr[33], (EditText) objArr[36], (EditText) objArr[35], (LinearLayout) objArr[54], (AppCompatSpinner) objArr[17], (RadioButton) objArr[43], (TextView) objArr[11], (TextView) objArr[9], (EditText) objArr[7], (LinearLayout) objArr[46], (TextView) objArr[8], (TextView) objArr[6], (SeekBar) objArr[3], (LinearLayout) objArr[44], (Button) objArr[13], (EditText) objArr[40], (EditText) objArr[42], (EditText) objArr[41], (LinearLayout) objArr[50], (EditText) objArr[15], (AppCompatSpinner) objArr[18], (AppCompatSpinner) objArr[4], (RadioGroup) objArr[1]);
        this.mDirtyFlags = -1L;
        this.mDirtyFlags_1 = -1L;
        this.channel.setTag(null);
        this.keybedChannel.setTag(null);
        this.keybedMaxVel.setTag(null);
        this.keybedMinVel.setTag(null);
        this.knobCCStr.setTag(null);
        this.knobMaxText.setTag(null);
        this.knobMaxVal.setTag(null);
        this.knobMinText.setTag(null);
        this.knobMinVal.setTag(null);
        this.knobSeekbar.setTag(null);
        this.knobType.setTag(null);
        this.ledVel.setTag(null);
        this.maxVel.setTag(null);
        ScrollView scrollView = (ScrollView) objArr[0];
        this.mboundView0 = scrollView;
        scrollView.setTag(null);
        TextView textView = (TextView) objArr[2];
        this.mboundView2 = textView;
        textView.setTag(null);
        TextView textView2 = (TextView) objArr[21];
        this.mboundView21 = textView2;
        textView2.setTag(null);
        AppCompatSpinner appCompatSpinner = (AppCompatSpinner) objArr[24];
        this.mboundView24 = appCompatSpinner;
        appCompatSpinner.setTag(null);
        AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) objArr[25];
        this.mboundView25 = appCompatSpinner2;
        appCompatSpinner2.setTag(null);
        EditText editText = (EditText) objArr[26];
        this.mboundView26 = editText;
        editText.setTag(null);
        AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) objArr[32];
        this.mboundView32 = appCompatSpinner3;
        appCompatSpinner3.setTag(null);
        TextView textView3 = (TextView) objArr[34];
        this.mboundView34 = textView3;
        textView3.setTag(null);
        AppCompatSpinner appCompatSpinner4 = (AppCompatSpinner) objArr[37];
        this.mboundView37 = appCompatSpinner4;
        appCompatSpinner4.setTag(null);
        AppCompatSpinner appCompatSpinner5 = (AppCompatSpinner) objArr[38];
        this.mboundView38 = appCompatSpinner5;
        appCompatSpinner5.setTag(null);
        AppCompatSpinner appCompatSpinner6 = (AppCompatSpinner) objArr[39];
        this.mboundView39 = appCompatSpinner6;
        appCompatSpinner6.setTag(null);
        this.minVel.setTag(null);
        this.modCCVal.setTag(null);
        this.modMaxVal.setTag(null);
        this.modMinVal.setTag(null);
        this.octive.setTag(null);
        this.padMaxText.setTag(null);
        this.padMinText.setTag(null);
        this.padNote.setTag(null);
        this.padNoteStr.setTag(null);
        this.padNoteText.setTag(null);
        this.padSeekbar.setTag(null);
        this.selectColorBtn.setTag(null);
        this.sustainCCVal.setTag(null);
        this.sustainDownVal.setTag(null);
        this.sustainUpVal.setTag(null);
        this.sysExValue.setTag(null);
        this.transpose.setTag(null);
        this.type.setTag(null);
        this.typeRadio.setTag(null);
        setRootTag(view);
        this.mCallback27 = new AfterTextChanged(this, 22);
        this.mCallback15 = new AfterTextChanged(this, 10);
        this.mCallback16 = new OnItemSelected(this, 11);
        this.mCallback28 = new OnItemSelected(this, 23);
        this.mCallback9 = new OnItemSelected(this, 4);
        this.mCallback25 = new AfterTextChanged(this, 20);
        this.mCallback37 = new AfterTextChanged(this, 32);
        this.mCallback13 = new OnClickListener(this, 8);
        this.mCallback8 = new OnItemSelected(this, 3);
        this.mCallback14 = new AfterTextChanged(this, 9);
        this.mCallback26 = new AfterTextChanged(this, 21);
        this.mCallback7 = new OnProgressChanged(this, 2);
        this.mCallback23 = new OnItemSelected(this, 18);
        this.mCallback35 = new AfterTextChanged(this, 30);
        this.mCallback11 = new AfterTextChanged(this, 6);
        this.mCallback36 = new AfterTextChanged(this, 31);
        this.mCallback12 = new AfterTextChanged(this, 7);
        this.mCallback24 = new OnItemSelected(this, 19);
        this.mCallback21 = new OnProgressChanged(this, 16);
        this.mCallback33 = new OnItemSelected(this, 28);
        this.mCallback20 = new AfterTextChanged(this, 15);
        this.mCallback34 = new OnItemSelected(this, 29);
        this.mCallback10 = new AfterTextChanged(this, 5);
        this.mCallback22 = new OnItemSelected(this, 17);
        this.mCallback30 = new AfterTextChanged(this, 25);
        this.mCallback32 = new OnItemSelected(this, 27);
        this.mCallback31 = new AfterTextChanged(this, 26);
        this.mCallback19 = new AfterTextChanged(this, 14);
        this.mCallback6 = new OnCheckedChangeListener(this, 1);
        this.mCallback17 = new OnItemSelected(this, 12);
        this.mCallback29 = new AfterTextChanged(this, 24);
        this.mCallback18 = new OnItemSelected(this, 13);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 0L;
            this.mDirtyFlags_1 = 8L;
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
            setListener((Smk25IIUsrFragment) obj);
            return true;
        }
        if (2 == i) {
            setData((Smk25IIUsr) obj);
            return true;
        }
        if (21 != i) {
            return false;
        }
        setUsrBinding((Smk25IIUsrBinding) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25iiUsrFBinding
    public void setListener(Smk25IIUsrFragment smk25IIUsrFragment) {
        this.mListener = smk25IIUsrFragment;
        synchronized (this) {
            this.mDirtyFlags |= 268435456;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25iiUsrFBinding
    public void setData(Smk25IIUsr smk25IIUsr) {
        this.mData = smk25IIUsr;
        synchronized (this) {
            this.mDirtyFlags |= 536870912;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25iiUsrFBinding
    public void setUsrBinding(Smk25IIUsrBinding smk25IIUsrBinding) {
        updateRegistration(27, smk25IIUsrBinding);
        this.mUsrBinding = smk25IIUsrBinding;
        synchronized (this) {
            this.mDirtyFlags |= 134217728;
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
                return onChangeDataEncoderParaUsrBindingKnobIndexCh((AddrU8) obj, i2);
            case 2:
                return onChangeDataKeyParaMax((AddrU8) obj, i2);
            case 3:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat2((AddrU8) obj, i2);
            case 4:
                return onChangeDataPadParaUsrBindingPadIndexDat2((AddrU8) obj, i2);
            case 5:
                return onChangeDataPadParaUsrBindingPadIndexDat0((AddrU8) obj, i2);
            case 6:
                return onChangeDataPedalParaDat2((AddrU8) obj, i2);
            case 7:
                return onChangeDataKeyParaMin((AddrU8) obj, i2);
            case 8:
                return onChangeDataKeyParaCh((AddrU8) obj, i2);
            case 9:
                return onChangeDataPadParaUsrBindingPadIndexCh((AddrU8) obj, i2);
            case 10:
                return onChangeDataPadParaUsrBindingPadIndexLed((AddrU8) obj, i2);
            case 11:
                return onChangeDataModParaDat1((AddrU8) obj, i2);
            case 12:
                return onChangeDataPadParaUsrBindingPadIndexDat1((AddrU8) obj, i2);
            case 13:
                return onChangeDataEncoderParaUsrBindingKnobIndexType((AddrU8) obj, i2);
            case 14:
                return onChangeDataKeyParaOctave((AddrI8) obj, i2);
            case 15:
                return onChangeDataPadParaUsrBindingPadIndexType((AddrU8) obj, i2);
            case 16:
                return onChangeDataPedalParaDat1((AddrU8) obj, i2);
            case 17:
                return onChangeDataModParaDat0((AddrU8) obj, i2);
            case 18:
                return onChangeDataModParaCh((AddrU8) obj, i2);
            case 19:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat0((AddrU8) obj, i2);
            case 20:
                return onChangeDataEncoderParaUsrBindingKnobIndexCurve((AddrU8) obj, i2);
            case 21:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat1((AddrU8) obj, i2);
            case 22:
                return onChangeDataPedalParaType((AddrU8) obj, i2);
            case 23:
                return onChangeDataKeyParaTranspose((AddrI8) obj, i2);
            case 24:
                return onChangeDataPedalParaDat0((AddrU8) obj, i2);
            case 25:
                return onChangeDataPitchParaCh((AddrU8) obj, i2);
            case 26:
                return onChangeDataPedalParaCh((AddrU8) obj, i2);
            case 27:
                return onChangeUsrBinding((Smk25IIUsrBinding) obj, i2);
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
            this.mDirtyFlags |= 1073741824;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 2147483648L;
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
            this.mDirtyFlags |= 4294967296L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat2(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 8589934592L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexDat2(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 17179869184L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexDat0(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 34359738368L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat2(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 68719476736L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaMin(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 137438953472L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 274877906944L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 549755813888L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexLed(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 1099511627776L;
        }
        return true;
    }

    private boolean onChangeDataModParaDat1(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 2199023255552L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexDat1(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 4398046511104L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexType(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 8796093022208L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaOctave(AddrI8 addrI8, int i) {
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
            this.mDirtyFlags |= 17592186044416L;
        }
        return true;
    }

    private boolean onChangeDataPadParaUsrBindingPadIndexType(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 35184372088832L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat1(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 70368744177664L;
        }
        return true;
    }

    private boolean onChangeDataModParaDat0(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 140737488355328L;
        }
        return true;
    }

    private boolean onChangeDataModParaCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 281474976710656L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat0(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 562949953421312L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexCurve(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 1125899906842624L;
        }
        return true;
    }

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat1(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 2251799813685248L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaType(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 4503599627370496L;
        }
        return true;
    }

    private boolean onChangeDataKeyParaTranspose(AddrI8 addrI8, int i) {
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
            this.mDirtyFlags |= 9007199254740992L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaDat0(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 18014398509481984L;
        }
        return true;
    }

    private boolean onChangeDataPitchParaCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 36028797018963968L;
        }
        return true;
    }

    private boolean onChangeDataPedalParaCh(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 72057594037927936L;
        }
        return true;
    }

    private boolean onChangeUsrBinding(Smk25IIUsrBinding smk25IIUsrBinding, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 134217728;
            }
            return true;
        }
        if (i == 11) {
            synchronized (this) {
                this.mDirtyFlags |= 144115188075855872L;
            }
            return true;
        }
        if (i == 15) {
            synchronized (this) {
                this.mDirtyFlags |= 288230376151711744L;
            }
            return true;
        }
        if (i == 14) {
            synchronized (this) {
                this.mDirtyFlags |= 576460752303423488L;
            }
            return true;
        }
        if (i == 13) {
            synchronized (this) {
                this.mDirtyFlags |= LockFreeTaskQueueCore.FROZEN_MASK;
            }
            return true;
        }
        if (i == 12) {
            synchronized (this) {
                this.mDirtyFlags |= LockFreeTaskQueueCore.CLOSED_MASK;
            }
            return true;
        }
        if (i == 6) {
            synchronized (this) {
                this.mDirtyFlags |= 4611686018427387904L;
            }
            return true;
        }
        if (i == 5) {
            synchronized (this) {
                this.mDirtyFlags |= Long.MIN_VALUE;
            }
            return true;
        }
        if (i == 8) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 1;
            }
            return true;
        }
        if (i == 7) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 2;
            }
            return true;
        }
        if (i != 10) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags_1 |= 4;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:101:0x01a2  */
    /* JADX WARN: Code duplicated, block: B:144:0x0245 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:145:0x0247  */
    /* JADX WARN: Code duplicated, block: B:146:0x024c  */
    /* JADX WARN: Code duplicated, block: B:149:0x0259  */
    /* JADX WARN: Code duplicated, block: B:153:0x0267  */
    /* JADX WARN: Code duplicated, block: B:155:0x026b  */
    /* JADX WARN: Code duplicated, block: B:156:0x0275  */
    /* JADX WARN: Code duplicated, block: B:159:0x027f  */
    /* JADX WARN: Code duplicated, block: B:160:0x0284  */
    /* JADX WARN: Code duplicated, block: B:164:0x0294  */
    /* JADX WARN: Code duplicated, block: B:168:0x02a0 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:169:0x02a2  */
    /* JADX WARN: Code duplicated, block: B:170:0x02ac  */
    /* JADX WARN: Code duplicated, block: B:173:0x02b7  */
    /* JADX WARN: Code duplicated, block: B:174:0x02bc  */
    /* JADX WARN: Code duplicated, block: B:178:0x02cc  */
    /* JADX WARN: Code duplicated, block: B:183:0x02d9 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:184:0x02db  */
    /* JADX WARN: Code duplicated, block: B:185:0x02e3  */
    /* JADX WARN: Code duplicated, block: B:188:0x02ec  */
    /* JADX WARN: Code duplicated, block: B:189:0x02f1  */
    /* JADX WARN: Code duplicated, block: B:197:0x0310 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:198:0x0312  */
    /* JADX WARN: Code duplicated, block: B:199:0x031d  */
    /* JADX WARN: Code duplicated, block: B:202:0x032a  */
    /* JADX WARN: Code duplicated, block: B:203:0x0336 A[PHI: r43 r48
  0x0336: PHI (r43v10 java.lang.String) = (r2v24 java.lang.String), (r43v12 java.lang.String) binds: [B:201:0x0328, B:196:0x030b] A[DONT_GENERATE, DONT_INLINE]
  0x0336: PHI (r48v13 java.lang.String) = (r48v11 java.lang.String), (r48v16 java.lang.String) binds: [B:201:0x0328, B:196:0x030b] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:279:0x0486 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:280:0x0488  */
    /* JADX WARN: Code duplicated, block: B:281:0x048d  */
    /* JADX WARN: Code duplicated, block: B:284:0x0498  */
    /* JADX WARN: Code duplicated, block: B:285:0x04c1 A[PHI: r52
  0x04c1: PHI (r52v9 java.lang.String) = (r52v8 java.lang.String), (r52v12 java.lang.String) binds: [B:283:0x0496, B:278:0x0483] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:364:0x064b A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:365:0x064d  */
    /* JADX WARN: Code duplicated, block: B:366:0x0652  */
    /* JADX WARN: Code duplicated, block: B:369:0x065d  */
    /* JADX WARN: Code duplicated, block: B:370:0x0662 A[PHI: r73
  0x0662: PHI (r73v4 int) = (r73v2 int), (r73v6 int) binds: [B:368:0x065b, B:363:0x0648] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:373:0x066e  */
    /* JADX WARN: Code duplicated, block: B:377:0x0680 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:378:0x0682  */
    /* JADX WARN: Code duplicated, block: B:379:0x0687  */
    /* JADX WARN: Code duplicated, block: B:382:0x0694  */
    /* JADX WARN: Code duplicated, block: B:386:0x06a2  */
    /* JADX WARN: Code duplicated, block: B:388:0x06a6  */
    /* JADX WARN: Code duplicated, block: B:389:0x06b0  */
    /* JADX WARN: Code duplicated, block: B:392:0x06ba  */
    /* JADX WARN: Code duplicated, block: B:393:0x06bf  */
    /* JADX WARN: Code duplicated, block: B:396:0x06c6  */
    /* JADX WARN: Code duplicated, block: B:400:0x06d3 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:401:0x06d5  */
    /* JADX WARN: Code duplicated, block: B:402:0x06df  */
    /* JADX WARN: Code duplicated, block: B:405:0x06ea  */
    /* JADX WARN: Code duplicated, block: B:406:0x06ef  */
    /* JADX WARN: Code duplicated, block: B:409:0x06fb  */
    /* JADX WARN: Code duplicated, block: B:413:0x0705  */
    /* JADX WARN: Code duplicated, block: B:415:0x070b  */
    /* JADX WARN: Code duplicated, block: B:416:0x0712  */
    /* JADX WARN: Code duplicated, block: B:419:0x071f  */
    /* JADX WARN: Code duplicated, block: B:424:0x072d A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:425:0x072f  */
    /* JADX WARN: Code duplicated, block: B:426:0x0737  */
    /* JADX WARN: Code duplicated, block: B:429:0x0740  */
    /* JADX WARN: Code duplicated, block: B:430:0x0745  */
    /* JADX WARN: Code duplicated, block: B:434:0x0758  */
    /* JADX WARN: Code duplicated, block: B:438:0x0764 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:439:0x0766  */
    /* JADX WARN: Code duplicated, block: B:440:0x0771  */
    /* JADX WARN: Code duplicated, block: B:443:0x077e  */
    /* JADX WARN: Code duplicated, block: B:444:0x078e A[PHI: r69 r72
  0x078e: PHI (r69v13 java.lang.String) = (r2v88 java.lang.String), (r69v16 java.lang.String) binds: [B:442:0x077c, B:437:0x075f] A[DONT_GENERATE, DONT_INLINE]
  0x078e: PHI (r72v8 java.lang.String) = (r72v6 java.lang.String), (r72v9 java.lang.String) binds: [B:442:0x077c, B:437:0x075f] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:481:0x0828 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:482:0x082a  */
    /* JADX WARN: Code duplicated, block: B:483:0x082f  */
    /* JADX WARN: Code duplicated, block: B:486:0x083a  */
    /* JADX WARN: Code duplicated, block: B:487:0x083f A[PHI: r76
  0x083f: PHI (r76v15 java.lang.String) = (r76v13 java.lang.String), (r76v17 java.lang.String) binds: [B:485:0x0838, B:480:0x0825] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:490:0x084b  */
    /* JADX WARN: Code duplicated, block: B:494:0x0857 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:495:0x0859  */
    /* JADX WARN: Code duplicated, block: B:496:0x085e  */
    /* JADX WARN: Code duplicated, block: B:499:0x0869  */
    /* JADX WARN: Code duplicated, block: B:500:0x086e  */
    /* JADX WARN: Code duplicated, block: B:504:0x087e  */
    /* JADX WARN: Code duplicated, block: B:508:0x0891 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:509:0x0893  */
    /* JADX WARN: Code duplicated, block: B:510:0x0898  */
    /* JADX WARN: Code duplicated, block: B:513:0x08a5  */
    /* JADX WARN: Code duplicated, block: B:517:0x08b3  */
    /* JADX WARN: Code duplicated, block: B:519:0x08b7  */
    /* JADX WARN: Code duplicated, block: B:520:0x08bf  */
    /* JADX WARN: Code duplicated, block: B:523:0x08c9  */
    /* JADX WARN: Code duplicated, block: B:524:0x08ce  */
    /* JADX WARN: Code duplicated, block: B:528:0x08d9  */
    /* JADX WARN: Code duplicated, block: B:532:0x08e6 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:533:0x08e8  */
    /* JADX WARN: Code duplicated, block: B:534:0x08f2  */
    /* JADX WARN: Code duplicated, block: B:537:0x08fc  */
    /* JADX WARN: Code duplicated, block: B:538:0x0901  */
    /* JADX WARN: Code duplicated, block: B:541:0x090d  */
    /* JADX WARN: Code duplicated, block: B:545:0x0917  */
    /* JADX WARN: Code duplicated, block: B:548:0x0926  */
    /* JADX WARN: Code duplicated, block: B:553:0x0934  */
    /* JADX WARN: Code duplicated, block: B:555:0x0938  */
    /* JADX WARN: Code duplicated, block: B:556:0x0940  */
    /* JADX WARN: Code duplicated, block: B:559:0x0949  */
    /* JADX WARN: Code duplicated, block: B:560:0x094e  */
    /* JADX WARN: Code duplicated, block: B:564:0x0960  */
    /* JADX WARN: Code duplicated, block: B:568:0x096a A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:569:0x096c  */
    /* JADX WARN: Code duplicated, block: B:570:0x0971  */
    /* JADX WARN: Code duplicated, block: B:573:0x097c  */
    /* JADX WARN: Code duplicated, block: B:574:0x0987 A[PHI: r40
  0x0987: PHI (r40v4 java.lang.String) = (r40v3 java.lang.String), (r40v6 java.lang.String) binds: [B:572:0x097a, B:567:0x0967] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:591:0x09c5 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:592:0x09c7  */
    /* JADX WARN: Code duplicated, block: B:593:0x09cc  */
    /* JADX WARN: Code duplicated, block: B:595:0x09d0  */
    /* JADX WARN: Code duplicated, block: B:596:0x09d7  */
    /* JADX WARN: Code duplicated, block: B:59:0x0124  */
    /* JADX WARN: Instruction removed from duplicated block: B:370:0x0662, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:487:0x083f, please report this as an issue */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r0v113 */
    /* JADX WARN: Type inference failed for: r0v114 */
    /* JADX WARN: Type inference failed for: r0v13 */
    /* JADX WARN: Type inference failed for: r0v14, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v28 */
    /* JADX WARN: Type inference failed for: r0v29, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v50 */
    /* JADX WARN: Type inference failed for: r0v51, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r0v54 */
    /* JADX WARN: Type inference failed for: r11v29 */
    /* JADX WARN: Type inference failed for: r11v6 */
    /* JADX WARN: Type inference failed for: r11v7, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrI8] */
    /* JADX WARN: Type inference failed for: r12v12 */
    /* JADX WARN: Type inference failed for: r12v13, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r12v61 */
    /* JADX WARN: Type inference failed for: r13v13, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r13v14 */
    /* JADX WARN: Type inference failed for: r13v15, types: [com.cubeSuite.entity.smk25II.Smk25IIPadPara] */
    /* JADX WARN: Type inference failed for: r13v16 */
    /* JADX WARN: Type inference failed for: r13v17, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r13v35 */
    /* JADX WARN: Type inference failed for: r13v36 */
    /* JADX WARN: Type inference failed for: r13v37 */
    /* JADX WARN: Type inference failed for: r13v8 */
    /* JADX WARN: Type inference failed for: r15v10, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrI8] */
    /* JADX WARN: Type inference failed for: r15v19 */
    /* JADX WARN: Type inference failed for: r15v20, types: [com.cubeSuite.entity.midikeyboard.PitchPara] */
    /* JADX WARN: Type inference failed for: r15v21 */
    /* JADX WARN: Type inference failed for: r15v22, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r15v40 */
    /* JADX WARN: Type inference failed for: r15v42 */
    /* JADX WARN: Type inference failed for: r15v43 */
    /* JADX WARN: Type inference failed for: r15v44 */
    /* JADX WARN: Type inference failed for: r15v45 */
    /* JADX WARN: Type inference failed for: r15v5 */
    /* JADX WARN: Type inference failed for: r15v6, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r15v9 */
    /* JADX WARN: Type inference failed for: r2v1 */
    /* JADX WARN: Type inference failed for: r2v108 */
    /* JADX WARN: Type inference failed for: r2v109, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v112 */
    /* JADX WARN: Type inference failed for: r2v113, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v116 */
    /* JADX WARN: Type inference failed for: r2v117, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r2v13 */
    /* JADX WARN: Type inference failed for: r2v2, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v209 */
    /* JADX WARN: Type inference failed for: r2v210 */
    /* JADX WARN: Type inference failed for: r2v211 */
    /* JADX WARN: Type inference failed for: r2v212 */
    /* JADX WARN: Type inference failed for: r2v213 */
    /* JADX WARN: Type inference failed for: r2v25 */
    /* JADX WARN: Type inference failed for: r2v26, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v29 */
    /* JADX WARN: Type inference failed for: r2v34 */
    /* JADX WARN: Type inference failed for: r2v35, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r2v36 */
    /* JADX WARN: Type inference failed for: r2v37, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v4 */
    /* JADX WARN: Type inference failed for: r2v46 */
    /* JADX WARN: Type inference failed for: r2v6 */
    /* JADX WARN: Type inference failed for: r2v60 */
    /* JADX WARN: Type inference failed for: r2v61, types: [com.cubeSuite.entity.midikeyboard.EncoderPara] */
    /* JADX WARN: Type inference failed for: r2v68 */
    /* JADX WARN: Type inference failed for: r2v69, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v7, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v75 */
    /* JADX WARN: Type inference failed for: r2v89 */
    /* JADX WARN: Type inference failed for: r2v90, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v93 */
    /* JADX WARN: Type inference failed for: r3v106 */
    /* JADX WARN: Type inference failed for: r3v107 */
    /* JADX WARN: Type inference failed for: r3v2 */
    /* JADX WARN: Type inference failed for: r3v24 */
    /* JADX WARN: Type inference failed for: r3v25, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v3, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r3v31 */
    /* JADX WARN: Type inference failed for: r3v4 */
    /* JADX WARN: Type inference failed for: r3v5, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v52 */
    /* JADX WARN: Type inference failed for: r3v53, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v60 */
    /* JADX WARN: Type inference failed for: r3v74 */
    /* JADX WARN: Type inference failed for: r3v75, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v81 */
    /* JADX WARN: Type inference failed for: r43v18 */
    /* JADX WARN: Type inference failed for: r43v3 */
    /* JADX WARN: Type inference failed for: r43v4, types: [com.cubeSuite.entity.midikeyboard.PedalPara] */
    /* JADX WARN: Type inference failed for: r4v10 */
    /* JADX WARN: Type inference failed for: r4v24 */
    /* JADX WARN: Type inference failed for: r4v25, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r4v27 */
    /* JADX WARN: Type inference failed for: r4v7 */
    /* JADX WARN: Type inference failed for: r4v8, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r5v23, types: [androidx.databinding.InverseBindingListener, androidx.databinding.adapters.AdapterViewBindingAdapter$OnNothingSelected, androidx.databinding.adapters.SeekBarBindingAdapter$OnStartTrackingTouch, androidx.databinding.adapters.SeekBarBindingAdapter$OnStopTrackingTouch, androidx.databinding.adapters.TextViewBindingAdapter$BeforeTextChanged, androidx.databinding.adapters.TextViewBindingAdapter$OnTextChanged] */
    /* JADX WARN: Type inference failed for: r6v1 */
    /* JADX WARN: Type inference failed for: r6v10, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r6v13 */
    /* JADX WARN: Type inference failed for: r6v14, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r6v15 */
    /* JADX WARN: Type inference failed for: r6v16, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r6v2 */
    /* JADX WARN: Type inference failed for: r6v3, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r6v40 */
    /* JADX WARN: Type inference failed for: r6v41 */
    /* JADX WARN: Type inference failed for: r6v42 */
    /* JADX WARN: Type inference failed for: r6v43 */
    /* JADX WARN: Type inference failed for: r6v44 */
    /* JADX WARN: Type inference failed for: r6v5 */
    /* JADX WARN: Type inference failed for: r6v6, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r6v9 */
    /* JADX WARN: Type inference failed for: r72v10 */
    /* JADX WARN: Type inference failed for: r72v12 */
    /* JADX WARN: Type inference failed for: r72v24 */
    /* JADX WARN: Type inference failed for: r72v25 */
    /* JADX WARN: Type inference failed for: r72v26 */
    /* JADX WARN: Type inference failed for: r72v4 */
    /* JADX WARN: Type inference failed for: r72v5, types: [com.cubeSuite.entity.midikeyboard.EncoderPara] */
    /* JADX WARN: Type inference failed for: r7v19 */
    /* JADX WARN: Type inference failed for: r7v3 */
    /* JADX WARN: Type inference failed for: r7v4, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r8v1 */
    /* JADX WARN: Type inference failed for: r8v16 */
    /* JADX WARN: Type inference failed for: r8v17 */
    /* JADX WARN: Type inference failed for: r8v2, types: [com.cubeSuite.entity.midikeyboard.ModPara] */
    /* JADX WARN: Type inference failed for: r8v3 */
    /* JADX WARN: Type inference failed for: r8v4, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r92v0, types: [com.cubeSuite.databinding.Smk25iiUsrFBindingImpl] */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        long j3;
        String strValueOf;
        String str;
        String strValueOf2;
        int data;
        int data2;
        int data3;
        ?? r15;
        ?? ch;
        int data4;
        PitchPara pitchPara;
        long j4;
        ?? type;
        int data5;
        ?? dat;
        ?? r2;
        int data6;
        String strValueOf3;
        ?? r4;
        int data7;
        String strValueOf4;
        ?? r3;
        int data8;
        String strValueOf5;
        String str2;
        ?? ch2;
        String str3;
        int data9;
        String str4;
        String str5;
        String strValueOf6;
        String strValueOf7;
        String strValueOf8;
        int i;
        String str6;
        String str7;
        ?? ch3;
        String str8;
        int i2;
        String str9;
        String str10;
        ?? r13;
        String str11;
        int i3;
        String str12;
        int i4;
        ?? r6;
        long j5;
        int data10;
        ?? r72;
        ?? type2;
        int i5;
        int data11;
        ?? dat2;
        ?? r5;
        int data12;
        String strValueOf9;
        ?? r7;
        String str13;
        int data13;
        String strValueOf10;
        String str14;
        ?? r8;
        int data14;
        String strValueOf11;
        String str15;
        String str16;
        ?? curve;
        String str17;
        int data15;
        String str18;
        String str19;
        String str20;
        String str21;
        String str22;
        ?? r73;
        int padIndex;
        String strValueOf12;
        ?? r14;
        String sysEx;
        ?? ch4;
        String str23;
        int data16;
        ?? led;
        int data17;
        String strValueOf13;
        ?? dat3;
        ?? r0;
        int data18;
        String strValueOf14;
        ?? r9;
        String str24;
        int data19;
        String strValueOf15;
        ?? r1;
        int data20;
        String strValueOf16;
        String str25;
        ?? type3;
        String str26;
        int data21;
        String str27;
        int i6;
        Smk25IIPadPara smk25IIPadPara;
        String str28;
        ?? r12;
        String str29;
        String[] padNoteStr;
        String str30;
        String str31;
        String padMinText;
        int i7;
        String str32;
        String str33;
        String str34;
        int i8;
        String str35;
        int i9;
        String str36;
        String str37;
        String str38;
        int i10;
        String str39;
        int i11;
        String str40;
        String str41;
        String str42;
        int i12;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
            j2 = this.mDirtyFlags_1;
            this.mDirtyFlags_1 = 0L;
        }
        Smk25IIUsrFragment smk25IIUsrFragment = this.mListener;
        Smk25IIUsr smk25IIUsr = this.mData;
        Smk25IIUsrBinding smk25IIUsrBinding = this.mUsrBinding;
        int i13 = ((144115189015379968L & j) > 0L ? 1 : ((144115189015379968L & j) == 0L ? 0 : -1));
        if (((-3746994889972252673L) & j) == 0 && (j2 & 12) == 0) {
            j4 = j;
            j5 = j2;
            smk25IIUsrBinding = smk25IIUsrBinding;
            i13 = i13;
            str12 = null;
            str7 = null;
            strValueOf = null;
            i3 = 0;
            r6 = 0;
            i4 = 0;
            data4 = 0;
            strValueOf2 = null;
            data = 0;
            data5 = 0;
            i2 = 0;
            r13 = 0;
            data9 = 0;
            str11 = null;
            j3 = -4607736561248231414L;
            str = null;
            data3 = 0;
            str5 = null;
            str10 = null;
            str9 = null;
        } else {
            if ((j & 9025208597889412L) == 0 && (j2 & 8) == 0) {
                data2 = 0;
                data3 = 0;
                data = 0;
                strValueOf2 = null;
                strValueOf = null;
                j3 = -4607736561248231414L;
                str = null;
            } else {
                KeyPara keyPara = smk25IIUsr != null ? smk25IIUsr.getKeyPara() : null;
                if ((j & 4831838212L) == 0 && (j2 & 8) == 0) {
                    strValueOf = null;
                    j3 = -4607736561248231414L;
                } else {
                    AddrU8 max = keyPara != null ? keyPara.getMax() : null;
                    j3 = -4607736561248231414L;
                    updateRegistration(2, max);
                    strValueOf = String.valueOf(max != null ? max.getData() : 0);
                }
                if ((j & 137975824512L) == 0 && (j2 & 8) == 0) {
                    strValueOf2 = null;
                    str = null;
                } else {
                    AddrU8 min = keyPara != null ? keyPara.getMin() : null;
                    str = null;
                    updateRegistration(7, min);
                    strValueOf2 = String.valueOf(min != null ? min.getData() : 0);
                }
                if ((j & 275414778112L) == 0 && (j2 & 8) == 0) {
                    data = 0;
                } else {
                    ?? ch5 = keyPara != null ? keyPara.getCh() : str;
                    updateRegistration(8, ch5);
                    if (ch5 != 0) {
                        data = ch5.getData();
                    } else {
                        data = 0;
                    }
                }
                if ((j & 17592722931712L) == 0 && (j2 & 8) == 0) {
                    data2 = 0;
                } else {
                    ?? octave = keyPara != null ? keyPara.getOctave() : str;
                    updateRegistration(14, octave);
                    data2 = (octave != 0 ? octave.getData() : (byte) 0) + 3;
                }
                if ((j & 9007199800000512L) == 0 && (j2 & 8) == 0) {
                    data3 = 0;
                } else {
                    ?? transpose = keyPara != null ? keyPara.getTranspose() : str;
                    updateRegistration(23, transpose);
                    data3 = (transpose != 0 ? transpose.getData() : (byte) 0) + 12;
                }
            }
            if ((j & 36028797589389312L) == 0 && (j2 & 8) == 0) {
                data4 = 0;
            } else {
                if (smk25IIUsr != null) {
                    pitchPara = smk25IIUsr.getPitchPara();
                } else {
                    r15 = str;
                }
                if (r15 != 0) {
                    r15 = pitchPara;
                    ch = r15.getCh();
                } else {
                    r15 = pitchPara;
                    ch = str;
                }
                updateRegistration(25, ch);
                if (ch != 0) {
                    data4 = ch.getData();
                } else {
                    data4 = 0;
                }
            }
            Object padPara = (((j & 720617224544491056L) == 0 && (j2 & 8) == 0) || smk25IIUsr == null) ? str : smk25IIUsr.getPadPara();
            Object encoderPara = (((j & j3) == 0 && (j2 & 8) == 0) || smk25IIUsr == null) ? str : smk25IIUsr.getEncoderPara();
            if ((j & 94646030263451712L) == 0 && (j2 & 8) == 0) {
                j4 = j;
                data4 = data4;
                str4 = str;
                str5 = str4;
                strValueOf5 = str5;
                data9 = 0;
                data5 = 0;
            } else {
                ?? pedalPara = smk25IIUsr != null ? smk25IIUsr.getPedalPara() : str;
                if ((j & 4503600168435712L) == 0 && (j2 & 8) == 0) {
                    j4 = j;
                } else {
                    if (pedalPara != 0) {
                        type = pedalPara.getType();
                        j4 = j;
                    } else {
                        j4 = j;
                        type = str;
                    }
                    updateRegistration(22, type);
                    data5 = type != 0 ? type.getData() : 0;
                    if ((j4 & 18084836526850112L) == 0 || (j2 & 8) != 0) {
                        if (pedalPara != 0) {
                            dat = pedalPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j4 & 69256347712L) == 0 || (j2 & 8) != 0) {
                            if (dat != 0) {
                                r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                            } else {
                                r2 = str;
                            }
                            updateRegistration(6, r2);
                            if (r2 != 0) {
                                data6 = r2.getData();
                            } else {
                                data6 = 0;
                            }
                            strValueOf3 = String.valueOf(data6);
                        } else {
                            strValueOf3 = str;
                        }
                        if ((j4 & 70369281114112L) == 0 || (j2 & 8) != 0) {
                            if (dat != 0) {
                                r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                            } else {
                                r4 = str;
                            }
                            updateRegistration(16, r4);
                            if (r4 != 0) {
                                data7 = r4.getData();
                            } else {
                                data7 = 0;
                            }
                            strValueOf4 = String.valueOf(data7);
                        } else {
                            strValueOf4 = str;
                        }
                        if ((j4 & 18014399063130112L) == 0 || (j2 & 8) != 0) {
                            if (dat != 0) {
                                r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                            } else {
                                r3 = str;
                            }
                            updateRegistration(24, r3);
                            if (r3 != 0) {
                                data8 = r3.getData();
                            } else {
                                data8 = 0;
                            }
                            strValueOf5 = String.valueOf(data8);
                        } else {
                            strValueOf5 = str;
                        }
                        str2 = strValueOf3;
                    } else {
                        data5 = data5;
                        data4 = data4;
                        str2 = str;
                        strValueOf4 = str2;
                        strValueOf5 = strValueOf4;
                    }
                    if ((j4 & 72057594641907712L) == 0 || (j2 & 8) != 0) {
                        if (pedalPara != 0) {
                            ch2 = pedalPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str3 = strValueOf4;
                        updateRegistration(26, ch2);
                        if (ch2 != 0) {
                            data9 = ch2.getData();
                            str4 = str2;
                            str5 = str3;
                        }
                    } else {
                        str2 = str2;
                        str3 = strValueOf4;
                    }
                    str4 = str2;
                    str5 = str3;
                    data9 = 0;
                }
                if ((j4 & 18084836526850112L) == 0) {
                    if (pedalPara != 0) {
                        dat = pedalPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j4 & 69256347712L) == 0) {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(6, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    } else {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(6, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    }
                    if ((j4 & 70369281114112L) == 0) {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(16, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    } else {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(16, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    }
                    if ((j4 & 18014399063130112L) == 0) {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(24, r3);
                        if (r3 != 0) {
                            data8 = r3.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf5 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(24, r3);
                        if (r3 != 0) {
                            data8 = r3.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf5 = String.valueOf(data8);
                    }
                    str2 = strValueOf3;
                } else {
                    if (pedalPara != 0) {
                        dat = pedalPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j4 & 69256347712L) == 0) {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(6, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    } else {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(6, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    }
                    if ((j4 & 70369281114112L) == 0) {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(16, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    } else {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(16, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    }
                    if ((j4 & 18014399063130112L) == 0) {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(24, r3);
                        if (r3 != 0) {
                            data8 = r3.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf5 = String.valueOf(data8);
                    } else {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(24, r3);
                        if (r3 != 0) {
                            data8 = r3.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf5 = String.valueOf(data8);
                    }
                    str2 = strValueOf3;
                }
                if ((j4 & 72057594641907712L) == 0) {
                    if (pedalPara != 0) {
                        ch2 = pedalPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str3 = strValueOf4;
                    updateRegistration(26, ch2);
                    if (ch2 != 0) {
                        data9 = ch2.getData();
                        str4 = str2;
                        str5 = str3;
                    } else {
                        str4 = str2;
                        str5 = str3;
                        data9 = 0;
                    }
                } else {
                    if (pedalPara != 0) {
                        ch2 = pedalPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str3 = strValueOf4;
                    updateRegistration(26, ch2);
                    if (ch2 != 0) {
                        data9 = ch2.getData();
                        str4 = str2;
                        str5 = str3;
                    } else {
                        str4 = str2;
                        str5 = str3;
                        data9 = 0;
                    }
                }
            }
            if ((j4 & 424413233547265L) == 0 && (j2 & 12) == 0) {
                str10 = str4;
                str5 = str5;
                str9 = strValueOf5;
                str12 = str;
                str7 = str12;
                str11 = str7;
                i2 = 0;
            } else {
                ?? modPara = smk25IIUsr != null ? smk25IIUsr.getModPara() : str;
                if ((j4 & 142938256574465L) == 0 && (j2 & 12) == 0) {
                    str4 = str4;
                    str5 = str5;
                    strValueOf5 = strValueOf5;
                    str6 = str;
                    str7 = str6;
                    strValueOf8 = str7;
                } else {
                    ?? dat4 = modPara != 0 ? modPara.getDat() : str;
                    if ((j4 & 1610612737) == 0 && (j2 & 8) == 0) {
                        strValueOf6 = str;
                    } else {
                        ?? r10 = dat4 != 0 ? (AddrU8) getFromArray((Object[]) dat4, 2) : str;
                        updateRegistration(0, r10);
                        strValueOf6 = String.valueOf(r10 != 0 ? r10.getData() : 0);
                    }
                    if ((j4 & 2199560128512L) == 0 && (j2 & 8) == 0) {
                        strValueOf7 = str;
                    } else {
                        ?? r11 = dat4 != 0 ? (AddrU8) getFromArray((Object[]) dat4, 1) : str;
                        updateRegistration(11, r11);
                        strValueOf7 = String.valueOf(r11 != 0 ? r11.getData() : 0);
                    }
                    if ((j4 & 140738159575040L) == 0 && (j2 & 12) == 0) {
                        str6 = strValueOf7;
                        strValueOf8 = str;
                        str7 = strValueOf6;
                    } else {
                        ?? r16 = dat4 != 0 ? (AddrU8) getFromArray((Object[]) dat4, 0) : str;
                        updateRegistration(17, r16);
                        int data22 = r16 != 0 ? r16.getData() : 0;
                        if ((j4 & 140738025357312L) == 0 && (j2 & 8) == 0) {
                            i = data22;
                            str6 = strValueOf7;
                            str7 = strValueOf6;
                            strValueOf8 = str;
                        } else {
                            strValueOf8 = String.valueOf(data22);
                            i = data22;
                            str6 = strValueOf7;
                            str7 = strValueOf6;
                        }
                    }
                    if ((j4 & 281475513843712L) == 0 || (j2 & 8) != 0) {
                        if (modPara != 0) {
                            ch3 = modPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(18, ch3);
                        if (ch3 != 0) {
                            int data23 = ch3.getData();
                            data4 = data4;
                            i13 = i13;
                            data = data;
                            i2 = i;
                            str9 = strValueOf5;
                            str10 = str4;
                            data3 = data3;
                            data5 = data5;
                            smk25IIUsrBinding = smk25IIUsrBinding;
                            strValueOf2 = strValueOf2;
                            r13 = padPara;
                            str11 = strValueOf8;
                            strValueOf = strValueOf;
                            data9 = data9;
                            int i14 = data2;
                            i3 = data23;
                            str12 = str8;
                            i4 = i14;
                            r6 = encoderPara;
                            j5 = j2;
                        }
                    } else {
                        str8 = str6;
                    }
                    str12 = str8;
                    i2 = i;
                    str9 = strValueOf5;
                    str10 = str4;
                    str11 = strValueOf8;
                }
                i = 0;
                if ((j4 & 281475513843712L) == 0) {
                    if (modPara != 0) {
                        ch3 = modPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(18, ch3);
                    if (ch3 != 0) {
                        int data24 = ch3.getData();
                        data4 = data4;
                        i13 = i13;
                        data = data;
                        i2 = i;
                        str9 = strValueOf5;
                        str10 = str4;
                        data3 = data3;
                        data5 = data5;
                        smk25IIUsrBinding = smk25IIUsrBinding;
                        strValueOf2 = strValueOf2;
                        r13 = padPara;
                        str11 = strValueOf8;
                        strValueOf = strValueOf;
                        data9 = data9;
                        int i15 = data2;
                        i3 = data24;
                        str12 = str8;
                        i4 = i15;
                        r6 = encoderPara;
                        j5 = j2;
                    } else {
                        str12 = str8;
                        i2 = i;
                        str9 = strValueOf5;
                        str10 = str4;
                        str11 = strValueOf8;
                    }
                } else {
                    if (modPara != 0) {
                        ch3 = modPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(18, ch3);
                    if (ch3 != 0) {
                        int data25 = ch3.getData();
                        data4 = data4;
                        i13 = i13;
                        data = data;
                        i2 = i;
                        str9 = strValueOf5;
                        str10 = str4;
                        data3 = data3;
                        data5 = data5;
                        smk25IIUsrBinding = smk25IIUsrBinding;
                        strValueOf2 = strValueOf2;
                        r13 = padPara;
                        str11 = strValueOf8;
                        strValueOf = strValueOf;
                        data9 = data9;
                        int i16 = data2;
                        i3 = data25;
                        str12 = str8;
                        i4 = i16;
                        r6 = encoderPara;
                        j5 = j2;
                    } else {
                        str12 = str8;
                        i2 = i;
                        str9 = strValueOf5;
                        str10 = str4;
                        str11 = strValueOf8;
                    }
                }
            }
            i4 = data2;
            r6 = encoderPara;
            i3 = 0;
            j5 = j2;
            r13 = padPara;
        }
        if ((j4 & (-139983709914089926L)) == 0 && (j5 & 15) == 0) {
            str12 = str12;
            str7 = str7;
            i4 = i4;
            str36 = str;
            str30 = str36;
            str31 = str30;
            str37 = str31;
            str41 = str37;
            str42 = str41;
            str39 = str42;
            str32 = str39;
            str38 = str32;
            strValueOf15 = str38;
            strValueOf16 = strValueOf15;
            str28 = strValueOf16;
            str40 = str28;
            str29 = str40;
            padMinText = str29;
            str33 = padMinText;
            str34 = str33;
            str35 = str34;
            i11 = 0;
            i9 = 0;
            i12 = 0;
            i8 = 0;
            i10 = 0;
            i7 = 0;
        } else {
            long j6 = j4 & 134217728;
            String knobMinText = ((j6 == 0 && (j5 & 9) == 0) || smk25IIUsrBinding == null) ? str : smk25IIUsrBinding.getKnobMinText();
            String padNoteText = (((j4 & 288230376285929472L) == 0 && (j5 & 8) == 0) || smk25IIUsrBinding == null) ? str : smk25IIUsrBinding.getPadNoteText();
            String knobMaxText = ((j6 == 0 && (j5 & 10) == 0) || smk25IIUsrBinding == null) ? str : smk25IIUsrBinding.getKnobMaxText();
            long j7 = j4 & (-4611123067802353664L);
            ?? knobCCStr = ((j7 == 0 && (j5 & 8) == 0) || smk25IIUsrBinding == null) ? str : smk25IIUsrBinding.getKnobCCStr();
            String padMaxText = (((j4 & 2305843009347911680L) == 0 && (j5 & 8) == 0) || smk25IIUsrBinding == null) ? str : smk25IIUsrBinding.getPadMaxText();
            if ((j4 & j3) == 0 && (j5 & 8) == 0) {
                str12 = str12;
                str7 = str7;
                str18 = str;
                str22 = str18;
                str21 = str22;
                str20 = str21;
                str19 = str20;
                data15 = 0;
                i5 = 0;
                data11 = 0;
            } else {
                int knobIndex = smk25IIUsrBinding != null ? smk25IIUsrBinding.getKnobIndex() : 0;
                String strValueOf17 = ((j4 & 4611686018561605632L) == 0 && (j5 & 8) == 0) ? str : String.valueOf(knobIndex + 1);
                ?? r17 = r6 != 0 ? (EncoderPara) getFromArray((Object[]) r6, knobIndex) : str;
                if ((j4 & 4611686021245960194L) == 0 && (j5 & 8) == 0) {
                    r73 = r17;
                } else {
                    ?? ch6 = r17 != 0 ? r17.getCh() : str;
                    ?? r74 = r17;
                    updateRegistration(1, ch6);
                    r73 = r74;
                    if (ch6 != 0) {
                        data10 = ch6.getData();
                        r72 = r74;
                    }
                    if ((j4 & 4611694815191506944L) == 0 || (j5 & 8) != 0) {
                        if (r72 != 0) {
                            type2 = r72.getType();
                        } else {
                            type2 = str;
                        }
                        i5 = data10;
                        updateRegistration(13, type2);
                        data11 = type2 != 0 ? type2.getData() : 0;
                        if ((j4 & (-4608871259396636664L)) == 0 || (j5 & 8) != 0) {
                            if (r72 != 0) {
                                dat2 = r72.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j4 & 4611686027688411144L) == 0 || (j5 & 8) != 0) {
                                if (dat2 != 0) {
                                    r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                                } else {
                                    r5 = str;
                                }
                                updateRegistration(3, r5);
                                if (r5 != 0) {
                                    data12 = r5.getData();
                                } else {
                                    data12 = 0;
                                }
                                strValueOf9 = String.valueOf(data12);
                            } else {
                                strValueOf9 = str;
                            }
                            if (j7 == 0 || (j5 & 8) != 0) {
                                if (dat2 != 0) {
                                    r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                                } else {
                                    r7 = str;
                                }
                                str13 = strValueOf9;
                                updateRegistration(19, r7);
                                if (r7 != 0) {
                                    data13 = r7.getData();
                                } else {
                                    data13 = 0;
                                }
                                if ((j4 & 4612248969052422144L) == 0 || (j5 & 8) != 0) {
                                    strValueOf10 = String.valueOf(data13);
                                } else {
                                    strValueOf10 = str;
                                }
                                if (knobCCStr != 0) {
                                    str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                                } else {
                                    str14 = str;
                                }
                            } else {
                                str13 = strValueOf9;
                                str14 = str;
                                strValueOf10 = str14;
                            }
                            if ((j4 & 4613937818914258944L) == 0 || (j5 & 8) != 0) {
                                if (dat2 != 0) {
                                    r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                                } else {
                                    r8 = str;
                                }
                                updateRegistration(21, r8);
                                if (r8 != 0) {
                                    data14 = r8.getData();
                                } else {
                                    data14 = 0;
                                }
                                strValueOf11 = String.valueOf(data14);
                            } else {
                                strValueOf11 = str;
                            }
                            str15 = str14;
                            str16 = str13;
                        } else {
                            data11 = data11;
                            str7 = str7;
                            str16 = str;
                            str15 = str16;
                            strValueOf10 = str15;
                            strValueOf11 = strValueOf10;
                        }
                        if ((j4 & 4612811919006367744L) == 0 || (j5 & 8) != 0) {
                            if (r72 != 0) {
                                curve = r72.getCurve();
                            } else {
                                curve = str;
                            }
                            str17 = str15;
                            updateRegistration(20, curve);
                            if (curve != 0) {
                                String str43 = strValueOf10;
                                data15 = curve.getData();
                                str18 = str16;
                                str19 = strValueOf17;
                                str20 = strValueOf11;
                                str21 = str43;
                                str22 = str17;
                            }
                        } else {
                            str16 = str16;
                            str17 = str15;
                        }
                        str18 = str16;
                        str22 = str17;
                        str19 = strValueOf17;
                        str20 = strValueOf11;
                        str21 = strValueOf10;
                        data15 = 0;
                    } else {
                        i5 = data10;
                    }
                    if ((j4 & (-4608871259396636664L)) == 0) {
                        if (r72 != 0) {
                            dat2 = r72.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j4 & 4611686027688411144L) == 0) {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(3, r5);
                            if (r5 != 0) {
                                data12 = r5.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        } else {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(3, r5);
                            if (r5 != 0) {
                                data12 = r5.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        }
                        if (j7 == 0) {
                            if (dat2 != 0) {
                                r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                            } else {
                                r7 = str;
                            }
                            str13 = strValueOf9;
                            updateRegistration(19, r7);
                            if (r7 != 0) {
                                data13 = r7.getData();
                            } else {
                                data13 = 0;
                            }
                            if ((j4 & 4612248969052422144L) == 0) {
                                strValueOf10 = String.valueOf(data13);
                            } else {
                                strValueOf10 = String.valueOf(data13);
                            }
                            if (knobCCStr != 0) {
                                str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                            } else {
                                str14 = str;
                            }
                        } else {
                            if (dat2 != 0) {
                                r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                            } else {
                                r7 = str;
                            }
                            str13 = strValueOf9;
                            updateRegistration(19, r7);
                            if (r7 != 0) {
                                data13 = r7.getData();
                            } else {
                                data13 = 0;
                            }
                            if ((j4 & 4612248969052422144L) == 0) {
                                strValueOf10 = String.valueOf(data13);
                            } else {
                                strValueOf10 = String.valueOf(data13);
                            }
                            if (knobCCStr != 0) {
                                str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                            } else {
                                str14 = str;
                            }
                        }
                        if ((j4 & 4613937818914258944L) == 0) {
                            if (dat2 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(21, r8);
                            if (r8 != 0) {
                                data14 = r8.getData();
                            } else {
                                data14 = 0;
                            }
                            strValueOf11 = String.valueOf(data14);
                        } else {
                            if (dat2 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(21, r8);
                            if (r8 != 0) {
                                data14 = r8.getData();
                            } else {
                                data14 = 0;
                            }
                            strValueOf11 = String.valueOf(data14);
                        }
                        str15 = str14;
                        str16 = str13;
                    } else {
                        if (r72 != 0) {
                            dat2 = r72.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j4 & 4611686027688411144L) == 0) {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(3, r5);
                            if (r5 != 0) {
                                data12 = r5.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        } else {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(3, r5);
                            if (r5 != 0) {
                                data12 = r5.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        }
                        if (j7 == 0) {
                            if (dat2 != 0) {
                                r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                            } else {
                                r7 = str;
                            }
                            str13 = strValueOf9;
                            updateRegistration(19, r7);
                            if (r7 != 0) {
                                data13 = r7.getData();
                            } else {
                                data13 = 0;
                            }
                            if ((j4 & 4612248969052422144L) == 0) {
                                strValueOf10 = String.valueOf(data13);
                            } else {
                                strValueOf10 = String.valueOf(data13);
                            }
                            if (knobCCStr != 0) {
                                str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                            } else {
                                str14 = str;
                            }
                        } else {
                            if (dat2 != 0) {
                                r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                            } else {
                                r7 = str;
                            }
                            str13 = strValueOf9;
                            updateRegistration(19, r7);
                            if (r7 != 0) {
                                data13 = r7.getData();
                            } else {
                                data13 = 0;
                            }
                            if ((j4 & 4612248969052422144L) == 0) {
                                strValueOf10 = String.valueOf(data13);
                            } else {
                                strValueOf10 = String.valueOf(data13);
                            }
                            if (knobCCStr != 0) {
                                str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                            } else {
                                str14 = str;
                            }
                        }
                        if ((j4 & 4613937818914258944L) == 0) {
                            if (dat2 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(21, r8);
                            if (r8 != 0) {
                                data14 = r8.getData();
                            } else {
                                data14 = 0;
                            }
                            strValueOf11 = String.valueOf(data14);
                        } else {
                            if (dat2 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(21, r8);
                            if (r8 != 0) {
                                data14 = r8.getData();
                            } else {
                                data14 = 0;
                            }
                            strValueOf11 = String.valueOf(data14);
                        }
                        str15 = str14;
                        str16 = str13;
                    }
                    if ((j4 & 4612811919006367744L) == 0) {
                        if (r72 != 0) {
                            curve = r72.getCurve();
                        } else {
                            curve = str;
                        }
                        str17 = str15;
                        updateRegistration(20, curve);
                        if (curve != 0) {
                            String str44 = strValueOf10;
                            data15 = curve.getData();
                            str18 = str16;
                            str19 = strValueOf17;
                            str20 = strValueOf11;
                            str21 = str44;
                            str22 = str17;
                        } else {
                            str18 = str16;
                            str22 = str17;
                            str19 = strValueOf17;
                            str20 = strValueOf11;
                            str21 = strValueOf10;
                            data15 = 0;
                        }
                    } else {
                        if (r72 != 0) {
                            curve = r72.getCurve();
                        } else {
                            curve = str;
                        }
                        str17 = str15;
                        updateRegistration(20, curve);
                        if (curve != 0) {
                            String str45 = strValueOf10;
                            data15 = curve.getData();
                            str18 = str16;
                            str19 = strValueOf17;
                            str20 = strValueOf11;
                            str21 = str45;
                            str22 = str17;
                        } else {
                            str18 = str16;
                            str22 = str17;
                            str19 = strValueOf17;
                            str20 = strValueOf11;
                            str21 = strValueOf10;
                            data15 = 0;
                        }
                    }
                }
                data10 = 0;
                r72 = r73;
                if ((j4 & 4611694815191506944L) == 0) {
                    if (r72 != 0) {
                        type2 = r72.getType();
                    } else {
                        type2 = str;
                    }
                    i5 = data10;
                    updateRegistration(13, type2);
                    if (type2 != 0) {
                    }
                } else {
                    if (r72 != 0) {
                        type2 = r72.getType();
                    } else {
                        type2 = str;
                    }
                    i5 = data10;
                    updateRegistration(13, type2);
                    if (type2 != 0) {
                    }
                }
                if ((j4 & (-4608871259396636664L)) == 0) {
                    if (r72 != 0) {
                        dat2 = r72.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j4 & 4611686027688411144L) == 0) {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(3, r5);
                        if (r5 != 0) {
                            data12 = r5.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    } else {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(3, r5);
                        if (r5 != 0) {
                            data12 = r5.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    }
                    if (j7 == 0) {
                        if (dat2 != 0) {
                            r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r7 = str;
                        }
                        str13 = strValueOf9;
                        updateRegistration(19, r7);
                        if (r7 != 0) {
                            data13 = r7.getData();
                        } else {
                            data13 = 0;
                        }
                        if ((j4 & 4612248969052422144L) == 0) {
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            strValueOf10 = String.valueOf(data13);
                        }
                        if (knobCCStr != 0) {
                            str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                        } else {
                            str14 = str;
                        }
                    } else {
                        if (dat2 != 0) {
                            r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r7 = str;
                        }
                        str13 = strValueOf9;
                        updateRegistration(19, r7);
                        if (r7 != 0) {
                            data13 = r7.getData();
                        } else {
                            data13 = 0;
                        }
                        if ((j4 & 4612248969052422144L) == 0) {
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            strValueOf10 = String.valueOf(data13);
                        }
                        if (knobCCStr != 0) {
                            str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                        } else {
                            str14 = str;
                        }
                    }
                    if ((j4 & 4613937818914258944L) == 0) {
                        if (dat2 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(21, r8);
                        if (r8 != 0) {
                            data14 = r8.getData();
                        } else {
                            data14 = 0;
                        }
                        strValueOf11 = String.valueOf(data14);
                    } else {
                        if (dat2 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(21, r8);
                        if (r8 != 0) {
                            data14 = r8.getData();
                        } else {
                            data14 = 0;
                        }
                        strValueOf11 = String.valueOf(data14);
                    }
                    str15 = str14;
                    str16 = str13;
                } else {
                    if (r72 != 0) {
                        dat2 = r72.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j4 & 4611686027688411144L) == 0) {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(3, r5);
                        if (r5 != 0) {
                            data12 = r5.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    } else {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(3, r5);
                        if (r5 != 0) {
                            data12 = r5.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    }
                    if (j7 == 0) {
                        if (dat2 != 0) {
                            r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r7 = str;
                        }
                        str13 = strValueOf9;
                        updateRegistration(19, r7);
                        if (r7 != 0) {
                            data13 = r7.getData();
                        } else {
                            data13 = 0;
                        }
                        if ((j4 & 4612248969052422144L) == 0) {
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            strValueOf10 = String.valueOf(data13);
                        }
                        if (knobCCStr != 0) {
                            str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                        } else {
                            str14 = str;
                        }
                    } else {
                        if (dat2 != 0) {
                            r7 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r7 = str;
                        }
                        str13 = strValueOf9;
                        updateRegistration(19, r7);
                        if (r7 != 0) {
                            data13 = r7.getData();
                        } else {
                            data13 = 0;
                        }
                        if ((j4 & 4612248969052422144L) == 0) {
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            strValueOf10 = String.valueOf(data13);
                        }
                        if (knobCCStr != 0) {
                            str14 = (String) getFromArray((Object[]) knobCCStr, data13);
                        } else {
                            str14 = str;
                        }
                    }
                    if ((j4 & 4613937818914258944L) == 0) {
                        if (dat2 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(21, r8);
                        if (r8 != 0) {
                            data14 = r8.getData();
                        } else {
                            data14 = 0;
                        }
                        strValueOf11 = String.valueOf(data14);
                    } else {
                        if (dat2 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(21, r8);
                        if (r8 != 0) {
                            data14 = r8.getData();
                        } else {
                            data14 = 0;
                        }
                        strValueOf11 = String.valueOf(data14);
                    }
                    str15 = str14;
                    str16 = str13;
                }
                if ((j4 & 4612811919006367744L) == 0) {
                    if (r72 != 0) {
                        curve = r72.getCurve();
                    } else {
                        curve = str;
                    }
                    str17 = str15;
                    updateRegistration(20, curve);
                    if (curve != 0) {
                        String str46 = strValueOf10;
                        data15 = curve.getData();
                        str18 = str16;
                        str19 = strValueOf17;
                        str20 = strValueOf11;
                        str21 = str46;
                        str22 = str17;
                    } else {
                        str18 = str16;
                        str22 = str17;
                        str19 = strValueOf17;
                        str20 = strValueOf11;
                        str21 = strValueOf10;
                        data15 = 0;
                    }
                } else {
                    if (r72 != 0) {
                        curve = r72.getCurve();
                    } else {
                        curve = str;
                    }
                    str17 = str15;
                    updateRegistration(20, curve);
                    if (curve != 0) {
                        String str47 = strValueOf10;
                        data15 = curve.getData();
                        str18 = str16;
                        str19 = strValueOf17;
                        str20 = strValueOf11;
                        str21 = str47;
                        str22 = str17;
                    } else {
                        str18 = str16;
                        str22 = str17;
                        str19 = strValueOf17;
                        str20 = strValueOf11;
                        str21 = strValueOf10;
                        data15 = 0;
                    }
                }
            }
            if ((j4 & 720617224544491056L) == 0 && (j5 & 8) == 0) {
                str18 = str18;
                str22 = str22;
                str27 = str;
                strValueOf16 = str27;
                strValueOf15 = strValueOf16;
                strValueOf12 = strValueOf15;
                str26 = strValueOf12;
                str23 = str26;
                i6 = 0;
                data21 = 0;
                padIndex = 0;
                data16 = 0;
            } else {
                padIndex = smk25IIUsrBinding != null ? smk25IIUsrBinding.getPadIndex() : 0;
                strValueOf12 = ((j4 & 144115188210073600L) == 0 && (j5 & 8) == 0) ? str : String.valueOf(padIndex + 1);
                if (r13 != 0) {
                    smk25IIPadPara = (Smk25IIPadPara) getFromArray((Object[]) r13, padIndex);
                } else {
                    r14 = str;
                }
                if (i13 == 0 && (j5 & 8) == 0) {
                    r14 = smk25IIPadPara;
                    padIndex = padIndex;
                } else {
                    Sys sys = r14 != 0 ? r14.getSys() : str;
                    if (smk25IIUsrFragment != null) {
                        sysEx = smk25IIUsrFragment.getSysEx(sys);
                    }
                    if ((j4 & 144115738502758912L) == 0 || (j5 & 8) != 0) {
                        if (r14 != 0) {
                            ch4 = r14.getCh();
                        } else {
                            ch4 = str;
                        }
                        str23 = sysEx;
                        updateRegistration(9, ch4);
                        data16 = ch4 != 0 ? ch4.getData() : 0;
                        if ((j4 & 144116288258573312L) == 0 || (j5 & 8) != 0) {
                            if (r14 != 0) {
                                led = r14.getLed();
                            } else {
                                led = str;
                            }
                            updateRegistration(10, led);
                            if (led != 0) {
                                data17 = led.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf13 = String.valueOf(data17);
                        } else {
                            strValueOf13 = str;
                        }
                        if ((j4 & 720580390636490800L) == 0 || (j5 & 8) != 0) {
                            if (r14 != 0) {
                                dat3 = r14.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j4 & 144115205926813712L) == 0 || (j5 & 8) != 0) {
                                if (dat3 != 0) {
                                    r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                                } else {
                                    r0 = str;
                                }
                                updateRegistration(4, r0);
                                if (r0 != 0) {
                                    data18 = r0.getData();
                                } else {
                                    data18 = 0;
                                }
                                strValueOf14 = String.valueOf(data18);
                            } else {
                                strValueOf14 = str;
                            }
                            if ((j4 & 720575975410106400L) == 0 || (j5 & 8) != 0) {
                                if (dat3 != 0) {
                                    r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                                } else {
                                    r9 = str;
                                }
                                str24 = strValueOf14;
                                updateRegistration(5, r9);
                                if (r9 != 0) {
                                    data19 = r9.getData();
                                } else {
                                    data19 = 0;
                                }
                                if ((j4 & 144115223106682912L) == 0 || (j5 & 8) != 0) {
                                    strValueOf15 = String.valueOf(data19);
                                } else {
                                    strValueOf15 = str;
                                }
                            } else {
                                str24 = strValueOf14;
                                strValueOf15 = str;
                                data19 = 0;
                            }
                            if ((j4 & 144119586793459712L) == 0 || (j5 & 8) != 0) {
                                if (dat3 != 0) {
                                    r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                                } else {
                                    r1 = str;
                                }
                                updateRegistration(12, r1);
                                if (r1 != 0) {
                                    data20 = r1.getData();
                                } else {
                                    data20 = 0;
                                }
                                strValueOf16 = String.valueOf(data20);
                            } else {
                                strValueOf16 = str;
                            }
                            str25 = str24;
                        } else {
                            strValueOf13 = strValueOf13;
                            str22 = str22;
                            str25 = str;
                            strValueOf16 = str25;
                            strValueOf15 = strValueOf16;
                            data19 = 0;
                        }
                        if ((j4 & 144150373119066112L) == 0 || (j5 & 8) != 0) {
                            if (r14 != 0) {
                                type3 = r14.getType();
                            } else {
                                type3 = str;
                            }
                            str26 = str25;
                            updateRegistration(15, type3);
                            if (type3 != 0) {
                                data21 = type3.getData();
                                str27 = strValueOf13;
                                i6 = data19;
                            }
                        } else {
                            str26 = str25;
                        }
                        str27 = strValueOf13;
                        i6 = data19;
                        data21 = 0;
                    } else {
                        str23 = sysEx;
                    }
                    if ((j4 & 144116288258573312L) == 0) {
                        if (r14 != 0) {
                            led = r14.getLed();
                        } else {
                            led = str;
                        }
                        updateRegistration(10, led);
                        if (led != 0) {
                            data17 = led.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    } else {
                        if (r14 != 0) {
                            led = r14.getLed();
                        } else {
                            led = str;
                        }
                        updateRegistration(10, led);
                        if (led != 0) {
                            data17 = led.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    }
                    if ((j4 & 720580390636490800L) == 0) {
                        if (r14 != 0) {
                            dat3 = r14.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j4 & 144115205926813712L) == 0) {
                            if (dat3 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data18 = r0.getData();
                            } else {
                                data18 = 0;
                            }
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            if (dat3 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data18 = r0.getData();
                            } else {
                                data18 = 0;
                            }
                            strValueOf14 = String.valueOf(data18);
                        }
                        if ((j4 & 720575975410106400L) == 0) {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str24 = strValueOf14;
                            updateRegistration(5, r9);
                            if (r9 != 0) {
                                data19 = r9.getData();
                            } else {
                                data19 = 0;
                            }
                            if ((j4 & 144115223106682912L) == 0) {
                                strValueOf15 = String.valueOf(data19);
                            } else {
                                strValueOf15 = String.valueOf(data19);
                            }
                        } else {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str24 = strValueOf14;
                            updateRegistration(5, r9);
                            if (r9 != 0) {
                                data19 = r9.getData();
                            } else {
                                data19 = 0;
                            }
                            if ((j4 & 144115223106682912L) == 0) {
                                strValueOf15 = String.valueOf(data19);
                            } else {
                                strValueOf15 = String.valueOf(data19);
                            }
                        }
                        if ((j4 & 144119586793459712L) == 0) {
                            if (dat3 != 0) {
                                r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r1 = str;
                            }
                            updateRegistration(12, r1);
                            if (r1 != 0) {
                                data20 = r1.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf16 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r1 = str;
                            }
                            updateRegistration(12, r1);
                            if (r1 != 0) {
                                data20 = r1.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf16 = String.valueOf(data20);
                        }
                        str25 = str24;
                    } else {
                        if (r14 != 0) {
                            dat3 = r14.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j4 & 144115205926813712L) == 0) {
                            if (dat3 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data18 = r0.getData();
                            } else {
                                data18 = 0;
                            }
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            if (dat3 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data18 = r0.getData();
                            } else {
                                data18 = 0;
                            }
                            strValueOf14 = String.valueOf(data18);
                        }
                        if ((j4 & 720575975410106400L) == 0) {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str24 = strValueOf14;
                            updateRegistration(5, r9);
                            if (r9 != 0) {
                                data19 = r9.getData();
                            } else {
                                data19 = 0;
                            }
                            if ((j4 & 144115223106682912L) == 0) {
                                strValueOf15 = String.valueOf(data19);
                            } else {
                                strValueOf15 = String.valueOf(data19);
                            }
                        } else {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str24 = strValueOf14;
                            updateRegistration(5, r9);
                            if (r9 != 0) {
                                data19 = r9.getData();
                            } else {
                                data19 = 0;
                            }
                            if ((j4 & 144115223106682912L) == 0) {
                                strValueOf15 = String.valueOf(data19);
                            } else {
                                strValueOf15 = String.valueOf(data19);
                            }
                        }
                        if ((j4 & 144119586793459712L) == 0) {
                            if (dat3 != 0) {
                                r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r1 = str;
                            }
                            updateRegistration(12, r1);
                            if (r1 != 0) {
                                data20 = r1.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf16 = String.valueOf(data20);
                        } else {
                            if (dat3 != 0) {
                                r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r1 = str;
                            }
                            updateRegistration(12, r1);
                            if (r1 != 0) {
                                data20 = r1.getData();
                            } else {
                                data20 = 0;
                            }
                            strValueOf16 = String.valueOf(data20);
                        }
                        str25 = str24;
                    }
                    if ((j4 & 144150373119066112L) == 0) {
                        if (r14 != 0) {
                            type3 = r14.getType();
                        } else {
                            type3 = str;
                        }
                        str26 = str25;
                        updateRegistration(15, type3);
                        if (type3 != 0) {
                            data21 = type3.getData();
                            str27 = strValueOf13;
                            i6 = data19;
                        } else {
                            str27 = strValueOf13;
                            i6 = data19;
                            data21 = 0;
                        }
                    } else {
                        if (r14 != 0) {
                            type3 = r14.getType();
                        } else {
                            type3 = str;
                        }
                        str26 = str25;
                        updateRegistration(15, type3);
                        if (type3 != 0) {
                            data21 = type3.getData();
                            str27 = strValueOf13;
                            i6 = data19;
                        } else {
                            str27 = strValueOf13;
                            i6 = data19;
                            data21 = 0;
                        }
                    }
                }
                sysEx = str;
                if ((j4 & 144115738502758912L) == 0) {
                    if (r14 != 0) {
                        ch4 = r14.getCh();
                    } else {
                        ch4 = str;
                    }
                    str23 = sysEx;
                    updateRegistration(9, ch4);
                    if (ch4 != 0) {
                    }
                } else {
                    if (r14 != 0) {
                        ch4 = r14.getCh();
                    } else {
                        ch4 = str;
                    }
                    str23 = sysEx;
                    updateRegistration(9, ch4);
                    if (ch4 != 0) {
                    }
                }
                if ((j4 & 144116288258573312L) == 0) {
                    if (r14 != 0) {
                        led = r14.getLed();
                    } else {
                        led = str;
                    }
                    updateRegistration(10, led);
                    if (led != 0) {
                        data17 = led.getData();
                    } else {
                        data17 = 0;
                    }
                    strValueOf13 = String.valueOf(data17);
                } else {
                    if (r14 != 0) {
                        led = r14.getLed();
                    } else {
                        led = str;
                    }
                    updateRegistration(10, led);
                    if (led != 0) {
                        data17 = led.getData();
                    } else {
                        data17 = 0;
                    }
                    strValueOf13 = String.valueOf(data17);
                }
                if ((j4 & 720580390636490800L) == 0) {
                    if (r14 != 0) {
                        dat3 = r14.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j4 & 144115205926813712L) == 0) {
                        if (dat3 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data18 = r0.getData();
                        } else {
                            data18 = 0;
                        }
                        strValueOf14 = String.valueOf(data18);
                    } else {
                        if (dat3 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data18 = r0.getData();
                        } else {
                            data18 = 0;
                        }
                        strValueOf14 = String.valueOf(data18);
                    }
                    if ((j4 & 720575975410106400L) == 0) {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str24 = strValueOf14;
                        updateRegistration(5, r9);
                        if (r9 != 0) {
                            data19 = r9.getData();
                        } else {
                            data19 = 0;
                        }
                        if ((j4 & 144115223106682912L) == 0) {
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            strValueOf15 = String.valueOf(data19);
                        }
                    } else {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str24 = strValueOf14;
                        updateRegistration(5, r9);
                        if (r9 != 0) {
                            data19 = r9.getData();
                        } else {
                            data19 = 0;
                        }
                        if ((j4 & 144115223106682912L) == 0) {
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            strValueOf15 = String.valueOf(data19);
                        }
                    }
                    if ((j4 & 144119586793459712L) == 0) {
                        if (dat3 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(12, r1);
                        if (r1 != 0) {
                            data20 = r1.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf16 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(12, r1);
                        if (r1 != 0) {
                            data20 = r1.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf16 = String.valueOf(data20);
                    }
                    str25 = str24;
                } else {
                    if (r14 != 0) {
                        dat3 = r14.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j4 & 144115205926813712L) == 0) {
                        if (dat3 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data18 = r0.getData();
                        } else {
                            data18 = 0;
                        }
                        strValueOf14 = String.valueOf(data18);
                    } else {
                        if (dat3 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data18 = r0.getData();
                        } else {
                            data18 = 0;
                        }
                        strValueOf14 = String.valueOf(data18);
                    }
                    if ((j4 & 720575975410106400L) == 0) {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str24 = strValueOf14;
                        updateRegistration(5, r9);
                        if (r9 != 0) {
                            data19 = r9.getData();
                        } else {
                            data19 = 0;
                        }
                        if ((j4 & 144115223106682912L) == 0) {
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            strValueOf15 = String.valueOf(data19);
                        }
                    } else {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str24 = strValueOf14;
                        updateRegistration(5, r9);
                        if (r9 != 0) {
                            data19 = r9.getData();
                        } else {
                            data19 = 0;
                        }
                        if ((j4 & 144115223106682912L) == 0) {
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            strValueOf15 = String.valueOf(data19);
                        }
                    }
                    if ((j4 & 144119586793459712L) == 0) {
                        if (dat3 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(12, r1);
                        if (r1 != 0) {
                            data20 = r1.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf16 = String.valueOf(data20);
                    } else {
                        if (dat3 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(12, r1);
                        if (r1 != 0) {
                            data20 = r1.getData();
                        } else {
                            data20 = 0;
                        }
                        strValueOf16 = String.valueOf(data20);
                    }
                    str25 = str24;
                }
                if ((j4 & 144150373119066112L) == 0) {
                    if (r14 != 0) {
                        type3 = r14.getType();
                    } else {
                        type3 = str;
                    }
                    str26 = str25;
                    updateRegistration(15, type3);
                    if (type3 != 0) {
                        data21 = type3.getData();
                        str27 = strValueOf13;
                        i6 = data19;
                    } else {
                        str27 = strValueOf13;
                        i6 = data19;
                        data21 = 0;
                    }
                } else {
                    if (r14 != 0) {
                        type3 = r14.getType();
                    } else {
                        type3 = str;
                    }
                    str26 = str25;
                    updateRegistration(15, type3);
                    if (type3 != 0) {
                        data21 = type3.getData();
                        str27 = strValueOf13;
                        i6 = data19;
                    } else {
                        str27 = strValueOf13;
                        i6 = data19;
                        data21 = 0;
                    }
                }
            }
            if ((j4 & 140738159575040L) == 0 && (j5 & 12) == 0) {
                str27 = str27;
            } else {
                ?? modCCStr = smk25IIUsrBinding != null ? smk25IIUsrBinding.getModCCStr() : str;
                if (modCCStr != 0) {
                    str28 = (String) getFromArray((Object[]) modCCStr, i2);
                }
                if ((j4 & 720575975410106400L) == 0 || (j5 & 8) != 0) {
                    if (smk25IIUsrBinding != null) {
                        padNoteStr = smk25IIUsrBinding.getPadNoteStr();
                    } else {
                        r12 = str;
                    }
                    if (r12 != 0) {
                        r12 = padNoteStr;
                        str29 = (String) getFromArray((Object[]) r12, i6);
                    } else {
                        r12 = padNoteStr;
                        str29 = str;
                    }
                } else {
                    r12 = padNoteStr;
                    str29 = str;
                }
                if (((j4 & 1152921504741064704L) == 0 || (j5 & 8) != 0) && smk25IIUsrBinding != null) {
                    str30 = str20;
                    str31 = str18;
                    padMinText = smk25IIUsrBinding.getPadMinText();
                } else {
                    str30 = str20;
                    str31 = str18;
                    padMinText = str;
                }
                i7 = data21;
                str32 = knobMaxText;
                str33 = padNoteText;
                str34 = padMaxText;
                i8 = i5;
                str35 = str23;
                i9 = data16;
                str36 = str27;
                str37 = str26;
                str38 = str19;
                i10 = data15;
                str39 = knobMinText;
                i11 = data11;
                str40 = str21;
                str41 = str22;
                str42 = strValueOf12;
                i12 = padIndex;
            }
            str28 = str;
            if ((j4 & 720575975410106400L) == 0) {
                if (smk25IIUsrBinding != null) {
                    padNoteStr = smk25IIUsrBinding.getPadNoteStr();
                } else {
                    r12 = str;
                }
                if (r12 != 0) {
                    r12 = padNoteStr;
                    str29 = (String) getFromArray((Object[]) r12, i6);
                } else {
                    r12 = padNoteStr;
                    str29 = str;
                }
            } else {
                if (smk25IIUsrBinding != null) {
                    padNoteStr = smk25IIUsrBinding.getPadNoteStr();
                } else {
                    r12 = str;
                }
                if (r12 != 0) {
                    r12 = padNoteStr;
                    str29 = (String) getFromArray((Object[]) r12, i6);
                } else {
                    r12 = padNoteStr;
                    str29 = str;
                }
            }
            if ((j4 & 1152921504741064704L) == 0) {
                str30 = str20;
                str31 = str18;
                padMinText = smk25IIUsrBinding.getPadMinText();
            } else {
                str30 = str20;
                str31 = str18;
                padMinText = smk25IIUsrBinding.getPadMinText();
            }
            i7 = data21;
            str32 = knobMaxText;
            str33 = padNoteText;
            str34 = padMaxText;
            i8 = i5;
            str35 = str23;
            i9 = data16;
            str36 = str27;
            str37 = str26;
            str38 = str19;
            i10 = data15;
            str39 = knobMinText;
            i11 = data11;
            str40 = str21;
            str41 = str22;
            str42 = strValueOf12;
            i12 = padIndex;
        }
        if ((j4 & 144115738502758912L) != 0 || (j5 & 8) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.channel, i9);
        }
        long j8 = j5 & 8;
        if (j8 != 0) {
            ?? r18 = str;
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.channel, this.mCallback9, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keybedChannel, this.mCallback16, r18, r18);
            TextViewBindingAdapter.setTextWatcher(this.keybedMaxVel, r18, r18, this.mCallback20, r18);
            TextViewBindingAdapter.setTextWatcher(this.keybedMinVel, r18, r18, this.mCallback19, r18);
            TextViewBindingAdapter.setTextWatcher(this.knobMaxVal, r18, r18, this.mCallback27, r18);
            TextViewBindingAdapter.setTextWatcher(this.knobMinVal, r18, r18, this.mCallback26, r18);
            SeekBarBindingAdapter.setOnSeekBarChangeListener(this.knobSeekbar, r18, r18, this.mCallback21, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.knobType, this.mCallback22, r18, r18);
            TextViewBindingAdapter.setTextWatcher(this.ledVel, r18, r18, this.mCallback14, r18);
            TextViewBindingAdapter.setTextWatcher(this.maxVel, r18, r18, this.mCallback12, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView24, this.mCallback23, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView25, this.mCallback24, r18, r18);
            TextViewBindingAdapter.setTextWatcher(this.mboundView26, r18, r18, this.mCallback25, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView32, this.mCallback28, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView37, this.mCallback32, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView38, this.mCallback33, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView39, this.mCallback34, r18, r18);
            TextViewBindingAdapter.setTextWatcher(this.minVel, r18, r18, this.mCallback11, r18);
            TextViewBindingAdapter.setTextWatcher(this.modCCVal, r18, r18, this.mCallback29, r18);
            TextViewBindingAdapter.setTextWatcher(this.modMaxVal, r18, r18, this.mCallback31, r18);
            TextViewBindingAdapter.setTextWatcher(this.modMinVal, r18, r18, this.mCallback30, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.octive, this.mCallback17, r18, r18);
            TextViewBindingAdapter.setTextWatcher(this.padNote, r18, r18, this.mCallback10, r18);
            SeekBarBindingAdapter.setOnSeekBarChangeListener(this.padSeekbar, r18, r18, this.mCallback7, r18);
            this.selectColorBtn.setOnClickListener(this.mCallback13);
            TextViewBindingAdapter.setTextWatcher(this.sustainCCVal, r18, r18, this.mCallback35, r18);
            TextViewBindingAdapter.setTextWatcher(this.sustainDownVal, r18, r18, this.mCallback37, r18);
            TextViewBindingAdapter.setTextWatcher(this.sustainUpVal, r18, r18, this.mCallback36, r18);
            TextViewBindingAdapter.setTextWatcher(this.sysExValue, r18, r18, this.mCallback15, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.transpose, this.mCallback18, r18, r18);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.type, this.mCallback8, r18, r18);
            RadioGroupBindingAdapter.setListeners(this.typeRadio, this.mCallback6, r18);
        }
        if ((j4 & 275414778112L) != 0 || j8 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keybedChannel, data);
        }
        if ((j4 & 4831838212L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.keybedMaxVel, strValueOf);
        }
        if ((j4 & 137975824512L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.keybedMinVel, strValueOf2);
        }
        if ((j4 & (-4611123067802353664L)) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.knobCCStr, str41);
        }
        long j9 = j4 & 134217728;
        if (j9 != 0 || (j5 & 10) != 0) {
            TextViewBindingAdapter.setText(this.knobMaxText, str32);
        }
        if ((j4 & 4611686027688411144L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.knobMaxVal, str31);
        }
        if (j9 != 0 || (j5 & 9) != 0) {
            TextViewBindingAdapter.setText(this.knobMinText, str39);
        }
        if ((j4 & 4613937818914258944L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.knobMinVal, str30);
        }
        if ((j4 & 144115188210073600L) != 0 || r12 != 0) {
            SeekBarBindingAdapter.setProgress(this.knobSeekbar, i12);
            TextViewBindingAdapter.setText(this.mboundView2, str42);
            SeekBarBindingAdapter.setProgress(this.padSeekbar, i12);
        }
        if ((j4 & 4611694815191506944L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.knobType, i11);
        }
        if ((j4 & 144116288258573312L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.ledVel, str36);
        }
        if ((j4 & 144115205926813712L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.maxVel, str37);
        }
        if ((j4 & 4611686018561605632L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.mboundView21, str38);
        }
        if ((j4 & 4612811919006367744L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView24, i10);
        }
        if ((j4 & 4611686021245960194L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView25, i8);
        }
        if ((j4 & 4612248969052422144L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.mboundView26, str40);
        }
        if ((j4 & 281475513843712L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView32, i3);
        }
        if ((j4 & 140738159575040L) != 0 || (j5 & 12) != 0) {
            TextViewBindingAdapter.setText(this.mboundView34, str28);
        }
        if ((j4 & 36028797589389312L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView37, data4);
        }
        if ((j4 & 4503600168435712L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView38, data5);
        }
        if ((j4 & 72057594641907712L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView39, data9);
        }
        if ((j4 & 144119586793459712L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.minVel, strValueOf16);
        }
        if ((j4 & 140738025357312L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.modCCVal, str11);
        }
        if ((j4 & 1610612737) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.modMaxVal, str7);
        }
        if ((j4 & 2199560128512L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.modMinVal, str12);
        }
        if ((j4 & 17592722931712L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.octive, i4);
        }
        if ((j4 & 2305843009347911680L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.padMaxText, str34);
        }
        if ((j4 & 1152921504741064704L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.padMinText, padMinText);
        }
        if ((j4 & 144115223106682912L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.padNote, strValueOf15);
        }
        if ((j4 & 720575975410106400L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.padNoteStr, str29);
        }
        if ((j4 & 288230376285929472L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.padNoteText, str33);
        }
        if ((j4 & 18014399063130112L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.sustainCCVal, str9);
        }
        if ((j4 & 69256347712L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.sustainDownVal, str10);
        }
        if ((j4 & 70369281114112L) != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.sustainUpVal, str5);
        }
        if (i13 != 0 || r12 != 0) {
            TextViewBindingAdapter.setText(this.sysExValue, str35);
        }
        if ((j4 & 9007199800000512L) != 0 || r12 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.transpose, data3);
        }
        if ((j4 & 144150373119066112L) == 0 && r12 == 0) {
            return;
        }
        AdapterViewBindingAdapter.setSelectedItemPosition(this.type, i7);
    }

    @Override // com.cubeSuite.generated.callback.AfterTextChanged.Listener
    public final void _internalCallbackAfterTextChanged(int i, Editable editable) {
        Smk25IIPadPara[] padPara;
        Smk25IIPadPara smk25IIPadPara;
        AddrU8[] dat;
        Smk25IIPadPara[] padPara2;
        Smk25IIPadPara smk25IIPadPara2;
        AddrU8[] dat2;
        Smk25IIPadPara[] padPara3;
        Smk25IIPadPara smk25IIPadPara3;
        AddrU8[] dat3;
        Smk25IIPadPara[] padPara4;
        Smk25IIPadPara smk25IIPadPara4;
        Smk25IIPadPara[] padPara5;
        Smk25IIPadPara smk25IIPadPara5;
        KeyPara keyPara;
        KeyPara keyPara2;
        EncoderPara[] encoderPara;
        EncoderPara encoderPara2;
        AddrU8[] dat4;
        EncoderPara[] encoderPara3;
        EncoderPara encoderPara4;
        AddrU8[] dat5;
        EncoderPara[] encoderPara5;
        EncoderPara encoderPara6;
        AddrU8[] dat6;
        ModPara modPara;
        AddrU8[] dat7;
        ModPara modPara2;
        AddrU8[] dat8;
        ModPara modPara3;
        AddrU8[] dat9;
        PedalPara pedalPara;
        AddrU8[] dat10;
        PedalPara pedalPara2;
        AddrU8[] dat11;
        PedalPara pedalPara3;
        AddrU8[] dat12;
        switch (i) {
            case 5:
                Smk25IIUsrFragment smk25IIUsrFragment = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr = this.mData;
                if (smk25IIUsrFragment != null && smk25IIUsr != null && (padPara = smk25IIUsr.getPadPara()) != null && smk25IIUsrBinding != null && (smk25IIPadPara = (Smk25IIPadPara) getFromArray(padPara, smk25IIUsrBinding.getPadIndex())) != null && (dat = smk25IIPadPara.getDat()) != null) {
                    smk25IIUsrFragment.setNum(editable, (AddrU8) getFromArray(dat, 0), 0, 127);
                    break;
                }
                break;
            case 6:
                Smk25IIUsrFragment smk25IIUsrFragment2 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding2 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr2 = this.mData;
                if (smk25IIUsrFragment2 != null && smk25IIUsr2 != null && (padPara2 = smk25IIUsr2.getPadPara()) != null && smk25IIUsrBinding2 != null && (smk25IIPadPara2 = (Smk25IIPadPara) getFromArray(padPara2, smk25IIUsrBinding2.getPadIndex())) != null && (dat2 = smk25IIPadPara2.getDat()) != null) {
                    smk25IIUsrFragment2.setNum(editable, (AddrU8) getFromArray(dat2, 1), 0, 127);
                    break;
                }
                break;
            case 7:
                Smk25IIUsrFragment smk25IIUsrFragment3 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding3 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr3 = this.mData;
                if (smk25IIUsrFragment3 != null && smk25IIUsr3 != null && (padPara3 = smk25IIUsr3.getPadPara()) != null && smk25IIUsrBinding3 != null && (smk25IIPadPara3 = (Smk25IIPadPara) getFromArray(padPara3, smk25IIUsrBinding3.getPadIndex())) != null && (dat3 = smk25IIPadPara3.getDat()) != null) {
                    smk25IIUsrFragment3.setNum(editable, (AddrU8) getFromArray(dat3, 2), 0, 127);
                    break;
                }
                break;
            case 9:
                Smk25IIUsrFragment smk25IIUsrFragment4 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding4 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr4 = this.mData;
                if (smk25IIUsrFragment4 != null && smk25IIUsr4 != null && (padPara4 = smk25IIUsr4.getPadPara()) != null && smk25IIUsrBinding4 != null && (smk25IIPadPara4 = (Smk25IIPadPara) getFromArray(padPara4, smk25IIUsrBinding4.getPadIndex())) != null) {
                    smk25IIUsrFragment4.setNum(editable, smk25IIPadPara4.getLed(), 0, 255);
                    break;
                }
                break;
            case 10:
                Smk25IIUsrFragment smk25IIUsrFragment5 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding5 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr5 = this.mData;
                if (smk25IIUsrFragment5 != null && smk25IIUsr5 != null && (padPara5 = smk25IIUsr5.getPadPara()) != null && smk25IIUsrBinding5 != null && (smk25IIPadPara5 = (Smk25IIPadPara) getFromArray(padPara5, smk25IIUsrBinding5.getPadIndex())) != null) {
                    smk25IIUsrFragment5.setSysEx(editable, smk25IIPadPara5.getSys());
                    break;
                }
                break;
            case 14:
                Smk25IIUsrFragment smk25IIUsrFragment6 = this.mListener;
                Smk25IIUsr smk25IIUsr6 = this.mData;
                if (smk25IIUsrFragment6 != null && smk25IIUsr6 != null && (keyPara = smk25IIUsr6.getKeyPara()) != null) {
                    smk25IIUsrFragment6.setNum(editable, keyPara.getMin(), 0, 127);
                    break;
                }
                break;
            case 15:
                Smk25IIUsrFragment smk25IIUsrFragment7 = this.mListener;
                Smk25IIUsr smk25IIUsr7 = this.mData;
                if (smk25IIUsrFragment7 != null && smk25IIUsr7 != null && (keyPara2 = smk25IIUsr7.getKeyPara()) != null) {
                    smk25IIUsrFragment7.setNum(editable, keyPara2.getMax(), 0, 127);
                    break;
                }
                break;
            case 20:
                Smk25IIUsrFragment smk25IIUsrFragment8 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding6 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr8 = this.mData;
                if (smk25IIUsrFragment8 != null && smk25IIUsr8 != null && (encoderPara = smk25IIUsr8.getEncoderPara()) != null && smk25IIUsrBinding6 != null && (encoderPara2 = (EncoderPara) getFromArray(encoderPara, smk25IIUsrBinding6.getKnobIndex())) != null && (dat4 = encoderPara2.getDat()) != null) {
                    smk25IIUsrFragment8.setNum(editable, (AddrU8) getFromArray(dat4, 0), 0, 127);
                    break;
                }
                break;
            case 21:
                Smk25IIUsrFragment smk25IIUsrFragment9 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding7 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr9 = this.mData;
                if (smk25IIUsrFragment9 != null && smk25IIUsr9 != null && (encoderPara3 = smk25IIUsr9.getEncoderPara()) != null && smk25IIUsrBinding7 != null && (encoderPara4 = (EncoderPara) getFromArray(encoderPara3, smk25IIUsrBinding7.getKnobIndex())) != null && (dat5 = encoderPara4.getDat()) != null) {
                    smk25IIUsrFragment9.setNum(editable, (AddrU8) getFromArray(dat5, 1), 0, 127);
                    break;
                }
                break;
            case 22:
                Smk25IIUsrFragment smk25IIUsrFragment10 = this.mListener;
                Smk25IIUsrBinding smk25IIUsrBinding8 = this.mUsrBinding;
                Smk25IIUsr smk25IIUsr10 = this.mData;
                if (smk25IIUsrFragment10 != null && smk25IIUsr10 != null && (encoderPara5 = smk25IIUsr10.getEncoderPara()) != null && smk25IIUsrBinding8 != null && (encoderPara6 = (EncoderPara) getFromArray(encoderPara5, smk25IIUsrBinding8.getKnobIndex())) != null && (dat6 = encoderPara6.getDat()) != null) {
                    smk25IIUsrFragment10.setNum(editable, (AddrU8) getFromArray(dat6, 2), 0, 127);
                    break;
                }
                break;
            case 24:
                Smk25IIUsrFragment smk25IIUsrFragment11 = this.mListener;
                Smk25IIUsr smk25IIUsr11 = this.mData;
                if (smk25IIUsrFragment11 != null && smk25IIUsr11 != null && (modPara = smk25IIUsr11.getModPara()) != null && (dat7 = modPara.getDat()) != null) {
                    smk25IIUsrFragment11.setNum(editable, (AddrU8) getFromArray(dat7, 0), 0, 127);
                    break;
                }
                break;
            case 25:
                Smk25IIUsrFragment smk25IIUsrFragment12 = this.mListener;
                Smk25IIUsr smk25IIUsr12 = this.mData;
                if (smk25IIUsrFragment12 != null && smk25IIUsr12 != null && (modPara2 = smk25IIUsr12.getModPara()) != null && (dat8 = modPara2.getDat()) != null) {
                    smk25IIUsrFragment12.setNum(editable, (AddrU8) getFromArray(dat8, 1), 0, 127);
                    break;
                }
                break;
            case 26:
                Smk25IIUsrFragment smk25IIUsrFragment13 = this.mListener;
                Smk25IIUsr smk25IIUsr13 = this.mData;
                if (smk25IIUsrFragment13 != null && smk25IIUsr13 != null && (modPara3 = smk25IIUsr13.getModPara()) != null && (dat9 = modPara3.getDat()) != null) {
                    smk25IIUsrFragment13.setNum(editable, (AddrU8) getFromArray(dat9, 2), 0, 127);
                    break;
                }
                break;
            case 30:
                Smk25IIUsrFragment smk25IIUsrFragment14 = this.mListener;
                Smk25IIUsr smk25IIUsr14 = this.mData;
                if (smk25IIUsrFragment14 != null && smk25IIUsr14 != null && (pedalPara = smk25IIUsr14.getPedalPara()) != null && (dat10 = pedalPara.getDat()) != null) {
                    smk25IIUsrFragment14.setNum(editable, (AddrU8) getFromArray(dat10, 0), 0, 127);
                    break;
                }
                break;
            case 31:
                Smk25IIUsrFragment smk25IIUsrFragment15 = this.mListener;
                Smk25IIUsr smk25IIUsr15 = this.mData;
                if (smk25IIUsrFragment15 != null && smk25IIUsr15 != null && (pedalPara2 = smk25IIUsr15.getPedalPara()) != null && (dat11 = pedalPara2.getDat()) != null) {
                    smk25IIUsrFragment15.setNum(editable, (AddrU8) getFromArray(dat11, 1), 0, 127);
                    break;
                }
                break;
            case 32:
                Smk25IIUsrFragment smk25IIUsrFragment16 = this.mListener;
                Smk25IIUsr smk25IIUsr16 = this.mData;
                if (smk25IIUsrFragment16 != null && smk25IIUsr16 != null && (pedalPara3 = smk25IIUsr16.getPedalPara()) != null && (dat12 = pedalPara3.getDat()) != null) {
                    smk25IIUsrFragment16.setNum(editable, (AddrU8) getFromArray(dat12, 2), 0, 127);
                    break;
                }
                break;
        }
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        Smk25IIPadPara[] padPara;
        Smk25IIPadPara smk25IIPadPara;
        ModPara modPara;
        KeyPara keyPara;
        KeyPara keyPara2;
        KeyPara keyPara3;
        EncoderPara[] encoderPara;
        EncoderPara encoderPara2;
        EncoderPara[] encoderPara3;
        EncoderPara encoderPara4;
        PitchPara pitchPara;
        PedalPara pedalPara;
        PedalPara pedalPara2;
        if (i == 3) {
            Smk25IIUsrFragment smk25IIUsrFragment = this.mListener;
            if (smk25IIUsrFragment != null) {
                smk25IIUsrFragment.padTypeChange(i2);
                return;
            }
            return;
        }
        if (i == 4) {
            Smk25IIUsrFragment smk25IIUsrFragment2 = this.mListener;
            Smk25IIUsrBinding smk25IIUsrBinding = this.mUsrBinding;
            Smk25IIUsr smk25IIUsr = this.mData;
            if (smk25IIUsrFragment2 == null || smk25IIUsr == null || (padPara = smk25IIUsr.getPadPara()) == null || smk25IIUsrBinding == null || (smk25IIPadPara = (Smk25IIPadPara) getFromArray(padPara, smk25IIUsrBinding.getPadIndex())) == null) {
                return;
            }
            smk25IIUsrFragment2.U8DataChange(i2, smk25IIPadPara.getCh());
            return;
        }
        if (i != 23) {
            switch (i) {
                case 11:
                    Smk25IIUsrFragment smk25IIUsrFragment3 = this.mListener;
                    Smk25IIUsr smk25IIUsr2 = this.mData;
                    if (smk25IIUsrFragment3 != null && smk25IIUsr2 != null && (keyPara = smk25IIUsr2.getKeyPara()) != null) {
                        smk25IIUsrFragment3.U8DataChange(i2, keyPara.getCh());
                        break;
                    }
                    break;
                case 12:
                    Smk25IIUsrFragment smk25IIUsrFragment4 = this.mListener;
                    Smk25IIUsr smk25IIUsr3 = this.mData;
                    if (smk25IIUsrFragment4 != null && smk25IIUsr3 != null && (keyPara2 = smk25IIUsr3.getKeyPara()) != null) {
                        smk25IIUsrFragment4.I8DataChange(i2 - 3, keyPara2.getOctave());
                        break;
                    }
                    break;
                case 13:
                    Smk25IIUsrFragment smk25IIUsrFragment5 = this.mListener;
                    Smk25IIUsr smk25IIUsr4 = this.mData;
                    if (smk25IIUsrFragment5 != null && smk25IIUsr4 != null && (keyPara3 = smk25IIUsr4.getKeyPara()) != null) {
                        smk25IIUsrFragment5.I8DataChange(i2 - 12, keyPara3.getTranspose());
                        break;
                    }
                    break;
                default:
                    switch (i) {
                        case 17:
                            Smk25IIUsrFragment smk25IIUsrFragment6 = this.mListener;
                            if (smk25IIUsrFragment6 != null) {
                                smk25IIUsrFragment6.knobTypeChange(i2);
                            }
                            break;
                        case 18:
                            Smk25IIUsrFragment smk25IIUsrFragment7 = this.mListener;
                            Smk25IIUsrBinding smk25IIUsrBinding2 = this.mUsrBinding;
                            Smk25IIUsr smk25IIUsr5 = this.mData;
                            if (smk25IIUsrFragment7 != null && smk25IIUsr5 != null && (encoderPara = smk25IIUsr5.getEncoderPara()) != null && smk25IIUsrBinding2 != null && (encoderPara2 = (EncoderPara) getFromArray(encoderPara, smk25IIUsrBinding2.getKnobIndex())) != null) {
                                smk25IIUsrFragment7.U8DataChange(i2, encoderPara2.getCurve());
                                break;
                            }
                            break;
                        case 19:
                            Smk25IIUsrFragment smk25IIUsrFragment8 = this.mListener;
                            Smk25IIUsrBinding smk25IIUsrBinding3 = this.mUsrBinding;
                            Smk25IIUsr smk25IIUsr6 = this.mData;
                            if (smk25IIUsrFragment8 != null && smk25IIUsr6 != null && (encoderPara3 = smk25IIUsr6.getEncoderPara()) != null && smk25IIUsrBinding3 != null && (encoderPara4 = (EncoderPara) getFromArray(encoderPara3, smk25IIUsrBinding3.getKnobIndex())) != null) {
                                smk25IIUsrFragment8.U8DataChange(i2, encoderPara4.getCh());
                                break;
                            }
                            break;
                        default:
                            switch (i) {
                                case 27:
                                    Smk25IIUsrFragment smk25IIUsrFragment9 = this.mListener;
                                    Smk25IIUsr smk25IIUsr7 = this.mData;
                                    if (smk25IIUsrFragment9 != null && smk25IIUsr7 != null && (pitchPara = smk25IIUsr7.getPitchPara()) != null) {
                                        smk25IIUsrFragment9.U8DataChange(i2, pitchPara.getCh());
                                        break;
                                    }
                                    break;
                                case 28:
                                    Smk25IIUsrFragment smk25IIUsrFragment10 = this.mListener;
                                    Smk25IIUsr smk25IIUsr8 = this.mData;
                                    if (smk25IIUsrFragment10 != null && smk25IIUsr8 != null && (pedalPara = smk25IIUsr8.getPedalPara()) != null) {
                                        smk25IIUsrFragment10.U8DataChange(i2, pedalPara.getType());
                                        break;
                                    }
                                    break;
                                case 29:
                                    Smk25IIUsrFragment smk25IIUsrFragment11 = this.mListener;
                                    Smk25IIUsr smk25IIUsr9 = this.mData;
                                    if (smk25IIUsrFragment11 != null && smk25IIUsr9 != null && (pedalPara2 = smk25IIUsr9.getPedalPara()) != null) {
                                        smk25IIUsrFragment11.U8DataChange(i2, pedalPara2.getCh());
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
        Smk25IIUsrFragment smk25IIUsrFragment12 = this.mListener;
        Smk25IIUsr smk25IIUsr10 = this.mData;
        if (smk25IIUsrFragment12 == null || smk25IIUsr10 == null || (modPara = smk25IIUsr10.getModPara()) == null) {
            return;
        }
        smk25IIUsrFragment12.U8DataChange(i2, modPara.getCh());
    }

    @Override // com.cubeSuite.generated.callback.OnClickListener.Listener
    public final void _internalCallbackOnClick(int i, View view) {
        Smk25IIPadPara[] padPara;
        Smk25IIPadPara smk25IIPadPara;
        Smk25IIUsrFragment smk25IIUsrFragment = this.mListener;
        Smk25IIUsrBinding smk25IIUsrBinding = this.mUsrBinding;
        Smk25IIUsr smk25IIUsr = this.mData;
        if (smk25IIUsrFragment == null || smk25IIUsr == null || (padPara = smk25IIUsr.getPadPara()) == null || smk25IIUsrBinding == null || (smk25IIPadPara = (Smk25IIPadPara) getFromArray(padPara, smk25IIUsrBinding.getPadIndex())) == null) {
            return;
        }
        smk25IIUsrFragment.showColorDialog(smk25IIPadPara.getColor());
    }

    @Override // com.cubeSuite.generated.callback.OnProgressChanged.Listener
    public final void _internalCallbackOnProgressChanged(int i, SeekBar seekBar, int i2, boolean z) {
        Smk25IIUsrFragment smk25IIUsrFragment;
        if (i != 2) {
            if (i == 16 && (smk25IIUsrFragment = this.mListener) != null) {
                smk25IIUsrFragment.knobChange(i2);
                return;
            }
            return;
        }
        Smk25IIUsrFragment smk25IIUsrFragment2 = this.mListener;
        if (smk25IIUsrFragment2 != null) {
            smk25IIUsrFragment2.padChange(i2);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnCheckedChangeListener.Listener
    public final void _internalCallbackOnCheckedChanged(int i, RadioGroup radioGroup, int i2) {
        Smk25IIUsrFragment smk25IIUsrFragment = this.mListener;
        if (smk25IIUsrFragment != null) {
            smk25IIUsrFragment.itemSelectChange(radioGroup);
        }
    }
}
