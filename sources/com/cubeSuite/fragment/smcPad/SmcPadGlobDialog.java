package com.cubeSuite.fragment.smcPad;

import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.cubeSuite.R;
import com.cubeSuite.communication.SmcPadCommunication;
import com.cubeSuite.customControl.MidiComBox;
import com.cubeSuite.entity.smcPad.SmcPadEntry;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadGlobDialog extends Dialog {
    private final int LINER_PADDING;
    private SmcPadEntry.SmcPadGlob glob;
    private TextView m_title;
    private MidiComBox octive;
    private MidiComBox padAfter;
    private MidiComBox padCurve;
    private MidiComBox preset;
    public SmcPadCommunication smcPadCommunication;
    SmcPadGlobDialogCallback smcPadGlobDialogCallback;
    private MidiComBox trans;

    public SmcPadGlobDialog(Context context, String str) {
        super(context);
        this.LINER_PADDING = dpToPx(16);
        init(context, Color.parseColor("#273c75"), str);
    }

    private void init(Context context, int i, String str) {
        requestWindowFeature(1);
        FrameLayout frameLayout = new FrameLayout(context);
        frameLayout.setLayoutParams(new ViewGroup.LayoutParams(-1, -2));
        LinearLayout linearLayout = new LinearLayout(context);
        linearLayout.setLayoutParams(new FrameLayout.LayoutParams(-1, -2));
        linearLayout.setOrientation(1);
        int i2 = this.LINER_PADDING;
        linearLayout.setPadding(i2, i2, i2, i2);
        TextView textView = new TextView(context);
        this.m_title = textView;
        textView.setText(str);
        this.m_title.setTextSize(24.0f);
        this.m_title.setTextColor(R.color.accent);
        this.m_title.setGravity(17);
        linearLayout.addView(this.m_title);
        MidiComBox midiComBox = new MidiComBox(context, "Preset", Arrays.asList(context.getResources().getStringArray(R.array.SmcPadPreset)), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadGlobDialog$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m275lambda$init$0$comcubeSuitefragmentsmcPadSmcPadGlobDialog(i3);
            }
        });
        this.preset = midiComBox;
        linearLayout.addView(midiComBox);
        MidiComBox midiComBox2 = new MidiComBox(context, "PadCurve", Arrays.asList(" 1 ", " 2 ", " 3 ", " 4 "), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadGlobDialog$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m276lambda$init$1$comcubeSuitefragmentsmcPadSmcPadGlobDialog(i3);
            }
        });
        this.padCurve = midiComBox2;
        linearLayout.addView(midiComBox2);
        MidiComBox midiComBox3 = new MidiComBox(context, "PadAfterTouch", Arrays.asList(" 0 ", " 1 "), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadGlobDialog$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m277lambda$init$2$comcubeSuitefragmentsmcPadSmcPadGlobDialog(i3);
            }
        });
        this.padAfter = midiComBox3;
        linearLayout.addView(midiComBox3);
        MidiComBox midiComBox4 = new MidiComBox(context, "Octive", Arrays.asList(" 1 ", " 2 ", " 3 ", " 4 ", " 5 ", " 6 ", " 7 "), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadGlobDialog$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m278lambda$init$3$comcubeSuitefragmentsmcPadSmcPadGlobDialog(i3);
            }
        });
        this.octive = midiComBox4;
        linearLayout.addView(midiComBox4);
        ArrayList arrayList = new ArrayList();
        for (int i3 = -16; i3 < 17; i3++) {
            arrayList.add("   " + i3 + "   ");
        }
        MidiComBox midiComBox5 = new MidiComBox(context, "Transpose", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadGlobDialog$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m279lambda$init$4$comcubeSuitefragmentsmcPadSmcPadGlobDialog(i4);
            }
        });
        this.trans = midiComBox5;
        linearLayout.addView(midiComBox5);
        frameLayout.addView(linearLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-fragment-smcPad-SmcPadGlobDialog, reason: not valid java name */
    /* synthetic */ void m275lambda$init$0$comcubeSuitefragmentsmcPadSmcPadGlobDialog(int i) {
        this.glob.preset.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.preset);
        this.smcPadGlobDialogCallback.onSelectPreset(i);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-fragment-smcPad-SmcPadGlobDialog, reason: not valid java name */
    /* synthetic */ void m276lambda$init$1$comcubeSuitefragmentsmcPadSmcPadGlobDialog(int i) {
        this.glob.padcurve.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.padcurve);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-fragment-smcPad-SmcPadGlobDialog, reason: not valid java name */
    /* synthetic */ void m277lambda$init$2$comcubeSuitefragmentsmcPadSmcPadGlobDialog(int i) {
        this.glob.padaft.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.padaft);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-fragment-smcPad-SmcPadGlobDialog, reason: not valid java name */
    /* synthetic */ void m278lambda$init$3$comcubeSuitefragmentsmcPadSmcPadGlobDialog(int i) {
        this.glob.octave.setData(i);
        this.smcPadGlobDialogCallback.onSelectOctive(i);
        this.smcPadCommunication.sendGlobU8(this.glob.octave);
        if (i == 0 || i == 6) {
            this.glob.transpose.setData((byte) 0);
            this.trans.spinner.setSelection(16);
            this.smcPadCommunication.sendGlobI8(this.glob.transpose);
            this.trans.setVisibility(8);
            this.smcPadGlobDialogCallback.onSelectTranspose(0);
            return;
        }
        this.trans.setVisibility(0);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-fragment-smcPad-SmcPadGlobDialog, reason: not valid java name */
    /* synthetic */ void m279lambda$init$4$comcubeSuitefragmentsmcPadSmcPadGlobDialog(int i) {
        int i2 = i - 16;
        this.glob.transpose.setData((byte) i2);
        this.smcPadCommunication.sendGlobI8(this.glob.transpose);
        this.smcPadGlobDialogCallback.onSelectTranspose(i2);
    }

    public void updateData(SmcPadEntry.SmcPadGlob smcPadGlob) {
        this.glob = smcPadGlob;
        this.preset.spinner.setSelection(smcPadGlob.preset.getData());
        this.padCurve.spinner.setSelection(smcPadGlob.padcurve.getData());
        this.padAfter.spinner.setSelection(smcPadGlob.padaft.getData());
        this.octive.spinner.setSelection(smcPadGlob.octave.getData());
        this.trans.spinner.setSelection(smcPadGlob.transpose.getData() + 16);
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
