package com.cubeSuite.databinding;

import android.util.SparseIntArray;
import android.view.View;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.SeekBarBindingAdapter;
import androidx.databinding.adapters.TextViewBindingAdapter;
import com.cubeSuite.R;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;
import com.cubeSuite.fragment.midikeyboard.MidikeyboardPresetsFragment;
import com.cubeSuite.generated.callback.OnClickListener;
import com.cubeSuite.generated.callback.OnProgressChanged;

/* JADX INFO: loaded from: classes.dex */
public class PresetsFragmentBindingImpl extends PresetsFragmentBinding implements OnClickListener.Listener, OnProgressChanged.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private final SeekBarBindingAdapter.OnProgressChanged mCallback126;
    private final View.OnClickListener mCallback127;
    private final View.OnClickListener mCallback128;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;
    private final TextView mboundView1;
    private final Button mboundView3;
    private final Button mboundView4;

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.usrFragment, 5);
    }

    public PresetsFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 6, sIncludes, sViewsWithIds));
    }

    private PresetsFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 1, (SeekBar) objArr[2], (FrameLayout) objArr[5]);
        this.mDirtyFlags = -1L;
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        TextView textView = (TextView) objArr[1];
        this.mboundView1 = textView;
        textView.setTag(null);
        Button button = (Button) objArr[3];
        this.mboundView3 = button;
        button.setTag(null);
        Button button2 = (Button) objArr[4];
        this.mboundView4 = button2;
        button2.setTag(null);
        this.preset.setTag(null);
        setRootTag(view);
        this.mCallback128 = new OnClickListener(this, 3);
        this.mCallback126 = new OnProgressChanged(this, 1);
        this.mCallback127 = new OnClickListener(this, 2);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 16L;
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
            setListener((MidikeyboardPresetsFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Glob) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.PresetsFragmentBinding
    public void setListener(MidikeyboardPresetsFragment midikeyboardPresetsFragment) {
        this.mListener = midikeyboardPresetsFragment;
        synchronized (this) {
            this.mDirtyFlags |= 2;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.PresetsFragmentBinding
    public void setData(Glob glob) {
        this.mData = glob;
        synchronized (this) {
            this.mDirtyFlags |= 4;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        if (i != 0) {
            return false;
        }
        return onChangeDataPreset((AddrU8) obj, i2);
    }

    private boolean onChangeDataPreset(AddrU8 addrU8, int i) {
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
            this.mDirtyFlags |= 8;
        }
        return true;
    }

    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        String strValueOf;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        MidikeyboardPresetsFragment midikeyboardPresetsFragment = this.mListener;
        Glob glob = this.mData;
        long j2 = 29 & j;
        int data = 0;
        if (j2 != 0) {
            AddrU8 preset = glob != null ? glob.getPreset() : null;
            updateRegistration(0, preset);
            data = preset != null ? preset.getData() : 0;
            strValueOf = String.valueOf(data + 1);
        } else {
            strValueOf = null;
        }
        if (j2 != 0) {
            TextViewBindingAdapter.setText(this.mboundView1, strValueOf);
            SeekBarBindingAdapter.setProgress(this.preset, data);
        }
        if ((j & 16) != 0) {
            this.mboundView3.setOnClickListener(this.mCallback127);
            this.mboundView4.setOnClickListener(this.mCallback128);
            SeekBarBindingAdapter.setOnSeekBarChangeListener(this.preset, null, null, this.mCallback126, null);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnClickListener.Listener
    public final void _internalCallbackOnClick(int i, View view) {
        MidikeyboardPresetsFragment midikeyboardPresetsFragment;
        if (i != 2) {
            if (i == 3 && (midikeyboardPresetsFragment = this.mListener) != null) {
                midikeyboardPresetsFragment.resetConfig();
                return;
            }
            return;
        }
        MidikeyboardPresetsFragment midikeyboardPresetsFragment2 = this.mListener;
        if (midikeyboardPresetsFragment2 != null) {
            midikeyboardPresetsFragment2.saveConfig();
        }
    }

    @Override // com.cubeSuite.generated.callback.OnProgressChanged.Listener
    public final void _internalCallbackOnProgressChanged(int i, SeekBar seekBar, int i2, boolean z) {
        MidikeyboardPresetsFragment midikeyboardPresetsFragment = this.mListener;
        Glob glob = this.mData;
        if (midikeyboardPresetsFragment == null || glob == null) {
            return;
        }
        midikeyboardPresetsFragment.changeConfig(i2, glob.getPreset());
    }
}
