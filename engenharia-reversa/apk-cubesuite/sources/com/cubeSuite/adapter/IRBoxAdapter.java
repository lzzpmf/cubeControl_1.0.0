package com.cubeSuite.adapter;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class IRBoxAdapter extends RecyclerView.Adapter<VH> {
    IRBoxAlertCallback callback;
    private List<String> mDatas;

    public interface IRBoxAlertCallback {
        void onItemClick(int i);
    }

    public static class VH extends RecyclerView.ViewHolder {
        public final TextView title;

        public VH(View view) {
            super(view);
            this.title = (TextView) view.findViewById(R.id.title);
        }
    }

    public void setCallback(IRBoxAlertCallback iRBoxAlertCallback) {
        this.callback = iRBoxAlertCallback;
    }

    public IRBoxAdapter(List<String> list) {
        this.mDatas = list;
    }

    public void updateData(List<String> list) {
        this.mDatas = list;
        notifyDataSetChanged();
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(VH vh, final int i) {
        vh.title.setText(this.mDatas.get(i).substring(this.mDatas.get(i).lastIndexOf("/") + 1));
        vh.itemView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.adapter.IRBoxAdapter.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                IRBoxAdapter.this.callback.onItemClick(i);
            }
        });
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        return this.mDatas.size();
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public VH onCreateViewHolder(ViewGroup viewGroup, int i) {
        return new VH(LayoutInflater.from(viewGroup.getContext()).inflate(R.layout.ir_box_item, viewGroup, false));
    }
}
