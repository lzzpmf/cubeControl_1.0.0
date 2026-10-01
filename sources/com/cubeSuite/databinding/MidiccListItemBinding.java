package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class MidiccListItemBinding implements ViewBinding {
    public final TextView leftText;
    public final TextView rightText;
    private final ConstraintLayout rootView;

    private MidiccListItemBinding(ConstraintLayout constraintLayout, TextView textView, TextView textView2) {
        this.rootView = constraintLayout;
        this.leftText = textView;
        this.rightText = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static MidiccListItemBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static MidiccListItemBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.midicc_list_item, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static MidiccListItemBinding bind(View view) {
        int i = R.id.leftText;
        TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.leftText);
        if (textView != null) {
            i = R.id.rightText;
            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.rightText);
            if (textView2 != null) {
                return new MidiccListItemBinding((ConstraintLayout) view, textView, textView2);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
