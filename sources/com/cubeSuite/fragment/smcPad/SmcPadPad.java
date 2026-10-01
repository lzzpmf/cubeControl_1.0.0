package com.cubeSuite.fragment.smcPad;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListAdapter;
import android.widget.RadioGroup;
import com.cubeSuite.R;
import com.cubeSuite.entity.smcPad.SmcPadEntry;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadPad extends LinearLayout {
    private SmcPadEntry.SmcPadGlob glob;
    GridView gridView;
    MyBaseAdapter myBaseAdapter;
    RadioGroup radioGroup;
    SmcPadPadCallback smcPadPadCallback;

    public interface SmcPadPadCallback {
        void onItemClick(int i);
    }

    public SmcPadPad(Context context) {
        super(context);
        initView(context);
    }

    public SmcPadPad(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        initView(context);
    }

    public SmcPadPad(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        initView(context);
    }

    public SmcPadPad(Context context, AttributeSet attributeSet, int i, int i2) {
        super(context, attributeSet, i, i2);
        initView(context);
    }

    protected void initView(Context context) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.smc_pad_pad, this);
        this.gridView = (GridView) viewInflate.findViewById(R.id.pad_grid_view);
        MyBaseAdapter myBaseAdapter = new MyBaseAdapter(context);
        this.myBaseAdapter = myBaseAdapter;
        this.gridView.setAdapter((ListAdapter) myBaseAdapter);
        RadioGroup radioGroup = (RadioGroup) viewInflate.findViewById(R.id.pad_bank_group);
        this.radioGroup = radioGroup;
        radioGroup.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadPad.1
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup2, int i) {
                switch (i) {
                    case R.id.pad_bank_group1 /* 2131296809 */:
                        SmcPadPad.this.smcPadPadCallback.onItemClick(0);
                        break;
                    case R.id.pad_bank_group2 /* 2131296810 */:
                        SmcPadPad.this.smcPadPadCallback.onItemClick(1);
                        break;
                }
            }
        });
    }

    public void updateData(SmcPadEntry.SmcPadGlob smcPadGlob) {
        this.glob = smcPadGlob;
        if (smcPadGlob.padbank.getData() == 0) {
            this.radioGroup.check(R.id.pad_bank_group1);
        } else {
            this.radioGroup.check(R.id.pad_bank_group2);
        }
    }

    public class MyBaseAdapter extends BaseAdapter {
        private final int[] groupIndex = {12, 13, 14, 15, 8, 9, 10, 11, 4, 5, 6, 7, 0, 1, 2, 3};
        private final Context mContext;

        @Override // android.widget.Adapter
        public long getItemId(int i) {
            return i;
        }

        class ViewHolder {
            ImageView bg;

            ViewHolder() {
            }
        }

        public MyBaseAdapter(Context context) {
            this.mContext = context;
        }

        @Override // android.widget.Adapter
        public int getCount() {
            return this.groupIndex.length;
        }

        @Override // android.widget.Adapter
        public Object getItem(int i) {
            return Integer.valueOf(this.groupIndex[i]);
        }

        @Override // android.widget.Adapter
        public View getView(int i, View view, ViewGroup viewGroup) {
            ViewHolder viewHolder;
            if (view == null) {
                view = LayoutInflater.from(this.mContext).inflate(R.layout.smc_pad_item, viewGroup, false);
                viewHolder = new ViewHolder();
                viewHolder.bg = (ImageView) view.findViewById(R.id.pad_bg);
                view.setTag(viewHolder);
            } else {
                viewHolder = (ViewHolder) view.getTag();
            }
            viewHolder.bg.setImageResource(SmcPadPad.this.getResources().getIdentifier("smc_pad_pad_" + i, "drawable", this.mContext.getPackageName()));
            return view;
        }
    }
}
