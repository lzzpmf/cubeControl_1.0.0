package com.cubeSuite.fragment;

import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.graphics.drawable.StateListDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.GridLayout;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.communication.SmcCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.MidiComBox;
import com.cubeSuite.customControl.SmcKnobAlert;
import com.cubeSuite.customControl.SmcSliderAlert;
import com.cubeSuite.customControl.SmcTrackAlert;
import com.cubeSuite.entity.SmcMixerEntry.SmcMixerEntry;
import com.cubeSuite.utils.FileUtils;
import java.lang.reflect.Array;
import java.util.Arrays;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class SmcMixerFragment extends ControlBaseFragment {
    private SmcMixerEntry entry;
    SmcKnobAlert m_knobAlert;
    private View m_mainView;
    private SmcCommunication smcCommunication;
    SmcTrackAlert smcGlobeAlert;
    SmcSliderAlert smcSliderAlert;
    SmcTrackAlert smcTrackAlert;
    int currentSelectedPresetIndex = 0;
    int currentSelectedKnobIndex = 0;
    int currentSelectedSliderIndex = 0;
    int currentSelectedTrackIndexI = 0;
    int currentSelectedTrackIndexJ = 0;
    int currentSelectedGlobeIndex = 0;
    private final Button[] m_knobButtonArr = new Button[8];
    private final Button[] m_sliderButtonArr = new Button[8];
    private final ImageButton[][] m_trackButtonArr = (ImageButton[][]) Array.newInstance((Class<?>) ImageButton.class, 8, 4);
    private final ImageButton[] m_globButtonArr = new ImageButton[11];
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.SmcMixerFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            if (SmcMixerFragment.this.smcCommunication == null) {
                SmcMixerFragment.this.smcCommunication = new SmcCommunication(ControlBaseFragment.communicationUtil);
                SmcMixerFragment.this.m_knobAlert.smcCommunication = SmcMixerFragment.this.smcCommunication;
                SmcMixerFragment.this.smcSliderAlert.smcCommunication = SmcMixerFragment.this.smcCommunication;
                SmcMixerFragment.this.smcTrackAlert.smcCommunication = SmcMixerFragment.this.smcCommunication;
                SmcMixerFragment.this.smcGlobeAlert.smcCommunication = SmcMixerFragment.this.smcCommunication;
            }
            AlertDialogUtil.getInstance(SmcMixerFragment.this.activity).closeDialog();
            return false;
        }
    });

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        this.activity = (ControlAndPdfActivity) getActivity();
        this.m_mainView = layoutInflater.inflate(R.layout.fragment_smc_mixer, viewGroup, false);
        initView();
        return this.m_mainView;
    }

    private void initView() {
        int i;
        int color = Color.parseColor("#D7D7D7");
        int color2 = Color.parseColor("#D7D7D7");
        int color3 = Color.parseColor("#D7D7D7");
        int color4 = Color.parseColor("#D7D7D7");
        this.m_knobAlert = new SmcKnobAlert(getActivity(), "Knob");
        this.smcSliderAlert = new SmcSliderAlert(getActivity(), "Slider");
        this.smcTrackAlert = new SmcTrackAlert(getActivity(), "Track Button");
        this.smcGlobeAlert = new SmcTrackAlert(getActivity(), "Globe Button");
        ((LinearLayout) this.m_mainView.findViewById(R.id.smc_header)).addView(new MidiComBox(getContext(), "Preset", Arrays.asList(getContext().getResources().getStringArray(R.array.SmcPreset)), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.SmcMixerFragment$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i2) {
                this.f$0.m231lambda$initView$0$comcubeSuitefragmentSmcMixerFragment(i2);
            }
        }), 0);
        this.m_mainView.findViewById(R.id.smc_save).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcMixerFragment.this.saveConfig();
            }
        });
        this.m_mainView.findViewById(R.id.smc_reset).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment.3
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                SmcMixerFragment.this.resetConfig();
            }
        });
        LinearLayout linearLayout = (LinearLayout) this.m_mainView.findViewById(R.id.knobLayout);
        final int i2 = 0;
        while (true) {
            if (i2 >= 8) {
                break;
            }
            this.m_knobButtonArr[i2] = new Button(getContext());
            int i3 = i2 + 1;
            this.m_knobButtonArr[i2].setText(String.valueOf(i3));
            this.m_knobButtonArr[i2].setTextColor(-1);
            this.m_knobButtonArr[i2].setTextSize(18.0f);
            this.m_knobButtonArr[i2].setPadding(15, 15, 15, 15);
            this.m_knobButtonArr[i2].setTypeface(Typeface.DEFAULT_BOLD);
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.setShape(0);
            gradientDrawable.setCornerRadius(100.0f);
            gradientDrawable.setColor(color);
            gradientDrawable.setStroke(4, -1);
            StateListDrawable stateListDrawable = new StateListDrawable();
            stateListDrawable.addState(new int[0], gradientDrawable);
            this.m_knobButtonArr[i2].setBackground(stateListDrawable);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(160, 160);
            layoutParams.gravity = 17;
            layoutParams.setMargins(20, 20, 20, 20);
            linearLayout.addView(this.m_knobButtonArr[i2], layoutParams);
            this.m_knobButtonArr[i2].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment$$ExternalSyntheticLambda1
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m232lambda$initView$1$comcubeSuitefragmentSmcMixerFragment(i2, view);
                }
            });
            i2 = i3;
        }
        LinearLayout linearLayout2 = (LinearLayout) this.m_mainView.findViewById(R.id.sliderLayout);
        final int i4 = 0;
        for (i = 8; i4 < i; i = 8) {
            this.m_sliderButtonArr[i4] = new Button(getContext());
            int i5 = i4 + 1;
            this.m_sliderButtonArr[i4].setText(String.valueOf(i5));
            this.m_sliderButtonArr[i4].setTextColor(-1);
            this.m_sliderButtonArr[i4].setTextSize(18.0f);
            this.m_sliderButtonArr[i4].setPadding(15, 15, 15, 15);
            this.m_sliderButtonArr[i4].setTypeface(Typeface.DEFAULT_BOLD);
            GradientDrawable gradientDrawable2 = new GradientDrawable();
            gradientDrawable2.setShape(0);
            gradientDrawable2.setCornerRadius(5.0f);
            gradientDrawable2.setColor(color2);
            gradientDrawable2.setStroke(4, -1);
            StateListDrawable stateListDrawable2 = new StateListDrawable();
            stateListDrawable2.addState(new int[0], gradientDrawable2);
            this.m_sliderButtonArr[i4].setBackground(stateListDrawable2);
            LinearLayout.LayoutParams layoutParams2 = new LinearLayout.LayoutParams(160, 320);
            layoutParams2.gravity = 17;
            layoutParams2.setMargins(20, 20, 20, 20);
            linearLayout2.addView(this.m_sliderButtonArr[i4], layoutParams2);
            this.m_sliderButtonArr[i4].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment$$ExternalSyntheticLambda2
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m233lambda$initView$2$comcubeSuitefragmentSmcMixerFragment(i4, view);
                }
            });
            i4 = i5;
        }
        GridLayout gridLayout = (GridLayout) this.m_mainView.findViewById(R.id.trackButtonLayout);
        for (final int i6 = 0; i6 < 8; i6++) {
            for (final int i7 = 0; i7 < 4; i7++) {
                this.m_trackButtonArr[i6][i7] = new ImageButton(getContext());
                if (i7 == 0) {
                    this.m_trackButtonArr[i6][i7].setImageResource(R.drawable.smc_mix_track_1);
                } else if (i7 == 1) {
                    this.m_trackButtonArr[i6][i7].setImageResource(R.drawable.smc_mix_track_2);
                } else if (i7 == 2) {
                    this.m_trackButtonArr[i6][i7].setImageResource(R.drawable.smc_mix_track_3);
                } else if (i7 == 3) {
                    this.m_trackButtonArr[i6][i7].setImageResource(R.drawable.smc_mix_track_4);
                }
                this.m_trackButtonArr[i6][i7].setPadding(15, 15, 15, 15);
                this.m_trackButtonArr[i6][i7].setScaleType(ImageView.ScaleType.CENTER_INSIDE);
                GradientDrawable gradientDrawable3 = new GradientDrawable();
                gradientDrawable3.setShape(0);
                gradientDrawable3.setCornerRadius(5.0f);
                gradientDrawable3.setColor(color3);
                gradientDrawable3.setStroke(4, -1);
                StateListDrawable stateListDrawable3 = new StateListDrawable();
                stateListDrawable3.addState(new int[0], gradientDrawable3);
                this.m_trackButtonArr[i6][i7].setBackground(stateListDrawable3);
                LinearLayout.LayoutParams layoutParams3 = new LinearLayout.LayoutParams(160, 160);
                layoutParams3.gravity = 17;
                layoutParams3.setMargins(20, 20, 20, 20);
                gridLayout.addView(this.m_trackButtonArr[i6][i7], layoutParams3);
                this.m_trackButtonArr[i6][i7].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment$$ExternalSyntheticLambda3
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m234lambda$initView$3$comcubeSuitefragmentSmcMixerFragment(i6, i7, view);
                    }
                });
            }
        }
        LinearLayout linearLayout3 = (LinearLayout) this.m_mainView.findViewById(R.id.globButtonLayout);
        for (final int i8 = 0; i8 < 11; i8++) {
            int identifier = getResources().getIdentifier("smc_mix_but_" + i8, "drawable", getContext().getPackageName());
            this.m_globButtonArr[i8] = new ImageButton(getContext());
            this.m_globButtonArr[i8].setScaleType(ImageView.ScaleType.CENTER_INSIDE);
            this.m_globButtonArr[i8].setImageResource(identifier);
            this.m_globButtonArr[i8].setPadding(15, 15, 15, 15);
            GradientDrawable gradientDrawable4 = new GradientDrawable();
            gradientDrawable4.setShape(0);
            gradientDrawable4.setCornerRadius(5.0f);
            gradientDrawable4.setColor(color4);
            gradientDrawable4.setStroke(4, -1);
            StateListDrawable stateListDrawable4 = new StateListDrawable();
            stateListDrawable4.addState(new int[0], gradientDrawable4);
            this.m_globButtonArr[i8].setBackground(stateListDrawable4);
            LinearLayout.LayoutParams layoutParams4 = new LinearLayout.LayoutParams(160, 106);
            layoutParams4.gravity = 17;
            layoutParams4.setMargins(20, 20, 20, 20);
            linearLayout3.addView(this.m_globButtonArr[i8], layoutParams4);
            this.m_globButtonArr[i8].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SmcMixerFragment$$ExternalSyntheticLambda4
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m235lambda$initView$4$comcubeSuitefragmentSmcMixerFragment(i8, view);
                }
            });
        }
    }

    /* JADX INFO: renamed from: lambda$initView$0$com-cubeSuite-fragment-SmcMixerFragment, reason: not valid java name */
    /* synthetic */ void m231lambda$initView$0$comcubeSuitefragmentSmcMixerFragment(int i) {
        this.currentSelectedPresetIndex = i;
        this.smcCommunication.sendDataToDevice((byte) 4, 0L, new byte[]{(byte) i});
    }

    /* JADX INFO: renamed from: lambda$initView$1$com-cubeSuite-fragment-SmcMixerFragment, reason: not valid java name */
    /* synthetic */ void m232lambda$initView$1$comcubeSuitefragmentSmcMixerFragment(int i, View view) {
        this.m_knobButtonArr[this.currentSelectedKnobIndex].setSelected(false);
        this.currentSelectedKnobIndex = i;
        view.setSelected(true);
        this.m_knobAlert.updateData(this.entry.usr[this.currentSelectedPresetIndex].knob[i], "Knob[" + (i + 1) + "]", this.currentSelectedPresetIndex == 1);
        this.m_knobAlert.show();
    }

    /* JADX INFO: renamed from: lambda$initView$2$com-cubeSuite-fragment-SmcMixerFragment, reason: not valid java name */
    /* synthetic */ void m233lambda$initView$2$comcubeSuitefragmentSmcMixerFragment(int i, View view) {
        this.m_sliderButtonArr[this.currentSelectedSliderIndex].setSelected(false);
        view.setSelected(true);
        this.currentSelectedSliderIndex = i;
        this.smcSliderAlert.updateData(this.entry.usr[this.currentSelectedPresetIndex].slider[i], "Slider[" + (i + 1) + "]", this.currentSelectedPresetIndex == 1);
        this.smcSliderAlert.show();
    }

    /* JADX INFO: renamed from: lambda$initView$3$com-cubeSuite-fragment-SmcMixerFragment, reason: not valid java name */
    /* synthetic */ void m234lambda$initView$3$comcubeSuitefragmentSmcMixerFragment(int i, int i2, View view) {
        this.m_trackButtonArr[this.currentSelectedTrackIndexI][this.currentSelectedTrackIndexJ].setSelected(false);
        view.setSelected(true);
        this.currentSelectedTrackIndexI = i;
        this.currentSelectedTrackIndexJ = i2;
        this.smcTrackAlert.updateData(this.entry.usr[this.currentSelectedPresetIndex].track[(i * 4) + i2], "Track[" + (i + 1) + "][" + (i2 + 1) + "]", this.currentSelectedPresetIndex == 1);
        this.smcTrackAlert.show();
    }

    /* JADX INFO: renamed from: lambda$initView$4$com-cubeSuite-fragment-SmcMixerFragment, reason: not valid java name */
    /* synthetic */ void m235lambda$initView$4$comcubeSuitefragmentSmcMixerFragment(int i, View view) {
        this.m_globButtonArr[this.currentSelectedGlobeIndex].setSelected(false);
        this.currentSelectedGlobeIndex = i;
        view.setSelected(true);
        this.smcGlobeAlert.updateData(this.entry.usr[this.currentSelectedPresetIndex].globe[i], "Globe[" + (i + 1) + "]", this.currentSelectedPresetIndex == 1);
        this.smcGlobeAlert.show();
    }

    public void saveConfig() {
        this.smcCommunication.saveConfig(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.SmcMixerFragment.4
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    AlertDialogUtil.getInstance(SmcMixerFragment.this.getActivity()).showDialog().setTitle(R.string.success).setContent(R.string.saveConfigSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                } else {
                    AlertDialogUtil.getInstance(SmcMixerFragment.this.getActivity()).showDialog().setTitle(R.string.failure).setContent(R.string.saveConfigFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
                }
            }
        });
    }

    public void resetConfig() {
        AlertDialogUtil.getInstance(getActivity()).showDialog().setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.SmcMixerFragment.5
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                AlertDialogUtil.getInstance(SmcMixerFragment.this.getActivity()).setTitle(R.string.hint).setContent(R.string.recoveringData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE);
                final byte[] file = FileUtils.readFile(SmcMixerFragment.this.getResources().openRawResource(R.raw.smkb));
                SmcMixerFragment.this.smcCommunication.resetFactory(file, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.SmcMixerFragment.5.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            SmcMixerFragment.this.entry.setUsrData(file);
                            SmcMixerFragment.this.handler.sendEmptyMessageDelayed(0, 2000L);
                        } else {
                            AlertDialogUtil.getInstance(SmcMixerFragment.this.getActivity()).showDialog().setTitle(R.string.error).setContent(R.string.resetFailure).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.SmcMixerFragment.5.1.1
                            }).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM);
                        }
                    }
                });
            }
        }, false).setTitle(R.string.warning).setContent(R.string.hintResetFactory).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE);
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        this.entry = new SmcMixerEntry();
        CommunicationUtil communicationUtil = communicationUtil;
        Objects.requireNonNull(this.entry);
        communicationUtil.splitReadData((byte) 5, 0L, 6282, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.SmcMixerFragment.6
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    SmcMixerFragment.this.entry.setUsrData(bArr);
                    SmcMixerFragment.this.handler.sendEmptyMessageDelayed(0, 1000L);
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }
}
