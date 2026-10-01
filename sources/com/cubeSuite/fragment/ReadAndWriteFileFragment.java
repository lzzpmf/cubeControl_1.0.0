package com.cubeSuite.fragment;

import android.app.Activity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ScrollView;
import androidx.fragment.app.Fragment;
import com.cubeSuite.R;
import com.cubeSuite.customControl.SlidingDeleteItem;

/* JADX INFO: loaded from: classes.dex */
public class ReadAndWriteFileFragment extends Fragment {
    Activity activity;
    private Button btnSave;
    private ScrollView scrollFileList;
    private View thisView;

    static /* synthetic */ void lambda$initView$0(View view) {
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.thisView = layoutInflater.inflate(R.layout.read_and_write_file_fragment, viewGroup, false);
        initView();
        this.activity = getActivity();
        return this.thisView;
    }

    public void initView() {
        this.btnSave = (Button) this.thisView.findViewById(R.id.save);
        this.scrollFileList = (ScrollView) this.thisView.findViewById(R.id.fileList);
        SlidingDeleteItem slidingDeleteItem = new SlidingDeleteItem(this.activity);
        View viewInflate = LayoutInflater.from(this.activity).inflate(R.layout.skip_item, (ViewGroup) null);
        View viewInflate2 = LayoutInflater.from(this.activity).inflate(R.layout.sliding_delete_item, (ViewGroup) null);
        viewInflate2.findViewById(R.id.btnDelete).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.ReadAndWriteFileFragment$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                ReadAndWriteFileFragment.lambda$initView$0(view);
            }
        });
        slidingDeleteItem.setScrollView(this.scrollFileList);
        slidingDeleteItem.addContentView(viewInflate);
        slidingDeleteItem.addMenuView(viewInflate2);
        slidingDeleteItem.setBtnDelete((Button) viewInflate2.findViewById(R.id.btnDelete));
    }
}
