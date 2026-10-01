package com.cubeSuite.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public final class ReadAndWriteFileFragmentBinding implements ViewBinding {
    public final EditText editTextTextPersonName;
    public final ScrollView fileList;
    public final LinearLayout main;
    private final LinearLayout rootView;

    private ReadAndWriteFileFragmentBinding(LinearLayout linearLayout, EditText editText, ScrollView scrollView, LinearLayout linearLayout2) {
        this.rootView = linearLayout;
        this.editTextTextPersonName = editText;
        this.fileList = scrollView;
        this.main = linearLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ReadAndWriteFileFragmentBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ReadAndWriteFileFragmentBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.read_and_write_file_fragment, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ReadAndWriteFileFragmentBinding bind(View view) {
        int i = R.id.editTextTextPersonName;
        EditText editText = (EditText) ViewBindings.findChildViewById(view, R.id.editTextTextPersonName);
        if (editText != null) {
            i = R.id.fileList;
            ScrollView scrollView = (ScrollView) ViewBindings.findChildViewById(view, R.id.fileList);
            if (scrollView != null) {
                LinearLayout linearLayout = (LinearLayout) view;
                return new ReadAndWriteFileFragmentBinding(linearLayout, editText, scrollView, linearLayout);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}
