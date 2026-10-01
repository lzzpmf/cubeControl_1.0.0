package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class HomeListActivityBinding implements ViewBinding {
    public final RecyclerView homeRecyclerView;
    public final ConstraintLayout rootLayout;
    private final ConstraintLayout rootView;
    public final Button searchBleBtn;
    public final TextView textView;
    public final Button updatesAvailable;

    private HomeListActivityBinding(ConstraintLayout constraintLayout, RecyclerView recyclerView, ConstraintLayout constraintLayout2, Button button, TextView textView, Button button2) {
        this.rootView = constraintLayout;
        this.homeRecyclerView = recyclerView;
        this.rootLayout = constraintLayout2;
        this.searchBleBtn = button;
        this.textView = textView;
        this.updatesAvailable = button2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public ConstraintLayout getRoot() {
        return this.rootView;
    }

    public static HomeListActivityBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static HomeListActivityBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.home_list_activity, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static HomeListActivityBinding bind(View view) {
        int i = R.id.home_recycler_view;
        RecyclerView recyclerView = (RecyclerView) ViewBindings.findChildViewById(view, R.id.home_recycler_view);
        if (recyclerView != null) {
            ConstraintLayout constraintLayout = (ConstraintLayout) view;
            i = R.id.search_ble_btn;
            Button button = (Button) ViewBindings.findChildViewById(view, R.id.search_ble_btn);
            if (button != null) {
                i = R.id.textView;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, R.id.textView);
                if (textView != null) {
                    i = R.id.updatesAvailable;
                    Button button2 = (Button) ViewBindings.findChildViewById(view, R.id.updatesAvailable);
                    if (button2 != null) {
                        return new HomeListActivityBinding(constraintLayout, recyclerView, constraintLayout, button, textView, button2);
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
