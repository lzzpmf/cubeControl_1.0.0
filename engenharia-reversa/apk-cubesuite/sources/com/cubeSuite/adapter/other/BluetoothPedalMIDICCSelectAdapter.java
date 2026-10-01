package com.cubeSuite.adapter.other;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;
import com.cubeSuite.callback.Callback;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class BluetoothPedalMIDICCSelectAdapter extends RecyclerView.Adapter<CustomViewHolder> {
    private Callback.HolderItemClick callback;
    private CustomViewHolder currentItem;
    private int currentSelect;
    private List<String> leftTexts;
    private List<String> rightTexts;

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public long getItemId(int i) {
        return i;
    }

    public BluetoothPedalMIDICCSelectAdapter(int i, List<String> list, List<String> list2) {
        this.currentSelect = i;
        this.leftTexts = list;
        this.rightTexts = list2;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public CustomViewHolder onCreateViewHolder(ViewGroup viewGroup, int i) {
        return new CustomViewHolder(LayoutInflater.from(viewGroup.getContext()).inflate(R.layout.midicc_list_item, viewGroup, false));
    }

    public void setItemClick(Callback.HolderItemClick holderItemClick) {
        this.callback = holderItemClick;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(final CustomViewHolder customViewHolder, final int i) {
        customViewHolder.setInfo(this.leftTexts.get(i), this.rightTexts.get(i));
        if (this.currentSelect == i) {
            customViewHolder.itemView.setBackgroundResource(R.drawable.bg_underline_accent);
            this.currentItem = customViewHolder;
        } else {
            customViewHolder.itemView.setBackgroundResource(R.drawable.bg_underline_dominant_hue);
        }
        customViewHolder.itemView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.adapter.other.BluetoothPedalMIDICCSelectAdapter$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m93xace10047(i, customViewHolder, view);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$onBindViewHolder$0$com-cubeSuite-adapter-other-BluetoothPedalMIDICCSelectAdapter, reason: not valid java name */
    /* synthetic */ void m93xace10047(int i, CustomViewHolder customViewHolder, View view) {
        if (this.currentSelect == i) {
            return;
        }
        this.currentSelect = i;
        CustomViewHolder customViewHolder2 = this.currentItem;
        if (customViewHolder2 != null) {
            customViewHolder2.itemView.setBackgroundResource(R.drawable.bg_underline_dominant_hue);
        }
        this.currentItem = customViewHolder;
        customViewHolder.itemView.setBackgroundResource(R.drawable.bg_underline_accent);
        Callback.HolderItemClick holderItemClick = this.callback;
        if (holderItemClick != null) {
            holderItemClick.click(i);
        }
    }

    public static class CustomViewHolder extends RecyclerView.ViewHolder {
        View itemView;
        private TextView leftText;
        private TextView rightText;

        public void setInfo(String str, String str2) {
            this.leftText.setText(str);
            this.rightText.setText(str2);
        }

        public void setInfo(int i, int i2) {
            this.leftText.setText(i);
            this.rightText.setText(i2);
        }

        public CustomViewHolder(View view) {
            super(view);
            this.itemView = view;
            this.leftText = (TextView) view.findViewById(R.id.leftText);
            this.rightText = (TextView) view.findViewById(R.id.rightText);
        }
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        return this.leftTexts.size();
    }
}
