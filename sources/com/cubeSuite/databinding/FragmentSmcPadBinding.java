package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.fragment.smcPad.SmcPadButton;
import com.cubeSuite.fragment.smcPad.SmcPadKnob;
import com.cubeSuite.fragment.smcPad.SmcPadPad;
import com.google.android.material.tabs.TabLayout;

/* JADX INFO: loaded from: classes.dex */
public final class FragmentSmcPadBinding implements ViewBinding {
    public final Button glob;
    public final Button noteRepeat;
    public final Button reset;
    private final LinearLayout rootView;
    public final Button save;
    public final SmcPadButton smcPadButton;
    public final SmcPadKnob smcPadKnob;
    public final SmcPadPad smcPadPad;
    public final TabLayout tabLayout;

    private FragmentSmcPadBinding(LinearLayout linearLayout, Button button, Button button2, Button button3, Button button4, SmcPadButton smcPadButton, SmcPadKnob smcPadKnob, SmcPadPad smcPadPad, TabLayout tabLayout) {
        this.rootView = linearLayout;
        this.glob = button;
        this.noteRepeat = button2;
        this.reset = button3;
        this.save = button4;
        this.smcPadButton = smcPadButton;
        this.smcPadKnob = smcPadKnob;
        this.smcPadPad = smcPadPad;
        this.tabLayout = tabLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static FragmentSmcPadBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FragmentSmcPadBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fragment_smc_pad, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FragmentSmcPadBinding bind(View view) {
        int i = R.id.glob;
        Button button = (Button) ViewBindings.findChildViewById(view, R.id.glob);
        if (button != null) {
            i = R.id.note_repeat;
            Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.note_repeat);
            if (button2 != null) {
                i = R.id.reset;
                Button button3 = (Button) ViewBindings.findChildViewById(view, R.id.reset);
                if (button3 != null) {
                    i = R.id.save;
                    Button button4 = (Button) ViewBindings.findChildViewById(view, R.id.save);
                    if (button4 != null) {
                        i = R.id.smc_pad_button;
                        SmcPadButton smcPadButton = (SmcPadButton) ViewBindings.findChildViewById(view, R.id.smc_pad_button);
                        if (smcPadButton != null) {
                            i = R.id.smc_pad_knob;
                            SmcPadKnob smcPadKnob = (SmcPadKnob) ViewBindings.findChildViewById(view, R.id.smc_pad_knob);
                            if (smcPadKnob != null) {
                                i = R.id.smc_pad_pad;
                                SmcPadPad smcPadPad = (SmcPadPad) ViewBindings.findChildViewById(view, R.id.smc_pad_pad);
                                if (smcPadPad != null) {
                                    i = R.id.tab_layout;
                                    TabLayout tabLayout = (TabLayout) ViewBindings.findChildViewById(view, R.id.tab_layout);
                                    if (tabLayout != null) {
                                        return new FragmentSmcPadBinding((LinearLayout) view, button, button2, button3, button4, smcPadButton, smcPadKnob, smcPadPad, tabLayout);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
