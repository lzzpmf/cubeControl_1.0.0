package com.cubeSuite.adapter.sequencer;

import android.app.Activity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class ToneStrListAdapter extends RecyclerView.Adapter<ViewHolder> {
    private final String[] TONE_TYPE;
    private final Activity activity;
    public int getToneNum;

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView toneText;

        public ViewHolder(View view) {
            super(view);
            this.toneText = (TextView) view.findViewById(R.id.toneText);
        }
    }

    public ToneStrListAdapter(Activity activity) {
        String[] strArr = {"QWE", "RTY", "YUI", "OPA", "SDF", "GHJ", "KLQ", "ZXC", "VBN", "FHJ", "CDR", "GGH", "KIN", "FDC", "ESX", "ASC"};
        this.TONE_TYPE = strArr;
        this.getToneNum = strArr.length;
        this.activity = activity;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public ViewHolder onCreateViewHolder(ViewGroup viewGroup, int i) {
        return new ViewHolder(LayoutInflater.from(viewGroup.getContext()).inflate(R.layout.tone_str_item, viewGroup, false));
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(ViewHolder viewHolder, int i) {
        if (i % 2 == 0) {
            viewHolder.toneText.setTextColor(this.activity.getResources().getColor(R.color.seq1));
        } else {
            viewHolder.toneText.setTextColor(this.activity.getResources().getColor(R.color.seq2));
        }
        viewHolder.toneText.setText(this.TONE_TYPE[i]);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        return this.getToneNum;
    }
}
