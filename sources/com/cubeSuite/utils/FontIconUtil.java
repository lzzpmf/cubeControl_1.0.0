package com.cubeSuite.utils;

import android.content.Context;
import android.graphics.Typeface;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

/* JADX INFO: loaded from: classes.dex */
public class FontIconUtil {
    public static final String FONTAWESOME = "fonts/fontawesome-webfont.ttf";
    public static final String ROOT = "fonts/";

    public static Typeface getTypeface(Context context, String str) {
        return Typeface.createFromAsset(context.getAssets(), str);
    }

    public static void markAsIconContainer(View view, Typeface typeface) {
        if (view instanceof ViewGroup) {
            ViewGroup viewGroup = (ViewGroup) view;
            for (int i = 0; i < viewGroup.getChildCount(); i++) {
                markAsIconContainer(viewGroup.getChildAt(i), typeface);
            }
            return;
        }
        if (view instanceof TextView) {
            ((TextView) view).setTypeface(typeface);
        }
    }
}
