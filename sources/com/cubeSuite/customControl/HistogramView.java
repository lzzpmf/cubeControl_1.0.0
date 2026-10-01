package com.cubeSuite.customControl;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;
import android.util.AttributeSet;
import android.view.View;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class HistogramView extends View {
    private final int DEFAULT_BLOCK_COLOR;
    private final int DEFAULT_TEXT_COLOR;
    private int blockColor;
    private boolean isEnable;
    private final RectF mBorder;
    private float mMax;
    private Paint mPaint;
    private final RectF mReachedRectF;
    private float mTextSize;
    private Orientation orientation;
    private int progress;

    private enum Orientation {
        Horizontal,
        Vertical
    }

    public void setOrientation(Orientation orientation) {
        this.orientation = orientation;
    }

    public HistogramView(Context context) {
        this(context, null);
    }

    public HistogramView(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public HistogramView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mMax = 15.0f;
        this.DEFAULT_BLOCK_COLOR = getResources().getColor(R.color.dominantHue);
        this.DEFAULT_TEXT_COLOR = getResources().getColor(R.color.accent);
        this.progress = 12;
        this.mReachedRectF = new RectF(0.0f, 0.0f, 0.0f, 0.0f);
        this.mBorder = new RectF(0.0f, 0.0f, 0.0f, 0.0f);
        this.isEnable = false;
        this.orientation = Orientation.Horizontal;
        initAttrs(context, attributeSet);
        initPainters();
    }

    private void initAttrs(Context context, AttributeSet attributeSet) {
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.BlockProgressBar);
        this.mMax = typedArrayObtainStyledAttributes.getInteger(2, (int) this.mMax);
        this.blockColor = typedArrayObtainStyledAttributes.getColor(1, this.DEFAULT_BLOCK_COLOR);
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
        super.onDraw(canvas);
        calculateDrawRectSize();
        this.mPaint.setStyle(Paint.Style.FILL_AND_STROKE);
        this.mPaint.setColor(this.blockColor);
        canvas.drawRect(this.mReachedRectF, this.mPaint);
        this.mPaint.setColor(getResources().getColor(R.color.accent));
        this.mPaint.setStrokeWidth(4.0f);
        this.mPaint.setStyle(Paint.Style.STROKE);
        canvas.drawRect(this.mBorder, this.mPaint);
    }

    private void calculateDrawRectSize() {
        this.mReachedRectF.left = getPaddingLeft();
        this.mReachedRectF.bottom = getHeight();
        int iOrdinal = this.orientation.ordinal();
        if (iOrdinal == 0) {
            this.mReachedRectF.right = ((getWidth() - getPaddingRight()) / this.mMax) * this.progress;
            this.mBorder.right = getWidth() - getPaddingRight();
            this.mReachedRectF.top = 0.0f;
        } else if (iOrdinal == 1) {
            this.mReachedRectF.right = getWidth() - getPaddingRight();
            RectF rectF = this.mReachedRectF;
            float height = getHeight();
            float f = this.mMax;
            rectF.top = (height / f) * (f - this.progress);
            this.mBorder.top = getHeight();
        }
        this.mBorder.left = this.mReachedRectF.left;
        this.mBorder.bottom = this.mReachedRectF.bottom;
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
