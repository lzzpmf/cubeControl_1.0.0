package com.cubeSuite.adapter;

import android.R;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

/* JADX INFO: loaded from: classes.dex */
public class SpinnerAdapter extends ArrayAdapter<String> {
    private Context mContext;
    private String[] mStringArray;

    public SpinnerAdapter(Context context, String[] strArr) {
        super(context, R.layout.simple_spinner_item, strArr);
        this.mContext = context;
        this.mStringArray = strArr;
    }

    @Override // android.widget.ArrayAdapter, android.widget.BaseAdapter, android.widget.SpinnerAdapter
    public View getDropDownView(int i, View view, ViewGroup viewGroup) {
        if (view == null) {
            view = LayoutInflater.from(this.mContext).inflate(R.layout.simple_spinner_dropdown_item, viewGroup, false);
        }
        TextView textView = (TextView) view.findViewById(R.id.text1);
        textView.setText(this.mStringArray[i]);
        textView.setTextSize(18.0f);
        textView.setTextColor(this.mContext.getResources().getColor(com.cubeSuite.R.color.dominantHue));
        return view;
    }

    @Override // android.widget.ArrayAdapter, android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        if (view == null) {
            view = LayoutInflater.from(this.mContext).inflate(R.layout.simple_list_item_checked, viewGroup, false);
        }
        TextView textView = (TextView) view.findViewById(R.id.text1);
        textView.setText(this.mStringArray[i]);
        textView.setTextSize(18.0f);
        textView.setTextColor(this.mContext.getResources().getColor(com.cubeSuite.R.color.dominantHue));
        return view;
    }
}
