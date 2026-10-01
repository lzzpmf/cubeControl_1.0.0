package androidx.constraintlayout.solver.widgets;

import androidx.constraintlayout.solver.Cache;
import androidx.constraintlayout.solver.LinearSystem;
import androidx.constraintlayout.solver.SolverVariable;
import androidx.constraintlayout.solver.widgets.analyzer.ChainRun;
import androidx.constraintlayout.solver.widgets.analyzer.HorizontalWidgetRun;
import androidx.constraintlayout.solver.widgets.analyzer.VerticalWidgetRun;
import androidx.constraintlayout.solver.widgets.analyzer.WidgetRun;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;

/* JADX INFO: loaded from: classes.dex */
public class ConstraintWidget {
    public static final int ANCHOR_BASELINE = 4;
    public static final int ANCHOR_BOTTOM = 3;
    public static final int ANCHOR_LEFT = 0;
    public static final int ANCHOR_RIGHT = 1;
    public static final int ANCHOR_TOP = 2;
    private static final boolean AUTOTAG_CENTER = false;
    public static final int CHAIN_PACKED = 2;
    public static final int CHAIN_SPREAD = 0;
    public static final int CHAIN_SPREAD_INSIDE = 1;
    public static float DEFAULT_BIAS = 0.5f;
    static final int DIMENSION_HORIZONTAL = 0;
    static final int DIMENSION_VERTICAL = 1;
    protected static final int DIRECT = 2;
    public static final int GONE = 8;
    public static final int HORIZONTAL = 0;
    public static final int INVISIBLE = 4;
    public static final int MATCH_CONSTRAINT_PERCENT = 2;
    public static final int MATCH_CONSTRAINT_RATIO = 3;
    public static final int MATCH_CONSTRAINT_RATIO_RESOLVED = 4;
    public static final int MATCH_CONSTRAINT_SPREAD = 0;
    public static final int MATCH_CONSTRAINT_WRAP = 1;
    protected static final int SOLVER = 1;
    public static final int UNKNOWN = -1;
    private static final boolean USE_WRAP_DIMENSION_FOR_SPREAD = false;
    public static final int VERTICAL = 1;
    public static final int VISIBLE = 0;
    private static final int WRAP = -2;
    private boolean hasBaseline;
    public ChainRun horizontalChainRun;
    public HorizontalWidgetRun horizontalRun;
    private boolean inPlaceholder;
    public boolean[] isTerminalWidget;
    protected ArrayList<ConstraintAnchor> mAnchors;
    ConstraintAnchor mBaseline;
    int mBaselineDistance;
    public ConstraintAnchor mBottom;
    boolean mBottomHasCentered;
    ConstraintAnchor mCenter;
    ConstraintAnchor mCenterX;
    ConstraintAnchor mCenterY;
    private float mCircleConstraintAngle;
    private Object mCompanionWidget;
    private int mContainerItemSkip;
    private String mDebugName;
    public float mDimensionRatio;
    protected int mDimensionRatioSide;
    int mDistToBottom;
    int mDistToLeft;
    int mDistToRight;
    int mDistToTop;
    boolean mGroupsToSolver;
    int mHeight;
    float mHorizontalBiasPercent;
    boolean mHorizontalChainFixedPosition;
    int mHorizontalChainStyle;
    ConstraintWidget mHorizontalNextWidget;
    public int mHorizontalResolution;
    boolean mHorizontalWrapVisited;
    private boolean mInVirtuaLayout;
    public boolean mIsHeightWrapContent;
    private boolean[] mIsInBarrier;
    public boolean mIsWidthWrapContent;
    public ConstraintAnchor mLeft;
    boolean mLeftHasCentered;
    public ConstraintAnchor[] mListAnchors;
    public DimensionBehaviour[] mListDimensionBehaviors;
    protected ConstraintWidget[] mListNextMatchConstraintsWidget;
    public int mMatchConstraintDefaultHeight;
    public int mMatchConstraintDefaultWidth;
    public int mMatchConstraintMaxHeight;
    public int mMatchConstraintMaxWidth;
    public int mMatchConstraintMinHeight;
    public int mMatchConstraintMinWidth;
    public float mMatchConstraintPercentHeight;
    public float mMatchConstraintPercentWidth;
    private int[] mMaxDimension;
    protected int mMinHeight;
    protected int mMinWidth;
    protected ConstraintWidget[] mNextChainWidget;
    protected int mOffsetX;
    protected int mOffsetY;
    boolean mOptimizerMeasurable;
    public ConstraintWidget mParent;
    int mRelX;
    int mRelY;
    float mResolvedDimensionRatio;
    int mResolvedDimensionRatioSide;
    boolean mResolvedHasRatio;
    public int[] mResolvedMatchConstraintDefault;
    public ConstraintAnchor mRight;
    boolean mRightHasCentered;
    public ConstraintAnchor mTop;
    boolean mTopHasCentered;
    private String mType;
    float mVerticalBiasPercent;
    boolean mVerticalChainFixedPosition;
    int mVerticalChainStyle;
    ConstraintWidget mVerticalNextWidget;
    public int mVerticalResolution;
    boolean mVerticalWrapVisited;
    private int mVisibility;
    public float[] mWeight;
    int mWidth;
    protected int mX;
    protected int mY;
    public boolean measured;
    public WidgetRun[] run;
    public ChainRun verticalChainRun;
    public VerticalWidgetRun verticalRun;
    public int[] wrapMeasure;

    public enum DimensionBehaviour {
        FIXED,
        WRAP_CONTENT,
        MATCH_CONSTRAINT,
        MATCH_PARENT
    }

    public WidgetRun getRun(int i) {
        if (i == 0) {
            return this.horizontalRun;
        }
        if (i == 1) {
            return this.verticalRun;
        }
        return null;
    }

    public boolean isInVirtualLayout() {
        return this.mInVirtuaLayout;
    }

    public void setInVirtualLayout(boolean z) {
        this.mInVirtuaLayout = z;
    }

    public int getMaxHeight() {
        return this.mMaxDimension[1];
    }

    public int getMaxWidth() {
        return this.mMaxDimension[0];
    }

    public void setMaxWidth(int i) {
        this.mMaxDimension[0] = i;
    }

    public void setMaxHeight(int i) {
        this.mMaxDimension[1] = i;
    }

    public boolean isSpreadWidth() {
        return this.mMatchConstraintDefaultWidth == 0 && this.mDimensionRatio == 0.0f && this.mMatchConstraintMinWidth == 0 && this.mMatchConstraintMaxWidth == 0 && this.mListDimensionBehaviors[0] == DimensionBehaviour.MATCH_CONSTRAINT;
    }

    public boolean isSpreadHeight() {
        return this.mMatchConstraintDefaultHeight == 0 && this.mDimensionRatio == 0.0f && this.mMatchConstraintMinHeight == 0 && this.mMatchConstraintMaxHeight == 0 && this.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT;
    }

    public void setHasBaseline(boolean z) {
        this.hasBaseline = z;
    }

    public boolean getHasBaseline() {
        return this.hasBaseline;
    }

    public boolean isInPlaceholder() {
        return this.inPlaceholder;
    }

    public void setInPlaceholder(boolean z) {
        this.inPlaceholder = z;
    }

    protected void setInBarrier(int i, boolean z) {
        this.mIsInBarrier[i] = z;
    }

    public void reset() {
        this.mLeft.reset();
        this.mTop.reset();
        this.mRight.reset();
        this.mBottom.reset();
        this.mBaseline.reset();
        this.mCenterX.reset();
        this.mCenterY.reset();
        this.mCenter.reset();
        this.mParent = null;
        this.mCircleConstraintAngle = 0.0f;
        this.mWidth = 0;
        this.mHeight = 0;
        this.mDimensionRatio = 0.0f;
        this.mDimensionRatioSide = -1;
        this.mX = 0;
        this.mY = 0;
        this.mOffsetX = 0;
        this.mOffsetY = 0;
        this.mBaselineDistance = 0;
        this.mMinWidth = 0;
        this.mMinHeight = 0;
        float f = DEFAULT_BIAS;
        this.mHorizontalBiasPercent = f;
        this.mVerticalBiasPercent = f;
        this.mListDimensionBehaviors[0] = DimensionBehaviour.FIXED;
        this.mListDimensionBehaviors[1] = DimensionBehaviour.FIXED;
        this.mCompanionWidget = null;
        this.mContainerItemSkip = 0;
        this.mVisibility = 0;
        this.mType = null;
        this.mHorizontalWrapVisited = false;
        this.mVerticalWrapVisited = false;
        this.mHorizontalChainStyle = 0;
        this.mVerticalChainStyle = 0;
        this.mHorizontalChainFixedPosition = false;
        this.mVerticalChainFixedPosition = false;
        float[] fArr = this.mWeight;
        fArr[0] = -1.0f;
        fArr[1] = -1.0f;
        this.mHorizontalResolution = -1;
        this.mVerticalResolution = -1;
        int[] iArr = this.mMaxDimension;
        iArr[0] = Integer.MAX_VALUE;
        iArr[1] = Integer.MAX_VALUE;
        this.mMatchConstraintDefaultWidth = 0;
        this.mMatchConstraintDefaultHeight = 0;
        this.mMatchConstraintPercentWidth = 1.0f;
        this.mMatchConstraintPercentHeight = 1.0f;
        this.mMatchConstraintMaxWidth = Integer.MAX_VALUE;
        this.mMatchConstraintMaxHeight = Integer.MAX_VALUE;
        this.mMatchConstraintMinWidth = 0;
        this.mMatchConstraintMinHeight = 0;
        this.mResolvedHasRatio = false;
        this.mResolvedDimensionRatioSide = -1;
        this.mResolvedDimensionRatio = 1.0f;
        this.mOptimizerMeasurable = false;
        this.mGroupsToSolver = false;
        boolean[] zArr = this.isTerminalWidget;
        zArr[0] = true;
        zArr[1] = true;
        this.mInVirtuaLayout = false;
        boolean[] zArr2 = this.mIsInBarrier;
        zArr2[0] = false;
        zArr2[1] = false;
    }

    public ConstraintWidget() {
        this.measured = false;
        this.run = new WidgetRun[2];
        this.horizontalRun = new HorizontalWidgetRun(this);
        this.verticalRun = new VerticalWidgetRun(this);
        this.isTerminalWidget = new boolean[]{true, true};
        this.wrapMeasure = new int[]{0, 0, 0, 0};
        this.mResolvedHasRatio = false;
        this.mHorizontalResolution = -1;
        this.mVerticalResolution = -1;
        this.mMatchConstraintDefaultWidth = 0;
        this.mMatchConstraintDefaultHeight = 0;
        this.mResolvedMatchConstraintDefault = new int[2];
        this.mMatchConstraintMinWidth = 0;
        this.mMatchConstraintMaxWidth = 0;
        this.mMatchConstraintPercentWidth = 1.0f;
        this.mMatchConstraintMinHeight = 0;
        this.mMatchConstraintMaxHeight = 0;
        this.mMatchConstraintPercentHeight = 1.0f;
        this.mResolvedDimensionRatioSide = -1;
        this.mResolvedDimensionRatio = 1.0f;
        this.mMaxDimension = new int[]{Integer.MAX_VALUE, Integer.MAX_VALUE};
        this.mCircleConstraintAngle = 0.0f;
        this.hasBaseline = false;
        this.mInVirtuaLayout = false;
        this.mLeft = new ConstraintAnchor(this, ConstraintAnchor.Type.LEFT);
        this.mTop = new ConstraintAnchor(this, ConstraintAnchor.Type.TOP);
        this.mRight = new ConstraintAnchor(this, ConstraintAnchor.Type.RIGHT);
        this.mBottom = new ConstraintAnchor(this, ConstraintAnchor.Type.BOTTOM);
        this.mBaseline = new ConstraintAnchor(this, ConstraintAnchor.Type.BASELINE);
        this.mCenterX = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER_X);
        this.mCenterY = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER_Y);
        ConstraintAnchor constraintAnchor = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER);
        this.mCenter = constraintAnchor;
        this.mListAnchors = new ConstraintAnchor[]{this.mLeft, this.mRight, this.mTop, this.mBottom, this.mBaseline, constraintAnchor};
        this.mAnchors = new ArrayList<>();
        this.mIsInBarrier = new boolean[2];
        this.mListDimensionBehaviors = new DimensionBehaviour[]{DimensionBehaviour.FIXED, DimensionBehaviour.FIXED};
        this.mParent = null;
        this.mWidth = 0;
        this.mHeight = 0;
        this.mDimensionRatio = 0.0f;
        this.mDimensionRatioSide = -1;
        this.mX = 0;
        this.mY = 0;
        this.mRelX = 0;
        this.mRelY = 0;
        this.mOffsetX = 0;
        this.mOffsetY = 0;
        this.mBaselineDistance = 0;
        float f = DEFAULT_BIAS;
        this.mHorizontalBiasPercent = f;
        this.mVerticalBiasPercent = f;
        this.mContainerItemSkip = 0;
        this.mVisibility = 0;
        this.mDebugName = null;
        this.mType = null;
        this.mOptimizerMeasurable = false;
        this.mGroupsToSolver = false;
        this.mHorizontalChainStyle = 0;
        this.mVerticalChainStyle = 0;
        this.mWeight = new float[]{-1.0f, -1.0f};
        this.mListNextMatchConstraintsWidget = new ConstraintWidget[]{null, null};
        this.mNextChainWidget = new ConstraintWidget[]{null, null};
        this.mHorizontalNextWidget = null;
        this.mVerticalNextWidget = null;
        addAnchors();
    }

    public ConstraintWidget(int i, int i2, int i3, int i4) {
        this.measured = false;
        this.run = new WidgetRun[2];
        this.horizontalRun = new HorizontalWidgetRun(this);
        this.verticalRun = new VerticalWidgetRun(this);
        this.isTerminalWidget = new boolean[]{true, true};
        this.wrapMeasure = new int[]{0, 0, 0, 0};
        this.mResolvedHasRatio = false;
        this.mHorizontalResolution = -1;
        this.mVerticalResolution = -1;
        this.mMatchConstraintDefaultWidth = 0;
        this.mMatchConstraintDefaultHeight = 0;
        this.mResolvedMatchConstraintDefault = new int[2];
        this.mMatchConstraintMinWidth = 0;
        this.mMatchConstraintMaxWidth = 0;
        this.mMatchConstraintPercentWidth = 1.0f;
        this.mMatchConstraintMinHeight = 0;
        this.mMatchConstraintMaxHeight = 0;
        this.mMatchConstraintPercentHeight = 1.0f;
        this.mResolvedDimensionRatioSide = -1;
        this.mResolvedDimensionRatio = 1.0f;
        this.mMaxDimension = new int[]{Integer.MAX_VALUE, Integer.MAX_VALUE};
        this.mCircleConstraintAngle = 0.0f;
        this.hasBaseline = false;
        this.mInVirtuaLayout = false;
        this.mLeft = new ConstraintAnchor(this, ConstraintAnchor.Type.LEFT);
        this.mTop = new ConstraintAnchor(this, ConstraintAnchor.Type.TOP);
        this.mRight = new ConstraintAnchor(this, ConstraintAnchor.Type.RIGHT);
        this.mBottom = new ConstraintAnchor(this, ConstraintAnchor.Type.BOTTOM);
        this.mBaseline = new ConstraintAnchor(this, ConstraintAnchor.Type.BASELINE);
        this.mCenterX = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER_X);
        this.mCenterY = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER_Y);
        ConstraintAnchor constraintAnchor = new ConstraintAnchor(this, ConstraintAnchor.Type.CENTER);
        this.mCenter = constraintAnchor;
        this.mListAnchors = new ConstraintAnchor[]{this.mLeft, this.mRight, this.mTop, this.mBottom, this.mBaseline, constraintAnchor};
        this.mAnchors = new ArrayList<>();
        this.mIsInBarrier = new boolean[2];
        this.mListDimensionBehaviors = new DimensionBehaviour[]{DimensionBehaviour.FIXED, DimensionBehaviour.FIXED};
        this.mParent = null;
        this.mDimensionRatio = 0.0f;
        this.mDimensionRatioSide = -1;
        this.mRelX = 0;
        this.mRelY = 0;
        this.mOffsetX = 0;
        this.mOffsetY = 0;
        this.mBaselineDistance = 0;
        float f = DEFAULT_BIAS;
        this.mHorizontalBiasPercent = f;
        this.mVerticalBiasPercent = f;
        this.mContainerItemSkip = 0;
        this.mVisibility = 0;
        this.mDebugName = null;
        this.mType = null;
        this.mOptimizerMeasurable = false;
        this.mGroupsToSolver = false;
        this.mHorizontalChainStyle = 0;
        this.mVerticalChainStyle = 0;
        this.mWeight = new float[]{-1.0f, -1.0f};
        this.mListNextMatchConstraintsWidget = new ConstraintWidget[]{null, null};
        this.mNextChainWidget = new ConstraintWidget[]{null, null};
        this.mHorizontalNextWidget = null;
        this.mVerticalNextWidget = null;
        this.mX = i;
        this.mY = i2;
        this.mWidth = i3;
        this.mHeight = i4;
        addAnchors();
    }

    public ConstraintWidget(int i, int i2) {
        this(0, 0, i, i2);
    }

    public void resetSolverVariables(Cache cache) {
        this.mLeft.resetSolverVariable(cache);
        this.mTop.resetSolverVariable(cache);
        this.mRight.resetSolverVariable(cache);
        this.mBottom.resetSolverVariable(cache);
        this.mBaseline.resetSolverVariable(cache);
        this.mCenter.resetSolverVariable(cache);
        this.mCenterX.resetSolverVariable(cache);
        this.mCenterY.resetSolverVariable(cache);
    }

    private void addAnchors() {
        this.mAnchors.add(this.mLeft);
        this.mAnchors.add(this.mTop);
        this.mAnchors.add(this.mRight);
        this.mAnchors.add(this.mBottom);
        this.mAnchors.add(this.mCenterX);
        this.mAnchors.add(this.mCenterY);
        this.mAnchors.add(this.mCenter);
        this.mAnchors.add(this.mBaseline);
    }

    public boolean isRoot() {
        return this.mParent == null;
    }

    public ConstraintWidget getParent() {
        return this.mParent;
    }

    public void setParent(ConstraintWidget constraintWidget) {
        this.mParent = constraintWidget;
    }

    public void setWidthWrapContent(boolean z) {
        this.mIsWidthWrapContent = z;
    }

    public boolean isWidthWrapContent() {
        return this.mIsWidthWrapContent;
    }

    public void setHeightWrapContent(boolean z) {
        this.mIsHeightWrapContent = z;
    }

    public boolean isHeightWrapContent() {
        return this.mIsHeightWrapContent;
    }

    public void connectCircularConstraint(ConstraintWidget constraintWidget, float f, int i) {
        immediateConnect(ConstraintAnchor.Type.CENTER, constraintWidget, ConstraintAnchor.Type.CENTER, i, 0);
        this.mCircleConstraintAngle = f;
    }

    public String getType() {
        return this.mType;
    }

    public void setType(String str) {
        this.mType = str;
    }

    public void setVisibility(int i) {
        this.mVisibility = i;
    }

    public int getVisibility() {
        return this.mVisibility;
    }

    public String getDebugName() {
        return this.mDebugName;
    }

    public void setDebugName(String str) {
        this.mDebugName = str;
    }

    public void setDebugSolverName(LinearSystem linearSystem, String str) {
        this.mDebugName = str;
        SolverVariable solverVariableCreateObjectVariable = linearSystem.createObjectVariable(this.mLeft);
        SolverVariable solverVariableCreateObjectVariable2 = linearSystem.createObjectVariable(this.mTop);
        SolverVariable solverVariableCreateObjectVariable3 = linearSystem.createObjectVariable(this.mRight);
        SolverVariable solverVariableCreateObjectVariable4 = linearSystem.createObjectVariable(this.mBottom);
        solverVariableCreateObjectVariable.setName(str + ".left");
        solverVariableCreateObjectVariable2.setName(str + ".top");
        solverVariableCreateObjectVariable3.setName(str + ".right");
        solverVariableCreateObjectVariable4.setName(str + ".bottom");
        if (this.mBaselineDistance > 0) {
            linearSystem.createObjectVariable(this.mBaseline).setName(str + ".baseline");
        }
    }

    public void createObjectVariables(LinearSystem linearSystem) {
        linearSystem.createObjectVariable(this.mLeft);
        linearSystem.createObjectVariable(this.mTop);
        linearSystem.createObjectVariable(this.mRight);
        linearSystem.createObjectVariable(this.mBottom);
        if (this.mBaselineDistance > 0) {
            linearSystem.createObjectVariable(this.mBaseline);
        }
    }

    public String toString() {
        return (this.mType != null ? "type: " + this.mType + " " : "") + (this.mDebugName != null ? "id: " + this.mDebugName + " " : "") + "(" + this.mX + ", " + this.mY + ") - (" + this.mWidth + " x " + this.mHeight + ")";
    }

    public int getX() {
        ConstraintWidget constraintWidget = this.mParent;
        if (constraintWidget != null && (constraintWidget instanceof ConstraintWidgetContainer)) {
            return ((ConstraintWidgetContainer) constraintWidget).mPaddingLeft + this.mX;
        }
        return this.mX;
    }

    public int getY() {
        ConstraintWidget constraintWidget = this.mParent;
        if (constraintWidget != null && (constraintWidget instanceof ConstraintWidgetContainer)) {
            return ((ConstraintWidgetContainer) constraintWidget).mPaddingTop + this.mY;
        }
        return this.mY;
    }

    public int getWidth() {
        if (this.mVisibility == 8) {
            return 0;
        }
        return this.mWidth;
    }

    public int getOptimizerWrapWidth() {
        int i = this.mWidth;
        int iMax = 0;
        if (this.mListDimensionBehaviors[0] != DimensionBehaviour.MATCH_CONSTRAINT) {
            return i;
        }
        if (this.mMatchConstraintDefaultWidth == 1) {
            iMax = Math.max(this.mMatchConstraintMinWidth, i);
        } else {
            int i2 = this.mMatchConstraintMinWidth;
            if (i2 > 0) {
                this.mWidth = i2;
                iMax = i2;
            }
        }
        int i3 = this.mMatchConstraintMaxWidth;
        return (i3 <= 0 || i3 >= iMax) ? iMax : i3;
    }

    public int getOptimizerWrapHeight() {
        int iMax = this.mHeight;
        if (this.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT) {
            if (this.mMatchConstraintDefaultHeight == 1) {
                iMax = Math.max(this.mMatchConstraintMinHeight, iMax);
            } else {
                iMax = this.mMatchConstraintMinHeight;
                if (iMax > 0) {
                    this.mHeight = iMax;
                } else {
                    iMax = 0;
                }
            }
            int i = this.mMatchConstraintMaxHeight;
            if (i > 0 && i < iMax) {
                return i;
            }
        }
        return iMax;
    }

    public int getHeight() {
        if (this.mVisibility == 8) {
            return 0;
        }
        return this.mHeight;
    }

    public int getLength(int i) {
        if (i == 0) {
            return getWidth();
        }
        if (i == 1) {
            return getHeight();
        }
        return 0;
    }

    protected int getRootX() {
        return this.mX + this.mOffsetX;
    }

    protected int getRootY() {
        return this.mY + this.mOffsetY;
    }

    public int getMinWidth() {
        return this.mMinWidth;
    }

    public int getMinHeight() {
        return this.mMinHeight;
    }

    public int getLeft() {
        return getX();
    }

    public int getTop() {
        return getY();
    }

    public int getRight() {
        return getX() + this.mWidth;
    }

    public int getBottom() {
        return getY() + this.mHeight;
    }

    public int getHorizontalMargin() {
        ConstraintAnchor constraintAnchor = this.mLeft;
        int i = constraintAnchor != null ? constraintAnchor.mMargin : 0;
        ConstraintAnchor constraintAnchor2 = this.mRight;
        return constraintAnchor2 != null ? i + constraintAnchor2.mMargin : i;
    }

    public int getVerticalMargin() {
        int i = this.mLeft != null ? this.mTop.mMargin : 0;
        return this.mRight != null ? i + this.mBottom.mMargin : i;
    }

    public float getHorizontalBiasPercent() {
        return this.mHorizontalBiasPercent;
    }

    public float getVerticalBiasPercent() {
        return this.mVerticalBiasPercent;
    }

    public float getBiasPercent(int i) {
        if (i == 0) {
            return this.mHorizontalBiasPercent;
        }
        if (i == 1) {
            return this.mVerticalBiasPercent;
        }
        return -1.0f;
    }

    public boolean hasBaseline() {
        return this.hasBaseline;
    }

    public int getBaselineDistance() {
        return this.mBaselineDistance;
    }

    public Object getCompanionWidget() {
        return this.mCompanionWidget;
    }

    public ArrayList<ConstraintAnchor> getAnchors() {
        return this.mAnchors;
    }

    public void setX(int i) {
        this.mX = i;
    }

    public void setY(int i) {
        this.mY = i;
    }

    public void setOrigin(int i, int i2) {
        this.mX = i;
        this.mY = i2;
    }

    public void setOffset(int i, int i2) {
        this.mOffsetX = i;
        this.mOffsetY = i2;
    }

    public void setGoneMargin(ConstraintAnchor.Type type, int i) {
        int i2 = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[type.ordinal()];
        if (i2 == 1) {
            this.mLeft.mGoneMargin = i;
            return;
        }
        if (i2 == 2) {
            this.mTop.mGoneMargin = i;
        } else if (i2 == 3) {
            this.mRight.mGoneMargin = i;
        } else {
            if (i2 != 4) {
                return;
            }
            this.mBottom.mGoneMargin = i;
        }
    }

    public void setWidth(int i) {
        this.mWidth = i;
        int i2 = this.mMinWidth;
        if (i < i2) {
            this.mWidth = i2;
        }
    }

    public void setHeight(int i) {
        this.mHeight = i;
        int i2 = this.mMinHeight;
        if (i < i2) {
            this.mHeight = i2;
        }
    }

    public void setLength(int i, int i2) {
        if (i2 == 0) {
            setWidth(i);
        } else if (i2 == 1) {
            setHeight(i);
        }
    }

    public void setHorizontalMatchStyle(int i, int i2, int i3, float f) {
        this.mMatchConstraintDefaultWidth = i;
        this.mMatchConstraintMinWidth = i2;
        if (i3 == Integer.MAX_VALUE) {
            i3 = 0;
        }
        this.mMatchConstraintMaxWidth = i3;
        this.mMatchConstraintPercentWidth = f;
        if (f <= 0.0f || f >= 1.0f || i != 0) {
            return;
        }
        this.mMatchConstraintDefaultWidth = 2;
    }

    public void setVerticalMatchStyle(int i, int i2, int i3, float f) {
        this.mMatchConstraintDefaultHeight = i;
        this.mMatchConstraintMinHeight = i2;
        if (i3 == Integer.MAX_VALUE) {
            i3 = 0;
        }
        this.mMatchConstraintMaxHeight = i3;
        this.mMatchConstraintPercentHeight = f;
        if (f <= 0.0f || f >= 1.0f || i != 0) {
            return;
        }
        this.mMatchConstraintDefaultHeight = 2;
    }

    /* JADX WARN: Code duplicated, block: B:39:0x0086 A[PHI: r0
  0x0086: PHI (r0v2 int) = (r0v1 int), (r0v0 int), (r0v0 int), (r0v0 int), (r0v0 int), (r0v0 int) binds: [B:46:0x0086, B:36:0x007f, B:24:0x0051, B:26:0x0057, B:28:0x0063, B:30:0x0067] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:39:0x0086 -> B:40:0x0087). Please report as a decompilation issue!!! */
    public void setDimensionRatio(String str) {
        float fAbs;
        int i = 0;
        if (str == null || str.length() == 0) {
            this.mDimensionRatio = 0.0f;
            return;
        }
        int length = str.length();
        int iIndexOf = str.indexOf(44);
        int i2 = 0;
        int i3 = -1;
        if (iIndexOf > 0 && iIndexOf < length - 1) {
            String strSubstring = str.substring(0, iIndexOf);
            if (!strSubstring.equalsIgnoreCase("W")) {
                i2 = strSubstring.equalsIgnoreCase("H") ? 1 : -1;
            }
            i3 = i2;
            i2 = iIndexOf + 1;
        }
        int iIndexOf2 = str.indexOf(58);
        try {
            if (iIndexOf2 >= 0 && iIndexOf2 < length - 1) {
                String strSubstring2 = str.substring(i2, iIndexOf2);
                String strSubstring3 = str.substring(iIndexOf2 + 1);
                if (strSubstring2.length() <= 0 || strSubstring3.length() <= 0) {
                    fAbs = i;
                } else {
                    float f = Float.parseFloat(strSubstring2);
                    float f2 = Float.parseFloat(strSubstring3);
                    if (f <= 0.0f || f2 <= 0.0f) {
                        fAbs = i;
                    } else if (i3 == 1) {
                        fAbs = Math.abs(f2 / f);
                    } else {
                        fAbs = Math.abs(f / f2);
                    }
                }
            } else {
                String strSubstring4 = str.substring(i2);
                if (strSubstring4.length() > 0) {
                    fAbs = Float.parseFloat(strSubstring4);
                } else {
                    fAbs = i;
                }
            }
        } catch (NumberFormatException unused) {
        }
        i = (fAbs > i ? 1 : (fAbs == i ? 0 : -1));
        if (i > 0) {
            this.mDimensionRatio = fAbs;
            this.mDimensionRatioSide = i3;
        }
    }

    public void setDimensionRatio(float f, int i) {
        this.mDimensionRatio = f;
        this.mDimensionRatioSide = i;
    }

    public float getDimensionRatio() {
        return this.mDimensionRatio;
    }

    public int getDimensionRatioSide() {
        return this.mDimensionRatioSide;
    }

    public void setHorizontalBiasPercent(float f) {
        this.mHorizontalBiasPercent = f;
    }

    public void setVerticalBiasPercent(float f) {
        this.mVerticalBiasPercent = f;
    }

    public void setMinWidth(int i) {
        if (i < 0) {
            this.mMinWidth = 0;
        } else {
            this.mMinWidth = i;
        }
    }

    public void setMinHeight(int i) {
        if (i < 0) {
            this.mMinHeight = 0;
        } else {
            this.mMinHeight = i;
        }
    }

    public void setDimension(int i, int i2) {
        this.mWidth = i;
        int i3 = this.mMinWidth;
        if (i < i3) {
            this.mWidth = i3;
        }
        this.mHeight = i2;
        int i4 = this.mMinHeight;
        if (i2 < i4) {
            this.mHeight = i4;
        }
    }

    public void setFrame(int i, int i2, int i3, int i4) {
        int i5;
        int i6;
        int i7 = i3 - i;
        int i8 = i4 - i2;
        this.mX = i;
        this.mY = i2;
        if (this.mVisibility == 8) {
            this.mWidth = 0;
            this.mHeight = 0;
            return;
        }
        if (this.mListDimensionBehaviors[0] == DimensionBehaviour.FIXED && i7 < (i6 = this.mWidth)) {
            i7 = i6;
        }
        if (this.mListDimensionBehaviors[1] == DimensionBehaviour.FIXED && i8 < (i5 = this.mHeight)) {
            i8 = i5;
        }
        this.mWidth = i7;
        this.mHeight = i8;
        int i9 = this.mMinHeight;
        if (i8 < i9) {
            this.mHeight = i9;
        }
        int i10 = this.mMinWidth;
        if (i7 < i10) {
            this.mWidth = i10;
        }
    }

    public void setFrame(int i, int i2, int i3) {
        if (i3 == 0) {
            setHorizontalDimension(i, i2);
        } else if (i3 == 1) {
            setVerticalDimension(i, i2);
        }
    }

    public void setHorizontalDimension(int i, int i2) {
        this.mX = i;
        int i3 = i2 - i;
        this.mWidth = i3;
        int i4 = this.mMinWidth;
        if (i3 < i4) {
            this.mWidth = i4;
        }
    }

    public void setVerticalDimension(int i, int i2) {
        this.mY = i;
        int i3 = i2 - i;
        this.mHeight = i3;
        int i4 = this.mMinHeight;
        if (i3 < i4) {
            this.mHeight = i4;
        }
    }

    int getRelativePositioning(int i) {
        if (i == 0) {
            return this.mRelX;
        }
        if (i == 1) {
            return this.mRelY;
        }
        return 0;
    }

    void setRelativePositioning(int i, int i2) {
        if (i2 == 0) {
            this.mRelX = i;
        } else if (i2 == 1) {
            this.mRelY = i;
        }
    }

    public void setBaselineDistance(int i) {
        this.mBaselineDistance = i;
        this.hasBaseline = i > 0;
    }

    public void setCompanionWidget(Object obj) {
        this.mCompanionWidget = obj;
    }

    public void setContainerItemSkip(int i) {
        if (i >= 0) {
            this.mContainerItemSkip = i;
        } else {
            this.mContainerItemSkip = 0;
        }
    }

    public int getContainerItemSkip() {
        return this.mContainerItemSkip;
    }

    public void setHorizontalWeight(float f) {
        this.mWeight[0] = f;
    }

    public void setVerticalWeight(float f) {
        this.mWeight[1] = f;
    }

    public void setHorizontalChainStyle(int i) {
        this.mHorizontalChainStyle = i;
    }

    public int getHorizontalChainStyle() {
        return this.mHorizontalChainStyle;
    }

    public void setVerticalChainStyle(int i) {
        this.mVerticalChainStyle = i;
    }

    public int getVerticalChainStyle() {
        return this.mVerticalChainStyle;
    }

    public boolean allowedInBarrier() {
        return this.mVisibility != 8;
    }

    public void immediateConnect(ConstraintAnchor.Type type, ConstraintWidget constraintWidget, ConstraintAnchor.Type type2, int i, int i2) {
        getAnchor(type).connect(constraintWidget.getAnchor(type2), i, i2, true);
    }

    public void connect(ConstraintAnchor constraintAnchor, ConstraintAnchor constraintAnchor2, int i) {
        if (constraintAnchor.getOwner() == this) {
            connect(constraintAnchor.getType(), constraintAnchor2.getOwner(), constraintAnchor2.getType(), i);
        }
    }

    public void connect(ConstraintAnchor.Type type, ConstraintWidget constraintWidget, ConstraintAnchor.Type type2) {
        connect(type, constraintWidget, type2, 0);
    }

    public void connect(ConstraintAnchor.Type type, ConstraintWidget constraintWidget, ConstraintAnchor.Type type2, int i) {
        boolean z;
        if (type == ConstraintAnchor.Type.CENTER) {
            if (type2 == ConstraintAnchor.Type.CENTER) {
                ConstraintAnchor anchor = getAnchor(ConstraintAnchor.Type.LEFT);
                ConstraintAnchor anchor2 = getAnchor(ConstraintAnchor.Type.RIGHT);
                ConstraintAnchor anchor3 = getAnchor(ConstraintAnchor.Type.TOP);
                ConstraintAnchor anchor4 = getAnchor(ConstraintAnchor.Type.BOTTOM);
                boolean z2 = true;
                if ((anchor == null || !anchor.isConnected()) && (anchor2 == null || !anchor2.isConnected())) {
                    connect(ConstraintAnchor.Type.LEFT, constraintWidget, ConstraintAnchor.Type.LEFT, 0);
                    connect(ConstraintAnchor.Type.RIGHT, constraintWidget, ConstraintAnchor.Type.RIGHT, 0);
                    z = true;
                } else {
                    z = false;
                }
                if ((anchor3 == null || !anchor3.isConnected()) && (anchor4 == null || !anchor4.isConnected())) {
                    connect(ConstraintAnchor.Type.TOP, constraintWidget, ConstraintAnchor.Type.TOP, 0);
                    connect(ConstraintAnchor.Type.BOTTOM, constraintWidget, ConstraintAnchor.Type.BOTTOM, 0);
                } else {
                    z2 = false;
                }
                if (z && z2) {
                    getAnchor(ConstraintAnchor.Type.CENTER).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.CENTER), 0);
                    return;
                } else if (z) {
                    getAnchor(ConstraintAnchor.Type.CENTER_X).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.CENTER_X), 0);
                    return;
                } else {
                    if (z2) {
                        getAnchor(ConstraintAnchor.Type.CENTER_Y).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.CENTER_Y), 0);
                        return;
                    }
                    return;
                }
            }
            if (type2 == ConstraintAnchor.Type.LEFT || type2 == ConstraintAnchor.Type.RIGHT) {
                connect(ConstraintAnchor.Type.LEFT, constraintWidget, type2, 0);
                connect(ConstraintAnchor.Type.RIGHT, constraintWidget, type2, 0);
                getAnchor(ConstraintAnchor.Type.CENTER).connect(constraintWidget.getAnchor(type2), 0);
                return;
            } else {
                if (type2 == ConstraintAnchor.Type.TOP || type2 == ConstraintAnchor.Type.BOTTOM) {
                    connect(ConstraintAnchor.Type.TOP, constraintWidget, type2, 0);
                    connect(ConstraintAnchor.Type.BOTTOM, constraintWidget, type2, 0);
                    getAnchor(ConstraintAnchor.Type.CENTER).connect(constraintWidget.getAnchor(type2), 0);
                    return;
                }
                return;
            }
        }
        if (type == ConstraintAnchor.Type.CENTER_X && (type2 == ConstraintAnchor.Type.LEFT || type2 == ConstraintAnchor.Type.RIGHT)) {
            ConstraintAnchor anchor5 = getAnchor(ConstraintAnchor.Type.LEFT);
            ConstraintAnchor anchor6 = constraintWidget.getAnchor(type2);
            ConstraintAnchor anchor7 = getAnchor(ConstraintAnchor.Type.RIGHT);
            anchor5.connect(anchor6, 0);
            anchor7.connect(anchor6, 0);
            getAnchor(ConstraintAnchor.Type.CENTER_X).connect(anchor6, 0);
            return;
        }
        if (type == ConstraintAnchor.Type.CENTER_Y && (type2 == ConstraintAnchor.Type.TOP || type2 == ConstraintAnchor.Type.BOTTOM)) {
            ConstraintAnchor anchor8 = constraintWidget.getAnchor(type2);
            getAnchor(ConstraintAnchor.Type.TOP).connect(anchor8, 0);
            getAnchor(ConstraintAnchor.Type.BOTTOM).connect(anchor8, 0);
            getAnchor(ConstraintAnchor.Type.CENTER_Y).connect(anchor8, 0);
            return;
        }
        if (type == ConstraintAnchor.Type.CENTER_X && type2 == ConstraintAnchor.Type.CENTER_X) {
            getAnchor(ConstraintAnchor.Type.LEFT).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.LEFT), 0);
            getAnchor(ConstraintAnchor.Type.RIGHT).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.RIGHT), 0);
            getAnchor(ConstraintAnchor.Type.CENTER_X).connect(constraintWidget.getAnchor(type2), 0);
            return;
        }
        if (type == ConstraintAnchor.Type.CENTER_Y && type2 == ConstraintAnchor.Type.CENTER_Y) {
            getAnchor(ConstraintAnchor.Type.TOP).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.TOP), 0);
            getAnchor(ConstraintAnchor.Type.BOTTOM).connect(constraintWidget.getAnchor(ConstraintAnchor.Type.BOTTOM), 0);
            getAnchor(ConstraintAnchor.Type.CENTER_Y).connect(constraintWidget.getAnchor(type2), 0);
            return;
        }
        ConstraintAnchor anchor9 = getAnchor(type);
        ConstraintAnchor anchor10 = constraintWidget.getAnchor(type2);
        if (anchor9.isValidConnection(anchor10)) {
            if (type == ConstraintAnchor.Type.BASELINE) {
                ConstraintAnchor anchor11 = getAnchor(ConstraintAnchor.Type.TOP);
                ConstraintAnchor anchor12 = getAnchor(ConstraintAnchor.Type.BOTTOM);
                if (anchor11 != null) {
                    anchor11.reset();
                }
                if (anchor12 != null) {
                    anchor12.reset();
                }
                i = 0;
            } else if (type == ConstraintAnchor.Type.TOP || type == ConstraintAnchor.Type.BOTTOM) {
                ConstraintAnchor anchor13 = getAnchor(ConstraintAnchor.Type.BASELINE);
                if (anchor13 != null) {
                    anchor13.reset();
                }
                ConstraintAnchor anchor14 = getAnchor(ConstraintAnchor.Type.CENTER);
                if (anchor14.getTarget() != anchor10) {
                    anchor14.reset();
                }
                ConstraintAnchor opposite = getAnchor(type).getOpposite();
                ConstraintAnchor anchor15 = getAnchor(ConstraintAnchor.Type.CENTER_Y);
                if (anchor15.isConnected()) {
                    opposite.reset();
                    anchor15.reset();
                }
            } else if (type == ConstraintAnchor.Type.LEFT || type == ConstraintAnchor.Type.RIGHT) {
                ConstraintAnchor anchor16 = getAnchor(ConstraintAnchor.Type.CENTER);
                if (anchor16.getTarget() != anchor10) {
                    anchor16.reset();
                }
                ConstraintAnchor opposite2 = getAnchor(type).getOpposite();
                ConstraintAnchor anchor17 = getAnchor(ConstraintAnchor.Type.CENTER_X);
                if (anchor17.isConnected()) {
                    opposite2.reset();
                    anchor17.reset();
                }
            }
            anchor9.connect(anchor10, i);
        }
    }

    public void resetAllConstraints() {
        resetAnchors();
        setVerticalBiasPercent(DEFAULT_BIAS);
        setHorizontalBiasPercent(DEFAULT_BIAS);
    }

    public void resetAnchor(ConstraintAnchor constraintAnchor) {
        if (getParent() != null && (getParent() instanceof ConstraintWidgetContainer) && ((ConstraintWidgetContainer) getParent()).handlesInternalConstraints()) {
            return;
        }
        ConstraintAnchor anchor = getAnchor(ConstraintAnchor.Type.LEFT);
        ConstraintAnchor anchor2 = getAnchor(ConstraintAnchor.Type.RIGHT);
        ConstraintAnchor anchor3 = getAnchor(ConstraintAnchor.Type.TOP);
        ConstraintAnchor anchor4 = getAnchor(ConstraintAnchor.Type.BOTTOM);
        ConstraintAnchor anchor5 = getAnchor(ConstraintAnchor.Type.CENTER);
        ConstraintAnchor anchor6 = getAnchor(ConstraintAnchor.Type.CENTER_X);
        ConstraintAnchor anchor7 = getAnchor(ConstraintAnchor.Type.CENTER_Y);
        if (constraintAnchor == anchor5) {
            if (anchor.isConnected() && anchor2.isConnected() && anchor.getTarget() == anchor2.getTarget()) {
                anchor.reset();
                anchor2.reset();
            }
            if (anchor3.isConnected() && anchor4.isConnected() && anchor3.getTarget() == anchor4.getTarget()) {
                anchor3.reset();
                anchor4.reset();
            }
            this.mHorizontalBiasPercent = 0.5f;
            this.mVerticalBiasPercent = 0.5f;
        } else if (constraintAnchor == anchor6) {
            if (anchor.isConnected() && anchor2.isConnected() && anchor.getTarget().getOwner() == anchor2.getTarget().getOwner()) {
                anchor.reset();
                anchor2.reset();
            }
            this.mHorizontalBiasPercent = 0.5f;
        } else if (constraintAnchor == anchor7) {
            if (anchor3.isConnected() && anchor4.isConnected() && anchor3.getTarget().getOwner() == anchor4.getTarget().getOwner()) {
                anchor3.reset();
                anchor4.reset();
            }
            this.mVerticalBiasPercent = 0.5f;
        } else if (constraintAnchor == anchor || constraintAnchor == anchor2) {
            if (anchor.isConnected() && anchor.getTarget() == anchor2.getTarget()) {
                anchor5.reset();
            }
        } else if ((constraintAnchor == anchor3 || constraintAnchor == anchor4) && anchor3.isConnected() && anchor3.getTarget() == anchor4.getTarget()) {
            anchor5.reset();
        }
        constraintAnchor.reset();
    }

    public void resetAnchors() {
        ConstraintWidget parent = getParent();
        if (parent != null && (parent instanceof ConstraintWidgetContainer) && ((ConstraintWidgetContainer) getParent()).handlesInternalConstraints()) {
            return;
        }
        int size = this.mAnchors.size();
        for (int i = 0; i < size; i++) {
            this.mAnchors.get(i).reset();
        }
    }

    public ConstraintAnchor getAnchor(ConstraintAnchor.Type type) {
        switch (AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[type.ordinal()]) {
            case 1:
                return this.mLeft;
            case 2:
                return this.mTop;
            case 3:
                return this.mRight;
            case 4:
                return this.mBottom;
            case 5:
                return this.mBaseline;
            case 6:
                return this.mCenter;
            case 7:
                return this.mCenterX;
            case 8:
                return this.mCenterY;
            case 9:
                return null;
            default:
                throw new AssertionError(type.name());
        }
    }

    public DimensionBehaviour getHorizontalDimensionBehaviour() {
        return this.mListDimensionBehaviors[0];
    }

    public DimensionBehaviour getVerticalDimensionBehaviour() {
        return this.mListDimensionBehaviors[1];
    }

    public DimensionBehaviour getDimensionBehaviour(int i) {
        if (i == 0) {
            return getHorizontalDimensionBehaviour();
        }
        if (i == 1) {
            return getVerticalDimensionBehaviour();
        }
        return null;
    }

    public void setHorizontalDimensionBehaviour(DimensionBehaviour dimensionBehaviour) {
        this.mListDimensionBehaviors[0] = dimensionBehaviour;
    }

    public void setVerticalDimensionBehaviour(DimensionBehaviour dimensionBehaviour) {
        this.mListDimensionBehaviors[1] = dimensionBehaviour;
    }

    public boolean isInHorizontalChain() {
        if (this.mLeft.mTarget == null || this.mLeft.mTarget.mTarget != this.mLeft) {
            return this.mRight.mTarget != null && this.mRight.mTarget.mTarget == this.mRight;
        }
        return true;
    }

    public ConstraintWidget getPreviousChainMember(int i) {
        if (i == 0) {
            if (this.mLeft.mTarget == null) {
                return null;
            }
            ConstraintAnchor constraintAnchor = this.mLeft.mTarget.mTarget;
            ConstraintAnchor constraintAnchor2 = this.mLeft;
            if (constraintAnchor == constraintAnchor2) {
                return constraintAnchor2.mTarget.mOwner;
            }
            return null;
        }
        if (i != 1 || this.mTop.mTarget == null) {
            return null;
        }
        ConstraintAnchor constraintAnchor3 = this.mTop.mTarget.mTarget;
        ConstraintAnchor constraintAnchor4 = this.mTop;
        if (constraintAnchor3 == constraintAnchor4) {
            return constraintAnchor4.mTarget.mOwner;
        }
        return null;
    }

    public ConstraintWidget getNextChainMember(int i) {
        if (i == 0) {
            if (this.mRight.mTarget == null) {
                return null;
            }
            ConstraintAnchor constraintAnchor = this.mRight.mTarget.mTarget;
            ConstraintAnchor constraintAnchor2 = this.mRight;
            if (constraintAnchor == constraintAnchor2) {
                return constraintAnchor2.mTarget.mOwner;
            }
            return null;
        }
        if (i != 1 || this.mBottom.mTarget == null) {
            return null;
        }
        ConstraintAnchor constraintAnchor3 = this.mBottom.mTarget.mTarget;
        ConstraintAnchor constraintAnchor4 = this.mBottom;
        if (constraintAnchor3 == constraintAnchor4) {
            return constraintAnchor4.mTarget.mOwner;
        }
        return null;
    }

    public ConstraintWidget getHorizontalChainControlWidget() {
        if (!isInHorizontalChain()) {
            return null;
        }
        ConstraintWidget constraintWidget = this;
        ConstraintWidget constraintWidget2 = null;
        while (constraintWidget2 == null && constraintWidget != null) {
            ConstraintAnchor anchor = constraintWidget.getAnchor(ConstraintAnchor.Type.LEFT);
            ConstraintAnchor target = anchor == null ? null : anchor.getTarget();
            ConstraintWidget owner = target == null ? null : target.getOwner();
            if (owner == getParent()) {
                return constraintWidget;
            }
            ConstraintAnchor target2 = owner == null ? null : owner.getAnchor(ConstraintAnchor.Type.RIGHT).getTarget();
            if (target2 == null || target2.getOwner() == constraintWidget) {
                constraintWidget = owner;
            } else {
                constraintWidget2 = constraintWidget;
            }
        }
        return constraintWidget2;
    }

    public boolean isInVerticalChain() {
        if (this.mTop.mTarget == null || this.mTop.mTarget.mTarget != this.mTop) {
            return this.mBottom.mTarget != null && this.mBottom.mTarget.mTarget == this.mBottom;
        }
        return true;
    }

    public ConstraintWidget getVerticalChainControlWidget() {
        if (!isInVerticalChain()) {
            return null;
        }
        ConstraintWidget constraintWidget = this;
        ConstraintWidget constraintWidget2 = null;
        while (constraintWidget2 == null && constraintWidget != null) {
            ConstraintAnchor anchor = constraintWidget.getAnchor(ConstraintAnchor.Type.TOP);
            ConstraintAnchor target = anchor == null ? null : anchor.getTarget();
            ConstraintWidget owner = target == null ? null : target.getOwner();
            if (owner == getParent()) {
                return constraintWidget;
            }
            ConstraintAnchor target2 = owner == null ? null : owner.getAnchor(ConstraintAnchor.Type.BOTTOM).getTarget();
            if (target2 == null || target2.getOwner() == constraintWidget) {
                constraintWidget = owner;
            } else {
                constraintWidget2 = constraintWidget;
            }
        }
        return constraintWidget2;
    }

    private boolean isChainHead(int i) {
        int i2 = i * 2;
        if (this.mListAnchors[i2].mTarget == null) {
            return false;
        }
        ConstraintAnchor constraintAnchor = this.mListAnchors[i2].mTarget.mTarget;
        ConstraintAnchor[] constraintAnchorArr = this.mListAnchors;
        if (constraintAnchor == constraintAnchorArr[i2]) {
            return false;
        }
        int i3 = i2 + 1;
        return constraintAnchorArr[i3].mTarget != null && this.mListAnchors[i3].mTarget.mTarget == this.mListAnchors[i3];
    }

    /* JADX WARN: Code duplicated, block: B:143:0x025e  */
    /* JADX WARN: Code duplicated, block: B:147:0x0267 A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:151:0x0273  */
    /* JADX WARN: Code duplicated, block: B:154:0x027a  */
    /* JADX WARN: Code duplicated, block: B:156:0x027e  */
    /* JADX WARN: Code duplicated, block: B:159:0x029c  */
    /* JADX WARN: Code duplicated, block: B:161:0x02a4  */
    /* JADX WARN: Code duplicated, block: B:174:0x02e3  */
    /* JADX WARN: Code duplicated, block: B:175:0x02ea  */
    /* JADX WARN: Code duplicated, block: B:178:0x02f0  */
    /* JADX WARN: Code duplicated, block: B:179:0x02f7  */
    /* JADX WARN: Code duplicated, block: B:181:0x034c  */
    /* JADX WARN: Code duplicated, block: B:184:0x0366  */
    /* JADX WARN: Code duplicated, block: B:197:0x03b3  */
    /* JADX WARN: Code duplicated, block: B:200:0x03c3  */
    /* JADX WARN: Code duplicated, block: B:201:0x03c5  */
    /* JADX WARN: Code duplicated, block: B:203:0x03c8  */
    /* JADX WARN: Code duplicated, block: B:205:0x03d0  */
    /* JADX WARN: Code duplicated, block: B:208:0x03d6  */
    /* JADX WARN: Code duplicated, block: B:210:0x03d9  */
    /* JADX WARN: Code duplicated, block: B:211:0x03db  */
    /* JADX WARN: Code duplicated, block: B:213:0x03df  */
    /* JADX WARN: Code duplicated, block: B:218:0x03e9  */
    /* JADX WARN: Code duplicated, block: B:221:0x03ef  */
    /* JADX WARN: Code duplicated, block: B:222:0x03f6  */
    /* JADX WARN: Code duplicated, block: B:225:0x03fc  */
    /* JADX WARN: Code duplicated, block: B:228:0x0406  */
    /* JADX WARN: Code duplicated, block: B:230:0x040a  */
    /* JADX WARN: Code duplicated, block: B:232:0x0417  */
    /* JADX WARN: Code duplicated, block: B:234:0x0424  */
    /* JADX WARN: Code duplicated, block: B:236:0x0431  */
    /* JADX WARN: Code duplicated, block: B:238:0x0435  */
    /* JADX WARN: Code duplicated, block: B:240:0x0483  */
    /* JADX WARN: Code duplicated, block: B:243:0x048a  */
    /* JADX WARN: Code duplicated, block: B:245:0x0491  */
    /* JADX WARN: Code duplicated, block: B:246:0x04a1  */
    /* JADX WARN: Code duplicated, block: B:247:0x04b2  */
    /* JADX WARN: Code duplicated, block: B:250:0x04bc  */
    /* JADX WARN: Code duplicated, block: B:253:? A[RETURN, SYNTHETIC] */
    /* JADX WARN: Multi-variable type inference failed */
    public void addToSolver(LinearSystem linearSystem) {
        boolean z;
        boolean z2;
        boolean z3;
        boolean z4;
        SolverVariable solverVariable;
        int i;
        boolean z5;
        boolean z6;
        boolean z7;
        boolean z8;
        boolean z9;
        boolean z10;
        boolean z11;
        boolean z12;
        boolean z13;
        SolverVariable solverVariable2;
        int i2;
        int i3;
        SolverVariable solverVariable3;
        SolverVariable solverVariable4;
        SolverVariable solverVariable5;
        int i4;
        int i5;
        int i6;
        int i7;
        int i8;
        LinearSystem linearSystem2;
        int i9;
        int i10;
        boolean z14;
        ConstraintWidget constraintWidget;
        SolverVariable solverVariableCreateObjectVariable;
        boolean z15;
        int i11;
        ConstraintWidget constraintWidget2;
        SolverVariable solverVariableCreateObjectVariable2;
        ConstraintWidget constraintWidget3;
        SolverVariable solverVariableCreateObjectVariable3;
        int i12;
        boolean zIsInHorizontalChain;
        boolean zIsInVerticalChain;
        LinearSystem linearSystem3 = linearSystem;
        SolverVariable solverVariableCreateObjectVariable4 = linearSystem3.createObjectVariable(this.mLeft);
        SolverVariable solverVariableCreateObjectVariable5 = linearSystem3.createObjectVariable(this.mRight);
        SolverVariable solverVariableCreateObjectVariable6 = linearSystem3.createObjectVariable(this.mTop);
        SolverVariable solverVariableCreateObjectVariable7 = linearSystem3.createObjectVariable(this.mBottom);
        SolverVariable solverVariableCreateObjectVariable8 = linearSystem3.createObjectVariable(this.mBaseline);
        if (LinearSystem.sMetrics != null) {
            LinearSystem.sMetrics.widgets++;
        }
        if (this.horizontalRun.start.resolved && this.horizontalRun.end.resolved && this.verticalRun.start.resolved && this.verticalRun.end.resolved) {
            if (LinearSystem.sMetrics != null) {
                LinearSystem.sMetrics.graphSolved++;
            }
            linearSystem3.addEquality(solverVariableCreateObjectVariable4, this.horizontalRun.start.value);
            linearSystem3.addEquality(solverVariableCreateObjectVariable5, this.horizontalRun.end.value);
            linearSystem3.addEquality(solverVariableCreateObjectVariable6, this.verticalRun.start.value);
            linearSystem3.addEquality(solverVariableCreateObjectVariable7, this.verticalRun.end.value);
            linearSystem3.addEquality(solverVariableCreateObjectVariable8, this.verticalRun.baseline.value);
            ConstraintWidget constraintWidget4 = this.mParent;
            if (constraintWidget4 != null) {
                boolean z16 = constraintWidget4 != null && constraintWidget4.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT;
                ConstraintWidget constraintWidget5 = this.mParent;
                boolean z17 = constraintWidget5 != null && constraintWidget5.mListDimensionBehaviors[1] == DimensionBehaviour.WRAP_CONTENT;
                if (z16 && this.isTerminalWidget[0] && !isInHorizontalChain()) {
                    linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(this.mParent.mRight), solverVariableCreateObjectVariable5, 0, 8);
                }
                if (z17 && this.isTerminalWidget[1] && !isInVerticalChain()) {
                    linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(this.mParent.mBottom), solverVariableCreateObjectVariable7, 0, 8);
                    return;
                }
            }
            return;
        }
        if (LinearSystem.sMetrics != null) {
            LinearSystem.sMetrics.linearSolved++;
        }
        ConstraintWidget constraintWidget6 = this.mParent;
        if (constraintWidget6 != null) {
            z = constraintWidget6 != null && constraintWidget6.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT;
            ConstraintWidget constraintWidget7 = this.mParent;
            z2 = constraintWidget7 != null && constraintWidget7.mListDimensionBehaviors[1] == DimensionBehaviour.WRAP_CONTENT;
            if (isChainHead(0)) {
                ((ConstraintWidgetContainer) this.mParent).addChain(this, 0);
                zIsInHorizontalChain = true;
            } else {
                zIsInHorizontalChain = isInHorizontalChain();
            }
            if (isChainHead(1)) {
                ((ConstraintWidgetContainer) this.mParent).addChain(this, 1);
                zIsInVerticalChain = true;
            } else {
                zIsInVerticalChain = isInVerticalChain();
            }
            if (!zIsInHorizontalChain && z && this.mVisibility != 8 && this.mLeft.mTarget == null && this.mRight.mTarget == null) {
                linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(this.mParent.mRight), solverVariableCreateObjectVariable5, 0, 1);
            }
            if (!zIsInVerticalChain && z2 && this.mVisibility != 8 && this.mTop.mTarget == null && this.mBottom.mTarget == null && this.mBaseline == null) {
                linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(this.mParent.mBottom), solverVariableCreateObjectVariable7, 0, 1);
            }
            z3 = zIsInHorizontalChain;
            z4 = zIsInVerticalChain;
        } else {
            z = false;
            z2 = false;
            z3 = false;
            z4 = false;
        }
        int i13 = this.mWidth;
        int i14 = this.mMinWidth;
        if (i13 < i14) {
            i13 = i14;
        }
        int i15 = this.mHeight;
        int i16 = this.mMinHeight;
        if (i15 < i16) {
            i15 = i16;
        }
        boolean z18 = this.mListDimensionBehaviors[0] != DimensionBehaviour.MATCH_CONSTRAINT;
        boolean z19 = this.mListDimensionBehaviors[1] != DimensionBehaviour.MATCH_CONSTRAINT;
        this.mResolvedDimensionRatioSide = this.mDimensionRatioSide;
        float f = this.mDimensionRatio;
        this.mResolvedDimensionRatio = f;
        int i17 = this.mMatchConstraintDefaultWidth;
        int i18 = this.mMatchConstraintDefaultHeight;
        if (f > 0.0f && this.mVisibility != 8) {
            solverVariable = solverVariableCreateObjectVariable7;
            if (this.mListDimensionBehaviors[0] == DimensionBehaviour.MATCH_CONSTRAINT && i17 == 0) {
                i17 = 3;
            }
            if (this.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT && i18 == 0) {
                i18 = 3;
            }
            if (this.mListDimensionBehaviors[0] == DimensionBehaviour.MATCH_CONSTRAINT && this.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT && i17 == 3 && i18 == 3) {
                setupDimensionRatio(z, z2, z18, z19);
            } else {
                if (this.mListDimensionBehaviors[0] == DimensionBehaviour.MATCH_CONSTRAINT && i17 == 3) {
                    this.mResolvedDimensionRatioSide = 0;
                    i13 = (int) (this.mResolvedDimensionRatio * this.mHeight);
                    i = i15;
                    if (this.mListDimensionBehaviors[1] != DimensionBehaviour.MATCH_CONSTRAINT) {
                        i17 = 4;
                    } else {
                        z5 = true;
                    }
                    int[] iArr = this.mResolvedMatchConstraintDefault;
                    iArr[0] = i17;
                    iArr[1] = i18;
                    this.mResolvedHasRatio = z5;
                    if (z5) {
                    }
                    if (this.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT) {
                        z7 = false;
                    } else {
                        z7 = false;
                    }
                    if (z7) {
                        i13 = 0;
                    }
                    z8 = !this.mCenter.isConnected();
                    boolean[] zArr = this.mIsInBarrier;
                    z9 = zArr[0];
                    z10 = zArr[1];
                    z11 = z5;
                    if (this.mHorizontalResolution == 2) {
                        z12 = z;
                        z13 = z2;
                        solverVariable2 = solverVariableCreateObjectVariable6;
                        i2 = i18;
                        i3 = i17;
                    } else {
                        if (this.horizontalRun.start.resolved) {
                        }
                        constraintWidget2 = this.mParent;
                        if (constraintWidget2 != null) {
                            solverVariableCreateObjectVariable2 = linearSystem3.createObjectVariable(constraintWidget2.mRight);
                        } else {
                            solverVariableCreateObjectVariable2 = null;
                        }
                        constraintWidget3 = this.mParent;
                        if (constraintWidget3 != null) {
                            solverVariableCreateObjectVariable3 = linearSystem3.createObjectVariable(constraintWidget3.mLeft);
                        } else {
                            solverVariableCreateObjectVariable3 = null;
                        }
                        i2 = i18;
                        i3 = i17;
                        z12 = z;
                        solverVariable2 = solverVariableCreateObjectVariable6;
                        linearSystem3 = linearSystem;
                        SolverVariable solverVariable6 = solverVariableCreateObjectVariable3;
                        z13 = z2;
                        applyConstraints(linearSystem3, true, z12, z13, this.isTerminalWidget[0], solverVariable6, solverVariableCreateObjectVariable2, this.mListDimensionBehaviors[0], z7, this.mLeft, this.mRight, this.mX, i13, this.mMinWidth, this.mMaxDimension[0], this.mHorizontalBiasPercent, z6, z3, z4, z9, i3, i2, this.mMatchConstraintMinWidth, this.mMatchConstraintMaxWidth, this.mMatchConstraintPercentWidth, z8);
                    }
                    if (this.verticalRun.start.resolved) {
                        solverVariable3 = solverVariable2;
                        solverVariable4 = solverVariable;
                        solverVariable5 = solverVariableCreateObjectVariable8;
                        i4 = 0;
                        i5 = 1;
                        i6 = 8;
                        i7 = 1;
                    } else {
                        solverVariable3 = solverVariable2;
                        solverVariable4 = solverVariable;
                        solverVariable5 = solverVariableCreateObjectVariable8;
                        i4 = 0;
                        i5 = 1;
                        i6 = 8;
                        i7 = 1;
                    }
                    if (this.mVerticalResolution == 2) {
                        i8 = i4;
                    } else {
                        i8 = i7;
                    }
                    if (i8 == 0) {
                        if (this.mListDimensionBehaviors[i5] == DimensionBehaviour.WRAP_CONTENT) {
                            i9 = i4;
                        } else {
                            i9 = i4;
                        }
                        if (i9 != 0) {
                            i10 = i4;
                        } else {
                            i10 = i;
                        }
                        if (z11) {
                            z14 = i4;
                        } else {
                            z14 = i4;
                        }
                        constraintWidget = this.mParent;
                        if (constraintWidget != null) {
                            solverVariableCreateObjectVariable = linearSystem3.createObjectVariable(constraintWidget.mBottom);
                        } else {
                            solverVariableCreateObjectVariable = null;
                        }
                        ConstraintWidget constraintWidget8 = this.mParent;
                        if (constraintWidget8 != null) {
                        }
                        if (this.mBaselineDistance <= 0) {
                            z15 = z8;
                            linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                            if (this.mBaseline.mTarget != null) {
                                linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                                if (z13) {
                                    linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                                }
                                z15 = i4;
                            } else if (this.mVisibility == i6) {
                                z15 = z8;
                                linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                                z15 = z8;
                            }
                        } else {
                            z15 = z8;
                            linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                            if (this.mBaseline.mTarget != null) {
                                linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                                if (z13) {
                                    linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                                }
                                z15 = i4;
                            } else if (this.mVisibility == i6) {
                                z15 = z8;
                                linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                                z15 = z8;
                            }
                        }
                        z15 = z8;
                        z15 = z8;
                        applyConstraints(linearSystem, false, z13, z12, this.isTerminalWidget[i5], solverVariableCreateObjectVariable, solverVariableCreateObjectVariable, this.mListDimensionBehaviors[i5], i9, this.mTop, this.mBottom, this.mY, i10, this.mMinHeight, this.mMaxDimension[i5], this.mVerticalBiasPercent, z14, z4, z3, z10, i2, i3, this.mMatchConstraintMinHeight, this.mMatchConstraintMaxHeight, this.mMatchConstraintPercentHeight, z15);
                    }
                    if (z11 == 0) {
                        linearSystem2 = linearSystem;
                    } else if (this.mResolvedDimensionRatioSide == 1) {
                        linearSystem.addRatio(solverVariable4, solverVariable3, solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, this.mResolvedDimensionRatio, 8);
                        linearSystem2 = linearSystem;
                    } else {
                        linearSystem.addRatio(solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, solverVariable4, solverVariable3, this.mResolvedDimensionRatio, 8);
                        linearSystem2 = linearSystem;
                    }
                    if (this.mCenter.isConnected()) {
                        linearSystem2.addCenterPoint(this, this.mCenter.getTarget().getOwner(), (float) Math.toRadians(this.mCircleConstraintAngle + 90.0f), this.mCenter.getMargin());
                    }
                }
                if (this.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT && i18 == 3) {
                    this.mResolvedDimensionRatioSide = 1;
                    if (this.mDimensionRatioSide == -1) {
                        this.mResolvedDimensionRatio = 1.0f / this.mResolvedDimensionRatio;
                    }
                    i15 = (int) (this.mResolvedDimensionRatio * this.mWidth);
                    if (this.mListDimensionBehaviors[0] != DimensionBehaviour.MATCH_CONSTRAINT) {
                        i = i15;
                        i18 = 4;
                    }
                    int[] iArr2 = this.mResolvedMatchConstraintDefault;
                    iArr2[0] = i17;
                    iArr2[1] = i18;
                    this.mResolvedHasRatio = z5;
                    z6 = !z5 && ((i12 = this.mResolvedDimensionRatioSide) == 0 || i12 == -1);
                    if (this.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT || !(this instanceof ConstraintWidgetContainer)) {
                        z7 = false;
                    } else {
                        z7 = true;
                    }
                    if (z7) {
                        i13 = 0;
                    }
                    z8 = !this.mCenter.isConnected();
                    boolean[] zArr2 = this.mIsInBarrier;
                    z9 = zArr2[0];
                    z10 = zArr2[1];
                    z11 = z5;
                    if (this.mHorizontalResolution == 2) {
                        z12 = z;
                        z13 = z2;
                        solverVariable2 = solverVariableCreateObjectVariable6;
                        i2 = i18;
                        i3 = i17;
                    } else if (this.horizontalRun.start.resolved || !this.horizontalRun.end.resolved) {
                        constraintWidget2 = this.mParent;
                        if (constraintWidget2 != null) {
                            solverVariableCreateObjectVariable2 = linearSystem3.createObjectVariable(constraintWidget2.mRight);
                        } else {
                            solverVariableCreateObjectVariable2 = null;
                        }
                        constraintWidget3 = this.mParent;
                        if (constraintWidget3 != null) {
                            solverVariableCreateObjectVariable3 = linearSystem3.createObjectVariable(constraintWidget3.mLeft);
                        } else {
                            solverVariableCreateObjectVariable3 = null;
                        }
                        i2 = i18;
                        i3 = i17;
                        z12 = z;
                        solverVariable2 = solverVariableCreateObjectVariable6;
                        linearSystem3 = linearSystem;
                        SolverVariable solverVariable7 = solverVariableCreateObjectVariable3;
                        z13 = z2;
                        applyConstraints(linearSystem3, true, z12, z13, this.isTerminalWidget[0], solverVariable7, solverVariableCreateObjectVariable2, this.mListDimensionBehaviors[0], z7, this.mLeft, this.mRight, this.mX, i13, this.mMinWidth, this.mMaxDimension[0], this.mHorizontalBiasPercent, z6, z3, z4, z9, i3, i2, this.mMatchConstraintMinWidth, this.mMatchConstraintMaxWidth, this.mMatchConstraintPercentWidth, z8);
                    } else {
                        linearSystem3.addEquality(solverVariableCreateObjectVariable4, this.horizontalRun.start.value);
                        linearSystem3.addEquality(solverVariableCreateObjectVariable5, this.horizontalRun.end.value);
                        if (this.mParent != null && z && this.isTerminalWidget[0] && !isInHorizontalChain()) {
                            linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(this.mParent.mRight), solverVariableCreateObjectVariable5, 0, 8);
                        }
                        z12 = z;
                        z13 = z2;
                        solverVariable2 = solverVariableCreateObjectVariable6;
                        i2 = i18;
                        i3 = i17;
                    }
                    if (this.verticalRun.start.resolved || !this.verticalRun.end.resolved) {
                        solverVariable3 = solverVariable2;
                        solverVariable4 = solverVariable;
                        solverVariable5 = solverVariableCreateObjectVariable8;
                        i4 = 0;
                        i5 = 1;
                        i6 = 8;
                        i7 = 1;
                    } else {
                        solverVariable3 = solverVariable2;
                        linearSystem3.addEquality(solverVariable3, this.verticalRun.start.value);
                        solverVariable4 = solverVariable;
                        linearSystem3.addEquality(solverVariable4, this.verticalRun.end.value);
                        solverVariable5 = solverVariableCreateObjectVariable8;
                        linearSystem3.addEquality(solverVariable5, this.verticalRun.baseline.value);
                        ConstraintWidget constraintWidget9 = this.mParent;
                        if (constraintWidget9 == null || z4 || !z13) {
                            i4 = 0;
                            i5 = 1;
                        } else {
                            i5 = 1;
                            if (this.isTerminalWidget[1]) {
                                i4 = 0;
                                i6 = 8;
                                linearSystem3.addGreaterThan(linearSystem3.createObjectVariable(constraintWidget9.mBottom), solverVariable4, 0, 8);
                            } else {
                                i4 = 0;
                            }
                            i7 = i4;
                        }
                        i6 = 8;
                        i7 = i4;
                    }
                    if (this.mVerticalResolution == 2) {
                        i8 = i4;
                    } else {
                        i8 = i7;
                    }
                    if (i8 == 0) {
                        if (this.mListDimensionBehaviors[i5] == DimensionBehaviour.WRAP_CONTENT || !(this instanceof ConstraintWidgetContainer)) {
                            i9 = i4;
                        } else {
                            i9 = i5;
                        }
                        if (i9 != 0) {
                            i10 = i4;
                        } else {
                            i10 = i;
                        }
                        if (z11 || !((i11 = this.mResolvedDimensionRatioSide) == i5 || i11 == -1)) {
                            z14 = i4;
                        } else {
                            z14 = i5;
                        }
                        constraintWidget = this.mParent;
                        if (constraintWidget != null) {
                            solverVariableCreateObjectVariable = linearSystem3.createObjectVariable(constraintWidget.mBottom);
                        } else {
                            solverVariableCreateObjectVariable = null;
                        }
                        ConstraintWidget constraintWidget10 = this.mParent;
                        SolverVariable solverVariableCreateObjectVariable9 = constraintWidget10 != null ? linearSystem3.createObjectVariable(constraintWidget10.mTop) : null;
                        if (this.mBaselineDistance <= 0 || this.mVisibility == i6) {
                            z15 = z8;
                            linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                            if (this.mBaseline.mTarget != null) {
                                linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                                if (z13) {
                                    linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                                }
                                z15 = i4;
                            } else if (this.mVisibility == i6) {
                                z15 = z8;
                                linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                                z15 = z8;
                            }
                        }
                        z15 = z8;
                        z15 = z8;
                        applyConstraints(linearSystem, false, z13, z12, this.isTerminalWidget[i5], solverVariableCreateObjectVariable9, solverVariableCreateObjectVariable, this.mListDimensionBehaviors[i5], i9, this.mTop, this.mBottom, this.mY, i10, this.mMinHeight, this.mMaxDimension[i5], this.mVerticalBiasPercent, z14, z4, z3, z10, i2, i3, this.mMatchConstraintMinHeight, this.mMatchConstraintMaxHeight, this.mMatchConstraintPercentHeight, z15);
                    }
                    if (z11 == 0) {
                        linearSystem2 = linearSystem;
                    } else if (this.mResolvedDimensionRatioSide == 1) {
                        linearSystem.addRatio(solverVariable4, solverVariable3, solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, this.mResolvedDimensionRatio, 8);
                        linearSystem2 = linearSystem;
                    } else {
                        linearSystem.addRatio(solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, solverVariable4, solverVariable3, this.mResolvedDimensionRatio, 8);
                        linearSystem2 = linearSystem;
                    }
                    if (this.mCenter.isConnected()) {
                        linearSystem2.addCenterPoint(this, this.mCenter.getTarget().getOwner(), (float) Math.toRadians(this.mCircleConstraintAngle + 90.0f), this.mCenter.getMargin());
                    }
                }
            }
            i = i15;
            z5 = true;
            int[] iArr3 = this.mResolvedMatchConstraintDefault;
            iArr3[0] = i17;
            iArr3[1] = i18;
            this.mResolvedHasRatio = z5;
            if (z5) {
            }
            if (this.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT) {
                z7 = false;
            } else {
                z7 = false;
            }
            if (z7) {
                i13 = 0;
            }
            z8 = !this.mCenter.isConnected();
            boolean[] zArr3 = this.mIsInBarrier;
            z9 = zArr3[0];
            z10 = zArr3[1];
            z11 = z5;
            if (this.mHorizontalResolution == 2) {
                z12 = z;
                z13 = z2;
                solverVariable2 = solverVariableCreateObjectVariable6;
                i2 = i18;
                i3 = i17;
            } else {
                if (this.horizontalRun.start.resolved) {
                }
                constraintWidget2 = this.mParent;
                if (constraintWidget2 != null) {
                    solverVariableCreateObjectVariable2 = linearSystem3.createObjectVariable(constraintWidget2.mRight);
                } else {
                    solverVariableCreateObjectVariable2 = null;
                }
                constraintWidget3 = this.mParent;
                if (constraintWidget3 != null) {
                    solverVariableCreateObjectVariable3 = linearSystem3.createObjectVariable(constraintWidget3.mLeft);
                } else {
                    solverVariableCreateObjectVariable3 = null;
                }
                i2 = i18;
                i3 = i17;
                z12 = z;
                solverVariable2 = solverVariableCreateObjectVariable6;
                linearSystem3 = linearSystem;
                SolverVariable solverVariable8 = solverVariableCreateObjectVariable3;
                z13 = z2;
                applyConstraints(linearSystem3, true, z12, z13, this.isTerminalWidget[0], solverVariable8, solverVariableCreateObjectVariable2, this.mListDimensionBehaviors[0], z7, this.mLeft, this.mRight, this.mX, i13, this.mMinWidth, this.mMaxDimension[0], this.mHorizontalBiasPercent, z6, z3, z4, z9, i3, i2, this.mMatchConstraintMinWidth, this.mMatchConstraintMaxWidth, this.mMatchConstraintPercentWidth, z8);
            }
            if (this.verticalRun.start.resolved) {
                solverVariable3 = solverVariable2;
                solverVariable4 = solverVariable;
                solverVariable5 = solverVariableCreateObjectVariable8;
                i4 = 0;
                i5 = 1;
                i6 = 8;
                i7 = 1;
            } else {
                solverVariable3 = solverVariable2;
                solverVariable4 = solverVariable;
                solverVariable5 = solverVariableCreateObjectVariable8;
                i4 = 0;
                i5 = 1;
                i6 = 8;
                i7 = 1;
            }
            if (this.mVerticalResolution == 2) {
                i8 = i4;
            } else {
                i8 = i7;
            }
            if (i8 == 0) {
                if (this.mListDimensionBehaviors[i5] == DimensionBehaviour.WRAP_CONTENT) {
                    i9 = i4;
                } else {
                    i9 = i4;
                }
                if (i9 != 0) {
                    i10 = i4;
                } else {
                    i10 = i;
                }
                if (z11) {
                    z14 = i4;
                } else {
                    z14 = i4;
                }
                constraintWidget = this.mParent;
                if (constraintWidget != null) {
                    solverVariableCreateObjectVariable = linearSystem3.createObjectVariable(constraintWidget.mBottom);
                } else {
                    solverVariableCreateObjectVariable = null;
                }
                ConstraintWidget constraintWidget11 = this.mParent;
                if (constraintWidget11 != null) {
                }
                if (this.mBaselineDistance <= 0) {
                    z15 = z8;
                    linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                    if (this.mBaseline.mTarget != null) {
                        linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                        if (z13) {
                            linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                        }
                        z15 = i4;
                    } else if (this.mVisibility == i6) {
                        z15 = z8;
                        linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                        z15 = z8;
                    }
                } else {
                    z15 = z8;
                    linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                    if (this.mBaseline.mTarget != null) {
                        linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                        if (z13) {
                            linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                        }
                        z15 = i4;
                    } else if (this.mVisibility == i6) {
                        z15 = z8;
                        linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                        z15 = z8;
                    }
                }
                z15 = z8;
                z15 = z8;
                applyConstraints(linearSystem, false, z13, z12, this.isTerminalWidget[i5], solverVariableCreateObjectVariable9, solverVariableCreateObjectVariable, this.mListDimensionBehaviors[i5], i9, this.mTop, this.mBottom, this.mY, i10, this.mMinHeight, this.mMaxDimension[i5], this.mVerticalBiasPercent, z14, z4, z3, z10, i2, i3, this.mMatchConstraintMinHeight, this.mMatchConstraintMaxHeight, this.mMatchConstraintPercentHeight, z15);
            }
            if (z11 == 0) {
                linearSystem2 = linearSystem;
            } else if (this.mResolvedDimensionRatioSide == 1) {
                linearSystem.addRatio(solverVariable4, solverVariable3, solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, this.mResolvedDimensionRatio, 8);
                linearSystem2 = linearSystem;
            } else {
                linearSystem.addRatio(solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, solverVariable4, solverVariable3, this.mResolvedDimensionRatio, 8);
                linearSystem2 = linearSystem;
            }
            if (this.mCenter.isConnected()) {
                linearSystem2.addCenterPoint(this, this.mCenter.getTarget().getOwner(), (float) Math.toRadians(this.mCircleConstraintAngle + 90.0f), this.mCenter.getMargin());
            }
        }
        solverVariable = solverVariableCreateObjectVariable7;
        i = i15;
        z5 = false;
        int[] iArr4 = this.mResolvedMatchConstraintDefault;
        iArr4[0] = i17;
        iArr4[1] = i18;
        this.mResolvedHasRatio = z5;
        if (z5) {
        }
        if (this.mListDimensionBehaviors[0] == DimensionBehaviour.WRAP_CONTENT) {
            z7 = false;
        } else {
            z7 = false;
        }
        if (z7) {
            i13 = 0;
        }
        z8 = !this.mCenter.isConnected();
        boolean[] zArr4 = this.mIsInBarrier;
        z9 = zArr4[0];
        z10 = zArr4[1];
        z11 = z5;
        if (this.mHorizontalResolution == 2) {
            z12 = z;
            z13 = z2;
            solverVariable2 = solverVariableCreateObjectVariable6;
            i2 = i18;
            i3 = i17;
        } else {
            if (this.horizontalRun.start.resolved) {
            }
            constraintWidget2 = this.mParent;
            if (constraintWidget2 != null) {
                solverVariableCreateObjectVariable2 = linearSystem3.createObjectVariable(constraintWidget2.mRight);
            } else {
                solverVariableCreateObjectVariable2 = null;
            }
            constraintWidget3 = this.mParent;
            if (constraintWidget3 != null) {
                solverVariableCreateObjectVariable3 = linearSystem3.createObjectVariable(constraintWidget3.mLeft);
            } else {
                solverVariableCreateObjectVariable3 = null;
            }
            i2 = i18;
            i3 = i17;
            z12 = z;
            solverVariable2 = solverVariableCreateObjectVariable6;
            linearSystem3 = linearSystem;
            SolverVariable solverVariable9 = solverVariableCreateObjectVariable3;
            z13 = z2;
            applyConstraints(linearSystem3, true, z12, z13, this.isTerminalWidget[0], solverVariable9, solverVariableCreateObjectVariable2, this.mListDimensionBehaviors[0], z7, this.mLeft, this.mRight, this.mX, i13, this.mMinWidth, this.mMaxDimension[0], this.mHorizontalBiasPercent, z6, z3, z4, z9, i3, i2, this.mMatchConstraintMinWidth, this.mMatchConstraintMaxWidth, this.mMatchConstraintPercentWidth, z8);
        }
        if (this.verticalRun.start.resolved) {
            solverVariable3 = solverVariable2;
            solverVariable4 = solverVariable;
            solverVariable5 = solverVariableCreateObjectVariable8;
            i4 = 0;
            i5 = 1;
            i6 = 8;
            i7 = 1;
        } else {
            solverVariable3 = solverVariable2;
            solverVariable4 = solverVariable;
            solverVariable5 = solverVariableCreateObjectVariable8;
            i4 = 0;
            i5 = 1;
            i6 = 8;
            i7 = 1;
        }
        if (this.mVerticalResolution == 2) {
            i8 = i4;
        } else {
            i8 = i7;
        }
        if (i8 == 0) {
            if (this.mListDimensionBehaviors[i5] == DimensionBehaviour.WRAP_CONTENT) {
                i9 = i4;
            } else {
                i9 = i4;
            }
            if (i9 != 0) {
                i10 = i4;
            } else {
                i10 = i;
            }
            if (z11) {
                z14 = i4;
            } else {
                z14 = i4;
            }
            constraintWidget = this.mParent;
            if (constraintWidget != null) {
                solverVariableCreateObjectVariable = linearSystem3.createObjectVariable(constraintWidget.mBottom);
            } else {
                solverVariableCreateObjectVariable = null;
            }
            ConstraintWidget constraintWidget12 = this.mParent;
            if (constraintWidget12 != null) {
            }
            if (this.mBaselineDistance <= 0) {
                z15 = z8;
                linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                if (this.mBaseline.mTarget != null) {
                    linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                    if (z13) {
                        linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                    }
                    z15 = i4;
                } else if (this.mVisibility == i6) {
                    z15 = z8;
                    linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                    z15 = z8;
                }
            } else {
                z15 = z8;
                linearSystem3.addEquality(solverVariable5, solverVariable3, getBaselineDistance(), i6);
                if (this.mBaseline.mTarget != null) {
                    linearSystem3.addEquality(solverVariable5, linearSystem3.createObjectVariable(this.mBaseline.mTarget), i4, i6);
                    if (z13) {
                        linearSystem3.addGreaterThan(solverVariableCreateObjectVariable, linearSystem3.createObjectVariable(this.mBottom), i4, 5);
                    }
                    z15 = i4;
                } else if (this.mVisibility == i6) {
                    z15 = z8;
                    linearSystem3.addEquality(solverVariable5, solverVariable3, i4, i6);
                    z15 = z8;
                }
            }
            z15 = z8;
            z15 = z8;
            applyConstraints(linearSystem, false, z13, z12, this.isTerminalWidget[i5], solverVariableCreateObjectVariable9, solverVariableCreateObjectVariable, this.mListDimensionBehaviors[i5], i9, this.mTop, this.mBottom, this.mY, i10, this.mMinHeight, this.mMaxDimension[i5], this.mVerticalBiasPercent, z14, z4, z3, z10, i2, i3, this.mMatchConstraintMinHeight, this.mMatchConstraintMaxHeight, this.mMatchConstraintPercentHeight, z15);
        }
        if (z11 == 0) {
            linearSystem2 = linearSystem;
        } else if (this.mResolvedDimensionRatioSide == 1) {
            linearSystem.addRatio(solverVariable4, solverVariable3, solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, this.mResolvedDimensionRatio, 8);
            linearSystem2 = linearSystem;
        } else {
            linearSystem.addRatio(solverVariableCreateObjectVariable5, solverVariableCreateObjectVariable4, solverVariable4, solverVariable3, this.mResolvedDimensionRatio, 8);
            linearSystem2 = linearSystem;
        }
        if (this.mCenter.isConnected()) {
            linearSystem2.addCenterPoint(this, this.mCenter.getTarget().getOwner(), (float) Math.toRadians(this.mCircleConstraintAngle + 90.0f), this.mCenter.getMargin());
        }
    }

    boolean addFirst() {
        return (this instanceof VirtualLayout) || (this instanceof Guideline);
    }

    public void setupDimensionRatio(boolean z, boolean z2, boolean z3, boolean z4) {
        if (this.mResolvedDimensionRatioSide == -1) {
            if (z3 && !z4) {
                this.mResolvedDimensionRatioSide = 0;
            } else if (!z3 && z4) {
                this.mResolvedDimensionRatioSide = 1;
                if (this.mDimensionRatioSide == -1) {
                    this.mResolvedDimensionRatio = 1.0f / this.mResolvedDimensionRatio;
                }
            }
        }
        if (this.mResolvedDimensionRatioSide == 0 && (!this.mTop.isConnected() || !this.mBottom.isConnected())) {
            this.mResolvedDimensionRatioSide = 1;
        } else if (this.mResolvedDimensionRatioSide == 1 && (!this.mLeft.isConnected() || !this.mRight.isConnected())) {
            this.mResolvedDimensionRatioSide = 0;
        }
        if (this.mResolvedDimensionRatioSide == -1 && (!this.mTop.isConnected() || !this.mBottom.isConnected() || !this.mLeft.isConnected() || !this.mRight.isConnected())) {
            if (this.mTop.isConnected() && this.mBottom.isConnected()) {
                this.mResolvedDimensionRatioSide = 0;
            } else if (this.mLeft.isConnected() && this.mRight.isConnected()) {
                this.mResolvedDimensionRatio = 1.0f / this.mResolvedDimensionRatio;
                this.mResolvedDimensionRatioSide = 1;
            }
        }
        if (this.mResolvedDimensionRatioSide == -1) {
            int i = this.mMatchConstraintMinWidth;
            if (i > 0 && this.mMatchConstraintMinHeight == 0) {
                this.mResolvedDimensionRatioSide = 0;
            } else {
                if (i != 0 || this.mMatchConstraintMinHeight <= 0) {
                    return;
                }
                this.mResolvedDimensionRatio = 1.0f / this.mResolvedDimensionRatio;
                this.mResolvedDimensionRatioSide = 1;
            }
        }
    }

    /* JADX WARN: Code duplicated, block: B:100:0x017b  */
    /* JADX WARN: Code duplicated, block: B:102:0x01b2  */
    /* JADX WARN: Code duplicated, block: B:105:0x01c2 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:106:0x01c4  */
    /* JADX WARN: Code duplicated, block: B:107:0x01cb  */
    /* JADX WARN: Code duplicated, block: B:108:0x01cd A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:111:0x01d3 A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:115:0x01db A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:119:0x01f0  */
    /* JADX WARN: Code duplicated, block: B:121:0x01f3 A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:184:0x02c7  */
    /* JADX WARN: Code duplicated, block: B:186:0x02cd  */
    /* JADX WARN: Code duplicated, block: B:188:0x02d3  */
    /* JADX WARN: Code duplicated, block: B:190:0x02f5  */
    /* JADX WARN: Code duplicated, block: B:194:0x0304  */
    /* JADX WARN: Code duplicated, block: B:195:0x0306  */
    /* JADX WARN: Code duplicated, block: B:203:0x0317  */
    /* JADX WARN: Code duplicated, block: B:212:0x033c  */
    /* JADX WARN: Code duplicated, block: B:214:0x0344  */
    /* JADX WARN: Code duplicated, block: B:225:0x0359  */
    /* JADX WARN: Code duplicated, block: B:230:0x0363  */
    /* JADX WARN: Code duplicated, block: B:232:0x0367  */
    /* JADX WARN: Code duplicated, block: B:233:0x0369  */
    /* JADX WARN: Code duplicated, block: B:236:0x0371  */
    /* JADX WARN: Code duplicated, block: B:242:0x037f A[PHI: r4
  0x037f: PHI (r4v31 int) = (r4v30 int), (r4v35 int), (r4v35 int), (r4v35 int) binds: [B:235:0x036f, B:237:0x0375, B:238:0x0377, B:240:0x037b] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:245:0x0391 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:246:0x0393  */
    /* JADX WARN: Code duplicated, block: B:247:0x0398  */
    /* JADX WARN: Code duplicated, block: B:249:0x039b  */
    /* JADX WARN: Code duplicated, block: B:258:0x03b1  */
    /* JADX WARN: Code duplicated, block: B:259:0x03b7  */
    /* JADX WARN: Code duplicated, block: B:261:0x03ba A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:270:0x03d1  */
    /* JADX WARN: Code duplicated, block: B:272:0x03d8 A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:296:? A[ADDED_TO_REGION, RETURN, SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:299:? A[ADDED_TO_REGION, RETURN, SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:303:? A[RETURN, SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:31:0x0085  */
    /* JADX WARN: Code duplicated, block: B:32:0x0089  */
    /* JADX WARN: Code duplicated, block: B:34:0x008d  */
    /* JADX WARN: Code duplicated, block: B:38:0x0099 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:39:0x009b A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:41:0x00a9  */
    /* JADX WARN: Code duplicated, block: B:42:0x00ae  */
    /* JADX WARN: Code duplicated, block: B:44:0x00b3 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:45:0x00b5  */
    /* JADX WARN: Code duplicated, block: B:47:0x00be  */
    /* JADX WARN: Code duplicated, block: B:50:0x00c6  */
    /* JADX WARN: Code duplicated, block: B:51:0x00ca  */
    /* JADX WARN: Code duplicated, block: B:54:0x00da  */
    /* JADX WARN: Code duplicated, block: B:66:0x0100  */
    /* JADX WARN: Code duplicated, block: B:67:0x0102  */
    /* JADX WARN: Code duplicated, block: B:69:0x0105  */
    /* JADX WARN: Code duplicated, block: B:70:0x0107  */
    /* JADX WARN: Code duplicated, block: B:77:0x0112  */
    /* JADX WARN: Code duplicated, block: B:80:0x011c  */
    /* JADX WARN: Code duplicated, block: B:83:0x0121  */
    /* JADX WARN: Code duplicated, block: B:86:0x012a A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:87:0x012c  */
    /* JADX WARN: Code duplicated, block: B:88:0x0130 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:89:0x0132  */
    /* JADX WARN: Code duplicated, block: B:90:0x013a  */
    /* JADX WARN: Code duplicated, block: B:92:0x014e  */
    /* JADX WARN: Code duplicated, block: B:94:0x0151  */
    private void applyConstraints(LinearSystem linearSystem, boolean z, boolean z2, boolean z3, boolean z4, SolverVariable solverVariable, SolverVariable solverVariable2, DimensionBehaviour dimensionBehaviour, boolean z5, ConstraintAnchor constraintAnchor, ConstraintAnchor constraintAnchor2, int i, int i2, int i3, int i4, float f, boolean z6, boolean z7, boolean z8, boolean z9, int i5, int i6, int i7, int i8, float f2, boolean z10) {
        int i9;
        boolean z11;
        int iMin;
        int i10;
        int i11;
        int i12;
        boolean z12;
        SolverVariable solverVariableCreateObjectVariable;
        SolverVariable solverVariableCreateObjectVariable2;
        int i13;
        char c;
        boolean z13;
        boolean z14;
        boolean z15;
        int i14;
        int i15;
        boolean z16;
        boolean z17;
        boolean z18;
        SolverVariable solverVariable3;
        SolverVariable solverVariable4;
        int i16;
        ConstraintWidget constraintWidget;
        int iMin2;
        int i17;
        boolean z19;
        int margin;
        int i18;
        int i19;
        int i20;
        int i21;
        int i22;
        int i23;
        LinearSystem linearSystem2 = linearSystem;
        int i24 = i8;
        SolverVariable solverVariableCreateObjectVariable3 = linearSystem2.createObjectVariable(constraintAnchor);
        SolverVariable solverVariableCreateObjectVariable4 = linearSystem2.createObjectVariable(constraintAnchor2);
        SolverVariable solverVariableCreateObjectVariable5 = linearSystem2.createObjectVariable(constraintAnchor.getTarget());
        SolverVariable solverVariableCreateObjectVariable6 = linearSystem2.createObjectVariable(constraintAnchor2.getTarget());
        if (LinearSystem.getMetrics() != null) {
            LinearSystem.getMetrics().nonresolvedWidgets++;
        }
        boolean zIsConnected = constraintAnchor.isConnected();
        boolean zIsConnected2 = constraintAnchor2.isConnected();
        boolean zIsConnected3 = this.mCenter.isConnected();
        int i25 = zIsConnected2 ? (zIsConnected ? 1 : 0) + 1 : zIsConnected ? 1 : 0;
        if (zIsConnected3) {
            i25++;
        }
        int i26 = z6 ? 3 : i5;
        int i27 = AnonymousClass1.$SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[dimensionBehaviour.ordinal()];
        if (i27 != 1 && i27 != 2 && i27 != 3 && i27 == 4) {
            i9 = i26;
            if (i9 != 4) {
                z11 = true;
            }
            if (this.mVisibility == 8) {
                iMin = 0;
                z11 = false;
            } else {
                iMin = i2;
            }
            if (!z10) {
                if (zIsConnected && !zIsConnected2 && !zIsConnected3) {
                    linearSystem2.addEquality(solverVariableCreateObjectVariable3, i);
                } else if (zIsConnected && !zIsConnected2) {
                    i10 = 8;
                    linearSystem2.addEquality(solverVariableCreateObjectVariable3, solverVariableCreateObjectVariable5, constraintAnchor.getMargin(), 8);
                }
                i10 = 8;
            } else {
                i10 = 8;
            }
            if (!z11) {
                if (i25 == 2 && !z6 && (i9 == 1 || i9 == 0)) {
                    int iMax = Math.max(i7, iMin);
                    if (i24 > 0) {
                        iMax = Math.min(i24, iMax);
                    }
                    linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMax, 8);
                    z12 = false;
                } else {
                    if (i7 == -2) {
                        i11 = iMin;
                    } else {
                        i11 = i7;
                    }
                    if (i24 == -2) {
                        i12 = iMin;
                    } else {
                        i12 = i24;
                    }
                    if (iMin > 0 && i9 != 1) {
                        iMin = 0;
                    }
                    if (i11 > 0) {
                        linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i11, 8);
                        iMin = Math.max(iMin, i11);
                    }
                    if (i12 > 0) {
                        if (z2 || i9 != 1) {
                            linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i12, 8);
                        }
                        iMin = Math.min(iMin, i12);
                    }
                    if (i9 == 1) {
                        if (z2) {
                            linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                        } else if (z7) {
                            linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 5);
                            linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                        } else {
                            linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 5);
                            linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                        }
                        solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                        z12 = z11;
                        solverVariableCreateObjectVariable6 = solverVariableCreateObjectVariable6;
                        z4 = z4;
                        i11 = i11;
                        i24 = i12;
                        i25 = i25 == true ? 1 : 0;
                    } else {
                        if (i9 == 2) {
                            if (constraintAnchor.getType() != ConstraintAnchor.Type.TOP || constraintAnchor.getType() == ConstraintAnchor.Type.BOTTOM) {
                                solverVariableCreateObjectVariable = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.TOP));
                                solverVariableCreateObjectVariable2 = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.BOTTOM));
                            } else {
                                solverVariableCreateObjectVariable = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.LEFT));
                                solverVariableCreateObjectVariable2 = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.RIGHT));
                            }
                            SolverVariable solverVariable5 = solverVariableCreateObjectVariable2;
                            solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                            linearSystem2.addConstraint(linearSystem2.createRow().createRowDimensionRatio(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, solverVariable5, solverVariableCreateObjectVariable, f2));
                            z4 = z4;
                            z12 = false;
                        } else {
                            solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                            z12 = z11;
                            z4 = true;
                        }
                        i24 = i12;
                    }
                }
                if (z10) {
                    i13 = 8;
                    c = 2;
                } else {
                    if (z7) {
                        if ((!zIsConnected || zIsConnected2 || zIsConnected3) && (!zIsConnected || zIsConnected2)) {
                            if (zIsConnected && zIsConnected2) {
                                linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable6, -constraintAnchor2.getMargin(), 8);
                                if (z2) {
                                    linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable, 0, 5);
                                }
                            } else if (zIsConnected && zIsConnected2) {
                                ConstraintWidget constraintWidget2 = constraintAnchor.mTarget.mOwner;
                                ConstraintWidget constraintWidget3 = constraintAnchor2.mTarget.mOwner;
                                ConstraintWidget parent = getParent();
                                int i28 = 6;
                                if (z12) {
                                    if (i9 == 0) {
                                        if (i24 == 0 && i11 == 0) {
                                            i23 = 8;
                                            i14 = 8;
                                            z15 = false;
                                            z16 = true;
                                        } else {
                                            i23 = 5;
                                            i14 = 5;
                                            z15 = true;
                                            z16 = false;
                                        }
                                        if ((constraintWidget2 instanceof Barrier) || (constraintWidget3 instanceof Barrier)) {
                                            i14 = 4;
                                        }
                                        i15 = i23;
                                        z14 = false;
                                    } else if (i9 == 1) {
                                        i28 = 6;
                                        z14 = true;
                                        z15 = true;
                                        i14 = 4;
                                        i15 = 8;
                                        z16 = false;
                                    } else if (i9 == 3) {
                                        if (this.mResolvedDimensionRatioSide == -1) {
                                            if (z8) {
                                                i28 = z2 ? 5 : 4;
                                            } else {
                                                i28 = 8;
                                            }
                                            z14 = true;
                                            z15 = true;
                                            i14 = 5;
                                            i15 = 8;
                                        } else if (z6) {
                                            if (i6 == 2 || i6 == 1) {
                                                i21 = 5;
                                                i22 = 4;
                                            } else {
                                                i21 = 8;
                                                i22 = 5;
                                            }
                                            i15 = i21;
                                            i14 = i22;
                                            i28 = 6;
                                            z14 = true;
                                            z15 = true;
                                        } else {
                                            if (i24 > 0) {
                                                z14 = true;
                                                z15 = true;
                                                i14 = 5;
                                            } else if (i24 != 0 || i11 != 0) {
                                                z14 = true;
                                                z15 = true;
                                                i14 = 4;
                                            } else if (z8) {
                                                i15 = (constraintWidget2 == parent || constraintWidget3 == parent) ? 5 : 4;
                                                i28 = 6;
                                                z14 = true;
                                                z15 = true;
                                                i14 = 4;
                                            } else {
                                                z14 = true;
                                                z15 = true;
                                                i14 = 8;
                                            }
                                            i15 = 5;
                                        }
                                        z16 = true;
                                    } else {
                                        z14 = false;
                                        z15 = false;
                                    }
                                    if (z14 || solverVariableCreateObjectVariable5 != solverVariableCreateObjectVariable6 || constraintWidget2 == parent) {
                                        z17 = z14;
                                        z18 = true;
                                    } else {
                                        z17 = false;
                                        z18 = false;
                                    }
                                    if (z15) {
                                        if (this.mVisibility == 8) {
                                            i28 = 4;
                                        }
                                        SolverVariable solverVariable6 = solverVariableCreateObjectVariable6;
                                        i16 = 8;
                                        int i29 = i28;
                                        constraintWidget = constraintWidget3;
                                        solverVariable4 = solverVariableCreateObjectVariable5;
                                        linearSystem2 = linearSystem;
                                        SolverVariable solverVariable7 = solverVariableCreateObjectVariable4;
                                        linearSystem2.addCentering(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), f, solverVariable6, solverVariable7, constraintAnchor2.getMargin(), i29);
                                        solverVariableCreateObjectVariable3 = solverVariableCreateObjectVariable3;
                                        solverVariable3 = solverVariable6;
                                        solverVariableCreateObjectVariable4 = solverVariable7;
                                    } else {
                                        linearSystem2 = linearSystem;
                                        solverVariable3 = solverVariableCreateObjectVariable6;
                                        solverVariable4 = solverVariableCreateObjectVariable5;
                                        i16 = 8;
                                        constraintWidget = constraintWidget3;
                                    }
                                    if (this.mVisibility == i16) {
                                        return;
                                    }
                                    if (z17) {
                                        if (z2 || solverVariable4 == solverVariable3 || z12 || !((constraintWidget2 instanceof Barrier) || (constraintWidget instanceof Barrier))) {
                                            i20 = i15;
                                        } else {
                                            i20 = 6;
                                        }
                                        linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), i20);
                                        linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariable3, -constraintAnchor2.getMargin(), i20);
                                        i15 = i20;
                                    }
                                    if (z2 || !z9 || (constraintWidget2 instanceof Barrier) || (constraintWidget instanceof Barrier)) {
                                        iMin2 = i14;
                                        i17 = i15;
                                        z19 = z18;
                                    } else {
                                        iMin2 = 6;
                                        i17 = 6;
                                        z19 = true;
                                    }
                                    if (z19) {
                                        if (z16 && (!z8 || z3)) {
                                            if (constraintWidget2 != parent && constraintWidget != parent) {
                                                i28 = iMin2;
                                            }
                                            if ((constraintWidget2 instanceof Guideline) || (constraintWidget instanceof Guideline)) {
                                                i28 = 5;
                                            }
                                            if ((constraintWidget2 instanceof Barrier) || (constraintWidget instanceof Barrier)) {
                                                i28 = 5;
                                            }
                                            if (z8) {
                                                i19 = 5;
                                            } else {
                                                i19 = i28;
                                            }
                                            iMin2 = Math.max(i19, iMin2);
                                        }
                                        if (z2) {
                                            iMin2 = Math.min(i17, iMin2);
                                            if (z6 || z8 || !(constraintWidget2 == parent || constraintWidget == parent)) {
                                                i18 = iMin2;
                                            } else {
                                                i18 = 4;
                                            }
                                        } else {
                                            i18 = iMin2;
                                        }
                                        linearSystem2.addEquality(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), i18);
                                        linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariable3, -constraintAnchor2.getMargin(), i18);
                                    }
                                    if (z2) {
                                        if (solverVariable == solverVariable4) {
                                            margin = constraintAnchor.getMargin();
                                        } else {
                                            margin = 0;
                                        }
                                        if (solverVariable4 != solverVariable) {
                                            linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable, margin, 5);
                                        }
                                    }
                                    if (z2 && z12 && i3 == 0 && i11 == 0) {
                                        if (!z12 && i9 == 3) {
                                            linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, i16);
                                        } else {
                                            linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, 5);
                                        }
                                    }
                                } else {
                                    z14 = true;
                                    z15 = true;
                                }
                                i14 = 4;
                                i15 = 5;
                                z16 = false;
                                if (z14) {
                                    z17 = z14;
                                    z18 = true;
                                } else {
                                    z17 = z14;
                                    z18 = true;
                                }
                                if (z15) {
                                    if (this.mVisibility == 8) {
                                        i28 = 4;
                                    }
                                    SolverVariable solverVariable8 = solverVariableCreateObjectVariable6;
                                    i16 = 8;
                                    int i210 = i28;
                                    constraintWidget = constraintWidget3;
                                    solverVariable4 = solverVariableCreateObjectVariable5;
                                    linearSystem2 = linearSystem;
                                    SolverVariable solverVariable9 = solverVariableCreateObjectVariable4;
                                    linearSystem2.addCentering(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), f, solverVariable8, solverVariable9, constraintAnchor2.getMargin(), i210);
                                    solverVariableCreateObjectVariable3 = solverVariableCreateObjectVariable3;
                                    solverVariable3 = solverVariable8;
                                    solverVariableCreateObjectVariable4 = solverVariable9;
                                } else {
                                    linearSystem2 = linearSystem;
                                    solverVariable3 = solverVariableCreateObjectVariable6;
                                    solverVariable4 = solverVariableCreateObjectVariable5;
                                    i16 = 8;
                                    constraintWidget = constraintWidget3;
                                }
                                if (this.mVisibility == i16) {
                                    return;
                                }
                                if (z17) {
                                    if (z2) {
                                        i20 = i15;
                                    } else {
                                        i20 = i15;
                                    }
                                    linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), i20);
                                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariable3, -constraintAnchor2.getMargin(), i20);
                                    i15 = i20;
                                }
                                if (z2) {
                                    iMin2 = i14;
                                    i17 = i15;
                                    z19 = z18;
                                } else {
                                    iMin2 = i14;
                                    i17 = i15;
                                    z19 = z18;
                                }
                                if (z19) {
                                    if (z16) {
                                        if (constraintWidget2 != parent) {
                                            i28 = iMin2;
                                        }
                                        if (constraintWidget2 instanceof Guideline) {
                                            i28 = 5;
                                        } else {
                                            i28 = 5;
                                        }
                                        if (constraintWidget2 instanceof Barrier) {
                                            i28 = 5;
                                        } else {
                                            i28 = 5;
                                        }
                                        if (z8) {
                                            i19 = 5;
                                        } else {
                                            i19 = i28;
                                        }
                                        iMin2 = Math.max(i19, iMin2);
                                    }
                                    if (z2) {
                                        iMin2 = Math.min(i17, iMin2);
                                        if (z6) {
                                            i18 = iMin2;
                                        } else {
                                            i18 = iMin2;
                                        }
                                    } else {
                                        i18 = iMin2;
                                    }
                                    linearSystem2.addEquality(solverVariableCreateObjectVariable3, solverVariable4, constraintAnchor.getMargin(), i18);
                                    linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariable3, -constraintAnchor2.getMargin(), i18);
                                }
                                if (z2) {
                                    if (solverVariable == solverVariable4) {
                                        margin = constraintAnchor.getMargin();
                                    } else {
                                        margin = 0;
                                    }
                                    if (solverVariable4 != solverVariable) {
                                        linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable, margin, 5);
                                    }
                                }
                                if (z2) {
                                    if (!z12) {
                                        linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, 5);
                                    } else {
                                        linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, 5);
                                    }
                                }
                            }
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                        if (z2 || !z4) {
                            return;
                        }
                        int margin2 = constraintAnchor2.mTarget != null ? constraintAnchor2.getMargin() : 0;
                        if (solverVariable3 != solverVariable2) {
                            linearSystem2.addGreaterThan(solverVariable2, solverVariableCreateObjectVariable4, margin2, 5);
                            return;
                        }
                        return;
                    }
                    c = 2;
                    i13 = 8;
                }
                if (i25 >= c && z2 && z4) {
                    linearSystem2.addGreaterThan(solverVariableCreateObjectVariable3, solverVariable, 0, i13);
                    boolean z20 = z || this.mBaseline.mTarget == null;
                    if (z || this.mBaseline.mTarget == null) {
                        z13 = z20;
                    } else {
                        ConstraintWidget constraintWidget4 = this.mBaseline.mTarget.mOwner;
                        z13 = constraintWidget4.mDimensionRatio != 0.0f && constraintWidget4.mListDimensionBehaviors[0] == DimensionBehaviour.MATCH_CONSTRAINT && constraintWidget4.mListDimensionBehaviors[1] == DimensionBehaviour.MATCH_CONSTRAINT;
                    }
                    if (z13) {
                        linearSystem2.addGreaterThan(solverVariable2, solverVariableCreateObjectVariable4, 0, i13);
                        return;
                    }
                    return;
                }
                return;
            }
            if (z5) {
                linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, 3);
                if (i3 > 0) {
                    linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i3, 8);
                }
                if (i4 < Integer.MAX_VALUE) {
                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i4, 8);
                }
            } else {
                linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, i10);
            }
            z12 = z11;
            i11 = i7;
            if (z10) {
                i13 = 8;
                c = 2;
            } else {
                if (z7) {
                    if (zIsConnected) {
                        if (zIsConnected) {
                            if (zIsConnected) {
                                solverVariable3 = solverVariableCreateObjectVariable6;
                            } else {
                                solverVariable3 = solverVariableCreateObjectVariable6;
                            }
                        } else if (zIsConnected) {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                    } else if (zIsConnected) {
                        if (zIsConnected) {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                    } else if (zIsConnected) {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    } else {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    }
                    if (z2) {
                        return;
                    } else {
                        return;
                    }
                }
                c = 2;
                i13 = 8;
            }
            if (i25 >= c) {
            }
        }
        i9 = i26;
        z11 = false;
        if (this.mVisibility == 8) {
            iMin = 0;
            z11 = false;
        } else {
            iMin = i2;
        }
        if (!z10) {
            i10 = 8;
        } else if (zIsConnected) {
            if (zIsConnected) {
                i10 = 8;
            } else {
                i10 = 8;
            }
        } else if (zIsConnected) {
            i10 = 8;
        } else {
            i10 = 8;
        }
        if (!z11) {
            if (i25 == 2) {
            }
            if (i7 == -2) {
                i11 = iMin;
            } else {
                i11 = i7;
            }
            if (i24 == -2) {
                i12 = iMin;
            } else {
                i12 = i24;
            }
            if (iMin > 0) {
                iMin = 0;
            }
            if (i11 > 0) {
                linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i11, 8);
                iMin = Math.max(iMin, i11);
            }
            if (i12 > 0) {
                if (z2) {
                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i12, 8);
                } else {
                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i12, 8);
                }
                iMin = Math.min(iMin, i12);
            }
            if (i9 == 1) {
                if (z2) {
                    linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                } else if (z7) {
                    linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 5);
                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                } else {
                    linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 5);
                    linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, 8);
                }
                solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                z12 = z11;
                solverVariableCreateObjectVariable6 = solverVariableCreateObjectVariable6;
                z4 = z4;
                i11 = i11;
                i24 = i12;
                i25 = i25 == true ? 1 : 0;
            } else {
                if (i9 == 2) {
                    if (constraintAnchor.getType() != ConstraintAnchor.Type.TOP) {
                        solverVariableCreateObjectVariable = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.TOP));
                        solverVariableCreateObjectVariable2 = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.BOTTOM));
                    } else {
                        solverVariableCreateObjectVariable = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.TOP));
                        solverVariableCreateObjectVariable2 = linearSystem2.createObjectVariable(this.mParent.getAnchor(ConstraintAnchor.Type.BOTTOM));
                    }
                    SolverVariable solverVariable10 = solverVariableCreateObjectVariable2;
                    solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                    linearSystem2.addConstraint(linearSystem2.createRow().createRowDimensionRatio(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, solverVariable10, solverVariableCreateObjectVariable, f2));
                    z4 = z4;
                    z12 = false;
                } else {
                    solverVariableCreateObjectVariable4 = solverVariableCreateObjectVariable4;
                    z12 = z11;
                    z4 = true;
                }
                i24 = i12;
            }
            if (z10) {
                i13 = 8;
                c = 2;
            } else {
                if (z7) {
                    if (zIsConnected) {
                        if (zIsConnected) {
                            if (zIsConnected) {
                                solverVariable3 = solverVariableCreateObjectVariable6;
                            } else {
                                solverVariable3 = solverVariableCreateObjectVariable6;
                            }
                        } else if (zIsConnected) {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                    } else if (zIsConnected) {
                        if (zIsConnected) {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                    } else if (zIsConnected) {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    } else {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    }
                    if (z2) {
                        return;
                    } else {
                        return;
                    }
                }
                c = 2;
                i13 = 8;
            }
            if (i25 >= c) {
            }
        }
        if (z5) {
            linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, 0, 3);
            if (i3 > 0) {
                linearSystem2.addGreaterThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i3, 8);
            }
            if (i4 < Integer.MAX_VALUE) {
                linearSystem2.addLowerThan(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, i4, 8);
            }
        } else {
            linearSystem2.addEquality(solverVariableCreateObjectVariable4, solverVariableCreateObjectVariable3, iMin, i10);
        }
        z12 = z11;
        i11 = i7;
        if (z10) {
            i13 = 8;
            c = 2;
        } else {
            if (z7) {
                if (zIsConnected) {
                    if (zIsConnected) {
                        if (zIsConnected) {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        } else {
                            solverVariable3 = solverVariableCreateObjectVariable6;
                        }
                    } else if (zIsConnected) {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    } else {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    }
                } else if (zIsConnected) {
                    if (zIsConnected) {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    } else {
                        solverVariable3 = solverVariableCreateObjectVariable6;
                    }
                } else if (zIsConnected) {
                    solverVariable3 = solverVariableCreateObjectVariable6;
                } else {
                    solverVariable3 = solverVariableCreateObjectVariable6;
                }
                if (z2) {
                    return;
                } else {
                    return;
                }
            }
            c = 2;
            i13 = 8;
        }
        if (i25 >= c) {
        }
    }

    /* JADX INFO: renamed from: androidx.constraintlayout.solver.widgets.ConstraintWidget$1, reason: invalid class name */
    static /* synthetic */ class AnonymousClass1 {
        static final /* synthetic */ int[] $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type;
        static final /* synthetic */ int[] $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour;

        static {
            int[] iArr = new int[DimensionBehaviour.values().length];
            $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour = iArr;
            try {
                iArr[DimensionBehaviour.FIXED.ordinal()] = 1;
            } catch (NoSuchFieldError unused) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[DimensionBehaviour.WRAP_CONTENT.ordinal()] = 2;
            } catch (NoSuchFieldError unused2) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[DimensionBehaviour.MATCH_PARENT.ordinal()] = 3;
            } catch (NoSuchFieldError unused3) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintWidget$DimensionBehaviour[DimensionBehaviour.MATCH_CONSTRAINT.ordinal()] = 4;
            } catch (NoSuchFieldError unused4) {
            }
            int[] iArr2 = new int[ConstraintAnchor.Type.values().length];
            $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type = iArr2;
            try {
                iArr2[ConstraintAnchor.Type.LEFT.ordinal()] = 1;
            } catch (NoSuchFieldError unused5) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.TOP.ordinal()] = 2;
            } catch (NoSuchFieldError unused6) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.RIGHT.ordinal()] = 3;
            } catch (NoSuchFieldError unused7) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.BOTTOM.ordinal()] = 4;
            } catch (NoSuchFieldError unused8) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.BASELINE.ordinal()] = 5;
            } catch (NoSuchFieldError unused9) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.CENTER.ordinal()] = 6;
            } catch (NoSuchFieldError unused10) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.CENTER_X.ordinal()] = 7;
            } catch (NoSuchFieldError unused11) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.CENTER_Y.ordinal()] = 8;
            } catch (NoSuchFieldError unused12) {
            }
            try {
                $SwitchMap$androidx$constraintlayout$solver$widgets$ConstraintAnchor$Type[ConstraintAnchor.Type.NONE.ordinal()] = 9;
            } catch (NoSuchFieldError unused13) {
            }
        }
    }

    public void updateFromSolver(LinearSystem linearSystem) {
        int objectVariableValue = linearSystem.getObjectVariableValue(this.mLeft);
        int objectVariableValue2 = linearSystem.getObjectVariableValue(this.mTop);
        int objectVariableValue3 = linearSystem.getObjectVariableValue(this.mRight);
        int objectVariableValue4 = linearSystem.getObjectVariableValue(this.mBottom);
        if (this.horizontalRun.start.resolved && this.horizontalRun.end.resolved) {
            objectVariableValue = this.horizontalRun.start.value;
            objectVariableValue3 = this.horizontalRun.end.value;
        }
        if (this.verticalRun.start.resolved && this.verticalRun.end.resolved) {
            objectVariableValue2 = this.verticalRun.start.value;
            objectVariableValue4 = this.verticalRun.end.value;
        }
        int i = objectVariableValue4 - objectVariableValue2;
        if (objectVariableValue3 - objectVariableValue < 0 || i < 0 || objectVariableValue == Integer.MIN_VALUE || objectVariableValue == Integer.MAX_VALUE || objectVariableValue2 == Integer.MIN_VALUE || objectVariableValue2 == Integer.MAX_VALUE || objectVariableValue3 == Integer.MIN_VALUE || objectVariableValue3 == Integer.MAX_VALUE || objectVariableValue4 == Integer.MIN_VALUE || objectVariableValue4 == Integer.MAX_VALUE) {
            objectVariableValue = 0;
            objectVariableValue4 = 0;
            objectVariableValue2 = 0;
            objectVariableValue3 = 0;
        }
        setFrame(objectVariableValue, objectVariableValue2, objectVariableValue3, objectVariableValue4);
    }

    public void copy(ConstraintWidget constraintWidget, HashMap<ConstraintWidget, ConstraintWidget> map) {
        this.mHorizontalResolution = constraintWidget.mHorizontalResolution;
        this.mVerticalResolution = constraintWidget.mVerticalResolution;
        this.mMatchConstraintDefaultWidth = constraintWidget.mMatchConstraintDefaultWidth;
        this.mMatchConstraintDefaultHeight = constraintWidget.mMatchConstraintDefaultHeight;
        int[] iArr = this.mResolvedMatchConstraintDefault;
        int[] iArr2 = constraintWidget.mResolvedMatchConstraintDefault;
        iArr[0] = iArr2[0];
        iArr[1] = iArr2[1];
        this.mMatchConstraintMinWidth = constraintWidget.mMatchConstraintMinWidth;
        this.mMatchConstraintMaxWidth = constraintWidget.mMatchConstraintMaxWidth;
        this.mMatchConstraintMinHeight = constraintWidget.mMatchConstraintMinHeight;
        this.mMatchConstraintMaxHeight = constraintWidget.mMatchConstraintMaxHeight;
        this.mMatchConstraintPercentHeight = constraintWidget.mMatchConstraintPercentHeight;
        this.mIsWidthWrapContent = constraintWidget.mIsWidthWrapContent;
        this.mIsHeightWrapContent = constraintWidget.mIsHeightWrapContent;
        this.mResolvedDimensionRatioSide = constraintWidget.mResolvedDimensionRatioSide;
        this.mResolvedDimensionRatio = constraintWidget.mResolvedDimensionRatio;
        int[] iArr3 = constraintWidget.mMaxDimension;
        this.mMaxDimension = Arrays.copyOf(iArr3, iArr3.length);
        this.mCircleConstraintAngle = constraintWidget.mCircleConstraintAngle;
        this.hasBaseline = constraintWidget.hasBaseline;
        this.inPlaceholder = constraintWidget.inPlaceholder;
        this.mLeft.reset();
        this.mTop.reset();
        this.mRight.reset();
        this.mBottom.reset();
        this.mBaseline.reset();
        this.mCenterX.reset();
        this.mCenterY.reset();
        this.mCenter.reset();
        this.mListDimensionBehaviors = (DimensionBehaviour[]) Arrays.copyOf(this.mListDimensionBehaviors, 2);
        this.mParent = this.mParent == null ? null : map.get(constraintWidget.mParent);
        this.mWidth = constraintWidget.mWidth;
        this.mHeight = constraintWidget.mHeight;
        this.mDimensionRatio = constraintWidget.mDimensionRatio;
        this.mDimensionRatioSide = constraintWidget.mDimensionRatioSide;
        this.mX = constraintWidget.mX;
        this.mY = constraintWidget.mY;
        this.mRelX = constraintWidget.mRelX;
        this.mRelY = constraintWidget.mRelY;
        this.mOffsetX = constraintWidget.mOffsetX;
        this.mOffsetY = constraintWidget.mOffsetY;
        this.mBaselineDistance = constraintWidget.mBaselineDistance;
        this.mMinWidth = constraintWidget.mMinWidth;
        this.mMinHeight = constraintWidget.mMinHeight;
        this.mHorizontalBiasPercent = constraintWidget.mHorizontalBiasPercent;
        this.mVerticalBiasPercent = constraintWidget.mVerticalBiasPercent;
        this.mCompanionWidget = constraintWidget.mCompanionWidget;
        this.mContainerItemSkip = constraintWidget.mContainerItemSkip;
        this.mVisibility = constraintWidget.mVisibility;
        this.mDebugName = constraintWidget.mDebugName;
        this.mType = constraintWidget.mType;
        this.mDistToTop = constraintWidget.mDistToTop;
        this.mDistToLeft = constraintWidget.mDistToLeft;
        this.mDistToRight = constraintWidget.mDistToRight;
        this.mDistToBottom = constraintWidget.mDistToBottom;
        this.mLeftHasCentered = constraintWidget.mLeftHasCentered;
        this.mRightHasCentered = constraintWidget.mRightHasCentered;
        this.mTopHasCentered = constraintWidget.mTopHasCentered;
        this.mBottomHasCentered = constraintWidget.mBottomHasCentered;
        this.mHorizontalWrapVisited = constraintWidget.mHorizontalWrapVisited;
        this.mVerticalWrapVisited = constraintWidget.mVerticalWrapVisited;
        this.mOptimizerMeasurable = constraintWidget.mOptimizerMeasurable;
        this.mGroupsToSolver = constraintWidget.mGroupsToSolver;
        this.mHorizontalChainStyle = constraintWidget.mHorizontalChainStyle;
        this.mVerticalChainStyle = constraintWidget.mVerticalChainStyle;
        this.mHorizontalChainFixedPosition = constraintWidget.mHorizontalChainFixedPosition;
        this.mVerticalChainFixedPosition = constraintWidget.mVerticalChainFixedPosition;
        float[] fArr = this.mWeight;
        float[] fArr2 = constraintWidget.mWeight;
        fArr[0] = fArr2[0];
        fArr[1] = fArr2[1];
        ConstraintWidget[] constraintWidgetArr = this.mListNextMatchConstraintsWidget;
        ConstraintWidget[] constraintWidgetArr2 = constraintWidget.mListNextMatchConstraintsWidget;
        constraintWidgetArr[0] = constraintWidgetArr2[0];
        constraintWidgetArr[1] = constraintWidgetArr2[1];
        ConstraintWidget[] constraintWidgetArr3 = this.mNextChainWidget;
        ConstraintWidget[] constraintWidgetArr4 = constraintWidget.mNextChainWidget;
        constraintWidgetArr3[0] = constraintWidgetArr4[0];
        constraintWidgetArr3[1] = constraintWidgetArr4[1];
        ConstraintWidget constraintWidget2 = constraintWidget.mHorizontalNextWidget;
        this.mHorizontalNextWidget = constraintWidget2 == null ? null : map.get(constraintWidget2);
        ConstraintWidget constraintWidget3 = constraintWidget.mVerticalNextWidget;
        this.mVerticalNextWidget = constraintWidget3 != null ? map.get(constraintWidget3) : null;
    }

    public void updateFromRuns(boolean z, boolean z2) {
        int i;
        int i2;
        boolean zIsResolved = z & this.horizontalRun.isResolved();
        boolean zIsResolved2 = z2 & this.verticalRun.isResolved();
        int i3 = this.horizontalRun.start.value;
        int i4 = this.verticalRun.start.value;
        int i5 = this.horizontalRun.end.value;
        int i6 = this.verticalRun.end.value;
        int i7 = i6 - i4;
        if (i5 - i3 < 0 || i7 < 0 || i3 == Integer.MIN_VALUE || i3 == Integer.MAX_VALUE || i4 == Integer.MIN_VALUE || i4 == Integer.MAX_VALUE || i5 == Integer.MIN_VALUE || i5 == Integer.MAX_VALUE || i6 == Integer.MIN_VALUE || i6 == Integer.MAX_VALUE) {
            i3 = 0;
            i4 = 0;
            i5 = 0;
            i6 = 0;
        }
        int i8 = i5 - i3;
        int i9 = i6 - i4;
        if (zIsResolved) {
            this.mX = i3;
        }
        if (zIsResolved2) {
            this.mY = i4;
        }
        if (this.mVisibility == 8) {
            this.mWidth = 0;
            this.mHeight = 0;
            return;
        }
        if (zIsResolved) {
            if (this.mListDimensionBehaviors[0] == DimensionBehaviour.FIXED && i8 < (i2 = this.mWidth)) {
                i8 = i2;
            }
            this.mWidth = i8;
            int i10 = this.mMinWidth;
            if (i8 < i10) {
                this.mWidth = i10;
            }
        }
        if (zIsResolved2) {
            if (this.mListDimensionBehaviors[1] == DimensionBehaviour.FIXED && i9 < (i = this.mHeight)) {
                i9 = i;
            }
            this.mHeight = i9;
            int i11 = this.mMinHeight;
            if (i9 < i11) {
                this.mHeight = i11;
            }
        }
    }
}
