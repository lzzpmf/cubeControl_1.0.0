package com.cubeSuite.adapter.sequencer;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.recyclerview.widget.RecyclerView;
import com.cubeSuite.R;
import com.cubeSuite.customControl.BlockProgressBar;
import com.cubeSuite.entity.SeqBlockPoint;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class SequencerBlockListAdapter extends RecyclerView.Adapter<ViewHolder> {
    private int beatNum = 4;
    private int noteNum = 4;
    List<SeqBlockPoint> seqBlockPointList;

    public int getColumn() {
        return this.beatNum * this.noteNum;
    }

    public int getBeatNum() {
        return this.beatNum;
    }

    public void setBeatNum(int i) {
        this.beatNum = i;
    }

    public int getNoteNum() {
        return this.noteNum;
    }

    public void setNoteNum(int i) {
        this.noteNum = i;
    }

    public class ViewHolder extends RecyclerView.ViewHolder {
        BlockProgressBar blockProgressBar;

        public ViewHolder(View view) {
            super(view);
            this.blockProgressBar = (BlockProgressBar) view.findViewById(R.id.blockItem);
        }
    }

    public SequencerBlockListAdapter(List<SeqBlockPoint> list) {
        this.seqBlockPointList = list;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public ViewHolder onCreateViewHolder(ViewGroup viewGroup, int i) {
        return new ViewHolder(LayoutInflater.from(viewGroup.getContext()).inflate(R.layout.sequencer_item, viewGroup, false));
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(ViewHolder viewHolder, int i) {
        viewHolder.setIsRecyclable(false);
        int[] iArr = new int[2];
        viewHolder.blockProgressBar.getLocationOnScreen(iArr);
        SeqBlockPoint seqBlockPoint = this.seqBlockPointList.get(i);
        int i2 = iArr[0];
        int i3 = iArr[1];
        seqBlockPoint.setPos(i2, i3, i2, i3);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        return this.seqBlockPointList.size();
    }
}
