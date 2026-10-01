package com.cubeSuite.fragment;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RadioGroup;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class SelectDrumFragment extends Fragment {
    Activity activity;
    private RadioGroup btnSelectDrum;
    private View thisView;

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.select_foot_ctrl_mode_layout, viewGroup, false);
        initView();
        this.activity = getActivity();
        return this.thisView;
    }

    public void initView() {
        RadioGroup radioGroup = (RadioGroup) this.thisView.findViewById(R.id.btnSelectDrum);
        this.btnSelectDrum = radioGroup;
        radioGroup.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.SelectDrumFragment.1
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup2, int i) {
                Log.e("asdasdsada", String.valueOf(i));
            }
        });
    }
}
