package com.cubeSuite.utils;

import android.text.InputFilter;
import android.text.Spanned;

/* JADX INFO: loaded from: classes.dex */
public class NumEditFilter implements InputFilter {
    public int maxValue;
    public int minValue;

    public NumEditFilter(int i, int i2) {
        this.minValue = i;
        this.maxValue = i2;
    }

    public NumEditFilter() {
        this.minValue = 0;
        this.maxValue = 127;
    }

    @Override // android.text.InputFilter
    public CharSequence filter(CharSequence charSequence, int i, int i2, Spanned spanned, int i3, int i4) {
        String string = spanned.toString();
        if ("".equals(charSequence.toString())) {
            return null;
        }
        double num = StringUtils.getNum(string + charSequence.toString());
        if (num > this.maxValue) {
            return spanned.subSequence(i3, i4);
        }
        if (num == this.minValue && charSequence.toString().equals(".")) {
            return spanned.subSequence(i3, i4);
        }
        return ((Object) spanned.subSequence(i3, i4)) + charSequence.toString();
    }
}
