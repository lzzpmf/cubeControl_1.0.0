package com.cubeSuite.customControl;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;
import android.text.TextUtils;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class BlockProgressBar extends View {
    private final int DEFAULT_BLOCK_COLOR;
    private final int DEFAULT_TEXT_COLOR;
    private int blockColor;
    private BlockListener blockListener;
    private int coords;
    private boolean highlight;
    private int initialBlockColor;
    private boolean isAdjustable;
    private boolean isEnable;
    private boolean isPlay;
    private final RectF mBorder;
    private float mMax;
    private Paint mPaint;
    private String mPrefix;
    private final RectF mReachedRectF;
    private String mSuffix;
    private int mTextColor;
    private float mTextSize;
    private final RectF mUnreachedRectF;
    private int progress;
    private float round;
    private int tone;
    int[] toneArray;

    public interface BlockListener {
        void listenerValueChange(int i, int i2, int i3);
    }

    public void setAdjustable(boolean z) {
        this.isAdjustable = z;
        invalidate();
    }

    public void setTone(int i) {
        this.tone = this.toneArray[i];
    }

    public int getTone() {
        return this.tone;
    }

    public void setCoords(int i, int i2) {
        this.coords = (i2 * 16) + i;
    }

    public BlockProgressBar(Context context) {
        this(context, null);
    }

    public BlockProgressBar(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public BlockProgressBar(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mMax = 15.0f;
        this.DEFAULT_BLOCK_COLOR = getResources().getColor(R.color.dominantHue);
        this.DEFAULT_TEXT_COLOR = getResources().getColor(R.color.accent);
        this.tone = 0;
        this.mSuffix = "%";
        this.mPrefix = "";
        this.progress = 12;
        this.isPlay = false;
        this.mUnreachedRectF = new RectF(0.0f, 0.0f, 0.0f, 0.0f);
        this.mReachedRectF = new RectF(0.0f, 0.0f, 0.0f, 0.0f);
        this.mBorder = new RectF(0.0f, 0.0f, 0.0f, 0.0f);
        this.highlight = false;
        this.isEnable = false;
        this.isAdjustable = false;
        this.toneArray = new int[]{36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 49, 51, 53, 57, 58};
        initAttrs(context, attributeSet);
        initPainters();
    }

    public void setValueChange(BlockListener blockListener) {
        this.blockListener = blockListener;
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        if (!this.isAdjustable || !this.isEnable) {
            return false;
        }
        getParent().requestDisallowInterceptTouchEvent(true);
        if (motionEvent.getAction() == 0) {
            this.highlight = true;
        }
        if (motionEvent.getAction() == 1) {
            this.highlight = false;
        }
        this.blockListener.listenerValueChange(motionEvent.getAction(), this.progress, this.coords);
        setProgress(this.mMax - (motionEvent.getY() / (this.mUnreachedRectF.height() / this.mMax)));
        return true;
    }

    public int getCoords() {
        return this.coords;
    }

    private void initAttrs(Context context, AttributeSet attributeSet) {
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.BlockProgressBar);
        this.mMax = typedArrayObtainStyledAttributes.getInteger(2, (int) this.mMax);
        this.blockColor = typedArrayObtainStyledAttributes.getColor(1, this.DEFAULT_BLOCK_COLOR);
        this.mTextColor = typedArrayObtainStyledAttributes.getColor(7, this.DEFAULT_TEXT_COLOR);
        this.mSuffix = TextUtils.isEmpty(typedArrayObtainStyledAttributes.getString(6)) ? this.mSuffix : typedArrayObtainStyledAttributes.getString(6);
        this.mPrefix = TextUtils.isEmpty(typedArrayObtainStyledAttributes.getString(3)) ? this.mPrefix : typedArrayObtainStyledAttributes.getString(3);
        this.round = typedArrayObtainStyledAttributes.getDimension(5, dp2px(0.0f));
        this.mTextSize = typedArrayObtainStyledAttributes.getDimension(8, dp2px(16.0f));
        typedArrayObtainStyledAttributes.recycle();
    }

    private void initPainters() {
        Paint paint = new Paint(1);
        this.mPaint = paint;
        paint.setAntiAlias(true);
        this.mPaint.setStrokeCap(Paint.Cap.ROUND);
        this.mPaint.setTextAlign(Paint.Align.RIGHT);
        this.mPaint.setTextSize(this.mTextSize);
    }

    public void setTextSize(float f) {
        this.mPaint.setTextSize(f);
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        Canvas canvas2;
        super.onDraw(canvas);
        calculateDrawRectFWithoutProgressText();
        this.mPaint.setStyle(Paint.Style.FILL_AND_STROKE);
        if (this.isAdjustable) {
            if (this.isEnable) {
                this.mPaint.setColor(this.blockColor);
                if (this.highlight) {
                    this.mPaint.setAlpha(255);
                } else {
                    this.mPaint.setAlpha(210);
                }
                canvas.drawRect(this.mUnreachedRectF, this.mPaint);
                this.mPaint.setStrokeWidth(7.0f);
                this.mPaint.setStyle(Paint.Style.STROKE);
                this.mPaint.setColor(getResources().getColor(R.color.accent));
                float fMin = Math.min(Math.max(this.mReachedRectF.top, 5.0f), this.mUnreachedRectF.height() - 5.0f);
                canvas2 = canvas;
                canvas2.drawLine(this.mReachedRectF.left, fMin, this.mReachedRectF.right, fMin, this.mPaint);
            } else {
                canvas2 = canvas;
                this.mPaint.setColor(this.blockColor);
                this.mPaint.setAlpha(90);
                canvas2.drawRect(this.mUnreachedRectF, this.mPaint);
            }
        } else {
            canvas2 = canvas;
            this.mPaint.setColor(this.blockColor);
            if (!this.isEnable) {
                this.mPaint.setAlpha(90);
            } else {
                this.mPaint.setAlpha(210);
            }
            canvas2.drawRect(this.mUnreachedRectF, this.mPaint);
        }
        if (this.isPlay) {
            this.mPaint.setColor(getResources().getColor(R.color.accent));
            this.mPaint.setStrokeWidth(4.0f);
            this.mPaint.setStyle(Paint.Style.STROKE);
            canvas2.drawRect(this.mBorder, this.mPaint);
        }
    }

    public void setPlay(boolean z) {
        this.isPlay = z;
        invalidate();
    }

    private void calculateDrawRectFWithoutProgressText() {
        this.mReachedRectF.left = getPaddingLeft();
        this.mReachedRectF.right = getWidth() - getPaddingRight();
        this.mReachedRectF.bottom = getHeight();
        this.mUnreachedRectF.left = getPaddingLeft();
        this.mUnreachedRectF.top = 0.0f;
        this.mUnreachedRectF.right = getWidth() - getPaddingRight();
        this.mUnreachedRectF.bottom = getHeight();
        this.mBorder.left = getPaddingLeft();
        this.mBorder.top = 0.0f;
        this.mBorder.right = getWidth() - getPaddingRight();
        this.mBorder.bottom = getHeight();
        RectF rectF = this.mReachedRectF;
        float fHeight = this.mUnreachedRectF.height();
        float f = this.mMax;
        rectF.top = (fHeight / f) * (f - this.progress);
    }

    public float getParentLeft() {
        return ((View) getParent()).getLeft();
    }

    public float getParentTop() {
        return ((View) getParent()).getTop();
    }

    public float getParentRight() {
        return ((View) getParent()).getRight();
    }

    public float getParentBottom() {
        return ((View) getParent()).getBottom();
    }

    public float getParentTranslationX() {
        return ((View) getParent()).getX();
    }

    public float getParentTranslationY() {
        return ((View) getParent()).getY();
    }

    public float getMax() {
        return this.mMax;
    }

    public int getProgress() {
        return this.progress;
    }

    public void setMax(int i) {
        this.mMax = i;
        invalidate();
    }

    public void setProgress(float f) {
        this.progress = (int) Math.max(0.0f, Math.min(f, this.mMax));
        invalidate();
    }

    public void setBlockColor(int i) {
        this.blockColor = i;
        if (this.initialBlockColor == 0) {
            this.initialBlockColor = i;
        }
        invalidate();
    }

    private int dp2px(float f) {
        return (int) ((f * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }

    public boolean isEnable() {
        return this.isEnable;
    }

    public void setEnable(boolean z) {
        this.isEnable = z;
        invalidate();
    }
}
