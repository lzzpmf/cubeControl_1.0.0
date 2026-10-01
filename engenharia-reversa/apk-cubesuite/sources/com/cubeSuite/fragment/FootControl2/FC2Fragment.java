package com.cubeSuite.fragment.FootControl2;

import android.graphics.Color;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.provider.DocumentsContract;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.ImageButton;
import android.widget.SpinnerAdapter;
import android.widget.TextView;
import android.widget.Toast;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import com.contrarywind.listener.OnItemSelectedListener;
import com.contrarywind.view.WheelView;
import com.cubeSuite.Global.Global;
import com.cubeSuite.R;
import com.cubeSuite.activitys.BaseDeviceActivity;
import com.cubeSuite.activitys.ControlAndPdfActivity;
import com.cubeSuite.adapter.other.ArrayWheelAdapter;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.communication.Communication;
import com.cubeSuite.communication.CommunicationUtil;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.customControl.ButtonIconView;
import com.cubeSuite.customControl.ListAlertDialog;
import com.cubeSuite.customControl.SelectCombKey;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.fc2.BluetoothPedalMIDICCSelectAdapter1;
import com.cubeSuite.entity.fc2.FC2Struct;
import com.cubeSuite.fragment.ControlBaseFragment;
import com.cubeSuite.utils.PathUtil;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Iterator;
import java.util.List;
import java.util.Objects;

/* JADX INFO: loaded from: classes.dex */
public class FC2Fragment extends ControlBaseFragment {
    private AppCompatSpinner advancedCustomMode1Spinner;
    private View advancedCustomMode1View;
    private View customControlView;
    private AppCompatSpinner customKeySpinner;
    private View customKeyboardView;
    EditInterfaceListPopupWindow editInterfaceListPopupWindow;
    public ActivityResultLauncher<Uri> exportLauncher;
    String fileName;
    public ActivityResultLauncher<String[]> importLauncher;
    private View maxGroupCountContainer31;
    private AppCompatSpinner maxGroupCountSpinner15;
    private AppCompatSpinner maxGroupCountSpinner31;
    SwitchCompat midiSwitch;
    private AppCompatSpinner mixKeyModeSpinner;
    private View mixKeyView;
    private TextView modeExplainTv;
    private WheelView modeSelect;
    private View pcdispContainer;
    SwitchCompat pcdispSwitch;
    TextView sendMidiChannel;
    TrsMidiPopupWindow trsMidiWindow;
    private FC2Struct fcEntry = new FC2Struct();
    private final int INTERFACE_TYPE = 4;
    int advMode1Page = 0;
    int advMixKeyPage = 0;
    int customKeyboardPage = 0;
    private final Button[] customKeyboardBtnArr = new Button[4];
    private Button[] customControlButArr = new Button[5];
    private final Button[] advMode1BtnArr = new Button[4];
    private final Button[] mixKeyBtnArr = new Button[4];
    private EditAdvMode1ListPopupWindow[][] editAdvMode1ListPopupWindow = (EditAdvMode1ListPopupWindow[][]) Array.newInstance((Class<?>) EditAdvMode1ListPopupWindow.class, 10, 4);
    private MixKeyListPopupWindow[][] mixKeyPopupWindow = (MixKeyListPopupWindow[][]) Array.newInstance((Class<?>) MixKeyListPopupWindow.class, 6, 4);
    private final int[] FOOT_SWITCH_NAME = {R.string.footSwitchA, R.string.footSwitchB, R.string.footSwitchC, R.string.footSwitchD, R.string.pedal};
    int[] modelsTextExplain = {R.string.programSwitchA2Explain, R.string.programSwitchB1Explain, R.string.customModeExplain, R.string.advancedCustomModeExplain, R.string.manufacturerEquipmentControlModeExplain, R.string.touchScreenModeExplain, R.string.videoModelExplain, R.string.keyboardModeAExplain, R.string.keyboardModeBExplain, R.string.multimediaKeyModeExplain, R.string.customKeyboardMode1Explain, R.string.mixKeyModeExplain, R.string.SpeakerModeExplain};
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            if (message.what != 0) {
                return false;
            }
            AlertDialogUtil.getInstance(FC2Fragment.this.activity).closeDialog();
            return false;
        }
    });

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        super.onCreate(bundle);
        final int i = 0;
        View viewInflate = layoutInflater.inflate(R.layout.fc2_layout, viewGroup, false);
        this.modeSelect = (WheelView) viewInflate.findViewById(R.id.modeSelect);
        this.modeExplainTv = (TextView) viewInflate.findViewById(R.id.mode_explain_tv);
        this.customControlView = viewInflate.findViewById(R.id.customControlView);
        this.advancedCustomMode1View = viewInflate.findViewById(R.id.advancedCustomMode1View);
        this.mixKeyView = viewInflate.findViewById(R.id.mixKeyView);
        this.customKeyboardView = viewInflate.findViewById(R.id.customKeyboardView);
        ButtonIconView buttonIconView = (ButtonIconView) viewInflate.findViewById(R.id.trsMidi);
        ButtonIconView buttonIconView2 = (ButtonIconView) viewInflate.findViewById(R.id.midiChannelMinus);
        ButtonIconView buttonIconView3 = (ButtonIconView) viewInflate.findViewById(R.id.midiChannelPlus);
        this.sendMidiChannel = (TextView) viewInflate.findViewById(R.id.sendMidiChannel);
        this.customControlButArr[0] = (Button) viewInflate.findViewById(R.id.customControlBut1);
        this.customControlButArr[1] = (Button) viewInflate.findViewById(R.id.customControlBut2);
        this.customControlButArr[2] = (Button) viewInflate.findViewById(R.id.customControlBut3);
        this.customControlButArr[3] = (Button) viewInflate.findViewById(R.id.customControlBut4);
        this.customControlButArr[4] = (Button) viewInflate.findViewById(R.id.customControlBut5);
        this.customKeyboardBtnArr[0] = (Button) viewInflate.findViewById(R.id.keyA);
        this.customKeyboardBtnArr[1] = (Button) viewInflate.findViewById(R.id.keyB);
        this.customKeyboardBtnArr[2] = (Button) viewInflate.findViewById(R.id.keyC);
        this.customKeyboardBtnArr[3] = (Button) viewInflate.findViewById(R.id.keyD);
        this.customKeySpinner = (AppCompatSpinner) viewInflate.findViewById(R.id.custom_key_spinner);
        ImageButton imageButton = (ImageButton) viewInflate.findViewById(R.id.updatePedalParam);
        this.advMode1BtnArr[0] = (Button) viewInflate.findViewById(R.id.advancedCustomMode1But1);
        this.advMode1BtnArr[1] = (Button) viewInflate.findViewById(R.id.advancedCustomMode1But2);
        this.advMode1BtnArr[2] = (Button) viewInflate.findViewById(R.id.advancedCustomMode1But3);
        this.advMode1BtnArr[3] = (Button) viewInflate.findViewById(R.id.advancedCustomMode1But4);
        this.mixKeyBtnArr[0] = (Button) viewInflate.findViewById(R.id.advancedCustomMode2But1);
        this.mixKeyBtnArr[1] = (Button) viewInflate.findViewById(R.id.advancedCustomMode2But2);
        this.mixKeyBtnArr[2] = (Button) viewInflate.findViewById(R.id.advancedCustomMode2But3);
        this.mixKeyBtnArr[3] = (Button) viewInflate.findViewById(R.id.advancedCustomMode2But4);
        this.advancedCustomMode1Spinner = (AppCompatSpinner) viewInflate.findViewById(R.id.advancedCustomMode1Spinner);
        this.mixKeyModeSpinner = (AppCompatSpinner) viewInflate.findViewById(R.id.mixKeyModeSpinner);
        this.maxGroupCountContainer31 = viewInflate.findViewById(R.id.maxGroupCountContainer31);
        this.maxGroupCountSpinner31 = (AppCompatSpinner) viewInflate.findViewById(R.id.maxGroupCountSpinner31);
        this.maxGroupCountSpinner15 = (AppCompatSpinner) viewInflate.findViewById(R.id.maxGroupCountSpinner15);
        this.midiSwitch = (SwitchCompat) viewInflate.findViewById(R.id.midiSwitch);
        this.pcdispSwitch = (SwitchCompat) viewInflate.findViewById(R.id.pcdispSwitch);
        this.pcdispContainer = viewInflate.findViewById(R.id.pcdispContainer);
        this.midiSwitch.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.2
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                FC2Fragment.this.fcEntry.bankMidi.setData(z ? 1 : 0);
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.bankMidi);
            }
        });
        this.pcdispSwitch.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.3
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                FC2Fragment.this.fcEntry.pcdisp.setData(z ? 1 : 0);
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.pcdisp);
            }
        });
        ArrayList arrayList = new ArrayList();
        for (int i2 = 0; i2 < 32; i2++) {
            arrayList.add(((i2 * 4) + 3) + "");
        }
        this.maxGroupCountSpinner31.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.custom_spiner_item, arrayList));
        this.maxGroupCountSpinner31.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.4
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i3, long j) {
                if (FC2Fragment.this.fcEntry.state.getData() == 0) {
                    FC2Fragment.this.fcEntry.bankMax[0].setData(i3);
                    FC2Fragment fC2Fragment = FC2Fragment.this;
                    fC2Fragment.sendData(fC2Fragment.fcEntry.bankMax[0]);
                } else if (FC2Fragment.this.fcEntry.state.getData() == 1) {
                    FC2Fragment.this.fcEntry.bankMax[1].setData(i3);
                    FC2Fragment fC2Fragment2 = FC2Fragment.this;
                    fC2Fragment2.sendData(fC2Fragment2.fcEntry.bankMax[1]);
                }
            }
        });
        ArrayList arrayList2 = new ArrayList();
        int i3 = 0;
        while (i3 < 8) {
            i3++;
            arrayList2.add(i3 + "");
        }
        this.maxGroupCountSpinner15.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.custom_spiner_item, arrayList2));
        this.maxGroupCountSpinner15.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.5
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (FC2Fragment.this.fcEntry.state.getData() == 3) {
                    FC2Fragment.this.fcEntry.bankMax[2].setData(i4);
                    FC2Fragment fC2Fragment = FC2Fragment.this;
                    fC2Fragment.sendData(fC2Fragment.fcEntry.bankMax[2]);
                }
            }
        });
        ((Button) viewInflate.findViewById(R.id.foot_ctrl_import)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.6
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                FC2Fragment.this.importLauncher.launch(new String[]{"*/*"});
            }
        });
        ((Button) viewInflate.findViewById(R.id.foot_ctrl_export)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.7
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                new FootCtrl2NameInputBox(FC2Fragment.this.getContext()).setCallback(new FootCtrl2NameInputBox.FootCtrl2NameInputBoxCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.7.1
                    @Override // com.cubeSuite.fragment.FootControl2.FootCtrl2NameInputBox.FootCtrl2NameInputBoxCallback
                    public void onSave(String str) {
                        FC2Fragment.this.fileName = str;
                        FC2Fragment.this.exportLauncher.launch(Uri.parse(""));
                    }
                }).show();
            }
        });
        this.advancedCustomMode1Spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.8
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (FC2Fragment.this.advMode1Page == i4) {
                    return;
                }
                FC2Fragment.this.advMode1Page = i4;
                FC2Fragment.this.fcEntry.usrpage.setData(i4);
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.usrpage);
            }
        });
        this.mixKeyModeSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.9
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (FC2Fragment.this.advMixKeyPage == i4) {
                    return;
                }
                FC2Fragment.this.advMixKeyPage = i4;
                FC2Fragment.this.fcEntry.mixpage.setData(i4);
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.mixpage);
            }
        });
        this.customKeySpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.10
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (FC2Fragment.this.customKeyboardPage == i4) {
                    return;
                }
                FC2Fragment.this.customKeyboardPage = i4;
                FC2Fragment.this.fcEntry.hidpage.setData(i4);
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.hidpage);
            }
        });
        ArrayList arrayList3 = new ArrayList();
        int i4 = 0;
        while (i4 < 8) {
            i4++;
            arrayList3.add(i4 + "");
        }
        this.advancedCustomMode1Spinner.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.custom_spiner_item, arrayList3));
        ArrayList arrayList4 = new ArrayList();
        int i5 = 0;
        while (i5 < 6) {
            i5++;
            arrayList4.add(i5 + "");
        }
        this.mixKeyModeSpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.custom_spiner_item, arrayList4));
        ArrayList arrayList5 = new ArrayList();
        int i6 = 0;
        while (i6 < 6) {
            i6++;
            arrayList5.add(i6 + "");
        }
        this.customKeySpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.custom_spiner_item, arrayList5));
        final int i7 = 0;
        while (true) {
            Button[] buttonArr = this.advMode1BtnArr;
            if (i7 >= buttonArr.length) {
                break;
            }
            buttonArr[i7].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda0
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m156x17154b47(i7, view);
                }
            });
            i7++;
        }
        final int i8 = 0;
        while (true) {
            Button[] buttonArr2 = this.mixKeyBtnArr;
            if (i8 >= buttonArr2.length) {
                break;
            }
            buttonArr2[i8].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda2
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m157x169ee548(i8, view);
                }
            });
            i8++;
        }
        imageButton.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda3
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m158x16287f49(view);
            }
        });
        final int i9 = 0;
        while (true) {
            Button[] buttonArr3 = this.customKeyboardBtnArr;
            if (i9 >= buttonArr3.length) {
                break;
            }
            buttonArr3[i9].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda4
                @Override // android.view.View.OnClickListener
                public final void onClick(View view) {
                    this.f$0.m159x15b2194a(i9, view);
                }
            });
            i9++;
        }
        while (true) {
            Button[] buttonArr4 = this.customControlButArr;
            if (i < buttonArr4.length) {
                buttonArr4[i].setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda5
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m160x153bb34b(i, view);
                    }
                });
                i++;
            } else {
                buttonIconView.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.11
                    @Override // android.view.View.OnClickListener
                    public void onClick(View view) {
                        FC2Fragment.this.showTrsMidiSlect();
                    }
                });
                buttonIconView3.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda6
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m161x14c54d4c(view);
                    }
                });
                buttonIconView2.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda7
                    @Override // android.view.View.OnClickListener
                    public final void onClick(View view) {
                        this.f$0.m162x144ee74d(view);
                    }
                });
                initModeSelect();
                this.importLauncher = registerForActivityResult(new ActivityResultContracts.OpenMultipleDocuments(), new ActivityResultCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda8
                    @Override // androidx.activity.result.ActivityResultCallback
                    public final void onActivityResult(Object obj) {
                        this.f$0.m163x13d8814e((List) obj);
                    }
                });
                this.exportLauncher = registerForActivityResult(new ActivityResultContracts.OpenDocumentTree(), new ActivityResultCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda9
                    @Override // androidx.activity.result.ActivityResultCallback
                    public final void onActivityResult(Object obj) {
                        this.f$0.m164x13621b4f((Uri) obj);
                    }
                });
                return viewInflate;
            }
        }
    }

    /* JADX INFO: renamed from: lambda$onCreateView$0$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m156x17154b47(int i, View view) {
        showEditAdvMode1ListPopupWindow(i);
    }

    /* JADX INFO: renamed from: lambda$onCreateView$1$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m157x169ee548(int i, View view) {
        showMixKeyListPopupWindow(i);
    }

    /* JADX INFO: renamed from: lambda$onCreateView$2$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m158x16287f49(View view) {
        showEditMidiWindow();
    }

    /* JADX INFO: renamed from: lambda$onCreateView$3$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m159x15b2194a(int i, View view) {
        showCustomKeyboardDialog(i);
    }

    /* JADX INFO: renamed from: lambda$onCreateView$4$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m160x153bb34b(int i, View view) {
        showCustomControlDialog(i);
    }

    /* JADX INFO: renamed from: lambda$onCreateView$5$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m161x14c54d4c(View view) {
        if (this.fcEntry.ch.getData() < 15) {
            this.fcEntry.ch.setData(this.fcEntry.ch.getData() + 1);
            this.sendMidiChannel.setText(String.valueOf(this.fcEntry.ch.getData() + 1));
            sendData(this.fcEntry.ch);
        }
    }

    /* JADX INFO: renamed from: lambda$onCreateView$6$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m162x144ee74d(View view) {
        if (this.fcEntry.ch.getData() > 0) {
            this.fcEntry.ch.setData(this.fcEntry.ch.getData() - 1);
            this.sendMidiChannel.setText(String.valueOf(this.fcEntry.ch.getData() + 1));
            sendData(this.fcEntry.ch);
        }
    }

    /* JADX INFO: renamed from: lambda$onCreateView$7$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m163x13d8814e(List list) {
        for (int i = 0; i < list.size(); i++) {
            String path = PathUtil.getPath(getContext(), (Uri) list.get(i));
            if (PathUtil.getFileExtension(path).equals("fcp")) {
                final ArrayList arrayList = new ArrayList();
                try {
                    FileInputStream fileInputStream = new FileInputStream(path);
                    while (true) {
                        try {
                            int i2 = fileInputStream.read();
                            if (i2 == -1) {
                                break;
                            } else {
                                arrayList.add(Byte.valueOf((byte) i2));
                            }
                            AlertDialogUtil.getInstance(this.activity).setContent(R.string.file_importing).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).showDialog();
                            communicationUtil.splitWriteData((byte) 4, Communication.byteListToByteArr(arrayList), 0L, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.12
                                @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                                public void writeDataCallback(boolean z) {
                                    if (z) {
                                        FC2Fragment.this.fcEntry.setData(Communication.byteListToByteArr(arrayList));
                                        FC2Fragment.this.initView();
                                    }
                                    AlertDialogUtil.getInstance(FC2Fragment.this.activity).closeDialog();
                                }
                            });
                        } catch (Throwable th) {
                            try {
                                fileInputStream.close();
                            } catch (Throwable th2) {
                                th.addSuppressed(th2);
                            }
                            throw th;
                        }
                        e.printStackTrace();
                    }
                    fileInputStream.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
                AlertDialogUtil.getInstance(this.activity).setContent(R.string.file_importing).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).showDialog();
                communicationUtil.splitWriteData((byte) 4, Communication.byteListToByteArr(arrayList), 0L, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.12
                    @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
                    public void writeDataCallback(boolean z) {
                        if (z) {
                            FC2Fragment.this.fcEntry.setData(Communication.byteListToByteArr(arrayList));
                            FC2Fragment.this.initView();
                        }
                        AlertDialogUtil.getInstance(FC2Fragment.this.activity).closeDialog();
                    }
                });
            } else {
                Toast.makeText(getContext(), R.string.file_type_error, 0).show();
            }
        }
    }

    /* JADX INFO: renamed from: lambda$onCreateView$8$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m164x13621b4f(Uri uri) {
        if (uri == null) {
            Toast.makeText(getContext(), "uri null", 0).show();
            return;
        }
        try {
            Uri uriCreateDocument = DocumentsContract.createDocument(getContext().getContentResolver(), DocumentsContract.buildDocumentUriUsingTree(uri, DocumentsContract.getTreeDocumentId(uri)), "application/octet-stream", this.fileName + ".fcp");
            if (uriCreateDocument == null) {
                Toast.makeText(getContext(), "createDocument failed", 0).show();
                return;
            }
            OutputStream outputStreamOpenOutputStream = getContext().getContentResolver().openOutputStream(uriCreateDocument);
            if (outputStreamOpenOutputStream != null) {
                try {
                    Iterator<Byte> it = this.fcEntry.getData().iterator();
                    while (it.hasNext()) {
                        outputStreamOpenOutputStream.write(it.next().byteValue());
                    }
                    Toast.makeText(getContext(), "Success", 0).show();
                } catch (Throwable th) {
                    if (outputStreamOpenOutputStream != null) {
                        try {
                            outputStreamOpenOutputStream.close();
                        } catch (Throwable th2) {
                            th.addSuppressed(th2);
                        }
                    }
                    throw th;
                }
            }
            if (outputStreamOpenOutputStream != null) {
                outputStreamOpenOutputStream.close();
            }
        } catch (IOException e) {
            e.printStackTrace();
            Toast.makeText(getContext(), e.getMessage(), 0).show();
        }
    }

    void showEditAdvMode1ListPopupWindow(int i) {
        int selectedItemPosition = this.advancedCustomMode1Spinner.getSelectedItemPosition();
        EditAdvMode1ListPopupWindow[] editAdvMode1ListPopupWindowArr = this.editAdvMode1ListPopupWindow[selectedItemPosition];
        if (editAdvMode1ListPopupWindowArr[i] == null) {
            editAdvMode1ListPopupWindowArr[i] = new EditAdvMode1ListPopupWindow(getActivity(), getView(), getString(this.FOOT_SWITCH_NAME[i]), communicationUtil);
        }
        this.editAdvMode1ListPopupWindow[selectedItemPosition][i].updateView(this.fcEntry.advCustom1[selectedItemPosition][i]);
        this.editAdvMode1ListPopupWindow[selectedItemPosition][i].showAtLocation(getView(), 80, 0, 0);
    }

    void showMixKeyListPopupWindow(int i) {
        int selectedItemPosition = this.mixKeyModeSpinner.getSelectedItemPosition();
        MixKeyListPopupWindow[] mixKeyListPopupWindowArr = this.mixKeyPopupWindow[selectedItemPosition];
        if (mixKeyListPopupWindowArr[i] == null) {
            mixKeyListPopupWindowArr[i] = new MixKeyListPopupWindow(getActivity(), getView(), getString(this.FOOT_SWITCH_NAME[i]), communicationUtil);
        }
        this.mixKeyPopupWindow[selectedItemPosition][i].updateView(this.fcEntry.mixKey[selectedItemPosition][i]);
        this.mixKeyPopupWindow[selectedItemPosition][i].showAtLocation(getView(), 80, 0, 0);
    }

    void showEditMidiWindow() {
        if (this.editInterfaceListPopupWindow == null) {
            this.editInterfaceListPopupWindow = new EditInterfaceListPopupWindow(getActivity(), getView(), communicationUtil);
        }
        this.editInterfaceListPopupWindow.updateView(this.fcEntry.midiCodeTap);
        this.editInterfaceListPopupWindow.showAtLocation(getView(), 80, 0, 0);
    }

    private void showCustomKeyboardDialog(final int i) {
        final int selectedItemPosition = this.customKeySpinner.getSelectedItemPosition();
        new SelectCombKey(getContext()).setTitle("FootSwitch[" + (i + 1) + "]").setData(this.fcEntry.customKey[selectedItemPosition][i][0].getData(), this.fcEntry.customKey[selectedItemPosition][i][1].getData(), this.fcEntry.customKey[selectedItemPosition][i][2].getData()).showDialog().setListening(new SelectCombKey.Listening() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.13
            @Override // com.cubeSuite.customControl.SelectCombKey.Listening
            public void dataChange(byte b, byte b2, byte b3) {
                FC2Fragment.this.fcEntry.customKey[selectedItemPosition][i][0].setData(b);
                FC2Fragment.this.fcEntry.customKey[selectedItemPosition][i][1].setData(b2);
                FC2Fragment.this.fcEntry.customKey[selectedItemPosition][i][2].setData(b3);
                ControlBaseFragment.communicationUtil.splitWriteData((byte) 4, new byte[]{b, b2, b3}, FC2Fragment.this.fcEntry.customKey[selectedItemPosition][i][0].getAddr());
            }
        });
    }

    public void showCustomControlDialog(final int i) {
        String[] strArr = Global.BluetoothPedalCustomCommand.MIDICC_STR;
        final String[] strArr2 = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR;
        if (i == 4) {
            strArr = Global.BluetoothPedalCustomCommand.MIDICC_STR_KNOB;
            strArr2 = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR_KNOB;
        }
        BluetoothPedalMIDICCSelectAdapter1 bluetoothPedalMIDICCSelectAdapter1 = new BluetoothPedalMIDICCSelectAdapter1(this.fcEntry.usr[i][1].getData(), Arrays.asList(strArr), Arrays.asList(strArr2));
        bluetoothPedalMIDICCSelectAdapter1.setHasStableIds(true);
        bluetoothPedalMIDICCSelectAdapter1.setItemClick(new Callback.HolderItemClick() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda10
            @Override // com.cubeSuite.callback.Callback.HolderItemClick
            public final void click(int i2) {
                this.f$0.m166xc342b24d(i, strArr2, i2);
            }
        });
        ListAlertDialog listAlertDialog = new ListAlertDialog(getContext());
        listAlertDialog.setTitle(this.FOOT_SWITCH_NAME[i]);
        listAlertDialog.setContent(bluetoothPedalMIDICCSelectAdapter1);
        if (i != 4) {
            listAlertDialog.setToggleChecked(this.fcEntry.usr[i][0].getData() == 1);
            listAlertDialog.setToggleClick(new ListAlertDialog.ToggleClick() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment$$ExternalSyntheticLambda1
                @Override // com.cubeSuite.customControl.ListAlertDialog.ToggleClick
                public final void toggleClick(CompoundButton compoundButton, boolean z) {
                    this.f$0.m165xe627c947(i, compoundButton, z);
                }
            });
        }
        listAlertDialog.showDialog();
    }

    /* JADX INFO: renamed from: lambda$showCustomControlDialog$9$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m166xc342b24d(int i, String[] strArr, int i2) {
        this.fcEntry.usr[i][1].setData(i2);
        this.customControlButArr[i].setText(strArr[i2]);
        sendData(this.fcEntry.usr[i][1]);
    }

    /* JADX INFO: renamed from: lambda$showCustomControlDialog$10$com-cubeSuite-fragment-FootControl2-FC2Fragment, reason: not valid java name */
    /* synthetic */ void m165xe627c947(int i, CompoundButton compoundButton, boolean z) {
        this.fcEntry.usr[i][0].setData(z ? 1 : 0);
        sendData(this.fcEntry.usr[i][0]);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showTrsMidiSlect() {
        if (this.trsMidiWindow == null) {
            this.trsMidiWindow = new TrsMidiPopupWindow(getActivity(), communicationUtil);
        }
        this.trsMidiWindow.updateView(this.fcEntry);
        this.trsMidiWindow.showAtLocation(getView(), 80, 0, 0);
    }

    void initModeSelect() {
        this.modeSelect.setItemsVisibleCount(6);
        this.modeSelect.setTextSize(14.0f);
        this.modeSelect.setCyclic(false);
        this.modeSelect.setTextColorCenter(Color.rgb(255, 255, 255));
        this.modeSelect.setTextColorOut(Color.rgb(51, 234, 184));
        this.modeSelect.setDividerColor(Color.rgb(51, 234, 184));
        ArrayList arrayList = new ArrayList();
        arrayList.add(getString(R.string.programSwitchA));
        arrayList.add(getString(R.string.programSwitchB));
        arrayList.add(getString(R.string.customMode));
        arrayList.add(getString(R.string.advancedCustomMode1));
        arrayList.add(getString(R.string.manufacturerEquipmentControlMode));
        arrayList.add(getString(R.string.touchScreenMode));
        arrayList.add(getString(R.string.videoModel));
        arrayList.add(getString(R.string.keyboardModeA));
        arrayList.add(getString(R.string.keyboardModeB));
        arrayList.add(getString(R.string.multimediaKeyMode));
        arrayList.add(getString(R.string.customKeyboardMode));
        arrayList.add(getString(R.string.MixedKeyMode));
        arrayList.add(getString(R.string.SpeakerControlMode));
        this.modeSelect.setAdapter(new ArrayWheelAdapter(arrayList));
        this.modeSelect.setOnItemSelectedListener(new OnItemSelectedListener() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.14
            @Override // com.contrarywind.listener.OnItemSelectedListener
            public void onItemSelected(int i) {
                FC2Fragment.this.fcEntry.state.setData(i);
                FC2Fragment.this.showStateDetailView(i);
                FC2Fragment.this.updateMaxGroupCount();
                FC2Fragment fC2Fragment = FC2Fragment.this;
                fC2Fragment.sendData(fC2Fragment.fcEntry.state);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendData(AddrU8 addrU8) {
        communicationUtil.splitWriteData((byte) 4, new byte[]{addrU8.getByteData()}, addrU8.getAddr());
    }

    void showStateDetailView(int i) {
        if (i == 0) {
            this.pcdispContainer.setVisibility(0);
        } else {
            this.pcdispContainer.setVisibility(8);
        }
        if (i == 0 || i == 1) {
            this.maxGroupCountContainer31.setVisibility(0);
        } else {
            this.maxGroupCountContainer31.setVisibility(8);
        }
        if (i == 2) {
            this.customControlView.setVisibility(0);
        } else {
            this.customControlView.setVisibility(8);
        }
        if (i == 3) {
            this.advancedCustomMode1View.setVisibility(0);
        } else {
            this.advancedCustomMode1View.setVisibility(8);
        }
        if (i == 11) {
            this.mixKeyView.setVisibility(0);
        } else {
            this.mixKeyView.setVisibility(8);
        }
        if (i == 10) {
            this.customKeyboardView.setVisibility(0);
        } else {
            this.customKeyboardView.setVisibility(8);
        }
        this.modeExplainTv.setText(this.modelsTextExplain[i]);
    }

    void initView() {
        this.modeSelect.setCurrentItem(this.fcEntry.state.getData());
        showStateDetailView(this.fcEntry.state.getData());
        this.advMode1Page = this.fcEntry.usrpage.getData();
        this.advMixKeyPage = this.fcEntry.mixpage.getData();
        this.advancedCustomMode1Spinner.setSelection(this.fcEntry.usrpage.getData());
        this.mixKeyModeSpinner.setSelection(this.fcEntry.mixpage.getData());
        this.customKeyboardPage = this.fcEntry.hidpage.getData();
        this.customKeySpinner.setSelection(this.fcEntry.hidpage.getData());
        this.sendMidiChannel.setText(String.valueOf(this.fcEntry.ch.getData() + 1));
        String[] strArr = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR;
        for (int i = 0; i < 5; i++) {
            if (i == 4) {
                strArr = Global.BluetoothPedalCustomCommand.CUSTOM_COMMAND_STR_KNOB;
            }
            this.customControlButArr[i].setText(strArr[this.fcEntry.usr[i][1].getData()]);
        }
        updateMaxGroupCount();
        this.midiSwitch.setChecked(this.fcEntry.bankMidi.getData() != 0);
        this.pcdispSwitch.setChecked(this.fcEntry.pcdisp.getData() != 0);
    }

    void updateMaxGroupCount() {
        if (this.fcEntry.state.getData() == 0) {
            this.maxGroupCountSpinner31.setSelection(this.fcEntry.bankMax[0].getData());
        } else if (this.fcEntry.state.getData() == 1) {
            this.maxGroupCountSpinner31.setSelection(this.fcEntry.bankMax[1].getData());
        } else if (this.fcEntry.state.getData() == 3) {
            this.maxGroupCountSpinner15.setSelection(this.fcEntry.bankMax[2].getData());
        }
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public void initData() {
        CommunicationUtil communicationUtil = communicationUtil;
        Objects.requireNonNull(this.fcEntry);
        communicationUtil.splitReadData((byte) 4, 0L, 23646, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.fragment.FootControl2.FC2Fragment.15
            @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
            public void readDataCallback(boolean z, byte[] bArr) {
                if (z) {
                    FC2Fragment.this.fcEntry.setData(bArr);
                    FC2Fragment.this.initView();
                    FC2Fragment.this.handler.sendEmptyMessageDelayed(0, 100L);
                }
            }
        });
    }

    @Override // com.cubeSuite.fragment.ControlBaseFragment
    public BaseDeviceActivity getBaseActivity() {
        return (ControlAndPdfActivity) getActivity();
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
    }
}
