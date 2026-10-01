package com.cubeSuite.customControl;

import android.animation.ValueAnimator;
import android.content.Context;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewConfiguration;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import androidx.cardview.widget.CardView;
import androidx.viewpager.widget.ViewPager;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class SlidingDeleteItem extends LinearLayout {
    public static final String TAG = "LeftSlideView";
    private Button btnDelete;
    private boolean isReCompute;
    private boolean isRemove;
    private int mAnimDuring;
    private CardView mCardView;
    private View mContentView;
    private Context mContext;
    private int mDelLength;
    private float mInitX;
    private float mInitY;
    private View mMenuView;
    private int mRightCanSlide;
    private OnDelViewStatusChangeLister mStatusChangeLister;
    private int mTouchSlop;
    private ValueAnimator mValueAnimator;
    private ViewPager mViewPager;
    private ScrollView scrollView;

    public interface OnDelViewStatusChangeLister {
        void onStatusChange(boolean z);
    }

    public interface SlidingDeleteItemListener {
        void onClick(View view);
    }

    private void initView() {
    }

    public SlidingDeleteItem(Context context) {
        this(context, null);
    }

    public SlidingDeleteItem(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public SlidingDeleteItem(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mAnimDuring = 400;
        this.mDelLength = 76;
        this.isReCompute = true;
        this.isRemove = true;
        this.mContext = context;
        init();
    }

    private int dp2px(float f) {
        return (int) ((f * getResources().getDisplayMetrics().density) + 0.5f);
    }

    private void init() {
        this.mTouchSlop = ViewConfiguration.get(this.mContext).getScaledTouchSlop();
        this.mRightCanSlide = dp2px(this.mDelLength);
        setBackgroundColor(0);
        setOrientation(0);
        initView();
    }

    public void addContentView(View view) {
        this.mContentView = view;
        view.setTag("contentView");
        View viewFindViewWithTag = findViewWithTag("contentView");
        if (viewFindViewWithTag != null) {
            removeView(viewFindViewWithTag);
        }
        addView(this.mContentView, new LinearLayout.LayoutParams(-1, -1));
    }

    public void setOnClick(final SlidingDeleteItemListener slidingDeleteItemListener) {
        View view = this.mContentView;
        Objects.requireNonNull(slidingDeleteItemListener);
        view.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SlidingDeleteItem$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view2) {
                slidingDeleteItemListener.onClick(view2);
            }
        });
    }

    public View getContentView() {
        return this.mContentView;
    }

    public void addMenuView(View view) {
        this.mMenuView = view;
        view.setTag("menuView");
        View viewFindViewWithTag = findViewWithTag("menuView");
        if (viewFindViewWithTag != null) {
            removeView(viewFindViewWithTag);
        }
        addView(this.mMenuView, new LinearLayout.LayoutParams(this.mRightCanSlide, -1));
    }

    public Button getBtnDelete() {
        return this.btnDelete;
    }

    public void setBtnDelete(Button button) {
        this.btnDelete = button;
    }

    public void deleteAnimator() {
        scrollTo(0, 0);
    }

    public void setViewPager(ViewPager viewPager) {
        this.mViewPager = viewPager;
    }

    public void setScrollView(ScrollView scrollView) {
        this.scrollView = scrollView;
    }

    public void setCardView(CardView cardView) {
        this.mCardView = cardView;
    }

    public void setStatusChangeLister(OnDelViewStatusChangeLister onDelViewStatusChangeLister) {
        this.mStatusChangeLister = onDelViewStatusChangeLister;
    }

    public void setRemove(boolean z) {
        this.isRemove = z;
    }

    /* JADX WARN: Code duplicated, block: B:46:0x00c2  */
    /* JADX WARN: Code duplicated, block: B:48:0x00c6  */
    @Override // android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        ScrollView scrollView;
        if (!this.isRemove) {
            return super.onInterceptTouchEvent(motionEvent);
        }
        int actionMasked = motionEvent.getActionMasked();
        if (actionMasked == 0) {
            this.mInitX = motionEvent.getRawX() + getScrollX();
            this.mInitY = motionEvent.getRawY();
            clearAnim();
            ViewPager viewPager = this.mViewPager;
            if (viewPager != null) {
                viewPager.requestDisallowInterceptTouchEvent(true);
            }
            CardView cardView = this.mCardView;
            if (cardView != null) {
                cardView.requestDisallowInterceptTouchEvent(true);
            }
        } else if (actionMasked == 1) {
            scrollView = this.scrollView;
            if (scrollView != null) {
                scrollView.requestDisallowInterceptTouchEvent(false);
                this.isReCompute = true;
            }
        } else if (actionMasked != 2) {
            if (actionMasked == 3) {
                scrollView = this.scrollView;
                if (scrollView != null) {
                    scrollView.requestDisallowInterceptTouchEvent(false);
                    this.isReCompute = true;
                }
            }
        } else {
            if (this.mInitX - motionEvent.getRawX() < 0.0f) {
                ScrollView scrollView2 = this.scrollView;
                if (scrollView2 != null && this.isReCompute) {
                    scrollView2.requestDisallowInterceptTouchEvent(false);
                    this.isReCompute = false;
                }
                ViewPager viewPager2 = this.mViewPager;
                if (viewPager2 != null) {
                    viewPager2.requestDisallowInterceptTouchEvent(true);
                }
                return false;
            }
            if (Math.abs(motionEvent.getRawY() - this.mInitY) >= this.mTouchSlop && Math.abs(motionEvent.getRawY() - this.mInitY) > Math.abs((this.mInitX - motionEvent.getRawX()) - getScrollX())) {
                ScrollView scrollView3 = this.scrollView;
                if (scrollView3 != null && this.isReCompute) {
                    scrollView3.requestDisallowInterceptTouchEvent(false);
                    this.isReCompute = false;
                }
                return false;
            }
            if (Math.abs((this.mInitX - motionEvent.getRawX()) - getScrollX()) >= this.mTouchSlop && Math.abs(motionEvent.getRawY() - this.mInitY) <= Math.abs((this.mInitX - motionEvent.getRawX()) - getScrollX())) {
                ScrollView scrollView4 = this.scrollView;
                if (scrollView4 != null && this.isReCompute) {
                    scrollView4.requestDisallowInterceptTouchEvent(true);
                    this.isReCompute = false;
                }
                return true;
            }
        }
        return super.onInterceptTouchEvent(motionEvent);
    }

    /* JADX WARN: Code restructure failed: missing block: B:9:0x000e, code lost:
    
        if (r0 != 3) goto L68;
     */
    @Override // android.view.View
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct code enable 'Show inconsistent code' option in preferences
    */
    public boolean onTouchEvent(android.view.MotionEvent r7) {
        /*
            Method dump skipped, instruction units count: 294
            To view this dump change 'Code comments level' option to 'DEBUG'
        */
        throw new UnsupportedOperationException("Method not decompiled: com.cubeSuite.customControl.SlidingDeleteItem.onTouchEvent(android.view.MotionEvent):boolean");
    }

    private void clearAnim() {
        ValueAnimator valueAnimator = this.mValueAnimator;
        if (valueAnimator == null) {
            return;
        }
        valueAnimator.end();
        this.mValueAnimator.cancel();
        this.mValueAnimator = null;
    }

    private void upAnim() {
        int scrollX = getScrollX();
        int i = this.mRightCanSlide;
        if (scrollX == i || scrollX == 0) {
            OnDelViewStatusChangeLister onDelViewStatusChangeLister = this.mStatusChangeLister;
            if (onDelViewStatusChangeLister != null) {
                onDelViewStatusChangeLister.onStatusChange(scrollX == i);
                return;
            }
            return;
        }
        clearAnim();
        int i2 = this.mRightCanSlide;
        if (scrollX >= i2 / 2) {
            ValueAnimator valueAnimatorOfInt = ValueAnimator.ofInt(scrollX, i2);
            this.mValueAnimator = valueAnimatorOfInt;
            valueAnimatorOfInt.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() { // from class: com.cubeSuite.customControl.SlidingDeleteItem.1
                @Override // android.animation.ValueAnimator.AnimatorUpdateListener
                public void onAnimationUpdate(ValueAnimator valueAnimator) {
                    SlidingDeleteItem.this.scrollTo(((Integer) valueAnimator.getAnimatedValue()).intValue(), 0);
                }
            });
            this.mValueAnimator.setDuration(this.mAnimDuring);
            this.mValueAnimator.start();
            OnDelViewStatusChangeLister onDelViewStatusChangeLister2 = this.mStatusChangeLister;
            if (onDelViewStatusChangeLister2 != null) {
                onDelViewStatusChangeLister2.onStatusChange(true);
                return;
            }
            return;
        }
        ValueAnimator valueAnimatorOfInt2 = ValueAnimator.ofInt(scrollX, 0);
        this.mValueAnimator = valueAnimatorOfInt2;
        valueAnimatorOfInt2.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() { // from class: com.cubeSuite.customControl.SlidingDeleteItem.2
            @Override // android.animation.ValueAnimator.AnimatorUpdateListener
            public void onAnimationUpdate(ValueAnimator valueAnimator) {
                SlidingDeleteItem.this.scrollTo(((Integer) valueAnimator.getAnimatedValue()).intValue(), 0);
            }
        });
        this.mValueAnimator.setDuration(this.mAnimDuring);
        this.mValueAnimator.start();
        OnDelViewStatusChangeLister onDelViewStatusChangeLister3 = this.mStatusChangeLister;
        if (onDelViewStatusChangeLister3 != null) {
            onDelViewStatusChangeLister3.onStatusChange(false);
        }
    }

    public void resetDelStatus() {
        int scrollX = getScrollX();
        if (scrollX == 0) {
            return;
        }
        clearAnim();
        ValueAnimator valueAnimatorOfInt = ValueAnimator.ofInt(scrollX, 0);
        this.mValueAnimator = valueAnimatorOfInt;
        valueAnimatorOfInt.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() { // from class: com.cubeSuite.customControl.SlidingDeleteItem.3
            @Override // android.animation.ValueAnimator.AnimatorUpdateListener
            public void onAnimationUpdate(ValueAnimator valueAnimator) {
                SlidingDeleteItem.this.scrollTo(((Integer) valueAnimator.getAnimatedValue()).intValue(), 0);
            }
        });
        this.mValueAnimator.setDuration(this.mAnimDuring);
        this.mValueAnimator.start();
    }
}
