package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class HomeListItemBinding implements ViewBinding {
    public final ImageView effectUnitImg;
    public final TextView effectUnitText;
    public final TextView needPair;
    private final LinearLayout rootView;

    private HomeListItemBinding(LinearLayout linearLayout, ImageView imageView, TextView textView, TextView textView2) {
        this.rootView = linearLayout;
        this.effectUnitImg = imageView;
        this.effectUnitText = textView;
        this.needPair = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static HomeListItemBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static HomeListItemBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.home_list_item, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static HomeListItemBinding bind(View view) {
        int i = R.id.effect_unit_img;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, R.id.effect_unit_img);
        if (imageView != null) {
            i = R.id.effect_unit_text;
            TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.effect_unit_text);
            if (textView != null) {
                i = R.id.needPair;
                TextView textView2 = (TextView) ViewBindings.findChildViewById(view, R.id.needPair);
                if (textView2 != null) {
                    return new HomeListItemBinding((LinearLayout) view, imageView, textView, textView2);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
