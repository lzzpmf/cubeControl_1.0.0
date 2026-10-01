package com.cubeSuite.customControl;

import android.content.Context;
import android.graphics.Color;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.SpinnerAdapter;
import android.widget.TextView;
import androidx.core.view.GravityCompat;
import com.cubeSuite.R;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class MidiComBox extends LinearLayout {
    Context context;
    boolean isFirstTime;
    TextView name;
    int selectedItem;
    public Spinner spinner;

    public interface DataChange {
        void dataChange(int i);
    }

    public MidiComBox(Context context, String str, List<String> list, final DataChange dataChange) {
        super(context);
        this.isFirstTime = true;
        this.selectedItem = -1;
        this.context = context;
        int iDpToPx = dpToPx(16);
        setLayoutParams(new LinearLayout.LayoutParams(-1, -2));
        setOrientation(0);
        setPadding(iDpToPx, iDpToPx, iDpToPx, iDpToPx);
        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(-2, -2);
        layoutParams.gravity = GravityCompat.START;
        TextView textView = new TextView(context);
        this.name = textView;
        textView.setLayoutParams(layoutParams);
        this.name.setTextColor(-1);
        this.name.setText(str);
        addView(this.name);
        LinearLayout.LayoutParams layoutParams2 = new LinearLayout.LayoutParams(0, -2, 1.0f);
        LinearLayout linearLayout = new LinearLayout(context);
        linearLayout.setLayoutParams(layoutParams2);
        addView(linearLayout);
        LinearLayout.LayoutParams layoutParams3 = new LinearLayout.LayoutParams(-2, -2);
        layoutParams3.gravity = GravityCompat.END;
        Spinner spinner = new Spinner(context);
        this.spinner = spinner;
        spinner.setLayoutParams(layoutParams3);
        setupAdapter(list);
        this.spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.MidiComBox.1
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long j) {
                if (!MidiComBox.this.isFirstTime) {
                    dataChange.dataChange(i);
                }
                MidiComBox.this.selectedItem = i;
                MidiComBox.this.isFirstTime = false;
            }
        });
        addView(this.spinner);
    }

    void setupAdapter(final List<String> list) {
        this.isFirstTime = true;
        this.spinner.setAdapter((SpinnerAdapter) new ArrayAdapter<String>(this.context, R.layout.smc_spinner_item, list) { // from class: com.cubeSuite.customControl.MidiComBox.2
            @Override // android.widget.ArrayAdapter, android.widget.BaseAdapter, android.widget.SpinnerAdapter
            public View getDropDownView(int i, View view, ViewGroup viewGroup) {
                TextView textView = new TextView(MidiComBox.this.context);
                textView.setPadding(8, 8, 8, 8);
                textView.setTextSize(18.0f);
                textView.setText((CharSequence) list.get(i));
                if (MidiComBox.this.selectedItem == i) {
                    textView.setBackgroundColor(Color.parseColor("#FF4081"));
                    textView.setTextColor(Color.parseColor("#ffffff"));
                    return textView;
                }
                textView.setBackgroundColor(Color.parseColor("#ffffff"));
                textView.setTextColor(Color.parseColor("#000000"));
                return textView;
            }
        });
    }

    public void updateName(String str) {
        this.name.setText(str);
    }

    private int dpToPx(int i) {
        return (int) ((i * getContext().getResources().getDisplayMetrics().density) + 0.5f);
    }
}
