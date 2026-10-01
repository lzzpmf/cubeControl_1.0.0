package es.voghdev.pdfviewpager.library.subscaleview;

import java.util.Arrays;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class SubsamplingScaleImageViewConstants {
    public static final int EASE_IN_OUT_QUAD = 2;
    public static final int EASE_OUT_QUAD = 1;
    public static final int ORIENTATION_0 = 0;
    public static final int ORIENTATION_90 = 90;
    public static final int ORIENTATION_USE_EXIF = -1;
    public static final int ORIGIN_ANIM = 1;
    public static final int ORIGIN_DOUBLE_TAP_ZOOM = 4;
    public static final int ORIGIN_FLING = 3;
    public static final int ORIGIN_TOUCH = 2;
    public static final int PAN_LIMIT_CENTER = 3;
    public static final int PAN_LIMIT_INSIDE = 1;
    public static final int PAN_LIMIT_OUTSIDE = 2;
    public static final int SCALE_TYPE_CENTER_CROP = 2;
    public static final int SCALE_TYPE_CENTER_INSIDE = 1;
    public static final int SCALE_TYPE_CUSTOM = 3;
    public static final int SCALE_TYPE_START = 4;
    public static final int ZOOM_FOCUS_CENTER = 2;
    public static final int ZOOM_FOCUS_CENTER_IMMEDIATE = 3;
    public static final int ZOOM_FOCUS_FIXED = 1;
    public static final String TAG = "SubsamplingScaleImageView";
    public static final int ORIENTATION_180 = 180;
    public static final int ORIENTATION_270 = 270;
    public static final List<Integer> VALID_ORIENTATIONS = Arrays.asList(0, 90, Integer.valueOf(ORIENTATION_180), Integer.valueOf(ORIENTATION_270), -1);
    public static final List<Integer> VALID_ZOOM_STYLES = Arrays.asList(1, 2, 3);
    public static final List<Integer> VALID_EASING_STYLES = Arrays.asList(2, 1);
    public static final List<Integer> VALID_PAN_LIMITS = Arrays.asList(1, 2, 3);
    public static final List<Integer> VALID_SCALE_TYPES = Arrays.asList(2, 1, 3, 4);
}
