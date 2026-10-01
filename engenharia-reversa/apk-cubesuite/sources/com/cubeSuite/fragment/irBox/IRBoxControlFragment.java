package com.cubeSuite.fragment.irBox;

import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CompoundButton;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.SpinnerAdapter;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.core.content.ContextCompat;
import androidx.databinding.DataBindingUtil;
import androidx.recyclerview.widget.DividerItemDecoration;
import androidx.recyclerview.widget.LinearLayoutManager;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.adapter.IRBoxAdapter;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.Communication;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.EqView;
import com.cubeSuite.customControl.IRBoxAlert;
import com.cubeSuite.customControl.LoadView;
import com.cubeSuite.databinding.IrBoxContorlFragmentBinding;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.irbox.IRBoxEntry;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.utils.FileUtils;
import com.cubeSuite.utils.PathUtil;
import com.cubeSuite.utils.WaveUtils;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class IRBoxControlFragment extends ControlBaseFragment {
    public ActivityResultLauncher<String[]> activityResultLauncher;
    private IrBoxContorlFragmentBinding binding;
    ArrayAdapter effectSpinnerAdapter;
    IRBoxAdapter irBoxAdapter;
    private IRBoxEntry entry = new IRBoxEntry();
    private List<String> wavFileListPath = new ArrayList();
    private WaveUtils waveUtils = new WaveUtils();
    int currentEffectIndex = 0;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance(IRBoxControlFragment.this.activity).closeDialog();
            return false;
        }
    });

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        this.activityResultLauncher = registerForActivityResult(new ActivityResultContracts.OpenMultipleDocuments(), new ActivityResultCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment$$ExternalSyntheticLambda0
            @Override // androidx.activity.result.ActivityResultCallback
            public final void onActivityResult(Object obj) {
                this.f$0.m236x48054102((List) obj);
            }
        });
        IrBoxContorlFragmentBinding irBoxContorlFragmentBinding = (IrBoxContorlFragmentBinding) DataBindingUtil.inflate(layoutInflater, R.layout.ir_box_contorl_fragment, viewGroup, false);
        this.binding = irBoxContorlFragmentBinding;
        irBoxContorlFragmentBinding.irRv.setLayoutManager(new LinearLayoutManager(getActivity()));
        DividerItemDecoration dividerItemDecoration = new DividerItemDecoration(getActivity(), 1);
        dividerItemDecoration.setDrawable(ContextCompat.getDrawable(getActivity(), R.drawable.rv_divider));
        this.binding.irRv.addItemDecoration(dividerItemDecoration);
        setListener();
        return this.binding.getRoot();
    }

    /* JADX INFO: renamed from: lambda$onCreateView$0$com-cubeSuite-fragment-irBox-IRBoxControlFragment, reason: not valid java name */
    /* synthetic */ void m236x48054102(List list) {
        for (int i = 0; i < list.size(); i++) {
            this.wavFileListPath.add(PathUtil.getPath(getContext(), (Uri) list.get(i)));
            this.irBoxAdapter.updateData(this.wavFileListPath);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setAdapter() {
        this.effectSpinnerAdapter = new ArrayAdapter(getContext(), R.layout.spinner_item, R.id.tvItem, this.entry.irBox.nameArr);
        this.binding.effectSpinner.setAdapter((SpinnerAdapter) this.effectSpinnerAdapter);
        IRBoxAdapter iRBoxAdapter = new IRBoxAdapter(this.wavFileListPath);
        this.irBoxAdapter = iRBoxAdapter;
        iRBoxAdapter.setCallback(new IRBoxAdapter.IRBoxAlertCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.2
            @Override // com.cubeSuite.adapter.IRBoxAdapter.IRBoxAlertCallback
            public void onItemClick(int i) {
                IRBoxControlFragment.this.loadWaveData(i);
            }
        });
        this.binding.irRv.setAdapter(this.irBoxAdapter);
    }

    private void setListener() {
        this.binding.volMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.3
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                int data = IRBoxControlFragment.this.entry.irBox.cad.lvl.getData();
                if (data == 0) {
                    return;
                }
                int i = data - 1;
                IRBoxControlFragment.this.entry.irBox.cad.lvl.setData(i);
                IRBoxControlFragment.this.binding.irVolText.setText(String.valueOf(i));
                IRBoxControlFragment iRBoxControlFragment = IRBoxControlFragment.this;
                iRBoxControlFragment.sendVolData(iRBoxControlFragment.entry.irBox.cad.lvl);
                IRBoxControlFragment.this.binding.irVolumeSeek.setProgress(IRBoxControlFragment.this.entry.irBox.cad.lvl.getData());
            }
        });
        this.binding.volAdd.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.4
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                int data = IRBoxControlFragment.this.entry.irBox.cad.lvl.getData();
                if (data == 127) {
                    return;
                }
                int i = data + 1;
                IRBoxControlFragment.this.entry.irBox.cad.lvl.setData(i);
                IRBoxControlFragment.this.binding.irVolText.setText(String.valueOf(i));
                IRBoxControlFragment iRBoxControlFragment = IRBoxControlFragment.this;
                iRBoxControlFragment.sendVolData(iRBoxControlFragment.entry.irBox.cad.lvl);
                IRBoxControlFragment.this.binding.irVolumeSeek.setProgress(IRBoxControlFragment.this.entry.irBox.cad.lvl.getData());
            }
        });
        this.binding.irVolumeSeek.setMin(0);
        this.binding.irVolumeSeek.setMax(127);
        this.binding.irVolumeSeek.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.5
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                IRBoxControlFragment.this.entry.irBox.cad.lvl.setData(i);
                IRBoxControlFragment.this.binding.irVolText.setText(String.valueOf(i));
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
                IRBoxControlFragment iRBoxControlFragment = IRBoxControlFragment.this;
                iRBoxControlFragment.sendVolData(iRBoxControlFragment.entry.irBox.cad.lvl);
            }
        });
        this.binding.switchIr.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.6
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                IRBoxControlFragment.this.entry.irBox.head.cabon.setData(z ? 1 : 0);
                IRBoxControlFragment iRBoxControlFragment = IRBoxControlFragment.this;
                iRBoxControlFragment.enableIrOrEq(iRBoxControlFragment.entry.irBox.head.cabon);
                IRBoxControlFragment.this.binding.irContainer.setVisibility(z ? 0 : 8);
            }
        });
        this.binding.switchEq.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.7
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                IRBoxControlFragment.this.entry.irBox.head.eqon.setData(z ? 1 : 0);
                IRBoxControlFragment iRBoxControlFragment = IRBoxControlFragment.this;
                iRBoxControlFragment.enableIrOrEq(iRBoxControlFragment.entry.irBox.head.eqon);
                IRBoxControlFragment.this.binding.eqView.setVisibility(z ? 0 : 8);
            }
        });
        this.binding.openFileBut.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.8
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                IRBoxControlFragment.this.activityResultLauncher.launch(new String[]{"audio/x-wav"});
            }
        });
        this.binding.saveBut.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.9
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                new IRBoxAlert(IRBoxControlFragment.this.getContext(), IRBoxControlFragment.this.entry.irBox.nameArr, IRBoxControlFragment.this.currentEffectIndex).setCallback(new IRBoxAlert.IRBoxAlertCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.9.1
                    @Override // com.cubeSuite.customControl.IRBoxAlert.IRBoxAlertCallback
                    public void onSave(String str, int i) {
                        IRBoxControlFragment.this.entry.irBox.nameArr[i] = "(" + (i + 1) + ")" + str;
                        IRBoxControlFragment.this.sendNameData(str, i);
                        IRBoxControlFragment.this.effectSpinnerAdapter.notifyDataSetChanged();
                    }
                }).show();
            }
        });
        this.binding.synBut.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.10
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                IRBoxControlFragment.this.synData();
            }
        });
        this.binding.effectSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.11
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long j) {
                if (i == IRBoxControlFragment.this.currentEffectIndex) {
                    return;
                }
                IRBoxControlFragment.this.currentEffectIndex = i;
                IRBoxControlFragment.this.changeEffect();
            }
        });
        this.binding.eqView.setHzDbQListen(new EqView.HzDbQEListen() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.12
            @Override // com.cubeSuite.customControl.EqView.HzDbQEListen
            public void hzChange(AddrU16 addrU16) {
                IRBoxControlFragment.this.sendU16Data(addrU16);
            }

            @Override // com.cubeSuite.customControl.EqView.HzDbQEListen
            public void dbChange(AddrI8 addrI8) {
                IRBoxControlFragment.this.sendI8Data(addrI8);
            }

            @Override // com.cubeSuite.customControl.EqView.HzDbQEListen
            public void qChange(AddrU8 addrU8) {
                IRBoxControlFragment.this.sendU8Data(addrU8);
            }

            @Override // com.cubeSuite.customControl.EqView.HzDbQEListen
            public void enChange(AddrU8 addrU8) {
                IRBoxControlFragment.this.sendU8Data(addrU8);
            }

            @Override // com.cubeSuite.customControl.EqView.HzDbQEListen
            public void resetEq() {
                IRBoxControlFragment.this.sendEq();
            }
        });
        this.binding.typeRadio.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.13
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup, int i) {
                if (i == R.id.eq_radio) {
                    IRBoxControlFragment.this.binding.irEdit.setVisibility(8);
                    IRBoxControlFragment.this.binding.editEqView.setVisibility(0);
                } else {
                    if (i != R.id.ir_radio) {
                        return;
                    }
                    IRBoxControlFragment.this.binding.irEdit.setVisibility(0);
                    IRBoxControlFragment.this.binding.editEqView.setVisibility(8);
                }
            }
        });
    }

    void sendEq() {
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < 9; i++) {
            arrayList.add(Byte.valueOf((byte) this.entry.irBox.eq.enable[i].getData()));
        }
        for (int i2 = 0; i2 < 5; i2++) {
            arrayList.add(Byte.valueOf((byte) this.entry.irBox.eq.peakQ[i2].getData()));
        }
        for (int i3 = 0; i3 < 5; i3++) {
            arrayList.add(Byte.valueOf(this.entry.irBox.eq.peakDb[i3].getData()));
        }
        arrayList.add(Byte.valueOf(this.entry.irBox.eq.lsDb.getData()));
        arrayList.add(Byte.valueOf(this.entry.irBox.eq.hsDb.getData()));
        arrayList.add(Byte.valueOf((byte) this.entry.irBox.eq.rsv.getData()));
        for (int i4 = 0; i4 < 5; i4++) {
            byte[] bArrU16AddU8Arr = Communication.u16AddU8Arr(this.entry.irBox.eq.peakHz[i4].getData());
            arrayList.add(Byte.valueOf(bArrU16AddU8Arr[0]));
            arrayList.add(Byte.valueOf(bArrU16AddU8Arr[1]));
        }
        byte[] bArrU16AddU8Arr2 = Communication.u16AddU8Arr(this.entry.irBox.eq.lsHz.getData());
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr2[0]));
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr2[1]));
        byte[] bArrU16AddU8Arr3 = Communication.u16AddU8Arr(this.entry.irBox.eq.hsHz.getData());
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr3[0]));
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr3[1]));
        byte[] bArrU16AddU8Arr4 = Communication.u16AddU8Arr(this.entry.irBox.eq.lpHz.getData());
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr4[0]));
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr4[1]));
        byte[] bArrU16AddU8Arr5 = Communication.u16AddU8Arr(this.entry.irBox.eq.hpHz.getData());
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr5[0]));
        arrayList.add(Byte.valueOf(bArrU16AddU8Arr5[1]));
        communicationUtil.splitWriteData((byte) 5, Communication.byteListToByteArr(arrayList), this.entry.irBox.eq.enable[0].getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.14
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(2);
                }
            }
        });
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
    }

    /* JADX INFO: renamed from: com.cubeSuite.fragment.irBox.IRBoxControlFragment$15, reason: invalid class name */
    class AnonymousClass15 implements Callback.BleCommunicationCallback {
        AnonymousClass15() {
        }

        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
        public void readDataCallback(boolean z, final byte[] bArr) {
            if (z) {
                try {
                    Thread.sleep(200L);
                    CommunicationUtil communicationUtil = ControlBaseFragment.communicationUtil;
                    Objects.requireNonNull(IRBoxControlFragment.this.entry);
                    communicationUtil.splitReadData((byte) 5, 0L, 8192, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.15.1
                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                        public void readDataCallback(boolean z2, final byte[] bArr2) {
                            if (z2) {
                                try {
                                    Thread.sleep(200L);
                                    ControlBaseFragment.communicationUtil.splitReadData((byte) 4, 536870912L, 1, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.15.1.1
                                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                                        public void readDataCallback(boolean z3, byte[] bArr3) {
                                            if (z3) {
                                                IRBoxControlFragment.this.currentEffectIndex = bArr3[0];
                                                IRBoxControlFragment.this.entry.setNameData(bArr);
                                                IRBoxControlFragment.this.entry.setData(bArr2);
                                                IRBoxControlFragment.this.binding.eqView.setEqData(IRBoxControlFragment.this.entry.irBox.eq);
                                                IRBoxControlFragment.this.setAdapter();
                                                IRBoxControlFragment.this.bindData();
                                                IRBoxControlFragment.this.handler.sendEmptyMessageDelayed(0, 1000L);
                                            }
                                        }
                                    });
                                } catch (Exception unused) {
                                }
                            }
                        }
                    });
                } catch (Exception unused) {
                }
            }
        }
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        try {
            Thread.sleep(200L);
            CommunicationUtil communicationUtil = communicationUtil;
            Objects.requireNonNull(this.entry);
            communicationUtil.splitReadData((byte) 5, 2147483648L, 544, new AnonymousClass15());
        } catch (Exception unused) {
        }
    }

    public void synData() {
        final LoadView title = new LoadView(getActivity()).setTitle("Loading");
        title.show();
        CommunicationUtil communicationUtil = communicationUtil;
        Objects.requireNonNull(this.entry);
        communicationUtil.splitReadData((byte) 5, 0L, 8192, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.16
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(final boolean z, final byte[] bArr) {
                ControlBaseFragment.communicationUtil.splitReadData((byte) 4, 536870912L, 1, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.16.1
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void readDataCallback(boolean z2, byte[] bArr2) {
                        if (z && z2) {
                            IRBoxControlFragment.this.currentEffectIndex = bArr2[0];
                            IRBoxControlFragment.this.entry.setData(bArr);
                            IRBoxControlFragment.this.binding.eqView.setEqData(IRBoxControlFragment.this.entry.irBox.eq);
                            IRBoxControlFragment.this.bindData();
                        }
                        title.dismiss();
                    }
                });
            }
        });
    }

    public void changeEffect() {
        final LoadView title = new LoadView(getActivity()).setTitle("Loading");
        title.show();
        CommunicationUtil communicationUtil = communicationUtil;
        int i = this.currentEffectIndex;
        communicationUtil.splitWriteData((byte) 4, new byte[]{(byte) i}, ((long) i) + 3758096384L, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.17
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    CommunicationUtil communicationUtil2 = ControlBaseFragment.communicationUtil;
                    Objects.requireNonNull(IRBoxControlFragment.this.entry);
                    communicationUtil2.splitReadData((byte) 5, 0L, 8192, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.17.1
                        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                        public void readDataCallback(boolean z2, byte[] bArr) {
                            if (z2) {
                                IRBoxControlFragment.this.entry.setData(bArr);
                                IRBoxControlFragment.this.binding.eqView.setEqData(IRBoxControlFragment.this.entry.irBox.eq);
                                IRBoxControlFragment.this.bindData();
                            }
                            title.dismiss();
                        }
                    });
                    return;
                }
                title.dismiss();
            }
        });
    }

    public void bindData() {
        this.binding.effectSpinner.setSelection(this.currentEffectIndex);
        this.binding.switchIr.setChecked(this.entry.irBox.head.cabon.getData() == 1);
        this.binding.switchEq.setChecked(this.entry.irBox.head.eqon.getData() == 1);
        this.binding.irVolumeSeek.setProgress(this.entry.irBox.cad.lvl.getData());
        this.binding.irVolText.setText(String.valueOf(this.entry.irBox.cad.lvl.getData()));
        this.binding.irContainer.setVisibility(this.entry.irBox.head.cabon.getData() == 1 ? 0 : 8);
        this.binding.eqView.setVisibility(this.entry.irBox.head.eqon.getData() != 1 ? 8 : 0);
    }

    public void loadWaveData(int i) {
        if (this.wavFileListPath.size() == 0) {
            return;
        }
        final LoadView title = new LoadView(getActivity()).setTitle("Loading");
        title.show();
        List<Byte> file = FileUtils.readFile(this.wavFileListPath.get(i));
        WaveUtils waveUtils = this.waveUtils;
        Objects.requireNonNull(this.entry);
        if (waveUtils.load(file, 6144)) {
            List<Byte> listLinearResampling = this.waveUtils.linearResampling(1.0d);
            ArrayList arrayList = new ArrayList();
            arrayList.add((byte) 67);
            arrayList.add((byte) 65);
            arrayList.add((byte) 66);
            arrayList.add((byte) 0);
            for (int i2 = 0; i2 < 14; i2++) {
                arrayList.add((byte) 0);
            }
            arrayList.add((byte) 2);
            arrayList.add(Byte.valueOf((byte) this.entry.irBox.cad.lvl.getData()));
            arrayList.addAll(listLinearResampling);
            communicationUtil.splitWriteData((byte) 5, Communication.byteListToByteArr(arrayList), this.entry.irBox.cad.magic[0].getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.18
                @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                public void writeDataCallback(boolean z) {
                    title.dismiss();
                    if (z) {
                        IRBoxControlFragment.this.refresh(1);
                    }
                }
            });
        }
    }

    public void sendNameData(String str, final int i) {
        byte[] bArrCopyOfRange;
        byte[] bytes = str.getBytes(StandardCharsets.UTF_8);
        int length = bytes.length;
        Objects.requireNonNull(this.entry);
        if (length > 17) {
            Objects.requireNonNull(this.entry);
            bArrCopyOfRange = Arrays.copyOfRange(bytes, 0, 17);
        } else {
            Objects.requireNonNull(this.entry);
            byte[] bArr = new byte[17];
            int i2 = 0;
            while (true) {
                Objects.requireNonNull(this.entry);
                if (i2 >= 17) {
                    break;
                }
                if (i2 < bytes.length) {
                    bArr[i2] = bytes[i2];
                } else {
                    bArr[i2] = 0;
                }
                i2++;
            }
            bArrCopyOfRange = bArr;
        }
        byte[] bArr2 = new byte[24];
        Objects.requireNonNull(this.entry);
        System.arraycopy(bArrCopyOfRange, 0, bArr2, 0, 17);
        bArr2[17] = (byte) this.entry.irBox.head.cabon.getData();
        bArr2[18] = (byte) this.entry.irBox.head.eqon.getData();
        bArr2[19] = 112;
        bArr2[20] = 97;
        bArr2[21] = 116;
        bArr2[22] = 99;
        bArr2[23] = 104;
        communicationUtil.splitWriteData((byte) 5, bArr2, this.entry.irBox.head.nameAddr, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.19
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.save(i);
                }
            }
        });
    }

    public void enableIrOrEq(AddrU8 addrU8) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{(byte) addrU8.getData()}, addrU8.getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.20
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(1);
                }
            }
        });
    }

    public void sendVolData(AddrU8 addrU8) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{(byte) addrU8.getData()}, addrU8.getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.21
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(3);
                }
            }
        });
    }

    public void refresh(int i) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{(byte) i}, 2684354560L);
    }

    public void save(int i) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{0}, ((long) i) + 4026532096L);
    }

    public void sendU16Data(AddrU16 addrU16) {
        communicationUtil.splitWriteData((byte) 5, Communication.u16AddU8Arr(addrU16.getData()), addrU16.getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.22
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(2);
                }
            }
        });
    }

    public void sendU8Data(AddrU8 addrU8) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{(byte) addrU8.getData()}, addrU8.getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.23
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(2);
                }
            }
        });
    }

    public void sendI8Data(AddrI8 addrI8) {
        communicationUtil.splitWriteData((byte) 5, new byte[]{addrI8.getData()}, addrI8.getAddr(), new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.irBox.IRBoxControlFragment.24
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void writeDataCallback(boolean z) {
                if (z) {
                    IRBoxControlFragment.this.refresh(2);
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return (ControlAndPdfActivity) getActivity();
    }
}
