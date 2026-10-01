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
import com.cubeSuite.entity.midikeyboard.BtnPara;
import com.cubeSuite.entity.midikeyboard.EncoderPara;
import com.cubeSuite.entity.midikeyboard.KeyPara;
import com.cubeSuite.entity.midikeyboard.ModPara;
import com.cubeSuite.entity.midikeyboard.PadPara;
import com.cubeSuite.entity.midikeyboard.PedalPara;
import com.cubeSuite.entity.midikeyboard.PitchPara;
import com.cubeSuite.entity.midikeyboard.Sys;
import com.cubeSuite.entity.midikeyboard.Usr;
import com.cubeSuite.entity.midikeyboard.UsrBinding;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardUsrFragment;
import com.cubeSuite.generated.callback.AfterTextChanged;
import com.cubeSuite.generated.callback.OnCheckedChangeListener;
import com.cubeSuite.generated.callback.OnClickListener;
import com.cubeSuite.generated.callback.OnItemSelected;
import com.cubeSuite.generated.callback.OnProgressChanged;
import kotlinx.coroutines.internal.LockFreeTaskQueueCore;
import okhttp3.internal.http2.Http2Stream;
import okio.SegmentPool;

/* JADX INFO: loaded from: classes.dex */
public class UsrFragmentBindingImpl extends UsrFragmentBinding implements AfterTextChanged.Listener, OnItemSelected.Listener, OnProgressChanged.Listener, OnCheckedChangeListener.Listener, OnClickListener.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private final TextViewBindingAdapter.AfterTextChanged mCallback100;
    private final TextViewBindingAdapter.AfterTextChanged mCallback101;
    private final SeekBarBindingAdapter.OnProgressChanged mCallback102;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback103;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback104;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback105;
    private final TextViewBindingAdapter.AfterTextChanged mCallback106;
    private final TextViewBindingAdapter.AfterTextChanged mCallback107;
    private final TextViewBindingAdapter.AfterTextChanged mCallback108;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback109;
    private final TextViewBindingAdapter.AfterTextChanged mCallback110;
    private final TextViewBindingAdapter.AfterTextChanged mCallback111;
    private final TextViewBindingAdapter.AfterTextChanged mCallback112;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback113;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback114;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback115;
    private final TextViewBindingAdapter.AfterTextChanged mCallback116;
    private final TextViewBindingAdapter.AfterTextChanged mCallback117;
    private final TextViewBindingAdapter.AfterTextChanged mCallback118;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback119;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback120;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback121;
    private final TextViewBindingAdapter.AfterTextChanged mCallback122;
    private final TextViewBindingAdapter.AfterTextChanged mCallback123;
    private final TextViewBindingAdapter.AfterTextChanged mCallback124;
    private final TextViewBindingAdapter.AfterTextChanged mCallback125;
    private final RadioGroup.OnCheckedChangeListener mCallback89;
    private final SeekBarBindingAdapter.OnProgressChanged mCallback90;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback91;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback92;
    private final TextViewBindingAdapter.AfterTextChanged mCallback93;
    private final TextViewBindingAdapter.AfterTextChanged mCallback94;
    private final TextViewBindingAdapter.AfterTextChanged mCallback95;
    private final View.OnClickListener mCallback96;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback97;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback98;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback99;
    private long mDirtyFlags;
    private long mDirtyFlags_1;
    private final ScrollView mboundView0;
    private final TextView mboundView19;
    private final TextView mboundView2;
    private final AppCompatSpinner mboundView22;
    private final AppCompatSpinner mboundView23;
    private final EditText mboundView24;
    private final AppCompatSpinner mboundView30;
    private final TextView mboundView32;
    private final AppCompatSpinner mboundView35;
    private final AppCompatSpinner mboundView36;
    private final AppCompatSpinner mboundView37;
    private final AppCompatSpinner mboundView41;
    private final AppCompatSpinner mboundView43;
    private final TextView mboundView44;
    private final EditText mboundView45;
    private final TextView mboundView47;
    private final TextView mboundView49;

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.padBtn, 52);
        sparseIntArray.put(R.id.pads, 53);
        sparseIntArray.put(R.id.channelLayout, 54);
        sparseIntArray.put(R.id.padNoteLayout, 55);
        sparseIntArray.put(R.id.minValueLayout, 56);
        sparseIntArray.put(R.id.maxValueLayout, 57);
        sparseIntArray.put(R.id.keybed, 58);
        sparseIntArray.put(R.id.knobs, 59);
        sparseIntArray.put(R.id.knobCCLayout, 60);
        sparseIntArray.put(R.id.mps, 61);
        sparseIntArray.put(R.id.ppr, 62);
        sparseIntArray.put(R.id.psrChannelLayout, 63);
        sparseIntArray.put(R.id.psrCCLayout, 64);
        sparseIntArray.put(R.id.psrValue1Layout, 65);
        sparseIntArray.put(R.id.psrValue2Layout, 66);
        sparseIntArray.put(R.id.sysExLayout, 67);
    }

    public UsrFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 68, sIncludes, sViewsWithIds));
    }

    private UsrFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 32, (AppCompatSpinner) objArr[5], (LinearLayout) objArr[54], (LinearLayout) objArr[58], (AppCompatSpinner) objArr[14], (EditText) objArr[18], (EditText) objArr[17], (LinearLayout) objArr[60], (TextView) objArr[25], (TextView) objArr[28], (EditText) objArr[29], (TextView) objArr[26], (EditText) objArr[27], (SeekBar) objArr[20], (AppCompatSpinner) objArr[21], (LinearLayout) objArr[59], (LinearLayout) objArr[57], (EditText) objArr[12], (LinearLayout) objArr[56], (EditText) objArr[10], (EditText) objArr[31], (EditText) objArr[34], (EditText) objArr[33], (LinearLayout) objArr[61], (AppCompatSpinner) objArr[15], (RadioButton) objArr[52], (TextView) objArr[11], (TextView) objArr[9], (EditText) objArr[7], (LinearLayout) objArr[55], (TextView) objArr[8], (TextView) objArr[6], (SeekBar) objArr[3], (LinearLayout) objArr[53], (LinearLayout) objArr[62], (LinearLayout) objArr[64], (TextView) objArr[46], (LinearLayout) objArr[63], (AppCompatSpinner) objArr[42], (EditText) objArr[48], (LinearLayout) objArr[65], (EditText) objArr[50], (LinearLayout) objArr[66], (Button) objArr[13], (EditText) objArr[38], (EditText) objArr[40], (EditText) objArr[39], (LinearLayout) objArr[67], (EditText) objArr[51], (AppCompatSpinner) objArr[16], (AppCompatSpinner) objArr[4], (RadioGroup) objArr[1]);
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
        this.maxVel.setTag(null);
        ScrollView scrollView = (ScrollView) objArr[0];
        this.mboundView0 = scrollView;
        scrollView.setTag(null);
        TextView textView = (TextView) objArr[19];
        this.mboundView19 = textView;
        textView.setTag(null);
        TextView textView2 = (TextView) objArr[2];
        this.mboundView2 = textView2;
        textView2.setTag(null);
        AppCompatSpinner appCompatSpinner = (AppCompatSpinner) objArr[22];
        this.mboundView22 = appCompatSpinner;
        appCompatSpinner.setTag(null);
        AppCompatSpinner appCompatSpinner2 = (AppCompatSpinner) objArr[23];
        this.mboundView23 = appCompatSpinner2;
        appCompatSpinner2.setTag(null);
        EditText editText = (EditText) objArr[24];
        this.mboundView24 = editText;
        editText.setTag(null);
        AppCompatSpinner appCompatSpinner3 = (AppCompatSpinner) objArr[30];
        this.mboundView30 = appCompatSpinner3;
        appCompatSpinner3.setTag(null);
        TextView textView3 = (TextView) objArr[32];
        this.mboundView32 = textView3;
        textView3.setTag(null);
        AppCompatSpinner appCompatSpinner4 = (AppCompatSpinner) objArr[35];
        this.mboundView35 = appCompatSpinner4;
        appCompatSpinner4.setTag(null);
        AppCompatSpinner appCompatSpinner5 = (AppCompatSpinner) objArr[36];
        this.mboundView36 = appCompatSpinner5;
        appCompatSpinner5.setTag(null);
        AppCompatSpinner appCompatSpinner6 = (AppCompatSpinner) objArr[37];
        this.mboundView37 = appCompatSpinner6;
        appCompatSpinner6.setTag(null);
        AppCompatSpinner appCompatSpinner7 = (AppCompatSpinner) objArr[41];
        this.mboundView41 = appCompatSpinner7;
        appCompatSpinner7.setTag(null);
        AppCompatSpinner appCompatSpinner8 = (AppCompatSpinner) objArr[43];
        this.mboundView43 = appCompatSpinner8;
        appCompatSpinner8.setTag(null);
        TextView textView4 = (TextView) objArr[44];
        this.mboundView44 = textView4;
        textView4.setTag(null);
        EditText editText2 = (EditText) objArr[45];
        this.mboundView45 = editText2;
        editText2.setTag(null);
        TextView textView5 = (TextView) objArr[47];
        this.mboundView47 = textView5;
        textView5.setTag(null);
        TextView textView6 = (TextView) objArr[49];
        this.mboundView49 = textView6;
        textView6.setTag(null);
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
        this.psrCCStr.setTag(null);
        this.psrMode.setTag(null);
        this.psrValue1.setTag(null);
        this.psrValue2.setTag(null);
        this.selectColorBtn.setTag(null);
        this.sustainCCVal.setTag(null);
        this.sustainDownVal.setTag(null);
        this.sustainUpVal.setTag(null);
        this.sysExValue.setTag(null);
        this.transpose.setTag(null);
        this.type.setTag(null);
        this.typeRadio.setTag(null);
        setRootTag(view);
        this.mCallback94 = new AfterTextChanged(this, 6);
        this.mCallback108 = new AfterTextChanged(this, 20);
        this.mCallback95 = new AfterTextChanged(this, 7);
        this.mCallback109 = new OnItemSelected(this, 21);
        this.mCallback106 = new AfterTextChanged(this, 18);
        this.mCallback118 = new AfterTextChanged(this, 30);
        this.mCallback92 = new OnItemSelected(this, 4);
        this.mCallback107 = new AfterTextChanged(this, 19);
        this.mCallback93 = new AfterTextChanged(this, 5);
        this.mCallback119 = new OnItemSelected(this, 31);
        this.mCallback120 = new OnItemSelected(this, 32);
        this.mCallback90 = new OnProgressChanged(this, 2);
        this.mCallback121 = new OnItemSelected(this, 33);
        this.mCallback91 = new OnItemSelected(this, 3);
        this.mCallback124 = new AfterTextChanged(this, 36);
        this.mCallback100 = new AfterTextChanged(this, 12);
        this.mCallback112 = new AfterTextChanged(this, 24);
        this.mCallback113 = new OnItemSelected(this, 25);
        this.mCallback125 = new AfterTextChanged(this, 37);
        this.mCallback101 = new AfterTextChanged(this, 13);
        this.mCallback89 = new OnCheckedChangeListener(this, 1);
        this.mCallback122 = new AfterTextChanged(this, 34);
        this.mCallback110 = new AfterTextChanged(this, 22);
        this.mCallback111 = new AfterTextChanged(this, 23);
        this.mCallback123 = new AfterTextChanged(this, 35);
        this.mCallback104 = new OnItemSelected(this, 16);
        this.mCallback116 = new AfterTextChanged(this, 28);
        this.mCallback99 = new OnItemSelected(this, 11);
        this.mCallback117 = new AfterTextChanged(this, 29);
        this.mCallback105 = new OnItemSelected(this, 17);
        this.mCallback102 = new OnProgressChanged(this, 14);
        this.mCallback114 = new OnItemSelected(this, 26);
        this.mCallback96 = new OnClickListener(this, 8);
        this.mCallback115 = new OnItemSelected(this, 27);
        this.mCallback103 = new OnItemSelected(this, 15);
        this.mCallback98 = new OnItemSelected(this, 10);
        this.mCallback97 = new OnItemSelected(this, 9);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 0L;
            this.mDirtyFlags_1 = SegmentPool.MAX_SIZE;
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
            setListener((MidikeyboardUsrFragment) obj);
            return true;
        }
        if (2 == i) {
            setData((Usr) obj);
            return true;
        }
        if (21 != i) {
            return false;
        }
        setUsrBinding((UsrBinding) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.UsrFragmentBinding
    public void setListener(MidikeyboardUsrFragment midikeyboardUsrFragment) {
        this.mListener = midikeyboardUsrFragment;
        synchronized (this) {
            this.mDirtyFlags |= 4294967296L;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.UsrFragmentBinding
    public void setData(Usr usr) {
        this.mData = usr;
        synchronized (this) {
            this.mDirtyFlags |= 8589934592L;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.UsrFragmentBinding
    public void setUsrBinding(UsrBinding usrBinding) {
        updateRegistration(31, usrBinding);
        this.mUsrBinding = usrBinding;
        synchronized (this) {
            this.mDirtyFlags |= 2147483648L;
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
                return onChangeDataBtnParaUsrBindingPsrIndexType((AddrU8) obj, i2);
            case 4:
                return onChangeDataBtnParaUsrBindingPsrIndexDat1((AddrU8) obj, i2);
            case 5:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat2((AddrU8) obj, i2);
            case 6:
                return onChangeDataPadParaUsrBindingPadIndexDat2((AddrU8) obj, i2);
            case 7:
                return onChangeDataPadParaUsrBindingPadIndexDat0((AddrU8) obj, i2);
            case 8:
                return onChangeDataPedalParaDat2((AddrU8) obj, i2);
            case 9:
                return onChangeDataKeyParaMin((AddrU8) obj, i2);
            case 10:
                return onChangeDataKeyParaCh((AddrU8) obj, i2);
            case 11:
                return onChangeDataPadParaUsrBindingPadIndexCh((AddrU8) obj, i2);
            case 12:
                return onChangeDataModParaDat1((AddrU8) obj, i2);
            case 13:
                return onChangeDataBtnParaUsrBindingPsrIndexDat2((AddrU8) obj, i2);
            case 14:
                return onChangeDataPadParaUsrBindingPadIndexDat1((AddrU8) obj, i2);
            case 15:
                return onChangeDataEncoderParaUsrBindingKnobIndexType((AddrU8) obj, i2);
            case 16:
                return onChangeDataKeyParaOctave((AddrI8) obj, i2);
            case 17:
                return onChangeDataPadParaUsrBindingPadIndexType((AddrU8) obj, i2);
            case 18:
                return onChangeDataPedalParaDat1((AddrU8) obj, i2);
            case 19:
                return onChangeDataModParaDat0((AddrU8) obj, i2);
            case 20:
                return onChangeDataBtnParaUsrBindingPsrIndexCh((AddrU8) obj, i2);
            case 21:
                return onChangeDataModParaCh((AddrU8) obj, i2);
            case 22:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat0((AddrU8) obj, i2);
            case 23:
                return onChangeDataEncoderParaUsrBindingKnobIndexCurve((AddrU8) obj, i2);
            case 24:
                return onChangeDataBtnParaUsrBindingPsrIndexDat0((AddrU8) obj, i2);
            case 25:
                return onChangeDataEncoderParaUsrBindingKnobIndexDat1((AddrU8) obj, i2);
            case 26:
                return onChangeDataPedalParaType((AddrU8) obj, i2);
            case 27:
                return onChangeDataKeyParaTranspose((AddrI8) obj, i2);
            case 28:
                return onChangeDataPedalParaDat0((AddrU8) obj, i2);
            case 29:
                return onChangeDataPitchParaCh((AddrU8) obj, i2);
            case 30:
                return onChangeDataPedalParaCh((AddrU8) obj, i2);
            case 31:
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

    private boolean onChangeDataBtnParaUsrBindingPsrIndexType(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataBtnParaUsrBindingPsrIndexDat1(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat2(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPadParaUsrBindingPadIndexDat2(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPadParaUsrBindingPadIndexDat0(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataKeyParaCh(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPadParaUsrBindingPadIndexCh(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataModParaDat1(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataBtnParaUsrBindingPsrIndexDat2(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPadParaUsrBindingPadIndexDat1(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexType(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataKeyParaOctave(AddrI8 addrI8, int i) {
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

    private boolean onChangeDataPadParaUsrBindingPadIndexType(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPedalParaDat1(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataModParaDat0(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataBtnParaUsrBindingPsrIndexCh(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataModParaCh(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat0(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexCurve(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataBtnParaUsrBindingPsrIndexDat0(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataEncoderParaUsrBindingKnobIndexDat1(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPedalParaType(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataKeyParaTranspose(AddrI8 addrI8, int i) {
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

    private boolean onChangeDataPedalParaDat0(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPitchParaCh(AddrU8 addrU8, int i) {
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

    private boolean onChangeDataPedalParaCh(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1073741824;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags_1 |= 1;
        }
        return true;
    }

    private boolean onChangeUsrBinding(UsrBinding usrBinding, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 2147483648L;
            }
            return true;
        }
        if (i == 11) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 2;
            }
            return true;
        }
        if (i == 15) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 4;
            }
            return true;
        }
        if (i == 14) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 8;
            }
            return true;
        }
        if (i == 13) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 16;
            }
            return true;
        }
        if (i == 12) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 32;
            }
            return true;
        }
        if (i == 6) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 64;
            }
            return true;
        }
        if (i == 5) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 128;
            }
            return true;
        }
        if (i == 8) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 256;
            }
            return true;
        }
        if (i == 7) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 512;
            }
            return true;
        }
        if (i == 10) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 1024;
            }
            return true;
        }
        if (i == 18) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 2048;
            }
            return true;
        }
        if (i == 17) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 4096;
            }
            return true;
        }
        if (i == 16) {
            synchronized (this) {
                this.mDirtyFlags_1 |= 8192;
            }
            return true;
        }
        if (i == 19) {
            synchronized (this) {
                this.mDirtyFlags_1 |= Http2Stream.EMIT_BUFFER_SIZE;
            }
            return true;
        }
        if (i != 20) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags_1 |= 32768;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:101:0x01a3  */
    /* JADX WARN: Code duplicated, block: B:151:0x026a A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:152:0x026c  */
    /* JADX WARN: Code duplicated, block: B:153:0x0271  */
    /* JADX WARN: Code duplicated, block: B:156:0x027e  */
    /* JADX WARN: Code duplicated, block: B:160:0x028c  */
    /* JADX WARN: Code duplicated, block: B:162:0x0290  */
    /* JADX WARN: Code duplicated, block: B:163:0x029a  */
    /* JADX WARN: Code duplicated, block: B:166:0x02a5  */
    /* JADX WARN: Code duplicated, block: B:167:0x02aa  */
    /* JADX WARN: Code duplicated, block: B:171:0x02ba  */
    /* JADX WARN: Code duplicated, block: B:175:0x02c6 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:176:0x02c8  */
    /* JADX WARN: Code duplicated, block: B:177:0x02d2  */
    /* JADX WARN: Code duplicated, block: B:180:0x02dd  */
    /* JADX WARN: Code duplicated, block: B:181:0x02e2  */
    /* JADX WARN: Code duplicated, block: B:185:0x02f2  */
    /* JADX WARN: Code duplicated, block: B:190:0x02ff A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:191:0x0301  */
    /* JADX WARN: Code duplicated, block: B:192:0x0309  */
    /* JADX WARN: Code duplicated, block: B:195:0x0312  */
    /* JADX WARN: Code duplicated, block: B:196:0x0317  */
    /* JADX WARN: Code duplicated, block: B:204:0x0336 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:205:0x0338  */
    /* JADX WARN: Code duplicated, block: B:206:0x0343  */
    /* JADX WARN: Code duplicated, block: B:209:0x0350  */
    /* JADX WARN: Code duplicated, block: B:210:0x035c A[PHI: r42 r47
  0x035c: PHI (r42v10 java.lang.String) = (r2v24 java.lang.String), (r42v12 java.lang.String) binds: [B:208:0x034e, B:203:0x0331] A[DONT_GENERATE, DONT_INLINE]
  0x035c: PHI (r47v13 java.lang.String) = (r47v11 java.lang.String), (r47v16 java.lang.String) binds: [B:208:0x034e, B:203:0x0331] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:286:0x04b1 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:287:0x04b3  */
    /* JADX WARN: Code duplicated, block: B:288:0x04b8  */
    /* JADX WARN: Code duplicated, block: B:291:0x04c3  */
    /* JADX WARN: Code duplicated, block: B:292:0x04e8 A[PHI: r51
  0x04e8: PHI (r51v9 java.lang.String) = (r51v8 java.lang.String), (r51v12 java.lang.String) binds: [B:290:0x04c1, B:285:0x04ae] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:332:0x0611  */
    /* JADX WARN: Code duplicated, block: B:334:0x0617 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:335:0x0619  */
    /* JADX WARN: Code duplicated, block: B:336:0x061e  */
    /* JADX WARN: Code duplicated, block: B:338:0x0622  */
    /* JADX WARN: Code duplicated, block: B:339:0x0627  */
    /* JADX WARN: Code duplicated, block: B:346:0x063e A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:347:0x0640  */
    /* JADX WARN: Code duplicated, block: B:348:0x0645  */
    /* JADX WARN: Code duplicated, block: B:351:0x0650  */
    /* JADX WARN: Code duplicated, block: B:352:0x0655 A[PHI: r72
  0x0655: PHI (r72v4 java.lang.String) = (r72v2 java.lang.String), (r72v6 java.lang.String) binds: [B:350:0x064e, B:345:0x063b] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:355:0x0661  */
    /* JADX WARN: Code duplicated, block: B:360:0x0674 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:361:0x0676  */
    /* JADX WARN: Code duplicated, block: B:362:0x067b  */
    /* JADX WARN: Code duplicated, block: B:365:0x0688  */
    /* JADX WARN: Code duplicated, block: B:369:0x0696  */
    /* JADX WARN: Code duplicated, block: B:371:0x069a  */
    /* JADX WARN: Code duplicated, block: B:372:0x06a4  */
    /* JADX WARN: Code duplicated, block: B:375:0x06ae  */
    /* JADX WARN: Code duplicated, block: B:376:0x06b3  */
    /* JADX WARN: Code duplicated, block: B:380:0x06c3  */
    /* JADX WARN: Code duplicated, block: B:384:0x06cf A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:385:0x06d1  */
    /* JADX WARN: Code duplicated, block: B:386:0x06db  */
    /* JADX WARN: Code duplicated, block: B:389:0x06e6  */
    /* JADX WARN: Code duplicated, block: B:390:0x06eb  */
    /* JADX WARN: Code duplicated, block: B:394:0x06f6  */
    /* JADX WARN: Code duplicated, block: B:400:0x0709  */
    /* JADX WARN: Code duplicated, block: B:401:0x0711  */
    /* JADX WARN: Code duplicated, block: B:404:0x071a  */
    /* JADX WARN: Code duplicated, block: B:405:0x071f  */
    /* JADX WARN: Code duplicated, block: B:407:0x0722  */
    /* JADX WARN: Code duplicated, block: B:411:0x072d  */
    /* JADX WARN: Code duplicated, block: B:487:0x084d A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:488:0x084f  */
    /* JADX WARN: Code duplicated, block: B:489:0x0854  */
    /* JADX WARN: Code duplicated, block: B:492:0x085f  */
    /* JADX WARN: Code duplicated, block: B:493:0x0864 A[PHI: r85
  0x0864: PHI (r85v4 int) = (r85v2 int), (r85v6 int) binds: [B:491:0x085d, B:486:0x084a] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:496:0x0870  */
    /* JADX WARN: Code duplicated, block: B:500:0x0883 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:501:0x0885  */
    /* JADX WARN: Code duplicated, block: B:502:0x088a  */
    /* JADX WARN: Code duplicated, block: B:505:0x0897  */
    /* JADX WARN: Code duplicated, block: B:509:0x08a5  */
    /* JADX WARN: Code duplicated, block: B:511:0x08a9  */
    /* JADX WARN: Code duplicated, block: B:512:0x08b3  */
    /* JADX WARN: Code duplicated, block: B:515:0x08bd  */
    /* JADX WARN: Code duplicated, block: B:516:0x08c2  */
    /* JADX WARN: Code duplicated, block: B:519:0x08c9  */
    /* JADX WARN: Code duplicated, block: B:523:0x08d6 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:524:0x08d8  */
    /* JADX WARN: Code duplicated, block: B:525:0x08e2  */
    /* JADX WARN: Code duplicated, block: B:528:0x08ed  */
    /* JADX WARN: Code duplicated, block: B:529:0x08f2  */
    /* JADX WARN: Code duplicated, block: B:531:0x08f5  */
    /* JADX WARN: Code duplicated, block: B:535:0x08ff  */
    /* JADX WARN: Code duplicated, block: B:537:0x0905  */
    /* JADX WARN: Code duplicated, block: B:538:0x090c  */
    /* JADX WARN: Code duplicated, block: B:541:0x0919  */
    /* JADX WARN: Code duplicated, block: B:545:0x0923 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:546:0x0925  */
    /* JADX WARN: Code duplicated, block: B:547:0x092d  */
    /* JADX WARN: Code duplicated, block: B:550:0x0936  */
    /* JADX WARN: Code duplicated, block: B:551:0x093b  */
    /* JADX WARN: Code duplicated, block: B:555:0x094b  */
    /* JADX WARN: Code duplicated, block: B:559:0x0955 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:560:0x0957  */
    /* JADX WARN: Code duplicated, block: B:561:0x095c  */
    /* JADX WARN: Code duplicated, block: B:564:0x0967  */
    /* JADX WARN: Code duplicated, block: B:565:0x096f A[PHI: r79
  0x096f: PHI (r79v4 java.lang.String) = (r79v3 java.lang.String), (r79v5 java.lang.String) binds: [B:563:0x0965, B:558:0x0952] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:59:0x0120  */
    /* JADX WARN: Code duplicated, block: B:603:0x0a16 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:604:0x0a18  */
    /* JADX WARN: Code duplicated, block: B:605:0x0a1d  */
    /* JADX WARN: Code duplicated, block: B:608:0x0a2a  */
    /* JADX WARN: Code duplicated, block: B:612:0x0a38  */
    /* JADX WARN: Code duplicated, block: B:614:0x0a3c  */
    /* JADX WARN: Code duplicated, block: B:615:0x0a44  */
    /* JADX WARN: Code duplicated, block: B:618:0x0a4e  */
    /* JADX WARN: Code duplicated, block: B:619:0x0a53  */
    /* JADX WARN: Code duplicated, block: B:623:0x0a5e  */
    /* JADX WARN: Code duplicated, block: B:627:0x0a6b  */
    /* JADX WARN: Code duplicated, block: B:629:0x0a6f  */
    /* JADX WARN: Code duplicated, block: B:630:0x0a79  */
    /* JADX WARN: Code duplicated, block: B:633:0x0a83  */
    /* JADX WARN: Code duplicated, block: B:634:0x0a88  */
    /* JADX WARN: Code duplicated, block: B:636:0x0a8b  */
    /* JADX WARN: Code duplicated, block: B:640:0x0a95  */
    /* JADX WARN: Code duplicated, block: B:643:0x0aa4  */
    /* JADX WARN: Code duplicated, block: B:648:0x0ab2  */
    /* JADX WARN: Code duplicated, block: B:650:0x0ab6  */
    /* JADX WARN: Code duplicated, block: B:651:0x0abe  */
    /* JADX WARN: Code duplicated, block: B:654:0x0ac7  */
    /* JADX WARN: Code duplicated, block: B:655:0x0acc  */
    /* JADX WARN: Code duplicated, block: B:663:0x0ae8 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:664:0x0aea  */
    /* JADX WARN: Code duplicated, block: B:665:0x0aef  */
    /* JADX WARN: Code duplicated, block: B:668:0x0afa  */
    /* JADX WARN: Code duplicated, block: B:669:0x0b05 A[PHI: r38
  0x0b05: PHI (r38v5 java.lang.String) = (r38v4 java.lang.String), (r38v7 java.lang.String) binds: [B:667:0x0af8, B:662:0x0ae5] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:686:0x0b43 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:687:0x0b45  */
    /* JADX WARN: Code duplicated, block: B:688:0x0b4a  */
    /* JADX WARN: Code duplicated, block: B:690:0x0b4e  */
    /* JADX WARN: Code duplicated, block: B:691:0x0b55  */
    /* JADX WARN: Code duplicated, block: B:696:0x0b63 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:697:0x0b65  */
    /* JADX WARN: Code duplicated, block: B:698:0x0b6a  */
    /* JADX WARN: Code duplicated, block: B:700:0x0b6e  */
    /* JADX WARN: Code duplicated, block: B:701:0x0b75  */
    /* JADX WARN: Code duplicated, block: B:703:0x0b79  */
    /* JADX WARN: Code duplicated, block: B:705:0x0b82 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:706:0x0b84  */
    /* JADX WARN: Code duplicated, block: B:707:0x0b89  */
    /* JADX WARN: Instruction removed from duplicated block: B:352:0x0655, please report this as an issue */
    /* JADX WARN: Instruction removed from duplicated block: B:493:0x0864, please report this as an issue */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r0v100 */
    /* JADX WARN: Type inference failed for: r0v108 */
    /* JADX WARN: Type inference failed for: r0v139 */
    /* JADX WARN: Type inference failed for: r0v14 */
    /* JADX WARN: Type inference failed for: r0v140 */
    /* JADX WARN: Type inference failed for: r0v36 */
    /* JADX WARN: Type inference failed for: r0v37, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r0v52 */
    /* JADX WARN: Type inference failed for: r0v53, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v55 */
    /* JADX WARN: Type inference failed for: r0v57 */
    /* JADX WARN: Type inference failed for: r0v58, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v67 */
    /* JADX WARN: Type inference failed for: r0v68, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v7 */
    /* JADX WARN: Type inference failed for: r0v73 */
    /* JADX WARN: Type inference failed for: r0v75 */
    /* JADX WARN: Type inference failed for: r0v76, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v8, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r0v96 */
    /* JADX WARN: Type inference failed for: r0v97, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r111v0, types: [com.cubeSuite.databinding.UsrFragmentBindingImpl] */
    /* JADX WARN: Type inference failed for: r11v29 */
    /* JADX WARN: Type inference failed for: r11v4 */
    /* JADX WARN: Type inference failed for: r11v5, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrI8] */
    /* JADX WARN: Type inference failed for: r13v13, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r13v14 */
    /* JADX WARN: Type inference failed for: r13v15, types: [com.cubeSuite.entity.midikeyboard.BtnPara] */
    /* JADX WARN: Type inference failed for: r13v21 */
    /* JADX WARN: Type inference failed for: r13v22, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r13v25 */
    /* JADX WARN: Type inference failed for: r13v26, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r13v27 */
    /* JADX WARN: Type inference failed for: r13v28, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r13v43 */
    /* JADX WARN: Type inference failed for: r13v44, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r13v47 */
    /* JADX WARN: Type inference failed for: r13v48, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r13v65 */
    /* JADX WARN: Type inference failed for: r13v66 */
    /* JADX WARN: Type inference failed for: r13v67 */
    /* JADX WARN: Type inference failed for: r13v68 */
    /* JADX WARN: Type inference failed for: r13v69 */
    /* JADX WARN: Type inference failed for: r13v70 */
    /* JADX WARN: Type inference failed for: r13v71 */
    /* JADX WARN: Type inference failed for: r13v72 */
    /* JADX WARN: Type inference failed for: r15v10, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrI8] */
    /* JADX WARN: Type inference failed for: r15v19 */
    /* JADX WARN: Type inference failed for: r15v20, types: [com.cubeSuite.entity.midikeyboard.PitchPara] */
    /* JADX WARN: Type inference failed for: r15v21 */
    /* JADX WARN: Type inference failed for: r15v22, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r15v42 */
    /* JADX WARN: Type inference failed for: r15v43 */
    /* JADX WARN: Type inference failed for: r15v44 */
    /* JADX WARN: Type inference failed for: r15v45 */
    /* JADX WARN: Type inference failed for: r15v5 */
    /* JADX WARN: Type inference failed for: r15v6, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r15v9 */
    /* JADX WARN: Type inference failed for: r2v1 */
    /* JADX WARN: Type inference failed for: r2v102 */
    /* JADX WARN: Type inference failed for: r2v128 */
    /* JADX WARN: Type inference failed for: r2v129, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r2v13 */
    /* JADX WARN: Type inference failed for: r2v2, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v225 */
    /* JADX WARN: Type inference failed for: r2v226 */
    /* JADX WARN: Type inference failed for: r2v227 */
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
    /* JADX WARN: Type inference failed for: r2v62 */
    /* JADX WARN: Type inference failed for: r2v63, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v65 */
    /* JADX WARN: Type inference failed for: r2v69 */
    /* JADX WARN: Type inference failed for: r2v7, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v70, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r2v72 */
    /* JADX WARN: Type inference failed for: r2v87 */
    /* JADX WARN: Type inference failed for: r2v88, types: [com.cubeSuite.entity.midikeyboard.EncoderPara] */
    /* JADX WARN: Type inference failed for: r2v95 */
    /* JADX WARN: Type inference failed for: r2v96, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v186 */
    /* JADX WARN: Type inference failed for: r3v187 */
    /* JADX WARN: Type inference failed for: r3v2 */
    /* JADX WARN: Type inference failed for: r3v24 */
    /* JADX WARN: Type inference failed for: r3v25, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v3, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r3v31 */
    /* JADX WARN: Type inference failed for: r3v4 */
    /* JADX WARN: Type inference failed for: r3v5, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v52 */
    /* JADX WARN: Type inference failed for: r3v53, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r3v59 */
    /* JADX WARN: Type inference failed for: r42v3 */
    /* JADX WARN: Type inference failed for: r42v30 */
    /* JADX WARN: Type inference failed for: r42v4, types: [com.cubeSuite.entity.midikeyboard.PedalPara] */
    /* JADX WARN: Type inference failed for: r4v10 */
    /* JADX WARN: Type inference failed for: r4v24 */
    /* JADX WARN: Type inference failed for: r4v25, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r4v27 */
    /* JADX WARN: Type inference failed for: r4v7 */
    /* JADX WARN: Type inference failed for: r4v8, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r5v20, types: [androidx.databinding.InverseBindingListener, androidx.databinding.adapters.AdapterViewBindingAdapter$OnNothingSelected, androidx.databinding.adapters.SeekBarBindingAdapter$OnStartTrackingTouch, androidx.databinding.adapters.SeekBarBindingAdapter$OnStopTrackingTouch, androidx.databinding.adapters.TextViewBindingAdapter$BeforeTextChanged, androidx.databinding.adapters.TextViewBindingAdapter$OnTextChanged] */
    /* JADX WARN: Type inference failed for: r6v1 */
    /* JADX WARN: Type inference failed for: r6v2, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r6v24 */
    /* JADX WARN: Type inference failed for: r6v25 */
    /* JADX WARN: Type inference failed for: r6v26 */
    /* JADX WARN: Type inference failed for: r6v3 */
    /* JADX WARN: Type inference failed for: r6v4, types: [com.cubeSuite.entity.midikeyboard.PadPara] */
    /* JADX WARN: Type inference failed for: r6v5 */
    /* JADX WARN: Type inference failed for: r6v6, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r71v0 */
    /* JADX WARN: Type inference failed for: r71v10 */
    /* JADX WARN: Type inference failed for: r71v11 */
    /* JADX WARN: Type inference failed for: r71v19 */
    /* JADX WARN: Type inference failed for: r71v2, types: [com.cubeSuite.entity.midikeyboard.BtnPara] */
    /* JADX WARN: Type inference failed for: r71v20 */
    /* JADX WARN: Type inference failed for: r71v21 */
    /* JADX WARN: Type inference failed for: r7v0 */
    /* JADX WARN: Type inference failed for: r7v1, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r7v11 */
    /* JADX WARN: Type inference failed for: r7v12, types: [java.lang.Object[]] */
    /* JADX WARN: Type inference failed for: r7v13 */
    /* JADX WARN: Type inference failed for: r7v14, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r7v19 */
    /* JADX WARN: Type inference failed for: r7v20, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r7v3 */
    /* JADX WARN: Type inference failed for: r7v34 */
    /* JADX WARN: Type inference failed for: r7v35 */
    /* JADX WARN: Type inference failed for: r7v36 */
    /* JADX WARN: Type inference failed for: r7v37 */
    /* JADX WARN: Type inference failed for: r7v38 */
    /* JADX WARN: Type inference failed for: r7v39 */
    /* JADX WARN: Type inference failed for: r7v4, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r7v7 */
    /* JADX WARN: Type inference failed for: r7v8, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    /* JADX WARN: Type inference failed for: r84v14 */
    /* JADX WARN: Type inference failed for: r84v15 */
    /* JADX WARN: Type inference failed for: r84v16 */
    /* JADX WARN: Type inference failed for: r84v2 */
    /* JADX WARN: Type inference failed for: r84v3, types: [com.cubeSuite.entity.midikeyboard.EncoderPara] */
    /* JADX WARN: Type inference failed for: r84v4 */
    /* JADX WARN: Type inference failed for: r84v6 */
    /* JADX WARN: Type inference failed for: r8v1 */
    /* JADX WARN: Type inference failed for: r8v2, types: [com.cubeSuite.entity.midikeyboard.ModPara] */
    /* JADX WARN: Type inference failed for: r8v20 */
    /* JADX WARN: Type inference failed for: r8v21 */
    /* JADX WARN: Type inference failed for: r8v3 */
    /* JADX WARN: Type inference failed for: r8v4, types: [androidx.databinding.Observable, com.cubeSuite.entity.addrData.AddrU8] */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        long j3;
        AddrU8 max;
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
        String str9;
        Object obj;
        String str10;
        String str11;
        int i2;
        int i3;
        String str12;
        ?? r6;
        ?? r7;
        long j5;
        ?? r13;
        String knobMinText;
        int psrIndex;
        ?? r71;
        ?? r72;
        int data10;
        Sys sys;
        String sysEx;
        ?? ch4;
        String str13;
        int data11;
        ?? dat2;
        ?? r0;
        int data12;
        String strValueOf9;
        ?? r5;
        int data13;
        String strValueOf10;
        long j6;
        String str14;
        ?? r14;
        int data14;
        String strValueOf11;
        String str15;
        ?? r73;
        String padNoteText;
        String knobMaxText;
        String psrCCText;
        String psrValue1Text;
        String padMaxText;
        String strValueOf12;
        int data15;
        ?? r84;
        ?? type2;
        int i4;
        int data16;
        ?? dat3;
        ?? r8;
        int data17;
        String strValueOf13;
        ?? r9;
        String str16;
        int data18;
        String strValueOf14;
        String str17;
        ?? r10;
        int data19;
        String strValueOf15;
        ?? curve;
        String str18;
        int data20;
        String str19;
        ?? r85;
        int padIndex;
        String strValueOf16;
        String str20;
        int data21;
        ?? dat4;
        ?? r1;
        int data22;
        String strValueOf17;
        long j7;
        String str21;
        ?? r11;
        int data23;
        String strValueOf18;
        ?? r12;
        int data24;
        String strValueOf19;
        String str22;
        ?? type3;
        String str23;
        int data25;
        String str24;
        int i5;
        String str25;
        ?? r16;
        String str26;
        String[] modCCStr;
        ?? r17;
        String str27;
        String[] padNoteStr;
        String psrValue2Text;
        String padMinText;
        String str28;
        int i6;
        String str29;
        int i7;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
            j2 = this.mDirtyFlags_1;
            this.mDirtyFlags_1 = 0L;
        }
        MidikeyboardUsrFragment midikeyboardUsrFragment = this.mListener;
        Usr usr = this.mData;
        UsrBinding usrBinding = this.mUsrBinding;
        int i8 = ((15032385536L & j) > 0L ? 1 : ((15032385536L & j) == 0L ? 0 : -1));
        if (j == 0 && (j2 & 77003) == 0) {
            j4 = j;
            j5 = j2;
            usrBinding = usrBinding;
            i8 = i8;
            j3 = 77309411332L;
            str12 = null;
            str7 = null;
            strValueOf = null;
            i3 = 0;
            r6 = 0;
            r7 = 0;
            data4 = 0;
            strValueOf2 = null;
            data = 0;
            data5 = 0;
            i2 = 0;
            r13 = 0;
            data9 = 0;
            str9 = null;
            str = null;
            data3 = 0;
            str11 = null;
            str10 = null;
            strValueOf5 = null;
            data2 = 0;
        } else {
            if ((j & 2306995374843299332L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                j3 = 77309411332L;
                data2 = 0;
                data3 = 0;
                data = 0;
                strValueOf2 = null;
                strValueOf = null;
                str = null;
            } else {
                KeyPara keyPara = usr != null ? usr.getKeyPara() : null;
                if ((j & 77309411332L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    j3 = 77309411332L;
                    strValueOf = null;
                } else {
                    if (keyPara != null) {
                        max = keyPara.getMax();
                        j3 = 77309411332L;
                    } else {
                        j3 = 77309411332L;
                        max = null;
                    }
                    updateRegistration(2, max);
                    strValueOf = String.valueOf(max != null ? max.getData() : 0);
                }
                if ((j & 8804682957312L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    strValueOf2 = null;
                    str = null;
                } else {
                    AddrU8 min = keyPara != null ? keyPara.getMin() : null;
                    str = null;
                    updateRegistration(9, min);
                    strValueOf2 = String.valueOf(min != null ? min.getData() : 0);
                }
                if ((j & 17600775980032L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    data = 0;
                } else {
                    ?? ch5 = keyPara != null ? keyPara.getCh() : str;
                    updateRegistration(10, ch5);
                    if (ch5 != 0) {
                        data = ch5.getData();
                    } else {
                        data = 0;
                    }
                }
                if ((j & 1125908496842752L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    data2 = 0;
                } else {
                    ?? octave = keyPara != null ? keyPara.getOctave() : str;
                    updateRegistration(16, octave);
                    data2 = (octave != 0 ? octave.getData() : (byte) 0) + 3;
                }
                if ((j & 2305843017937846272L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    data3 = 0;
                } else {
                    ?? transpose = keyPara != null ? keyPara.getTranspose() : str;
                    updateRegistration(27, transpose);
                    data3 = (transpose != 0 ? transpose.getData() : (byte) 0) + 12;
                }
            }
            if ((j & (-9223372027727970304L)) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                data4 = 0;
            } else {
                if (usr != null) {
                    pitchPara = usr.getPitchPara();
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
                updateRegistration(29, ch);
                if (ch != 0) {
                    data4 = ch.getData();
                } else {
                    data4 = 0;
                }
            }
            Object btnPara = (((j & 306385939516629016L) == 0 && (j2 & 75776) == 0) || usr == null) ? str : usr.getBtnPara();
            Object padPara = (((j & 2571768434936000L) == 0 && (j2 & 65546) == 0) || usr == null) ? str : usr.getPadPara();
            Object encoderPara = (((j & 793197079269769250L) == 0 && (j2 & 65728) == 0) || usr == null) ? str : usr.getEncoderPara();
            if ((j & 5769115530707599616L) == 0 && (j2 & 65537) == 0) {
                j4 = j;
                data4 = data4;
                str4 = str;
                str5 = str4;
                strValueOf5 = str5;
                data9 = 0;
                data5 = 0;
            } else {
                ?? pedalPara = usr != null ? usr.getPedalPara() : str;
                if ((j & 1152921513263890432L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                    j4 = j;
                } else {
                    if (pedalPara != 0) {
                        type = pedalPara.getType();
                        j4 = j;
                    } else {
                        j4 = j;
                        type = str;
                    }
                    updateRegistration(26, type);
                    data5 = type != 0 ? type.getData() : 0;
                    if ((j4 & 4616194024959901952L) == 0 || (j2 & SegmentPool.MAX_SIZE) != 0) {
                        if (pedalPara != 0) {
                            dat = pedalPara.getDat();
                        } else {
                            dat = str;
                        }
                        if ((j4 & 4406636445952L) == 0 || (j2 & SegmentPool.MAX_SIZE) != 0) {
                            if (dat != 0) {
                                r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                            } else {
                                r2 = str;
                            }
                            updateRegistration(8, r2);
                            if (r2 != 0) {
                                data6 = r2.getData();
                            } else {
                                data6 = 0;
                            }
                            strValueOf3 = String.valueOf(data6);
                        } else {
                            strValueOf3 = str;
                        }
                        if ((j4 & 4503608217567232L) == 0 || (j2 & SegmentPool.MAX_SIZE) != 0) {
                            if (dat != 0) {
                                r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                            } else {
                                r4 = str;
                            }
                            updateRegistration(18, r4);
                            if (r4 != 0) {
                                data7 = r4.getData();
                            } else {
                                data7 = 0;
                            }
                            strValueOf4 = String.valueOf(data7);
                        } else {
                            strValueOf4 = str;
                        }
                        if ((j4 & 4611686027285757952L) == 0 || (j2 & SegmentPool.MAX_SIZE) != 0) {
                            if (dat != 0) {
                                r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                            } else {
                                r3 = str;
                            }
                            updateRegistration(28, r3);
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
                    if ((j4 & 9663676416L) == 0 || (j2 & 65537) != 0) {
                        if (pedalPara != 0) {
                            ch2 = pedalPara.getCh();
                        } else {
                            ch2 = str;
                        }
                        str3 = strValueOf4;
                        updateRegistration(30, ch2);
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
                if ((j4 & 4616194024959901952L) == 0) {
                    if (pedalPara != 0) {
                        dat = pedalPara.getDat();
                    } else {
                        dat = str;
                    }
                    if ((j4 & 4406636445952L) == 0) {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(8, r2);
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
                        updateRegistration(8, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    }
                    if ((j4 & 4503608217567232L) == 0) {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(18, r4);
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
                        updateRegistration(18, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    }
                    if ((j4 & 4611686027285757952L) == 0) {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(28, r3);
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
                        updateRegistration(28, r3);
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
                    if ((j4 & 4406636445952L) == 0) {
                        if (dat != 0) {
                            r2 = (AddrU8) getFromArray((Object[]) dat, 2);
                        } else {
                            r2 = str;
                        }
                        updateRegistration(8, r2);
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
                        updateRegistration(8, r2);
                        if (r2 != 0) {
                            data6 = r2.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf3 = String.valueOf(data6);
                    }
                    if ((j4 & 4503608217567232L) == 0) {
                        if (dat != 0) {
                            r4 = (AddrU8) getFromArray((Object[]) dat, 1);
                        } else {
                            r4 = str;
                        }
                        updateRegistration(18, r4);
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
                        updateRegistration(18, r4);
                        if (r4 != 0) {
                            data7 = r4.getData();
                        } else {
                            data7 = 0;
                        }
                        strValueOf4 = String.valueOf(data7);
                    }
                    if ((j4 & 4611686027285757952L) == 0) {
                        if (dat != 0) {
                            r3 = (AddrU8) getFromArray((Object[]) dat, 0);
                        } else {
                            r3 = str;
                        }
                        updateRegistration(28, r3);
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
                        updateRegistration(28, r3);
                        if (r3 != 0) {
                            data8 = r3.getData();
                        } else {
                            data8 = 0;
                        }
                        strValueOf5 = String.valueOf(data8);
                    }
                    str2 = strValueOf3;
                }
                if ((j4 & 9663676416L) == 0) {
                    if (pedalPara != 0) {
                        ch2 = pedalPara.getCh();
                    } else {
                        ch2 = str;
                    }
                    str3 = strValueOf4;
                    updateRegistration(30, ch2);
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
                    updateRegistration(30, ch2);
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
            if ((j4 & 45106392937795585L) == 0 && (j2 & 66560) == 0) {
                str10 = str4;
                str11 = str5;
                strValueOf5 = strValueOf5;
                str12 = str;
                str7 = str12;
                i3 = 0;
                obj = btnPara;
                str9 = str7;
                i2 = 0;
            } else {
                ?? modPara = usr != null ? usr.getModPara() : str;
                if ((j4 & 9077595916734465L) == 0 && (j2 & 66560) == 0) {
                    str4 = str4;
                    str5 = str5;
                    strValueOf5 = strValueOf5;
                    str6 = str;
                    str7 = str6;
                    strValueOf8 = str7;
                } else {
                    ?? dat5 = modPara != 0 ? modPara.getDat() : str;
                    if ((j4 & 25769803777L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                        strValueOf6 = str;
                    } else {
                        ?? r18 = dat5 != 0 ? (AddrU8) getFromArray((Object[]) dat5, 2) : str;
                        updateRegistration(0, r18);
                        strValueOf6 = String.valueOf(r18 != 0 ? r18.getData() : 0);
                    }
                    if ((j4 & 70377334116352L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                        strValueOf7 = str;
                    } else {
                        ?? r19 = dat5 != 0 ? (AddrU8) getFromArray((Object[]) dat5, 1) : str;
                        updateRegistration(12, r19);
                        strValueOf7 = String.valueOf(r19 != 0 ? r19.getData() : 0);
                    }
                    if ((j4 & 9007209992683520L) == 0 && (j2 & 66560) == 0) {
                        str6 = strValueOf7;
                        strValueOf8 = str;
                        str7 = strValueOf6;
                    } else {
                        ?? r20 = dat5 != 0 ? (AddrU8) getFromArray((Object[]) dat5, 0) : str;
                        updateRegistration(19, r20);
                        int data26 = r20 != 0 ? r20.getData() : 0;
                        if ((j4 & 9007207845199872L) == 0 && (j2 & SegmentPool.MAX_SIZE) == 0) {
                            i = data26;
                            str6 = strValueOf7;
                            str7 = strValueOf6;
                            strValueOf8 = str;
                        } else {
                            strValueOf8 = String.valueOf(data26);
                            i = data26;
                            str6 = strValueOf7;
                            str7 = strValueOf6;
                        }
                    }
                    if ((j4 & 36028805610995712L) == 0 || (j2 & SegmentPool.MAX_SIZE) != 0) {
                        if (modPara != 0) {
                            ch3 = modPara.getCh();
                        } else {
                            ch3 = str;
                        }
                        str8 = str6;
                        updateRegistration(21, ch3);
                        if (ch3 != 0) {
                            int data27 = ch3.getData();
                            Object obj2 = btnPara;
                            str9 = strValueOf8;
                            obj = obj2;
                            String str30 = str5;
                            str10 = str4;
                            str11 = str30;
                            i2 = i;
                            i3 = data27;
                            str12 = str8;
                        }
                    } else {
                        str8 = str6;
                    }
                    Object obj3 = btnPara;
                    str9 = strValueOf8;
                    obj = obj3;
                    String str31 = str5;
                    str10 = str4;
                    str11 = str31;
                    str12 = str8;
                    i2 = i;
                    i3 = 0;
                }
                i = 0;
                if ((j4 & 36028805610995712L) == 0) {
                    if (modPara != 0) {
                        ch3 = modPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(21, ch3);
                    if (ch3 != 0) {
                        int data28 = ch3.getData();
                        Object obj4 = btnPara;
                        str9 = strValueOf8;
                        obj = obj4;
                        String str32 = str5;
                        str10 = str4;
                        str11 = str32;
                        i2 = i;
                        i3 = data28;
                        str12 = str8;
                    } else {
                        Object obj5 = btnPara;
                        str9 = strValueOf8;
                        obj = obj5;
                        String str33 = str5;
                        str10 = str4;
                        str11 = str33;
                        str12 = str8;
                        i2 = i;
                        i3 = 0;
                    }
                } else {
                    if (modPara != 0) {
                        ch3 = modPara.getCh();
                    } else {
                        ch3 = str;
                    }
                    str8 = str6;
                    updateRegistration(21, ch3);
                    if (ch3 != 0) {
                        int data29 = ch3.getData();
                        Object obj6 = btnPara;
                        str9 = strValueOf8;
                        obj = obj6;
                        String str34 = str5;
                        str10 = str4;
                        str11 = str34;
                        i2 = i;
                        i3 = data29;
                        str12 = str8;
                    } else {
                        Object obj7 = btnPara;
                        str9 = strValueOf8;
                        obj = obj7;
                        String str35 = str5;
                        str10 = str4;
                        str11 = str35;
                        str12 = str8;
                        i2 = i;
                        i3 = 0;
                    }
                }
            }
            r6 = padPara;
            r7 = encoderPara;
            j5 = j2;
            r13 = obj;
        }
        if ((j4 & 1111161965001763066L) == 0 && (j5 & 131070) == 0) {
            str12 = str12;
            str7 = str7;
            i3 = i3;
            data4 = data4;
            data5 = data5;
            data9 = data9;
            str9 = str9;
            str19 = str;
            str24 = str19;
            strValueOf12 = str24;
            strValueOf16 = strValueOf12;
            str29 = strValueOf16;
            knobMinText = str29;
            knobMaxText = knobMinText;
            str16 = knobMaxText;
            str15 = str16;
            str25 = str15;
            str28 = str25;
            strValueOf19 = str28;
            strValueOf11 = strValueOf19;
            str26 = strValueOf11;
            strValueOf14 = str26;
            str27 = strValueOf14;
            psrValue2Text = str27;
            padMinText = psrValue2Text;
            str13 = padMinText;
            padNoteText = str13;
            str14 = padNoteText;
            psrCCText = str14;
            psrValue1Text = psrCCText;
            padMaxText = psrValue1Text;
            i7 = 0;
            i6 = 0;
            data16 = 0;
            padIndex = 0;
            i4 = 0;
            psrIndex = 0;
            data11 = 0;
            data10 = 0;
            data25 = 0;
        } else {
            long j8 = j4 & 2147483648L;
            knobMinText = ((j8 == 0 && (j5 & 65792) == 0) || usrBinding == null) ? str : usrBinding.getKnobMinText();
            if ((j4 & 306385939516629016L) == 0 && (j5 & 75776) == 0) {
                str12 = str12;
                str15 = str;
                strValueOf11 = str15;
                str13 = strValueOf11;
                str14 = str13;
                data14 = 0;
                psrIndex = 0;
                data11 = 0;
                data10 = 0;
            } else {
                psrIndex = usrBinding != null ? usrBinding.getPsrIndex() : 0;
                ?? r110 = r13 != 0 ? (BtnPara) getFromArray((Object[]) r13, psrIndex) : str;
                if ((j4 & 148176371720L) == 0 && (j5 & 67584) == 0) {
                    psrIndex = psrIndex;
                    r73 = r110;
                } else {
                    ?? type4 = r110 != 0 ? r110.getType() : str;
                    r71 = r110;
                    updateRegistration(3, type4);
                    r73 = r71;
                    data10 = type4 != 0 ? type4.getData() : 0;
                    if (i8 == 0) {
                        r72 = r71;
                        if ((j5 & 67584) != 0) {
                            if (r72 != 0) {
                                sys = r72.getSys();
                            } else {
                                sys = str;
                            }
                            if (midikeyboardUsrFragment != null) {
                                sysEx = midikeyboardUsrFragment.getSysEx(sys);
                            } else {
                                sysEx = str;
                            }
                        } else {
                            sysEx = str;
                        }
                    } else {
                        if (r72 != 0) {
                            sys = r72.getSys();
                        } else {
                            sys = str;
                        }
                        if (midikeyboardUsrFragment != null) {
                            sysEx = midikeyboardUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    }
                    if ((j4 & 18014409247948800L) == 0 || (j5 & 67584) != 0) {
                        if (r72 != 0) {
                            ch4 = r72.getCh();
                        } else {
                            ch4 = str;
                        }
                        str13 = sysEx;
                        updateRegistration(20, ch4);
                        data11 = ch4 != 0 ? ch4.getData() : 0;
                        if ((j4 & 288371399272177680L) == 0 || (j5 & 75776) != 0) {
                            if (r72 != 0) {
                                dat2 = r72.getDat();
                            } else {
                                dat2 = str;
                            }
                            if ((j4 & 285615325200L) == 0 || (j5 & 67584) != 0) {
                                if (dat2 != 0) {
                                    r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                                } else {
                                    r0 = str;
                                }
                                updateRegistration(4, r0);
                                if (r0 != 0) {
                                    data12 = r0.getData();
                                } else {
                                    data12 = 0;
                                }
                                strValueOf9 = String.valueOf(data12);
                            } else {
                                strValueOf9 = str;
                            }
                            if ((j4 & 140748225781760L) == 0 || (j5 & 67584) != 0) {
                                if (dat2 != 0) {
                                    r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                                } else {
                                    r5 = str;
                                }
                                updateRegistration(13, r5);
                                if (r5 != 0) {
                                    data13 = r5.getData();
                                } else {
                                    data13 = 0;
                                }
                                strValueOf10 = String.valueOf(data13);
                            } else {
                                strValueOf10 = str;
                            }
                            j6 = j4 & 288230386905907200L;
                            if (j6 == 0 || (j5 & 75776) != 0) {
                                str14 = strValueOf10;
                                if (dat2 != 0) {
                                    r14 = (AddrU8) getFromArray((Object[]) dat2, 0);
                                } else {
                                    r14 = str;
                                }
                                updateRegistration(24, r14);
                                if (r14 != 0) {
                                    data14 = r14.getData();
                                } else {
                                    data14 = 0;
                                }
                                if (j6 == 0 || (j5 & 67584) != 0) {
                                    strValueOf11 = String.valueOf(data14);
                                } else {
                                    strValueOf11 = str;
                                }
                                str15 = strValueOf9;
                            } else {
                                str14 = strValueOf10;
                                strValueOf11 = str;
                                str15 = strValueOf9;
                            }
                        } else {
                            data11 = data11;
                            data10 = data10;
                            str15 = str;
                            strValueOf11 = str15;
                            str14 = strValueOf11;
                        }
                        data14 = 0;
                    } else {
                        str13 = sysEx;
                    }
                    if ((j4 & 288371399272177680L) == 0) {
                        if (r72 != 0) {
                            dat2 = r72.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j4 & 285615325200L) == 0) {
                            if (dat2 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data12 = r0.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        } else {
                            if (dat2 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data12 = r0.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        }
                        if ((j4 & 140748225781760L) == 0) {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(13, r5);
                            if (r5 != 0) {
                                data13 = r5.getData();
                            } else {
                                data13 = 0;
                            }
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(13, r5);
                            if (r5 != 0) {
                                data13 = r5.getData();
                            } else {
                                data13 = 0;
                            }
                            strValueOf10 = String.valueOf(data13);
                        }
                        j6 = j4 & 288230386905907200L;
                        if (j6 == 0) {
                        }
                        str14 = strValueOf10;
                        if (dat2 != 0) {
                            r14 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r14 = str;
                        }
                        updateRegistration(24, r14);
                        if (r14 != 0) {
                            data14 = r14.getData();
                        } else {
                            data14 = 0;
                        }
                        if (j6 == 0) {
                            strValueOf11 = String.valueOf(data14);
                        } else {
                            strValueOf11 = String.valueOf(data14);
                        }
                        str15 = strValueOf9;
                    } else {
                        if (r72 != 0) {
                            dat2 = r72.getDat();
                        } else {
                            dat2 = str;
                        }
                        if ((j4 & 285615325200L) == 0) {
                            if (dat2 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data12 = r0.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        } else {
                            if (dat2 != 0) {
                                r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                            } else {
                                r0 = str;
                            }
                            updateRegistration(4, r0);
                            if (r0 != 0) {
                                data12 = r0.getData();
                            } else {
                                data12 = 0;
                            }
                            strValueOf9 = String.valueOf(data12);
                        }
                        if ((j4 & 140748225781760L) == 0) {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(13, r5);
                            if (r5 != 0) {
                                data13 = r5.getData();
                            } else {
                                data13 = 0;
                            }
                            strValueOf10 = String.valueOf(data13);
                        } else {
                            if (dat2 != 0) {
                                r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                            } else {
                                r5 = str;
                            }
                            updateRegistration(13, r5);
                            if (r5 != 0) {
                                data13 = r5.getData();
                            } else {
                                data13 = 0;
                            }
                            strValueOf10 = String.valueOf(data13);
                        }
                        j6 = j4 & 288230386905907200L;
                        if (j6 == 0) {
                        }
                        str14 = strValueOf10;
                        if (dat2 != 0) {
                            r14 = (AddrU8) getFromArray((Object[]) dat2, 0);
                        } else {
                            r14 = str;
                        }
                        updateRegistration(24, r14);
                        if (r14 != 0) {
                            data14 = r14.getData();
                        } else {
                            data14 = 0;
                        }
                        if (j6 == 0) {
                            strValueOf11 = String.valueOf(data14);
                        } else {
                            strValueOf11 = String.valueOf(data14);
                        }
                        str15 = strValueOf9;
                    }
                }
                r72 = r73;
                if (i8 == 0) {
                    r72 = r71;
                    if ((j5 & 67584) != 0) {
                        if (r72 != 0) {
                            sys = r72.getSys();
                        } else {
                            sys = str;
                        }
                        if (midikeyboardUsrFragment != null) {
                            sysEx = midikeyboardUsrFragment.getSysEx(sys);
                        } else {
                            sysEx = str;
                        }
                    } else {
                        sysEx = str;
                    }
                } else {
                    if (r72 != 0) {
                        sys = r72.getSys();
                    } else {
                        sys = str;
                    }
                    if (midikeyboardUsrFragment != null) {
                        sysEx = midikeyboardUsrFragment.getSysEx(sys);
                    } else {
                        sysEx = str;
                    }
                }
                if ((j4 & 18014409247948800L) == 0) {
                    if (r72 != 0) {
                        ch4 = r72.getCh();
                    } else {
                        ch4 = str;
                    }
                    str13 = sysEx;
                    updateRegistration(20, ch4);
                    if (ch4 != 0) {
                    }
                } else {
                    if (r72 != 0) {
                        ch4 = r72.getCh();
                    } else {
                        ch4 = str;
                    }
                    str13 = sysEx;
                    updateRegistration(20, ch4);
                    if (ch4 != 0) {
                    }
                }
                if ((j4 & 288371399272177680L) == 0) {
                    if (r72 != 0) {
                        dat2 = r72.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j4 & 285615325200L) == 0) {
                        if (dat2 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data12 = r0.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    } else {
                        if (dat2 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data12 = r0.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    }
                    if ((j4 & 140748225781760L) == 0) {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(13, r5);
                        if (r5 != 0) {
                            data13 = r5.getData();
                        } else {
                            data13 = 0;
                        }
                        strValueOf10 = String.valueOf(data13);
                    } else {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(13, r5);
                        if (r5 != 0) {
                            data13 = r5.getData();
                        } else {
                            data13 = 0;
                        }
                        strValueOf10 = String.valueOf(data13);
                    }
                    j6 = j4 & 288230386905907200L;
                    if (j6 == 0) {
                    }
                    str14 = strValueOf10;
                    if (dat2 != 0) {
                        r14 = (AddrU8) getFromArray((Object[]) dat2, 0);
                    } else {
                        r14 = str;
                    }
                    updateRegistration(24, r14);
                    if (r14 != 0) {
                        data14 = r14.getData();
                    } else {
                        data14 = 0;
                    }
                    if (j6 == 0) {
                        strValueOf11 = String.valueOf(data14);
                    } else {
                        strValueOf11 = String.valueOf(data14);
                    }
                    str15 = strValueOf9;
                } else {
                    if (r72 != 0) {
                        dat2 = r72.getDat();
                    } else {
                        dat2 = str;
                    }
                    if ((j4 & 285615325200L) == 0) {
                        if (dat2 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data12 = r0.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    } else {
                        if (dat2 != 0) {
                            r0 = (AddrU8) getFromArray((Object[]) dat2, 1);
                        } else {
                            r0 = str;
                        }
                        updateRegistration(4, r0);
                        if (r0 != 0) {
                            data12 = r0.getData();
                        } else {
                            data12 = 0;
                        }
                        strValueOf9 = String.valueOf(data12);
                    }
                    if ((j4 & 140748225781760L) == 0) {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(13, r5);
                        if (r5 != 0) {
                            data13 = r5.getData();
                        } else {
                            data13 = 0;
                        }
                        strValueOf10 = String.valueOf(data13);
                    } else {
                        if (dat2 != 0) {
                            r5 = (AddrU8) getFromArray((Object[]) dat2, 2);
                        } else {
                            r5 = str;
                        }
                        updateRegistration(13, r5);
                        if (r5 != 0) {
                            data13 = r5.getData();
                        } else {
                            data13 = 0;
                        }
                        strValueOf10 = String.valueOf(data13);
                    }
                    j6 = j4 & 288230386905907200L;
                    if (j6 == 0) {
                    }
                    str14 = strValueOf10;
                    if (dat2 != 0) {
                        r14 = (AddrU8) getFromArray((Object[]) dat2, 0);
                    } else {
                        r14 = str;
                    }
                    updateRegistration(24, r14);
                    if (r14 != 0) {
                        data14 = r14.getData();
                    } else {
                        data14 = 0;
                    }
                    if (j6 == 0) {
                        strValueOf11 = String.valueOf(data14);
                    } else {
                        strValueOf11 = String.valueOf(data14);
                    }
                    str15 = strValueOf9;
                }
            }
            padNoteText = ((j8 == 0 && (j5 & 65540) == 0) || usrBinding == null) ? str : usrBinding.getPadNoteText();
            knobMaxText = ((j8 == 0 && (j5 & 66048) == 0) || usrBinding == null) ? str : usrBinding.getKnobMaxText();
            psrCCText = ((j8 == 0 && (j5 & 69632) == 0) || usrBinding == null) ? str : usrBinding.getPsrCCText();
            psrValue1Text = ((j8 == 0 && (j5 & 81920) == 0) || usrBinding == null) ? str : usrBinding.getPsrValue1Text();
            long j9 = j4 & 72057604779540480L;
            ?? knobCCStr = ((j9 == 0 && (j5 & 65728) == 0) || usrBinding == null) ? str : usrBinding.getKnobCCStr();
            padMaxText = ((j8 == 0 && (j5 & 65568) == 0) || usrBinding == null) ? str : usrBinding.getPadMaxText();
            if ((j4 & 793197079269769250L) == 0 && (j5 & 65728) == 0) {
                strValueOf11 = strValueOf11;
                str7 = str7;
                str19 = str;
                strValueOf15 = str19;
                strValueOf14 = strValueOf15;
                strValueOf12 = strValueOf14;
                str16 = strValueOf12;
                data20 = 0;
                i4 = 0;
                data16 = 0;
            } else {
                int knobIndex = usrBinding != null ? usrBinding.getKnobIndex() : 0;
                strValueOf12 = (j8 == 0 && (j5 & 65600) == 0) ? str : String.valueOf(knobIndex + 1);
                ?? r21 = r7 != 0 ? (EncoderPara) getFromArray((Object[]) r7, knobIndex) : str;
                if ((j4 & 45097156610L) == 0 && (j5 & 65600) == 0) {
                    r85 = r21;
                } else {
                    ?? ch6 = r21 != 0 ? r21.getCh() : str;
                    ?? r86 = r21;
                    updateRegistration(1, ch6);
                    r85 = r86;
                    if (ch6 != 0) {
                        data15 = ch6.getData();
                        r84 = r86;
                    }
                    if ((j4 & 562960690872320L) == 0 || (j5 & 65600) != 0) {
                        if (r84 != 0) {
                            type2 = r84.getType();
                        } else {
                            type2 = str;
                        }
                        i4 = data15;
                        updateRegistration(15, type2);
                        data16 = type2 != 0 ? type2.getData() : 0;
                        if ((j4 & 648518906872332320L) == 0 || (j5 & 65728) != 0) {
                            if (r84 != 0) {
                                dat3 = r84.getDat();
                            } else {
                                dat3 = str;
                            }
                            if ((j4 & 560493232160L) == 0 || (j5 & 65600) != 0) {
                                if (dat3 != 0) {
                                    r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                                } else {
                                    r8 = str;
                                }
                                updateRegistration(5, r8);
                                if (r8 != 0) {
                                    data17 = r8.getData();
                                } else {
                                    data17 = 0;
                                }
                                strValueOf13 = String.valueOf(data17);
                            } else {
                                strValueOf13 = str;
                            }
                            if (j9 == 0 || (j5 & 65728) != 0) {
                                if (dat3 != 0) {
                                    r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                                } else {
                                    r9 = str;
                                }
                                str16 = strValueOf13;
                                updateRegistration(22, r9);
                                if (r9 != 0) {
                                    data18 = r9.getData();
                                } else {
                                    data18 = 0;
                                }
                                if (j9 == 0 || (j5 & 65600) != 0) {
                                    strValueOf14 = String.valueOf(data18);
                                } else {
                                    strValueOf14 = str;
                                }
                                if (knobCCStr != 0) {
                                    str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                                } else {
                                    str17 = str;
                                }
                            } else {
                                str16 = strValueOf13;
                                str17 = str;
                                strValueOf14 = str17;
                            }
                            if ((j4 & 576460763074396160L) == 0 || (j5 & 65600) != 0) {
                                if (dat3 != 0) {
                                    r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                                } else {
                                    r10 = str;
                                }
                                updateRegistration(25, r10);
                                if (r10 != 0) {
                                    data19 = r10.getData();
                                } else {
                                    data19 = 0;
                                }
                                strValueOf15 = String.valueOf(data19);
                            } else {
                                strValueOf15 = str;
                            }
                        } else {
                            data16 = data16;
                            str7 = str7;
                            str17 = str;
                            strValueOf15 = str17;
                            strValueOf14 = strValueOf15;
                            str16 = strValueOf14;
                        }
                        if ((j4 & 144115198821662720L) == 0 || (j5 & 65600) != 0) {
                            if (r84 != 0) {
                                curve = r84.getCurve();
                            } else {
                                curve = str;
                            }
                            str18 = str17;
                            updateRegistration(23, curve);
                            if (curve != 0) {
                                data20 = curve.getData();
                                str19 = str18;
                            }
                        } else {
                            str18 = str17;
                        }
                        str19 = str18;
                        data20 = 0;
                    } else {
                        i4 = data15;
                    }
                    if ((j4 & 648518906872332320L) == 0) {
                        if (r84 != 0) {
                            dat3 = r84.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j4 & 560493232160L) == 0) {
                            if (dat3 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(5, r8);
                            if (r8 != 0) {
                                data17 = r8.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf13 = String.valueOf(data17);
                        } else {
                            if (dat3 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(5, r8);
                            if (r8 != 0) {
                                data17 = r8.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf13 = String.valueOf(data17);
                        }
                        if (j9 == 0) {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str16 = strValueOf13;
                            updateRegistration(22, r9);
                            if (r9 != 0) {
                                data18 = r9.getData();
                            } else {
                                data18 = 0;
                            }
                            if (j9 == 0) {
                                strValueOf14 = String.valueOf(data18);
                            } else {
                                strValueOf14 = String.valueOf(data18);
                            }
                            if (knobCCStr != 0) {
                                str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                            } else {
                                str17 = str;
                            }
                        } else {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str16 = strValueOf13;
                            updateRegistration(22, r9);
                            if (r9 != 0) {
                                data18 = r9.getData();
                            } else {
                                data18 = 0;
                            }
                            if (j9 == 0) {
                                strValueOf14 = String.valueOf(data18);
                            } else {
                                strValueOf14 = String.valueOf(data18);
                            }
                            if (knobCCStr != 0) {
                                str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                            } else {
                                str17 = str;
                            }
                        }
                        if ((j4 & 576460763074396160L) == 0) {
                            if (dat3 != 0) {
                                r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r10 = str;
                            }
                            updateRegistration(25, r10);
                            if (r10 != 0) {
                                data19 = r10.getData();
                            } else {
                                data19 = 0;
                            }
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            if (dat3 != 0) {
                                r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r10 = str;
                            }
                            updateRegistration(25, r10);
                            if (r10 != 0) {
                                data19 = r10.getData();
                            } else {
                                data19 = 0;
                            }
                            strValueOf15 = String.valueOf(data19);
                        }
                    } else {
                        if (r84 != 0) {
                            dat3 = r84.getDat();
                        } else {
                            dat3 = str;
                        }
                        if ((j4 & 560493232160L) == 0) {
                            if (dat3 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(5, r8);
                            if (r8 != 0) {
                                data17 = r8.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf13 = String.valueOf(data17);
                        } else {
                            if (dat3 != 0) {
                                r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                            } else {
                                r8 = str;
                            }
                            updateRegistration(5, r8);
                            if (r8 != 0) {
                                data17 = r8.getData();
                            } else {
                                data17 = 0;
                            }
                            strValueOf13 = String.valueOf(data17);
                        }
                        if (j9 == 0) {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str16 = strValueOf13;
                            updateRegistration(22, r9);
                            if (r9 != 0) {
                                data18 = r9.getData();
                            } else {
                                data18 = 0;
                            }
                            if (j9 == 0) {
                                strValueOf14 = String.valueOf(data18);
                            } else {
                                strValueOf14 = String.valueOf(data18);
                            }
                            if (knobCCStr != 0) {
                                str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                            } else {
                                str17 = str;
                            }
                        } else {
                            if (dat3 != 0) {
                                r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                            } else {
                                r9 = str;
                            }
                            str16 = strValueOf13;
                            updateRegistration(22, r9);
                            if (r9 != 0) {
                                data18 = r9.getData();
                            } else {
                                data18 = 0;
                            }
                            if (j9 == 0) {
                                strValueOf14 = String.valueOf(data18);
                            } else {
                                strValueOf14 = String.valueOf(data18);
                            }
                            if (knobCCStr != 0) {
                                str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                            } else {
                                str17 = str;
                            }
                        }
                        if ((j4 & 576460763074396160L) == 0) {
                            if (dat3 != 0) {
                                r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r10 = str;
                            }
                            updateRegistration(25, r10);
                            if (r10 != 0) {
                                data19 = r10.getData();
                            } else {
                                data19 = 0;
                            }
                            strValueOf15 = String.valueOf(data19);
                        } else {
                            if (dat3 != 0) {
                                r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                            } else {
                                r10 = str;
                            }
                            updateRegistration(25, r10);
                            if (r10 != 0) {
                                data19 = r10.getData();
                            } else {
                                data19 = 0;
                            }
                            strValueOf15 = String.valueOf(data19);
                        }
                    }
                    if ((j4 & 144115198821662720L) == 0) {
                        if (r84 != 0) {
                            curve = r84.getCurve();
                        } else {
                            curve = str;
                        }
                        str18 = str17;
                        updateRegistration(23, curve);
                        if (curve != 0) {
                            data20 = curve.getData();
                            str19 = str18;
                        } else {
                            str19 = str18;
                            data20 = 0;
                        }
                    } else {
                        if (r84 != 0) {
                            curve = r84.getCurve();
                        } else {
                            curve = str;
                        }
                        str18 = str17;
                        updateRegistration(23, curve);
                        if (curve != 0) {
                            data20 = curve.getData();
                            str19 = str18;
                        } else {
                            str19 = str18;
                            data20 = 0;
                        }
                    }
                }
                data15 = 0;
                r84 = r85;
                if ((j4 & 562960690872320L) == 0) {
                    if (r84 != 0) {
                        type2 = r84.getType();
                    } else {
                        type2 = str;
                    }
                    i4 = data15;
                    updateRegistration(15, type2);
                    if (type2 != 0) {
                    }
                } else {
                    if (r84 != 0) {
                        type2 = r84.getType();
                    } else {
                        type2 = str;
                    }
                    i4 = data15;
                    updateRegistration(15, type2);
                    if (type2 != 0) {
                    }
                }
                if ((j4 & 648518906872332320L) == 0) {
                    if (r84 != 0) {
                        dat3 = r84.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j4 & 560493232160L) == 0) {
                        if (dat3 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(5, r8);
                        if (r8 != 0) {
                            data17 = r8.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    } else {
                        if (dat3 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(5, r8);
                        if (r8 != 0) {
                            data17 = r8.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    }
                    if (j9 == 0) {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str16 = strValueOf13;
                        updateRegistration(22, r9);
                        if (r9 != 0) {
                            data18 = r9.getData();
                        } else {
                            data18 = 0;
                        }
                        if (j9 == 0) {
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            strValueOf14 = String.valueOf(data18);
                        }
                        if (knobCCStr != 0) {
                            str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                        } else {
                            str17 = str;
                        }
                    } else {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str16 = strValueOf13;
                        updateRegistration(22, r9);
                        if (r9 != 0) {
                            data18 = r9.getData();
                        } else {
                            data18 = 0;
                        }
                        if (j9 == 0) {
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            strValueOf14 = String.valueOf(data18);
                        }
                        if (knobCCStr != 0) {
                            str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                        } else {
                            str17 = str;
                        }
                    }
                    if ((j4 & 576460763074396160L) == 0) {
                        if (dat3 != 0) {
                            r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r10 = str;
                        }
                        updateRegistration(25, r10);
                        if (r10 != 0) {
                            data19 = r10.getData();
                        } else {
                            data19 = 0;
                        }
                        strValueOf15 = String.valueOf(data19);
                    } else {
                        if (dat3 != 0) {
                            r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r10 = str;
                        }
                        updateRegistration(25, r10);
                        if (r10 != 0) {
                            data19 = r10.getData();
                        } else {
                            data19 = 0;
                        }
                        strValueOf15 = String.valueOf(data19);
                    }
                } else {
                    if (r84 != 0) {
                        dat3 = r84.getDat();
                    } else {
                        dat3 = str;
                    }
                    if ((j4 & 560493232160L) == 0) {
                        if (dat3 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(5, r8);
                        if (r8 != 0) {
                            data17 = r8.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    } else {
                        if (dat3 != 0) {
                            r8 = (AddrU8) getFromArray((Object[]) dat3, 2);
                        } else {
                            r8 = str;
                        }
                        updateRegistration(5, r8);
                        if (r8 != 0) {
                            data17 = r8.getData();
                        } else {
                            data17 = 0;
                        }
                        strValueOf13 = String.valueOf(data17);
                    }
                    if (j9 == 0) {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str16 = strValueOf13;
                        updateRegistration(22, r9);
                        if (r9 != 0) {
                            data18 = r9.getData();
                        } else {
                            data18 = 0;
                        }
                        if (j9 == 0) {
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            strValueOf14 = String.valueOf(data18);
                        }
                        if (knobCCStr != 0) {
                            str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                        } else {
                            str17 = str;
                        }
                    } else {
                        if (dat3 != 0) {
                            r9 = (AddrU8) getFromArray((Object[]) dat3, 0);
                        } else {
                            r9 = str;
                        }
                        str16 = strValueOf13;
                        updateRegistration(22, r9);
                        if (r9 != 0) {
                            data18 = r9.getData();
                        } else {
                            data18 = 0;
                        }
                        if (j9 == 0) {
                            strValueOf14 = String.valueOf(data18);
                        } else {
                            strValueOf14 = String.valueOf(data18);
                        }
                        if (knobCCStr != 0) {
                            str17 = (String) getFromArray((Object[]) knobCCStr, data18);
                        } else {
                            str17 = str;
                        }
                    }
                    if ((j4 & 576460763074396160L) == 0) {
                        if (dat3 != 0) {
                            r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r10 = str;
                        }
                        updateRegistration(25, r10);
                        if (r10 != 0) {
                            data19 = r10.getData();
                        } else {
                            data19 = 0;
                        }
                        strValueOf15 = String.valueOf(data19);
                    } else {
                        if (dat3 != 0) {
                            r10 = (AddrU8) getFromArray((Object[]) dat3, 1);
                        } else {
                            r10 = str;
                        }
                        updateRegistration(25, r10);
                        if (r10 != 0) {
                            data19 = r10.getData();
                        } else {
                            data19 = 0;
                        }
                        strValueOf15 = String.valueOf(data19);
                    }
                }
                if ((j4 & 144115198821662720L) == 0) {
                    if (r84 != 0) {
                        curve = r84.getCurve();
                    } else {
                        curve = str;
                    }
                    str18 = str17;
                    updateRegistration(23, curve);
                    if (curve != 0) {
                        data20 = curve.getData();
                        str19 = str18;
                    } else {
                        str19 = str18;
                        data20 = 0;
                    }
                } else {
                    if (r84 != 0) {
                        curve = r84.getCurve();
                    } else {
                        curve = str;
                    }
                    str18 = str17;
                    updateRegistration(23, curve);
                    if (curve != 0) {
                        data20 = curve.getData();
                        str19 = str18;
                    } else {
                        str19 = str18;
                        data20 = 0;
                    }
                }
            }
            if ((j4 & 2571768434936000L) == 0 && (j5 & 65546) == 0) {
                str19 = str19;
                str20 = strValueOf15;
                strValueOf14 = strValueOf14;
                str24 = str;
                strValueOf19 = str24;
                strValueOf18 = strValueOf19;
                strValueOf16 = strValueOf18;
                i5 = 0;
                data25 = 0;
                padIndex = 0;
                data21 = 0;
            } else {
                padIndex = usrBinding != null ? usrBinding.getPadIndex() : 0;
                strValueOf16 = (j8 == 0 && (j5 & 65538) == 0) ? str : String.valueOf(padIndex + 1);
                ?? r22 = r6 != 0 ? (PadPara) getFromArray((Object[]) r6, padIndex) : str;
                if ((j4 & 35195109509120L) == 0 && (j5 & 65538) == 0) {
                    padIndex = padIndex;
                    str20 = strValueOf15;
                } else {
                    ?? ch7 = r22 != 0 ? r22.getCh() : str;
                    str20 = strValueOf15;
                    updateRegistration(11, ch7);
                    data21 = ch7 != 0 ? ch7.getData() : 0;
                    if ((j4 & 284784249028800L) == 0 || (j5 & 65546) != 0) {
                        if (r22 != 0) {
                            dat4 = r22.getDat();
                        } else {
                            dat4 = str;
                        }
                        if ((j4 & 1110249046080L) == 0 || (j5 & 65538) != 0) {
                            if (dat4 != 0) {
                                r1 = (AddrU8) getFromArray((Object[]) dat4, 2);
                            } else {
                                r1 = str;
                            }
                            updateRegistration(6, r1);
                            if (r1 != 0) {
                                data22 = r1.getData();
                            } else {
                                data22 = 0;
                            }
                            strValueOf17 = String.valueOf(data22);
                        } else {
                            strValueOf17 = str;
                        }
                        j7 = j4 & 2209760673920L;
                        if (j7 == 0 || (j5 & 65546) != 0) {
                            str21 = strValueOf17;
                            if (dat4 != 0) {
                                r11 = (AddrU8) getFromArray((Object[]) dat4, 0);
                            } else {
                                r11 = str;
                            }
                            updateRegistration(7, r11);
                            if (r11 != 0) {
                                data23 = r11.getData();
                            } else {
                                data23 = 0;
                            }
                            if (j7 == 0 || (j5 & 65538) != 0) {
                                strValueOf18 = String.valueOf(data23);
                            } else {
                                strValueOf18 = str;
                            }
                        } else {
                            str21 = strValueOf17;
                            strValueOf18 = str;
                            data23 = 0;
                        }
                        if ((j4 & 281485714145280L) == 0 || (j5 & 65538) != 0) {
                            if (dat4 != 0) {
                                r12 = (AddrU8) getFromArray((Object[]) dat4, 1);
                            } else {
                                r12 = str;
                            }
                            updateRegistration(14, r12);
                            if (r12 != 0) {
                                data24 = r12.getData();
                            } else {
                                data24 = 0;
                            }
                            strValueOf19 = String.valueOf(data24);
                        } else {
                            strValueOf19 = str;
                        }
                        str22 = str21;
                    } else {
                        data21 = data21;
                        strValueOf14 = strValueOf14;
                        str22 = str;
                        strValueOf19 = str22;
                        strValueOf18 = strValueOf19;
                        data23 = 0;
                    }
                    if ((j4 & 2251810551234560L) == 0 || (j5 & 65538) != 0) {
                        if (r22 != 0) {
                            type3 = r22.getType();
                        } else {
                            type3 = str;
                        }
                        str23 = str22;
                        updateRegistration(17, type3);
                        if (type3 != 0) {
                            data25 = type3.getData();
                            str24 = str23;
                            i5 = data23;
                        }
                    } else {
                        str23 = str22;
                    }
                    str24 = str23;
                    i5 = data23;
                    data25 = 0;
                }
                if ((j4 & 284784249028800L) == 0) {
                    if (r22 != 0) {
                        dat4 = r22.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j4 & 1110249046080L) == 0) {
                        if (dat4 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat4, 2);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(6, r1);
                        if (r1 != 0) {
                            data22 = r1.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf17 = String.valueOf(data22);
                    } else {
                        if (dat4 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat4, 2);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(6, r1);
                        if (r1 != 0) {
                            data22 = r1.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf17 = String.valueOf(data22);
                    }
                    j7 = j4 & 2209760673920L;
                    if (j7 == 0) {
                        str21 = strValueOf17;
                        if (dat4 != 0) {
                            r11 = (AddrU8) getFromArray((Object[]) dat4, 0);
                        } else {
                            r11 = str;
                        }
                        updateRegistration(7, r11);
                        if (r11 != 0) {
                            data23 = r11.getData();
                        } else {
                            data23 = 0;
                        }
                        if (j7 == 0) {
                            strValueOf18 = String.valueOf(data23);
                        } else {
                            strValueOf18 = String.valueOf(data23);
                        }
                    } else {
                        str21 = strValueOf17;
                        if (dat4 != 0) {
                            r11 = (AddrU8) getFromArray((Object[]) dat4, 0);
                        } else {
                            r11 = str;
                        }
                        updateRegistration(7, r11);
                        if (r11 != 0) {
                            data23 = r11.getData();
                        } else {
                            data23 = 0;
                        }
                        if (j7 == 0) {
                            strValueOf18 = String.valueOf(data23);
                        } else {
                            strValueOf18 = String.valueOf(data23);
                        }
                    }
                    if ((j4 & 281485714145280L) == 0) {
                        if (dat4 != 0) {
                            r12 = (AddrU8) getFromArray((Object[]) dat4, 1);
                        } else {
                            r12 = str;
                        }
                        updateRegistration(14, r12);
                        if (r12 != 0) {
                            data24 = r12.getData();
                        } else {
                            data24 = 0;
                        }
                        strValueOf19 = String.valueOf(data24);
                    } else {
                        if (dat4 != 0) {
                            r12 = (AddrU8) getFromArray((Object[]) dat4, 1);
                        } else {
                            r12 = str;
                        }
                        updateRegistration(14, r12);
                        if (r12 != 0) {
                            data24 = r12.getData();
                        } else {
                            data24 = 0;
                        }
                        strValueOf19 = String.valueOf(data24);
                    }
                    str22 = str21;
                } else {
                    if (r22 != 0) {
                        dat4 = r22.getDat();
                    } else {
                        dat4 = str;
                    }
                    if ((j4 & 1110249046080L) == 0) {
                        if (dat4 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat4, 2);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(6, r1);
                        if (r1 != 0) {
                            data22 = r1.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf17 = String.valueOf(data22);
                    } else {
                        if (dat4 != 0) {
                            r1 = (AddrU8) getFromArray((Object[]) dat4, 2);
                        } else {
                            r1 = str;
                        }
                        updateRegistration(6, r1);
                        if (r1 != 0) {
                            data22 = r1.getData();
                        } else {
                            data22 = 0;
                        }
                        strValueOf17 = String.valueOf(data22);
                    }
                    j7 = j4 & 2209760673920L;
                    if (j7 == 0) {
                        str21 = strValueOf17;
                        if (dat4 != 0) {
                            r11 = (AddrU8) getFromArray((Object[]) dat4, 0);
                        } else {
                            r11 = str;
                        }
                        updateRegistration(7, r11);
                        if (r11 != 0) {
                            data23 = r11.getData();
                        } else {
                            data23 = 0;
                        }
                        if (j7 == 0) {
                            strValueOf18 = String.valueOf(data23);
                        } else {
                            strValueOf18 = String.valueOf(data23);
                        }
                    } else {
                        str21 = strValueOf17;
                        if (dat4 != 0) {
                            r11 = (AddrU8) getFromArray((Object[]) dat4, 0);
                        } else {
                            r11 = str;
                        }
                        updateRegistration(7, r11);
                        if (r11 != 0) {
                            data23 = r11.getData();
                        } else {
                            data23 = 0;
                        }
                        if (j7 == 0) {
                            strValueOf18 = String.valueOf(data23);
                        } else {
                            strValueOf18 = String.valueOf(data23);
                        }
                    }
                    if ((j4 & 281485714145280L) == 0) {
                        if (dat4 != 0) {
                            r12 = (AddrU8) getFromArray((Object[]) dat4, 1);
                        } else {
                            r12 = str;
                        }
                        updateRegistration(14, r12);
                        if (r12 != 0) {
                            data24 = r12.getData();
                        } else {
                            data24 = 0;
                        }
                        strValueOf19 = String.valueOf(data24);
                    } else {
                        if (dat4 != 0) {
                            r12 = (AddrU8) getFromArray((Object[]) dat4, 1);
                        } else {
                            r12 = str;
                        }
                        updateRegistration(14, r12);
                        if (r12 != 0) {
                            data24 = r12.getData();
                        } else {
                            data24 = 0;
                        }
                        strValueOf19 = String.valueOf(data24);
                    }
                    str22 = str21;
                }
                if ((j4 & 2251810551234560L) == 0) {
                    if (r22 != 0) {
                        type3 = r22.getType();
                    } else {
                        type3 = str;
                    }
                    str23 = str22;
                    updateRegistration(17, type3);
                    if (type3 != 0) {
                        data25 = type3.getData();
                        str24 = str23;
                        i5 = data23;
                    } else {
                        str24 = str23;
                        i5 = data23;
                        data25 = 0;
                    }
                } else {
                    if (r22 != 0) {
                        type3 = r22.getType();
                    } else {
                        type3 = str;
                    }
                    str23 = str22;
                    updateRegistration(17, type3);
                    if (type3 != 0) {
                        data25 = type3.getData();
                        str24 = str23;
                        i5 = data23;
                    } else {
                        str24 = str23;
                        i5 = data23;
                        data25 = 0;
                    }
                }
            }
            if ((j4 & 288230386905907200L) == 0 && (j5 & 75776) == 0) {
                str24 = str24;
            } else {
                ?? psrCCStr = usrBinding != null ? usrBinding.getPsrCCStr() : str;
                if (psrCCStr != 0) {
                    str25 = (String) getFromArray((Object[]) psrCCStr, data14);
                }
                if ((j4 & 9007209992683520L) == 0 || (j5 & 66560) != 0) {
                    if (usrBinding != null) {
                        modCCStr = usrBinding.getModCCStr();
                    } else {
                        r16 = str;
                    }
                    if (r16 != 0) {
                        r16 = modCCStr;
                        str26 = (String) getFromArray((Object[]) r16, i2);
                    } else {
                        r16 = modCCStr;
                        str26 = str;
                    }
                } else {
                    r16 = modCCStr;
                    str26 = str;
                }
                if ((j4 & 2209760673920L) == 0 || (j5 & 65546) != 0) {
                    if (usrBinding != null) {
                        padNoteStr = usrBinding.getPadNoteStr();
                    } else {
                        r17 = str;
                    }
                    if (r17 != 0) {
                        str27 = (String) getFromArray((Object[]) r17, i5);
                    } else {
                        r17 = padNoteStr;
                        str27 = str;
                    }
                } else {
                    r17 = padNoteStr;
                    str27 = str;
                }
                if (j8 == 0) {
                    r17 = padNoteStr;
                    if ((j5 & 98304) != 0) {
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
                padMinText = ((j8 == 0 || (j5 & 65552) != 0) && usrBinding != null) ? usrBinding.getPadMinText() : str;
                str28 = strValueOf18;
                i6 = data20;
                str29 = str20;
                i7 = data21;
            }
            str25 = str;
            if ((j4 & 9007209992683520L) == 0) {
                if (usrBinding != null) {
                    modCCStr = usrBinding.getModCCStr();
                } else {
                    r16 = str;
                }
                if (r16 != 0) {
                    r16 = modCCStr;
                    str26 = (String) getFromArray((Object[]) r16, i2);
                } else {
                    r16 = modCCStr;
                    str26 = str;
                }
            } else {
                if (usrBinding != null) {
                    modCCStr = usrBinding.getModCCStr();
                } else {
                    r16 = str;
                }
                if (r16 != 0) {
                    r16 = modCCStr;
                    str26 = (String) getFromArray((Object[]) r16, i2);
                } else {
                    r16 = modCCStr;
                    str26 = str;
                }
            }
            if ((j4 & 2209760673920L) == 0) {
                if (usrBinding != null) {
                    padNoteStr = usrBinding.getPadNoteStr();
                } else {
                    r17 = str;
                }
                if (r17 != 0) {
                    str27 = (String) getFromArray((Object[]) r17, i5);
                } else {
                    r17 = padNoteStr;
                    str27 = str;
                }
            } else {
                if (usrBinding != null) {
                    padNoteStr = usrBinding.getPadNoteStr();
                } else {
                    r17 = str;
                }
                if (r17 != 0) {
                    str27 = (String) getFromArray((Object[]) r17, i5);
                } else {
                    r17 = padNoteStr;
                    str27 = str;
                }
            }
            if (j8 == 0) {
                r17 = padNoteStr;
                if ((j5 & 98304) != 0) {
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
            if (j8 == 0) {
            }
            str28 = strValueOf18;
            i6 = data20;
            str29 = str20;
            i7 = data21;
        }
        if ((j4 & 35195109509120L) != 0 || (j5 & 65538) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.channel, i7);
        }
        long j10 = j5 & SegmentPool.MAX_SIZE;
        if (j10 != 0) {
            ?? r23 = str;
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.channel, this.mCallback92, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keybedChannel, this.mCallback97, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.keybedMaxVel, r23, r23, this.mCallback101, r23);
            TextViewBindingAdapter.setTextWatcher(this.keybedMinVel, r23, r23, this.mCallback100, r23);
            TextViewBindingAdapter.setTextWatcher(this.knobMaxVal, r23, r23, this.mCallback108, r23);
            TextViewBindingAdapter.setTextWatcher(this.knobMinVal, r23, r23, this.mCallback107, r23);
            SeekBarBindingAdapter.setOnSeekBarChangeListener(this.knobSeekbar, r23, r23, this.mCallback102, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.knobType, this.mCallback103, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.maxVel, r23, r23, this.mCallback95, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView22, this.mCallback104, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView23, this.mCallback105, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.mboundView24, r23, r23, this.mCallback106, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView30, this.mCallback109, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView35, this.mCallback113, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView36, this.mCallback114, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView37, this.mCallback115, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView41, this.mCallback119, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mboundView43, this.mCallback121, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.mboundView45, r23, r23, this.mCallback122, r23);
            TextViewBindingAdapter.setTextWatcher(this.minVel, r23, r23, this.mCallback94, r23);
            TextViewBindingAdapter.setTextWatcher(this.modCCVal, r23, r23, this.mCallback110, r23);
            TextViewBindingAdapter.setTextWatcher(this.modMaxVal, r23, r23, this.mCallback112, r23);
            TextViewBindingAdapter.setTextWatcher(this.modMinVal, r23, r23, this.mCallback111, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.octive, this.mCallback98, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.padNote, r23, r23, this.mCallback93, r23);
            SeekBarBindingAdapter.setOnSeekBarChangeListener(this.padSeekbar, r23, r23, this.mCallback90, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.psrMode, this.mCallback120, r23, r23);
            TextViewBindingAdapter.setTextWatcher(this.psrValue1, r23, r23, this.mCallback123, r23);
            TextViewBindingAdapter.setTextWatcher(this.psrValue2, r23, r23, this.mCallback124, r23);
            this.selectColorBtn.setOnClickListener(this.mCallback96);
            TextViewBindingAdapter.setTextWatcher(this.sustainCCVal, r23, r23, this.mCallback116, r23);
            TextViewBindingAdapter.setTextWatcher(this.sustainDownVal, r23, r23, this.mCallback118, r23);
            TextViewBindingAdapter.setTextWatcher(this.sustainUpVal, r23, r23, this.mCallback117, r23);
            TextViewBindingAdapter.setTextWatcher(this.sysExValue, r23, r23, this.mCallback125, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.transpose, this.mCallback99, r23, r23);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.type, this.mCallback91, r23, r23);
            RadioGroupBindingAdapter.setListeners(this.typeRadio, this.mCallback89, r23);
        }
        if ((j4 & 17600775980032L) != 0 || j10 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keybedChannel, data);
        }
        if ((j4 & j3) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.keybedMaxVel, strValueOf);
        }
        if ((j4 & 8804682957312L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.keybedMinVel, strValueOf2);
        }
        long j11 = j4 & 72057604779540480L;
        if (j11 != 0 || (j5 & 65728) != 0) {
            TextViewBindingAdapter.setText(this.knobCCStr, str19);
        }
        long j12 = j4 & 2147483648L;
        if (j12 != 0 || (j5 & 66048) != 0) {
            TextViewBindingAdapter.setText(this.knobMaxText, knobMaxText);
        }
        if ((j4 & 560493232160L) != 0 || (j5 & 65600) != 0) {
            TextViewBindingAdapter.setText(this.knobMaxVal, str16);
        }
        if (j12 != 0 || (j5 & 65792) != 0) {
            TextViewBindingAdapter.setText(this.knobMinText, knobMinText);
        }
        if ((j4 & 576460763074396160L) != 0 || (j5 & 65600) != 0) {
            TextViewBindingAdapter.setText(this.knobMinVal, str29);
        }
        if (j12 != 0 || (j5 & 65538) != 0) {
            SeekBarBindingAdapter.setProgress(this.knobSeekbar, padIndex);
            TextViewBindingAdapter.setText(this.mboundView2, strValueOf16);
            SeekBarBindingAdapter.setProgress(this.padSeekbar, padIndex);
        }
        if ((j4 & 562960690872320L) != 0 || (j5 & 65600) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.knobType, data16);
        }
        if ((j4 & 1110249046080L) != 0 || (j5 & 65538) != 0) {
            TextViewBindingAdapter.setText(this.maxVel, str24);
        }
        if (j12 != 0 || (j5 & 65600) != 0) {
            TextViewBindingAdapter.setText(this.mboundView19, strValueOf12);
        }
        if ((j4 & 144115198821662720L) != 0 || (j5 & 65600) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView22, i6);
        }
        if ((j4 & 45097156610L) != 0 || (j5 & 65600) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView23, i4);
        }
        if (j11 != 0 || (j5 & 65600) != 0) {
            TextViewBindingAdapter.setText(this.mboundView24, strValueOf14);
        }
        if ((j4 & 36028805610995712L) != 0 || r3 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView30, i3);
        }
        if ((j4 & 9007209992683520L) != 0 || (j5 & 66560) != 0) {
            TextViewBindingAdapter.setText(this.mboundView32, str26);
        }
        if ((j4 & (-9223372027727970304L)) != 0 || r3 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView35, data4);
        }
        if ((j4 & 1152921513263890432L) != 0 || r3 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView36, data5);
        }
        if ((j4 & 9663676416L) != 0 || (j5 & 65537) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView37, data9);
        }
        if (j12 != 0 || (j5 & 67584) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView41, psrIndex);
        }
        if ((j4 & 18014409247948800L) != 0 || (j5 & 67584) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mboundView43, data11);
        }
        if (j12 != 0 || (j5 & 69632) != 0) {
            TextViewBindingAdapter.setText(this.mboundView44, psrCCText);
        }
        long j13 = j4 & 288230386905907200L;
        if (j13 != 0 || (j5 & 67584) != 0) {
            TextViewBindingAdapter.setText(this.mboundView45, strValueOf11);
        }
        if (j12 != 0 || (j5 & 81920) != 0) {
            TextViewBindingAdapter.setText(this.mboundView47, psrValue1Text);
        }
        if (j12 != 0 || (j5 & 98304) != 0) {
            TextViewBindingAdapter.setText(this.mboundView49, psrValue2Text);
        }
        if ((j4 & 281485714145280L) != 0 || (j5 & 65538) != 0) {
            TextViewBindingAdapter.setText(this.minVel, strValueOf19);
        }
        if ((j4 & 9007207845199872L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.modCCVal, str9);
        }
        if ((j4 & 25769803777L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.modMaxVal, str7);
        }
        if ((j4 & 70377334116352L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.modMinVal, str12);
        }
        if ((j4 & 1125908496842752L) != 0 || r3 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.octive, data2);
        }
        if (j12 != 0 || (j5 & 65568) != 0) {
            TextViewBindingAdapter.setText(this.padMaxText, padMaxText);
        }
        if (j12 != 0 || (j5 & 65552) != 0) {
            TextViewBindingAdapter.setText(this.padMinText, padMinText);
        }
        long j14 = j4 & 2209760673920L;
        if (j14 != 0 || (j5 & 65538) != 0) {
            TextViewBindingAdapter.setText(this.padNote, str28);
        }
        if (j14 != 0 || (j5 & 65546) != 0) {
            TextViewBindingAdapter.setText(this.padNoteStr, str27);
        }
        if (j12 != 0 || (j5 & 65540) != 0) {
            TextViewBindingAdapter.setText(this.padNoteText, padNoteText);
        }
        if (j13 != 0 || (j5 & 75776) != 0) {
            TextViewBindingAdapter.setText(this.psrCCStr, str25);
        }
        if ((j4 & 148176371720L) != 0 || (j5 & 67584) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.psrMode, data10);
        }
        if ((j4 & 285615325200L) != 0 || (j5 & 67584) != 0) {
            TextViewBindingAdapter.setText(this.psrValue1, str15);
        }
        if ((j4 & 140748225781760L) != 0 || (j5 & 67584) != 0) {
            TextViewBindingAdapter.setText(this.psrValue2, str14);
        }
        if ((j4 & 4611686027285757952L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.sustainCCVal, strValueOf5);
        }
        if ((j4 & 4406636445952L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.sustainDownVal, str10);
        }
        if ((j4 & 4503608217567232L) != 0 || r3 != 0) {
            TextViewBindingAdapter.setText(this.sustainUpVal, str11);
        }
        if (i8 != 0 || (j5 & 67584) != 0) {
            TextViewBindingAdapter.setText(this.sysExValue, str13);
        }
        if ((j4 & 2305843017937846272L) != 0 || r3 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.transpose, data3);
        }
        if ((j4 & 2251810551234560L) == 0 && (j5 & 65538) == 0) {
            return;
        }
        AdapterViewBindingAdapter.setSelectedItemPosition(this.type, data25);
    }

    @Override // com.cubeSuite.generated.callback.AfterTextChanged.Listener
    public final void _internalCallbackAfterTextChanged(int i, Editable editable) {
        PadPara[] padPara;
        PadPara padPara2;
        AddrU8[] dat;
        PadPara[] padPara3;
        PadPara padPara4;
        AddrU8[] dat2;
        PadPara[] padPara5;
        PadPara padPara6;
        AddrU8[] dat3;
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
        BtnPara[] btnPara;
        BtnPara btnPara2;
        AddrU8[] dat13;
        BtnPara[] btnPara3;
        BtnPara btnPara4;
        AddrU8[] dat14;
        BtnPara[] btnPara5;
        BtnPara btnPara6;
        AddrU8[] dat15;
        BtnPara[] btnPara7;
        BtnPara btnPara8;
        if (i == 5) {
            MidikeyboardUsrFragment midikeyboardUsrFragment = this.mListener;
            UsrBinding usrBinding = this.mUsrBinding;
            Usr usr = this.mData;
            if (midikeyboardUsrFragment == null || usr == null || (padPara = usr.getPadPara()) == null || usrBinding == null || (padPara2 = (PadPara) getFromArray(padPara, usrBinding.getPadIndex())) == null || (dat = padPara2.getDat()) == null) {
                return;
            }
            midikeyboardUsrFragment.setNum(editable, (AddrU8) getFromArray(dat, 0), 0, 127);
            return;
        }
        if (i == 6) {
            MidikeyboardUsrFragment midikeyboardUsrFragment2 = this.mListener;
            UsrBinding usrBinding2 = this.mUsrBinding;
            Usr usr2 = this.mData;
            if (midikeyboardUsrFragment2 == null || usr2 == null || (padPara3 = usr2.getPadPara()) == null || usrBinding2 == null || (padPara4 = (PadPara) getFromArray(padPara3, usrBinding2.getPadIndex())) == null || (dat2 = padPara4.getDat()) == null) {
                return;
            }
            midikeyboardUsrFragment2.setNum(editable, (AddrU8) getFromArray(dat2, 1), 0, 127);
            return;
        }
        if (i == 7) {
            MidikeyboardUsrFragment midikeyboardUsrFragment3 = this.mListener;
            UsrBinding usrBinding3 = this.mUsrBinding;
            Usr usr3 = this.mData;
            if (midikeyboardUsrFragment3 == null || usr3 == null || (padPara5 = usr3.getPadPara()) == null || usrBinding3 == null || (padPara6 = (PadPara) getFromArray(padPara5, usrBinding3.getPadIndex())) == null || (dat3 = padPara6.getDat()) == null) {
                return;
            }
            midikeyboardUsrFragment3.setNum(editable, (AddrU8) getFromArray(dat3, 2), 0, 127);
            return;
        }
        if (i == 12) {
            MidikeyboardUsrFragment midikeyboardUsrFragment4 = this.mListener;
            Usr usr4 = this.mData;
            if (midikeyboardUsrFragment4 == null || usr4 == null || (keyPara = usr4.getKeyPara()) == null) {
                return;
            }
            midikeyboardUsrFragment4.setNum(editable, keyPara.getMin(), 0, 127);
            return;
        }
        if (i != 13) {
            switch (i) {
                case 18:
                    MidikeyboardUsrFragment midikeyboardUsrFragment5 = this.mListener;
                    UsrBinding usrBinding4 = this.mUsrBinding;
                    Usr usr5 = this.mData;
                    if (midikeyboardUsrFragment5 != null && usr5 != null && (encoderPara = usr5.getEncoderPara()) != null && usrBinding4 != null && (encoderPara2 = (EncoderPara) getFromArray(encoderPara, usrBinding4.getKnobIndex())) != null && (dat4 = encoderPara2.getDat()) != null) {
                        midikeyboardUsrFragment5.setNum(editable, (AddrU8) getFromArray(dat4, 0), 0, 127);
                        break;
                    }
                    break;
                case 19:
                    MidikeyboardUsrFragment midikeyboardUsrFragment6 = this.mListener;
                    UsrBinding usrBinding5 = this.mUsrBinding;
                    Usr usr6 = this.mData;
                    if (midikeyboardUsrFragment6 != null && usr6 != null && (encoderPara3 = usr6.getEncoderPara()) != null && usrBinding5 != null && (encoderPara4 = (EncoderPara) getFromArray(encoderPara3, usrBinding5.getKnobIndex())) != null && (dat5 = encoderPara4.getDat()) != null) {
                        midikeyboardUsrFragment6.setNum(editable, (AddrU8) getFromArray(dat5, 1), 0, 127);
                        break;
                    }
                    break;
                case 20:
                    MidikeyboardUsrFragment midikeyboardUsrFragment7 = this.mListener;
                    UsrBinding usrBinding6 = this.mUsrBinding;
                    Usr usr7 = this.mData;
                    if (midikeyboardUsrFragment7 != null && usr7 != null && (encoderPara5 = usr7.getEncoderPara()) != null && usrBinding6 != null && (encoderPara6 = (EncoderPara) getFromArray(encoderPara5, usrBinding6.getKnobIndex())) != null && (dat6 = encoderPara6.getDat()) != null) {
                        midikeyboardUsrFragment7.setNum(editable, (AddrU8) getFromArray(dat6, 2), 0, 127);
                        break;
                    }
                    break;
                default:
                    switch (i) {
                        case 22:
                            MidikeyboardUsrFragment midikeyboardUsrFragment8 = this.mListener;
                            Usr usr8 = this.mData;
                            if (midikeyboardUsrFragment8 != null && usr8 != null && (modPara = usr8.getModPara()) != null && (dat7 = modPara.getDat()) != null) {
                                midikeyboardUsrFragment8.setNum(editable, (AddrU8) getFromArray(dat7, 0), 0, 127);
                                break;
                            }
                            break;
                        case 23:
                            MidikeyboardUsrFragment midikeyboardUsrFragment9 = this.mListener;
                            Usr usr9 = this.mData;
                            if (midikeyboardUsrFragment9 != null && usr9 != null && (modPara2 = usr9.getModPara()) != null && (dat8 = modPara2.getDat()) != null) {
                                midikeyboardUsrFragment9.setNum(editable, (AddrU8) getFromArray(dat8, 1), 0, 127);
                                break;
                            }
                            break;
                        case 24:
                            MidikeyboardUsrFragment midikeyboardUsrFragment10 = this.mListener;
                            Usr usr10 = this.mData;
                            if (midikeyboardUsrFragment10 != null && usr10 != null && (modPara3 = usr10.getModPara()) != null && (dat9 = modPara3.getDat()) != null) {
                                midikeyboardUsrFragment10.setNum(editable, (AddrU8) getFromArray(dat9, 2), 0, 127);
                                break;
                            }
                            break;
                        default:
                            switch (i) {
                                case 28:
                                    MidikeyboardUsrFragment midikeyboardUsrFragment11 = this.mListener;
                                    Usr usr11 = this.mData;
                                    if (midikeyboardUsrFragment11 != null && usr11 != null && (pedalPara = usr11.getPedalPara()) != null && (dat10 = pedalPara.getDat()) != null) {
                                        midikeyboardUsrFragment11.setNum(editable, (AddrU8) getFromArray(dat10, 0), 0, 127);
                                        break;
                                    }
                                    break;
                                case 29:
                                    MidikeyboardUsrFragment midikeyboardUsrFragment12 = this.mListener;
                                    Usr usr12 = this.mData;
                                    if (midikeyboardUsrFragment12 != null && usr12 != null && (pedalPara2 = usr12.getPedalPara()) != null && (dat11 = pedalPara2.getDat()) != null) {
                                        midikeyboardUsrFragment12.setNum(editable, (AddrU8) getFromArray(dat11, 1), 0, 127);
                                        break;
                                    }
                                    break;
                                case 30:
                                    MidikeyboardUsrFragment midikeyboardUsrFragment13 = this.mListener;
                                    Usr usr13 = this.mData;
                                    if (midikeyboardUsrFragment13 != null && usr13 != null && (pedalPara3 = usr13.getPedalPara()) != null && (dat12 = pedalPara3.getDat()) != null) {
                                        midikeyboardUsrFragment13.setNum(editable, (AddrU8) getFromArray(dat12, 2), 0, 127);
                                        break;
                                    }
                                    break;
                                default:
                                    switch (i) {
                                        case 34:
                                            MidikeyboardUsrFragment midikeyboardUsrFragment14 = this.mListener;
                                            UsrBinding usrBinding7 = this.mUsrBinding;
                                            Usr usr14 = this.mData;
                                            if (midikeyboardUsrFragment14 != null && usr14 != null && (btnPara = usr14.getBtnPara()) != null && usrBinding7 != null && (btnPara2 = (BtnPara) getFromArray(btnPara, usrBinding7.getPsrIndex())) != null && (dat13 = btnPara2.getDat()) != null) {
                                                midikeyboardUsrFragment14.setNum(editable, (AddrU8) getFromArray(dat13, 0), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 35:
                                            MidikeyboardUsrFragment midikeyboardUsrFragment15 = this.mListener;
                                            UsrBinding usrBinding8 = this.mUsrBinding;
                                            Usr usr15 = this.mData;
                                            if (midikeyboardUsrFragment15 != null && usr15 != null && (btnPara3 = usr15.getBtnPara()) != null && usrBinding8 != null && (btnPara4 = (BtnPara) getFromArray(btnPara3, usrBinding8.getPsrIndex())) != null && (dat14 = btnPara4.getDat()) != null) {
                                                midikeyboardUsrFragment15.setNum(editable, (AddrU8) getFromArray(dat14, 1), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 36:
                                            MidikeyboardUsrFragment midikeyboardUsrFragment16 = this.mListener;
                                            UsrBinding usrBinding9 = this.mUsrBinding;
                                            Usr usr16 = this.mData;
                                            if (midikeyboardUsrFragment16 != null && usr16 != null && (btnPara5 = usr16.getBtnPara()) != null && usrBinding9 != null && (btnPara6 = (BtnPara) getFromArray(btnPara5, usrBinding9.getPsrIndex())) != null && (dat15 = btnPara6.getDat()) != null) {
                                                midikeyboardUsrFragment16.setNum(editable, (AddrU8) getFromArray(dat15, 2), 0, 127);
                                                break;
                                            }
                                            break;
                                        case 37:
                                            MidikeyboardUsrFragment midikeyboardUsrFragment17 = this.mListener;
                                            UsrBinding usrBinding10 = this.mUsrBinding;
                                            Usr usr17 = this.mData;
                                            if (midikeyboardUsrFragment17 != null && usr17 != null && (btnPara7 = usr17.getBtnPara()) != null && usrBinding10 != null && (btnPara8 = (BtnPara) getFromArray(btnPara7, usrBinding10.getPsrIndex())) != null) {
                                                midikeyboardUsrFragment17.setSysEx(editable, btnPara8.getSys());
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
        MidikeyboardUsrFragment midikeyboardUsrFragment18 = this.mListener;
        Usr usr18 = this.mData;
        if (midikeyboardUsrFragment18 == null || usr18 == null || (keyPara2 = usr18.getKeyPara()) == null) {
            return;
        }
        midikeyboardUsrFragment18.setNum(editable, keyPara2.getMax(), 0, 127);
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        PadPara[] padPara;
        PadPara padPara2;
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
        BtnPara[] btnPara;
        BtnPara btnPara2;
        if (i == 3) {
            MidikeyboardUsrFragment midikeyboardUsrFragment = this.mListener;
            if (midikeyboardUsrFragment != null) {
                midikeyboardUsrFragment.padTypeChange(i2);
                return;
            }
            return;
        }
        if (i == 4) {
            MidikeyboardUsrFragment midikeyboardUsrFragment2 = this.mListener;
            UsrBinding usrBinding = this.mUsrBinding;
            Usr usr = this.mData;
            if (midikeyboardUsrFragment2 == null || usr == null || (padPara = usr.getPadPara()) == null || usrBinding == null || (padPara2 = (PadPara) getFromArray(padPara, usrBinding.getPadIndex())) == null) {
                return;
            }
            midikeyboardUsrFragment2.U8DataChange(i2, padPara2.getCh());
            return;
        }
        if (i == 21) {
            MidikeyboardUsrFragment midikeyboardUsrFragment3 = this.mListener;
            Usr usr2 = this.mData;
            if (midikeyboardUsrFragment3 == null || usr2 == null || (modPara = usr2.getModPara()) == null) {
                return;
            }
            midikeyboardUsrFragment3.U8DataChange(i2, modPara.getCh());
            return;
        }
        switch (i) {
            case 9:
                MidikeyboardUsrFragment midikeyboardUsrFragment4 = this.mListener;
                Usr usr3 = this.mData;
                if (midikeyboardUsrFragment4 != null && usr3 != null && (keyPara = usr3.getKeyPara()) != null) {
                    midikeyboardUsrFragment4.U8DataChange(i2, keyPara.getCh());
                    break;
                }
                break;
            case 10:
                MidikeyboardUsrFragment midikeyboardUsrFragment5 = this.mListener;
                Usr usr4 = this.mData;
                if (midikeyboardUsrFragment5 != null && usr4 != null && (keyPara2 = usr4.getKeyPara()) != null) {
                    midikeyboardUsrFragment5.I8DataChange(i2 - 3, keyPara2.getOctave());
                    break;
                }
                break;
            case 11:
                MidikeyboardUsrFragment midikeyboardUsrFragment6 = this.mListener;
                Usr usr5 = this.mData;
                if (midikeyboardUsrFragment6 != null && usr5 != null && (keyPara3 = usr5.getKeyPara()) != null) {
                    midikeyboardUsrFragment6.I8DataChange(i2 - 12, keyPara3.getTranspose());
                    break;
                }
                break;
            default:
                switch (i) {
                    case 15:
                        MidikeyboardUsrFragment midikeyboardUsrFragment7 = this.mListener;
                        if (midikeyboardUsrFragment7 != null) {
                            midikeyboardUsrFragment7.knobTypeChange(i2);
                        }
                        break;
                    case 16:
                        MidikeyboardUsrFragment midikeyboardUsrFragment8 = this.mListener;
                        UsrBinding usrBinding2 = this.mUsrBinding;
                        Usr usr6 = this.mData;
                        if (midikeyboardUsrFragment8 != null && usr6 != null && (encoderPara = usr6.getEncoderPara()) != null && usrBinding2 != null && (encoderPara2 = (EncoderPara) getFromArray(encoderPara, usrBinding2.getKnobIndex())) != null) {
                            midikeyboardUsrFragment8.U8DataChange(i2, encoderPara2.getCurve());
                            break;
                        }
                        break;
                    case 17:
                        MidikeyboardUsrFragment midikeyboardUsrFragment9 = this.mListener;
                        UsrBinding usrBinding3 = this.mUsrBinding;
                        Usr usr7 = this.mData;
                        if (midikeyboardUsrFragment9 != null && usr7 != null && (encoderPara3 = usr7.getEncoderPara()) != null && usrBinding3 != null && (encoderPara4 = (EncoderPara) getFromArray(encoderPara3, usrBinding3.getKnobIndex())) != null) {
                            midikeyboardUsrFragment9.U8DataChange(i2, encoderPara4.getCh());
                            break;
                        }
                        break;
                    default:
                        switch (i) {
                            case 25:
                                MidikeyboardUsrFragment midikeyboardUsrFragment10 = this.mListener;
                                Usr usr8 = this.mData;
                                if (midikeyboardUsrFragment10 != null && usr8 != null && (pitchPara = usr8.getPitchPara()) != null) {
                                    midikeyboardUsrFragment10.U8DataChange(i2, pitchPara.getCh());
                                    break;
                                }
                                break;
                            case 26:
                                MidikeyboardUsrFragment midikeyboardUsrFragment11 = this.mListener;
                                Usr usr9 = this.mData;
                                if (midikeyboardUsrFragment11 != null && usr9 != null && (pedalPara = usr9.getPedalPara()) != null) {
                                    midikeyboardUsrFragment11.U8DataChange(i2, pedalPara.getType());
                                    break;
                                }
                                break;
                            case 27:
                                MidikeyboardUsrFragment midikeyboardUsrFragment12 = this.mListener;
                                Usr usr10 = this.mData;
                                if (midikeyboardUsrFragment12 != null && usr10 != null && (pedalPara2 = usr10.getPedalPara()) != null) {
                                    midikeyboardUsrFragment12.U8DataChange(i2, pedalPara2.getCh());
                                    break;
                                }
                                break;
                            default:
                                switch (i) {
                                    case 31:
                                        MidikeyboardUsrFragment midikeyboardUsrFragment13 = this.mListener;
                                        if (midikeyboardUsrFragment13 != null) {
                                            midikeyboardUsrFragment13.psrChange(i2);
                                        }
                                        break;
                                    case 32:
                                        MidikeyboardUsrFragment midikeyboardUsrFragment14 = this.mListener;
                                        if (midikeyboardUsrFragment14 != null) {
                                            midikeyboardUsrFragment14.psrModeChange(i2);
                                        }
                                        break;
                                    case 33:
                                        MidikeyboardUsrFragment midikeyboardUsrFragment15 = this.mListener;
                                        UsrBinding usrBinding4 = this.mUsrBinding;
                                        Usr usr11 = this.mData;
                                        if (midikeyboardUsrFragment15 != null && usr11 != null && (btnPara = usr11.getBtnPara()) != null && usrBinding4 != null && (btnPara2 = (BtnPara) getFromArray(btnPara, usrBinding4.getPsrIndex())) != null) {
                                            midikeyboardUsrFragment15.U8DataChange(i2, btnPara2.getCh());
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
    }

    @Override // com.cubeSuite.generated.callback.OnProgressChanged.Listener
    public final void _internalCallbackOnProgressChanged(int i, SeekBar seekBar, int i2, boolean z) {
        MidikeyboardUsrFragment midikeyboardUsrFragment;
        if (i != 2) {
            if (i == 14 && (midikeyboardUsrFragment = this.mListener) != null) {
                midikeyboardUsrFragment.knobChange(i2);
                return;
            }
            return;
        }
        MidikeyboardUsrFragment midikeyboardUsrFragment2 = this.mListener;
        if (midikeyboardUsrFragment2 != null) {
            midikeyboardUsrFragment2.padChange(i2);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnCheckedChangeListener.Listener
    public final void _internalCallbackOnCheckedChanged(int i, RadioGroup radioGroup, int i2) {
        MidikeyboardUsrFragment midikeyboardUsrFragment = this.mListener;
        if (midikeyboardUsrFragment != null) {
            midikeyboardUsrFragment.itemSelectChange(radioGroup);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnClickListener.Listener
    public final void _internalCallbackOnClick(int i, View view) {
        PadPara[] padPara;
        PadPara padPara2;
        MidikeyboardUsrFragment midikeyboardUsrFragment = this.mListener;
        UsrBinding usrBinding = this.mUsrBinding;
        Usr usr = this.mData;
        if (midikeyboardUsrFragment == null || usr == null || (padPara = usr.getPadPara()) == null || usrBinding == null || (padPara2 = (PadPara) getFromArray(padPara, usrBinding.getPadIndex())) == null) {
            return;
        }
        midikeyboardUsrFragment.showColorDialog(padPara2.getColor());
    }
}
