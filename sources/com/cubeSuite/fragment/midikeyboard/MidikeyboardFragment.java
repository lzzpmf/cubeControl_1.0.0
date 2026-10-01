package com.cubeSuite.fragment.midikeyboard;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import androidx.fragment.app.Fragment;
import androidx.viewpager.widget.ViewPager;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.adapter.BasicFragmentAdapter;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.communication.MidikeyboardCommunication;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.entity.midikeyboard.MidikeyboardEntry;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.google.android.material.tabs.TabLayout;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardFragment extends ControlBaseFragment {
    BasicFragmentAdapter adapter;
    private MidikeyboardEntry entry;
    private MidikeyboardCommunication mc;
    private TabLayout tabLayout;
    private ViewPager viewPager;
    private boolean isInitView = false;
    List<Fragment> fragmentList = new ArrayList();
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance(MidikeyboardFragment.this.activity).closeDialog();
            return false;
        }
    });
    private final String[] titleArray = {"Arp", "SC/CH", "Glob", "Presets"};

    static /* synthetic */ boolean lambda$onCreateView$0(View view, MotionEvent motionEvent) {
        return true;
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        this.entry = new MidikeyboardEntry();
        CommunicationUtil communicationUtil = communicationUtil;
        Objects.requireNonNull(this.entry);
        communicationUtil.splitReadData((byte) 4, 0L, 19, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardFragment.2
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    MidikeyboardFragment.this.entry.setGlobData(bArr);
                    CommunicationUtil communicationUtil2 = ControlBaseFragment.communicationUtil;
                    Objects.requireNonNull(MidikeyboardFragment.this.entry);
                    communicationUtil2.splitReadData((byte) 5, 0L, 3792, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardFragment.2.1
                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                        public void readDataCallback(boolean z2, byte[] bArr2) {
                            if (z2) {
                                MidikeyboardFragment.this.entry.setUsrData(bArr2);
                                MidikeyboardFragment.this.initView();
                                MidikeyboardFragment.this.handler.sendEmptyMessageDelayed(0, 1000L);
                            }
                        }
                    });
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return this.activity;
    }

    public void reLoadUsr() {
        this.fragmentList.remove(3);
        this.fragmentList.add(new MidikeyboardPresetsFragment(this.mc, this, this.entry));
        this.adapter = new BasicFragmentAdapter(getParentFragmentManager(), this.fragmentList, this.titleArray);
        this.viewPager.setOffscreenPageLimit(5);
        this.viewPager.setAdapter(this.adapter);
        this.tabLayout.setupWithViewPager(this.viewPager);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void initView() {
        try {
            getParentFragmentManager();
            if (this.fragmentList.size() == 0) {
                this.mc = new MidikeyboardCommunication(communicationUtil);
                this.fragmentList.add(new MidikeyboardArpFragment(this.entry.getGlob().getArp(), this.mc));
                this.fragmentList.add(new MidikeyboardScChFragment(this.entry.getGlob().getShift(), this.mc));
                this.fragmentList.add(new MidikeyboardGlobFragment(this.entry.getGlob(), this.mc));
                this.fragmentList.add(new MidikeyboardPresetsFragment(this.mc, this, this.entry));
            }
            this.adapter = new BasicFragmentAdapter(getParentFragmentManager(), this.fragmentList, this.titleArray);
            this.viewPager.setOffscreenPageLimit(5);
            this.viewPager.setAdapter(this.adapter);
            this.tabLayout.setupWithViewPager(this.viewPager);
        } catch (IllegalStateException unused) {
        }
    }

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        this.activity = (ControlAndPdfActivity) getActivity();
        View viewInflate = layoutInflater.inflate(R.layout.midikeyboard_control_fragment, viewGroup, false);
        this.tabLayout = (TabLayout) viewInflate.findViewById(R.id.tab_glob);
        ViewPager viewPager = (ViewPager) viewInflate.findViewById(R.id.view_pager_glob);
        this.viewPager = viewPager;
        viewPager.setOnTouchListener(new View.OnTouchListener() { // from class: com.cubeSuite.fragment.midikeyboard.MidikeyboardFragment$$ExternalSyntheticLambda0
            @Override // android.view.View.OnTouchListener
            public final boolean onTouch(View view, MotionEvent motionEvent) {
                return MidikeyboardFragment.lambda$onCreateView$0(view, motionEvent);
            }
        });
        return viewInflate;
    }
}
