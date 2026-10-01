package com.cubeSuite.fragment.smcPad;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.GridView;
import android.widget.LinearLayout;
import android.widget.ListAdapter;
import android.widget.TextView;
import com.cubeSuite.R;
import com.google.android.material.tabs.TabLayout;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadKnob extends LinearLayout {
    GridView gridView;
    private int[] groupIndex;
    private int[] groupOneIndex;
    private int[] groupTwoIndex;
    private TabLayout.OnTabSelectedListener listener;
    private TabLayout mTabLayout;
    MyBaseAdapter myBaseAdapter;

    public SmcPadKnob(Context context) {
        super(context);
        this.groupIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupOneIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupTwoIndex = new int[]{14, 15, 12, 13, 10, 11, 8, 9};
        this.listener = new TabLayout.OnTabSelectedListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnob.1
            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabReselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabUnselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabSelected(TabLayout.Tab tab) {
                int position = tab.getPosition();
                if (position == 0) {
                    SmcPadKnob smcPadKnob = SmcPadKnob.this;
                    smcPadKnob.groupIndex = smcPadKnob.groupOneIndex;
                } else if (position == 1) {
                    SmcPadKnob smcPadKnob2 = SmcPadKnob.this;
                    smcPadKnob2.groupIndex = smcPadKnob2.groupTwoIndex;
                }
                SmcPadKnob.this.myBaseAdapter.notifyDataSetChanged();
            }
        };
        initView(context);
    }

    public SmcPadKnob(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.groupIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupOneIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupTwoIndex = new int[]{14, 15, 12, 13, 10, 11, 8, 9};
        this.listener = new TabLayout.OnTabSelectedListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnob.1
            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabReselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabUnselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabSelected(TabLayout.Tab tab) {
                int position = tab.getPosition();
                if (position == 0) {
                    SmcPadKnob smcPadKnob = SmcPadKnob.this;
                    smcPadKnob.groupIndex = smcPadKnob.groupOneIndex;
                } else if (position == 1) {
                    SmcPadKnob smcPadKnob2 = SmcPadKnob.this;
                    smcPadKnob2.groupIndex = smcPadKnob2.groupTwoIndex;
                }
                SmcPadKnob.this.myBaseAdapter.notifyDataSetChanged();
            }
        };
        initView(context);
    }

    public SmcPadKnob(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.groupIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupOneIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupTwoIndex = new int[]{14, 15, 12, 13, 10, 11, 8, 9};
        this.listener = new TabLayout.OnTabSelectedListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnob.1
            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabReselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabUnselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabSelected(TabLayout.Tab tab) {
                int position = tab.getPosition();
                if (position == 0) {
                    SmcPadKnob smcPadKnob = SmcPadKnob.this;
                    smcPadKnob.groupIndex = smcPadKnob.groupOneIndex;
                } else if (position == 1) {
                    SmcPadKnob smcPadKnob2 = SmcPadKnob.this;
                    smcPadKnob2.groupIndex = smcPadKnob2.groupTwoIndex;
                }
                SmcPadKnob.this.myBaseAdapter.notifyDataSetChanged();
            }
        };
        initView(context);
    }

    public SmcPadKnob(Context context, AttributeSet attributeSet, int i, int i2) {
        super(context, attributeSet, i, i2);
        this.groupIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupOneIndex = new int[]{6, 7, 4, 5, 2, 3, 0, 1};
        this.groupTwoIndex = new int[]{14, 15, 12, 13, 10, 11, 8, 9};
        this.listener = new TabLayout.OnTabSelectedListener() { // from class: com.cubeSuite.fragment.smcPad.SmcPadKnob.1
            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabReselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabUnselected(TabLayout.Tab tab) {
            }

            @Override // com.google.android.material.tabs.TabLayout.BaseOnTabSelectedListener
            public void onTabSelected(TabLayout.Tab tab) {
                int position = tab.getPosition();
                if (position == 0) {
                    SmcPadKnob smcPadKnob = SmcPadKnob.this;
                    smcPadKnob.groupIndex = smcPadKnob.groupOneIndex;
                } else if (position == 1) {
                    SmcPadKnob smcPadKnob2 = SmcPadKnob.this;
                    smcPadKnob2.groupIndex = smcPadKnob2.groupTwoIndex;
                }
                SmcPadKnob.this.myBaseAdapter.notifyDataSetChanged();
            }
        };
        initView(context);
    }

    protected void initView(Context context) {
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.smc_pad_knob, this);
        this.gridView = (GridView) viewInflate.findViewById(R.id.knob_grid_view);
        MyBaseAdapter myBaseAdapter = new MyBaseAdapter(context);
        this.myBaseAdapter = myBaseAdapter;
        this.gridView.setAdapter((ListAdapter) myBaseAdapter);
        TabLayout tabLayout = (TabLayout) viewInflate.findViewById(R.id.tab_layout_knob);
        this.mTabLayout = tabLayout;
        tabLayout.addOnTabSelectedListener(this.listener);
    }

    public class MyBaseAdapter extends BaseAdapter {
        private Context mContext;

        @Override // android.widget.Adapter
        public long getItemId(int i) {
            return i;
        }

        class ViewHolder {
            TextView title;

            ViewHolder() {
            }
        }

        public MyBaseAdapter(Context context) {
            this.mContext = context;
        }

        @Override // android.widget.Adapter
        public int getCount() {
            return SmcPadKnob.this.groupIndex.length;
        }

        @Override // android.widget.Adapter
        public Object getItem(int i) {
            return Integer.valueOf(SmcPadKnob.this.groupIndex[i]);
        }

        @Override // android.widget.Adapter
        public View getView(int i, View view, ViewGroup viewGroup) {
            ViewHolder viewHolder;
            if (view == null) {
                view = LayoutInflater.from(this.mContext).inflate(R.layout.smc_knob_item, viewGroup, false);
                viewHolder = new ViewHolder();
                viewHolder.title = (TextView) view.findViewById(R.id.knob_title);
                view.setTag(viewHolder);
            } else {
                viewHolder = (ViewHolder) view.getTag();
            }
            viewHolder.title.setText("Knob " + (SmcPadKnob.this.groupIndex[i] + 1));
            return view;
        }
    }
}
