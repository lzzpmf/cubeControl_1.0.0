package com.cubeSuite.databinding;

import android.text.Editable;
import android.util.SparseIntArray;
import android.view.View;
import android.widget.AdapterView;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.LinearLayout;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.AdapterViewBindingAdapter;
import androidx.databinding.adapters.CompoundButtonBindingAdapter;
import androidx.databinding.adapters.TextViewBindingAdapter;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardGlobFragment;
import com.cubeSuite.generated.callback.AfterTextChanged;
import com.cubeSuite.generated.callback.OnCheckedChangeListener1;
import com.cubeSuite.generated.callback.OnItemSelected;

/* JADX INFO: loaded from: classes.dex */
public class GlobFragmentBindingImpl extends GlobFragmentBinding implements OnCheckedChangeListener1.Listener, OnItemSelected.Listener, AfterTextChanged.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds = null;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback81;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback82;
    private final TextViewBindingAdapter.AfterTextChanged mCallback83;
    private final CompoundButton.OnCheckedChangeListener mCallback84;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback85;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;

    public GlobFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 6, sIncludes, sViewsWithIds));
    }

    private GlobFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 5, (EditText) objArr[3], (AppCompatSpinner) objArr[2], (SwitchCompat) objArr[4], (AppCompatSpinner) objArr[1], (AppCompatSpinner) objArr[5]);
        this.mDirtyFlags = -1L;
        this.gate.setTag(null);
        this.keyCurve.setTag(null);
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        this.padAfterTouch.setTag(null);
        this.padCurve.setTag(null);
        this.port.setTag(null);
        setRootTag(view);
        this.mCallback84 = new OnCheckedChangeListener1(this, 4);
        this.mCallback82 = new OnItemSelected(this, 2);
        this.mCallback85 = new OnItemSelected(this, 5);
        this.mCallback83 = new AfterTextChanged(this, 3);
        this.mCallback81 = new OnItemSelected(this, 1);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 4096L;
        }
        requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean hasPendingBindings() {
        synchronized (this) {
            return this.mDirtyFlags != 0;
        }
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean setVariable(int i, Object obj) {
        if (9 == i) {
            setListener((MidikeyboardGlobFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Glob) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.GlobFragmentBinding
    public void setListener(MidikeyboardGlobFragment midikeyboardGlobFragment) {
        this.mListener = midikeyboardGlobFragment;
        synchronized (this) {
            this.mDirtyFlags |= 32;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.GlobFragmentBinding
    public void setData(Glob glob) {
        this.mData = glob;
        synchronized (this) {
            this.mDirtyFlags |= 64;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        if (i == 0) {
            return onChangeDataPadAft((AddrU8) obj, i2);
        }
        if (i == 1) {
            return onChangeDataInputMode((AddrU8) obj, i2);
        }
        if (i == 2) {
            return onChangeDataPadCurve((AddrU8) obj, i2);
        }
        if (i == 3) {
            return onChangeDataPitchDet((AddrU8) obj, i2);
        }
        if (i != 4) {
            return false;
        }
        return onChangeDataKeyCurve((AddrU8) obj, i2);
    }

    private boolean onChangeDataPadAft(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 128;
        }
        return true;
    }

    private boolean onChangeDataInputMode(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 256;
        }
        return true;
    }

    private boolean onChangeDataPadCurve(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 512;
        }
        return true;
    }

    private boolean onChangeDataPitchDet(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 1024;
        }
        return true;
    }

    private boolean onChangeDataKeyCurve(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 2048;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:19:0x003f  */
    /* JADX WARN: Code duplicated, block: B:28:0x0058  */
    /* JADX WARN: Code duplicated, block: B:37:0x0072  */
    /* JADX WARN: Code duplicated, block: B:57:0x00b5  */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        long j3;
        String strValueOf;
        int i;
        int data;
        boolean z;
        int data2;
        int data3;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        MidikeyboardGlobFragment midikeyboardGlobFragment = this.mListener;
        Glob glob = this.mData;
        if ((8159 & j) != 0) {
            j2 = 4676;
            if ((j & 4289) == 0) {
                z = false;
            } else {
                AddrU8 padAft = glob != null ? glob.getPadAft() : null;
                updateRegistration(0, padAft);
                if ((padAft != null ? padAft.getData() : 0) == 1) {
                    z = true;
                } else {
                    z = false;
                }
            }
            if ((j & 4418) == 0) {
                data2 = 0;
            } else {
                AddrU8 inputMode = glob != null ? glob.getInputMode() : null;
                updateRegistration(1, inputMode);
                if (inputMode != null) {
                    data2 = inputMode.getData();
                } else {
                    data2 = 0;
                }
            }
            if ((j & 4676) == 0) {
                data3 = 0;
            } else {
                AddrU8 padCurve = glob != null ? glob.getPadCurve() : null;
                updateRegistration(2, padCurve);
                if (padCurve != null) {
                    data3 = padCurve.getData();
                } else {
                    data3 = 0;
                }
            }
            if ((j & 5192) != 0) {
                AddrU8 pitchDet = glob != null ? glob.getPitchDet() : null;
                j3 = 6224;
                updateRegistration(3, pitchDet);
                strValueOf = String.valueOf(pitchDet != null ? pitchDet.getData() : 0);
            } else {
                j3 = 6224;
                strValueOf = null;
            }
            if ((j & j3) == 0) {
                i = data3;
                data = 0;
            } else {
                AddrU8 keyCurve = glob != null ? glob.getKeyCurve() : null;
                updateRegistration(4, keyCurve);
                if (keyCurve != null) {
                    int i2 = data3;
                    data = keyCurve.getData();
                    i = i2;
                } else {
                    i = data3;
                    data = 0;
                }
            }
        } else {
            j2 = 4676;
            j3 = 6224;
            strValueOf = null;
            i = 0;
            data = 0;
            z = false;
            data2 = 0;
        }
        if ((j & 5192) != 0) {
            TextViewBindingAdapter.setText(this.gate, strValueOf);
        }
        if ((4096 & j) != 0) {
            TextViewBindingAdapter.setTextWatcher(this.gate, null, null, this.mCallback83, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keyCurve, this.mCallback82, null, null);
            CompoundButtonBindingAdapter.setListeners(this.padAfterTouch, this.mCallback84, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.padCurve, this.mCallback81, null, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.port, this.mCallback85, null, null);
        }
        if ((j & j3) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keyCurve, data);
        }
        if ((j & 4289) != 0) {
            CompoundButtonBindingAdapter.setChecked(this.padAfterTouch, z);
        }
        if ((j & j2) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.padCurve, i);
        }
        if ((j & 4418) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.port, data2);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnCheckedChangeListener1.Listener
    public final void _internalCallbackOnCheckedChanged1(int i, CompoundButton compoundButton, boolean z) {
        MidikeyboardGlobFragment midikeyboardGlobFragment = this.mListener;
        Glob glob = this.mData;
        if (midikeyboardGlobFragment != null) {
            if (z) {
                if (glob != null) {
                    midikeyboardGlobFragment.U8DataChange(z ? 1 : 0, glob.getPadAft());
                    return;
                }
                return;
            }
            if (glob != null) {
                midikeyboardGlobFragment.U8DataChange(z ? 1 : 0, glob.getPadAft());
            }
        }
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        if (i == 1) {
            MidikeyboardGlobFragment midikeyboardGlobFragment = this.mListener;
            Glob glob = this.mData;
            if (midikeyboardGlobFragment == null || glob == null) {
                return;
            }
            midikeyboardGlobFragment.U8DataChange(i2, glob.getPadCurve());
            return;
        }
        if (i == 2) {
            MidikeyboardGlobFragment midikeyboardGlobFragment2 = this.mListener;
            Glob glob2 = this.mData;
            if (midikeyboardGlobFragment2 == null || glob2 == null) {
                return;
            }
            midikeyboardGlobFragment2.U8DataChange(i2, glob2.getKeyCurve());
            return;
        }
        if (i != 5) {
            return;
        }
        MidikeyboardGlobFragment midikeyboardGlobFragment3 = this.mListener;
        Glob glob3 = this.mData;
        if (midikeyboardGlobFragment3 == null || glob3 == null) {
            return;
        }
        midikeyboardGlobFragment3.U8DataChange(i2, glob3.getInputMode());
    }

    @Override // com.cubeSuite.generated.callback.AfterTextChanged.Listener
    public final void _internalCallbackAfterTextChanged(int i, Editable editable) {
        MidikeyboardGlobFragment midikeyboardGlobFragment = this.mListener;
        Glob glob = this.mData;
        if (midikeyboardGlobFragment == null || glob == null) {
            return;
        }
        midikeyboardGlobFragment.setNum(editable, glob.getPitchDet(), 0, 255);
    }
}
