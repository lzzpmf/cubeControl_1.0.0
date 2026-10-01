package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class MidiPortbFragmentBinding implements ViewBinding {
    public final Button deviceOnlyMode;
    public final Button hostOnlyMode;
    public final ImageView imageView;
    public final LinearLayout main;
    public final Button otgMode;
    private final LinearLayout rootView;

    private MidiPortbFragmentBinding(LinearLayout linearLayout, Button button, Button button2, ImageView imageView, LinearLayout linearLayout2, Button button3) {
        this.rootView = linearLayout;
        this.deviceOnlyMode = button;
        this.hostOnlyMode = button2;
        this.imageView = imageView;
        this.main = linearLayout2;
        this.otgMode = button3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static MidiPortbFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static MidiPortbFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.midi_portb_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static MidiPortbFragmentBinding bind(View view) {
        int i = R.id.deviceOnlyMode;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.deviceOnlyMode);
        if (button != null) {
            i = R.id.hostOnlyMode;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.hostOnlyMode);
            if (button2 != null) {
                i = R.id.imageView;
                ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.imageView);
                if (imageView != null) {
                    LinearLayout linearLayout = (LinearLayout) view;
                    i = R.id.otgMode;
                    Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.otgMode);
                    if (button3 != null) {
                        return new MidiPortbFragmentBinding(linearLayout, button, button2, imageView, linearLayout, button3);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
