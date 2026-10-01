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
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniGlobFragment;
import com.cubeSuite.generated.callback.OnItemSelected;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniGlobFragmentBindingImpl extends Smk25MiniGlobFragmentBinding implements OnItemSelected.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds = null;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback46;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback47;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;

    public Smk25MiniGlobFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 3, sIncludes, sViewsWithIds));
    }

    private Smk25MiniGlobFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 2, (AppCompatSpinner) objArr[1], (AppCompatSpinner) objArr[2]);
        this.mDirtyFlags = -1L;
        this.keyCurve.setTag(null);
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        this.port.setTag(null);
        setRootTag(view);
        this.mCallback47 = new OnItemSelected(this, 2);
        this.mCallback46 = new OnItemSelected(this, 1);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 64L;
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
            setListener((Smk25MiniGlobFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Smk25MiniGlob) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25MiniGlobFragmentBinding
    public void setListener(Smk25MiniGlobFragment smk25MiniGlobFragment) {
        this.mListener = smk25MiniGlobFragment;
        synchronized (this) {
            this.mDirtyFlags |= 4;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25MiniGlobFragmentBinding
    public void setData(Smk25MiniGlob smk25MiniGlob) {
        this.mData = smk25MiniGlob;
        synchronized (this) {
            this.mDirtyFlags |= 8;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        if (i == 0) {
            return onChangeDataInputMode((AddrU8) obj, i2);
        }
        if (i != 1) {
            return false;
        }
        return onChangeDataKeyCurve((AddrU8) obj, i2);
    }

    private boolean onChangeDataInputMode(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 16;
        }
        return true;
    }

    private boolean onChangeDataKeyCurve(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 32;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:15:0x0031  */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        int data;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        Smk25MiniGlobFragment smk25MiniGlobFragment = this.mListener;
        Smk25MiniGlob smk25MiniGlob = this.mData;
        int data2 = 0;
        if ((123 & j) != 0) {
            if ((j & 89) == 0) {
                data = 0;
            } else {
                AddrU8 inputMode = smk25MiniGlob != null ? smk25MiniGlob.getInputMode() : null;
                updateRegistration(0, inputMode);
                if (inputMode != null) {
                    data = inputMode.getData();
                } else {
                    data = 0;
                }
            }
            if ((j & 106) != 0) {
                AddrU8 keyCurve = smk25MiniGlob != null ? smk25MiniGlob.getKeyCurve() : null;
                updateRegistration(1, keyCurve);
                if (keyCurve != null) {
                    data2 = keyCurve.getData();
                }
            }
        } else {
            data = 0;
        }
        if ((106 & j) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.keyCurve, data2);
        }
        if ((64 & j) != 0) {
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.keyCurve, this.mCallback46, null, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.port, this.mCallback47, null, null);
        }
        if ((j & 89) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.port, data);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        if (i == 1) {
            Smk25MiniGlobFragment smk25MiniGlobFragment = this.mListener;
            Smk25MiniGlob smk25MiniGlob = this.mData;
            if (smk25MiniGlobFragment == null || smk25MiniGlob == null) {
                return;
            }
            smk25MiniGlobFragment.U8DataChange(i2, smk25MiniGlob.getKeyCurve());
            return;
        }
        if (i != 2) {
            return;
        }
        Smk25MiniGlobFragment smk25MiniGlobFragment2 = this.mListener;
        Smk25MiniGlob smk25MiniGlob2 = this.mData;
        if (smk25MiniGlobFragment2 == null || smk25MiniGlob2 == null) {
            return;
        }
        smk25MiniGlobFragment2.U8DataChange(i2, smk25MiniGlob2.getInputMode());
    }
}
