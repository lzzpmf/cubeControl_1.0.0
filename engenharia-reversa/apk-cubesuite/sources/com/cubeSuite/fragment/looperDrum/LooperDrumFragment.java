package com.cubeSuite.fragment.looperDrum;

import android.app.AlertDialog;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.res.Resources;
import android.graphics.drawable.StateListDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.os.StrictMode;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.core.view.InputDeviceCompat;
import com.cubeSuite.MainActivity;
import com.cubeSuite.R;
import com.cubeSuite.activitys.ActivityStackUtil;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.LooperCommunication;
import com.cubeSuite.communication.LooperDrumCommunication;
import com.cubeSuite.config.OtherEnum;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.BackHeadView;
import com.cubeSuite.customControl.BlockProgressBar;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.HistogramView;
import com.cubeSuite.customControl.ScrollViewPlus;
import com.cubeSuite.enumeration.DrumType;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.utils.FileUtils;
import com.cubeSuite.utils.StringUtils;
import com.google.android.material.textfield.TextInputEditText;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;

/* JADX INFO: loaded from: classes.dex */
public class LooperDrumFragment extends ControlBaseFragment {
    private static final int CANCEL_VEL = 10003;
    private static final int MSG_INIT_DATA = 10002;
    private static final int MSG_PLAY_MIDI = 10001;
    private final int MAX_BLOCK;
    private final String[] TONE_TYPE;
    private Button addBeat;
    private Button back;
    private int beatNum;
    private TextView beatText;
    LinearLayout blockList;
    private ScrollViewPlus blockSV;
    private boolean blockState;
    private Button btnLoadDrumList;
    private RadioGroup btnSelectDrum;
    private Button btnVelocity;
    AlertDialog.Builder builder;
    private Button copyAllDrum;
    private int currentPage;
    private int currentPlayColumn;
    AlertDialog dialog;
    private int drumIndex;
    private final DrumType drumType;
    private SeekBar gate;
    private TextView gateValue;
    private final Handler handler;
    EditText inputServer;
    private boolean isAdjustable;
    private boolean isPlay;
    private boolean isSetStartTime;
    private LooperDrumCommunication ldc;
    private final List<LinearLayout> linearPoints;
    private HistogramView linkageHistogram;
    private PopupWindow makeDrumWindow;
    private final int noteNum;
    private RadioGroup pageSelect;
    private Button pasteAllDrum;
    private ButtonIconView playBeat;
    private RadioButton power;
    private RadioGroup powerStep;
    private Button resetFactory;
    private PopupWindow selectDrumWindow;
    private Button showMakeDrum;
    private int showVelocity;
    private long startTime;
    private RadioButton step;
    private Button subBeat;
    private View thisView;
    private Timer timer;
    private ScrollViewPlus toneSV;
    LinearLayout toneStrList;
    private TimerTask updateTask;
    private Button velocityText;
    private SeekBar volume;
    private TextView volumeValue;

    public LooperDrumFragment() {
        this.isAdjustable = false;
        this.beatNum = 4;
        this.noteNum = 4;
        this.MAX_BLOCK = 16;
        String[] strArr = {"Crash 01", "Crash 02", "Ride 01", "Ride 02", "Hihat open", "Hihat 01", "Hihat 02", "Snare 01", "Snare 02", "Snare 03", "Snare Side", "Tom 01", "Tom 02", "Tom 03", "Kick 01", "Kick 02"};
        this.TONE_TYPE = strArr;
        this.linearPoints = new ArrayList(strArr.length);
        this.currentPage = 1;
        this.blockState = false;
        this.showVelocity = 150;
        this.isPlay = false;
        this.currentPlayColumn = 0;
        this.drumIndex = 0;
        this.handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.3
            @Override // android.os.Handler.Callback
            public boolean handleMessage(Message message) {
                int i = message.what;
                if (i == LooperDrumFragment.MSG_INIT_DATA) {
                    AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).closeDialog();
                } else if (i == LooperDrumFragment.CANCEL_VEL) {
                    LooperDrumFragment.this.isSetStartTime = false;
                }
                return false;
            }
        });
        this.startTime = 0L;
        this.isSetStartTime = false;
        this.drumType = DrumType.sinco;
    }

    public LooperDrumFragment(DrumType drumType) {
        this.isAdjustable = false;
        this.beatNum = 4;
        this.noteNum = 4;
        this.MAX_BLOCK = 16;
        String[] strArr = {"Crash 01", "Crash 02", "Ride 01", "Ride 02", "Hihat open", "Hihat 01", "Hihat 02", "Snare 01", "Snare 02", "Snare 03", "Snare Side", "Tom 01", "Tom 02", "Tom 03", "Kick 01", "Kick 02"};
        this.TONE_TYPE = strArr;
        this.linearPoints = new ArrayList(strArr.length);
        this.currentPage = 1;
        this.blockState = false;
        this.showVelocity = 150;
        this.isPlay = false;
        this.currentPlayColumn = 0;
        this.drumIndex = 0;
        this.handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.3
            @Override // android.os.Handler.Callback
            public boolean handleMessage(Message message) {
                int i = message.what;
                if (i == LooperDrumFragment.MSG_INIT_DATA) {
                    AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).closeDialog();
                } else if (i == LooperDrumFragment.CANCEL_VEL) {
                    LooperDrumFragment.this.isSetStartTime = false;
                }
                return false;
            }
        });
        this.startTime = 0L;
        this.isSetStartTime = false;
        this.drumType = drumType;
    }

    private void initMakeDrumWindow() {
        View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.sequencer_layout, (ViewGroup) null);
        this.makeDrumWindow = new PopupWindow(viewInflate, -1, -1, true);
        this.playBeat = (ButtonIconView) viewInflate.findViewById(R.id.playBeat);
        this.powerStep = (RadioGroup) viewInflate.findViewById(R.id.powerStep);
        this.blockSV = (ScrollViewPlus) viewInflate.findViewById(R.id.blockSV);
        this.toneSV = (ScrollViewPlus) viewInflate.findViewById(R.id.toneSV);
        this.power = (RadioButton) viewInflate.findViewById(R.id.power);
        this.step = (RadioButton) viewInflate.findViewById(R.id.step);
        this.addBeat = (Button) viewInflate.findViewById(R.id.addBeat);
        this.subBeat = (Button) viewInflate.findViewById(R.id.subBeat);
        this.beatText = (TextView) viewInflate.findViewById(R.id.beatText);
        this.blockList = (LinearLayout) viewInflate.findViewById(R.id.blockList);
        this.toneStrList = (LinearLayout) viewInflate.findViewById(R.id.toneStrList);
        this.pageSelect = (RadioGroup) viewInflate.findViewById(R.id.pageSelect);
        this.back = (Button) viewInflate.findViewById(R.id.back);
        this.btnVelocity = (Button) viewInflate.findViewById(R.id.velocity);
        this.btnLoadDrumList = (Button) viewInflate.findViewById(R.id.btnLoadDrumList);
        this.linkageHistogram = (HistogramView) viewInflate.findViewById(R.id.linkageHistogram);
        this.velocityText = (Button) viewInflate.findViewById(R.id.velocityText);
        Button button = (Button) viewInflate.findViewById(R.id.addVelocity);
        Button button2 = (Button) viewInflate.findViewById(R.id.subVelocity);
        Button button3 = (Button) viewInflate.findViewById(R.id.saveDrumData);
        Button button4 = (Button) viewInflate.findViewById(R.id.share);
        Button button5 = (Button) viewInflate.findViewById(R.id.loadDrum);
        this.beatText.setText(String.valueOf(this.beatNum));
        this.velocityText.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda7
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m238xcef977d(view);
            }
        });
        button5.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda8
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m239x270b161c(view);
            }
        });
        button4.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda9
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m240x412694bb(view);
            }
        });
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda10
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m241x5b42135a(view);
            }
        });
        button2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda12
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m242x755d91f9(view);
            }
        });
        button3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda13
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m243x8f791098(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$0$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m238xcef977d(View view) {
        this.dialog.show();
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$1$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m239x270b161c(View view) {
        final String clipboardContent = getClipboardContent(MainActivity.context);
        if (clipboardContent == null || clipboardContent.length() < 5) {
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.correctCopy, 1).show();
        } else {
            AlertDialogUtil.getInstance(this.activity).setTitle(R.string.hint).setContent(R.string.importDrumTips).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.1
                @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
                public void confirmBtnClick() {
                    byte[] drumData = LooperDrumFragment.getDrumData(clipboardContent);
                    if (drumData.length == 0) {
                        Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.incorrectFormat, 1).show();
                        return;
                    }
                    final byte[] bArr = new byte[512];
                    System.arraycopy(drumData, 1, bArr, 0, 512);
                    final byte b = drumData[0];
                    LooperDrumFragment.this.ldc.sendAllDrumData(bArr, b, LooperDrumFragment.this.drumIndex, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.1.1
                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                        public void writeDataCallback(boolean z) {
                            if (z) {
                                LooperDrumFragment.this.dataToMode(b, bArr);
                            }
                        }
                    });
                }
            }).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE).showDialog();
        }
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$2$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m240x412694bb(View view) {
        byte[] bArr = new byte[InputDeviceCompat.SOURCE_DPAD];
        bArr[0] = (byte) this.beatNum;
        StrictMode.setVmPolicy(new StrictMode.VmPolicy.Builder().build());
        for (int i = 0; i < this.linearPoints.size(); i++) {
            for (int i2 = 0; i2 < this.linearPoints.get(i).getChildCount(); i2++) {
                BlockProgressBar blockProgressBar = (BlockProgressBar) this.linearPoints.get(i).getChildAt(i2).findViewById(R.id.blockItem);
                if (blockProgressBar.isEnable()) {
                    bArr[(i2 * 16) + i + 1] = (byte) blockProgressBar.getProgress();
                }
            }
        }
        ((ClipboardManager) MainActivity.context.getSystemService("clipboard")).setPrimaryClip(ClipData.newPlainText("Label", getDrumCode(bArr)));
        Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.copyDrumCodeComplete, 1).show();
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$3$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m241x5b42135a(View view) {
        this.showVelocity++;
        setVelocity();
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$4$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m242x755d91f9(View view) {
        this.showVelocity--;
        setVelocity();
    }

    /* JADX INFO: renamed from: lambda$initMakeDrumWindow$5$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m243x8f791098(View view) {
        saveAllDrum();
    }

    public void saveAllDrum() {
        this.ldc.saveDrumData(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).closeDialog();
                if (z) {
                    Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.saveSuccess, 1).show();
                } else {
                    Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.saveFail, 1).show();
                }
            }
        });
    }

    public static String getClipboardContent(Context context) {
        ClipData primaryClip;
        ClipData.Item itemAt;
        CharSequence charSequenceCoerceToText;
        ClipboardManager clipboardManager = (ClipboardManager) context.getSystemService("clipboard");
        if (clipboardManager == null || (primaryClip = clipboardManager.getPrimaryClip()) == null || primaryClip.getItemCount() <= 0 || (itemAt = primaryClip.getItemAt(0)) == null || (charSequenceCoerceToText = itemAt.coerceToText(context)) == null) {
            return null;
        }
        return charSequenceCoerceToText.toString();
    }

    public static byte[] codeToData(String str) {
        int i;
        byte[] bArr = new byte[15872];
        int i2 = 0;
        int i3 = 0;
        byte bCharAt = 0;
        for (int i4 = 0; i4 < str.length(); i4++) {
            if (i4 % 2 == 0) {
                bCharAt = (byte) (str.charAt(i4) - '0');
            } else {
                byte bCharAt2 = (byte) (str.charAt(i4) - '0');
                int i5 = i3;
                while (true) {
                    i = i3 + bCharAt2;
                    if (i5 >= i) {
                        break;
                    }
                    bArr[i5] = bCharAt;
                    i2++;
                    i5++;
                }
                i3 = i;
            }
        }
        if (i2 < 15360) {
            return new byte[0];
        }
        int i6 = 0;
        while (i6 < 30) {
            int i7 = i6 + 1;
            int i8 = 512 * i7;
            for (int i9 = (bArr[i6] * 64) + i8; i9 < i8 + 512; i9++) {
                bArr[i9] = 0;
            }
            i6 = i7;
        }
        return bArr;
    }

    public static byte[] getDrumData(String str) {
        int i;
        byte[] bArr = new byte[InputDeviceCompat.SOURCE_DPAD];
        bArr[0] = (byte) (str.charAt(1) - '0');
        int iCharAt = str.charAt(str.length() - 1) - '0';
        int i2 = 1;
        int i3 = 0;
        int i4 = 0;
        byte bCharAt = 0;
        for (int i5 = 2; i5 < str.length() - 1; i5++) {
            if (i5 % 2 == 0) {
                bCharAt = (byte) (str.charAt(i5) - '0');
            } else {
                byte bCharAt2 = (byte) (str.charAt(i5) - '0');
                i3 += bCharAt2;
                Log.e("count", ((int) bCharAt2) + "");
                int i6 = i2;
                while (true) {
                    i = i2 + bCharAt2;
                    if (i6 >= i) {
                        break;
                    }
                    bArr[i6] = bCharAt;
                    i4 += bCharAt;
                    i6++;
                }
                if (i5 == str.length() - 2) {
                    System.out.println();
                }
                i2 = i;
            }
        }
        System.out.println(i3);
        return iCharAt == i4 % 10 ? bArr : new byte[0];
    }

    public static String dataToCode(byte[] bArr) {
        StringBuilder sb = new StringBuilder("");
        int i = 0;
        byte b = 0;
        int i2 = 0;
        while (i < bArr.length) {
            if (i == 0) {
                b = bArr[i];
            }
            if (b != bArr[i] || i2 > 70) {
                sb.append((char) (b + 48));
                sb.append((char) (i2 + 48));
                b = bArr[i];
                i2 = 0;
            }
            int i3 = i2 + 1;
            if (i == bArr.length - 1) {
                sb.append((char) (b + 48));
                sb.append((char) (i2 + 49));
            }
            i++;
            i2 = i3;
        }
        return sb.toString();
    }

    public String getDrumCode(byte[] bArr) {
        StringBuilder sb = new StringBuilder("d");
        sb.append(String.valueOf((int) bArr[0]));
        int i = 0;
        byte b = 0;
        int i2 = 0;
        int i3 = 1;
        while (i3 < bArr.length) {
            byte b2 = bArr[i3];
            i += b2;
            if (i3 == 1) {
                b = b2;
            }
            if (b != b2 || i2 > 70) {
                sb.append((char) (b + 48));
                sb.append((char) (i2 + 48));
                b = bArr[i3];
                i2 = 0;
            }
            int i4 = i2 + 1;
            if (i3 == bArr.length - 1) {
                sb.append((char) (b + 48));
                sb.append((char) (i2 + 49));
            }
            i3++;
            i2 = i4;
        }
        sb.append((char) ((i % 10) + 48));
        return sb.toString();
    }

    private void initSelectDrumWindow() {
        View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.select_drum_fragment, (ViewGroup) null);
        this.selectDrumWindow = new PopupWindow(viewInflate, -1, -1, true);
        this.btnSelectDrum = (RadioGroup) viewInflate.findViewById(R.id.btnSelectDrum);
        BackHeadView backHeadView = (BackHeadView) viewInflate.findViewById(R.id.backHead);
        this.btnSelectDrum.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda14
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public final void onCheckedChanged(RadioGroup radioGroup, int i) {
                this.f$0.m245x7c40efc8(radioGroup, i);
            }
        });
        backHeadView.setStartClick(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda15
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m246x965c6e67(view);
            }
        });
        this.btnLoadDrumList.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda16
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m247xb077ed06(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$initSelectDrumWindow$7$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m245x7c40efc8(RadioGroup radioGroup, int i) {
        for (final int i2 = 0; i2 < this.btnSelectDrum.getChildCount(); i2++) {
            if (((RadioButton) this.btnSelectDrum.getChildAt(i2)).isChecked()) {
                AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setTitle(R.string.hint).setContent(R.string.loading).showDialog();
                readDrumDataToView(i2, new LooperDrumCommunication.DrumDataCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda6
                    @Override // com.cubeSuite.communication.LooperDrumCommunication.DrumDataCallback
                    public final void dataCallback(boolean z, Map map) {
                        this.f$0.m244x62257129(i2, z, map);
                    }
                });
                return;
            }
        }
    }

    /* JADX INFO: renamed from: lambda$initSelectDrumWindow$6$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m244x62257129(int i, boolean z, Map map) {
        if (!z) {
            AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setContent(R.string.loadDataFail).setTitle(R.string.failure).showDialog();
            return;
        }
        this.btnLoadDrumList.setText((i > 9 ? new StringBuilder("Drum-") : new StringBuilder("Drum-0")).append(i + 1).toString());
        this.drumIndex = i;
        AlertDialogUtil.getInstance(this.activity).closeDialog();
        this.selectDrumWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initSelectDrumWindow$8$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m246x965c6e67(View view) {
        this.selectDrumWindow.dismiss();
    }

    /* JADX INFO: renamed from: lambda$initSelectDrumWindow$9$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m247xb077ed06(View view) {
        this.selectDrumWindow.showAtLocation(this.thisView, 80, 0, 0);
    }

    private int getBlockNum() {
        return this.beatNum * 4;
    }

    public void sendDataAndUpdateView() {
        int blockNum = getBlockNum();
        if (this.currentPlayColumn >= blockNum) {
            this.currentPlayColumn = 0;
        }
        ArrayList arrayList = new ArrayList(this.TONE_TYPE.length);
        for (LinearLayout linearLayout : this.linearPoints) {
            int i = this.currentPlayColumn;
            if (i == 0) {
                ((BlockProgressBar) linearLayout.getChildAt(blockNum - 1).findViewById(R.id.blockItem)).setPlay(false);
            } else {
                ((BlockProgressBar) linearLayout.getChildAt(i - 1).findViewById(R.id.blockItem)).setPlay(false);
            }
            BlockProgressBar blockProgressBar = (BlockProgressBar) linearLayout.getChildAt(this.currentPlayColumn).findViewById(R.id.blockItem);
            blockProgressBar.setPlay(true);
            if (blockProgressBar.isEnable()) {
                arrayList.add(Byte.valueOf((byte) blockProgressBar.getTone()));
                arrayList.add(Byte.valueOf((byte) (((double) blockProgressBar.getProgress()) * 8.46d)));
            }
        }
        this.ldc.playVoice(arrayList);
        this.currentPlayColumn++;
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        Resources resources;
        int i;
        super.onCreate(bundle);
        this.activity = (ControlAndPdfActivity) getActivity();
        this.thisView = layoutInflater.inflate(R.layout.looper_drum_fragment, viewGroup, false);
        TextInputEditText textInputEditText = new TextInputEditText(this.activity);
        this.inputServer = textInputEditText;
        textInputEditText.setInputType(3);
        AlertDialog.Builder builder = new AlertDialog.Builder(this.activity);
        this.builder = builder;
        builder.setTitle("Velocity(40~240)").setView(this.inputServer).setNegativeButton(R.string.cancel, new DialogInterface.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.4
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i2) {
                dialogInterface.dismiss();
            }
        });
        this.builder.setPositiveButton(R.string.confirm, new DialogInterface.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.5
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i2) {
                LooperDrumFragment looperDrumFragment = LooperDrumFragment.this;
                looperDrumFragment.showVelocity = StringUtils.getNum(looperDrumFragment.inputServer.getText().toString());
                LooperDrumFragment.this.setVelocity();
                LooperDrumFragment.this.inputServer.setText(LooperDrumFragment.this.showVelocity + "");
                dialogInterface.dismiss();
            }
        });
        this.dialog = this.builder.create();
        initView();
        OtherEnum.saveDrumDataDir = FileUtils.splicingFilePath(this.activity.getApplicationContext(), OtherEnum.SINCO_ROOT_FOLDER, OtherEnum.SINCO_DRUM_DATA_DIR, null, null);
        for (int i2 = 0; i2 < this.TONE_TYPE.length; i2++) {
            View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.tone_str_item, (ViewGroup) this.toneStrList, false);
            if (i2 % 2 == 0) {
                resources = getResources();
                i = R.color.seq1;
            } else {
                resources = getResources();
                i = R.color.seq2;
            }
            int color = resources.getColor(i);
            TextView textView = (TextView) viewInflate.findViewById(R.id.toneText);
            textView.setText(this.TONE_TYPE[i2]);
            textView.setTextColor(color);
            this.toneStrList.addView(viewInflate);
            LinearLayout linearLayout = new LinearLayout(this.activity);
            new LinearLayout.LayoutParams(-2, -2);
            linearLayout.setOrientation(0);
            this.linearPoints.add(linearLayout);
            int i3 = 0;
            while (i3 < this.beatNum * 4) {
                View viewInflate2 = LayoutInflater.from(this.activity).inflate(R.layout.sequencer_item, (ViewGroup) this.blockList, false);
                BlockProgressBar blockProgressBar = (BlockProgressBar) viewInflate2.findViewById(R.id.blockItem);
                blockProgressBar.setBlockColor(color);
                blockProgressBar.setTone(i2);
                blockProgressBar.setCoords(i2, this.linearPoints.get(i2).getChildCount());
                blockProgressBar.setValueChange(new BlockProgressBar.BlockListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda4
                    @Override // com.cubeSuite.customControl.BlockProgressBar.BlockListener
                    public final void listenerValueChange(int i4, int i5, int i6) {
                        this.f$0.m248x9b616ef4(i4, i5, i6);
                    }
                });
                i3++;
                if (i3 % 4 == 0) {
                    ViewGroup.MarginLayoutParams marginLayoutParams = (ViewGroup.MarginLayoutParams) blockProgressBar.getLayoutParams();
                    marginLayoutParams.setMargins(dp2px(1.0f), dp2px(1.0f), dp2px(4.0f), dp2px(1.0f));
                    blockProgressBar.setLayoutParams(marginLayoutParams);
                }
                linearLayout.addView(viewInflate2);
            }
            this.blockList.addView(linearLayout);
        }
        updatePageItem();
        return this.thisView;
    }

    /* JADX INFO: renamed from: lambda$onCreateView$10$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m248x9b616ef4(int i, int i2, int i3) {
        if (i == 1) {
            this.linkageHistogram.setVisibility(4);
            this.ldc.sendDrumData(i2, i3, this.drumIndex);
        } else {
            this.linkageHistogram.setVisibility(0);
        }
        this.linkageHistogram.setProgress(i2);
    }

    private void initView() {
        initMakeDrumWindow();
        initSelectDrumWindow();
        this.showMakeDrum = (Button) this.thisView.findViewById(R.id.showMakeDrum);
        this.resetFactory = (Button) this.thisView.findViewById(R.id.resetFactory);
        this.copyAllDrum = (Button) this.thisView.findViewById(R.id.copyAllDrum);
        this.pasteAllDrum = (Button) this.thisView.findViewById(R.id.pasteAllDrum);
        this.gate = (SeekBar) this.thisView.findViewById(R.id.gate);
        this.volume = (SeekBar) this.thisView.findViewById(R.id.volume);
        this.gateValue = (TextView) this.thisView.findViewById(R.id.gateValue);
        this.volumeValue = (TextView) this.thisView.findViewById(R.id.volumeValue);
        setEventListener();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void deleteTimer() {
        this.isPlay = false;
        if (this.currentPlayColumn > 0) {
            Iterator<LinearLayout> it = this.linearPoints.iterator();
            while (it.hasNext()) {
                ((BlockProgressBar) it.next().getChildAt(this.currentPlayColumn - 1).findViewById(R.id.blockItem)).setPlay(false);
            }
            this.currentPlayColumn = 0;
        }
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            this.playBeat.setText(R.string.play);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setVelocity() {
        this.showVelocity = Math.max(40, Math.min(this.showVelocity, 240));
        Timer timer = this.timer;
        if (timer != null && this.isPlay) {
            timer.cancel();
            this.timer = new Timer();
            TimerTask timerTask = new TimerTask() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.6
                @Override // java.util.TimerTask, java.lang.Runnable
                public void run() {
                    LooperDrumFragment.this.sendDataAndUpdateView();
                }
            };
            this.updateTask = timerTask;
            this.timer.schedule(timerTask, 0L, hzToMs(this.showVelocity));
        }
        this.velocityText.setText(String.valueOf(this.showVelocity));
    }

    private long hzToMs(long j) {
        return 60000 / (j * 4);
    }

    private void setEventListener() {
        this.gate.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.7
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (z) {
                    LooperDrumFragment.this.gateValue.setText(String.valueOf(i));
                    ControlBaseFragment.communicationUtil.writeDataToBleNotRes(LooperCommunication.looperMiniSetGate(i));
                }
            }
        });
        this.volume.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.8
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (z) {
                    LooperDrumFragment.this.volumeValue.setText(String.valueOf(i));
                    ControlBaseFragment.communicationUtil.writeDataToBleNotRes(LooperCommunication.looperAutoSetVolume(i));
                }
            }
        });
        this.copyAllDrum.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda11
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m250xe5b7863f(view);
            }
        });
        this.resetFactory.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda20
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m251xffd304de(view);
            }
        });
        this.pasteAllDrum.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda21
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m252x19ee837d(view);
            }
        });
        this.btnVelocity.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.12
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                if (motionEvent.getAction() == 1) {
                    if (LooperDrumFragment.this.isSetStartTime) {
                        LooperDrumFragment.this.showVelocity = (int) (60000.0d / (((System.currentTimeMillis() - LooperDrumFragment.this.startTime) / 4.0d) * 4.0d));
                        LooperDrumFragment.this.setVelocity();
                        LooperDrumFragment.this.startTime = System.currentTimeMillis();
                    } else {
                        LooperDrumFragment.this.startTime = System.currentTimeMillis();
                        LooperDrumFragment.this.isSetStartTime = true;
                        LooperDrumFragment.this.handler.sendEmptyMessageDelayed(LooperDrumFragment.CANCEL_VEL, 2000L);
                    }
                }
                return true;
            }
        });
        this.back.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda22
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m253x340a021c(view);
            }
        });
        this.makeDrumWindow.setOnDismissListener(new PopupWindow.OnDismissListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.14
            @Override // android.widget.PopupWindow.OnDismissListener
            public void onDismiss() {
                LooperDrumFragment.this.deleteTimer();
                LooperDrumFragment.this.makeDrumWindow.dismiss();
            }
        });
        this.showMakeDrum.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda23
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m254x4e2580bb(view);
            }
        });
        this.blockSV.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda24
            @Override // android.view.View.OnTouchListener
            public final boolean onTouch(View view, MotionEvent motionEvent) {
                return this.f$0.m255x6840ff5a(view, motionEvent);
            }
        });
        this.powerStep.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda25
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public final void onCheckedChanged(RadioGroup radioGroup, int i) {
                this.f$0.m256x825c7df9(radioGroup, i);
            }
        });
        this.toneSV.setOnScrollViewListener(new ScrollViewPlus.ScrollViewListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda1
            @Override // com.cubeSuite.customControl.ScrollViewPlus.ScrollViewListener
            public final void onScrollChanged(ScrollViewPlus scrollViewPlus, int i, int i2, int i3, int i4) {
                this.f$0.m257x9c77fc98(scrollViewPlus, i, i2, i3, i4);
            }
        });
        this.subBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda2
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m258xb6937b37(view);
            }
        });
        this.addBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m259xf4f05ce1(view);
            }
        });
        this.pageSelect.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda18
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public final void onCheckedChanged(RadioGroup radioGroup, int i) {
                this.f$0.m260xf0bdb80(radioGroup, i);
            }
        });
        this.playBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda19
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m261x29275a1f(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$setEventListener$11$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m250xe5b7863f(View view) {
        this.ldc.readAllData(new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.9
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    ((ClipboardManager) MainActivity.context.getSystemService("clipboard")).setPrimaryClip(ClipData.newPlainText("Label", LooperDrumFragment.dataToCode(bArr)));
                    Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.copyDrumCodeComplete, 1).show();
                }
            }
        });
    }

    /* JADX INFO: renamed from: lambda$setEventListener$12$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m251xffd304de(View view) {
        AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE).setTitle(R.string.warning).setContent(R.string.hintResetFactory).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.10
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                String stream;
                int i = AnonymousClass18.$SwitchMap$com$cubeSuite$enumeration$DrumType[LooperDrumFragment.this.drumType.ordinal()];
                if (i == 1) {
                    stream = FileUtils.readStream(LooperDrumFragment.this.getResources().openRawResource(R.raw.reset_factory_drum));
                } else if (i == 2) {
                    stream = FileUtils.readStream(LooperDrumFragment.this.getResources().openRawResource(R.raw.reset_factory_drum_cyd));
                } else {
                    stream = i != 3 ? "" : FileUtils.readStream(LooperDrumFragment.this.getResources().openRawResource(R.raw.drum_buddy));
                }
                byte[] bArrCodeToData = LooperDrumFragment.codeToData(stream);
                AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setTitle(R.string.hint).setContent(R.string.recoveringData);
                LooperDrumFragment.this.ldc.writeAllData(bArrCodeToData, new AnonymousClass1());
            }

            /* JADX INFO: renamed from: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$10$1, reason: invalid class name */
            class AnonymousClass1 implements Callback.BleCommunicationCallback {
                AnonymousClass1() {
                }

                @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                public void writeDataCallback(boolean z) {
                    if (z) {
                        LooperDrumFragment.this.readDrumDataToView(LooperDrumFragment.this.drumIndex, new LooperDrumCommunication.DrumDataCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$10$1$$ExternalSyntheticLambda0
                            @Override // com.cubeSuite.communication.LooperDrumCommunication.DrumDataCallback
                            public final void dataCallback(boolean z2, Map map) {
                                this.f$0.m263x7b6d7eb3(z2, map);
                            }
                        });
                    } else {
                        AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).closeDialog();
                        Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.resetFailure, 1).show();
                    }
                }

                /* JADX INFO: renamed from: lambda$writeDataCallback$0$com-cubeSuite-fragment-looperDrum-LooperDrumFragment$10$1, reason: not valid java name */
                /* synthetic */ void m263x7b6d7eb3(boolean z, Map map) {
                    if (z) {
                        LooperDrumFragment.this.saveAllDrum();
                    } else {
                        AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).closeDialog();
                        Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.resetFailure, 1).show();
                    }
                }
            }
        }, false).showDialog();
    }

    /* JADX INFO: renamed from: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$18, reason: invalid class name */
    static /* synthetic */ class AnonymousClass18 {
        static final /* synthetic */ int[] $SwitchMap$com$cubeSuite$enumeration$DrumType;

        static {
            int[] iArr = new int[DrumType.values().length];
            $SwitchMap$com$cubeSuite$enumeration$DrumType = iArr;
            try {
                iArr[DrumType.sinco.ordinal()] = 1;
            } catch (NoSuchFieldError unused) {
            }
            try {
                $SwitchMap$com$cubeSuite$enumeration$DrumType[DrumType.cyd.ordinal()] = 2;
            } catch (NoSuchFieldError unused2) {
            }
            try {
                $SwitchMap$com$cubeSuite$enumeration$DrumType[DrumType.drumBuddy.ordinal()] = 3;
            } catch (NoSuchFieldError unused3) {
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setEventListener$13$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m252x19ee837d(View view) {
        String clipboardContent = getClipboardContent(MainActivity.context);
        if (clipboardContent == null || clipboardContent.length() < 5) {
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.correctCopy, 1).show();
            return;
        }
        final byte[] bArrCodeToData = codeToData(clipboardContent);
        if (bArrCodeToData.length == 0) {
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.incorrectFormat, 1).show();
        } else {
            AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE).setTitle(R.string.warning).setContent(R.string.hintPasteDrum).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.11
                @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
                public void confirmBtnClick() {
                    AlertDialogUtil.getInstance(LooperDrumFragment.this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).setTitle(R.string.hint).setContent(R.string.pasteingData);
                    LooperDrumFragment.this.ldc.writeAllData(bArrCodeToData, new AnonymousClass1());
                }

                /* JADX INFO: renamed from: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$11$1, reason: invalid class name */
                class AnonymousClass1 implements Callback.BleCommunicationCallback {
                    AnonymousClass1() {
                    }

                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            LooperDrumFragment.this.readDrumDataToView(LooperDrumFragment.this.drumIndex, new LooperDrumCommunication.DrumDataCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$11$1$$ExternalSyntheticLambda0
                                @Override // com.cubeSuite.communication.LooperDrumCommunication.DrumDataCallback
                                public final void dataCallback(boolean z2, Map map) {
                                    this.f$0.m264x7b6d8274(z2, map);
                                }
                            });
                        }
                    }

                    /* JADX INFO: renamed from: lambda$writeDataCallback$0$com-cubeSuite-fragment-looperDrum-LooperDrumFragment$11$1, reason: not valid java name */
                    /* synthetic */ void m264x7b6d8274(boolean z, Map map) {
                        if (z) {
                            LooperDrumFragment.this.saveAllDrum();
                        }
                    }
                }
            }, false).showDialog();
        }
    }

    /* JADX INFO: renamed from: lambda$setEventListener$14$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m253x340a021c(View view) {
        AlertDialogUtil.getInstance(this.activity).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_VISIBLE).setTitle(R.string.hint).setContent(R.string.saveToDevice).setBtnClick(new AlertDialogUtil.BtnClick() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.13
            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void confirmBtnClick() {
                LooperDrumFragment.this.saveAllDrum();
                LooperDrumFragment.this.makeDrumWindow.dismiss();
            }

            @Override // com.cubeSuite.customControl.AlertDialogUtil.BtnClick
            public void cancelBtnClick() {
                LooperDrumFragment.this.makeDrumWindow.dismiss();
            }
        }).showDialog();
    }

    /* JADX INFO: renamed from: lambda$setEventListener$15$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m254x4e2580bb(View view) {
        this.makeDrumWindow.showAtLocation(this.thisView, 80, 0, 0);
    }

    /* JADX WARN: Code duplicated, block: B:29:0x00cd  */
    /* JADX INFO: renamed from: lambda$setEventListener$16$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ boolean m255x6840ff5a(View view, MotionEvent motionEvent) {
        boolean z = true;
        if (this.isAdjustable) {
            return true;
        }
        float rawX = motionEvent.getRawX();
        float rawY = motionEvent.getRawY();
        int[] iArr = new int[2];
        float width = this.linearPoints.get(0).getChildAt(0).getWidth();
        for (LinearLayout linearLayout : this.linearPoints) {
            int i = (this.currentPage - (z ? 1 : 0)) * 16;
            while (i < Math.min(this.currentPage * 16, this.beatNum * 4)) {
                linearLayout.getChildAt(i).getLocationOnScreen(iArr);
                int i2 = iArr[0];
                if (rawX > i2 && rawX < i2 + width) {
                    int i3 = iArr[z ? 1 : 0];
                    if (rawY > i3 && rawY < i3 + width) {
                        if (motionEvent.getAction() == 0) {
                            this.blockState = (((BlockProgressBar) linearLayout.getChildAt(i).findViewById(R.id.blockItem)).isEnable() ? 1 : 0) ^ (z ? 1 : 0);
                        }
                        final BlockProgressBar blockProgressBar = (BlockProgressBar) linearLayout.getChildAt(i).findViewById(R.id.blockItem);
                        boolean zIsEnable = blockProgressBar.isEnable();
                        boolean z2 = this.blockState;
                        if (zIsEnable == z2) {
                            return z;
                        }
                        blockProgressBar.setEnable(z2);
                        final boolean z3 = this.blockState;
                        if (blockProgressBar.isEnable()) {
                            this.ldc.sendDrumData(blockProgressBar.getProgress(), blockProgressBar.getCoords(), this.drumIndex, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.15
                                @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                                public void writeDataCallback(boolean z4) {
                                    if (z4) {
                                        return;
                                    }
                                    blockProgressBar.setEnable(!z3);
                                }
                            });
                        } else {
                            this.ldc.sendDrumData(0, blockProgressBar.getCoords(), this.drumIndex, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.16
                                @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                                public void writeDataCallback(boolean z4) {
                                    if (z4) {
                                        return;
                                    }
                                    blockProgressBar.setEnable(!z3);
                                }
                            });
                        }
                    }
                }
                i++;
                z = z ? 1 : 0;
            }
        }
        return z ? 1 : 0;
    }

    /* JADX INFO: renamed from: lambda$setEventListener$17$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m256x825c7df9(RadioGroup radioGroup, int i) {
        if (i == this.power.getId()) {
            this.isAdjustable = true;
        } else if (i == this.step.getId()) {
            this.isAdjustable = false;
        }
        for (LinearLayout linearLayout : this.linearPoints) {
            for (int i2 = 0; i2 < linearLayout.getChildCount(); i2++) {
                ((BlockProgressBar) linearLayout.getChildAt(i2).findViewById(R.id.blockItem)).setAdjustable(this.isAdjustable);
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setEventListener$18$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m257x9c77fc98(ScrollViewPlus scrollViewPlus, int i, int i2, int i3, int i4) {
        this.blockSV.scrollTo(i, i2);
    }

    /* JADX INFO: renamed from: lambda$setEventListener$19$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m258xb6937b37(View view) {
        int iMax = Math.max(this.beatNum - 1, 2);
        this.beatNum = iMax;
        this.beatText.setText(String.valueOf(iMax));
        this.ldc.sendBeatData(this.drumIndex, this.beatNum);
        if (this.beatNum <= 4) {
            for (int i = 0; i < this.pageSelect.getChildCount(); i++) {
                ((RadioButton) this.pageSelect.getChildAt(i)).setChecked(false);
            }
        }
        updateBlockNum();
        updatePageItem();
    }

    /* JADX INFO: renamed from: lambda$setEventListener$20$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m259xf4f05ce1(View view) {
        int i = this.beatNum;
        if (i >= 8) {
            return;
        }
        int i2 = i + 1;
        this.beatNum = i2;
        this.beatText.setText(String.valueOf(i2));
        this.ldc.addBeatData(this.drumIndex, this.beatNum);
        updateBlockNum();
        updatePageItem();
    }

    /* JADX INFO: renamed from: lambda$setEventListener$21$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m260xf0bdb80(RadioGroup radioGroup, int i) {
        for (int i2 = 0; i2 < this.pageSelect.getChildCount(); i2++) {
            if (((RadioButton) this.pageSelect.getChildAt(i2)).isChecked()) {
                this.currentPage = i2 + 1;
                updateBlockNum();
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setEventListener$22$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m261x29275a1f(View view) {
        if (this.isPlay) {
            deleteTimer();
            return;
        }
        this.playBeat.setText(R.string.stop);
        this.isPlay = true;
        this.timer = new Timer();
        TimerTask timerTask = new TimerTask() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment.17
            @Override // java.util.TimerTask, java.lang.Runnable
            public void run() {
                LooperDrumFragment.this.sendDataAndUpdateView();
            }
        };
        this.updateTask = timerTask;
        this.timer.schedule(timerTask, 0L, hzToMs(this.showVelocity));
    }

    private void updatePageItem() {
        int i = this.beatNum;
        int i2 = (i * 4) % 16;
        int i3 = (i * 4) / 16;
        if (i2 != 0) {
            i3++;
        }
        if (this.pageSelect.getChildCount() < i3) {
            for (int i4 = 0; i4 < i3 - this.pageSelect.getChildCount(); i4++) {
                RadioButton radioButton = new RadioButton(this.activity);
                radioButton.setTextSize(2, 14.0f);
                radioButton.setBackgroundResource(R.drawable.radio);
                radioButton.setButtonDrawable(new StateListDrawable());
                radioButton.setText("❉");
                radioButton.setTextColor(getResources().getColorStateList(R.drawable.btn_text_color));
                radioButton.setPadding(dp2px(9.0f), dp2px(5.0f), dp2px(9.0f), dp2px(5.0f));
                RadioGroup.LayoutParams layoutParams = new RadioGroup.LayoutParams(-2, -2);
                layoutParams.setMargins(0, 0, 0, 0);
                this.pageSelect.addView(radioButton, layoutParams);
            }
        }
        if (this.beatNum <= 4) {
            ((RadioButton) this.pageSelect.getChildAt(0)).setText("1 - " + this.beatNum);
        } else {
            ((RadioButton) this.pageSelect.getChildAt(0)).setText("1 - 4");
            int i5 = 1;
            while (i5 < this.pageSelect.getChildCount()) {
                int i6 = i5 + 1;
                if (i6 * 16 >= this.beatNum * 4) {
                    ((RadioButton) this.pageSelect.getChildAt(i5)).setText(((i5 * 4) + 1) + " - " + this.beatNum);
                } else {
                    ((RadioButton) this.pageSelect.getChildAt(i5)).setText(((i5 * 4) + 1) + " - " + (i6 * 4));
                }
                i5 = i6;
            }
        }
        int iMin = Math.min(this.currentPage, i3);
        this.currentPage = iMin;
        ((RadioButton) this.pageSelect.getChildAt(iMin - 1)).setSelected(true);
        int i7 = 0;
        while (i7 < this.pageSelect.getChildCount()) {
            this.pageSelect.getChildAt(i7).setVisibility(i7 < i3 ? 0 : 8);
            i7++;
        }
    }

    private void updateBlockNum() {
        Resources resources;
        int i;
        int i2 = this.beatNum * 4;
        int childCount = this.linearPoints.get(0).getChildCount();
        if (childCount < i2) {
            int i3 = (i2 - childCount) / 4;
            for (int i4 = 0; i4 < this.linearPoints.size(); i4++) {
                int i5 = 0;
                while (i5 < i3 * 4) {
                    if (i4 % 2 == 0) {
                        resources = getResources();
                        i = R.color.seq1;
                    } else {
                        resources = getResources();
                        i = R.color.seq2;
                    }
                    int color = resources.getColor(i);
                    View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.sequencer_item, (ViewGroup) this.blockList, false);
                    BlockProgressBar blockProgressBar = (BlockProgressBar) viewInflate.findViewById(R.id.blockItem);
                    blockProgressBar.setBlockColor(color);
                    blockProgressBar.setAdjustable(this.isAdjustable);
                    blockProgressBar.setCoords(i4, this.linearPoints.get(i4).getChildCount());
                    blockProgressBar.setTone(i4);
                    blockProgressBar.setValueChange(new BlockProgressBar.BlockListener() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda0
                        @Override // com.cubeSuite.customControl.BlockProgressBar.BlockListener
                        public final void listenerValueChange(int i6, int i7, int i8) {
                            this.f$0.m262xf2eb3174(i6, i7, i8);
                        }
                    });
                    i5++;
                    if (i5 % 4 == 0) {
                        ViewGroup.MarginLayoutParams marginLayoutParams = (ViewGroup.MarginLayoutParams) blockProgressBar.getLayoutParams();
                        marginLayoutParams.setMargins(dp2px(1.0f), dp2px(1.0f), dp2px(4.0f), dp2px(1.0f));
                        blockProgressBar.setLayoutParams(marginLayoutParams);
                    }
                    this.linearPoints.get(i4).addView(viewInflate);
                }
            }
        }
        for (int i6 = 0; i6 < this.linearPoints.size(); i6++) {
            int i7 = 0;
            while (i7 < this.linearPoints.get(i6).getChildCount()) {
                if (i7 >= this.beatNum * 4) {
                    BlockProgressBar blockProgressBar2 = (BlockProgressBar) this.linearPoints.get(i6).getChildAt(i7).findViewById(R.id.blockItem);
                    blockProgressBar2.setProgress(0.0f);
                    blockProgressBar2.setEnable(false);
                }
                int pageNum = getPageNum();
                int i8 = 8;
                if (pageNum == 1) {
                    this.linearPoints.get(i6).getChildAt(i7).setVisibility(i7 < i2 ? 0 : 8);
                } else if (this.currentPage == pageNum) {
                    View childAt = this.linearPoints.get(i6).getChildAt(i7);
                    if (i7 < i2 && i7 >= (this.currentPage - 1) * 16) {
                        i8 = 0;
                    }
                    childAt.setVisibility(i8);
                } else {
                    View childAt2 = this.linearPoints.get(i6).getChildAt(i7);
                    int i9 = this.currentPage;
                    if (i7 < i9 * 16 && i7 >= (i9 - 1) * 16) {
                        i8 = 0;
                    }
                    childAt2.setVisibility(i8);
                }
                i7++;
            }
        }
    }

    /* JADX INFO: renamed from: lambda$updateBlockNum$23$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m262xf2eb3174(int i, int i2, int i3) {
        if (i == 1) {
            this.ldc.sendDrumData(i2, i3, this.drumIndex);
        }
        this.linkageHistogram.setProgress(i2);
    }

    int getPageNum() {
        int i = this.beatNum;
        return ((i * 4) / 16) + ((i * 4) % 16 != 0 ? 1 : 0);
    }

    private int dp2px(float f) {
        return (int) ((f * getResources().getDisplayMetrics().density) + 0.5f);
    }

    public void readDrumDataToView(int i, final LooperDrumCommunication.DrumDataCallback drumDataCallback) {
        this.ldc.readDrumData(i, new LooperDrumCommunication.DrumDataCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda5
            @Override // com.cubeSuite.communication.LooperDrumCommunication.DrumDataCallback
            public final void dataCallback(boolean z, Map map) {
                this.f$0.m249xbef6b977(drumDataCallback, z, map);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$readDrumDataToView$24$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m249xbef6b977(LooperDrumCommunication.DrumDataCallback drumDataCallback, boolean z, Map map) {
        if (z) {
            drumDataCallback.dataCallback(true, map);
            dataToMode(((Byte) map.get("beat")).byteValue(), (byte[]) map.get("drumData"));
        } else {
            AlertDialogUtil.getInstance(this.activity).closeDialog();
            Toast.makeText(ActivityStackUtil.getInstance().getActivity(), R.string.loadDataFail, 1).show();
        }
    }

    public void dataToMode(byte b, byte[] bArr) {
        this.beatNum = Math.min(8, Math.max((int) b, 2));
        this.beatText.setText(String.valueOf((int) b));
        updateBlockNum();
        updatePageItem();
        for (int i = 0; i < this.linearPoints.size(); i++) {
            for (int i2 = 0; i2 < this.linearPoints.get(i).getChildCount(); i2++) {
                BlockProgressBar blockProgressBar = (BlockProgressBar) this.linearPoints.get(i).getChildAt(i2).findViewById(R.id.blockItem);
                int i3 = (i2 * 16) + i;
                if (bArr[i3] > 0) {
                    blockProgressBar.setEnable(true);
                    blockProgressBar.setProgress(bArr[i3]);
                } else {
                    blockProgressBar.setEnable(false);
                }
            }
        }
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        this.ldc = new LooperDrumCommunication(communicationUtil);
        this.drumIndex = 0;
        readDrumDataToView(0, new LooperDrumCommunication.DrumDataCallback() { // from class: com.cubeSuite.fragment.looperDrum.LooperDrumFragment$$ExternalSyntheticLambda17
            @Override // com.cubeSuite.communication.LooperDrumCommunication.DrumDataCallback
            public final void dataCallback(boolean z, Map map) {
                this.f$0.m237xf33ebdaa(z, map);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$initData$25$com-cubeSuite-fragment-looperDrum-LooperDrumFragment, reason: not valid java name */
    /* synthetic */ void m237xf33ebdaa(boolean z, Map map) {
        if (z) {
            AlertDialogUtil.getInstance(this.activity).closeDialog();
        }
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }
}
