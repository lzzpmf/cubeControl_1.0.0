package com.cubeSuite.databinding;

import android.util.SparseIntArray;
import android.view.View;
import android.widget.AdapterView;
import android.widget.LinearLayout;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.AdapterViewBindingAdapter;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Shift;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniScChFragment;
import com.cubeSuite.generated.callback.OnItemSelected;

/* JADX INFO: loaded from: classes.dex */
public class Smk25miniScChFragmentBindingImpl extends Smk25miniScChFragmentBinding implements OnItemSelected.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds = null;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback86;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback87;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback88;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;

    public Smk25miniScChFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 4, sIncludes, sViewsWithIds));
    }

    private Smk25miniScChFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 3, (AppCompatSpinner) objArr[3], (AppCompatSpinner) objArr[2], (AppCompatSpinner) objArr[1]);
        this.mDirtyFlags = -1L;
        this.chord.setTag(null);
        this.keyCurve.setTag(null);
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        this.type.setTag(null);
        setRootTag(view);
        this.mCallback87 = new OnItemSelected(this, 2);
        this.mCallback86 = new OnItemSelected(this, 1);
        this.mCallback88 = new OnItemSelected(this, 3);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 256L;
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
            setListener((Smk25MiniScChFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Shift) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25miniScChFragmentBinding
    public void setListener(Smk25MiniScChFragment smk25MiniScChFragment) {
        this.mListener = smk25MiniScChFragment;
        synchronized (this) {
            this.mDirtyFlags |= 8;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniScChFragmentBinding
    public void setData(Shift shift) {
        this.mData = shift;
        synchronized (this) {
            this.mDirtyFlags |= 16;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        if (i == 0) {
            return onChangeDataChord((AddrU8) obj, i2);
        }
        if (i == 1) {
            return onChangeDataMajor((AddrU8) obj, i2);
        }
        if (i != 2) {
            return false;
        }
        return onChangeDataScale((AddrU8) obj, i2);
    }

    private boolean onChangeDataChord(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 32;
        }
        return true;
    }

    private boolean onChangeDataMajor(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 64;
        }
        return true;
    }

    private boolean onChangeDataScale(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 128;
        }
        return true;
    }

    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        int data;
        boolean z;
        int iMin;
        int data2;
        boolean z2;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        Smk25MiniScChFragment smk25MiniScChFragment = this.mListener;
        Shift shift = this.mData;
        int i = 0;
        if ((503 & j) != 0) {
            long j3 = j & 305;
            if (j3 != 0) {
                AddrU8 chord = shift != null ? shift.getChord() : null;
                j2 = 338;
                updateRegistration(0, chord);
                data2 = chord != null ? chord.getData() : 0;
                z2 = data2 == 255;
                if (j3 != 0) {
                    j = z2 ? j | 1024 : j | 512;
                }
            } else {
                j2 = 338;
                data2 = 0;
                z2 = false;
            }
            if ((j & j2) != 0) {
                AddrU8 major = shift != null ? shift.getMajor() : null;
                updateRegistration(1, major);
                iMin = Math.min(major != null ? major.getData() : 0, 1);
            } else {
                iMin = 0;
            }
            long j4 = j & 404;
            if (j4 != 0) {
                AddrU8 scale = shift != null ? shift.getScale() : null;
                updateRegistration(2, scale);
                data = scale != null ? scale.getData() : 0;
                z = data == 255;
                if (j4 != 0) {
                    j = z ? j | 4096 : j | 2048;
                }
            } else {
                data = 0;
                z = false;
            }
        } else {
            j2 = 338;
            data = 0;
            z = false;
            iMin = 0;
            data2 = 0;
            z2 = false;
        }
        int iMin2 = (2048 & j) != 0 ? Math.min(data + 1, 11) : 0;
        int i2 = (512 & j) != 0 ? data2 + 1 : 0;
        long j5 = j & 305;
        if (j5 == 0 || z2) {
            i2 = 0;
        }
        long j6 = j & 404;
        if (j6 != 0 && !z) {
            i = iMin2;
        }
        if (j5 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.chord, i2);
        }
        if ((256 & j) != 0) {
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.chord, this.mCallback88, null, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keyCurve, this.mCallback87, null, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.type, this.mCallback86, null, null);
        }
        if (j6 != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keyCurve, i);
        }
        if ((j & j2) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.type, iMin);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        if (i == 1) {
            Smk25MiniScChFragment smk25MiniScChFragment = this.mListener;
            Shift shift = this.mData;
            if (smk25MiniScChFragment == null || shift == null) {
                return;
            }
            smk25MiniScChFragment.U8DataChange(i2, shift.getMajor());
            return;
        }
        if (i == 2) {
            Smk25MiniScChFragment smk25MiniScChFragment2 = this.mListener;
            Shift shift2 = this.mData;
            if (smk25MiniScChFragment2 != null) {
                if (i2 == 0) {
                    if (shift2 != null) {
                        smk25MiniScChFragment2.U8DataChange(i2 != 0 ? i2 - 1 : 255, shift2.getScale());
                        return;
                    }
                    return;
                } else {
                    if (shift2 != null) {
                        smk25MiniScChFragment2.U8DataChange(i2 != 0 ? i2 - 1 : 255, shift2.getScale());
                        return;
                    }
                    return;
                }
            }
            return;
        }
        if (i != 3) {
            return;
        }
        Smk25MiniScChFragment smk25MiniScChFragment3 = this.mListener;
        Shift shift3 = this.mData;
        if (smk25MiniScChFragment3 != null) {
            if (i2 == 0) {
                if (shift3 != null) {
                    smk25MiniScChFragment3.U8DataChange(i2 != 0 ? i2 - 1 : 255, shift3.getChord());
                }
            } else if (shift3 != null) {
                smk25MiniScChFragment3.U8DataChange(i2 != 0 ? i2 - 1 : 255, shift3.getChord());
            }
        }
    }
}
