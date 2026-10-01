package com.cubeSuite.databinding;

import android.util.SparseIntArray;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.EqView;

/* JADX INFO: loaded from: classes.dex */
public class IrBoxContorlFragmentBindingImpl extends IrBoxContorlFragmentBinding {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds;
    private long mDirtyFlags;

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
        sparseIntArray.put(R.id.syn_but, 1);
        sparseIntArray.put(R.id.save_but, 2);
        sparseIntArray.put(R.id.effect_spinner, 3);
        sparseIntArray.put(R.id.type_radio, 4);
        sparseIntArray.put(R.id.ir_radio, 5);
        sparseIntArray.put(R.id.eq_radio, 6);
        sparseIntArray.put(R.id.ir_edit, 7);
        sparseIntArray.put(R.id.switch_ir, 8);
        sparseIntArray.put(R.id.ir_container, 9);
        sparseIntArray.put(R.id.ir_vol_text, 10);
        sparseIntArray.put(R.id.vol_minus, 11);
        sparseIntArray.put(R.id.irVolumeSeek, 12);
        sparseIntArray.put(R.id.vol_add, 13);
        sparseIntArray.put(R.id.open_file_but, 14);
        sparseIntArray.put(R.id.ir_rv, 15);
        sparseIntArray.put(R.id.edit_eq_view, 16);
        sparseIntArray.put(R.id.switch_eq, 17);
        sparseIntArray.put(R.id.eq_view, 18);
    }

    public IrBoxContorlFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 19, sIncludes, sViewsWithIds));
    }

    private IrBoxContorlFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 0, (LinearLayout) objArr[16], (AppCompatSpinner) objArr[3], (RadioButton) objArr[6], (EqView) objArr[18], (LinearLayout) objArr[9], (LinearLayout) objArr[7], (RadioButton) objArr[5], (RecyclerView) objArr[15], (TextView) objArr[10], (SeekBar) objArr[12], (LinearLayout) objArr[0], (Button) objArr[14], (Button) objArr[2], (SwitchCompat) objArr[17], (SwitchCompat) objArr[8], (Button) objArr[1], (RadioGroup) objArr[4], (ImageView) objArr[13], (ImageView) objArr[11]);
        this.mDirtyFlags = -1L;
        this.main.setTag(null);
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
