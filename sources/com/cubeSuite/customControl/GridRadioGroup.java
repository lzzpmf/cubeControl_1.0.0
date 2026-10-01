package com.cubeSuite.customControl;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;
import android.widget.RadioGroup;
import androidx.constraintlayout.solver.widgets.analyzer.BasicMeasure;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class GridRadioGroup extends RadioGroup {
    private static final int HORIZONTAL_SPACING_DIP = 10;
    private static final int VERTICAL_SPACING_DIP = 15;
    private float horizontalSpacing;
    private int numColumns;
    private float verticalSpacing;

    public GridRadioGroup(Context context) {
        this(context, null);
    }

    public GridRadioGroup(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.verticalSpacing = 20.0f;
        this.horizontalSpacing = 12.0f;
        this.numColumns = 3;
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.GridRadioGroup);
        this.numColumns = typedArrayObtainStyledAttributes.getInt(1, this.numColumns);
        this.horizontalSpacing = typedArrayObtainStyledAttributes.getDimension(0, (int) (context.getResources().getDisplayMetrics().density * 10.0f));
        this.verticalSpacing = typedArrayObtainStyledAttributes.getDimension(2, (int) (context.getResources().getDisplayMetrics().density * 15.0f));
        typedArrayObtainStyledAttributes.recycle();
    }

    @Override // android.widget.LinearLayout, android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        int childCount = getChildCount();
        float paddingRight = ((i3 - i) - getPaddingRight()) - getPaddingLeft();
        float f = this.horizontalSpacing;
        int i5 = this.numColumns;
        int i6 = ((int) (paddingRight - (f * (i5 - 1)))) / i5;
        int paddingLeft = getPaddingLeft();
        int i7 = 0;
        for (int i8 = 0; i8 < childCount; i8++) {
            View childAt = getChildAt(i8);
            int measuredHeight = childAt.getMeasuredHeight();
            int paddingLeft2 = paddingLeft + i6;
            if (i8 % this.numColumns == 0) {
                paddingLeft2 = getPaddingLeft() + i6;
                i7++;
            }
            int paddingTop = (i7 * measuredHeight) + ((i7 - 1) * ((int) this.verticalSpacing)) + getPaddingTop();
            childAt.layout(paddingLeft2 - i6, paddingTop - measuredHeight, paddingLeft2, paddingTop);
            paddingLeft = (int) (paddingLeft2 + this.horizontalSpacing);
        }
    }

    @Override // android.widget.LinearLayout, android.view.View
    protected void onMeasure(int i, int i2) {
        int childCount = getChildCount();
        int size = View.MeasureSpec.getSize(i);
        float paddingRight = (size - getPaddingRight()) - getPaddingLeft();
        float f = this.horizontalSpacing;
        int i3 = this.numColumns;
        int i4 = ((int) (paddingRight - (f * (i3 - 1)))) / i3;
        int paddingTop = 0;
        int i5 = 0;
        for (int i6 = 0; i6 < childCount; i6++) {
            View childAt = getChildAt(i6);
            childAt.measure(View.MeasureSpec.makeMeasureSpec(i4, BasicMeasure.EXACTLY), 0);
            int measuredHeight = childAt.getMeasuredHeight();
            if (i6 % this.numColumns == 0) {
                i5++;
            }
            paddingTop = (measuredHeight * i5) + ((i5 - 1) * ((int) this.verticalSpacing)) + getPaddingTop() + getPaddingBottom();
        }
        setMeasuredDimension(size, paddingTop);
    }
}
