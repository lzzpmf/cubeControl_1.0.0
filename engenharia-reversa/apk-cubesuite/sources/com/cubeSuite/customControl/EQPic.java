package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.Point;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import com.cubeSuite.entity.irbox.IRBoxEntry;
import com.cubeSuite.utils.EqLine.EqLine;
import com.google.android.material.card.MaterialCardViewHelper;
import es.voghdev.pdfviewpager.library.subscaleview.SubsamplingScaleImageViewConstants;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class EQPic extends View {
    private final String[] DB_TEXT;
    private final String[] HZ_TEXT;
    private final int M_HEIGHT;
    private final int M_HEIGHT_2;
    private final int M_HEIGHT_60DB;
    private final int PEAK_TEXT_TOP_PADDING;
    private final int PX_RATE_POINT;
    private final float R;
    private final int TEXT_PX;
    private EqLine eqLine;
    private List<Float> eqLineY;
    EQPicHzDbListener eqPicHzDbListener;
    private Paint grayPaint;
    private IRBoxEntry.IRBoxEq irBoxEq;
    private int mFs;
    private Paint mPaint1;
    private Paint mPaint2;
    private int mPeakBandNum;
    private Paint mWhitePaint;
    private int m_currentPeak;
    private Point[] m_dbTextPoint;
    String[] m_eqPeakName;
    private Point[] m_eqPeakPoint;
    private Point[] m_hzTextPoint;
    private boolean m_isMovePeak;
    private int m_maxDb;
    private int m_maxHz;
    private int m_minDb;
    private int m_minHz;
    private float m_peakDiffX;
    private float m_peakDiffY;
    private int m_pointNum;
    private int m_width;

    public interface EQPicHzDbListener {
        void endMove(int i, int i2, int i3);

        void move(int i, int i2, int i3);

        void startMove(int i, int i2, int i3);
    }

    public int dbToY(int i) {
        return 150 - i;
    }

    public int yToDb(int i) {
        return 150 - i;
    }

    public EQPic(Context context) {
        this(context, null);
    }

    public EQPic(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public EQPic(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.DB_TEXT = new String[]{"12", "6", "0", "-6", "-12"};
        this.R = 30.0f;
        this.m_minHz = 30;
        this.m_maxHz = 18000;
        this.m_minDb = -120;
        this.m_maxDb = 120;
        this.PX_RATE_POINT = 10;
        this.mPeakBandNum = 5;
        this.mFs = 48000;
        this.m_pointNum = 0;
        this.TEXT_PX = 30;
        this.PEAK_TEXT_TOP_PADDING = 15;
        this.eqLine = new EqLine();
        this.eqLineY = new ArrayList();
        this.m_hzTextPoint = new Point[3];
        this.HZ_TEXT = new String[]{"100", "1K", "10K"};
        this.m_dbTextPoint = new Point[5];
        this.m_width = 0;
        this.M_HEIGHT = MaterialCardViewHelper.DEFAULT_FADE_ANIM_DURATION;
        this.M_HEIGHT_2 = 150;
        this.M_HEIGHT_60DB = 60;
        this.m_peakDiffX = 0.0f;
        this.m_peakDiffY = 0.0f;
        this.m_currentPeak = -1;
        this.m_isMovePeak = false;
        this.m_eqPeakName = new String[]{"HP", "LS", "P1", "P2", "P3", "P4", "P5", "HS", "LP"};
        int i2 = getResources().getDisplayMetrics().widthPixels;
        this.m_width = i2;
        this.m_pointNum = i2 / 10;
        initPaint();
        initLine();
    }

    public void setEqPicHzDbListener(EQPicHzDbListener eQPicHzDbListener) {
        this.eqPicHzDbListener = eQPicHzDbListener;
    }

    public void setData(IRBoxEntry.IRBoxEq iRBoxEq) {
        this.irBoxEq = iRBoxEq;
        updateView();
    }

    private void initLine() {
        this.m_eqPeakPoint = new Point[this.mPeakBandNum + 4];
        int i = 0;
        int i2 = 0;
        while (true) {
            Point[] pointArr = this.m_eqPeakPoint;
            if (i2 >= pointArr.length) {
                break;
            }
            pointArr[i2] = new Point();
            i2++;
        }
        this.eqLine.eq_resp_init(this.mPeakBandNum, this.m_pointNum, this.mFs);
        this.eqLine.eq_resp_config();
        for (int i3 = 0; i3 < this.eqLine.m_er.respf.size(); i3++) {
            this.eqLineY.add(Float.valueOf((float) (this.eqLine.m_er.respf.get(i3).doubleValue() * 10.0d)));
        }
        int i4 = 100;
        int i5 = 0;
        while (true) {
            Point[] pointArr2 = this.m_hzTextPoint;
            if (i5 >= pointArr2.length) {
                break;
            }
            pointArr2[i5] = new Point(getPxByHz(i4), SubsamplingScaleImageViewConstants.ORIENTATION_270);
            i4 *= 10;
            i5++;
        }
        while (true) {
            Point[] pointArr3 = this.m_dbTextPoint;
            if (i >= pointArr3.length) {
                return;
            }
            pointArr3[i] = new Point(this.m_width - 60, (i * 60) + 30);
            i++;
        }
    }

    private void setPeakPoint() {
        int pxByHz;
        int iDbToY;
        for (int i = 0; i < this.m_eqPeakPoint.length; i++) {
            if (i == 0) {
                pxByHz = getPxByHz(this.irBoxEq.hpHz.getData());
                iDbToY = dbToY(0);
            } else if (i == 1) {
                pxByHz = getPxByHz(this.irBoxEq.lsHz.getData());
                iDbToY = dbToY(this.irBoxEq.lsDb.getData());
            } else {
                int i2 = this.mPeakBandNum;
                if (i == i2 + 2) {
                    pxByHz = getPxByHz(this.irBoxEq.hsHz.getData());
                    iDbToY = dbToY(this.irBoxEq.hsDb.getData());
                } else if (i == i2 + 3) {
                    pxByHz = getPxByHz(this.irBoxEq.lpHz.getData());
                    iDbToY = dbToY(0);
                } else {
                    int i3 = i - 2;
                    pxByHz = getPxByHz(this.irBoxEq.peakHz[i3].getData());
                    iDbToY = dbToY(this.irBoxEq.peakDb[i3].getData());
                }
            }
            this.m_eqPeakPoint[i].set(pxByHz, iDbToY);
        }
    }

    private void setLine() {
        this.eqLine.m_er.para.hphz = this.irBoxEq.hpHz.getData();
        this.eqLine.m_er.para.lshz = this.irBoxEq.lsHz.getData();
        this.eqLine.m_er.para.lsdb = this.irBoxEq.lsDb.getData();
        this.eqLine.m_er.para.hshz = this.irBoxEq.hsHz.getData();
        this.eqLine.m_er.para.hsdb = this.irBoxEq.hsDb.getData();
        this.eqLine.m_er.para.lphz = this.irBoxEq.lpHz.getData();
        for (int i = 0; i < this.eqLine.m_er.para.enable.size(); i++) {
            this.eqLine.m_er.para.enable.set(i, Integer.valueOf(this.irBoxEq.enable[i].getData()));
        }
        for (int i2 = 0; i2 < this.eqLine.m_er.para.peakhz.size(); i2++) {
            this.eqLine.m_er.para.peakhz.set(i2, Integer.valueOf(this.irBoxEq.peakHz[i2].getData()));
            this.eqLine.m_er.para.peakqdb.set(i2, Integer.valueOf(this.irBoxEq.peakDb[i2].getData()));
            this.eqLine.m_er.para.peakq.set(i2, Integer.valueOf(this.irBoxEq.peakQ[i2].getData()));
        }
        this.eqLine.eq_resp_config();
        for (int i3 = 0; i3 < this.eqLine.m_er.respf.size(); i3++) {
            this.eqLineY.set(i3, Float.valueOf(Math.min(Math.max((float) (150.0d - (this.eqLine.m_er.respf.get(i3).doubleValue() * 10.0d)), 0.0f), 300.0f)));
        }
        invalidate();
    }

    public void updateView() {
        setLine();
        setPeakPoint();
        invalidate();
    }

    private void initPaint() {
        Paint paint = new Paint(1);
        this.mPaint1 = paint;
        paint.setAntiAlias(true);
        this.mPaint1.setStrokeCap(Paint.Cap.ROUND);
        this.mPaint1.setTextSize(30.0f);
        this.mPaint1.setColor(Color.rgb(51, 234, 184));
        this.mPaint1.setStyle(Paint.Style.STROKE);
        this.mPaint1.setStrokeWidth(1.0f);
        this.mPaint1.setTextAlign(Paint.Align.CENTER);
        Paint paint2 = new Paint(1);
        this.mPaint2 = paint2;
        paint2.setAntiAlias(true);
        this.mPaint2.setStrokeCap(Paint.Cap.ROUND);
        this.mPaint2.setColor(Color.rgb(51, 234, 184));
        this.mPaint2.setStyle(Paint.Style.STROKE);
        this.mPaint2.setStrokeWidth(3.0f);
        Paint paint3 = new Paint();
        this.mWhitePaint = paint3;
        paint3.setAntiAlias(true);
        this.mWhitePaint.setColor(Color.rgb(255, 255, 255));
        this.mWhitePaint.setStyle(Paint.Style.STROKE);
        this.mWhitePaint.setTextSize(30.0f);
        this.mWhitePaint.setStrokeWidth(3.0f);
        this.mWhitePaint.setTextAlign(Paint.Align.CENTER);
        Paint paint4 = new Paint();
        this.grayPaint = paint4;
        paint4.setAntiAlias(true);
        this.grayPaint.setColor(Color.rgb(128, 121, 123));
        this.grayPaint.setStyle(Paint.Style.STROKE);
        this.grayPaint.setTextSize(30.0f);
        this.grayPaint.setStrokeWidth(3.0f);
        this.grayPaint.setTextAlign(Paint.Align.CENTER);
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        if (this.irBoxEq == null) {
            return;
        }
        Path path = new Path();
        path.moveTo(0.0f, this.eqLineY.get(0).floatValue());
        for (int i = 0; i < this.m_pointNum; i++) {
            path.lineTo(i * 10, this.eqLineY.get(i).floatValue());
        }
        path.moveTo(0.0f, 0.0f);
        path.lineTo(this.m_width, 0.0f);
        path.lineTo(this.m_width, 300.0f);
        path.lineTo(0.0f, 300.0f);
        path.lineTo(0.0f, 0.0f);
        canvas.drawPath(path, this.mPaint2);
        Path path2 = new Path();
        int i2 = 0;
        while (true) {
            Point[] pointArr = this.m_dbTextPoint;
            if (i2 >= pointArr.length) {
                break;
            }
            path2.moveTo(pointArr[i2].x, this.m_dbTextPoint[i2].y);
            path2.lineTo(0.0f, this.m_dbTextPoint[i2].y);
            canvas.drawPath(path2, this.mPaint1);
            canvas.drawText(this.DB_TEXT[i2], this.m_dbTextPoint[i2].x + 30, this.m_dbTextPoint[i2].y, this.mPaint1);
            i2++;
        }
        canvas.drawText("20", 30.0f, 295.0f, this.mPaint1);
        canvas.drawText("20k", this.m_width - 30, 295.0f, this.mPaint1);
        int i3 = 0;
        while (true) {
            Point[] pointArr2 = this.m_hzTextPoint;
            if (i3 >= pointArr2.length) {
                break;
            }
            path2.moveTo(pointArr2[i3].x, this.m_hzTextPoint[i3].y);
            path2.lineTo(this.m_hzTextPoint[i3].x, 0.0f);
            canvas.drawPath(path2, this.mPaint1);
            canvas.drawText(this.HZ_TEXT[i3], this.m_hzTextPoint[i3].x, this.m_hzTextPoint[i3].y + 30, this.mPaint1);
            i3++;
        }
        for (int i4 = 0; i4 < this.m_eqPeakPoint.length; i4++) {
            if (this.irBoxEq.enable[i4].getData() == 0) {
                canvas.drawCircle(this.m_eqPeakPoint[i4].x, this.m_eqPeakPoint[i4].y, 30.0f, this.grayPaint);
                canvas.drawText(this.m_eqPeakName[i4], this.m_eqPeakPoint[i4].x, this.m_eqPeakPoint[i4].y + 15, this.grayPaint);
            } else {
                canvas.drawCircle(this.m_eqPeakPoint[i4].x, this.m_eqPeakPoint[i4].y, 30.0f, this.mWhitePaint);
                canvas.drawText(this.m_eqPeakName[i4], this.m_eqPeakPoint[i4].x, this.m_eqPeakPoint[i4].y + 15, this.mWhitePaint);
            }
        }
    }

    public int getPxByHz(int i) {
        return (int) Math.round((Math.log(i) - Math.log(20.0d)) / Math.log(Math.pow(1000.0d, 1.0d / ((double) this.m_width))));
    }

    public double getHzByPx(int i) {
        double dPow = Math.pow(Math.pow(1000.0d, 1.0f / this.m_width), i) * 20.0d;
        int i2 = this.m_minHz;
        if (dPow < i2) {
            dPow = i2;
        }
        int i3 = this.m_maxHz;
        return dPow > ((double) i3) ? i3 : dPow;
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        int actionMasked = motionEvent.getActionMasked();
        float x = motionEvent.getX();
        float y = motionEvent.getY();
        int i = 0;
        if (actionMasked == 0) {
            while (true) {
                Point[] pointArr = this.m_eqPeakPoint;
                if (i >= pointArr.length) {
                    break;
                }
                int i2 = pointArr[i].x;
                int i3 = this.m_eqPeakPoint[i].y;
                float f = i2;
                if (x < f + 30.0f && x > f - 30.0f) {
                    float f2 = i3;
                    if (y < f2 + 30.0f && y > f2 - 30.0f) {
                        this.m_peakDiffX = f - x;
                        this.m_peakDiffY = f2 - y;
                        this.m_isMovePeak = true;
                        this.m_currentPeak = i;
                        this.eqPicHzDbListener.startMove(i, -1, -1);
                        break;
                    }
                }
                i++;
            }
        } else if (actionMasked != 1) {
            if (actionMasked == 2 && this.m_isMovePeak) {
                int iMax = (int) Math.max(getPxByHz(this.m_minHz), Math.min(getPxByHz(this.m_maxHz), x + this.m_peakDiffX));
                int i4 = this.m_currentPeak;
                int iMin = (i4 == 0 || i4 == this.m_eqPeakPoint.length - 1) ? 150 : (int) Math.min(dbToY(this.m_minDb), Math.max(dbToY(this.m_maxDb), y + this.m_peakDiffY));
                this.m_eqPeakPoint[this.m_currentPeak].set(iMax, iMin);
                int i5 = this.m_currentPeak;
                if (i5 == 0) {
                    this.irBoxEq.hpHz.setData((int) getHzByPx(iMax));
                    this.eqPicHzDbListener.move(this.m_currentPeak, this.irBoxEq.hpHz.getData(), -1);
                } else if (i5 == 1) {
                    this.irBoxEq.lsHz.setData((int) getHzByPx(iMax));
                    this.irBoxEq.lsDb.setData((byte) yToDb(iMin));
                    this.eqPicHzDbListener.move(this.m_currentPeak, this.irBoxEq.lsHz.getData(), this.irBoxEq.lsDb.getData());
                } else if (i5 == 7) {
                    this.irBoxEq.hsHz.setData((int) getHzByPx(iMax));
                    this.irBoxEq.hsDb.setData((byte) yToDb(iMin));
                    this.eqPicHzDbListener.move(this.m_currentPeak, this.irBoxEq.hsHz.getData(), this.irBoxEq.hsDb.getData());
                } else if (i5 == 8) {
                    this.irBoxEq.lpHz.setData((int) getHzByPx(iMax));
                    this.eqPicHzDbListener.move(this.m_currentPeak, this.irBoxEq.lpHz.getData(), -1);
                } else {
                    this.irBoxEq.peakHz[this.m_currentPeak - 2].setData((int) getHzByPx(iMax));
                    this.irBoxEq.peakDb[this.m_currentPeak - 2].setData((byte) yToDb(iMin));
                    this.eqPicHzDbListener.move(this.m_currentPeak, this.irBoxEq.peakHz[this.m_currentPeak - 2].getData(), this.irBoxEq.peakDb[this.m_currentPeak - 2].getData());
                }
                setLine();
            }
        } else if (this.m_isMovePeak) {
            this.m_currentPeak = -1;
            this.m_isMovePeak = false;
            this.eqPicHzDbListener.endMove(-1, -1, -1);
        }
        return true;
    }
}
