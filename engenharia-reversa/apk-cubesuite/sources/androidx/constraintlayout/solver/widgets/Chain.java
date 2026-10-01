package androidx.constraintlayout.solver.widgets;

import androidx.constraintlayout.solver.ArrayRow;
import androidx.constraintlayout.solver.LinearSystem;
import androidx.constraintlayout.solver.SolverVariable;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
class Chain {
    private static final boolean DEBUG = false;

    Chain() {
    }

    static void applyChainConstraints(ConstraintWidgetContainer constraintWidgetContainer, LinearSystem linearSystem, int i) {
        int i2;
        ChainHead[] chainHeadArr;
        int i3;
        if (i == 0) {
            i2 = constraintWidgetContainer.mHorizontalChainsSize;
            chainHeadArr = constraintWidgetContainer.mHorizontalChainsArray;
            i3 = 0;
        } else {
            i2 = constraintWidgetContainer.mVerticalChainsSize;
            chainHeadArr = constraintWidgetContainer.mVerticalChainsArray;
            i3 = 2;
        }
        for (int i4 = 0; i4 < i2; i4++) {
            ChainHead chainHead = chainHeadArr[i4];
            chainHead.define();
            applyChainConstraints(constraintWidgetContainer, linearSystem, i, i3, chainHead);
        }
    }

    /* JADX WARN: Code duplicated, block: B:102:0x019c  */
    /* JADX WARN: Code duplicated, block: B:286:0x04fb  */
    /* JADX WARN: Code duplicated, block: B:287:0x0500  */
    /* JADX WARN: Code duplicated, block: B:290:0x0506  */
    /* JADX WARN: Code duplicated, block: B:291:0x050b  */
    /* JADX WARN: Code duplicated, block: B:293:0x050f  */
    /* JADX WARN: Code duplicated, block: B:295:0x0517  */
    /* JADX WARN: Code duplicated, block: B:298:0x0521  */
    /* JADX WARN: Code duplicated, block: B:29:0x004a A[PHI: r13 r14
  0x004a: PHI (r13v4 boolean) = (r13v2 boolean), (r13v51 boolean) binds: [B:28:0x0048, B:17:0x0035] A[DONT_GENERATE, DONT_INLINE]
  0x004a: PHI (r14v4 boolean) = (r14v2 boolean), (r14v9 boolean) binds: [B:28:0x0048, B:17:0x0035] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:30:0x004c A[PHI: r13 r14
  0x004c: PHI (r13v48 boolean) = (r13v2 boolean), (r13v51 boolean) binds: [B:28:0x0048, B:17:0x0035] A[DONT_GENERATE, DONT_INLINE]
  0x004c: PHI (r14v6 boolean) = (r14v2 boolean), (r14v9 boolean) binds: [B:28:0x0048, B:17:0x0035] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r0v20, types: [androidx.constraintlayout.solver.LinearSystem] */
    /* JADX WARN: Type inference failed for: r0v21 */
    /* JADX WARN: Type inference failed for: r0v5, types: [androidx.constraintlayout.solver.LinearSystem] */
    /* JADX WARN: Type inference failed for: r5v15 */
    /* JADX WARN: Type inference failed for: r5v16 */
    /* JADX WARN: Type inference failed for: r5v17, types: [androidx.constraintlayout.solver.SolverVariable] */
    /* JADX WARN: Type inference failed for: r5v20 */
    /* JADX WARN: Type inference failed for: r5v84 */
    /* JADX WARN: Type inference failed for: r7v1 */
    /* JADX WARN: Type inference failed for: r7v2, types: [androidx.constraintlayout.solver.widgets.ConstraintWidget] */
    /* JADX WARN: Type inference failed for: r7v37 */
    /* JADX WARN: Type inference failed for: r7v38 */
    /* JADX WARN: Type inference failed for: r7v39 */
    static void applyChainConstraints(ConstraintWidgetContainer constraintWidgetContainer, LinearSystem linearSystem, int i, int i2, ChainHead chainHead) {
        boolean z;
        boolean z2;
        boolean z3;
        float f;
        Object obj;
        ?? r0;
        ConstraintAnchor constraintAnchor;
        SolverVariable solverVariable;
        SolverVariable solverVariable2;
        ConstraintWidget constraintWidget;
        int i3;
        ConstraintAnchor constraintAnchor2;
        SolverVariable solverVariable3;
        SolverVariable solverVariable4;
        int i4;
        ConstraintAnchor constraintAnchor3;
        int i5;
        ConstraintAnchor constraintAnchor4;
        SolverVariable solverVariable5;
        ?? r5;
        ConstraintAnchor constraintAnchor5;
        SolverVariable solverVariable6;
        float f2;
        int size;
        int i6;
        ConstraintWidget constraintWidget2 = chainHead.mFirst;
        ConstraintWidget constraintWidget3 = chainHead.mLast;
        ConstraintWidget constraintWidget4 = chainHead.mFirstVisibleWidget;
        ConstraintWidget constraintWidget5 = chainHead.mLastVisibleWidget;
        ConstraintWidget constraintWidget6 = chainHead.mHead;
        float f3 = chainHead.mTotalWeight;
        ConstraintWidget constraintWidget7 = chainHead.mFirstMatchConstraintWidget;
        ConstraintWidget constraintWidget8 = chainHead.mLastMatchConstraintWidget;
        boolean z4 = constraintWidgetContainer.mListDimensionBehaviors[i] == ConstraintWidget.DimensionBehaviour.WRAP_CONTENT;
        if (i == 0) {
            z = constraintWidget6.mHorizontalChainStyle == 0;
            z2 = constraintWidget6.mHorizontalChainStyle == 1;
            if (constraintWidget6.mHorizontalChainStyle == 2) {
                z3 = true;
            } else {
                z3 = false;
            }
        } else {
            z = constraintWidget6.mVerticalChainStyle == 0;
            z2 = constraintWidget6.mVerticalChainStyle == 1;
            if (constraintWidget6.mVerticalChainStyle == 2) {
                z3 = true;
            } else {
                z3 = false;
            }
        }
        ?? r7 = constraintWidget2;
        boolean z5 = false;
        while (true) {
            f = f3;
            obj = null;
            if (z5) {
                break;
            }
            ConstraintAnchor constraintAnchor6 = r7.mListAnchors[i2];
            int i7 = z3 ? 1 : 4;
            int margin = constraintAnchor6.getMargin();
            boolean z6 = z4;
            boolean z7 = r7.mListDimensionBehaviors[i] == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT && r7.mResolvedMatchConstraintDefault[i] == 0;
            if (constraintAnchor6.mTarget != null && r7 != constraintWidget2) {
                margin += constraintAnchor6.mTarget.getMargin();
            }
            int i8 = margin;
            if (z3 && r7 != constraintWidget2 && r7 != constraintWidget4) {
                i7 = 5;
            }
            boolean z8 = z7;
            if (constraintAnchor6.mTarget != null) {
                if (r7 == constraintWidget4) {
                    linearSystem.addGreaterThan(constraintAnchor6.mSolverVariable, constraintAnchor6.mTarget.mSolverVariable, i8, 6);
                } else {
                    linearSystem.addGreaterThan(constraintAnchor6.mSolverVariable, constraintAnchor6.mTarget.mSolverVariable, i8, 8);
                }
                linearSystem.addEquality(constraintAnchor6.mSolverVariable, constraintAnchor6.mTarget.mSolverVariable, i8, (!z8 || z3) ? i7 : 5);
            } else {
                z3 = z3;
                z = z;
            }
            if (z6) {
                if (r7.getVisibility() == 8 || r7.mListDimensionBehaviors[i] != ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT) {
                    i6 = 0;
                } else {
                    i6 = 0;
                    linearSystem.addGreaterThan(r7.mListAnchors[i2 + 1].mSolverVariable, r7.mListAnchors[i2].mSolverVariable, 0, 5);
                }
                linearSystem.addGreaterThan(r7.mListAnchors[i2].mSolverVariable, constraintWidgetContainer.mListAnchors[i2].mSolverVariable, i6, 8);
            }
            ConstraintAnchor constraintAnchor7 = r7.mListAnchors[i2 + 1].mTarget;
            if (constraintAnchor7 != null) {
                ConstraintWidget constraintWidget9 = constraintAnchor7.mOwner;
                if (constraintWidget9.mListAnchors[i2].mTarget != null && constraintWidget9.mListAnchors[i2].mTarget.mOwner == r7) {
                    obj = constraintWidget9;
                }
            }
            if (obj != null) {
                r7 = obj;
            } else {
                z5 = true;
            }
            f3 = f;
            z4 = z6;
            z3 = z3;
            z = z;
            r7 = r7;
        }
        boolean z9 = z4;
        boolean z10 = z3;
        boolean z11 = z;
        if (constraintWidget5 != null) {
            int i9 = i2 + 1;
            if (constraintWidget3.mListAnchors[i9].mTarget != null) {
                ConstraintAnchor constraintAnchor8 = constraintWidget5.mListAnchors[i9];
                if (constraintWidget5.mListDimensionBehaviors[i] == ConstraintWidget.DimensionBehaviour.MATCH_CONSTRAINT && constraintWidget5.mResolvedMatchConstraintDefault[i] == 0 && !z10 && constraintAnchor8.mTarget.mOwner == constraintWidgetContainer) {
                    linearSystem.addEquality(constraintAnchor8.mSolverVariable, constraintAnchor8.mTarget.mSolverVariable, -constraintAnchor8.getMargin(), 5);
                } else if (z10 && constraintAnchor8.mTarget.mOwner == constraintWidgetContainer) {
                    linearSystem.addEquality(constraintAnchor8.mSolverVariable, constraintAnchor8.mTarget.mSolverVariable, -constraintAnchor8.getMargin(), 4);
                }
                linearSystem.addLowerThan(constraintAnchor8.mSolverVariable, constraintWidget3.mListAnchors[i9].mTarget.mSolverVariable, -constraintAnchor8.getMargin(), 6);
            }
        }
        if (z9) {
            int i10 = i2 + 1;
            linearSystem.addGreaterThan(constraintWidgetContainer.mListAnchors[i10].mSolverVariable, constraintWidget3.mListAnchors[i10].mSolverVariable, constraintWidget3.mListAnchors[i10].getMargin(), 8);
        }
        ArrayList<ConstraintWidget> arrayList = chainHead.mWeightedMatchConstraintsWidgets;
        if (arrayList != null && (size = arrayList.size()) > 1) {
            float f4 = (!chainHead.mHasUndefinedWeights || chainHead.mHasComplexMatchWeights) ? f : chainHead.mWidgetsMatchCount;
            float f5 = 0.0f;
            float f6 = 0.0f;
            ConstraintWidget constraintWidget10 = null;
            int i11 = 0;
            while (i11 < size) {
                ConstraintWidget constraintWidget11 = arrayList.get(i11);
                float f7 = constraintWidget11.mWeight[i];
                if (f7 < f5) {
                    if (chainHead.mHasComplexMatchWeights) {
                        f5 = f5;
                        linearSystem.addEquality(constraintWidget11.mListAnchors[i2 + 1].mSolverVariable, constraintWidget11.mListAnchors[i2].mSolverVariable, 0, 4);
                    } else {
                        f7 = 1.0f;
                    }
                    arrayList = arrayList;
                    i11++;
                    f5 = f5;
                    arrayList = arrayList;
                }
                float f8 = f7;
                if (f8 == f5) {
                    linearSystem.addEquality(constraintWidget11.mListAnchors[i2 + 1].mSolverVariable, constraintWidget11.mListAnchors[i2].mSolverVariable, 0, 8);
                    arrayList = arrayList;
                } else {
                    if (constraintWidget10 != null) {
                        SolverVariable solverVariable7 = constraintWidget10.mListAnchors[i2].mSolverVariable;
                        int i12 = i2 + 1;
                        SolverVariable solverVariable8 = constraintWidget10.mListAnchors[i12].mSolverVariable;
                        SolverVariable solverVariable9 = constraintWidget11.mListAnchors[i2].mSolverVariable;
                        SolverVariable solverVariable10 = constraintWidget11.mListAnchors[i12].mSolverVariable;
                        ArrayRow arrayRowCreateRow = linearSystem.createRow();
                        arrayRowCreateRow.createRowEqualMatchDimensions(f6, f4, f8, solverVariable7, solverVariable8, solverVariable9, solverVariable10);
                        linearSystem.addConstraint(arrayRowCreateRow);
                    }
                    constraintWidget10 = constraintWidget11;
                    f6 = f8;
                }
                i11++;
                f5 = f5;
                arrayList = arrayList;
            }
        }
        if (constraintWidget4 != null && (constraintWidget4 == constraintWidget5 || z10)) {
            ConstraintAnchor constraintAnchor9 = constraintWidget2.mListAnchors[i2];
            int i13 = i2 + 1;
            ConstraintAnchor constraintAnchor10 = constraintWidget3.mListAnchors[i13];
            SolverVariable solverVariable11 = constraintAnchor9.mTarget != null ? constraintAnchor9.mTarget.mSolverVariable : null;
            SolverVariable solverVariable12 = constraintAnchor10.mTarget != null ? constraintAnchor10.mTarget.mSolverVariable : null;
            ConstraintAnchor constraintAnchor11 = constraintWidget4.mListAnchors[i2];
            ConstraintAnchor constraintAnchor12 = constraintWidget5.mListAnchors[i13];
            if (solverVariable11 != null && solverVariable12 != null) {
                if (i == 0) {
                    f2 = constraintWidget6.mHorizontalBiasPercent;
                } else {
                    f2 = constraintWidget6.mVerticalBiasPercent;
                }
                linearSystem.addCentering(constraintAnchor11.mSolverVariable, solverVariable11, constraintAnchor11.getMargin(), f2, solverVariable12, constraintAnchor12.mSolverVariable, constraintAnchor12.getMargin(), 7);
            }
        } else {
            if (!z11 || constraintWidget4 == null) {
                int i14 = 8;
                if (z2 && constraintWidget4 != null) {
                    boolean z12 = chainHead.mWidgetsMatchCount > 0 && chainHead.mWidgetsCount == chainHead.mWidgetsMatchCount;
                    ConstraintWidget constraintWidget12 = constraintWidget4;
                    ConstraintWidget constraintWidget13 = constraintWidget12;
                    while (constraintWidget13 != null) {
                        ConstraintWidget constraintWidget14 = constraintWidget13.mNextChainWidget[i];
                        while (constraintWidget14 != null && constraintWidget14.getVisibility() == i14) {
                            constraintWidget14 = constraintWidget14.mNextChainWidget[i];
                        }
                        if (constraintWidget13 == constraintWidget4 || constraintWidget13 == constraintWidget5 || constraintWidget14 == null) {
                            constraintWidget12 = constraintWidget12;
                        } else {
                            if (constraintWidget14 == constraintWidget5) {
                                constraintWidget14 = null;
                            }
                            ConstraintAnchor constraintAnchor13 = constraintWidget13.mListAnchors[i2];
                            SolverVariable solverVariable13 = constraintAnchor13.mSolverVariable;
                            if (constraintAnchor13.mTarget != null) {
                                SolverVariable solverVariable14 = constraintAnchor13.mTarget.mSolverVariable;
                            }
                            int i15 = i2 + 1;
                            SolverVariable solverVariable15 = constraintWidget12.mListAnchors[i15].mSolverVariable;
                            int margin2 = constraintAnchor13.getMargin();
                            int margin3 = constraintWidget13.mListAnchors[i15].getMargin();
                            if (constraintWidget14 != null) {
                                constraintAnchor = constraintWidget14.mListAnchors[i2];
                                solverVariable = constraintAnchor.mSolverVariable;
                                solverVariable2 = constraintAnchor.mTarget != null ? constraintAnchor.mTarget.mSolverVariable : null;
                            } else {
                                constraintAnchor = constraintWidget5.mListAnchors[i2];
                                solverVariable = constraintAnchor != null ? constraintAnchor.mSolverVariable : null;
                                solverVariable2 = constraintWidget13.mListAnchors[i15].mSolverVariable;
                            }
                            if (constraintAnchor != null) {
                                margin3 += constraintAnchor.getMargin();
                            }
                            int i16 = margin3;
                            if (constraintWidget12 != null) {
                                margin2 += constraintWidget12.mListAnchors[i15].getMargin();
                            }
                            SolverVariable solverVariable16 = solverVariable;
                            int i17 = z12 ? 8 : 4;
                            if (solverVariable13 == null || solverVariable15 == null || solverVariable16 == null || solverVariable2 == null) {
                                constraintWidget = constraintWidget14;
                            } else {
                                constraintWidget = constraintWidget14;
                                linearSystem.addCentering(solverVariable13, solverVariable15, margin2, 0.5f, solverVariable16, solverVariable2, i16, i17);
                            }
                            constraintWidget14 = constraintWidget;
                        }
                        if (constraintWidget13.getVisibility() != 8) {
                            constraintWidget12 = constraintWidget13;
                        }
                        constraintWidget13 = constraintWidget14;
                        constraintWidget12 = constraintWidget12;
                        i14 = 8;
                    }
                    r0 = linearSystem;
                    ConstraintAnchor constraintAnchor14 = constraintWidget4.mListAnchors[i2];
                    ConstraintAnchor constraintAnchor15 = constraintWidget2.mListAnchors[i2].mTarget;
                    int i18 = i2 + 1;
                    ConstraintAnchor constraintAnchor16 = constraintWidget5.mListAnchors[i18];
                    ConstraintAnchor constraintAnchor17 = constraintWidget3.mListAnchors[i18].mTarget;
                    if (constraintAnchor15 != null) {
                        if (constraintWidget4 != constraintWidget5) {
                            r0.addEquality(constraintAnchor14.mSolverVariable, constraintAnchor15.mSolverVariable, constraintAnchor14.getMargin(), 5);
                        } else if (constraintAnchor17 != null) {
                            r0.addCentering(constraintAnchor14.mSolverVariable, constraintAnchor15.mSolverVariable, constraintAnchor14.getMargin(), 0.5f, constraintAnchor16.mSolverVariable, constraintAnchor17.mSolverVariable, constraintAnchor16.getMargin(), 5);
                        }
                    }
                    if (constraintAnchor17 != null && constraintWidget4 != constraintWidget5) {
                        r0.addEquality(constraintAnchor16.mSolverVariable, constraintAnchor17.mSolverVariable, -constraintAnchor16.getMargin(), 5);
                    }
                }
                if ((z11 && !z2) || constraintWidget4 == null || constraintWidget4 == constraintWidget5) {
                    return;
                }
                constraintAnchor3 = constraintWidget4.mListAnchors[i2];
                i5 = i2 + 1;
                constraintAnchor4 = constraintWidget5.mListAnchors[i5];
                if (constraintAnchor3.mTarget != null) {
                    solverVariable5 = constraintAnchor3.mTarget.mSolverVariable;
                } else {
                    solverVariable5 = null;
                }
                if (constraintAnchor4.mTarget != null) {
                    solverVariable6 = constraintAnchor4.mTarget.mSolverVariable;
                } else {
                    r5 = 0;
                }
                if (constraintWidget3 != constraintWidget5) {
                    constraintAnchor5 = constraintWidget3.mListAnchors[i5];
                    if (constraintAnchor5.mTarget != null) {
                        r5 = solverVariable6;
                        obj = constraintAnchor5.mTarget.mSolverVariable;
                    }
                    r5 = solverVariable6;
                    r5 = obj;
                }
                if (constraintWidget4 == constraintWidget5) {
                    constraintAnchor3 = constraintWidget4.mListAnchors[i2];
                    constraintAnchor4 = constraintWidget4.mListAnchors[i5];
                }
                if (solverVariable5 != null || r5 == 0) {
                }
                int margin4 = constraintAnchor3.getMargin();
                if (constraintWidget5 != null) {
                    constraintWidget3 = constraintWidget5;
                }
                r0.addCentering(constraintAnchor3.mSolverVariable, solverVariable5, margin4, 0.5f, r5, constraintAnchor4.mSolverVariable, constraintWidget3.mListAnchors[i5].getMargin(), 5);
                return;
            }
            boolean z13 = chainHead.mWidgetsMatchCount > 0 && chainHead.mWidgetsCount == chainHead.mWidgetsMatchCount;
            ConstraintWidget constraintWidget15 = constraintWidget4;
            ConstraintWidget constraintWidget16 = constraintWidget15;
            while (constraintWidget15 != null) {
                ConstraintWidget constraintWidget17 = constraintWidget15.mNextChainWidget[i];
                while (true) {
                    if (constraintWidget17 == null) {
                        i3 = 8;
                        break;
                    }
                    i3 = 8;
                    if (constraintWidget17.getVisibility() != 8) {
                        break;
                    } else {
                        constraintWidget17 = constraintWidget17.mNextChainWidget[i];
                    }
                }
                if (constraintWidget17 != null || constraintWidget15 == constraintWidget5) {
                    ConstraintAnchor constraintAnchor18 = constraintWidget15.mListAnchors[i2];
                    SolverVariable solverVariable17 = constraintAnchor18.mSolverVariable;
                    SolverVariable solverVariable18 = constraintAnchor18.mTarget != null ? constraintAnchor18.mTarget.mSolverVariable : null;
                    if (constraintWidget16 != constraintWidget15) {
                        solverVariable18 = constraintWidget16.mListAnchors[i2 + 1].mSolverVariable;
                    } else if (constraintWidget15 == constraintWidget4 && constraintWidget16 == constraintWidget15) {
                        solverVariable18 = constraintWidget2.mListAnchors[i2].mTarget != null ? constraintWidget2.mListAnchors[i2].mTarget.mSolverVariable : null;
                    }
                    int margin5 = constraintAnchor18.getMargin();
                    int i19 = i2 + 1;
                    int margin6 = constraintWidget15.mListAnchors[i19].getMargin();
                    if (constraintWidget17 != null) {
                        constraintAnchor2 = constraintWidget17.mListAnchors[i2];
                        solverVariable3 = constraintAnchor2.mSolverVariable;
                        solverVariable4 = constraintWidget15.mListAnchors[i19].mSolverVariable;
                    } else {
                        constraintAnchor2 = constraintWidget3.mListAnchors[i19].mTarget;
                        solverVariable3 = constraintAnchor2 != null ? constraintAnchor2.mSolverVariable : null;
                        solverVariable4 = constraintWidget15.mListAnchors[i19].mSolverVariable;
                    }
                    if (constraintAnchor2 != null) {
                        margin6 += constraintAnchor2.getMargin();
                    }
                    if (constraintWidget16 != null) {
                        margin5 += constraintWidget16.mListAnchors[i19].getMargin();
                    }
                    if (solverVariable17 == null || solverVariable18 == null || solverVariable3 == null || solverVariable4 == null) {
                        i4 = 8;
                    } else {
                        if (constraintWidget15 == constraintWidget4) {
                            margin5 = constraintWidget4.mListAnchors[i2].getMargin();
                        }
                        if (constraintWidget15 == constraintWidget5) {
                            margin6 = constraintWidget5.mListAnchors[i19].getMargin();
                        }
                        i4 = 8;
                        linearSystem.addCentering(solverVariable17, solverVariable18, margin5, 0.5f, solverVariable3, solverVariable4, margin6, z13 ? 8 : 5);
                    }
                } else {
                    i4 = i3;
                }
                if (constraintWidget15.getVisibility() != i4) {
                    constraintWidget16 = constraintWidget15;
                }
                constraintWidget15 = constraintWidget17;
            }
        }
        r0 = linearSystem;
        if (z11) {
        }
        constraintAnchor3 = constraintWidget4.mListAnchors[i2];
        i5 = i2 + 1;
        constraintAnchor4 = constraintWidget5.mListAnchors[i5];
        if (constraintAnchor3.mTarget != null) {
            solverVariable5 = constraintAnchor3.mTarget.mSolverVariable;
        } else {
            solverVariable5 = null;
        }
        if (constraintAnchor4.mTarget != null) {
            solverVariable6 = constraintAnchor4.mTarget.mSolverVariable;
        } else {
            r5 = 0;
        }
        if (constraintWidget3 != constraintWidget5) {
            constraintAnchor5 = constraintWidget3.mListAnchors[i5];
            if (constraintAnchor5.mTarget != null) {
                r5 = solverVariable6;
                obj = constraintAnchor5.mTarget.mSolverVariable;
            }
            r5 = solverVariable6;
            r5 = obj;
        }
        if (constraintWidget4 == constraintWidget5) {
            constraintAnchor3 = constraintWidget4.mListAnchors[i2];
            constraintAnchor4 = constraintWidget4.mListAnchors[i5];
        }
        if (solverVariable5 != null) {
        }
    }
}
