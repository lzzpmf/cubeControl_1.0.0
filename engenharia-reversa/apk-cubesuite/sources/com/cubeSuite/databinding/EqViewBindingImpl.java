package com.cubeSuite.databinding;

import android.util.SparseIntArray;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.R;
import com.cubeSuite.customControl.EQPic;

/* JADX INFO: loaded from: classes.dex */
public class EqViewBindingImpl extends EqViewBinding {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        return false;
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean setVariable(int i, Object obj) {
        return true;
    }

    static {
        SparseIntArray sparseIntArray = new SparseIntArray();
        sViewsWithIds = sparseIntArray;
        sparseIntArray.put(R.id.eqPic, 1);
        sparseIntArray.put(R.id.enable_tv, 2);
        sparseIntArray.put(R.id.enable_switch, 3);
        sparseIntArray.put(R.id.reset_but, 4);
        sparseIntArray.put(R.id.peak_spinner, 5);
        sparseIntArray.put(R.id.hz_minus, 6);
        sparseIntArray.put(R.id.hz_text, 7);
        sparseIntArray.put(R.id.hz, 8);
        sparseIntArray.put(R.id.hz_add, 9);
        sparseIntArray.put(R.id.db_container, 10);
        sparseIntArray.put(R.id.db_minus, 11);
        sparseIntArray.put(R.id.db_text, 12);
        sparseIntArray.put(R.id.db, 13);
        sparseIntArray.put(R.id.db_add, 14);
        sparseIntArray.put(R.id.q_container, 15);
        sparseIntArray.put(R.id.q_minus, 16);
        sparseIntArray.put(R.id.q_text, 17);
        sparseIntArray.put(R.id.q, 18);
        sparseIntArray.put(R.id.q_add, 19);
    }

    public EqViewBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 20, sIncludes, sViewsWithIds));
    }

    private EqViewBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 0, (SeekBar) objArr[13], (ImageView) objArr[14], (LinearLayout) objArr[10], (ImageView) objArr[11], (TextView) objArr[12], (SwitchCompat) objArr[3], (TextView) objArr[2], (EQPic) objArr[1], (SeekBar) objArr[8], (ImageView) objArr[9], (ImageView) objArr[6], (TextView) objArr[7], (AppCompatSpinner) objArr[5], (SeekBar) objArr[18], (ImageView) objArr[19], (LinearLayout) objArr[15], (ImageView) objArr[16], (TextView) objArr[17], (Button) objArr[4]);
        this.mDirtyFlags = -1L;
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        setRootTag(view);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 1L;
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
    protected void executeBindings() {
        synchronized (this) {
            this.mDirtyFlags = 0L;
        }
    }
}
