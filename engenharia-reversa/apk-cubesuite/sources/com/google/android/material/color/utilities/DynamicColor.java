package com.google.android.material.color.utilities;

import androidx.core.view.ViewCompat;
import java.util.HashMap;
import java.util.function.BiFunction;
import java.util.function.Function;

/* JADX INFO: loaded from: classes.dex */
public final class DynamicColor {
    public final Function<DynamicScheme, DynamicColor> background;
    public final Function<DynamicScheme, Double> chroma;
    private final HashMap<DynamicScheme, Hct> hctCache = new HashMap<>();
    public final Function<DynamicScheme, Double> hue;
    public final Function<DynamicScheme, Double> opacity;
    public final Function<DynamicScheme, Double> tone;
    public final Function<DynamicScheme, ToneDeltaConstraint> toneDeltaConstraint;
    public final Function<DynamicScheme, Double> toneMaxContrast;
    public final Function<DynamicScheme, Double> toneMinContrast;

    static /* synthetic */ TonalPalette lambda$fromArgb$0(TonalPalette tonalPalette, DynamicScheme dynamicScheme) {
        return tonalPalette;
    }

    static /* synthetic */ DynamicColor lambda$getTone$11(DynamicColor dynamicColor, DynamicScheme dynamicScheme) {
        return dynamicColor;
    }

    static /* synthetic */ Double lambda$toneMinContrastDefault$16(Double d) {
        return d;
    }

    public DynamicColor(Function<DynamicScheme, Double> function, Function<DynamicScheme, Double> function2, Function<DynamicScheme, Double> function3, Function<DynamicScheme, Double> function4, Function<DynamicScheme, DynamicColor> function5, Function<DynamicScheme, Double> function6, Function<DynamicScheme, Double> function7, Function<DynamicScheme, ToneDeltaConstraint> function8) {
        this.hue = function;
        this.chroma = function2;
        this.tone = function3;
        this.opacity = function4;
        this.background = function5;
        this.toneMinContrast = function6;
        this.toneMaxContrast = function7;
        this.toneDeltaConstraint = function8;
    }

    public static DynamicColor fromArgb(int i) {
        final Hct hctFromInt = Hct.fromInt(i);
        final TonalPalette tonalPaletteFromInt = TonalPalette.fromInt(i);
        return fromPalette(new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda3
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return DynamicColor.lambda$fromArgb$0(tonalPaletteFromInt, (DynamicScheme) obj);
            }
        }, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda4
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(hctFromInt.getTone());
            }
        });
    }

    public static DynamicColor fromArgb(final int i, Function<DynamicScheme, Double> function) {
        return fromPalette(new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda10
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return TonalPalette.fromInt(i);
            }
        }, function);
    }

    public static DynamicColor fromArgb(final int i, Function<DynamicScheme, Double> function, Function<DynamicScheme, DynamicColor> function2) {
        return fromPalette(new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda0
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return TonalPalette.fromInt(i);
            }
        }, function, function2);
    }

    public static DynamicColor fromArgb(final int i, Function<DynamicScheme, Double> function, Function<DynamicScheme, DynamicColor> function2, Function<DynamicScheme, ToneDeltaConstraint> function3) {
        return fromPalette(new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda16
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return TonalPalette.fromInt(i);
            }
        }, function, function2, function3);
    }

    public static DynamicColor fromPalette(Function<DynamicScheme, TonalPalette> function, Function<DynamicScheme, Double> function2) {
        return fromPalette(function, function2, null, null);
    }

    public static DynamicColor fromPalette(Function<DynamicScheme, TonalPalette> function, Function<DynamicScheme, Double> function2, Function<DynamicScheme, DynamicColor> function3) {
        return fromPalette(function, function2, function3, null);
    }

    public static DynamicColor fromPalette(final Function<DynamicScheme, TonalPalette> function, final Function<DynamicScheme, Double> function2, final Function<DynamicScheme, DynamicColor> function3, final Function<DynamicScheme, ToneDeltaConstraint> function4) {
        return new DynamicColor(new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda17
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(((TonalPalette) function.apply((DynamicScheme) obj)).getHue());
            }
        }, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda18
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(((TonalPalette) function.apply((DynamicScheme) obj)).getChroma());
            }
        }, function2, null, function3, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda1
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(DynamicColor.toneMinContrastDefault(function2, function3, (DynamicScheme) obj, function4));
            }
        }, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda2
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(DynamicColor.toneMaxContrastDefault(function2, function3, (DynamicScheme) obj, function4));
            }
        }, function4);
    }

    public int getArgb(DynamicScheme dynamicScheme) {
        int i = getHct(dynamicScheme).toInt();
        Function<DynamicScheme, Double> function = this.opacity;
        if (function == null) {
            return i;
        }
        return (MathUtils.clampInt(0, 255, (int) Math.round(function.apply(dynamicScheme).doubleValue() * 255.0d)) << 24) | (i & ViewCompat.MEASURED_SIZE_MASK);
    }

    public Hct getHct(DynamicScheme dynamicScheme) {
        Hct hct = this.hctCache.get(dynamicScheme);
        if (hct != null) {
            return hct;
        }
        Hct hctFrom = Hct.from(this.hue.apply(dynamicScheme).doubleValue(), this.chroma.apply(dynamicScheme).doubleValue(), getTone(dynamicScheme));
        if (this.hctCache.size() > 4) {
            this.hctCache.clear();
        }
        this.hctCache.put(dynamicScheme, hctFrom);
        return hctFrom;
    }

    /* JADX WARN: Code duplicated, block: B:36:0x00cb A[PHI: r10
  0x00cb: PHI (r10v1 double) = (r10v0 double), (r10v3 double) binds: [B:18:0x0054, B:34:0x00c3] A[DONT_GENERATE, DONT_INLINE]] */
    public double getTone(final DynamicScheme dynamicScheme) {
        final double dMax;
        final double dDoubleValue = this.tone.apply(dynamicScheme).doubleValue();
        boolean z = dynamicScheme.contrastLevel < 0.0d;
        if (dynamicScheme.contrastLevel != 0.0d) {
            double dDoubleValue2 = this.tone.apply(dynamicScheme).doubleValue();
            dDoubleValue = dDoubleValue2 + (((z ? this.toneMinContrast : this.toneMaxContrast).apply(dynamicScheme).doubleValue() - dDoubleValue2) * Math.abs(dynamicScheme.contrastLevel));
        }
        Function<DynamicScheme, DynamicColor> function = this.background;
        final DynamicColor dynamicColorApply = function == null ? null : function.apply(dynamicScheme);
        final double dMin = 1.0d;
        if (dynamicColorApply == null) {
            dMax = 21.0d;
        } else {
            Function<DynamicScheme, DynamicColor> function2 = dynamicColorApply.background;
            boolean z2 = (function2 == null || function2.apply(dynamicScheme) == null) ? false : true;
            double dRatioOfTones = Contrast.ratioOfTones(this.tone.apply(dynamicScheme).doubleValue(), dynamicColorApply.tone.apply(dynamicScheme).doubleValue());
            if (z) {
                dMin = z2 ? Contrast.ratioOfTones(this.toneMinContrast.apply(dynamicScheme).doubleValue(), dynamicColorApply.toneMinContrast.apply(dynamicScheme).doubleValue()) : 1.0d;
                dMax = dRatioOfTones;
            } else {
                double dRatioOfTones2 = Contrast.ratioOfTones(this.toneMaxContrast.apply(dynamicScheme).doubleValue(), dynamicColorApply.toneMaxContrast.apply(dynamicScheme).doubleValue());
                dMin = z2 ? Math.min(dRatioOfTones2, dRatioOfTones) : 1.0d;
                if (z2) {
                    dMax = Math.max(dRatioOfTones2, dRatioOfTones);
                } else {
                    dMax = 21.0d;
                }
            }
        }
        return calculateDynamicTone(dynamicScheme, this.tone, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda11
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(((DynamicColor) obj).getTone(dynamicScheme));
            }
        }, new BiFunction() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda12
            @Override // java.util.function.BiFunction
            public final Object apply(Object obj, Object obj2) {
                return Double.valueOf(dDoubleValue);
            }
        }, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda13
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return DynamicColor.lambda$getTone$11(this.f$0, (DynamicScheme) obj);
            }
        }, this.toneDeltaConstraint, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda14
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(dMin);
            }
        }, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda15
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return Double.valueOf(dMax);
            }
        });
    }

    public static double toneMinContrastDefault(final Function<DynamicScheme, Double> function, final Function<DynamicScheme, DynamicColor> function2, final DynamicScheme dynamicScheme, Function<DynamicScheme, ToneDeltaConstraint> function3) {
        return calculateDynamicTone(dynamicScheme, function, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda7
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return ((DynamicColor) obj).toneMinContrast.apply(dynamicScheme);
            }
        }, new BiFunction() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda8
            @Override // java.util.function.BiFunction
            public final Object apply(Object obj, Object obj2) {
                return DynamicColor.lambda$toneMinContrastDefault$15(function, dynamicScheme, function2, (Double) obj, (Double) obj2);
            }
        }, function2, function3, null, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda9
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return DynamicColor.lambda$toneMinContrastDefault$16((Double) obj);
            }
        });
    }

    static /* synthetic */ Double lambda$toneMinContrastDefault$15(Function function, DynamicScheme dynamicScheme, Function function2, Double d, Double d2) {
        double dDoubleValue = ((Double) function.apply(dynamicScheme)).doubleValue();
        if (d.doubleValue() >= 7.0d) {
            dDoubleValue = contrastingTone(d2.doubleValue(), 4.5d);
        } else if (d.doubleValue() >= 3.0d) {
            dDoubleValue = contrastingTone(d2.doubleValue(), 3.0d);
        } else if (function2 != null && function2.apply(dynamicScheme) != null && ((DynamicColor) function2.apply(dynamicScheme)).background != null && ((DynamicColor) function2.apply(dynamicScheme)).background.apply(dynamicScheme) != null) {
            dDoubleValue = contrastingTone(d2.doubleValue(), d.doubleValue());
        }
        return Double.valueOf(dDoubleValue);
    }

    public static double toneMaxContrastDefault(Function<DynamicScheme, Double> function, final Function<DynamicScheme, DynamicColor> function2, final DynamicScheme dynamicScheme, Function<DynamicScheme, ToneDeltaConstraint> function3) {
        return calculateDynamicTone(dynamicScheme, function, new Function() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda5
            @Override // java.util.function.Function
            public final Object apply(Object obj) {
                return ((DynamicColor) obj).toneMaxContrast.apply(dynamicScheme);
            }
        }, new BiFunction() { // from class: com.google.android.material.color.utilities.DynamicColor$$ExternalSyntheticLambda6
            @Override // java.util.function.BiFunction
            public final Object apply(Object obj, Object obj2) {
                return DynamicColor.lambda$toneMaxContrastDefault$18(function2, dynamicScheme, (Double) obj, (Double) obj2);
            }
        }, function2, function3, null, null);
    }

    static /* synthetic */ Double lambda$toneMaxContrastDefault$18(Function function, DynamicScheme dynamicScheme, Double d, Double d2) {
        if (function != null && function.apply(dynamicScheme) != null && ((DynamicColor) function.apply(dynamicScheme)).background != null && ((DynamicColor) function.apply(dynamicScheme)).background.apply(dynamicScheme) != null) {
            return Double.valueOf(contrastingTone(d2.doubleValue(), 7.0d));
        }
        return Double.valueOf(contrastingTone(d2.doubleValue(), Math.max(7.0d, d.doubleValue())));
    }

    public static double calculateDynamicTone(DynamicScheme dynamicScheme, Function<DynamicScheme, Double> function, Function<DynamicColor, Double> function2, BiFunction<Double, Double, Double> biFunction, Function<DynamicScheme, DynamicColor> function3, Function<DynamicScheme, ToneDeltaConstraint> function4, Function<Double, Double> function5, Function<Double, Double> function6) {
        double dDoubleValue = function.apply(dynamicScheme).doubleValue();
        DynamicColor dynamicColorApply = function3 == null ? null : function3.apply(dynamicScheme);
        if (dynamicColorApply == null) {
            return dDoubleValue;
        }
        double dRatioOfTones = Contrast.ratioOfTones(dDoubleValue, dynamicColorApply.tone.apply(dynamicScheme).doubleValue());
        double dDoubleValue2 = function2.apply(dynamicColorApply).doubleValue();
        double dDoubleValue3 = biFunction.apply(Double.valueOf(dRatioOfTones), Double.valueOf(dDoubleValue2)).doubleValue();
        double dRatioOfTones2 = Contrast.ratioOfTones(dDoubleValue2, dDoubleValue3);
        double dDoubleValue4 = 1.0d;
        if (function5 != null && function5.apply(Double.valueOf(dRatioOfTones)) != null) {
            dDoubleValue4 = function5.apply(Double.valueOf(dRatioOfTones)).doubleValue();
        }
        double dDoubleValue5 = 21.0d;
        if (function6 != null && function6.apply(Double.valueOf(dRatioOfTones)) != null) {
            dDoubleValue5 = function6.apply(Double.valueOf(dRatioOfTones)).doubleValue();
        }
        double dClampDouble = MathUtils.clampDouble(dDoubleValue4, dDoubleValue5, dRatioOfTones2);
        if (dClampDouble != dRatioOfTones2) {
            dDoubleValue3 = contrastingTone(dDoubleValue2, dClampDouble);
        }
        Function<DynamicScheme, DynamicColor> function7 = dynamicColorApply.background;
        if (function7 == null || function7.apply(dynamicScheme) == null) {
            dDoubleValue3 = enableLightForeground(dDoubleValue3);
        }
        return ensureToneDelta(dDoubleValue3, dDoubleValue, dynamicScheme, function4, function2);
    }

    static double ensureToneDelta(double d, double d2, DynamicScheme dynamicScheme, Function<DynamicScheme, ToneDeltaConstraint> function, Function<DynamicColor, Double> function2) {
        ToneDeltaConstraint toneDeltaConstraintApply = function == null ? null : function.apply(dynamicScheme);
        if (toneDeltaConstraintApply != null) {
            double d3 = toneDeltaConstraintApply.delta;
            double dDoubleValue = function2.apply(toneDeltaConstraintApply.keepAway).doubleValue();
            double dAbs = Math.abs(d - dDoubleValue);
            if (dAbs < d3) {
                int i = AnonymousClass1.$SwitchMap$com$google$android$material$color$utilities$TonePolarity[toneDeltaConstraintApply.keepAwayPolarity.ordinal()];
                if (i == 1) {
                    return MathUtils.clampDouble(0.0d, 100.0d, dDoubleValue + d3);
                }
                if (i == 2) {
                    return MathUtils.clampDouble(0.0d, 100.0d, dDoubleValue - d3);
                }
                if (i == 3) {
                    boolean z = d2 > toneDeltaConstraintApply.keepAway.tone.apply(dynamicScheme).doubleValue();
                    double dAbs2 = Math.abs(dAbs - d3);
                    return (!z ? d < dAbs2 : d + dAbs2 <= 100.0d) ? d - dAbs2 : d + dAbs2;
                }
            }
        }
        return d;
    }

    /* JADX INFO: renamed from: com.google.android.material.color.utilities.DynamicColor$1, reason: invalid class name */
    static /* synthetic */ class AnonymousClass1 {
        static final /* synthetic */ int[] $SwitchMap$com$google$android$material$color$utilities$TonePolarity;

        static {
            int[] iArr = new int[TonePolarity.values().length];
            $SwitchMap$com$google$android$material$color$utilities$TonePolarity = iArr;
            try {
                iArr[TonePolarity.DARKER.ordinal()] = 1;
            } catch (NoSuchFieldError unused) {
            }
            try {
                $SwitchMap$com$google$android$material$color$utilities$TonePolarity[TonePolarity.LIGHTER.ordinal()] = 2;
            } catch (NoSuchFieldError unused2) {
            }
            try {
                $SwitchMap$com$google$android$material$color$utilities$TonePolarity[TonePolarity.NO_PREFERENCE.ordinal()] = 3;
            } catch (NoSuchFieldError unused3) {
            }
        }
    }

    /* JADX WARN: Code duplicated, block: B:23:0x0044 A[RETURN] */
    /* JADX WARN: Code duplicated, block: B:24:0x0045 A[RETURN] */
    public static double contrastingTone(double d, double d2) {
        double dLighterUnsafe = Contrast.lighterUnsafe(d, d2);
        double dDarkerUnsafe = Contrast.darkerUnsafe(d, d2);
        double dRatioOfTones = Contrast.ratioOfTones(dLighterUnsafe, d);
        double dRatioOfTones2 = Contrast.ratioOfTones(dDarkerUnsafe, d);
        if (!tonePrefersLightForeground(d)) {
            if (dRatioOfTones2 >= d2 || dRatioOfTones2 >= dRatioOfTones) {
                return dDarkerUnsafe;
            }
            return dLighterUnsafe;
        }
        boolean z = Math.abs(dRatioOfTones - dRatioOfTones2) < 0.1d && dRatioOfTones < d2 && dRatioOfTones2 < d2;
        if (dRatioOfTones >= d2 || dRatioOfTones >= dRatioOfTones2 || z) {
            return dLighterUnsafe;
        }
        return dDarkerUnsafe;
    }

    public static double enableLightForeground(double d) {
        if (!tonePrefersLightForeground(d) || toneAllowsLightForeground(d)) {
            return d;
        }
        return 49.0d;
    }

    public static boolean tonePrefersLightForeground(double d) {
        return Math.round(d) < 60;
    }

    public static boolean toneAllowsLightForeground(double d) {
        return Math.round(d) <= 49;
    }
}
