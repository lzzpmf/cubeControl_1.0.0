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
public class SmcPadNoteDialog extends Dialog {
    private final int LINER_PADDING;
    private SmcPadEntry.SmcPadGlob glob;
    private MidiComBox latch;
    private TextView m_title;
    public SmcPadCommunication smcPadCommunication;
    private MidiComBox swing;
    private MidiComBox sync;
    private MidiComBox tempo;
    private MidiComBox time;

    public SmcPadNoteDialog(Context context, String str) {
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
        MidiComBox midiComBox = new MidiComBox(context, "Time", Arrays.asList(context.getResources().getStringArray(R.array.SmcPadTime)), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadNoteDialog$$ExternalSyntheticLambda0
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i3) {
                this.f$0.m286lambda$init$0$comcubeSuitefragmentsmcPadSmcPadNoteDialog(i3);
            }
        });
        this.time = midiComBox;
        linearLayout.addView(midiComBox);
        ArrayList arrayList = new ArrayList();
        for (int i3 = 30; i3 < 301; i3++) {
            arrayList.add("   " + i3 + "   ");
        }
        MidiComBox midiComBox2 = new MidiComBox(context, "Tempo", arrayList, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadNoteDialog$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m287lambda$init$1$comcubeSuitefragmentsmcPadSmcPadNoteDialog(i4);
            }
        });
        this.tempo = midiComBox2;
        linearLayout.addView(midiComBox2);
        MidiComBox midiComBox3 = new MidiComBox(context, "Sync", Arrays.asList(" 0 ", " 1 "), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadNoteDialog$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i4) {
                this.f$0.m288lambda$init$2$comcubeSuitefragmentsmcPadSmcPadNoteDialog(i4);
            }
        });
        this.sync = midiComBox3;
        linearLayout.addView(midiComBox3);
        ArrayList arrayList2 = new ArrayList();
        for (int i4 = 50; i4 < 76; i4++) {
            arrayList2.add("   " + i4 + "   ");
        }
        MidiComBox midiComBox4 = new MidiComBox(context, "Swing", arrayList2, new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadNoteDialog$$ExternalSyntheticLambda3
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m289lambda$init$3$comcubeSuitefragmentsmcPadSmcPadNoteDialog(i5);
            }
        });
        this.swing = midiComBox4;
        linearLayout.addView(midiComBox4);
        MidiComBox midiComBox5 = new MidiComBox(context, "Latch", Arrays.asList(" 0 ", " 1 "), new MidiComBox.DataChange() { // from class: com.cubeSuite.fragment.smcPad.SmcPadNoteDialog$$ExternalSyntheticLambda4
            @Override // com.cubeSuite.customControl.MidiComBox.DataChange
            public final void dataChange(int i5) {
                this.f$0.m290lambda$init$4$comcubeSuitefragmentsmcPadSmcPadNoteDialog(i5);
            }
        });
        this.latch = midiComBox5;
        linearLayout.addView(midiComBox5);
        frameLayout.addView(linearLayout);
        setContentView(frameLayout);
        WindowManager.LayoutParams attributes = getWindow().getAttributes();
        attributes.width = -1;
        attributes.height = -2;
        getWindow().setAttributes(attributes);
        frameLayout.setBackgroundColor(i);
    }

    /* JADX INFO: renamed from: lambda$init$0$com-cubeSuite-fragment-smcPad-SmcPadNoteDialog, reason: not valid java name */
    /* synthetic */ void m286lambda$init$0$comcubeSuitefragmentsmcPadSmcPadNoteDialog(int i) {
        this.glob.time.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.time);
    }

    /* JADX INFO: renamed from: lambda$init$1$com-cubeSuite-fragment-smcPad-SmcPadNoteDialog, reason: not valid java name */
    /* synthetic */ void m287lambda$init$1$comcubeSuitefragmentsmcPadSmcPadNoteDialog(int i) {
        this.glob.tempo.setData(i + 30);
        this.smcPadCommunication.sendGlobU16(this.glob.tempo);
    }

    /* JADX INFO: renamed from: lambda$init$2$com-cubeSuite-fragment-smcPad-SmcPadNoteDialog, reason: not valid java name */
    /* synthetic */ void m288lambda$init$2$comcubeSuitefragmentsmcPadSmcPadNoteDialog(int i) {
        this.glob.sync.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.sync);
    }

    /* JADX INFO: renamed from: lambda$init$3$com-cubeSuite-fragment-smcPad-SmcPadNoteDialog, reason: not valid java name */
    /* synthetic */ void m289lambda$init$3$comcubeSuitefragmentsmcPadSmcPadNoteDialog(int i) {
        this.glob.swing.setData(i + 50);
        this.smcPadCommunication.sendGlobU8(this.glob.swing);
    }

    /* JADX INFO: renamed from: lambda$init$4$com-cubeSuite-fragment-smcPad-SmcPadNoteDialog, reason: not valid java name */
    /* synthetic */ void m290lambda$init$4$comcubeSuitefragmentsmcPadSmcPadNoteDialog(int i) {
        this.glob.latch.setData(i);
        this.smcPadCommunication.sendGlobU8(this.glob.latch);
    }

    public void updateData(SmcPadEntry.SmcPadGlob smcPadGlob) {
        this.glob = smcPadGlob;
        this.time.spinner.setSelection(smcPadGlob.time.getData());
        this.tempo.spinner.setSelection(smcPadGlob.tempo.getData());
        this.sync.spinner.setSelection(smcPadGlob.sync.getData());
        this.swing.spinner.setSelection(smcPadGlob.swing.getData() - 50);
        this.latch.spinner.setSelection(smcPadGlob.latch.getData());
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
