package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.drawable.BitmapDrawable;
import android.util.AttributeSet;
import android.util.DisplayMetrics;
import android.view.MotionEvent;
import android.view.View;
import com.cubeSuite.R;
import com.cubeSuite.utils.AppUtil;

/* JADX INFO: loaded from: classes.dex */
public class XyzSelect extends View {
    private int m_blockSize;
    private Paint m_boldLinePaint;
    private int m_height;
    private Paint m_linePaint;
    private Bitmap m_microphone;
    private int m_width;
    private int m_x;
    private int m_xNum;
    private float m_xyLeft;
    private float m_xyTop;
    private int m_xyWidth;
    private int m_xyzEndX;
    private int m_xyzEndY;
    private XYZListening m_xyzListening;
    private int m_y;
    private int m_yNum;
    private int m_z;
    private int m_zNum;
    private int m_zPosX;
    private float m_zTop;

    public interface XYZListening {
        void xChange(int i);

        void yChange(int i);

        void zChange(int i);
    }

    public XyzSelect(Context context) {
        this(context, null);
    }

    public XyzSelect(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public XyzSelect(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.m_width = 0;
        this.m_height = 0;
        this.m_xyWidth = 0;
        this.m_zPosX = 0;
        this.m_xyLeft = 0.0f;
        this.m_xyTop = 0.0f;
        this.m_zTop = 0.0f;
        this.m_xNum = 13;
        this.m_yNum = 13;
        this.m_zNum = 13;
        this.m_blockSize = 0;
        this.m_microphone = ((BitmapDrawable) getResources().getDrawable(R.drawable.microphone)).getBitmap();
        this.m_x = 5;
        this.m_y = 6;
        this.m_z = 13;
        this.m_xyzEndX = 0;
        this.m_xyzEndY = 0;
        init();
    }

    @Override // android.view.View
    protected void onSizeChanged(int i, int i2, int i3, int i4) {
        super.onSizeChanged(i, i2, i3, i4);
        this.m_width = i;
        int i5 = (int) (((double) i) * 0.7d);
        this.m_height = i5;
        this.m_xyWidth = i5;
        this.m_zPosX = i - ((i - i5) / 2);
        int iMax = i5 / Math.max(Math.max(this.m_xNum, this.m_yNum), this.m_zNum);
        this.m_blockSize = iMax;
        this.m_microphone = AppUtil.getScaleBitmap(this.m_microphone, iMax, iMax);
        int i6 = this.m_blockSize;
        this.m_xyzEndX = this.m_xNum * i6;
        this.m_xyzEndY = this.m_yNum * i6;
        this.m_xyLeft = this.m_x * i6;
        this.m_xyTop = this.m_y * i6;
        this.m_zTop = this.m_z * i6;
        updateView();
    }

    public void setXyzListening(XYZListening xYZListening) {
        this.m_xyzListening = xYZListening;
    }

    public void setXyz(int i, int i2, int i3) {
        this.m_x = i;
        this.m_y = i2;
        this.m_z = i3;
        updateView();
    }

    public void updateView() {
        invalidate();
    }

    public void getViewSize() {
        new DisplayMetrics();
        getResources().getDisplayMetrics();
    }

    private void init() {
        Paint paint = new Paint();
        this.m_linePaint = paint;
        paint.setAntiAlias(true);
        this.m_linePaint.setStrokeCap(Paint.Cap.ROUND);
        this.m_linePaint.setTextAlign(Paint.Align.RIGHT);
        this.m_linePaint.setColor(Color.argb(100, 51, 234, 184));
        this.m_linePaint.setStyle(Paint.Style.STROKE);
        this.m_linePaint.setStrokeWidth(1.0f);
        Paint paint2 = new Paint();
        this.m_boldLinePaint = paint2;
        paint2.setAntiAlias(true);
        this.m_boldLinePaint.setStrokeCap(Paint.Cap.ROUND);
        this.m_boldLinePaint.setTextAlign(Paint.Align.RIGHT);
        this.m_boldLinePaint.setColor(Color.rgb(51, 234, 184));
        this.m_boldLinePaint.setStyle(Paint.Style.STROKE);
        this.m_boldLinePaint.setStrokeWidth(3.0f);
        getResources().getDisplayMetrics();
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        int i;
        float x = (int) motionEvent.getX();
        float y = (int) motionEvent.getY();
        if (motionEvent.getActionMasked() != 0 || this.m_xyzListening == null) {
            return true;
        }
        if (AppUtil.RangeCheck2D(x, y, 0, this.m_xyzEndX, 0, this.m_xyzEndY)) {
            int i2 = this.m_blockSize;
            int i3 = (int) (x / i2);
            int i4 = (int) (y / i2);
            if (this.m_x != i3) {
                this.m_x = i3;
                this.m_xyzListening.xChange(i3);
            }
            if (this.m_y != i4) {
                this.m_y = i4;
                this.m_xyzListening.yChange(i4);
            }
            updateView();
        }
        int i5 = this.m_zPosX;
        if (AppUtil.RangeCheck2D(x, y, i5, i5 + this.m_blockSize, 0, this.m_xyzEndY) && this.m_z != (i = (int) (y / this.m_blockSize))) {
            this.m_z = i;
            this.m_xyzListening.zChange(i);
            updateView();
        }
        return true;
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        Path path = new Path();
        for (int i = 1; i < this.m_xNum; i++) {
            path.moveTo(this.m_blockSize * i, 0.0f);
            path.lineTo(this.m_blockSize * i, this.m_xyzEndY);
        }
        for (int i2 = 1; i2 < this.m_yNum; i2++) {
            path.moveTo(0.0f, this.m_blockSize * i2);
            path.lineTo(this.m_xyzEndX, this.m_blockSize * i2);
        }
        for (int i3 = 1; i3 < this.m_zNum; i3++) {
            path.moveTo(this.m_zPosX, this.m_blockSize * i3);
            int i4 = this.m_zPosX;
            int i5 = this.m_blockSize;
            path.lineTo(i4 + i5, i5 * i3);
        }
        int i6 = this.m_x;
        int i7 = this.m_blockSize;
        this.m_xyLeft = i6 * i7;
        this.m_xyTop = this.m_y * i7;
        this.m_zTop = this.m_z * i7;
        canvas.drawPath(path, this.m_linePaint);
        int i8 = this.m_blockSize;
        canvas.drawRect(0.0f, 0.0f, this.m_xNum * i8, i8 * this.m_yNum, this.m_boldLinePaint);
        int i9 = this.m_zPosX;
        int i10 = this.m_blockSize;
        canvas.drawRect(i9, 0.0f, i9 + i10, i10 * this.m_yNum, this.m_boldLinePaint);
        canvas.drawBitmap(this.m_microphone, this.m_xyLeft, this.m_xyTop, this.m_boldLinePaint);
        canvas.drawBitmap(this.m_microphone, this.m_zPosX, this.m_zTop, this.m_boldLinePaint);
    }
}
