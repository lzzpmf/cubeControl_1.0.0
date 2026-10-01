package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.BlockProgressBar;

/* JADX INFO: loaded from: classes.dex */
public final class SequencerItemBinding implements ViewBinding {
    public final BlockProgressBar blockItem;
    private final FrameLayout rootView;

    private SequencerItemBinding(FrameLayout frameLayout, BlockProgressBar blockProgressBar) {
        this.rootView = frameLayout;
        this.blockItem = blockProgressBar;
    }

    @Override // androidx.viewbinding.ViewBinding
    public FrameLayout getRoot() {
        return this.rootView;
    }

    public static SequencerItemBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SequencerItemBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.sequencer_item, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SequencerItemBinding bind(View view) {
        BlockProgressBar blockProgressBar = (BlockProgressBar) ViewBindings.findChildViewById(view, R.id.blockItem);
        if (blockProgressBar != null) {
            return new SequencerItemBinding((FrameLayout) view, blockProgressBar);
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(R.id.blockItem)));
    }
}
