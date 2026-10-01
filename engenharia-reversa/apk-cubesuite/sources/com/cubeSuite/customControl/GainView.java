package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PointF;
import android.util.AttributeSet;
import android.view.View;
import com.cubeSuite.R;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class GainView extends View {
    private static final String TAG = "GainView";
    private int[] mGains;
    private Paint mPain;

    public GainView(Context context) {
        this(context, null);
    }

    public GainView(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public GainView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mGains = new int[]{0, 0, 0, 0, 0, 0, 0, 0};
        this.mPain = new Paint();
        Paint paint = new Paint(1);
        this.mPain = paint;
        paint.setAntiAlias(true);
        this.mPain.setColor(getResources().getColor(R.color.accent));
        this.mPain.setStrokeCap(Paint.Cap.ROUND);
        this.mPain.setStyle(Paint.Style.STROKE);
        this.mPain.setTextAlign(Paint.Align.RIGHT);
        this.mPain.setStrokeWidth(2.0f);
    }

    public void updateGain(int i, int i2) {
        this.mGains[i] = i2;
        postInvalidate();
    }

    private int[] getXIndex() {
        int[] iArr = new int[8];
        int width = ((getWidth() - getPaddingLeft()) - getPaddingRight()) / 7;
        for (int i = 0; i < 8; i++) {
            iArr[i] = width * i;
        }
        return iArr;
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        int[] iArr = new int[this.mGains.length];
        int i = 0;
        while (true) {
            int[] iArr2 = this.mGains;
            if (i >= iArr2.length) {
                break;
            }
            iArr[i] = -iArr2[i];
            i++;
        }
        int[] xIndex = getXIndex();
        int length = xIndex.length;
        int[] iArr3 = new int[length];
        int height = ((getHeight() - getPaddingTop()) - getPaddingBottom()) / 2;
        for (int i2 = 0; i2 < length; i2++) {
            iArr3[i2] = ((iArr[i2] * height) / 12) + height;
        }
        double d = Double.MAX_VALUE;
        double d2 = Double.MIN_VALUE;
        for (double d3 : xIndex) {
            if (d3 > d2) {
                d2 = d3;
            }
            if (d3 < d) {
                d = d3;
            }
        }
        int iSave = canvas.save();
        canvas.translate(getPaddingLeft(), getPaddingTop());
        Paint paint = this.mPain;
        float f = height;
        canvas.drawLine(0.0f, f, (getWidth() - getPaddingRight()) - getPaddingLeft(), f, paint);
        int length2 = xIndex.length;
        PointF[] pointFArr = new PointF[length2];
        for (int i3 = 0; i3 < length2; i3++) {
            PointF pointF = new PointF();
            pointFArr[i3] = pointF;
            pointF.x = xIndex[i3];
            pointFArr[i3].y = iArr3[i3];
        }
        List<PointF> controlPoints = getControlPoints(pointFArr);
        paint.setStrokeWidth(3.0f);
        Path path = new Path();
        createCurve2(controlPoints, pointFArr, path);
        canvas.drawPath(path, paint);
        canvas.restoreToCount(iSave);
    }

    void createCurve(PointF[] pointFArr, Path path) {
        float f;
        int length = pointFArr.length;
        PointF[] pointFArr2 = new PointF[length];
        int i = 0;
        while (true) {
            f = 2.0f;
            if (i >= length) {
                break;
            }
            int i2 = i + 1;
            int i3 = i2 % length;
            PointF pointF = new PointF();
            pointFArr2[i] = pointF;
            if (i == length - 1) {
                pointF.x = pointFArr[i].x;
                pointFArr2[i].y = (pointFArr[i].y + (((getHeight() - getPaddingTop()) - getPaddingBottom()) / 2.0f)) / 2.0f;
            } else {
                pointF.x = (pointFArr[i].x + pointFArr[i3].x) / 2.0f;
                pointFArr2[i].y = (pointFArr[i].y + pointFArr[i3].y) / 2.0f;
            }
            i = i2;
        }
        int i4 = length * 2;
        PointF[] pointFArr3 = new PointF[i4];
        int i5 = 0;
        while (i5 < length) {
            int i6 = ((i5 + length) - 1) % length;
            if (i5 == 0) {
                i6 = 0;
            }
            PointF pointF2 = new PointF();
            pointF2.x = (pointFArr2[i5].x + pointFArr2[i6].x) / f;
            pointF2.y = (pointFArr2[i5].y + pointFArr2[i6].y) / f;
            float f2 = pointFArr[i5].x - pointF2.x;
            float f3 = pointFArr[i5].y - pointF2.y;
            int i7 = i5 * 2;
            PointF pointF3 = new PointF();
            pointFArr3[i7] = pointF3;
            pointF3.x = pointFArr2[i6].x + f2;
            pointFArr3[i7].y = pointFArr2[i6].y + f3;
            float f4 = (pointFArr3[i7].x - pointFArr[i5].x) * 0.6f;
            float f5 = (pointFArr3[i7].y - pointFArr[i5].y) * 0.6f;
            pointFArr3[i7].x = pointFArr[i5].x + f4;
            pointFArr3[i7].y = pointFArr[i5].y + f5;
            int i8 = (i7 + 1) % i4;
            PointF pointF4 = new PointF();
            pointFArr3[i8] = pointF4;
            pointF4.x = pointFArr2[i5].x + f2;
            pointFArr3[i8].y = pointFArr2[i5].y + f3;
            float f6 = (pointFArr3[i8].x - pointFArr[i5].x) * 0.6f;
            float f7 = (pointFArr3[i8].y - pointFArr[i5].y) * 0.6f;
            pointFArr3[i8].x = pointFArr[i5].x + f6;
            pointFArr3[i8].y = pointFArr[i5].y + f7;
            i5++;
            f = 2.0f;
        }
        PointF[] pointFArr4 = new PointF[4];
        int i9 = 0;
        boolean z = true;
        while (i9 < length) {
            pointFArr4[0] = pointFArr[i9];
            int i10 = i9 * 2;
            PointF pointF5 = pointFArr3[i10 + 1];
            pointFArr4[1] = pointF5;
            if (pointF5.x < pointFArr[i9].x) {
                pointFArr4[1] = pointFArr[i9];
            }
            PointF pointF6 = pointFArr3[(i10 + 2) % i4];
            pointFArr4[2] = pointF6;
            if (pointF6.x < pointFArr[i9].x) {
                pointFArr4[2] = pointFArr[i9];
            }
            int i11 = i9 + 1;
            PointF pointF7 = pointFArr[i11 % length];
            pointFArr4[3] = pointF7;
            if (pointF7.x < pointFArr[i9].x) {
                pointFArr4[3] = pointFArr[i9];
            }
            float f8 = 1.0f;
            while (f8 >= 0.0f) {
                float fBezier3funcX = bezier3funcX(f8, pointFArr4);
                float fBezier3funcY = bezier3funcY(f8, pointFArr4);
                f8 = (float) (((double) f8) - 0.005d);
                if (z) {
                    path.moveTo(fBezier3funcX, fBezier3funcY);
                    z = false;
                } else {
                    path.lineTo(fBezier3funcX, fBezier3funcY);
                }
            }
            i9 = i11;
        }
    }

    private List<PointF> getControlPoints(PointF[] pointFArr) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(new PointF(pointFArr[0].x + ((pointFArr[1].x - pointFArr[0].x) * 0.3f), pointFArr[0].y));
        int i = 1;
        while (i < pointFArr.length - 1) {
            int i2 = i + 1;
            int i3 = i - 1;
            float f = (pointFArr[i2].y - pointFArr[i3].y) / (pointFArr[i2].x - pointFArr[i3].x);
            float f2 = pointFArr[i].y - (pointFArr[i].x * f);
            float f3 = pointFArr[i].x - ((pointFArr[i].x - pointFArr[i3].x) * 0.3f);
            arrayList.add(new PointF(f3, (f * f3) + f2));
            float f4 = pointFArr[i].x + ((pointFArr[i2].x - pointFArr[i].x) * 0.3f);
            arrayList.add(new PointF(f4, (f * f4) + f2));
            i = i2;
        }
        arrayList.add(new PointF(pointFArr[pointFArr.length - 1].x - ((pointFArr[pointFArr.length - 1].x - pointFArr[pointFArr.length - 2].x) * 0.3f), pointFArr[pointFArr.length - 1].y));
        return arrayList;
    }

    void createCurve2(List<PointF> list, PointF[] pointFArr, Path path) {
        int i = 0;
        while (i < pointFArr.length - 1) {
            path.moveTo(pointFArr[i].x, pointFArr[i].y);
            int i2 = i * 2;
            float f = list.get(i2).x;
            float f2 = list.get(i2).y;
            int i3 = i2 + 1;
            i++;
            path.cubicTo(f, f2, list.get(i3).x, list.get(i3).y, pointFArr[i].x, pointFArr[i].y);
        }
    }

    float bezier3funcX(float f, PointF[] pointFArr) {
        float f2 = pointFArr[0].x * f * f * f;
        float f3 = 1.0f - f;
        float f4 = pointFArr[1].x * 3.0f * f * f * f3;
        return f2 + f4 + (pointFArr[2].x * 3.0f * f * f3 * f3) + (pointFArr[3].x * f3 * f3 * f3);
    }

    float bezier3funcY(float f, PointF[] pointFArr) {
        float f2 = pointFArr[0].y * f * f * f;
        float f3 = 1.0f - f;
        float f4 = pointFArr[1].y * 3.0f * f * f * f3;
        return f2 + f4 + (pointFArr[2].y * 3.0f * f * f3 * f3) + (pointFArr[3].y * f3 * f3 * f3);
    }
}
