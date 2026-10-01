package com.cubeSuite.fragment;

import android.content.res.Resources;
import android.graphics.drawable.StateListDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.ItemTouchHelper;
import com.cubeSuite.R;
import com.cubeSuite.activitys.ActivityStackUtil;
import com.cubeSuite.communication.LooperDrumCommunication;
import com.cubeSuite.customControl.BlockProgressBar;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.ScrollViewPlus;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/* JADX INFO: loaded from: classes.dex */
public class SequencerFragment extends AppCompatActivity {
    private static final int MSG_PLAY_MIDI = 10001;
    private final String[] TONE_TYPE;
    private Button addBeat;
    private TextView beatText;
    LinearLayout blockList;
    private ScrollViewPlus blockSV;
    private LooperDrumCommunication ldc;
    private List<LinearLayout> linearPoints;
    private RadioGroup pageSelect;
    private ButtonIconView playBeat;
    private RadioButton power;
    private RadioGroup powerStep;
    private RadioButton step;
    private Button subBeat;
    private Timer timer;
    private ScrollViewPlus toneSV;
    LinearLayout toneStrList;
    private TimerTask updateTask;
    private boolean isAdjustable = false;
    private int beatNum = 4;
    private int noteNum = 4;
    private final int MAX_BLOCK = 16;
    private int currentPage = 1;
    private boolean blockState = false;
    private int velocity = ItemTouchHelper.Callback.DEFAULT_DRAG_ANIMATION_DURATION;
    private boolean isPlay = false;
    private int currentPlayColumn = 0;
    private Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.SequencerFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            int i = message.what;
            return false;
        }
    });

    private int getBlockNum() {
        return this.beatNum * this.noteNum;
    }

    public void sendDataAndUpdateView() {
        int blockNum = getBlockNum();
        if (this.currentPlayColumn >= blockNum) {
            this.currentPlayColumn = 0;
        }
        for (LinearLayout linearLayout : this.linearPoints) {
            int i = this.currentPlayColumn;
            if (i == 0) {
                ((BlockProgressBar) linearLayout.getChildAt(blockNum - 1).findViewById(R.id.blockItem)).setPlay(false);
            } else {
                ((BlockProgressBar) linearLayout.getChildAt(i - 1).findViewById(R.id.blockItem)).setPlay(false);
            }
            ((BlockProgressBar) linearLayout.getChildAt(this.currentPlayColumn).findViewById(R.id.blockItem)).setPlay(true);
        }
        this.currentPlayColumn++;
    }

    public SequencerFragment() {
        String[] strArr = {"QWE", "RTY", "YUI", "OPA", "SDF", "GHJ", "KLQ", "ZXC", "VBN", "FHJ", "CDR", "GGH", "KIN", "FDC", "ESX", "ASC"};
        this.TONE_TYPE = strArr;
        this.linearPoints = new ArrayList(strArr.length);
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        Resources resources;
        int i;
        super.onCreate(bundle);
        setContentView(R.layout.sequencer_layout);
        ActivityStackUtil.getInstance().addActivity(this);
        loadView();
        for (int i2 = 0; i2 < this.TONE_TYPE.length; i2++) {
            View viewInflate = LayoutInflater.from(this).inflate(R.layout.tone_str_item, (ViewGroup) this.toneStrList, false);
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
            LinearLayout linearLayout = new LinearLayout(this);
            new LinearLayout.LayoutParams(-2, -2);
            linearLayout.setOrientation(0);
            this.linearPoints.add(linearLayout);
            int i3 = 0;
            while (i3 < this.beatNum * this.noteNum) {
                View viewInflate2 = LayoutInflater.from(this).inflate(R.layout.sequencer_item, (ViewGroup) this.blockList, false);
                BlockProgressBar blockProgressBar = (BlockProgressBar) viewInflate2.findViewById(R.id.blockItem);
                blockProgressBar.setBlockColor(color);
                i3++;
                if (i3 % this.noteNum == 0) {
                    ViewGroup.MarginLayoutParams marginLayoutParams = (ViewGroup.MarginLayoutParams) blockProgressBar.getLayoutParams();
                    marginLayoutParams.setMargins(dp2px(1.0f), dp2px(1.0f), dp2px(4.0f), dp2px(1.0f));
                    blockProgressBar.setLayoutParams(marginLayoutParams);
                }
                linearLayout.addView(viewInflate2);
            }
            this.blockList.addView(linearLayout);
        }
        updatePageItem();
    }

    private void loadView() {
        this.playBeat = (ButtonIconView) findViewById(R.id.playBeat);
        this.powerStep = (RadioGroup) findViewById(R.id.powerStep);
        this.blockSV = (ScrollViewPlus) findViewById(R.id.blockSV);
        this.toneSV = (ScrollViewPlus) findViewById(R.id.toneSV);
        this.power = (RadioButton) findViewById(R.id.power);
        this.step = (RadioButton) findViewById(R.id.step);
        this.addBeat = (Button) findViewById(R.id.addBeat);
        this.subBeat = (Button) findViewById(R.id.subBeat);
        this.beatText = (TextView) findViewById(R.id.beatText);
        this.blockList = (LinearLayout) findViewById(R.id.blockList);
        this.toneStrList = (LinearLayout) findViewById(R.id.toneStrList);
        this.pageSelect = (RadioGroup) findViewById(R.id.pageSelect);
        this.beatText.setText(String.valueOf(this.beatNum));
        setEventListener();
    }

    private void setEventListener() {
        this.blockSV.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda0
            @Override // android.view.View.OnTouchListener
            public final boolean onTouch(View view, MotionEvent motionEvent) {
                return this.f$0.m224x6fa11f09(view, motionEvent);
            }
        });
        this.powerStep.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda1
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public final void onCheckedChanged(RadioGroup radioGroup, int i) {
                this.f$0.m225x77065428(radioGroup, i);
            }
        });
        this.toneSV.setOnScrollViewListener(new ScrollViewPlus.ScrollViewListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda2
            @Override // com.cubeSuite.customControl.ScrollViewPlus.ScrollViewListener
            public final void onScrollChanged(ScrollViewPlus scrollViewPlus, int i, int i2, int i3, int i4) {
                this.f$0.m226x7e6b8947(scrollViewPlus, i, i2, i3, i4);
            }
        });
        this.subBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m227x85d0be66(view);
            }
        });
        this.addBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda4
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m228x8d35f385(view);
            }
        });
        this.pageSelect.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda5
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public final void onCheckedChanged(RadioGroup radioGroup, int i) {
                this.f$0.m229x949b28a4(radioGroup, i);
            }
        });
        this.playBeat.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.SequencerFragment$$ExternalSyntheticLambda6
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m230x9c005dc3(view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$setEventListener$0$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ boolean m224x6fa11f09(View view, MotionEvent motionEvent) {
        if (this.isAdjustable) {
            return true;
        }
        float rawX = motionEvent.getRawX();
        float rawY = motionEvent.getRawY();
        int[] iArr = new int[2];
        float width = this.linearPoints.get(0).getChildAt(0).getWidth();
        for (LinearLayout linearLayout : this.linearPoints) {
            for (int i = (this.currentPage - 1) * 16; i < Math.min(this.currentPage * 16, this.beatNum * this.noteNum); i++) {
                linearLayout.getChildAt(i).getLocationOnScreen(iArr);
                int i2 = iArr[0];
                if (rawX > i2 && rawX < i2 + width) {
                    int i3 = iArr[1];
                    if (rawY > i3 && rawY < i3 + width) {
                        if (motionEvent.getAction() == 0) {
                            this.blockState = !((BlockProgressBar) linearLayout.getChildAt(i).findViewById(R.id.blockItem)).isEnable();
                        }
                        ((BlockProgressBar) linearLayout.getChildAt(i).findViewById(R.id.blockItem)).setEnable(this.blockState);
                    }
                }
            }
        }
        return true;
    }

    /* JADX INFO: renamed from: lambda$setEventListener$1$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m225x77065428(RadioGroup radioGroup, int i) {
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

    /* JADX INFO: renamed from: lambda$setEventListener$2$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m226x7e6b8947(ScrollViewPlus scrollViewPlus, int i, int i2, int i3, int i4) {
        this.blockSV.scrollTo(i, i2);
    }

    /* JADX INFO: renamed from: lambda$setEventListener$3$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m227x85d0be66(View view) {
        int iMax = Math.max(this.beatNum - 1, 2);
        this.beatNum = iMax;
        this.beatText.setText(String.valueOf(iMax));
        updateBlockNum();
        updatePageItem();
    }

    /* JADX INFO: renamed from: lambda$setEventListener$4$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m228x8d35f385(View view) {
        int iMin = Math.min(this.beatNum + 1, 8);
        this.beatNum = iMin;
        this.beatText.setText(String.valueOf(iMin));
        updateBlockNum();
        updatePageItem();
    }

    /* JADX INFO: renamed from: lambda$setEventListener$5$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m229x949b28a4(RadioGroup radioGroup, int i) {
        for (int i2 = 0; i2 < this.pageSelect.getChildCount(); i2++) {
            if (((RadioButton) this.pageSelect.getChildAt(i2)).isChecked()) {
                this.currentPage = i2 + 1;
                updateBlockNum();
            }
        }
    }

    /* JADX INFO: renamed from: lambda$setEventListener$6$com-cubeSuite-fragment-SequencerFragment, reason: not valid java name */
    /* synthetic */ void m230x9c005dc3(View view) {
        if (this.isPlay) {
            Timer timer = this.timer;
            if (timer != null) {
                timer.cancel();
                this.playBeat.setText(R.string.play);
                this.isPlay = false;
                return;
            }
            return;
        }
        this.playBeat.setText(R.string.stop);
        this.timer = new Timer();
        TimerTask timerTask = new TimerTask() { // from class: com.cubeSuite.fragment.SequencerFragment.2
            @Override // java.util.TimerTask, java.lang.Runnable
            public void run() {
                SequencerFragment.this.sendDataAndUpdateView();
            }
        };
        this.updateTask = timerTask;
        this.timer.schedule(timerTask, 0L, this.velocity);
        this.isPlay = true;
    }

    private void updatePageItem() {
        int i = this.beatNum;
        int i2 = this.noteNum;
        int i3 = (i * i2) % 16;
        int i4 = (i * i2) / 16;
        if (i3 != 0) {
            i4++;
        }
        if (this.pageSelect.getChildCount() < i4) {
            for (int i5 = 0; i5 < i4 - this.pageSelect.getChildCount(); i5++) {
                RadioButton radioButton = new RadioButton(this);
                radioButton.setTextSize(2, 14.0f);
                radioButton.setBackgroundResource(R.drawable.radio);
                radioButton.setButtonDrawable(new StateListDrawable());
                radioButton.setText("第" + (this.pageSelect.getChildCount() + i5) + "个");
                radioButton.setTextColor(getResources().getColorStateList(R.drawable.btn_text_color));
                radioButton.setPadding(dp2px(9.0f), dp2px(5.0f), dp2px(9.0f), dp2px(5.0f));
                RadioGroup.LayoutParams layoutParams = new RadioGroup.LayoutParams(-2, -2);
                layoutParams.setMargins(0, 0, 0, 0);
                this.pageSelect.addView(radioButton, layoutParams);
            }
        }
        int iMin = Math.min(this.currentPage, i4);
        this.currentPage = iMin;
        ((RadioButton) this.pageSelect.getChildAt(iMin - 1)).setSelected(true);
        int i6 = 0;
        while (i6 < this.pageSelect.getChildCount()) {
            this.pageSelect.getChildAt(i6).setVisibility(i6 < i4 ? 0 : 8);
            i6++;
        }
    }

    private void updateBlockNum() {
        Resources resources;
        int i;
        int i2 = this.beatNum * this.noteNum;
        int childCount = this.linearPoints.get(0).getChildCount();
        if (childCount < i2) {
            int i3 = (i2 - childCount) / this.noteNum;
            for (int i4 = 0; i4 < this.linearPoints.size(); i4++) {
                int i5 = 0;
                while (i5 < this.noteNum * i3) {
                    if (i4 % 2 == 0) {
                        resources = getResources();
                        i = R.color.seq1;
                    } else {
                        resources = getResources();
                        i = R.color.seq2;
                    }
                    int color = resources.getColor(i);
                    View viewInflate = LayoutInflater.from(this).inflate(R.layout.sequencer_item, (ViewGroup) this.blockList, false);
                    BlockProgressBar blockProgressBar = (BlockProgressBar) viewInflate.findViewById(R.id.blockItem);
                    blockProgressBar.setBlockColor(color);
                    blockProgressBar.setAdjustable(this.isAdjustable);
                    i5++;
                    if (i5 % this.noteNum == 0) {
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

    int getPageNum() {
        int i = this.beatNum;
        int i2 = this.noteNum;
        return ((i * i2) / 16) + ((i * i2) % 16 != 0 ? 1 : 0);
    }

    private int dp2px(float f) {
        return (int) ((f * getResources().getDisplayMetrics().density) + 0.5f);
    }

    @Override // androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onResume() {
        if (getRequestedOrientation() != 0) {
            setRequestedOrientation(0);
        }
        super.onResume();
    }
}
