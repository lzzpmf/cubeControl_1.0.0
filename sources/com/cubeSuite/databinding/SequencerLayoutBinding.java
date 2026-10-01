package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.HistogramView;
import com.cubeSuite.customControl.ScrollViewPlus;

/* JADX INFO: loaded from: classes.dex */
public final class SequencerLayoutBinding implements ViewBinding {
    public final ButtonIconView addBeat;
    public final ButtonIconView addVelocity;
    public final ButtonIconView back;
    public final TextView beatText;
    public final LinearLayout blockList;
    public final ScrollViewPlus blockSV;
    public final LinearLayout bottom;
    public final ButtonIconView btnLoadDrumList;
    public final FrameLayout head;
    public final HistogramView linkageHistogram;
    public final ButtonIconView loadDrum;
    public final RelativeLayout main;
    public final RadioGroup pageSelect;
    public final ButtonIconView playBeat;
    public final RadioButton power;
    public final RadioGroup powerStep;
    private final RelativeLayout rootView;
    public final ButtonIconView saveDrumData;
    public final ButtonIconView share;
    public final RadioButton step;
    public final ButtonIconView subBeat;
    public final ButtonIconView subVelocity;
    public final ScrollViewPlus toneSV;
    public final LinearLayout toneStrList;
    public final ButtonIconView velocity;
    public final Button velocityText;

    private SequencerLayoutBinding(RelativeLayout relativeLayout, ButtonIconView buttonIconView, ButtonIconView buttonIconView2, ButtonIconView buttonIconView3, TextView textView, LinearLayout linearLayout, ScrollViewPlus scrollViewPlus, LinearLayout linearLayout2, ButtonIconView buttonIconView4, FrameLayout frameLayout, HistogramView histogramView, ButtonIconView buttonIconView5, RelativeLayout relativeLayout2, RadioGroup radioGroup, ButtonIconView buttonIconView6, RadioButton radioButton, RadioGroup radioGroup2, ButtonIconView buttonIconView7, ButtonIconView buttonIconView8, RadioButton radioButton2, ButtonIconView buttonIconView9, ButtonIconView buttonIconView10, ScrollViewPlus scrollViewPlus2, LinearLayout linearLayout3, ButtonIconView buttonIconView11, Button button) {
        this.rootView = relativeLayout;
        this.addBeat = buttonIconView;
        this.addVelocity = buttonIconView2;
        this.back = buttonIconView3;
        this.beatText = textView;
        this.blockList = linearLayout;
        this.blockSV = scrollViewPlus;
        this.bottom = linearLayout2;
        this.btnLoadDrumList = buttonIconView4;
        this.head = frameLayout;
        this.linkageHistogram = histogramView;
        this.loadDrum = buttonIconView5;
        this.main = relativeLayout2;
        this.pageSelect = radioGroup;
        this.playBeat = buttonIconView6;
        this.power = radioButton;
        this.powerStep = radioGroup2;
        this.saveDrumData = buttonIconView7;
        this.share = buttonIconView8;
        this.step = radioButton2;
        this.subBeat = buttonIconView9;
        this.subVelocity = buttonIconView10;
        this.toneSV = scrollViewPlus2;
        this.toneStrList = linearLayout3;
        this.velocity = buttonIconView11;
        this.velocityText = button;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static SequencerLayoutBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static SequencerLayoutBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.sequencer_layout, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static SequencerLayoutBinding bind(View view) {
        int i = R.id.addBeat;
        ButtonIconView buttonIconView = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.addBeat);
        if (buttonIconView != null) {
            i = R.id.addVelocity;
            ButtonIconView buttonIconView2 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.addVelocity);
            if (buttonIconView2 != null) {
                i = R.id.back;
                ButtonIconView buttonIconView3 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.back);
                if (buttonIconView3 != null) {
                    i = R.id.beatText;
                    TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.beatText);
                    if (textView != null) {
                        i = R.id.blockList;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, R.id.blockList);
                        if (linearLayout != null) {
                            i = R.id.blockSV;
                            ScrollViewPlus scrollViewPlus = (ScrollViewPlus) ViewBindings.findChildViewById(view, R.id.blockSV);
                            if (scrollViewPlus != null) {
                                i = R.id.bottom;
                                LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.bottom);
                                if (linearLayout2 != null) {
                                    i = R.id.btnLoadDrumList;
                                    ButtonIconView buttonIconView4 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.btnLoadDrumList);
                                    if (buttonIconView4 != null) {
                                        i = R.id.head;
                                        FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, R.id.head);
                                        if (frameLayout != null) {
                                            i = R.id.linkageHistogram;
                                            HistogramView histogramView = (HistogramView) ViewBindings.findChildViewById(view, R.id.linkageHistogram);
                                            if (histogramView != null) {
                                                i = R.id.loadDrum;
                                                ButtonIconView buttonIconView5 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.loadDrum);
                                                if (buttonIconView5 != null) {
                                                    RelativeLayout relativeLayout = (RelativeLayout) view;
                                                    i = R.id.pageSelect;
                                                    RadioGroup radioGroup = (RadioGroup) ViewBindings.findChildViewById(view, R.id.pageSelect);
                                                    if (radioGroup != null) {
                                                        i = R.id.playBeat;
                                                        ButtonIconView buttonIconView6 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.playBeat);
                                                        if (buttonIconView6 != null) {
                                                            i = R.id.power;
                                                            RadioButton radioButton = (RadioButton) ViewBindings.findChildViewById(view, R.id.power);
                                                            if (radioButton != null) {
                                                                i = R.id.powerStep;
                                                                RadioGroup radioGroup2 = (RadioGroup) ViewBindings.findChildViewById(view, R.id.powerStep);
                                                                if (radioGroup2 != null) {
                                                                    i = R.id.saveDrumData;
                                                                    ButtonIconView buttonIconView7 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.saveDrumData);
                                                                    if (buttonIconView7 != null) {
                                                                        i = R.id.share;
                                                                        ButtonIconView buttonIconView8 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.share);
                                                                        if (buttonIconView8 != null) {
                                                                            i = R.id.step;
                                                                            RadioButton radioButton2 = (RadioButton) ViewBindings.findChildViewById(view, R.id.step);
                                                                            if (radioButton2 != null) {
                                                                                i = R.id.subBeat;
                                                                                ButtonIconView buttonIconView9 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.subBeat);
                                                                                if (buttonIconView9 != null) {
                                                                                    i = R.id.subVelocity;
                                                                                    ButtonIconView buttonIconView10 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.subVelocity);
                                                                                    if (buttonIconView10 != null) {
                                                                                        i = R.id.toneSV;
                                                                                        ScrollViewPlus scrollViewPlus2 = (ScrollViewPlus) ViewBindings.findChildViewById(view, R.id.toneSV);
                                                                                        if (scrollViewPlus2 != null) {
                                                                                            i = R.id.toneStrList;
                                                                                            LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, R.id.toneStrList);
                                                                                            if (linearLayout3 != null) {
                                                                                                i = R.id.velocity;
                                                                                                ButtonIconView buttonIconView11 = (ButtonIconView) ViewBindings.findChildViewById(view, R.id.velocity);
                                                                                                if (buttonIconView11 != null) {
                                                                                                    i = R.id.velocityText;
                                                                                                    Button button = (Button) ViewBindings.findChildViewById(view, R.id.velocityText);
                                                                                                    if (button != null) {
                                                                                                        return new SequencerLayoutBinding(relativeLayout, buttonIconView, buttonIconView2, buttonIconView3, textView, linearLayout, scrollViewPlus, linearLayout2, buttonIconView4, frameLayout, histogramView, buttonIconView5, relativeLayout, radioGroup, buttonIconView6, radioButton, radioGroup2, buttonIconView7, buttonIconView8, radioButton2, buttonIconView9, buttonIconView10, scrollViewPlus2, linearLayout3, buttonIconView11, button);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
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
