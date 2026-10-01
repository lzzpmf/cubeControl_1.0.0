package androidx.constraintlayout.widget;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.util.Log;
import android.util.SparseArray;
import android.util.SparseIntArray;
import android.view.View;
import android.view.ViewGroup;
import androidx.constraintlayout.solver.Metrics;
import androidx.constraintlayout.solver.widgets.ConstraintAnchor;
import androidx.constraintlayout.solver.widgets.ConstraintWidget;
import androidx.constraintlayout.solver.widgets.ConstraintWidgetContainer;
import androidx.constraintlayout.solver.widgets.Optimizer;
import androidx.constraintlayout.solver.widgets.analyzer.BasicMeasure;
import androidx.core.internal.view.SupportMenu;
import androidx.core.view.ViewCompat;
import java.util.ArrayList;
import java.util.HashMap;

/* JADX INFO: loaded from: classes.dex */
public class ConstraintLayout extends ViewGroup {
    private static final boolean DEBUG = false;
    private static final boolean DEBUG_DRAW_CONSTRAINTS = false;
    public static final int DESIGN_INFO_ID = 0;
    private static final boolean MEASURE = false;
    private static final String TAG = "ConstraintLayout";
    private static final boolean USE_CONSTRAINTS_HELPER = true;
    public static final String VERSION = "ConstraintLayout-2.0.1";
    SparseArray<View> mChildrenByIds;
    private ArrayList<ConstraintHelper> mConstraintHelpers;
    protected ConstraintLayoutStates mConstraintLayoutSpec;
    private ConstraintSet mConstraintSet;
    private int mConstraintSetId;
    private ConstraintsChangedListener mConstraintsChangedListener;
    private HashMap<String, Integer> mDesignIds;
    protected boolean mDirtyHierarchy;
    private int mLastMeasureHeight;
    int mLastMeasureHeightMode;
    int mLastMeasureHeightSize;
    private int mLastMeasureWidth;
    int mLastMeasureWidthMode;
    int mLastMeasureWidthSize;
    protected ConstraintWidgetContainer mLayoutWidget;
    private int mMaxHeight;
    private int mMaxWidth;
    Measurer mMeasurer;
    private Metrics mMetrics;
    private int mMinHeight;
    private int mMinWidth;
    private int mOnMeasureHeightMeasureSpec;
    private int mOnMeasureWidthMeasureSpec;
    private int mOptimizationLevel;
    private SparseArray<ConstraintWidget> mTempMapIdToWidget;

    @Override // android.view.ViewGroup
    public boolean shouldDelayChildPressedState() {
        return false;
    }

    public void setDesignInformation(int i, Object obj, Object obj2) {
        if (i == 0 && (obj instanceof String) && (obj2 instanceof Integer)) {
            if (this.mDesignIds == null) {
                this.mDesignIds = new HashMap<>();
            }
            String strSubstring = (String) obj;
            int iIndexOf = strSubstring.indexOf("/");
            if (iIndexOf != -1) {
                strSubstring = strSubstring.substring(iIndexOf + 1);
            }
            this.mDesignIds.put(strSubstring, Integer.valueOf(((Integer) obj2).intValue()));
        }
    }

    public Object getDesignInformation(int i, Object obj) {
        if (i != 0 || !(obj instanceof String)) {
            return null;
        }
        String str = (String) obj;
        HashMap<String, Integer> map = this.mDesignIds;
        if (map == null || !map.containsKey(str)) {
            return null;
        }
        return this.mDesignIds.get(str);
    }

    public ConstraintLayout(Context context) {
        super(context);
        this.mChildrenByIds = new SparseArray<>();
        this.mConstraintHelpers = new ArrayList<>(4);
        this.mLayoutWidget = new ConstraintWidgetContainer();
        this.mMinWidth = 0;
        this.mMinHeight = 0;
        this.mMaxWidth = Integer.MAX_VALUE;
        this.mMaxHeight = Integer.MAX_VALUE;
        this.mDirtyHierarchy = true;
        this.mOptimizationLevel = Optimizer.OPTIMIZATION_STANDARD;
        this.mConstraintSet = null;
        this.mConstraintLayoutSpec = null;
        this.mConstraintSetId = -1;
        this.mDesignIds = new HashMap<>();
        this.mLastMeasureWidth = -1;
        this.mLastMeasureHeight = -1;
        this.mLastMeasureWidthSize = -1;
        this.mLastMeasureHeightSize = -1;
        this.mLastMeasureWidthMode = 0;
        this.mLastMeasureHeightMode = 0;
        this.mTempMapIdToWidget = new SparseArray<>();
        this.mMeasurer = new Measurer(this);
        this.mOnMeasureWidthMeasureSpec = 0;
        this.mOnMeasureHeightMeasureSpec = 0;
        init(null, 0, 0);
    }

    public ConstraintLayout(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mChildrenByIds = new SparseArray<>();
        this.mConstraintHelpers = new ArrayList<>(4);
        this.mLayoutWidget = new ConstraintWidgetContainer();
        this.mMinWidth = 0;
        this.mMinHeight = 0;
        this.mMaxWidth = Integer.MAX_VALUE;
        this.mMaxHeight = Integer.MAX_VALUE;
        this.mDirtyHierarchy = true;
        this.mOptimizationLevel = Optimizer.OPTIMIZATION_STANDARD;
        this.mConstraintSet = null;
        this.mConstraintLayoutSpec = null;
        this.mConstraintSetId = -1;
        this.mDesignIds = new HashMap<>();
        this.mLastMeasureWidth = -1;
        this.mLastMeasureHeight = -1;
        this.mLastMeasureWidthSize = -1;
        this.mLastMeasureHeightSize = -1;
        this.mLastMeasureWidthMode = 0;
        this.mLastMeasureHeightMode = 0;
        this.mTempMapIdToWidget = new SparseArray<>();
        this.mMeasurer = new Measurer(this);
        this.mOnMeasureWidthMeasureSpec = 0;
        this.mOnMeasureHeightMeasureSpec = 0;
        init(attributeSet, 0, 0);
    }

    public ConstraintLayout(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mChildrenByIds = new SparseArray<>();
        this.mConstraintHelpers = new ArrayList<>(4);
        this.mLayoutWidget = new ConstraintWidgetContainer();
        this.mMinWidth = 0;
        this.mMinHeight = 0;
        this.mMaxWidth = Integer.MAX_VALUE;
        this.mMaxHeight = Integer.MAX_VALUE;
        this.mDirtyHierarchy = true;
        this.mOptimizationLevel = Optimizer.OPTIMIZATION_STANDARD;
        this.mConstraintSet = null;
        this.mConstraintLayoutSpec = null;
        this.mConstraintSetId = -1;
        this.mDesignIds = new HashMap<>();
        this.mLastMeasureWidth = -1;
        this.mLastMeasureHeight = -1;
        this.mLastMeasureWidthSize = -1;
        this.mLastMeasureHeightSize = -1;
        this.mLastMeasureWidthMode = 0;
        this.mLastMeasureHeightMode = 0;
        this.mTempMapIdToWidget = new SparseArray<>();
        this.mMeasurer = new Measurer(this);
        this.mOnMeasureWidthMeasureSpec = 0;
        this.mOnMeasureHeightMeasureSpec = 0;
        init(attributeSet, i, 0);
    }

    public ConstraintLayout(Context context, AttributeSet attributeSet, int i, int i2) {
        super(context, attributeSet, i, i2);
        this.mChildrenByIds = new SparseArray<>();
        this.mConstraintHelpers = new ArrayList<>(4);
        this.mLayoutWidget = new ConstraintWidgetContainer();
        this.mMinWidth = 0;
        this.mMinHeight = 0;
        this.mMaxWidth = Integer.MAX_VALUE;
        this.mMaxHeight = Integer.MAX_VALUE;
        this.mDirtyHierarchy = true;
        this.mOptimizationLevel = Optimizer.OPTIMIZATION_STANDARD;
        this.mConstraintSet = null;
        this.mConstraintLayoutSpec = null;
        this.mConstraintSetId = -1;
        this.mDesignIds = new HashMap<>();
        this.mLastMeasureWidth = -1;
        this.mLastMeasureHeight = -1;
        this.mLastMeasureWidthSize = -1;
        this.mLastMeasureHeightSize = -1;
        this.mLastMeasureWidthMode = 0;
        this.mLastMeasureHeightMode = 0;
        this.mTempMapIdToWidget = new SparseArray<>();
        this.mMeasurer = new Measurer(this);
        this.mOnMeasureWidthMeasureSpec = 0;
        this.mOnMeasureHeightMeasureSpec = 0;
        init(attributeSet, i, i2);
    }

    @Override // android.view.View
    public void setId(int i) {
        this.mChildrenByIds.remove(getId());
        super.setId(i);
        this.mChildrenByIds.put(getId(), this);
    }

    class Measurer implements BasicMeasure.Measurer {
        ConstraintLayout layout;
        int layoutHeightSpec;
        int layoutWidthSpec;
        int paddingBottom;
        int paddingHeight;
        int paddingTop;
        int paddingWidth;

        public void captureLayoutInfos(int i, int i2, int i3, int i4, int i5, int i6) {
            this.paddingTop = i3;
            this.paddingBottom = i4;
            this.paddingWidth = i5;
            this.paddingHeight = i6;
            this.layoutWidthSpec = i;
            this.layoutHeightSpec = i2;
        }

        public Measurer(ConstraintLayout constraintLayout) {
            this.layout = constraintLayout;
        }

        /* JADX WARN: Code duplicated, block: B:100:0x0175  */
        /* JADX WARN: Code duplicated, block: B:103:0x017d  */
        /* JADX WARN: Code duplicated, block: B:106:0x018b A[ADDED_TO_REGION] */
        /* JADX WARN: Code duplicated, block: B:115:0x019f  */
        /* JADX WARN: Code duplicated, block: B:117:0x01a3  */
        /* JADX WARN: Code duplicated, block: B:120:0x01b1  */
        /* JADX WARN: Code duplicated, block: B:123:0x01c2  */
        /* JADX WARN: Code duplicated, block: B:124:0x01cb  */
        /* JADX WARN: Code duplicated, block: B:126:0x01d5  */
        /* JADX WARN: Code duplicated, block: B:127:0x01de  */
        /* JADX WARN: Code duplicated, block: B:130:0x01ea  */
        /* JADX WARN: Code duplicated, block: B:131:0x01f1  */
        /* JADX WARN: Code duplicated, block: B:134:0x01f6  */
        /* JADX WARN: Code duplicated, block: B:137:0x0200  */
        /* JADX WARN: Code duplicated, block: B:138:0x0207  */
        /* JADX WARN: Code duplicated, block: B:141:0x020c  */
        /* JADX WARN: Code duplicated, block: B:144:0x0216 A[ADDED_TO_REGION] */
        /* JADX WARN: Code duplicated, block: B:146:0x021f A[DONT_INVERT] */
        /* JADX WARN: Code duplicated, block: B:147:0x0221 A[ADDED_TO_REGION] */
        /* JADX WARN: Code duplicated, block: B:150:0x022b A[ADDED_TO_REGION] */
        /* JADX WARN: Code duplicated, block: B:151:0x022d  */
        /* JADX WARN: Code duplicated, block: B:153:0x0231  */
        /* JADX WARN: Code duplicated, block: B:155:0x0237  */
        /* JADX WARN: Code duplicated, block: B:158:0x024e  */
        /* JADX WARN: Code duplicated, block: B:159:0x0251  */
        /* JADX WARN: Code duplicated, block: B:162:0x0257  */
        /* JADX WARN: Code duplicated, block: B:166:0x025f  */
        /* JADX WARN: Code duplicated, block: B:169:0x0267  */
        /* JADX WARN: Code duplicated, block: B:171:0x026b  */
        /* JADX WARN: Code duplicated, block: B:42:0x00c3 A[DONT_INVERT] */
        /* JADX WARN: Code duplicated, block: B:43:0x00c5 A[DONT_INVERT] */
        /* JADX WARN: Code duplicated, block: B:44:0x00c7  */
        /* JADX WARN: Code duplicated, block: B:46:0x00ca  */
        /* JADX WARN: Code duplicated, block: B:47:0x00cf  */
        /* JADX WARN: Code duplicated, block: B:49:0x00d9  */
        /* JADX WARN: Code duplicated, block: B:50:0x00db  */
        /* JADX WARN: Code duplicated, block: B:53:0x00e5  */
        /* JADX WARN: Code duplicated, block: B:54:0x00e7  */
        /* JADX WARN: Code duplicated, block: B:58:0x00f7  */
        /* JADX WARN: Code duplicated, block: B:63:0x0101 A[ADDED_TO_REGION] */
        /* JADX WARN: Code duplicated, block: B:65:0x010e  */
        /* JADX WARN: Code duplicated, block: B:66:0x011f  */
        /* JADX WARN: Code duplicated, block: B:68:0x012b  */
        /* JADX WARN: Code duplicated, block: B:72:0x013c  */
        /* JADX WARN: Code duplicated, block: B:73:0x013e  */
        /* JADX WARN: Code duplicated, block: B:76:0x0144  */
        /* JADX WARN: Code duplicated, block: B:77:0x0146  */
        /* JADX WARN: Code duplicated, block: B:80:0x014c  */
        /* JADX WARN: Code duplicated, block: B:84:0x0154  */
        /* JADX WARN: Code duplicated, block: B:87:0x0159  */
        /* JADX WARN: Code duplicated, block: B:91:0x0161  */
        /* JADX WARN: Code duplicated, block: B:94:0x0167  */
        /* JADX WARN: Code duplicated, block: B:97:0x016f  */
        @Override // androidx.constraintlayout.solver.widgets.analyzer.BasicMeasure.Measurer
        public final void measure(ConstraintWidget constraintWidget, BasicMeasure.Measure measure) {
            int iMakeMeasureSpec;
            boolean z;
            int i;
            int iMakeMeasureSpec2;
            boolean z2;
            boolean z3;
            boolean z4;
            boolean z5;
            boolean z6;
            boolean z7;
            boolean z8;
            LayoutParams layoutParams;
            int measuredWidth;
            int measuredHeight;
            int baseline;
            int measuredWidth2;
            int measuredHeight2;
            boolean z9;
            boolean z10;
            boolean z11;
            if (constraintWidget == null) {
                return;
            }
            if (constraintWidget.getVisibility() == 8 && !constraintWidget.isInPlaceholder()) {
                measure.measuredWidth = 0;
                measure.measuredHeight = 0;
                measure.measuredBaseline = 0;
                return;
            }
            ConstraintWidget.DimensionBehaviour dimensionBehaviour = measure.horizontalBehavior;
            ConstraintWidget.DimensionBehaviour dimensionBehaviour2 = measure.verticalBehavior;
            int i2 = measure.horizontalDimension;
            int i3 = measure.verticalDimension;
            int i4 = this.paddingTop + this.paddingBottom;
            int i5 = this.paddingWidth;
            View view = (View) constraintWidget.getCompanionWidget();
            int i6 = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[dimensionBehaviour.ordinal()];
            if (i6 == 1) {
                int iMakeMeasureSpec3 = View.MeasureSpec.makeMeasureSpec(i2, BasicMeasure.EXACTLY);
                constraintWidget.wrapMeasure[2] = i2;
                iMakeMeasureSpec = iMakeMeasureSpec3;
            } else {
                if (i6 == 2) {
                    iMakeMeasureSpec = ViewGroup.getChildMeasureSpec(this.layoutWidthSpec, i5, -2);
                    constraintWidget.wrapMeasure[2] = -2;
                } else if (i6 == 3) {
                    iMakeMeasureSpec = ViewGroup.getChildMeasureSpec(this.layoutWidthSpec, i5 + constraintWidget.getHorizontalMargin(), -1);
                    constraintWidget.wrapMeasure[2] = -1;
                } else {
                    if (i6 != 4) {
                        iMakeMeasureSpec = 0;
                        z = false;
                    } else {
                        iMakeMeasureSpec = ViewGroup.getChildMeasureSpec(this.layoutWidthSpec, i5, -2);
                        boolean z12 = constraintWidget.mMatchConstraintDefaultWidth == 1;
                        constraintWidget.wrapMeasure[2] = 0;
                        if (measure.useCurrentDimensions) {
                            boolean z13 = !(!z12 || constraintWidget.wrapMeasure[3] == 0 || constraintWidget.wrapMeasure[0] == constraintWidget.getWidth()) || (view instanceof Placeholder);
                            if (!z12 || z13) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(constraintWidget.getWidth(), BasicMeasure.EXACTLY);
                            }
                        }
                    }
                    i = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[dimensionBehaviour2.ordinal()];
                    if (i == 1) {
                        int iMakeMeasureSpec4 = View.MeasureSpec.makeMeasureSpec(i3, BasicMeasure.EXACTLY);
                        constraintWidget.wrapMeasure[3] = i3;
                        iMakeMeasureSpec2 = iMakeMeasureSpec4;
                    } else {
                        if (i == 2) {
                            iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                            constraintWidget.wrapMeasure[3] = -2;
                        } else if (i == 3) {
                            iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4 + constraintWidget.getVerticalMargin(), -1);
                            constraintWidget.wrapMeasure[3] = -1;
                        } else {
                            if (i != 4) {
                                iMakeMeasureSpec2 = 0;
                                z2 = false;
                            } else {
                                iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                                if (constraintWidget.mMatchConstraintDefaultHeight == 1) {
                                    z11 = true;
                                } else {
                                    z11 = false;
                                }
                                constraintWidget.wrapMeasure[3] = 0;
                                if (measure.useCurrentDimensions) {
                                    boolean z14 = !(z11 || constraintWidget.wrapMeasure[2] == 0 || constraintWidget.wrapMeasure[1] == constraintWidget.getHeight()) || (view instanceof Placeholder);
                                    if (z11 || z14) {
                                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(constraintWidget.getHeight(), BasicMeasure.EXACTLY);
                                    }
                                }
                            }
                            if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                                z3 = true;
                            } else {
                                z3 = false;
                            }
                            if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                                z4 = true;
                            } else {
                                z4 = false;
                            }
                            if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT || dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.FIXED) {
                                z5 = true;
                            } else {
                                z5 = false;
                            }
                            if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT || dimensionBehaviour == ConstraintWidget.DimensionBehaviour.FIXED) {
                                z6 = true;
                            } else {
                                z6 = false;
                            }
                            if (z3 || constraintWidget.mDimensionRatio <= 0.0f) {
                                z7 = false;
                            } else {
                                z7 = true;
                            }
                            if (z4 || constraintWidget.mDimensionRatio <= 0.0f) {
                                z8 = false;
                            } else {
                                z8 = true;
                            }
                            layoutParams = (LayoutParams) view.getLayoutParams();
                            if (measure.useCurrentDimensions && z3 && constraintWidget.mMatchConstraintDefaultWidth == 0 && z4 && constraintWidget.mMatchConstraintDefaultHeight == 0) {
                                measuredWidth2 = 0;
                                measuredHeight2 = 0;
                                baseline = 0;
                            } else {
                                if (!(view instanceof VirtualLayout) && (constraintWidget instanceof androidx.constraintlayout.solver.widgets.VirtualLayout)) {
                                    ((VirtualLayout) view).onMeasure((androidx.constraintlayout.solver.widgets.VirtualLayout) constraintWidget, iMakeMeasureSpec, iMakeMeasureSpec2);
                                } else {
                                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                }
                                measuredWidth = view.getMeasuredWidth();
                                measuredHeight = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                                if (z) {
                                    constraintWidget.wrapMeasure[0] = measuredWidth;
                                    constraintWidget.wrapMeasure[2] = measuredHeight;
                                } else {
                                    constraintWidget.wrapMeasure[0] = 0;
                                    constraintWidget.wrapMeasure[2] = 0;
                                }
                                if (z2) {
                                    constraintWidget.wrapMeasure[1] = measuredHeight;
                                    constraintWidget.wrapMeasure[3] = measuredWidth;
                                } else {
                                    constraintWidget.wrapMeasure[1] = 0;
                                    constraintWidget.wrapMeasure[3] = 0;
                                }
                                if (constraintWidget.mMatchConstraintMinWidth > 0) {
                                    measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                                } else {
                                    measuredWidth2 = measuredWidth;
                                }
                                if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                                    measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                                }
                                if (constraintWidget.mMatchConstraintMinHeight > 0) {
                                    measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                                } else {
                                    measuredHeight2 = measuredHeight;
                                }
                                if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                                    measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                                }
                                if (!z7 && z5) {
                                    measuredWidth2 = (int) ((measuredHeight2 * constraintWidget.mDimensionRatio) + 0.5f);
                                } else if (z8 && z6) {
                                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                                }
                                if (measuredWidth == measuredWidth2 || measuredHeight != measuredHeight2) {
                                    if (measuredWidth != measuredWidth2) {
                                        iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                    }
                                    if (measuredHeight != measuredHeight2) {
                                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                    }
                                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                    measuredWidth2 = view.getMeasuredWidth();
                                    measuredHeight2 = view.getMeasuredHeight();
                                    baseline = view.getBaseline();
                                }
                            }
                            if (baseline != -1) {
                                z9 = true;
                            } else {
                                z9 = false;
                            }
                            if (measuredWidth2 == measure.horizontalDimension || measuredHeight2 != measure.verticalDimension) {
                                z10 = true;
                            } else {
                                z10 = false;
                            }
                            measure.measuredNeedsSolverPass = z10;
                            if (layoutParams.needsBaseline) {
                                z9 = true;
                            }
                            if (z9 && baseline != -1 && constraintWidget.getBaselineDistance() != baseline) {
                                measure.measuredNeedsSolverPass = true;
                            }
                            measure.measuredWidth = measuredWidth2;
                            measure.measuredHeight = measuredHeight2;
                            measure.measuredHasBaseline = z9;
                            measure.measuredBaseline = baseline;
                        }
                        z2 = true;
                        if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                            z3 = true;
                        } else {
                            z3 = false;
                        }
                        if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                            z4 = true;
                        } else {
                            z4 = false;
                        }
                        if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                            z5 = true;
                        } else {
                            z5 = true;
                        }
                        if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                            z6 = true;
                        } else {
                            z6 = true;
                        }
                        if (z3) {
                            z7 = false;
                        } else {
                            z7 = false;
                        }
                        if (z4) {
                            z8 = false;
                        } else {
                            z8 = false;
                        }
                        layoutParams = (LayoutParams) view.getLayoutParams();
                        if (measure.useCurrentDimensions) {
                            if (!(view instanceof VirtualLayout)) {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            } else {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            }
                            measuredWidth = view.getMeasuredWidth();
                            measuredHeight = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                            if (z) {
                                constraintWidget.wrapMeasure[0] = measuredWidth;
                                constraintWidget.wrapMeasure[2] = measuredHeight;
                            } else {
                                constraintWidget.wrapMeasure[0] = 0;
                                constraintWidget.wrapMeasure[2] = 0;
                            }
                            if (z2) {
                                constraintWidget.wrapMeasure[1] = measuredHeight;
                                constraintWidget.wrapMeasure[3] = measuredWidth;
                            } else {
                                constraintWidget.wrapMeasure[1] = 0;
                                constraintWidget.wrapMeasure[3] = 0;
                            }
                            if (constraintWidget.mMatchConstraintMinWidth > 0) {
                                measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                            } else {
                                measuredWidth2 = measuredWidth;
                            }
                            if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                                measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                            }
                            if (constraintWidget.mMatchConstraintMinHeight > 0) {
                                measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                            } else {
                                measuredHeight2 = measuredHeight;
                            }
                            if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                                measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                            }
                            if (!z7) {
                                if (z8) {
                                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                                }
                            } else if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                            if (measuredWidth == measuredWidth2) {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            } else {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            }
                        } else {
                            if (!(view instanceof VirtualLayout)) {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            } else {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            }
                            measuredWidth = view.getMeasuredWidth();
                            measuredHeight = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                            if (z) {
                                constraintWidget.wrapMeasure[0] = measuredWidth;
                                constraintWidget.wrapMeasure[2] = measuredHeight;
                            } else {
                                constraintWidget.wrapMeasure[0] = 0;
                                constraintWidget.wrapMeasure[2] = 0;
                            }
                            if (z2) {
                                constraintWidget.wrapMeasure[1] = measuredHeight;
                                constraintWidget.wrapMeasure[3] = measuredWidth;
                            } else {
                                constraintWidget.wrapMeasure[1] = 0;
                                constraintWidget.wrapMeasure[3] = 0;
                            }
                            if (constraintWidget.mMatchConstraintMinWidth > 0) {
                                measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                            } else {
                                measuredWidth2 = measuredWidth;
                            }
                            if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                                measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                            }
                            if (constraintWidget.mMatchConstraintMinHeight > 0) {
                                measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                            } else {
                                measuredHeight2 = measuredHeight;
                            }
                            if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                                measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                            }
                            if (!z7) {
                                if (z8) {
                                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                                }
                            } else if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                            if (measuredWidth == measuredWidth2) {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            } else {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            }
                        }
                        if (baseline != -1) {
                            z9 = true;
                        } else {
                            z9 = false;
                        }
                        if (measuredWidth2 == measure.horizontalDimension) {
                            z10 = true;
                        } else {
                            z10 = true;
                        }
                        measure.measuredNeedsSolverPass = z10;
                        if (layoutParams.needsBaseline) {
                            z9 = true;
                        }
                        if (z9) {
                            measure.measuredNeedsSolverPass = true;
                        }
                        measure.measuredWidth = measuredWidth2;
                        measure.measuredHeight = measuredHeight2;
                        measure.measuredHasBaseline = z9;
                        measure.measuredBaseline = baseline;
                    }
                    z2 = false;
                    if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z3 = true;
                    } else {
                        z3 = false;
                    }
                    if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z4 = true;
                    } else {
                        z4 = false;
                    }
                    if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z5 = true;
                    } else {
                        z5 = true;
                    }
                    if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z6 = true;
                    } else {
                        z6 = true;
                    }
                    if (z3) {
                        z7 = false;
                    } else {
                        z7 = false;
                    }
                    if (z4) {
                        z8 = false;
                    } else {
                        z8 = false;
                    }
                    layoutParams = (LayoutParams) view.getLayoutParams();
                    if (measure.useCurrentDimensions) {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    } else {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    }
                    if (baseline != -1) {
                        z9 = true;
                    } else {
                        z9 = false;
                    }
                    if (measuredWidth2 == measure.horizontalDimension) {
                        z10 = true;
                    } else {
                        z10 = true;
                    }
                    measure.measuredNeedsSolverPass = z10;
                    if (layoutParams.needsBaseline) {
                        z9 = true;
                    }
                    if (z9) {
                        measure.measuredNeedsSolverPass = true;
                    }
                    measure.measuredWidth = measuredWidth2;
                    measure.measuredHeight = measuredHeight2;
                    measure.measuredHasBaseline = z9;
                    measure.measuredBaseline = baseline;
                }
                z = true;
                i = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[dimensionBehaviour2.ordinal()];
                if (i == 1) {
                    int iMakeMeasureSpec5 = View.MeasureSpec.makeMeasureSpec(i3, BasicMeasure.EXACTLY);
                    constraintWidget.wrapMeasure[3] = i3;
                    iMakeMeasureSpec2 = iMakeMeasureSpec5;
                } else {
                    if (i == 2) {
                        iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                        constraintWidget.wrapMeasure[3] = -2;
                    } else if (i == 3) {
                        iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4 + constraintWidget.getVerticalMargin(), -1);
                        constraintWidget.wrapMeasure[3] = -1;
                    } else {
                        if (i != 4) {
                            iMakeMeasureSpec2 = 0;
                            z2 = false;
                        } else {
                            iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                            if (constraintWidget.mMatchConstraintDefaultHeight == 1) {
                                z11 = true;
                            } else {
                                z11 = false;
                            }
                            constraintWidget.wrapMeasure[3] = 0;
                            if (measure.useCurrentDimensions) {
                                if (z11) {
                                }
                                if (z11) {
                                }
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(constraintWidget.getHeight(), BasicMeasure.EXACTLY);
                            }
                        }
                        if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                            z3 = true;
                        } else {
                            z3 = false;
                        }
                        if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                            z4 = true;
                        } else {
                            z4 = false;
                        }
                        if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                            z5 = true;
                        } else {
                            z5 = true;
                        }
                        if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                            z6 = true;
                        } else {
                            z6 = true;
                        }
                        if (z3) {
                            z7 = false;
                        } else {
                            z7 = false;
                        }
                        if (z4) {
                            z8 = false;
                        } else {
                            z8 = false;
                        }
                        layoutParams = (LayoutParams) view.getLayoutParams();
                        if (measure.useCurrentDimensions) {
                            if (!(view instanceof VirtualLayout)) {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            } else {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            }
                            measuredWidth = view.getMeasuredWidth();
                            measuredHeight = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                            if (z) {
                                constraintWidget.wrapMeasure[0] = measuredWidth;
                                constraintWidget.wrapMeasure[2] = measuredHeight;
                            } else {
                                constraintWidget.wrapMeasure[0] = 0;
                                constraintWidget.wrapMeasure[2] = 0;
                            }
                            if (z2) {
                                constraintWidget.wrapMeasure[1] = measuredHeight;
                                constraintWidget.wrapMeasure[3] = measuredWidth;
                            } else {
                                constraintWidget.wrapMeasure[1] = 0;
                                constraintWidget.wrapMeasure[3] = 0;
                            }
                            if (constraintWidget.mMatchConstraintMinWidth > 0) {
                                measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                            } else {
                                measuredWidth2 = measuredWidth;
                            }
                            if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                                measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                            }
                            if (constraintWidget.mMatchConstraintMinHeight > 0) {
                                measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                            } else {
                                measuredHeight2 = measuredHeight;
                            }
                            if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                                measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                            }
                            if (!z7) {
                                if (z8) {
                                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                                }
                            } else if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                            if (measuredWidth == measuredWidth2) {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            } else {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            }
                        } else {
                            if (!(view instanceof VirtualLayout)) {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            } else {
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            }
                            measuredWidth = view.getMeasuredWidth();
                            measuredHeight = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                            if (z) {
                                constraintWidget.wrapMeasure[0] = measuredWidth;
                                constraintWidget.wrapMeasure[2] = measuredHeight;
                            } else {
                                constraintWidget.wrapMeasure[0] = 0;
                                constraintWidget.wrapMeasure[2] = 0;
                            }
                            if (z2) {
                                constraintWidget.wrapMeasure[1] = measuredHeight;
                                constraintWidget.wrapMeasure[3] = measuredWidth;
                            } else {
                                constraintWidget.wrapMeasure[1] = 0;
                                constraintWidget.wrapMeasure[3] = 0;
                            }
                            if (constraintWidget.mMatchConstraintMinWidth > 0) {
                                measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                            } else {
                                measuredWidth2 = measuredWidth;
                            }
                            if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                                measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                            }
                            if (constraintWidget.mMatchConstraintMinHeight > 0) {
                                measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                            } else {
                                measuredHeight2 = measuredHeight;
                            }
                            if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                                measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                            }
                            if (!z7) {
                                if (z8) {
                                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                                }
                            } else if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                            if (measuredWidth == measuredWidth2) {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            } else {
                                if (measuredWidth != measuredWidth2) {
                                    iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                                }
                                if (measuredHeight != measuredHeight2) {
                                    iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                                }
                                view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                                measuredWidth2 = view.getMeasuredWidth();
                                measuredHeight2 = view.getMeasuredHeight();
                                baseline = view.getBaseline();
                            }
                        }
                        if (baseline != -1) {
                            z9 = true;
                        } else {
                            z9 = false;
                        }
                        if (measuredWidth2 == measure.horizontalDimension) {
                            z10 = true;
                        } else {
                            z10 = true;
                        }
                        measure.measuredNeedsSolverPass = z10;
                        if (layoutParams.needsBaseline) {
                            z9 = true;
                        }
                        if (z9) {
                            measure.measuredNeedsSolverPass = true;
                        }
                        measure.measuredWidth = measuredWidth2;
                        measure.measuredHeight = measuredHeight2;
                        measure.measuredHasBaseline = z9;
                        measure.measuredBaseline = baseline;
                    }
                    z2 = true;
                    if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z3 = true;
                    } else {
                        z3 = false;
                    }
                    if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z4 = true;
                    } else {
                        z4 = false;
                    }
                    if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z5 = true;
                    } else {
                        z5 = true;
                    }
                    if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z6 = true;
                    } else {
                        z6 = true;
                    }
                    if (z3) {
                        z7 = false;
                    } else {
                        z7 = false;
                    }
                    if (z4) {
                        z8 = false;
                    } else {
                        z8 = false;
                    }
                    layoutParams = (LayoutParams) view.getLayoutParams();
                    if (measure.useCurrentDimensions) {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    } else {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    }
                    if (baseline != -1) {
                        z9 = true;
                    } else {
                        z9 = false;
                    }
                    if (measuredWidth2 == measure.horizontalDimension) {
                        z10 = true;
                    } else {
                        z10 = true;
                    }
                    measure.measuredNeedsSolverPass = z10;
                    if (layoutParams.needsBaseline) {
                        z9 = true;
                    }
                    if (z9) {
                        measure.measuredNeedsSolverPass = true;
                    }
                    measure.measuredWidth = measuredWidth2;
                    measure.measuredHeight = measuredHeight2;
                    measure.measuredHasBaseline = z9;
                    measure.measuredBaseline = baseline;
                }
                z2 = false;
                if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                    z3 = true;
                } else {
                    z3 = false;
                }
                if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                    z4 = true;
                } else {
                    z4 = false;
                }
                if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                    z5 = true;
                } else {
                    z5 = true;
                }
                if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                    z6 = true;
                } else {
                    z6 = true;
                }
                if (z3) {
                    z7 = false;
                } else {
                    z7 = false;
                }
                if (z4) {
                    z8 = false;
                } else {
                    z8 = false;
                }
                layoutParams = (LayoutParams) view.getLayoutParams();
                if (measure.useCurrentDimensions) {
                    if (!(view instanceof VirtualLayout)) {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    } else {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    }
                    measuredWidth = view.getMeasuredWidth();
                    measuredHeight = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                    if (z) {
                        constraintWidget.wrapMeasure[0] = measuredWidth;
                        constraintWidget.wrapMeasure[2] = measuredHeight;
                    } else {
                        constraintWidget.wrapMeasure[0] = 0;
                        constraintWidget.wrapMeasure[2] = 0;
                    }
                    if (z2) {
                        constraintWidget.wrapMeasure[1] = measuredHeight;
                        constraintWidget.wrapMeasure[3] = measuredWidth;
                    } else {
                        constraintWidget.wrapMeasure[1] = 0;
                        constraintWidget.wrapMeasure[3] = 0;
                    }
                    if (constraintWidget.mMatchConstraintMinWidth > 0) {
                        measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                    } else {
                        measuredWidth2 = measuredWidth;
                    }
                    if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                        measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                    }
                    if (constraintWidget.mMatchConstraintMinHeight > 0) {
                        measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                    } else {
                        measuredHeight2 = measuredHeight;
                    }
                    if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                        measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                    }
                    if (!z7) {
                        if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                    } else if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                    if (measuredWidth == measuredWidth2) {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    } else {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    }
                } else {
                    if (!(view instanceof VirtualLayout)) {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    } else {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    }
                    measuredWidth = view.getMeasuredWidth();
                    measuredHeight = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                    if (z) {
                        constraintWidget.wrapMeasure[0] = measuredWidth;
                        constraintWidget.wrapMeasure[2] = measuredHeight;
                    } else {
                        constraintWidget.wrapMeasure[0] = 0;
                        constraintWidget.wrapMeasure[2] = 0;
                    }
                    if (z2) {
                        constraintWidget.wrapMeasure[1] = measuredHeight;
                        constraintWidget.wrapMeasure[3] = measuredWidth;
                    } else {
                        constraintWidget.wrapMeasure[1] = 0;
                        constraintWidget.wrapMeasure[3] = 0;
                    }
                    if (constraintWidget.mMatchConstraintMinWidth > 0) {
                        measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                    } else {
                        measuredWidth2 = measuredWidth;
                    }
                    if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                        measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                    }
                    if (constraintWidget.mMatchConstraintMinHeight > 0) {
                        measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                    } else {
                        measuredHeight2 = measuredHeight;
                    }
                    if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                        measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                    }
                    if (!z7) {
                        if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                    } else if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                    if (measuredWidth == measuredWidth2) {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    } else {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    }
                }
                if (baseline != -1) {
                    z9 = true;
                } else {
                    z9 = false;
                }
                if (measuredWidth2 == measure.horizontalDimension) {
                    z10 = true;
                } else {
                    z10 = true;
                }
                measure.measuredNeedsSolverPass = z10;
                if (layoutParams.needsBaseline) {
                    z9 = true;
                }
                if (z9) {
                    measure.measuredNeedsSolverPass = true;
                }
                measure.measuredWidth = measuredWidth2;
                measure.measuredHeight = measuredHeight2;
                measure.measuredHasBaseline = z9;
                measure.measuredBaseline = baseline;
            }
            z = false;
            i = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[dimensionBehaviour2.ordinal()];
            if (i == 1) {
                int iMakeMeasureSpec6 = View.MeasureSpec.makeMeasureSpec(i3, BasicMeasure.EXACTLY);
                constraintWidget.wrapMeasure[3] = i3;
                iMakeMeasureSpec2 = iMakeMeasureSpec6;
            } else {
                if (i == 2) {
                    iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                    constraintWidget.wrapMeasure[3] = -2;
                } else if (i == 3) {
                    iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4 + constraintWidget.getVerticalMargin(), -1);
                    constraintWidget.wrapMeasure[3] = -1;
                } else {
                    if (i != 4) {
                        iMakeMeasureSpec2 = 0;
                        z2 = false;
                    } else {
                        iMakeMeasureSpec2 = ViewGroup.getChildMeasureSpec(this.layoutHeightSpec, i4, -2);
                        if (constraintWidget.mMatchConstraintDefaultHeight == 1) {
                            z11 = true;
                        } else {
                            z11 = false;
                        }
                        constraintWidget.wrapMeasure[3] = 0;
                        if (measure.useCurrentDimensions) {
                            if (z11) {
                            }
                            if (z11) {
                            }
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(constraintWidget.getHeight(), BasicMeasure.EXACTLY);
                        }
                    }
                    if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z3 = true;
                    } else {
                        z3 = false;
                    }
                    if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                        z4 = true;
                    } else {
                        z4 = false;
                    }
                    if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z5 = true;
                    } else {
                        z5 = true;
                    }
                    if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                        z6 = true;
                    } else {
                        z6 = true;
                    }
                    if (z3) {
                        z7 = false;
                    } else {
                        z7 = false;
                    }
                    if (z4) {
                        z8 = false;
                    } else {
                        z8 = false;
                    }
                    layoutParams = (LayoutParams) view.getLayoutParams();
                    if (measure.useCurrentDimensions) {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    } else {
                        if (!(view instanceof VirtualLayout)) {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        } else {
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        }
                        measuredWidth = view.getMeasuredWidth();
                        measuredHeight = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                        if (z) {
                            constraintWidget.wrapMeasure[0] = measuredWidth;
                            constraintWidget.wrapMeasure[2] = measuredHeight;
                        } else {
                            constraintWidget.wrapMeasure[0] = 0;
                            constraintWidget.wrapMeasure[2] = 0;
                        }
                        if (z2) {
                            constraintWidget.wrapMeasure[1] = measuredHeight;
                            constraintWidget.wrapMeasure[3] = measuredWidth;
                        } else {
                            constraintWidget.wrapMeasure[1] = 0;
                            constraintWidget.wrapMeasure[3] = 0;
                        }
                        if (constraintWidget.mMatchConstraintMinWidth > 0) {
                            measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                        } else {
                            measuredWidth2 = measuredWidth;
                        }
                        if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                            measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                        }
                        if (constraintWidget.mMatchConstraintMinHeight > 0) {
                            measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                        } else {
                            measuredHeight2 = measuredHeight;
                        }
                        if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                            measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                        }
                        if (!z7) {
                            if (z8) {
                                measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                            }
                        } else if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                        if (measuredWidth == measuredWidth2) {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        } else {
                            if (measuredWidth != measuredWidth2) {
                                iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                            }
                            if (measuredHeight != measuredHeight2) {
                                iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                            }
                            view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                            measuredWidth2 = view.getMeasuredWidth();
                            measuredHeight2 = view.getMeasuredHeight();
                            baseline = view.getBaseline();
                        }
                    }
                    if (baseline != -1) {
                        z9 = true;
                    } else {
                        z9 = false;
                    }
                    if (measuredWidth2 == measure.horizontalDimension) {
                        z10 = true;
                    } else {
                        z10 = true;
                    }
                    measure.measuredNeedsSolverPass = z10;
                    if (layoutParams.needsBaseline) {
                        z9 = true;
                    }
                    if (z9) {
                        measure.measuredNeedsSolverPass = true;
                    }
                    measure.measuredWidth = measuredWidth2;
                    measure.measuredHeight = measuredHeight2;
                    measure.measuredHasBaseline = z9;
                    measure.measuredBaseline = baseline;
                }
                z2 = true;
                if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                    z3 = true;
                } else {
                    z3 = false;
                }
                if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                    z4 = true;
                } else {
                    z4 = false;
                }
                if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                    z5 = true;
                } else {
                    z5 = true;
                }
                if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                    z6 = true;
                } else {
                    z6 = true;
                }
                if (z3) {
                    z7 = false;
                } else {
                    z7 = false;
                }
                if (z4) {
                    z8 = false;
                } else {
                    z8 = false;
                }
                layoutParams = (LayoutParams) view.getLayoutParams();
                if (measure.useCurrentDimensions) {
                    if (!(view instanceof VirtualLayout)) {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    } else {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    }
                    measuredWidth = view.getMeasuredWidth();
                    measuredHeight = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                    if (z) {
                        constraintWidget.wrapMeasure[0] = measuredWidth;
                        constraintWidget.wrapMeasure[2] = measuredHeight;
                    } else {
                        constraintWidget.wrapMeasure[0] = 0;
                        constraintWidget.wrapMeasure[2] = 0;
                    }
                    if (z2) {
                        constraintWidget.wrapMeasure[1] = measuredHeight;
                        constraintWidget.wrapMeasure[3] = measuredWidth;
                    } else {
                        constraintWidget.wrapMeasure[1] = 0;
                        constraintWidget.wrapMeasure[3] = 0;
                    }
                    if (constraintWidget.mMatchConstraintMinWidth > 0) {
                        measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                    } else {
                        measuredWidth2 = measuredWidth;
                    }
                    if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                        measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                    }
                    if (constraintWidget.mMatchConstraintMinHeight > 0) {
                        measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                    } else {
                        measuredHeight2 = measuredHeight;
                    }
                    if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                        measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                    }
                    if (!z7) {
                        if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                    } else if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                    if (measuredWidth == measuredWidth2) {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    } else {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    }
                } else {
                    if (!(view instanceof VirtualLayout)) {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    } else {
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    }
                    measuredWidth = view.getMeasuredWidth();
                    measuredHeight = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                    if (z) {
                        constraintWidget.wrapMeasure[0] = measuredWidth;
                        constraintWidget.wrapMeasure[2] = measuredHeight;
                    } else {
                        constraintWidget.wrapMeasure[0] = 0;
                        constraintWidget.wrapMeasure[2] = 0;
                    }
                    if (z2) {
                        constraintWidget.wrapMeasure[1] = measuredHeight;
                        constraintWidget.wrapMeasure[3] = measuredWidth;
                    } else {
                        constraintWidget.wrapMeasure[1] = 0;
                        constraintWidget.wrapMeasure[3] = 0;
                    }
                    if (constraintWidget.mMatchConstraintMinWidth > 0) {
                        measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                    } else {
                        measuredWidth2 = measuredWidth;
                    }
                    if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                        measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                    }
                    if (constraintWidget.mMatchConstraintMinHeight > 0) {
                        measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                    } else {
                        measuredHeight2 = measuredHeight;
                    }
                    if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                        measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                    }
                    if (!z7) {
                        if (z8) {
                            measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                        }
                    } else if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                    if (measuredWidth == measuredWidth2) {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    } else {
                        if (measuredWidth != measuredWidth2) {
                            iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                        }
                        if (measuredHeight != measuredHeight2) {
                            iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                        }
                        view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                        measuredWidth2 = view.getMeasuredWidth();
                        measuredHeight2 = view.getMeasuredHeight();
                        baseline = view.getBaseline();
                    }
                }
                if (baseline != -1) {
                    z9 = true;
                } else {
                    z9 = false;
                }
                if (measuredWidth2 == measure.horizontalDimension) {
                    z10 = true;
                } else {
                    z10 = true;
                }
                measure.measuredNeedsSolverPass = z10;
                if (layoutParams.needsBaseline) {
                    z9 = true;
                }
                if (z9) {
                    measure.measuredNeedsSolverPass = true;
                }
                measure.measuredWidth = measuredWidth2;
                measure.measuredHeight = measuredHeight2;
                measure.measuredHasBaseline = z9;
                measure.measuredBaseline = baseline;
            }
            z2 = false;
            if (dimensionBehaviour == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                z3 = true;
            } else {
                z3 = false;
            }
            if (dimensionBehaviour2 == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                z4 = true;
            } else {
                z4 = false;
            }
            if (dimensionBehaviour2 != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                z5 = true;
            } else {
                z5 = true;
            }
            if (dimensionBehaviour != ConstraintWidget.DimensionBehaviour.MATCH_PARENT) {
                z6 = true;
            } else {
                z6 = true;
            }
            if (z3) {
                z7 = false;
            } else {
                z7 = false;
            }
            if (z4) {
                z8 = false;
            } else {
                z8 = false;
            }
            layoutParams = (LayoutParams) view.getLayoutParams();
            if (measure.useCurrentDimensions) {
                if (!(view instanceof VirtualLayout)) {
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                } else {
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                }
                measuredWidth = view.getMeasuredWidth();
                measuredHeight = view.getMeasuredHeight();
                baseline = view.getBaseline();
                if (z) {
                    constraintWidget.wrapMeasure[0] = measuredWidth;
                    constraintWidget.wrapMeasure[2] = measuredHeight;
                } else {
                    constraintWidget.wrapMeasure[0] = 0;
                    constraintWidget.wrapMeasure[2] = 0;
                }
                if (z2) {
                    constraintWidget.wrapMeasure[1] = measuredHeight;
                    constraintWidget.wrapMeasure[3] = measuredWidth;
                } else {
                    constraintWidget.wrapMeasure[1] = 0;
                    constraintWidget.wrapMeasure[3] = 0;
                }
                if (constraintWidget.mMatchConstraintMinWidth > 0) {
                    measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                } else {
                    measuredWidth2 = measuredWidth;
                }
                if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                    measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                }
                if (constraintWidget.mMatchConstraintMinHeight > 0) {
                    measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                } else {
                    measuredHeight2 = measuredHeight;
                }
                if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                    measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                }
                if (!z7) {
                    if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                } else if (z8) {
                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                }
                if (measuredWidth == measuredWidth2) {
                    if (measuredWidth != measuredWidth2) {
                        iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                    }
                    if (measuredHeight != measuredHeight2) {
                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                    }
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    measuredWidth2 = view.getMeasuredWidth();
                    measuredHeight2 = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                } else {
                    if (measuredWidth != measuredWidth2) {
                        iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                    }
                    if (measuredHeight != measuredHeight2) {
                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                    }
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    measuredWidth2 = view.getMeasuredWidth();
                    measuredHeight2 = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                }
            } else {
                if (!(view instanceof VirtualLayout)) {
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                } else {
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                }
                measuredWidth = view.getMeasuredWidth();
                measuredHeight = view.getMeasuredHeight();
                baseline = view.getBaseline();
                if (z) {
                    constraintWidget.wrapMeasure[0] = measuredWidth;
                    constraintWidget.wrapMeasure[2] = measuredHeight;
                } else {
                    constraintWidget.wrapMeasure[0] = 0;
                    constraintWidget.wrapMeasure[2] = 0;
                }
                if (z2) {
                    constraintWidget.wrapMeasure[1] = measuredHeight;
                    constraintWidget.wrapMeasure[3] = measuredWidth;
                } else {
                    constraintWidget.wrapMeasure[1] = 0;
                    constraintWidget.wrapMeasure[3] = 0;
                }
                if (constraintWidget.mMatchConstraintMinWidth > 0) {
                    measuredWidth2 = Math.max(constraintWidget.mMatchConstraintMinWidth, measuredWidth);
                } else {
                    measuredWidth2 = measuredWidth;
                }
                if (constraintWidget.mMatchConstraintMaxWidth > 0) {
                    measuredWidth2 = Math.min(constraintWidget.mMatchConstraintMaxWidth, measuredWidth2);
                }
                if (constraintWidget.mMatchConstraintMinHeight > 0) {
                    measuredHeight2 = Math.max(constraintWidget.mMatchConstraintMinHeight, measuredHeight);
                } else {
                    measuredHeight2 = measuredHeight;
                }
                if (constraintWidget.mMatchConstraintMaxHeight > 0) {
                    measuredHeight2 = Math.min(constraintWidget.mMatchConstraintMaxHeight, measuredHeight2);
                }
                if (!z7) {
                    if (z8) {
                        measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                    }
                } else if (z8) {
                    measuredHeight2 = (int) ((measuredWidth2 / constraintWidget.mDimensionRatio) + 0.5f);
                }
                if (measuredWidth == measuredWidth2) {
                    if (measuredWidth != measuredWidth2) {
                        iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                    }
                    if (measuredHeight != measuredHeight2) {
                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                    }
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    measuredWidth2 = view.getMeasuredWidth();
                    measuredHeight2 = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                } else {
                    if (measuredWidth != measuredWidth2) {
                        iMakeMeasureSpec = View.MeasureSpec.makeMeasureSpec(measuredWidth2, BasicMeasure.EXACTLY);
                    }
                    if (measuredHeight != measuredHeight2) {
                        iMakeMeasureSpec2 = View.MeasureSpec.makeMeasureSpec(measuredHeight2, BasicMeasure.EXACTLY);
                    }
                    view.measure(iMakeMeasureSpec, iMakeMeasureSpec2);
                    measuredWidth2 = view.getMeasuredWidth();
                    measuredHeight2 = view.getMeasuredHeight();
                    baseline = view.getBaseline();
                }
            }
            if (baseline != -1) {
                z9 = true;
            } else {
                z9 = false;
            }
            if (measuredWidth2 == measure.horizontalDimension) {
                z10 = true;
            } else {
                z10 = true;
            }
            measure.measuredNeedsSolverPass = z10;
            if (layoutParams.needsBaseline) {
                z9 = true;
            }
            if (z9) {
                measure.measuredNeedsSolverPass = true;
            }
            measure.measuredWidth = measuredWidth2;
            measure.measuredHeight = measuredHeight2;
            measure.measuredHasBaseline = z9;
            measure.measuredBaseline = baseline;
        }

        @Override // androidx.constraintlayout.solver.widgets.analyzer.BasicMeasure.Measurer
        public final void didMeasures() {
            int childCount = this.layout.getChildCount();
            for (int i = 0; i < childCount; i++) {
                View childAt = this.layout.getChildAt(i);
                if (childAt instanceof Placeholder) {
                    ((Placeholder) childAt).updatePostMeasure(this.layout);
                }
            }
            int size = this.layout.mConstraintHelpers.size();
            if (size > 0) {
                for (int i2 = 0; i2 < size; i2++) {
                    ((ConstraintHelper) this.layout.mConstraintHelpers.get(i2)).updatePostMeasure(this.layout);
                }
            }
        }
    }

    /* JADX INFO: renamed from: androidx.constraintlayout.widget.ConstraintLayout$1, reason: invalid class name */
    static /* synthetic */ class AnonymousClass1 {
        static final /* synthetic */ int[] $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour;

        static {
            int[] iArr = new int[ConstraintWidget.DimensionBehaviour.values().length];
            $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour = iArr;
            try {
                iArr[ConstraintWidget.DimensionBehaviour.FIXED.ordinal()] = 1;
            } catch (NoSuchFieldError unused) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[ConstraintWidget.DimensionBehaviour.WRAP_CONTENT.ordinal()] = 2;
            } catch (NoSuchFieldError unused2) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[ConstraintWidget.DimensionBehaviour.MATCH_PARENT.ordinal()] = 3;
            } catch (NoSuchFieldError unused3) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT.ordinal()] = 4;
            } catch (NoSuchFieldError unused4) {
            }
        }
    }

    private void init(AttributeSet attributeSet, int i, int i2) {
        this.mLayoutWidget.setCompanionWidget(this);
        this.mLayoutWidget.setMeasurer(this.mMeasurer);
        this.mChildrenByIds.put(getId(), this);
        this.mConstraintSet = null;
        if (attributeSet != null) {
            TypedArray typedArrayObtainStyledAttributes = getContext().obtainStyledAttributes(attributeSet, R.styleable.ConstraintLayout_Layout, i, i2);
            int indexCount = typedArrayObtainStyledAttributes.getIndexCount();
            for (int i3 = 0; i3 < indexCount; i3++) {
                int index = typedArrayObtainStyledAttributes.getIndex(i3);
                if (index == R.styleable.ConstraintLayout_Layout_android_minWidth) {
                    this.mMinWidth = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.mMinWidth);
                } else if (index == R.styleable.ConstraintLayout_Layout_android_minHeight) {
                    this.mMinHeight = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.mMinHeight);
                } else if (index == R.styleable.ConstraintLayout_Layout_android_maxWidth) {
                    this.mMaxWidth = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.mMaxWidth);
                } else if (index == R.styleable.ConstraintLayout_Layout_android_maxHeight) {
                    this.mMaxHeight = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.mMaxHeight);
                } else if (index == R.styleable.ConstraintLayout_Layout_layout_optimizationLevel) {
                    this.mOptimizationLevel = typedArrayObtainStyledAttributes.getInt(index, this.mOptimizationLevel);
                } else if (index == R.styleable.ConstraintLayout_Layout_layoutDescription) {
                    int resourceId = typedArrayObtainStyledAttributes.getResourceId(index, 0);
                    if (resourceId != 0) {
                        try {
                            parseLayoutDescription(resourceId);
                        } catch (Resources.NotFoundException unused) {
                            this.mConstraintLayoutSpec = null;
                        }
                    }
                } else if (index == R.styleable.ConstraintLayout_Layout_constraintSet) {
                    int resourceId2 = typedArrayObtainStyledAttributes.getResourceId(index, 0);
                    try {
                        ConstraintSet constraintSet = new ConstraintSet();
                        this.mConstraintSet = constraintSet;
                        constraintSet.load(getContext(), resourceId2);
                    } catch (Resources.NotFoundException unused2) {
                        this.mConstraintSet = null;
                    }
                    this.mConstraintSetId = resourceId2;
                }
            }
            typedArrayObtainStyledAttributes.recycle();
        }
        this.mLayoutWidget.setOptimizationLevel(this.mOptimizationLevel);
    }

    protected void parseLayoutDescription(int i) {
        this.mConstraintLayoutSpec = new ConstraintLayoutStates(getContext(), this, i);
    }

    @Override // android.view.ViewGroup
    public void addView(View view, int i, ViewGroup.LayoutParams layoutParams) {
        super.addView(view, i, layoutParams);
    }

    @Override // android.view.ViewGroup, android.view.ViewManager
    public void removeView(View view) {
        super.removeView(view);
    }

    @Override // android.view.ViewGroup
    public void onViewAdded(View view) {
        super.onViewAdded(view);
        ConstraintWidget viewWidget = getViewWidget(view);
        if ((view instanceof Guideline) && !(viewWidget instanceof androidx.constraintlayout.solver.widgets.Guideline)) {
            LayoutParams layoutParams = (LayoutParams) view.getLayoutParams();
            layoutParams.widget = new androidx.constraintlayout.solver.widgets.Guideline();
            layoutParams.isGuideline = true;
            ((androidx.constraintlayout.solver.widgets.Guideline) layoutParams.widget).setOrientation(layoutParams.orientation);
        }
        if (view instanceof ConstraintHelper) {
            ConstraintHelper constraintHelper = (ConstraintHelper) view;
            constraintHelper.validateParams();
            ((LayoutParams) view.getLayoutParams()).isHelper = true;
            if (!this.mConstraintHelpers.contains(constraintHelper)) {
                this.mConstraintHelpers.add(constraintHelper);
            }
        }
        this.mChildrenByIds.put(view.getId(), view);
        this.mDirtyHierarchy = true;
    }

    @Override // android.view.ViewGroup
    public void onViewRemoved(View view) {
        super.onViewRemoved(view);
        this.mChildrenByIds.remove(view.getId());
        this.mLayoutWidget.remove(getViewWidget(view));
        this.mConstraintHelpers.remove(view);
        this.mDirtyHierarchy = true;
    }

    public void setMinWidth(int i) {
        if (i == this.mMinWidth) {
            return;
        }
        this.mMinWidth = i;
        requestLayout();
    }

    public void setMinHeight(int i) {
        if (i == this.mMinHeight) {
            return;
        }
        this.mMinHeight = i;
        requestLayout();
    }

    public int getMinWidth() {
        return this.mMinWidth;
    }

    public int getMinHeight() {
        return this.mMinHeight;
    }

    public void setMaxWidth(int i) {
        if (i == this.mMaxWidth) {
            return;
        }
        this.mMaxWidth = i;
        requestLayout();
    }

    public void setMaxHeight(int i) {
        if (i == this.mMaxHeight) {
            return;
        }
        this.mMaxHeight = i;
        requestLayout();
    }

    public int getMaxWidth() {
        return this.mMaxWidth;
    }

    public int getMaxHeight() {
        return this.mMaxHeight;
    }

    private boolean updateHierarchy() {
        int childCount = getChildCount();
        boolean z = false;
        for (int i = 0; i < childCount; i++) {
            if (getChildAt(i).isLayoutRequested()) {
                z = true;
                break;
            }
        }
        if (z) {
            setChildrenConstraints();
        }
        return z;
    }

    private void setChildrenConstraints() {
        boolean zIsInEditMode = isInEditMode();
        int childCount = getChildCount();
        for (int i = 0; i < childCount; i++) {
            ConstraintWidget viewWidget = getViewWidget(getChildAt(i));
            if (viewWidget != null) {
                viewWidget.reset();
            }
        }
        if (zIsInEditMode) {
            for (int i2 = 0; i2 < childCount; i2++) {
                View childAt = getChildAt(i2);
                try {
                    String resourceName = getResources().getResourceName(childAt.getId());
                    setDesignInformation(0, resourceName, Integer.valueOf(childAt.getId()));
                    int iIndexOf = resourceName.indexOf(47);
                    if (iIndexOf != -1) {
                        resourceName = resourceName.substring(iIndexOf + 1);
                    }
                    getTargetWidget(childAt.getId()).setDebugName(resourceName);
                } catch (Resources.NotFoundException unused) {
                }
            }
        }
        if (this.mConstraintSetId != -1) {
            for (int i3 = 0; i3 < childCount; i3++) {
                View childAt2 = getChildAt(i3);
                if (childAt2.getId() == this.mConstraintSetId && (childAt2 instanceof Constraints)) {
                    this.mConstraintSet = ((Constraints) childAt2).getConstraintSet();
                }
            }
        }
        ConstraintSet constraintSet = this.mConstraintSet;
        if (constraintSet != null) {
            constraintSet.applyToInternal(this, true);
        }
        this.mLayoutWidget.removeAllChildren();
        int size = this.mConstraintHelpers.size();
        if (size > 0) {
            for (int i4 = 0; i4 < size; i4++) {
                this.mConstraintHelpers.get(i4).updatePreLayout(this);
            }
        }
        for (int i5 = 0; i5 < childCount; i5++) {
            View childAt3 = getChildAt(i5);
            if (childAt3 instanceof Placeholder) {
                ((Placeholder) childAt3).updatePreLayout(this);
            }
        }
        this.mTempMapIdToWidget.clear();
        this.mTempMapIdToWidget.put(0, this.mLayoutWidget);
        this.mTempMapIdToWidget.put(getId(), this.mLayoutWidget);
        for (int i6 = 0; i6 < childCount; i6++) {
            View childAt4 = getChildAt(i6);
            this.mTempMapIdToWidget.put(childAt4.getId(), getViewWidget(childAt4));
        }
        for (int i7 = 0; i7 < childCount; i7++) {
            View childAt5 = getChildAt(i7);
            ConstraintWidget viewWidget2 = getViewWidget(childAt5);
            if (viewWidget2 != null) {
                LayoutParams layoutParams = (LayoutParams) childAt5.getLayoutParams();
                this.mLayoutWidget.add(viewWidget2);
                applyConstraintsFromLayoutParams(zIsInEditMode, childAt5, viewWidget2, layoutParams, this.mTempMapIdToWidget);
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: protected */
    /* JADX WARN: Code duplicated, block: B:66:0x0155  */
    /* JADX WARN: Code duplicated, block: B:74:0x01ab  */
    /* JADX WARN: Code duplicated, block: B:77:0x01b4  */
    public void applyConstraintsFromLayoutParams(boolean z, View view, ConstraintWidget constraintWidget, LayoutParams layoutParams, SparseArray<ConstraintWidget> sparseArray) {
        ConstraintWidget constraintWidget2;
        ConstraintWidget constraintWidget3;
        ConstraintWidget constraintWidget4;
        ConstraintWidget constraintWidget5;
        ConstraintWidget constraintWidget6;
        View view2;
        ConstraintWidget constraintWidget7;
        layoutParams.validate();
        layoutParams.helped = false;
        constraintWidget.setVisibility(view.getVisibility());
        if (layoutParams.isInPlaceholder) {
            constraintWidget.setInPlaceholder(true);
            constraintWidget.setVisibility(8);
        }
        constraintWidget.setCompanionWidget(view);
        if (view instanceof ConstraintHelper) {
            ((ConstraintHelper) view).resolveRtl(constraintWidget, this.mLayoutWidget.isRtl());
        }
        if (layoutParams.isGuideline) {
            androidx.constraintlayout.solver.widgets.Guideline guideline = (androidx.constraintlayout.solver.widgets.Guideline) constraintWidget;
            int i = layoutParams.resolvedGuideBegin;
            int i2 = layoutParams.resolvedGuideEnd;
            float f = layoutParams.resolvedGuidePercent;
            if (f != -1.0f) {
                guideline.setGuidePercent(f);
                return;
            } else if (i != -1) {
                guideline.setGuideBegin(i);
                return;
            } else {
                if (i2 != -1) {
                    guideline.setGuideEnd(i2);
                    return;
                }
                return;
            }
        }
        int i3 = layoutParams.resolvedLeftToLeft;
        int i4 = layoutParams.resolvedLeftToRight;
        int i5 = layoutParams.resolvedRightToLeft;
        int i6 = layoutParams.resolvedRightToRight;
        int i7 = layoutParams.resolveGoneLeftMargin;
        int i8 = layoutParams.resolveGoneRightMargin;
        float f2 = layoutParams.resolvedHorizontalBias;
        if (layoutParams.circleConstraint != -1) {
            ConstraintWidget constraintWidget8 = sparseArray.get(layoutParams.circleConstraint);
            if (constraintWidget8 != null) {
                constraintWidget.connectCircularConstraint(constraintWidget8, layoutParams.circleAngle, layoutParams.circleRadius);
            }
            constraintWidget6 = constraintWidget;
        } else {
            if (i3 != -1) {
                ConstraintWidget constraintWidget9 = sparseArray.get(i3);
                if (constraintWidget9 != null) {
                    constraintWidget.immediateConnect(ConstraintAnchor.Type.LEFT, constraintWidget9, ConstraintAnchor.Type.LEFT, layoutParams.leftMargin, i7);
                }
            } else if (i4 != -1 && (constraintWidget2 = sparseArray.get(i4)) != null) {
                constraintWidget.immediateConnect(ConstraintAnchor.Type.LEFT, constraintWidget2, ConstraintAnchor.Type.RIGHT, layoutParams.leftMargin, i7);
            }
            if (i5 != -1) {
                ConstraintWidget constraintWidget10 = sparseArray.get(i5);
                if (constraintWidget10 != null) {
                    constraintWidget.immediateConnect(ConstraintAnchor.Type.RIGHT, constraintWidget10, ConstraintAnchor.Type.LEFT, layoutParams.rightMargin, i8);
                }
            } else if (i6 != -1 && (constraintWidget3 = sparseArray.get(i6)) != null) {
                constraintWidget.immediateConnect(ConstraintAnchor.Type.RIGHT, constraintWidget3, ConstraintAnchor.Type.RIGHT, layoutParams.rightMargin, i8);
            }
            if (layoutParams.topToTop != -1) {
                ConstraintWidget constraintWidget11 = sparseArray.get(layoutParams.topToTop);
                if (constraintWidget11 != null) {
                    constraintWidget.immediateConnect(ConstraintAnchor.Type.TOP, constraintWidget11, ConstraintAnchor.Type.TOP, layoutParams.topMargin, layoutParams.goneTopMargin);
                }
            } else if (layoutParams.topToBottom != -1 && (constraintWidget4 = sparseArray.get(layoutParams.topToBottom)) != null) {
                constraintWidget.immediateConnect(ConstraintAnchor.Type.TOP, constraintWidget4, ConstraintAnchor.Type.BOTTOM, layoutParams.topMargin, layoutParams.goneTopMargin);
            }
            if (layoutParams.bottomToTop != -1) {
                ConstraintWidget constraintWidget12 = sparseArray.get(layoutParams.bottomToTop);
                if (constraintWidget12 != null) {
                    constraintWidget.immediateConnect(ConstraintAnchor.Type.BOTTOM, constraintWidget12, ConstraintAnchor.Type.TOP, layoutParams.bottomMargin, layoutParams.goneBottomMargin);
                }
            } else {
                if (layoutParams.bottomToBottom != -1 && (constraintWidget5 = sparseArray.get(layoutParams.bottomToBottom)) != null) {
                    constraintWidget6 = constraintWidget;
                    constraintWidget6.immediateConnect(ConstraintAnchor.Type.BOTTOM, constraintWidget5, ConstraintAnchor.Type.BOTTOM, layoutParams.bottomMargin, layoutParams.goneBottomMargin);
                }
                if (layoutParams.baselineToBaseline != -1) {
                    view2 = this.mChildrenByIds.get(layoutParams.baselineToBaseline);
                    constraintWidget7 = sparseArray.get(layoutParams.baselineToBaseline);
                    if (constraintWidget7 != null && view2 != null && (view2.getLayoutParams() instanceof LayoutParams)) {
                        LayoutParams layoutParams2 = (LayoutParams) view2.getLayoutParams();
                        layoutParams.needsBaseline = true;
                        layoutParams2.needsBaseline = true;
                        constraintWidget6.getAnchor(ConstraintAnchor.Type.BASELINE).connect(constraintWidget7.getAnchor(ConstraintAnchor.Type.BASELINE), 0, -1, true);
                        constraintWidget6.setHasBaseline(true);
                        layoutParams2.widget.setHasBaseline(true);
                        constraintWidget6.getAnchor(ConstraintAnchor.Type.TOP).reset();
                        constraintWidget6.getAnchor(ConstraintAnchor.Type.BOTTOM).reset();
                    }
                }
                if (f2 >= 0.0f) {
                    constraintWidget6.setHorizontalBiasPercent(f2);
                }
                if (layoutParams.verticalBias >= 0.0f) {
                    constraintWidget6.setVerticalBiasPercent(layoutParams.verticalBias);
                }
            }
            constraintWidget6 = constraintWidget;
            if (layoutParams.baselineToBaseline != -1) {
                view2 = this.mChildrenByIds.get(layoutParams.baselineToBaseline);
                constraintWidget7 = sparseArray.get(layoutParams.baselineToBaseline);
                if (constraintWidget7 != null) {
                    LayoutParams layoutParams3 = (LayoutParams) view2.getLayoutParams();
                    layoutParams.needsBaseline = true;
                    layoutParams3.needsBaseline = true;
                    constraintWidget6.getAnchor(ConstraintAnchor.Type.BASELINE).connect(constraintWidget7.getAnchor(ConstraintAnchor.Type.BASELINE), 0, -1, true);
                    constraintWidget6.setHasBaseline(true);
                    layoutParams3.widget.setHasBaseline(true);
                    constraintWidget6.getAnchor(ConstraintAnchor.Type.TOP).reset();
                    constraintWidget6.getAnchor(ConstraintAnchor.Type.BOTTOM).reset();
                }
            }
            if (f2 >= 0.0f) {
                constraintWidget6.setHorizontalBiasPercent(f2);
            }
            if (layoutParams.verticalBias >= 0.0f) {
                constraintWidget6.setVerticalBiasPercent(layoutParams.verticalBias);
            }
        }
        if (z && (layoutParams.editorAbsoluteX != -1 || layoutParams.editorAbsoluteY != -1)) {
            constraintWidget6.setOrigin(layoutParams.editorAbsoluteX, layoutParams.editorAbsoluteY);
        }
        if (!layoutParams.horizontalDimensionFixed) {
            if (layoutParams.width == -1) {
                if (layoutParams.constrainedWidth) {
                    constraintWidget6.setHorizontalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT);
                } else {
                    constraintWidget6.setHorizontalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_PARENT);
                }
                constraintWidget6.getAnchor(ConstraintAnchor.Type.LEFT).mMargin = layoutParams.leftMargin;
                constraintWidget6.getAnchor(ConstraintAnchor.Type.RIGHT).mMargin = layoutParams.rightMargin;
            } else {
                constraintWidget6.setHorizontalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT);
                constraintWidget6.setWidth(0);
            }
        } else {
            constraintWidget6.setHorizontalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.FIXED);
            constraintWidget6.setWidth(layoutParams.width);
            if (layoutParams.width == -2) {
                constraintWidget6.setHorizontalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.WRAP_CONTENT);
            }
        }
        if (!layoutParams.verticalDimensionFixed) {
            if (layoutParams.height == -1) {
                if (layoutParams.constrainedHeight) {
                    constraintWidget6.setVerticalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT);
                } else {
                    constraintWidget6.setVerticalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_PARENT);
                }
                constraintWidget6.getAnchor(ConstraintAnchor.Type.TOP).mMargin = layoutParams.topMargin;
                constraintWidget6.getAnchor(ConstraintAnchor.Type.BOTTOM).mMargin = layoutParams.bottomMargin;
            } else {
                constraintWidget6.setVerticalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT);
                constraintWidget6.setHeight(0);
            }
        } else {
            constraintWidget6.setVerticalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.FIXED);
            constraintWidget6.setHeight(layoutParams.height);
            if (layoutParams.height == -2) {
                constraintWidget6.setVerticalDimensionBehaviour(ConstraintWidget.DimensionBehaviour.WRAP_CONTENT);
            }
        }
        constraintWidget6.setDimensionRatio(layoutParams.dimensionRatio);
        constraintWidget6.setHorizontalWeight(layoutParams.horizontalWeight);
        constraintWidget6.setVerticalWeight(layoutParams.verticalWeight);
        constraintWidget6.setHorizontalChainStyle(layoutParams.horizontalChainStyle);
        constraintWidget6.setVerticalChainStyle(layoutParams.verticalChainStyle);
        constraintWidget6.setHorizontalMatchStyle(layoutParams.matchConstraintDefaultWidth, layoutParams.matchConstraintMinWidth, layoutParams.matchConstraintMaxWidth, layoutParams.matchConstraintPercentWidth);
        constraintWidget6.setVerticalMatchStyle(layoutParams.matchConstraintDefaultHeight, layoutParams.matchConstraintMinHeight, layoutParams.matchConstraintMaxHeight, layoutParams.matchConstraintPercentHeight);
    }

    private final ConstraintWidget getTargetWidget(int i) {
        if (i == 0) {
            return this.mLayoutWidget;
        }
        View viewFindViewById = this.mChildrenByIds.get(i);
        if (viewFindViewById == null && (viewFindViewById = findViewById(i)) != null && viewFindViewById != this && viewFindViewById.getParent() == this) {
            onViewAdded(viewFindViewById);
        }
        if (viewFindViewById == this) {
            return this.mLayoutWidget;
        }
        if (viewFindViewById == null) {
            return null;
        }
        return ((LayoutParams) viewFindViewById.getLayoutParams()).widget;
    }

    public final ConstraintWidget getViewWidget(View view) {
        if (view == this) {
            return this.mLayoutWidget;
        }
        if (view == null) {
            return null;
        }
        return ((LayoutParams) view.getLayoutParams()).widget;
    }

    public void fillMetrics(Metrics metrics) {
        this.mMetrics = metrics;
        this.mLayoutWidget.fillMetrics(metrics);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    public void resolveSystem(ConstraintWidgetContainer constraintWidgetContainer, int i, int i2, int i3) {
        int i4;
        int mode = View.MeasureSpec.getMode(i2);
        int size = View.MeasureSpec.getSize(i2);
        int mode2 = View.MeasureSpec.getMode(i3);
        int size2 = View.MeasureSpec.getSize(i3);
        int iMax = Math.max(0, getPaddingTop());
        int iMax2 = Math.max(0, getPaddingBottom());
        int i5 = iMax + iMax2;
        int paddingWidth = getPaddingWidth();
        this.mMeasurer.captureLayoutInfos(i2, i3, iMax, iMax2, paddingWidth, i5);
        int iMax3 = Math.max(0, getPaddingStart());
        int iMax4 = Math.max(0, getPaddingEnd());
        if (iMax3 > 0 || iMax4 > 0) {
            if (isRtl()) {
                i4 = iMax4;
            }
            int i6 = size - paddingWidth;
            int i7 = size2 - i5;
            setSelfDimensionBehaviour(constraintWidgetContainer, mode, i6, mode2, i7);
            constraintWidgetContainer.measure(i, mode, i6, mode2, i7, this.mLastMeasureWidth, this.mLastMeasureHeight, i4, iMax);
        }
        iMax3 = Math.max(0, getPaddingLeft());
        i4 = iMax3;
        int i8 = size - paddingWidth;
        int i9 = size2 - i5;
        setSelfDimensionBehaviour(constraintWidgetContainer, mode, i8, mode2, i9);
        constraintWidgetContainer.measure(i, mode, i8, mode2, i9, this.mLastMeasureWidth, this.mLastMeasureHeight, i4, iMax);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    public void resolveMeasuredDimension(int i, int i2, int i3, int i4, boolean z, boolean z2) {
        int i5 = this.mMeasurer.paddingHeight;
        int iResolveSizeAndState = resolveSizeAndState(i3 + this.mMeasurer.paddingWidth, i, 0);
        int iResolveSizeAndState2 = resolveSizeAndState(i4 + i5, i2, 0);
        int i6 = iResolveSizeAndState & ViewCompat.MEASURED_SIZE_MASK;
        int i7 = iResolveSizeAndState2 & ViewCompat.MEASURED_SIZE_MASK;
        int iMin = Math.min(this.mMaxWidth, i6);
        int iMin2 = Math.min(this.mMaxHeight, i7);
        if (z) {
            iMin |= 16777216;
        }
        if (z2) {
            iMin2 |= 16777216;
        }
        setMeasuredDimension(iMin, iMin2);
        this.mLastMeasureWidth = iMin;
        this.mLastMeasureHeight = iMin2;
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        this.mOnMeasureWidthMeasureSpec = i;
        this.mOnMeasureHeightMeasureSpec = i2;
        this.mLayoutWidget.setRtl(isRtl());
        if (this.mDirtyHierarchy) {
            this.mDirtyHierarchy = false;
            if (updateHierarchy()) {
                this.mLayoutWidget.updateHierarchy();
            }
        }
        resolveSystem(this.mLayoutWidget, this.mOptimizationLevel, i, i2);
        resolveMeasuredDimension(i, i2, this.mLayoutWidget.getWidth(), this.mLayoutWidget.getHeight(), this.mLayoutWidget.isWidthMeasuredTooSmall(), this.mLayoutWidget.isHeightMeasuredTooSmall());
    }

    /* JADX INFO: Access modifiers changed from: protected */
    public boolean isRtl() {
        return (getContext().getApplicationInfo().flags & 4194304) != 0 && 1 == getLayoutDirection();
    }

    private int getPaddingWidth() {
        int iMax = Math.max(0, getPaddingLeft()) + Math.max(0, getPaddingRight());
        int iMax2 = Math.max(0, getPaddingStart()) + Math.max(0, getPaddingEnd());
        return iMax2 > 0 ? iMax2 : iMax;
    }

    /* JADX WARN: Code duplicated, block: B:17:0x0040 A[PHI: r3
  0x0040: PHI (r3v4 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour) = 
  (r3v3 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour)
  (r3v0 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour)
 binds: [B:20:0x004c, B:16:0x003e] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:6:0x001b A[PHI: r2
  0x001b: PHI (r2v4 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour) = 
  (r2v3 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour)
  (r2v0 androidx.constraintlayout.solver.widgets.ConstraintWidget$DimensionBehaviour)
 binds: [B:9:0x0027, B:5:0x0019] A[DONT_GENERATE, DONT_INLINE]] */
    protected void setSelfDimensionBehaviour(ConstraintWidgetContainer constraintWidgetContainer, int i, int i2, int i3, int i4) {
        int i5 = this.mMeasurer.paddingHeight;
        int i6 = this.mMeasurer.paddingWidth;
        ConstraintWidget.DimensionBehaviour dimensionBehaviour = ConstraintWidget.DimensionBehaviour.FIXED;
        ConstraintWidget.DimensionBehaviour dimensionBehaviour2 = ConstraintWidget.DimensionBehaviour.FIXED;
        int childCount = getChildCount();
        if (i == Integer.MIN_VALUE) {
            dimensionBehaviour = ConstraintWidget.DimensionBehaviour.WRAP_CONTENT;
            if (childCount == 0) {
                i2 = Math.max(0, this.mMinWidth);
            }
        } else if (i == 0) {
            dimensionBehaviour = ConstraintWidget.DimensionBehaviour.WRAP_CONTENT;
            if (childCount == 0) {
                i2 = Math.max(0, this.mMinWidth);
            } else {
                i2 = 0;
            }
        } else if (i != 1073741824) {
            i2 = 0;
        } else {
            i2 = Math.min(this.mMaxWidth - i6, i2);
        }
        if (i3 == Integer.MIN_VALUE) {
            dimensionBehaviour2 = ConstraintWidget.DimensionBehaviour.WRAP_CONTENT;
            if (childCount == 0) {
                i4 = Math.max(0, this.mMinHeight);
            }
        } else if (i3 == 0) {
            dimensionBehaviour2 = ConstraintWidget.DimensionBehaviour.WRAP_CONTENT;
            if (childCount == 0) {
                i4 = Math.max(0, this.mMinHeight);
            } else {
                i4 = 0;
            }
        } else if (i3 != 1073741824) {
            i4 = 0;
        } else {
            i4 = Math.min(this.mMaxHeight - i5, i4);
        }
        if (i2 != constraintWidgetContainer.getWidth() || i4 != constraintWidgetContainer.getHeight()) {
            constraintWidgetContainer.invalidateMeasures();
        }
        constraintWidgetContainer.setX(0);
        constraintWidgetContainer.setY(0);
        constraintWidgetContainer.setMaxWidth(this.mMaxWidth - i6);
        constraintWidgetContainer.setMaxHeight(this.mMaxHeight - i5);
        constraintWidgetContainer.setMinWidth(0);
        constraintWidgetContainer.setMinHeight(0);
        constraintWidgetContainer.setHorizontalDimensionBehaviour(dimensionBehaviour);
        constraintWidgetContainer.setWidth(i2);
        constraintWidgetContainer.setVerticalDimensionBehaviour(dimensionBehaviour2);
        constraintWidgetContainer.setHeight(i4);
        constraintWidgetContainer.setMinWidth(this.mMinWidth - i6);
        constraintWidgetContainer.setMinHeight(this.mMinHeight - i5);
    }

    public void setState(int i, int i2, int i3) {
        ConstraintLayoutStates constraintLayoutStates = this.mConstraintLayoutSpec;
        if (constraintLayoutStates != null) {
            constraintLayoutStates.updateConstraints(i, i2, i3);
        }
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        View content;
        int childCount = getChildCount();
        boolean zIsInEditMode = isInEditMode();
        for (int i5 = 0; i5 < childCount; i5++) {
            View childAt = getChildAt(i5);
            LayoutParams layoutParams = (LayoutParams) childAt.getLayoutParams();
            ConstraintWidget constraintWidget = layoutParams.widget;
            if ((childAt.getVisibility() != 8 || layoutParams.isGuideline || layoutParams.isHelper || layoutParams.isVirtualGroup || zIsInEditMode) && !layoutParams.isInPlaceholder) {
                int x = constraintWidget.getX();
                int y = constraintWidget.getY();
                int width = constraintWidget.getWidth() + x;
                int height = constraintWidget.getHeight() + y;
                childAt.layout(x, y, width, height);
                if ((childAt instanceof Placeholder) && (content = ((Placeholder) childAt).getContent()) != null) {
                    content.setVisibility(0);
                    content.layout(x, y, width, height);
                }
            }
        }
        int size = this.mConstraintHelpers.size();
        if (size > 0) {
            for (int i6 = 0; i6 < size; i6++) {
                this.mConstraintHelpers.get(i6).updatePostLayout(this);
            }
        }
    }

    public void setOptimizationLevel(int i) {
        this.mOptimizationLevel = i;
        this.mLayoutWidget.setOptimizationLevel(i);
    }

    public int getOptimizationLevel() {
        return this.mLayoutWidget.getOptimizationLevel();
    }

    @Override // android.view.ViewGroup
    public LayoutParams generateLayoutParams(AttributeSet attributeSet) {
        return new LayoutParams(getContext(), attributeSet);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // android.view.ViewGroup
    public LayoutParams generateDefaultLayoutParams() {
        return new LayoutParams(-2, -2);
    }

    @Override // android.view.ViewGroup
    protected ViewGroup.LayoutParams generateLayoutParams(ViewGroup.LayoutParams layoutParams) {
        return new LayoutParams(layoutParams);
    }

    @Override // android.view.ViewGroup
    protected boolean checkLayoutParams(ViewGroup.LayoutParams layoutParams) {
        return layoutParams instanceof LayoutParams;
    }

    public void setConstraintSet(ConstraintSet constraintSet) {
        this.mConstraintSet = constraintSet;
    }

    public View getViewById(int i) {
        return this.mChildrenByIds.get(i);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void dispatchDraw(Canvas canvas) {
        Object tag;
        int size;
        ArrayList<ConstraintHelper> arrayList = this.mConstraintHelpers;
        if (arrayList != null && (size = arrayList.size()) > 0) {
            for (int i = 0; i < size; i++) {
                this.mConstraintHelpers.get(i).updatePreDraw(this);
            }
        }
        super.dispatchDraw(canvas);
        if (isInEditMode()) {
            int childCount = getChildCount();
            float width = getWidth();
            float height = getHeight();
            for (int i2 = 0; i2 < childCount; i2++) {
                View childAt = getChildAt(i2);
                if (childAt.getVisibility() != 8 && (tag = childAt.getTag()) != null && (tag instanceof String)) {
                    String[] strArrSplit = ((String) tag).split(",");
                    if (strArrSplit.length == 4) {
                        int i3 = Integer.parseInt(strArrSplit[0]);
                        int i4 = Integer.parseInt(strArrSplit[1]);
                        int i5 = Integer.parseInt(strArrSplit[2]);
                        int i6 = (int) ((i3 / 1080.0f) * width);
                        int i7 = (int) ((i4 / 1920.0f) * height);
                        int i8 = (int) ((Integer.parseInt(strArrSplit[3]) / 1920.0f) * height);
                        Paint paint = new Paint();
                        paint.setColor(SupportMenu.CATEGORY_MASK);
                        float f = i6;
                        float f2 = i7;
                        float f3 = i6 + ((int) ((i5 / 1080.0f) * width));
                        canvas.drawLine(f, f2, f3, f2, paint);
                        float f4 = i7 + i8;
                        canvas.drawLine(f3, f2, f3, f4, paint);
                        canvas.drawLine(f3, f4, f, f4, paint);
                        canvas.drawLine(f, f4, f, f2, paint);
                        paint.setColor(-16711936);
                        canvas.drawLine(f, f2, f3, f4, paint);
                        canvas.drawLine(f, f4, f3, f2, paint);
                    }
                }
            }
        }
    }

    public void setOnConstraintsChanged(ConstraintsChangedListener constraintsChangedListener) {
        this.mConstraintsChangedListener = constraintsChangedListener;
        ConstraintLayoutStates constraintLayoutStates = this.mConstraintLayoutSpec;
        if (constraintLayoutStates != null) {
            constraintLayoutStates.setOnConstraintsChanged(constraintsChangedListener);
        }
    }

    public void loadLayoutDescription(int i) {
        if (i != 0) {
            try {
                this.mConstraintLayoutSpec = new ConstraintLayoutStates(getContext(), this, i);
                return;
            } catch (Resources.NotFoundException unused) {
                this.mConstraintLayoutSpec = null;
                return;
            }
        }
        this.mConstraintLayoutSpec = null;
    }

    public static class LayoutParams extends ViewGroup.MarginLayoutParams {
        public static final int BASELINE = 5;
        public static final int BOTTOM = 4;
        public static final int CHAIN_PACKED = 2;
        public static final int CHAIN_SPREAD = 0;
        public static final int CHAIN_SPREAD_INSIDE = 1;
        public static final int END = 7;
        public static final int HORIZONTAL = 0;
        public static final int LEFT = 1;
        public static final int MATCH_CONSTRAINT = 0;
        public static final int MATCH_CONSTRAINT_PERCENT = 2;
        public static final int MATCH_CONSTRAINT_SPREAD = 0;
        public static final int MATCH_CONSTRAINT_WRAP = 1;
        public static final int PARENT_ID = 0;
        public static final int RIGHT = 2;
        public static final int START = 6;
        public static final int TOP = 3;
        public static final int UNSET = -1;
        public static final int VERTICAL = 1;
        public int baselineToBaseline;
        public int bottomToBottom;
        public int bottomToTop;
        public float circleAngle;
        public int circleConstraint;
        public int circleRadius;
        public boolean constrainedHeight;
        public boolean constrainedWidth;
        public String constraintTag;
        public String dimensionRatio;
        int dimensionRatioSide;
        float dimensionRatioValue;
        public int editorAbsoluteX;
        public int editorAbsoluteY;
        public int endToEnd;
        public int endToStart;
        public int goneBottomMargin;
        public int goneEndMargin;
        public int goneLeftMargin;
        public int goneRightMargin;
        public int goneStartMargin;
        public int goneTopMargin;
        public int guideBegin;
        public int guideEnd;
        public float guidePercent;
        public boolean helped;
        public float horizontalBias;
        public int horizontalChainStyle;
        boolean horizontalDimensionFixed;
        public float horizontalWeight;
        boolean isGuideline;
        boolean isHelper;
        boolean isInPlaceholder;
        boolean isVirtualGroup;
        public int leftToLeft;
        public int leftToRight;
        public int matchConstraintDefaultHeight;
        public int matchConstraintDefaultWidth;
        public int matchConstraintMaxHeight;
        public int matchConstraintMaxWidth;
        public int matchConstraintMinHeight;
        public int matchConstraintMinWidth;
        public float matchConstraintPercentHeight;
        public float matchConstraintPercentWidth;
        boolean needsBaseline;
        public int orientation;
        int resolveGoneLeftMargin;
        int resolveGoneRightMargin;
        int resolvedGuideBegin;
        int resolvedGuideEnd;
        float resolvedGuidePercent;
        float resolvedHorizontalBias;
        int resolvedLeftToLeft;
        int resolvedLeftToRight;
        int resolvedRightToLeft;
        int resolvedRightToRight;
        public int rightToLeft;
        public int rightToRight;
        public int startToEnd;
        public int startToStart;
        public int topToBottom;
        public int topToTop;
        public float verticalBias;
        public int verticalChainStyle;
        boolean verticalDimensionFixed;
        public float verticalWeight;
        ConstraintWidget widget;

        public ConstraintWidget getConstraintWidget() {
            return this.widget;
        }

        public void setWidgetDebugName(String str) {
            this.widget.setDebugName(str);
        }

        public void reset() {
            ConstraintWidget constraintWidget = this.widget;
            if (constraintWidget != null) {
                constraintWidget.reset();
            }
        }

        public LayoutParams(LayoutParams layoutParams) {
            super((ViewGroup.MarginLayoutParams) layoutParams);
            this.guideBegin = -1;
            this.guideEnd = -1;
            this.guidePercent = -1.0f;
            this.leftToLeft = -1;
            this.leftToRight = -1;
            this.rightToLeft = -1;
            this.rightToRight = -1;
            this.topToTop = -1;
            this.topToBottom = -1;
            this.bottomToTop = -1;
            this.bottomToBottom = -1;
            this.baselineToBaseline = -1;
            this.circleConstraint = -1;
            this.circleRadius = 0;
            this.circleAngle = 0.0f;
            this.startToEnd = -1;
            this.startToStart = -1;
            this.endToStart = -1;
            this.endToEnd = -1;
            this.goneLeftMargin = -1;
            this.goneTopMargin = -1;
            this.goneRightMargin = -1;
            this.goneBottomMargin = -1;
            this.goneStartMargin = -1;
            this.goneEndMargin = -1;
            this.horizontalBias = 0.5f;
            this.verticalBias = 0.5f;
            this.dimensionRatio = null;
            this.dimensionRatioValue = 0.0f;
            this.dimensionRatioSide = 1;
            this.horizontalWeight = -1.0f;
            this.verticalWeight = -1.0f;
            this.horizontalChainStyle = 0;
            this.verticalChainStyle = 0;
            this.matchConstraintDefaultWidth = 0;
            this.matchConstraintDefaultHeight = 0;
            this.matchConstraintMinWidth = 0;
            this.matchConstraintMinHeight = 0;
            this.matchConstraintMaxWidth = 0;
            this.matchConstraintMaxHeight = 0;
            this.matchConstraintPercentWidth = 1.0f;
            this.matchConstraintPercentHeight = 1.0f;
            this.editorAbsoluteX = -1;
            this.editorAbsoluteY = -1;
            this.orientation = -1;
            this.constrainedWidth = false;
            this.constrainedHeight = false;
            this.constraintTag = null;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            this.needsBaseline = false;
            this.isGuideline = false;
            this.isHelper = false;
            this.isInPlaceholder = false;
            this.isVirtualGroup = false;
            this.resolvedLeftToLeft = -1;
            this.resolvedLeftToRight = -1;
            this.resolvedRightToLeft = -1;
            this.resolvedRightToRight = -1;
            this.resolveGoneLeftMargin = -1;
            this.resolveGoneRightMargin = -1;
            this.resolvedHorizontalBias = 0.5f;
            this.widget = new ConstraintWidget();
            this.helped = false;
            this.guideBegin = layoutParams.guideBegin;
            this.guideEnd = layoutParams.guideEnd;
            this.guidePercent = layoutParams.guidePercent;
            this.leftToLeft = layoutParams.leftToLeft;
            this.leftToRight = layoutParams.leftToRight;
            this.rightToLeft = layoutParams.rightToLeft;
            this.rightToRight = layoutParams.rightToRight;
            this.topToTop = layoutParams.topToTop;
            this.topToBottom = layoutParams.topToBottom;
            this.bottomToTop = layoutParams.bottomToTop;
            this.bottomToBottom = layoutParams.bottomToBottom;
            this.baselineToBaseline = layoutParams.baselineToBaseline;
            this.circleConstraint = layoutParams.circleConstraint;
            this.circleRadius = layoutParams.circleRadius;
            this.circleAngle = layoutParams.circleAngle;
            this.startToEnd = layoutParams.startToEnd;
            this.startToStart = layoutParams.startToStart;
            this.endToStart = layoutParams.endToStart;
            this.endToEnd = layoutParams.endToEnd;
            this.goneLeftMargin = layoutParams.goneLeftMargin;
            this.goneTopMargin = layoutParams.goneTopMargin;
            this.goneRightMargin = layoutParams.goneRightMargin;
            this.goneBottomMargin = layoutParams.goneBottomMargin;
            this.goneStartMargin = layoutParams.goneStartMargin;
            this.goneEndMargin = layoutParams.goneEndMargin;
            this.horizontalBias = layoutParams.horizontalBias;
            this.verticalBias = layoutParams.verticalBias;
            this.dimensionRatio = layoutParams.dimensionRatio;
            this.dimensionRatioValue = layoutParams.dimensionRatioValue;
            this.dimensionRatioSide = layoutParams.dimensionRatioSide;
            this.horizontalWeight = layoutParams.horizontalWeight;
            this.verticalWeight = layoutParams.verticalWeight;
            this.horizontalChainStyle = layoutParams.horizontalChainStyle;
            this.verticalChainStyle = layoutParams.verticalChainStyle;
            this.constrainedWidth = layoutParams.constrainedWidth;
            this.constrainedHeight = layoutParams.constrainedHeight;
            this.matchConstraintDefaultWidth = layoutParams.matchConstraintDefaultWidth;
            this.matchConstraintDefaultHeight = layoutParams.matchConstraintDefaultHeight;
            this.matchConstraintMinWidth = layoutParams.matchConstraintMinWidth;
            this.matchConstraintMaxWidth = layoutParams.matchConstraintMaxWidth;
            this.matchConstraintMinHeight = layoutParams.matchConstraintMinHeight;
            this.matchConstraintMaxHeight = layoutParams.matchConstraintMaxHeight;
            this.matchConstraintPercentWidth = layoutParams.matchConstraintPercentWidth;
            this.matchConstraintPercentHeight = layoutParams.matchConstraintPercentHeight;
            this.editorAbsoluteX = layoutParams.editorAbsoluteX;
            this.editorAbsoluteY = layoutParams.editorAbsoluteY;
            this.orientation = layoutParams.orientation;
            this.horizontalDimensionFixed = layoutParams.horizontalDimensionFixed;
            this.verticalDimensionFixed = layoutParams.verticalDimensionFixed;
            this.needsBaseline = layoutParams.needsBaseline;
            this.isGuideline = layoutParams.isGuideline;
            this.resolvedLeftToLeft = layoutParams.resolvedLeftToLeft;
            this.resolvedLeftToRight = layoutParams.resolvedLeftToRight;
            this.resolvedRightToLeft = layoutParams.resolvedRightToLeft;
            this.resolvedRightToRight = layoutParams.resolvedRightToRight;
            this.resolveGoneLeftMargin = layoutParams.resolveGoneLeftMargin;
            this.resolveGoneRightMargin = layoutParams.resolveGoneRightMargin;
            this.resolvedHorizontalBias = layoutParams.resolvedHorizontalBias;
            this.constraintTag = layoutParams.constraintTag;
            this.widget = layoutParams.widget;
        }

        private static class Table {
            public static final int ANDROID_ORIENTATION = 1;
            public static final int LAYOUT_CONSTRAINED_HEIGHT = 28;
            public static final int LAYOUT_CONSTRAINED_WIDTH = 27;
            public static final int LAYOUT_CONSTRAINT_BASELINE_CREATOR = 43;
            public static final int LAYOUT_CONSTRAINT_BASELINE_TO_BASELINE_OF = 16;
            public static final int LAYOUT_CONSTRAINT_BOTTOM_CREATOR = 42;
            public static final int LAYOUT_CONSTRAINT_BOTTOM_TO_BOTTOM_OF = 15;
            public static final int LAYOUT_CONSTRAINT_BOTTOM_TO_TOP_OF = 14;
            public static final int LAYOUT_CONSTRAINT_CIRCLE = 2;
            public static final int LAYOUT_CONSTRAINT_CIRCLE_ANGLE = 4;
            public static final int LAYOUT_CONSTRAINT_CIRCLE_RADIUS = 3;
            public static final int LAYOUT_CONSTRAINT_DIMENSION_RATIO = 44;
            public static final int LAYOUT_CONSTRAINT_END_TO_END_OF = 20;
            public static final int LAYOUT_CONSTRAINT_END_TO_START_OF = 19;
            public static final int LAYOUT_CONSTRAINT_GUIDE_BEGIN = 5;
            public static final int LAYOUT_CONSTRAINT_GUIDE_END = 6;
            public static final int LAYOUT_CONSTRAINT_GUIDE_PERCENT = 7;
            public static final int LAYOUT_CONSTRAINT_HEIGHT_DEFAULT = 32;
            public static final int LAYOUT_CONSTRAINT_HEIGHT_MAX = 37;
            public static final int LAYOUT_CONSTRAINT_HEIGHT_MIN = 36;
            public static final int LAYOUT_CONSTRAINT_HEIGHT_PERCENT = 38;
            public static final int LAYOUT_CONSTRAINT_HORIZONTAL_BIAS = 29;
            public static final int LAYOUT_CONSTRAINT_HORIZONTAL_CHAINSTYLE = 47;
            public static final int LAYOUT_CONSTRAINT_HORIZONTAL_WEIGHT = 45;
            public static final int LAYOUT_CONSTRAINT_LEFT_CREATOR = 39;
            public static final int LAYOUT_CONSTRAINT_LEFT_TO_LEFT_OF = 8;
            public static final int LAYOUT_CONSTRAINT_LEFT_TO_RIGHT_OF = 9;
            public static final int LAYOUT_CONSTRAINT_RIGHT_CREATOR = 41;
            public static final int LAYOUT_CONSTRAINT_RIGHT_TO_LEFT_OF = 10;
            public static final int LAYOUT_CONSTRAINT_RIGHT_TO_RIGHT_OF = 11;
            public static final int LAYOUT_CONSTRAINT_START_TO_END_OF = 17;
            public static final int LAYOUT_CONSTRAINT_START_TO_START_OF = 18;
            public static final int LAYOUT_CONSTRAINT_TAG = 51;
            public static final int LAYOUT_CONSTRAINT_TOP_CREATOR = 40;
            public static final int LAYOUT_CONSTRAINT_TOP_TO_BOTTOM_OF = 13;
            public static final int LAYOUT_CONSTRAINT_TOP_TO_TOP_OF = 12;
            public static final int LAYOUT_CONSTRAINT_VERTICAL_BIAS = 30;
            public static final int LAYOUT_CONSTRAINT_VERTICAL_CHAINSTYLE = 48;
            public static final int LAYOUT_CONSTRAINT_VERTICAL_WEIGHT = 46;
            public static final int LAYOUT_CONSTRAINT_WIDTH_DEFAULT = 31;
            public static final int LAYOUT_CONSTRAINT_WIDTH_MAX = 34;
            public static final int LAYOUT_CONSTRAINT_WIDTH_MIN = 33;
            public static final int LAYOUT_CONSTRAINT_WIDTH_PERCENT = 35;
            public static final int LAYOUT_EDITOR_ABSOLUTEX = 49;
            public static final int LAYOUT_EDITOR_ABSOLUTEY = 50;
            public static final int LAYOUT_GONE_MARGIN_BOTTOM = 24;
            public static final int LAYOUT_GONE_MARGIN_END = 26;
            public static final int LAYOUT_GONE_MARGIN_LEFT = 21;
            public static final int LAYOUT_GONE_MARGIN_RIGHT = 23;
            public static final int LAYOUT_GONE_MARGIN_START = 25;
            public static final int LAYOUT_GONE_MARGIN_TOP = 22;
            public static final int UNUSED = 0;
            public static final SparseIntArray map;

            private Table() {
            }

            static {
                SparseIntArray sparseIntArray = new SparseIntArray();
                map = sparseIntArray;
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintLeft_toLeftOf, 8);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintLeft_toRightOf, 9);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintRight_toLeftOf, 10);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintRight_toRightOf, 11);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintTop_toTopOf, 12);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintTop_toBottomOf, 13);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintBottom_toTopOf, 14);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintBottom_toBottomOf, 15);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintBaseline_toBaselineOf, 16);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintCircle, 2);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintCircleRadius, 3);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintCircleAngle, 4);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_editor_absoluteX, 49);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_editor_absoluteY, 50);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintGuide_begin, 5);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintGuide_end, 6);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintGuide_percent, 7);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_android_orientation, 1);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintStart_toEndOf, 17);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintStart_toStartOf, 18);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintEnd_toStartOf, 19);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintEnd_toEndOf, 20);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginLeft, 21);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginTop, 22);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginRight, 23);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginBottom, 24);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginStart, 25);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_goneMarginEnd, 26);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHorizontal_bias, 29);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintVertical_bias, 30);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintDimensionRatio, 44);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHorizontal_weight, 45);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintVertical_weight, 46);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHorizontal_chainStyle, 47);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintVertical_chainStyle, 48);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constrainedWidth, 27);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constrainedHeight, 28);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintWidth_default, 31);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHeight_default, 32);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintWidth_min, 33);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintWidth_max, 34);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintWidth_percent, 35);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHeight_min, 36);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHeight_max, 37);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintHeight_percent, 38);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintLeft_creator, 39);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintTop_creator, 40);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintRight_creator, 41);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintBottom_creator, 42);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintBaseline_creator, 43);
                sparseIntArray.append(R.styleable.ConstraintLayout_Layout_layout_constraintTag, 51);
            }
        }

        public LayoutParams(Context context, AttributeSet attributeSet) {
            int i;
            super(context, attributeSet);
            this.guideBegin = -1;
            this.guideEnd = -1;
            this.guidePercent = -1.0f;
            this.leftToLeft = -1;
            this.leftToRight = -1;
            this.rightToLeft = -1;
            this.rightToRight = -1;
            this.topToTop = -1;
            this.topToBottom = -1;
            this.bottomToTop = -1;
            this.bottomToBottom = -1;
            this.baselineToBaseline = -1;
            this.circleConstraint = -1;
            this.circleRadius = 0;
            this.circleAngle = 0.0f;
            this.startToEnd = -1;
            this.startToStart = -1;
            this.endToStart = -1;
            this.endToEnd = -1;
            this.goneLeftMargin = -1;
            this.goneTopMargin = -1;
            this.goneRightMargin = -1;
            this.goneBottomMargin = -1;
            this.goneStartMargin = -1;
            this.goneEndMargin = -1;
            this.horizontalBias = 0.5f;
            this.verticalBias = 0.5f;
            this.dimensionRatio = null;
            this.dimensionRatioValue = 0.0f;
            this.dimensionRatioSide = 1;
            this.horizontalWeight = -1.0f;
            this.verticalWeight = -1.0f;
            this.horizontalChainStyle = 0;
            this.verticalChainStyle = 0;
            this.matchConstraintDefaultWidth = 0;
            this.matchConstraintDefaultHeight = 0;
            this.matchConstraintMinWidth = 0;
            this.matchConstraintMinHeight = 0;
            this.matchConstraintMaxWidth = 0;
            this.matchConstraintMaxHeight = 0;
            this.matchConstraintPercentWidth = 1.0f;
            this.matchConstraintPercentHeight = 1.0f;
            this.editorAbsoluteX = -1;
            this.editorAbsoluteY = -1;
            this.orientation = -1;
            this.constrainedWidth = false;
            this.constrainedHeight = false;
            this.constraintTag = null;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            this.needsBaseline = false;
            this.isGuideline = false;
            this.isHelper = false;
            this.isInPlaceholder = false;
            this.isVirtualGroup = false;
            this.resolvedLeftToLeft = -1;
            this.resolvedLeftToRight = -1;
            this.resolvedRightToLeft = -1;
            this.resolvedRightToRight = -1;
            this.resolveGoneLeftMargin = -1;
            this.resolveGoneRightMargin = -1;
            this.resolvedHorizontalBias = 0.5f;
            this.widget = new ConstraintWidget();
            this.helped = false;
            TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.ConstraintLayout_Layout);
            int indexCount = typedArrayObtainStyledAttributes.getIndexCount();
            for (int i2 = 0; i2 < indexCount; i2++) {
                int index = typedArrayObtainStyledAttributes.getIndex(i2);
                int i3 = Table.map.get(index);
                switch (i3) {
                    case 1:
                        this.orientation = typedArrayObtainStyledAttributes.getInt(index, this.orientation);
                        break;
                    case 2:
                        int resourceId = typedArrayObtainStyledAttributes.getResourceId(index, this.circleConstraint);
                        this.circleConstraint = resourceId;
                        if (resourceId == -1) {
                            this.circleConstraint = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 3:
                        this.circleRadius = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.circleRadius);
                        break;
                    case 4:
                        float f = typedArrayObtainStyledAttributes.getFloat(index, this.circleAngle) % 360.0f;
                        this.circleAngle = f;
                        if (f < 0.0f) {
                            this.circleAngle = (360.0f - f) % 360.0f;
                        }
                        break;
                    case 5:
                        this.guideBegin = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.guideBegin);
                        break;
                    case 6:
                        this.guideEnd = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.guideEnd);
                        break;
                    case 7:
                        this.guidePercent = typedArrayObtainStyledAttributes.getFloat(index, this.guidePercent);
                        break;
                    case 8:
                        int resourceId2 = typedArrayObtainStyledAttributes.getResourceId(index, this.leftToLeft);
                        this.leftToLeft = resourceId2;
                        if (resourceId2 == -1) {
                            this.leftToLeft = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 9:
                        int resourceId3 = typedArrayObtainStyledAttributes.getResourceId(index, this.leftToRight);
                        this.leftToRight = resourceId3;
                        if (resourceId3 == -1) {
                            this.leftToRight = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 10:
                        int resourceId4 = typedArrayObtainStyledAttributes.getResourceId(index, this.rightToLeft);
                        this.rightToLeft = resourceId4;
                        if (resourceId4 == -1) {
                            this.rightToLeft = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 11:
                        int resourceId5 = typedArrayObtainStyledAttributes.getResourceId(index, this.rightToRight);
                        this.rightToRight = resourceId5;
                        if (resourceId5 == -1) {
                            this.rightToRight = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 12:
                        int resourceId6 = typedArrayObtainStyledAttributes.getResourceId(index, this.topToTop);
                        this.topToTop = resourceId6;
                        if (resourceId6 == -1) {
                            this.topToTop = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 13:
                        int resourceId7 = typedArrayObtainStyledAttributes.getResourceId(index, this.topToBottom);
                        this.topToBottom = resourceId7;
                        if (resourceId7 == -1) {
                            this.topToBottom = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 14:
                        int resourceId8 = typedArrayObtainStyledAttributes.getResourceId(index, this.bottomToTop);
                        this.bottomToTop = resourceId8;
                        if (resourceId8 == -1) {
                            this.bottomToTop = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 15:
                        int resourceId9 = typedArrayObtainStyledAttributes.getResourceId(index, this.bottomToBottom);
                        this.bottomToBottom = resourceId9;
                        if (resourceId9 == -1) {
                            this.bottomToBottom = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 16:
                        int resourceId10 = typedArrayObtainStyledAttributes.getResourceId(index, this.baselineToBaseline);
                        this.baselineToBaseline = resourceId10;
                        if (resourceId10 == -1) {
                            this.baselineToBaseline = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 17:
                        int resourceId11 = typedArrayObtainStyledAttributes.getResourceId(index, this.startToEnd);
                        this.startToEnd = resourceId11;
                        if (resourceId11 == -1) {
                            this.startToEnd = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 18:
                        int resourceId12 = typedArrayObtainStyledAttributes.getResourceId(index, this.startToStart);
                        this.startToStart = resourceId12;
                        if (resourceId12 == -1) {
                            this.startToStart = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 19:
                        int resourceId13 = typedArrayObtainStyledAttributes.getResourceId(index, this.endToStart);
                        this.endToStart = resourceId13;
                        if (resourceId13 == -1) {
                            this.endToStart = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 20:
                        int resourceId14 = typedArrayObtainStyledAttributes.getResourceId(index, this.endToEnd);
                        this.endToEnd = resourceId14;
                        if (resourceId14 == -1) {
                            this.endToEnd = typedArrayObtainStyledAttributes.getInt(index, -1);
                        }
                        break;
                    case 21:
                        this.goneLeftMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneLeftMargin);
                        break;
                    case 22:
                        this.goneTopMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneTopMargin);
                        break;
                    case 23:
                        this.goneRightMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneRightMargin);
                        break;
                    case 24:
                        this.goneBottomMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneBottomMargin);
                        break;
                    case 25:
                        this.goneStartMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneStartMargin);
                        break;
                    case 26:
                        this.goneEndMargin = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.goneEndMargin);
                        break;
                    case 27:
                        this.constrainedWidth = typedArrayObtainStyledAttributes.getBoolean(index, this.constrainedWidth);
                        break;
                    case 28:
                        this.constrainedHeight = typedArrayObtainStyledAttributes.getBoolean(index, this.constrainedHeight);
                        break;
                    case 29:
                        this.horizontalBias = typedArrayObtainStyledAttributes.getFloat(index, this.horizontalBias);
                        break;
                    case 30:
                        this.verticalBias = typedArrayObtainStyledAttributes.getFloat(index, this.verticalBias);
                        break;
                    case 31:
                        int i4 = typedArrayObtainStyledAttributes.getInt(index, 0);
                        this.matchConstraintDefaultWidth = i4;
                        if (i4 == 1) {
                            Log.e(ConstraintLayout.TAG, "layout_constraintWidth_default=\"wrap\" is deprecated.\nUse layout_width=\"WRAP_CONTENT\" and layout_constrainedWidth=\"true\" instead.");
                        }
                        break;
                    case 32:
                        int i5 = typedArrayObtainStyledAttributes.getInt(index, 0);
                        this.matchConstraintDefaultHeight = i5;
                        if (i5 == 1) {
                            Log.e(ConstraintLayout.TAG, "layout_constraintHeight_default=\"wrap\" is deprecated.\nUse layout_height=\"WRAP_CONTENT\" and layout_constrainedHeight=\"true\" instead.");
                        }
                        break;
                    case 33:
                        try {
                            this.matchConstraintMinWidth = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.matchConstraintMinWidth);
                        } catch (Exception unused) {
                            if (typedArrayObtainStyledAttributes.getInt(index, this.matchConstraintMinWidth) == -2) {
                                this.matchConstraintMinWidth = -2;
                            }
                        }
                        break;
                    case 34:
                        try {
                            this.matchConstraintMaxWidth = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.matchConstraintMaxWidth);
                        } catch (Exception unused2) {
                            if (typedArrayObtainStyledAttributes.getInt(index, this.matchConstraintMaxWidth) == -2) {
                                this.matchConstraintMaxWidth = -2;
                            }
                        }
                        break;
                    case 35:
                        this.matchConstraintPercentWidth = Math.max(0.0f, typedArrayObtainStyledAttributes.getFloat(index, this.matchConstraintPercentWidth));
                        this.matchConstraintDefaultWidth = 2;
                        break;
                    case 36:
                        try {
                            this.matchConstraintMinHeight = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.matchConstraintMinHeight);
                        } catch (Exception unused3) {
                            if (typedArrayObtainStyledAttributes.getInt(index, this.matchConstraintMinHeight) == -2) {
                                this.matchConstraintMinHeight = -2;
                            }
                        }
                        break;
                    case 37:
                        try {
                            this.matchConstraintMaxHeight = typedArrayObtainStyledAttributes.getDimensionPixelSize(index, this.matchConstraintMaxHeight);
                        } catch (Exception unused4) {
                            if (typedArrayObtainStyledAttributes.getInt(index, this.matchConstraintMaxHeight) == -2) {
                                this.matchConstraintMaxHeight = -2;
                            }
                        }
                        break;
                    case 38:
                        this.matchConstraintPercentHeight = Math.max(0.0f, typedArrayObtainStyledAttributes.getFloat(index, this.matchConstraintPercentHeight));
                        this.matchConstraintDefaultHeight = 2;
                        break;
                    default:
                        switch (i3) {
                            case 44:
                                String string = typedArrayObtainStyledAttributes.getString(index);
                                this.dimensionRatio = string;
                                this.dimensionRatioValue = Float.NaN;
                                this.dimensionRatioSide = -1;
                                if (string != null) {
                                    int length = string.length();
                                    int iIndexOf = this.dimensionRatio.indexOf(44);
                                    if (iIndexOf <= 0 || iIndexOf >= length - 1) {
                                        i = 0;
                                    } else {
                                        String strSubstring = this.dimensionRatio.substring(0, iIndexOf);
                                        if (strSubstring.equalsIgnoreCase("W")) {
                                            this.dimensionRatioSide = 0;
                                        } else if (strSubstring.equalsIgnoreCase("H")) {
                                            this.dimensionRatioSide = 1;
                                        }
                                        i = iIndexOf + 1;
                                    }
                                    int iIndexOf2 = this.dimensionRatio.indexOf(58);
                                    if (iIndexOf2 >= 0 && iIndexOf2 < length - 1) {
                                        String strSubstring2 = this.dimensionRatio.substring(i, iIndexOf2);
                                        String strSubstring3 = this.dimensionRatio.substring(iIndexOf2 + 1);
                                        if (strSubstring2.length() > 0 && strSubstring3.length() > 0) {
                                            try {
                                                float f2 = Float.parseFloat(strSubstring2);
                                                float f3 = Float.parseFloat(strSubstring3);
                                                if (f2 > 0.0f && f3 > 0.0f) {
                                                    if (this.dimensionRatioSide == 1) {
                                                        this.dimensionRatioValue = Math.abs(f3 / f2);
                                                    } else {
                                                        this.dimensionRatioValue = Math.abs(f2 / f3);
                                                    }
                                                }
                                            } catch (NumberFormatException unused5) {
                                            }
                                        }
                                    } else {
                                        String strSubstring4 = this.dimensionRatio.substring(i);
                                        if (strSubstring4.length() > 0) {
                                            this.dimensionRatioValue = Float.parseFloat(strSubstring4);
                                        }
                                    }
                                }
                                break;
                            case 45:
                                this.horizontalWeight = typedArrayObtainStyledAttributes.getFloat(index, this.horizontalWeight);
                                break;
                            case 46:
                                this.verticalWeight = typedArrayObtainStyledAttributes.getFloat(index, this.verticalWeight);
                                break;
                            case 47:
                                this.horizontalChainStyle = typedArrayObtainStyledAttributes.getInt(index, 0);
                                break;
                            case 48:
                                this.verticalChainStyle = typedArrayObtainStyledAttributes.getInt(index, 0);
                                break;
                            case 49:
                                this.editorAbsoluteX = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.editorAbsoluteX);
                                break;
                            case 50:
                                this.editorAbsoluteY = typedArrayObtainStyledAttributes.getDimensionPixelOffset(index, this.editorAbsoluteY);
                                break;
                            case 51:
                                this.constraintTag = typedArrayObtainStyledAttributes.getString(index);
                                break;
                        }
                        break;
                }
            }
            typedArrayObtainStyledAttributes.recycle();
            validate();
        }

        public void validate() {
            this.isGuideline = false;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            if (this.width == -2 && this.constrainedWidth) {
                this.horizontalDimensionFixed = false;
                if (this.matchConstraintDefaultWidth == 0) {
                    this.matchConstraintDefaultWidth = 1;
                }
            }
            if (this.height == -2 && this.constrainedHeight) {
                this.verticalDimensionFixed = false;
                if (this.matchConstraintDefaultHeight == 0) {
                    this.matchConstraintDefaultHeight = 1;
                }
            }
            if (this.width == 0 || this.width == -1) {
                this.horizontalDimensionFixed = false;
                if (this.width == 0 && this.matchConstraintDefaultWidth == 1) {
                    this.width = -2;
                    this.constrainedWidth = true;
                }
            }
            if (this.height == 0 || this.height == -1) {
                this.verticalDimensionFixed = false;
                if (this.height == 0 && this.matchConstraintDefaultHeight == 1) {
                    this.height = -2;
                    this.constrainedHeight = true;
                }
            }
            if (this.guidePercent == -1.0f && this.guideBegin == -1 && this.guideEnd == -1) {
                return;
            }
            this.isGuideline = true;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            if (!(this.widget instanceof androidx.constraintlayout.solver.widgets.Guideline)) {
                this.widget = new androidx.constraintlayout.solver.widgets.Guideline();
            }
            ((androidx.constraintlayout.solver.widgets.Guideline) this.widget).setOrientation(this.orientation);
        }

        public LayoutParams(int i, int i2) {
            super(i, i2);
            this.guideBegin = -1;
            this.guideEnd = -1;
            this.guidePercent = -1.0f;
            this.leftToLeft = -1;
            this.leftToRight = -1;
            this.rightToLeft = -1;
            this.rightToRight = -1;
            this.topToTop = -1;
            this.topToBottom = -1;
            this.bottomToTop = -1;
            this.bottomToBottom = -1;
            this.baselineToBaseline = -1;
            this.circleConstraint = -1;
            this.circleRadius = 0;
            this.circleAngle = 0.0f;
            this.startToEnd = -1;
            this.startToStart = -1;
            this.endToStart = -1;
            this.endToEnd = -1;
            this.goneLeftMargin = -1;
            this.goneTopMargin = -1;
            this.goneRightMargin = -1;
            this.goneBottomMargin = -1;
            this.goneStartMargin = -1;
            this.goneEndMargin = -1;
            this.horizontalBias = 0.5f;
            this.verticalBias = 0.5f;
            this.dimensionRatio = null;
            this.dimensionRatioValue = 0.0f;
            this.dimensionRatioSide = 1;
            this.horizontalWeight = -1.0f;
            this.verticalWeight = -1.0f;
            this.horizontalChainStyle = 0;
            this.verticalChainStyle = 0;
            this.matchConstraintDefaultWidth = 0;
            this.matchConstraintDefaultHeight = 0;
            this.matchConstraintMinWidth = 0;
            this.matchConstraintMinHeight = 0;
            this.matchConstraintMaxWidth = 0;
            this.matchConstraintMaxHeight = 0;
            this.matchConstraintPercentWidth = 1.0f;
            this.matchConstraintPercentHeight = 1.0f;
            this.editorAbsoluteX = -1;
            this.editorAbsoluteY = -1;
            this.orientation = -1;
            this.constrainedWidth = false;
            this.constrainedHeight = false;
            this.constraintTag = null;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            this.needsBaseline = false;
            this.isGuideline = false;
            this.isHelper = false;
            this.isInPlaceholder = false;
            this.isVirtualGroup = false;
            this.resolvedLeftToLeft = -1;
            this.resolvedLeftToRight = -1;
            this.resolvedRightToLeft = -1;
            this.resolvedRightToRight = -1;
            this.resolveGoneLeftMargin = -1;
            this.resolveGoneRightMargin = -1;
            this.resolvedHorizontalBias = 0.5f;
            this.widget = new ConstraintWidget();
            this.helped = false;
        }

        public LayoutParams(ViewGroup.LayoutParams layoutParams) {
            super(layoutParams);
            this.guideBegin = -1;
            this.guideEnd = -1;
            this.guidePercent = -1.0f;
            this.leftToLeft = -1;
            this.leftToRight = -1;
            this.rightToLeft = -1;
            this.rightToRight = -1;
            this.topToTop = -1;
            this.topToBottom = -1;
            this.bottomToTop = -1;
            this.bottomToBottom = -1;
            this.baselineToBaseline = -1;
            this.circleConstraint = -1;
            this.circleRadius = 0;
            this.circleAngle = 0.0f;
            this.startToEnd = -1;
            this.startToStart = -1;
            this.endToStart = -1;
            this.endToEnd = -1;
            this.goneLeftMargin = -1;
            this.goneTopMargin = -1;
            this.goneRightMargin = -1;
            this.goneBottomMargin = -1;
            this.goneStartMargin = -1;
            this.goneEndMargin = -1;
            this.horizontalBias = 0.5f;
            this.verticalBias = 0.5f;
            this.dimensionRatio = null;
            this.dimensionRatioValue = 0.0f;
            this.dimensionRatioSide = 1;
            this.horizontalWeight = -1.0f;
            this.verticalWeight = -1.0f;
            this.horizontalChainStyle = 0;
            this.verticalChainStyle = 0;
            this.matchConstraintDefaultWidth = 0;
            this.matchConstraintDefaultHeight = 0;
            this.matchConstraintMinWidth = 0;
            this.matchConstraintMinHeight = 0;
            this.matchConstraintMaxWidth = 0;
            this.matchConstraintMaxHeight = 0;
            this.matchConstraintPercentWidth = 1.0f;
            this.matchConstraintPercentHeight = 1.0f;
            this.editorAbsoluteX = -1;
            this.editorAbsoluteY = -1;
            this.orientation = -1;
            this.constrainedWidth = false;
            this.constrainedHeight = false;
            this.constraintTag = null;
            this.horizontalDimensionFixed = true;
            this.verticalDimensionFixed = true;
            this.needsBaseline = false;
            this.isGuideline = false;
            this.isHelper = false;
            this.isInPlaceholder = false;
            this.isVirtualGroup = false;
            this.resolvedLeftToLeft = -1;
            this.resolvedLeftToRight = -1;
            this.resolvedRightToLeft = -1;
            this.resolvedRightToRight = -1;
            this.resolveGoneLeftMargin = -1;
            this.resolveGoneRightMargin = -1;
            this.resolvedHorizontalBias = 0.5f;
            this.widget = new ConstraintWidget();
            this.helped = false;
        }

        /* JADX WARN: Code duplicated, block: B:17:0x0048  */
        /* JADX WARN: Code duplicated, block: B:20:0x004f  */
        /* JADX WARN: Code duplicated, block: B:23:0x0056  */
        /* JADX WARN: Code duplicated, block: B:26:0x005c  */
        /* JADX WARN: Code duplicated, block: B:29:0x0062  */
        /* JADX WARN: Code duplicated, block: B:36:0x0074  */
        /* JADX WARN: Code duplicated, block: B:37:0x007c A[DONT_INVERT] */
        /* JADX WARN: Code duplicated, block: B:38:0x007e  */
        /* JADX WARN: Code duplicated, block: B:39:0x0085 A[DONT_INVERT] */
        /* JADX WARN: Code duplicated, block: B:40:0x0087  */
        @Override // android.view.ViewGroup.MarginLayoutParams, android.view.ViewGroup.LayoutParams
        public void resolveLayoutDirection(int i) {
            int i2;
            int i3;
            int i4;
            int i5;
            int i6 = this.leftMargin;
            int i7 = this.rightMargin;
            super.resolveLayoutDirection(i);
            boolean z = false;
            boolean z2 = 1 == getLayoutDirection();
            this.resolvedRightToLeft = -1;
            this.resolvedRightToRight = -1;
            this.resolvedLeftToLeft = -1;
            this.resolvedLeftToRight = -1;
            this.resolveGoneLeftMargin = this.goneLeftMargin;
            this.resolveGoneRightMargin = this.goneRightMargin;
            float f = this.horizontalBias;
            this.resolvedHorizontalBias = f;
            int i8 = this.guideBegin;
            this.resolvedGuideBegin = i8;
            int i9 = this.guideEnd;
            this.resolvedGuideEnd = i9;
            float f2 = this.guidePercent;
            this.resolvedGuidePercent = f2;
            if (z2) {
                int i10 = this.startToEnd;
                if (i10 != -1) {
                    this.resolvedRightToLeft = i10;
                } else {
                    int i11 = this.startToStart;
                    if (i11 != -1) {
                        this.resolvedRightToRight = i11;
                    } else {
                        i2 = this.endToStart;
                        if (i2 != -1) {
                            this.resolvedLeftToRight = i2;
                            z = true;
                        }
                        i3 = this.endToEnd;
                        if (i3 != -1) {
                            this.resolvedLeftToLeft = i3;
                            z = true;
                        }
                        i4 = this.goneStartMargin;
                        if (i4 != -1) {
                            this.resolveGoneRightMargin = i4;
                        }
                        i5 = this.goneEndMargin;
                        if (i5 != -1) {
                            this.resolveGoneLeftMargin = i5;
                        }
                        if (z) {
                            this.resolvedHorizontalBias = 1.0f - f;
                        }
                        if (this.isGuideline && this.orientation == 1) {
                            if (f2 != -1.0f) {
                                this.resolvedGuidePercent = 1.0f - f2;
                                this.resolvedGuideBegin = -1;
                                this.resolvedGuideEnd = -1;
                            } else if (i8 != -1) {
                                this.resolvedGuideEnd = i8;
                                this.resolvedGuideBegin = -1;
                                this.resolvedGuidePercent = -1.0f;
                            } else if (i9 != -1) {
                                this.resolvedGuideBegin = i9;
                                this.resolvedGuideEnd = -1;
                                this.resolvedGuidePercent = -1.0f;
                            }
                        }
                    }
                }
                z = true;
                i2 = this.endToStart;
                if (i2 != -1) {
                    this.resolvedLeftToRight = i2;
                    z = true;
                }
                i3 = this.endToEnd;
                if (i3 != -1) {
                    this.resolvedLeftToLeft = i3;
                    z = true;
                }
                i4 = this.goneStartMargin;
                if (i4 != -1) {
                    this.resolveGoneRightMargin = i4;
                }
                i5 = this.goneEndMargin;
                if (i5 != -1) {
                    this.resolveGoneLeftMargin = i5;
                }
                if (z) {
                    this.resolvedHorizontalBias = 1.0f - f;
                }
                if (this.isGuideline) {
                    if (f2 != -1.0f) {
                        this.resolvedGuidePercent = 1.0f - f2;
                        this.resolvedGuideBegin = -1;
                        this.resolvedGuideEnd = -1;
                    } else if (i8 != -1) {
                        this.resolvedGuideEnd = i8;
                        this.resolvedGuideBegin = -1;
                        this.resolvedGuidePercent = -1.0f;
                    } else if (i9 != -1) {
                        this.resolvedGuideBegin = i9;
                        this.resolvedGuideEnd = -1;
                        this.resolvedGuidePercent = -1.0f;
                    }
                }
            } else {
                int i12 = this.startToEnd;
                if (i12 != -1) {
                    this.resolvedLeftToRight = i12;
                }
                int i13 = this.startToStart;
                if (i13 != -1) {
                    this.resolvedLeftToLeft = i13;
                }
                int i14 = this.endToStart;
                if (i14 != -1) {
                    this.resolvedRightToLeft = i14;
                }
                int i15 = this.endToEnd;
                if (i15 != -1) {
                    this.resolvedRightToRight = i15;
                }
                int i16 = this.goneStartMargin;
                if (i16 != -1) {
                    this.resolveGoneLeftMargin = i16;
                }
                int i17 = this.goneEndMargin;
                if (i17 != -1) {
                    this.resolveGoneRightMargin = i17;
                }
            }
            if (this.endToStart == -1 && this.endToEnd == -1 && this.startToStart == -1 && this.startToEnd == -1) {
                int i18 = this.rightToLeft;
                if (i18 != -1) {
                    this.resolvedRightToLeft = i18;
                    if (this.rightMargin <= 0 && i7 > 0) {
                        this.rightMargin = i7;
                    }
                } else {
                    int i19 = this.rightToRight;
                    if (i19 != -1) {
                        this.resolvedRightToRight = i19;
                        if (this.rightMargin <= 0 && i7 > 0) {
                            this.rightMargin = i7;
                        }
                    }
                }
                int i20 = this.leftToLeft;
                if (i20 != -1) {
                    this.resolvedLeftToLeft = i20;
                    if (this.leftMargin > 0 || i6 <= 0) {
                        return;
                    }
                    this.leftMargin = i6;
                    return;
                }
                int i21 = this.leftToRight;
                if (i21 != -1) {
                    this.resolvedLeftToRight = i21;
                    if (this.leftMargin > 0 || i6 <= 0) {
                        return;
                    }
                    this.leftMargin = i6;
                }
            }
        }

        public String getConstraintTag() {
            return this.constraintTag;
        }
    }

    @Override // android.view.View, android.view.ViewParent
    public void requestLayout() {
        markHierarchyDirty();
        super.requestLayout();
    }

    @Override // android.view.View
    public void forceLayout() {
        markHierarchyDirty();
        super.forceLayout();
    }

    private void markHierarchyDirty() {
        this.mDirtyHierarchy = true;
        this.mLastMeasureWidth = -1;
        this.mLastMeasureHeight = -1;
        this.mLastMeasureWidthSize = -1;
        this.mLastMeasureHeightSize = -1;
        this.mLastMeasureWidthMode = 0;
        this.mLastMeasureHeightMode = 0;
    }
}
