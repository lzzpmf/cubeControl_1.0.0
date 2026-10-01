package com.cubeSuite.databinding;

import android.util.SparseIntArray;
import android.view.View;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.entity.smk25Mini.Smk25MiniGlob;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniPresetsFragment;
import com.cubeSuite.generated.callback.OnClickListener;

/* JADX INFO: loaded from: classes.dex */
public class Smk25miniPresetsFragmentBindingImpl extends Smk25miniPresetsFragmentBinding implements OnClickListener.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private final View.OnClickListener mCallback4;
    private final View.OnClickListener mCallback5;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;
    private final Button mboundView1;
    private final Button mboundView2;

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        return false;
    }

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.usrFragment, 3);
    }

    public Smk25miniPresetsFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 4, sIncludes, sViewsWithIds));
    }

    private Smk25miniPresetsFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 0, (FrameLayout) objArr[3]);
        this.mDirtyFlags = -1L;
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        Button button = (Button) objArr[1];
        this.mboundView1 = button;
        button.setTag(null);
        Button button2 = (Button) objArr[2];
        this.mboundView2 = button2;
        button2.setTag(null);
        setRootTag(view);
        this.mCallback5 = new OnClickListener(this, 2);
        this.mCallback4 = new OnClickListener(this, 1);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 4L;
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
            setListener((Smk25MiniPresetsFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Smk25MiniGlob) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25miniPresetsFragmentBinding
    public void setListener(Smk25MiniPresetsFragment smk25MiniPresetsFragment) {
        this.mListener = smk25MiniPresetsFragment;
        synchronized (this) {
            this.mDirtyFlags |= 1;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniPresetsFragmentBinding
    public void setData(Smk25MiniGlob smk25MiniGlob) {
        this.mData = smk25MiniGlob;
    }

    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        Smk25MiniPresetsFragment smk25MiniPresetsFragment = this.mListener;
        if ((j & 4) != 0) {
            this.mboundView1.setOnClickListener(this.mCallback4);
            this.mboundView2.setOnClickListener(this.mCallback5);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnClickListener.Listener
    public final void _internalCallbackOnClick(int i, View view) {
        Smk25MiniPresetsFragment smk25MiniPresetsFragment;
        if (i != 1) {
            if (i == 2 && (smk25MiniPresetsFragment = this.mListener) != null) {
                smk25MiniPresetsFragment.resetConfig();
                return;
            }
            return;
        }
        Smk25MiniPresetsFragment smk25MiniPresetsFragment2 = this.mListener;
        if (smk25MiniPresetsFragment2 != null) {
            smk25MiniPresetsFragment2.saveConfig();
        }
    }
}
