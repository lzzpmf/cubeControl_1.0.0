package com.contrarywind.view;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.Typeface;
import android.os.Handler;
import android.text.TextUtils;
import android.util.AttributeSet;
import android.util.Log;
import android.view.GestureDetector;
import android.view.MotionEvent;
import android.view.View;
import com.contrarywind.adapter.WheelAdapter;
import com.contrarywind.interfaces.IPickerViewData;
import com.contrarywind.listener.LoopViewGestureListener;
import com.contrarywind.listener.OnItemSelectedListener;
import com.contrarywind.timer.InertiaTimerTask;
import com.contrarywind.timer.MessageHandler;
import com.contrarywind.timer.SmoothScrollTimerTask;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;

/* JADX INFO: loaded from: classes.dex */
public class WheelView extends View {
    private static final float SCALE_CONTENT = 0.8f;
    private static final String[] TIME_NUM = {"00", "01", "02", "03", "04", "05", "06", "07", "08", "09"};
    private static final int VELOCITY_FLING = 5;
    private float CENTER_CONTENT_OFFSET;
    private WheelAdapter adapter;
    private float centerY;
    private Context context;
    private int dividerColor;
    private DividerType dividerType;
    private int dividerWidth;
    private int drawCenterContentStart;
    private int drawOutContentStart;
    private float firstLineY;
    private GestureDetector gestureDetector;
    private Handler handler;
    private int initPosition;
    private boolean isAlphaGradient;
    private boolean isCenterLabel;
    private boolean isLoop;
    private boolean isOptions;
    private float itemHeight;
    private int itemsVisible;
    private String label;
    private float lineSpacingMultiplier;
    private ScheduledExecutorService mExecutor;
    private ScheduledFuture<?> mFuture;
    private int mGravity;
    private int mOffset;
    private int maxTextHeight;
    private int maxTextWidth;
    private int measuredHeight;
    private int measuredWidth;
    private OnItemSelectedListener onItemSelectedListener;
    private Paint paintCenterText;
    private Paint paintIndicator;
    private Paint paintOuterText;
    private int preCurrentIndex;
    private float previousY;
    private int radius;
    private float secondLineY;
    private int selectedItem;
    private long startTime;
    private int textColorCenter;
    private int textColorOut;
    private int textSize;
    private int textXOffset;
    private float totalScrollY;
    private Typeface typeface;
    private int widthMeasureSpec;

    public enum ACTION {
        CLICK,
        FLING,
        DAGGLE
    }

    public enum DividerType {
        FILL,
        WRAP,
        CIRCLE
    }

    public WheelView(Context context) {
        this(context, null);
    }

    public WheelView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.isOptions = false;
        this.isCenterLabel = true;
        this.mExecutor = Executors.newSingleThreadScheduledExecutor();
        this.typeface = Typeface.MONOSPACE;
        this.lineSpacingMultiplier = 1.6f;
        this.itemsVisible = 11;
        this.mOffset = 0;
        this.previousY = 0.0f;
        this.startTime = 0L;
        this.mGravity = 17;
        this.drawCenterContentStart = 0;
        this.drawOutContentStart = 0;
        this.isAlphaGradient = false;
        this.textSize = getResources().getDimensionPixelSize(R.dimen.pickerview_textsize);
        float f = getResources().getDisplayMetrics().density;
        if (f < 1.0f) {
            this.CENTER_CONTENT_OFFSET = 2.4f;
        } else if (1.0f <= f && f < 2.0f) {
            this.CENTER_CONTENT_OFFSET = 4.0f;
        } else if (2.0f <= f && f < 3.0f) {
            this.CENTER_CONTENT_OFFSET = 6.0f;
        } else if (f >= 3.0f) {
            this.CENTER_CONTENT_OFFSET = f * 2.5f;
        }
        if (attributeSet != null) {
            TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.pickerview, 0, 0);
            this.mGravity = typedArrayObtainStyledAttributes.getInt(R.styleable.pickerview_wheelview_gravity, 17);
            this.textColorOut = typedArrayObtainStyledAttributes.getColor(R.styleable.pickerview_wheelview_textColorOut, -5723992);
            this.textColorCenter = typedArrayObtainStyledAttributes.getColor(R.styleable.pickerview_wheelview_textColorCenter, -14013910);
            this.dividerColor = typedArrayObtainStyledAttributes.getColor(R.styleable.pickerview_wheelview_dividerColor, -2763307);
            this.dividerWidth = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.pickerview_wheelview_dividerWidth, 2);
            this.textSize = typedArrayObtainStyledAttributes.getDimensionPixelOffset(R.styleable.pickerview_wheelview_textSize, this.textSize);
            this.lineSpacingMultiplier = typedArrayObtainStyledAttributes.getFloat(R.styleable.pickerview_wheelview_lineSpacingMultiplier, this.lineSpacingMultiplier);
            typedArrayObtainStyledAttributes.recycle();
        }
        judgeLineSpace();
        initLoopView(context);
    }

    private void judgeLineSpace() {
        float f = this.lineSpacingMultiplier;
        if (f < 1.0f) {
            this.lineSpacingMultiplier = 1.0f;
        } else if (f > 4.0f) {
            this.lineSpacingMultiplier = 4.0f;
        }
    }

    private void initLoopView(Context context) {
        this.context = context;
        this.handler = new MessageHandler(this);
        GestureDetector gestureDetector = new GestureDetector(context, new LoopViewGestureListener(this));
        this.gestureDetector = gestureDetector;
        gestureDetector.setIsLongpressEnabled(false);
        this.isLoop = true;
        this.totalScrollY = 0.0f;
        this.initPosition = -1;
        initPaints();
    }

    private void initPaints() {
        Paint paint = new Paint();
        this.paintOuterText = paint;
        paint.setColor(this.textColorOut);
        this.paintOuterText.setAntiAlias(true);
        this.paintOuterText.setTypeface(this.typeface);
        this.paintOuterText.setTextSize(this.textSize);
        Paint paint2 = new Paint();
        this.paintCenterText = paint2;
        paint2.setColor(this.textColorCenter);
        this.paintCenterText.setAntiAlias(true);
        this.paintCenterText.setTextScaleX(1.1f);
        this.paintCenterText.setTypeface(this.typeface);
        this.paintCenterText.setTextSize(this.textSize);
        Paint paint3 = new Paint();
        this.paintIndicator = paint3;
        paint3.setColor(this.dividerColor);
        this.paintIndicator.setAntiAlias(true);
        setLayerType(1, null);
    }

    private void reMeasure() {
        if (this.adapter == null) {
            return;
        }
        measureTextWidthHeight();
        int i = (int) (this.itemHeight * (this.itemsVisible - 1));
        this.measuredHeight = (int) (((double) (i * 2)) / 3.141592653589793d);
        this.radius = (int) (((double) i) / 3.141592653589793d);
        this.measuredWidth = View.MeasureSpec.getSize(this.widthMeasureSpec);
        int i2 = this.measuredHeight;
        float f = this.itemHeight;
        this.firstLineY = (i2 - f) / 2.0f;
        float f2 = (i2 + f) / 2.0f;
        this.secondLineY = f2;
        this.centerY = (f2 - ((f - this.maxTextHeight) / 2.0f)) - this.CENTER_CONTENT_OFFSET;
        if (this.initPosition == -1) {
            if (this.isLoop) {
                this.initPosition = (this.adapter.getItemsCount() + 1) / 2;
            } else {
                this.initPosition = 0;
            }
        }
        this.preCurrentIndex = this.initPosition;
    }

    private void measureTextWidthHeight() {
        Rect rect = new Rect();
        for (int i = 0; i < this.adapter.getItemsCount(); i++) {
            String contentText = getContentText(this.adapter.getItem(i));
            this.paintCenterText.getTextBounds(contentText, 0, contentText.length(), rect);
            int iWidth = rect.width();
            if (iWidth > this.maxTextWidth) {
                this.maxTextWidth = iWidth;
            }
        }
        this.paintCenterText.getTextBounds("星期", 0, 2, rect);
        int iHeight = rect.height() + 2;
        this.maxTextHeight = iHeight;
        this.itemHeight = this.lineSpacingMultiplier * iHeight;
    }

    public void smoothScroll(ACTION action) {
        cancelFuture();
        if (action == ACTION.FLING || action == ACTION.DAGGLE) {
            float f = this.totalScrollY;
            float f2 = this.itemHeight;
            int i = (int) (((f % f2) + f2) % f2);
            this.mOffset = i;
            if (i > f2 / 2.0f) {
                this.mOffset = (int) (f2 - i);
            } else {
                this.mOffset = -i;
            }
        }
        this.mFuture = this.mExecutor.scheduleWithFixedDelay(new SmoothScrollTimerTask(this, this.mOffset), 0L, 10L, TimeUnit.MILLISECONDS);
    }

    public final void scrollBy(float f) {
        cancelFuture();
        this.mFuture = this.mExecutor.scheduleWithFixedDelay(new InertiaTimerTask(this, f), 0L, 5L, TimeUnit.MILLISECONDS);
    }

    public void cancelFuture() {
        ScheduledFuture<?> scheduledFuture = this.mFuture;
        if (scheduledFuture == null || scheduledFuture.isCancelled()) {
            return;
        }
        this.mFuture.cancel(true);
        this.mFuture = null;
    }

    public final void setCyclic(boolean z) {
        this.isLoop = z;
    }

    public final void setTypeface(Typeface typeface) {
        this.typeface = typeface;
        this.paintOuterText.setTypeface(typeface);
        this.paintCenterText.setTypeface(this.typeface);
    }

    public final void setTextSize(float f) {
        if (f > 0.0f) {
            int i = (int) (this.context.getResources().getDisplayMetrics().density * f);
            this.textSize = i;
            this.paintOuterText.setTextSize(i);
            this.paintCenterText.setTextSize(this.textSize);
        }
    }

    public final void setCurrentItem(int i) {
        this.selectedItem = i;
        this.initPosition = i;
        this.totalScrollY = 0.0f;
        invalidate();
    }

    public final void setOnItemSelectedListener(OnItemSelectedListener onItemSelectedListener) {
        this.onItemSelectedListener = onItemSelectedListener;
    }

    public final void setAdapter(WheelAdapter wheelAdapter) {
        this.adapter = wheelAdapter;
        reMeasure();
        invalidate();
    }

    public void setItemsVisibleCount(int i) {
        if (i % 2 == 0) {
            i++;
        }
        this.itemsVisible = i + 2;
    }

    public void setAlphaGradient(boolean z) {
        this.isAlphaGradient = z;
    }

    public final WheelAdapter getAdapter() {
        return this.adapter;
    }

    public final int getCurrentItem() {
        int i;
        WheelAdapter wheelAdapter = this.adapter;
        if (wheelAdapter == null) {
            return 0;
        }
        if (this.isLoop && ((i = this.selectedItem) < 0 || i >= wheelAdapter.getItemsCount())) {
            return Math.max(0, Math.min(Math.abs(Math.abs(this.selectedItem) - this.adapter.getItemsCount()), this.adapter.getItemsCount() - 1));
        }
        return Math.max(0, Math.min(this.selectedItem, this.adapter.getItemsCount() - 1));
    }

    public final void onItemSelected() {
        if (this.onItemSelectedListener != null) {
            postDelayed(new Runnable() { // from class: com.contrarywind.view.WheelView.1
                @Override // java.lang.Runnable
                public void run() {
                    WheelView.this.onItemSelectedListener.onItemSelected(WheelView.this.getCurrentItem());
                }
            }, 200L);
        }
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        float f;
        Canvas canvas2;
        float f2;
        int i;
        Object item;
        boolean z;
        String contentText;
        int i2;
        if (this.adapter == null) {
            return;
        }
        boolean z2 = false;
        int iMin = Math.min(Math.max(0, this.initPosition), this.adapter.getItemsCount() - 1);
        this.initPosition = iMin;
        try {
            this.preCurrentIndex = iMin + (((int) (this.totalScrollY / this.itemHeight)) % this.adapter.getItemsCount());
            while (true) {
                int i3 = this.itemsVisible;
                if (i >= i3) {
                    return;
                }
                int i4 = this.preCurrentIndex - ((i3 / 2) - i);
                if (this.isLoop) {
                    item = this.adapter.getItem(getLoopMappingIndex(i4));
                } else {
                    item = "";
                    if (i4 >= 0 && i4 <= this.adapter.getItemsCount() - 1) {
                        item = this.adapter.getItem(i4);
                    }
                }
                canvas2.save();
                double d = ((this.itemHeight * i) - f) / this.radius;
                float f3 = (float) (90.0d - ((d / 3.141592653589793d) * 180.0d));
                if (f3 > 90.0f || f3 < -90.0f) {
                    z = z2;
                    canvas2.restore();
                } else {
                    if (!this.isCenterLabel && !TextUtils.isEmpty(this.label) && !TextUtils.isEmpty(getContentText(item))) {
                        contentText = getContentText(item) + this.label;
                    } else {
                        contentText = getContentText(item);
                    }
                    float fPow = (float) Math.pow(Math.abs(f3) / 90.0f, 2.2d);
                    reMeasureTextSize(contentText);
                    measuredCenterContentStart(contentText);
                    measuredOutContentStart(contentText);
                    float fCos = (float) ((((double) this.radius) - (Math.cos(d) * ((double) this.radius))) - ((Math.sin(d) * ((double) this.maxTextHeight)) / 2.0d));
                    canvas2.translate(0.0f, fCos);
                    float f4 = this.firstLineY;
                    if (fCos <= f4 && this.maxTextHeight + fCos >= f4) {
                        canvas2.save();
                        canvas2.clipRect(0.0f, 0.0f, this.measuredWidth, this.firstLineY - fCos);
                        canvas2.scale(1.0f, ((float) Math.sin(d)) * SCALE_CONTENT);
                        setOutPaintStyle(fPow, f3);
                        canvas2.drawText(contentText, this.drawOutContentStart, this.maxTextHeight, this.paintOuterText);
                        canvas2.restore();
                        canvas2.save();
                        canvas2.clipRect(0.0f, this.firstLineY - fCos, this.measuredWidth, (int) this.itemHeight);
                        canvas2.scale(1.0f, ((float) Math.sin(d)) * 1.0f);
                        canvas2.drawText(contentText, this.drawCenterContentStart, this.maxTextHeight - this.CENTER_CONTENT_OFFSET, this.paintCenterText);
                        canvas2.restore();
                    } else {
                        float f5 = this.secondLineY;
                        if (fCos <= f5 && this.maxTextHeight + fCos >= f5) {
                            canvas2.save();
                            canvas2.clipRect(0.0f, 0.0f, this.measuredWidth, this.secondLineY - fCos);
                            canvas2.scale(1.0f, ((float) Math.sin(d)) * 1.0f);
                            canvas2.drawText(contentText, this.drawCenterContentStart, this.maxTextHeight - this.CENTER_CONTENT_OFFSET, this.paintCenterText);
                            canvas2.restore();
                            canvas2.save();
                            canvas2.clipRect(0.0f, this.secondLineY - fCos, this.measuredWidth, (int) this.itemHeight);
                            canvas2.scale(1.0f, ((float) Math.sin(d)) * SCALE_CONTENT);
                            setOutPaintStyle(fPow, f3);
                            canvas2.drawText(contentText, this.drawOutContentStart, this.maxTextHeight, this.paintOuterText);
                            canvas2.restore();
                        } else {
                            if (fCos >= f4) {
                                int i5 = this.maxTextHeight;
                                if (i5 + fCos <= f5) {
                                    canvas2.drawText(contentText, this.drawCenterContentStart, i5 - this.CENTER_CONTENT_OFFSET, this.paintCenterText);
                                    this.selectedItem = this.preCurrentIndex - ((this.itemsVisible / 2) - i);
                                }
                                canvas2.restore();
                                this.paintCenterText.setTextSize(this.textSize);
                            }
                            canvas2.save();
                            z = false;
                            canvas2.clipRect(0, 0, this.measuredWidth, (int) this.itemHeight);
                            canvas2.scale(1.0f, ((float) Math.sin(d)) * SCALE_CONTENT);
                            setOutPaintStyle(fPow, f3);
                            canvas2.drawText(contentText, this.drawOutContentStart + (this.textXOffset * fPow), this.maxTextHeight, this.paintOuterText);
                            canvas2.restore();
                            canvas2.restore();
                            this.paintCenterText.setTextSize(this.textSize);
                        }
                    }
                    z = false;
                    canvas2.restore();
                    this.paintCenterText.setTextSize(this.textSize);
                }
                i++;
                z2 = z;
                f = f;
            }
        } catch (ArithmeticException unused) {
            Log.e("WheelView", "出错了！adapter.getItemsCount() == 0，联动数据不匹配");
        }
        if (!this.isLoop) {
            if (this.preCurrentIndex < 0) {
                this.preCurrentIndex = 0;
            }
            if (this.preCurrentIndex > this.adapter.getItemsCount() - 1) {
                this.preCurrentIndex = this.adapter.getItemsCount() - 1;
            }
        } else {
            if (this.preCurrentIndex < 0) {
                this.preCurrentIndex = this.adapter.getItemsCount() + this.preCurrentIndex;
            }
            if (this.preCurrentIndex > this.adapter.getItemsCount() - 1) {
                this.preCurrentIndex -= this.adapter.getItemsCount();
            }
        }
        f = this.totalScrollY % this.itemHeight;
        if (this.dividerType == DividerType.WRAP) {
            if (TextUtils.isEmpty(this.label)) {
                i2 = (this.measuredWidth - this.maxTextWidth) / 2;
            } else {
                i2 = (this.measuredWidth - this.maxTextWidth) / 4;
            }
            float f6 = i2 - 12;
            float f7 = f6 <= 0.0f ? 10.0f : f6;
            float f8 = this.measuredWidth - f7;
            float f9 = this.firstLineY;
            canvas2 = canvas;
            canvas2.drawLine(f7, f9, f8, f9, this.paintIndicator);
            float f10 = this.secondLineY;
            canvas2.drawLine(f7, f10, f8, f10, this.paintIndicator);
        } else {
            canvas2 = canvas;
            if (this.dividerType == DividerType.CIRCLE) {
                this.paintIndicator.setStyle(Paint.Style.STROKE);
                this.paintIndicator.setStrokeWidth(this.dividerWidth);
                if (TextUtils.isEmpty(this.label)) {
                    f2 = (this.measuredWidth - this.maxTextWidth) / 2.0f;
                } else {
                    f2 = (this.measuredWidth - this.maxTextWidth) / 4.0f;
                }
                float f11 = f2 - 12.0f;
                float f12 = f11 > 0.0f ? f11 : 10.0f;
                canvas2.drawCircle(this.measuredWidth / 2.0f, this.measuredHeight / 2.0f, Math.max((this.measuredWidth - f12) - f12, this.itemHeight) / 1.8f, this.paintIndicator);
            } else {
                float f13 = this.firstLineY;
                canvas2.drawLine(0.0f, f13, this.measuredWidth, f13, this.paintIndicator);
                float f14 = this.secondLineY;
                canvas2 = canvas;
                canvas2.drawLine(0.0f, f14, this.measuredWidth, f14, this.paintIndicator);
            }
        }
        if (!TextUtils.isEmpty(this.label) && this.isCenterLabel) {
            canvas2.drawText(this.label, (this.measuredWidth - getTextWidth(this.paintCenterText, this.label)) - this.CENTER_CONTENT_OFFSET, this.centerY, this.paintCenterText);
        }
        i = 0;
    }

    private void setOutPaintStyle(float f, float f2) {
        int i;
        int i2 = this.textXOffset;
        if (i2 > 0) {
            i = 1;
        } else {
            i = i2 < 0 ? -1 : 0;
        }
        this.paintOuterText.setTextSkewX(i * (f2 <= 0.0f ? 1 : -1) * 0.5f * f);
        this.paintOuterText.setAlpha(this.isAlphaGradient ? (int) (((90.0f - Math.abs(f2)) / 90.0f) * 255.0f) : 255);
    }

    private void reMeasureTextSize(String str) {
        Rect rect = new Rect();
        this.paintCenterText.getTextBounds(str, 0, str.length(), rect);
        int i = this.textSize;
        for (int iWidth = rect.width(); iWidth > this.measuredWidth; iWidth = rect.width()) {
            i--;
            this.paintCenterText.setTextSize(i);
            this.paintCenterText.getTextBounds(str, 0, str.length(), rect);
        }
        this.paintOuterText.setTextSize(i);
    }

    private int getLoopMappingIndex(int i) {
        if (i < 0) {
            return getLoopMappingIndex(i + this.adapter.getItemsCount());
        }
        return i > this.adapter.getItemsCount() + (-1) ? getLoopMappingIndex(i - this.adapter.getItemsCount()) : i;
    }

    private String getContentText(Object obj) {
        if (obj == null) {
            return "";
        }
        if (obj instanceof IPickerViewData) {
            return ((IPickerViewData) obj).getPickerViewText();
        }
        if (obj instanceof Integer) {
            return getFixNum(((Integer) obj).intValue());
        }
        return obj.toString();
    }

    private String getFixNum(int i) {
        return (i < 0 || i >= 10) ? String.valueOf(i) : TIME_NUM[i];
    }

    private void measuredCenterContentStart(String str) {
        String str2;
        Rect rect = new Rect();
        this.paintCenterText.getTextBounds(str, 0, str.length(), rect);
        int i = this.mGravity;
        if (i == 3) {
            this.drawCenterContentStart = 0;
            return;
        }
        if (i == 5) {
            this.drawCenterContentStart = (this.measuredWidth - rect.width()) - ((int) this.CENTER_CONTENT_OFFSET);
            return;
        }
        if (i != 17) {
            return;
        }
        if (this.isOptions || (str2 = this.label) == null || str2.equals("") || !this.isCenterLabel) {
            this.drawCenterContentStart = (int) (((double) (this.measuredWidth - rect.width())) * 0.5d);
        } else {
            this.drawCenterContentStart = (int) (((double) (this.measuredWidth - rect.width())) * 0.25d);
        }
    }

    private void measuredOutContentStart(String str) {
        String str2;
        Rect rect = new Rect();
        this.paintOuterText.getTextBounds(str, 0, str.length(), rect);
        int i = this.mGravity;
        if (i == 3) {
            this.drawOutContentStart = 0;
            return;
        }
        if (i == 5) {
            this.drawOutContentStart = (this.measuredWidth - rect.width()) - ((int) this.CENTER_CONTENT_OFFSET);
            return;
        }
        if (i != 17) {
            return;
        }
        if (this.isOptions || (str2 = this.label) == null || str2.equals("") || !this.isCenterLabel) {
            this.drawOutContentStart = (int) (((double) (this.measuredWidth - rect.width())) * 0.5d);
        } else {
            this.drawOutContentStart = (int) (((double) (this.measuredWidth - rect.width())) * 0.25d);
        }
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        this.widthMeasureSpec = i;
        reMeasure();
        setMeasuredDimension(this.measuredWidth, this.measuredHeight);
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        boolean zOnTouchEvent = this.gestureDetector.onTouchEvent(motionEvent);
        float f = (-this.initPosition) * this.itemHeight;
        float itemsCount = ((this.adapter.getItemsCount() - 1) - this.initPosition) * this.itemHeight;
        int action = motionEvent.getAction();
        if (action == 0) {
            this.startTime = System.currentTimeMillis();
            cancelFuture();
            this.previousY = motionEvent.getRawY();
        } else {
            if (action == 2) {
                float rawY = this.previousY - motionEvent.getRawY();
                this.previousY = motionEvent.getRawY();
                float f2 = this.totalScrollY + rawY;
                this.totalScrollY = f2;
                if (!this.isLoop) {
                    float f3 = this.itemHeight;
                    if ((f2 - (f3 * 0.25f) < f && rawY < 0.0f) || ((f3 * 0.25f) + f2 > itemsCount && rawY > 0.0f)) {
                        this.totalScrollY = f2 - rawY;
                    }
                }
                return true;
            }
            if (!zOnTouchEvent) {
                float y = motionEvent.getY();
                int i = this.radius;
                double dAcos = Math.acos((i - y) / i) * ((double) this.radius);
                float f4 = this.itemHeight;
                this.mOffset = (int) (((((int) ((dAcos + ((double) (f4 / 2.0f))) / ((double) f4))) - (this.itemsVisible / 2)) * f4) - (((this.totalScrollY % f4) + f4) % f4));
                if (System.currentTimeMillis() - this.startTime > 120) {
                    smoothScroll(ACTION.DAGGLE);
                } else {
                    smoothScroll(ACTION.CLICK);
                }
            }
        }
        if (motionEvent.getAction() != 0) {
            invalidate();
        }
        return true;
    }

    public int getItemsCount() {
        WheelAdapter wheelAdapter = this.adapter;
        if (wheelAdapter != null) {
            return wheelAdapter.getItemsCount();
        }
        return 0;
    }

    public void setLabel(String str) {
        this.label = str;
    }

    public void isCenterLabel(boolean z) {
        this.isCenterLabel = z;
    }

    public void setGravity(int i) {
        this.mGravity = i;
    }

    public int getTextWidth(Paint paint, String str) {
        if (str == null || str.length() <= 0) {
            return 0;
        }
        int length = str.length();
        float[] fArr = new float[length];
        paint.getTextWidths(str, fArr);
        int iCeil = 0;
        for (int i = 0; i < length; i++) {
            iCeil += (int) Math.ceil(fArr[i]);
        }
        return iCeil;
    }

    public void setIsOptions(boolean z) {
        this.isOptions = z;
    }

    public void setTextColorOut(int i) {
        this.textColorOut = i;
        this.paintOuterText.setColor(i);
    }

    public void setTextColorCenter(int i) {
        this.textColorCenter = i;
        this.paintCenterText.setColor(i);
    }

    public void setTextXOffset(int i) {
        this.textXOffset = i;
        if (i != 0) {
            this.paintCenterText.setTextScaleX(1.0f);
        }
    }

    public void setDividerWidth(int i) {
        this.dividerWidth = i;
        this.paintIndicator.setStrokeWidth(i);
    }

    public void setDividerColor(int i) {
        this.dividerColor = i;
        this.paintIndicator.setColor(i);
    }

    public void setDividerType(DividerType dividerType) {
        this.dividerType = dividerType;
    }

    public void setLineSpacingMultiplier(float f) {
        if (f != 0.0f) {
            this.lineSpacingMultiplier = f;
            judgeLineSpace();
        }
    }

    public boolean isLoop() {
        return this.isLoop;
    }

    public float getTotalScrollY() {
        return this.totalScrollY;
    }

    public void setTotalScrollY(float f) {
        this.totalScrollY = f;
    }

    public float getItemHeight() {
        return this.itemHeight;
    }

    public int getInitPosition() {
        return this.initPosition;
    }

    @Override // android.view.View
    public Handler getHandler() {
        return this.handler;
    }
}
