package com.cubeSuite.customControl;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CompoundButton;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.SpinnerAdapter;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingUtil;
import com.contrarywind.timer.MessageHandler;
import com.cubeSuite.R;
import com.cubeSuite.databinding.EqViewBinding;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.irbox.IRBoxEntry;

/* JADX INFO: loaded from: classes.dex */
public class EqView extends LinearLayout {
    int currentPeakIndex;
    private IRBoxEntry.IRBoxEq irBoxEq;
    boolean isNotTrigger;
    private EqViewBinding m_binding;
    private HzDbQEListen m_hzDbQListen;

    public interface HzDbQEListen {
        void dbChange(AddrI8 addrI8);

        void enChange(AddrU8 addrU8);

        void hzChange(AddrU16 addrU16);

        void qChange(AddrU8 addrU8);

        void resetEq();
    }

    public EqView(Context context) {
        super(context);
        this.currentPeakIndex = 0;
        this.isNotTrigger = false;
    }

    public EqView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.currentPeakIndex = 0;
        this.isNotTrigger = false;
        EqViewBinding eqViewBinding = (EqViewBinding) DataBindingUtil.inflate(LayoutInflater.from(context), R.layout.eq_view, this, true);
        this.m_binding = eqViewBinding;
        eqViewBinding.eqPic.setEqPicHzDbListener(new EQPic.EQPicHzDbListener() { // from class: com.cubeSuite.customControl.EqView.1
            @Override // com.cubeSuite.customControl.EQPic.EQPicHzDbListener
            public void endMove(int i, int i2, int i3) {
            }

            @Override // com.cubeSuite.customControl.EQPic.EQPicHzDbListener
            public void startMove(int i, int i2, int i3) {
                EqView.this.currentPeakIndex = i;
                EqView.this.m_binding.peakSpinner.setSelection(EqView.this.currentPeakIndex);
                EqView.this.m_binding.qContainer.setVisibility((EqView.this.currentPeakIndex <= 1 || EqView.this.currentPeakIndex >= 7) ? 8 : 0);
                EqView.this.m_binding.dbContainer.setVisibility((EqView.this.currentPeakIndex <= 0 || EqView.this.currentPeakIndex >= 8) ? 8 : 0);
                if (EqView.this.irBoxEq.enable[EqView.this.currentPeakIndex].getData() == 0) {
                    EqView.this.irBoxEq.enable[EqView.this.currentPeakIndex].setData(1);
                    EqView eqView = EqView.this;
                    eqView.sendEnableData(eqView.currentPeakIndex);
                    EqView.this.m_binding.enableSwitch.setChecked(true);
                }
            }

            @Override // com.cubeSuite.customControl.EQPic.EQPicHzDbListener
            public void move(int i, int i2, int i3) {
                EqView.this.currentPeakIndex = i;
                if (EqView.this.currentPeakIndex == 0) {
                    EqView.this.m_binding.hz.setProgress(EqView.this.m_binding.eqPic.getPxByHz(i2));
                    EqView.this.m_binding.hzText.setText(((int) Math.ceil(i2)) + "HZ");
                    return;
                }
                if (EqView.this.currentPeakIndex == 1) {
                    EqView.this.m_binding.hz.setProgress(EqView.this.m_binding.eqPic.getPxByHz(i2));
                    EqView.this.m_binding.hzText.setText(((int) Math.ceil(i2)) + "HZ");
                    EqView.this.m_binding.db.setProgress(i3);
                    EqView.this.m_binding.dbText.setText((((double) i3) / 10.0d) + "DB");
                    return;
                }
                if (EqView.this.currentPeakIndex == 7) {
                    EqView.this.m_binding.hz.setProgress(EqView.this.m_binding.eqPic.getPxByHz(i2));
                    EqView.this.m_binding.hzText.setText(((int) Math.ceil(i2)) + "HZ");
                    EqView.this.m_binding.db.setProgress(i3);
                    EqView.this.m_binding.dbText.setText((((double) i3) / 10.0d) + "DB");
                    return;
                }
                if (EqView.this.currentPeakIndex == 8) {
                    EqView.this.m_binding.hz.setProgress(EqView.this.m_binding.eqPic.getPxByHz(i2));
                    EqView.this.m_binding.hzText.setText(((int) Math.ceil(i2)) + "HZ");
                } else {
                    EqView.this.m_binding.hz.setProgress(EqView.this.m_binding.eqPic.getPxByHz(i2));
                    EqView.this.m_binding.hzText.setText(((int) Math.ceil(i2)) + "HZ");
                    EqView.this.m_binding.db.setProgress(i3);
                    EqView.this.m_binding.dbText.setText((((double) i3) / 10.0d) + "DB");
                }
            }
        });
        init();
    }

    public void init() {
        this.m_binding.resetBut.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.resetEq();
            }
        });
        this.m_binding.peakSpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.spinner_item, R.id.tvItem, new String[]{"HP", "LS", "P1", "P2", "P3", "P4", "P5", "HS", "LP"}));
        this.m_binding.peakSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.EqView.3
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long j) {
                if (EqView.this.irBoxEq == null) {
                    return;
                }
                EqView.this.currentPeakIndex = i;
                EqView.this.updateHzDbQ();
            }
        });
        this.m_binding.hzMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.4
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusHz(0);
            }
        });
        this.m_binding.hzAdd.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.5
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusHz(1);
            }
        });
        this.m_binding.hz.setMin(this.m_binding.eqPic.getPxByHz(30));
        this.m_binding.hz.setMax(this.m_binding.eqPic.getPxByHz(18000));
        this.m_binding.hz.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.customControl.EqView.6
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (!EqView.this.isNotTrigger) {
                    int iRound = (int) Math.round(EqView.this.m_binding.eqPic.getHzByPx(i));
                    EqView.this.m_binding.hzText.setText(String.valueOf(iRound) + "HZ");
                    if (EqView.this.currentPeakIndex == 0) {
                        EqView.this.irBoxEq.hpHz.setData(iRound);
                    } else if (EqView.this.currentPeakIndex == 1) {
                        EqView.this.irBoxEq.lsHz.setData(iRound);
                    } else if (EqView.this.currentPeakIndex == 7) {
                        EqView.this.irBoxEq.hsHz.setData(iRound);
                    } else if (EqView.this.currentPeakIndex == 8) {
                        EqView.this.irBoxEq.lpHz.setData(iRound);
                    } else {
                        EqView.this.irBoxEq.peakHz[EqView.this.currentPeakIndex - 2].setData(iRound);
                    }
                    EqView.this.m_binding.eqPic.updateView();
                    return;
                }
                EqView.this.isNotTrigger = false;
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
                EqView eqView = EqView.this;
                eqView.sendHzData(eqView.currentPeakIndex);
            }
        });
        this.m_binding.dbMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.7
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusDb(0);
            }
        });
        this.m_binding.dbAdd.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.8
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusDb(1);
            }
        });
        this.m_binding.db.setMin(-120);
        this.m_binding.db.setMax(120);
        this.m_binding.db.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.customControl.EqView.9
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                byte b = (byte) i;
                if (EqView.this.currentPeakIndex == 1) {
                    EqView.this.irBoxEq.lsDb.setData(b);
                } else if (EqView.this.currentPeakIndex == 7) {
                    EqView.this.irBoxEq.hsDb.setData(b);
                } else if (EqView.this.currentPeakIndex > 1 && EqView.this.currentPeakIndex < 7) {
                    EqView.this.irBoxEq.peakDb[EqView.this.currentPeakIndex - 2].setData(b);
                }
                EqView.this.m_binding.dbText.setText((((double) i) / 10.0d) + "DB");
                EqView.this.m_binding.eqPic.updateView();
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
                EqView eqView = EqView.this;
                eqView.sendDbData(eqView.currentPeakIndex);
            }
        });
        this.m_binding.qMinus.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.10
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusQ(0);
            }
        });
        this.m_binding.qAdd.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.EqView.11
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                EqView.this.addOrMinusQ(1);
            }
        });
        this.m_binding.q.setMin(1);
        this.m_binding.q.setMax(160);
        this.m_binding.q.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: com.cubeSuite.customControl.EqView.12
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                if (EqView.this.currentPeakIndex > 1 && EqView.this.currentPeakIndex < 7) {
                    EqView.this.irBoxEq.peakQ[EqView.this.currentPeakIndex - 2].setData(i);
                }
                EqView.this.m_binding.qText.setText(i + "Q");
                EqView.this.m_binding.eqPic.updateView();
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
                EqView eqView = EqView.this;
                eqView.sendQData(eqView.currentPeakIndex);
            }
        });
        this.m_binding.enableSwitch.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: com.cubeSuite.customControl.EqView.13
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                EqView.this.irBoxEq.enable[EqView.this.currentPeakIndex].setData(z ? 1 : 0);
                EqView.this.m_binding.eqPic.updateView();
                EqView eqView = EqView.this;
                eqView.sendEnableData(eqView.currentPeakIndex);
            }
        });
    }

    void addOrMinusHz(int i) {
        int data;
        int i2;
        this.isNotTrigger = true;
        int i3 = this.currentPeakIndex;
        if (i3 == 0) {
            data = this.irBoxEq.hpHz.getData();
        } else if (i3 == 1) {
            data = this.irBoxEq.lsHz.getData();
        } else if (i3 == 7) {
            data = this.irBoxEq.hsHz.getData();
        } else if (i3 == 8) {
            data = this.irBoxEq.lpHz.getData();
        } else {
            data = this.irBoxEq.peakHz[this.currentPeakIndex - 2].getData();
        }
        if (i == 0) {
            if (data == 30) {
                return;
            } else {
                i2 = data - 1;
            }
        } else if (data == 18000) {
            return;
        } else {
            i2 = data + 1;
        }
        int i4 = this.currentPeakIndex;
        if (i4 == 0) {
            this.irBoxEq.hpHz.setData(i2);
        } else if (i4 == 1) {
            this.irBoxEq.lsHz.setData(i2);
        } else if (i4 == 7) {
            this.irBoxEq.hsHz.setData(i2);
        } else if (i4 == 8) {
            this.irBoxEq.lpHz.setData(i2);
        } else {
            this.irBoxEq.peakHz[this.currentPeakIndex - 2].setData(i2);
        }
        this.m_binding.hz.setProgress(this.m_binding.eqPic.getPxByHz(i2));
        this.m_binding.hzText.setText(String.valueOf(i2) + "HZ");
        this.m_binding.eqPic.updateView();
        sendHzData(this.currentPeakIndex);
    }

    /* JADX WARN: Code duplicated, block: B:10:0x0021  */
    void addOrMinusDb(int i) {
        byte data;
        int i2;
        int i3 = this.currentPeakIndex;
        if (i3 == 0) {
            data = 0;
        } else if (i3 == 1) {
            data = this.irBoxEq.lsDb.getData();
        } else if (i3 == 7) {
            data = this.irBoxEq.hsDb.getData();
        } else if (i3 == 8) {
            data = 0;
        } else {
            data = this.irBoxEq.peakDb[this.currentPeakIndex - 2].getData();
        }
        if (i == 0) {
            if (data == -120) {
                return;
            } else {
                i2 = data - 1;
            }
        } else if (data == 120) {
            return;
        } else {
            i2 = data + 1;
        }
        int i4 = this.currentPeakIndex;
        if (i4 != 0) {
            if (i4 == 1) {
                this.irBoxEq.lsDb.setData((byte) i2);
            } else if (i4 == 7) {
                this.irBoxEq.hsDb.setData((byte) i2);
            } else if (i4 != 8) {
                this.irBoxEq.peakDb[this.currentPeakIndex - 2].setData((byte) i2);
            }
        }
        this.m_binding.dbText.setText((((double) i2) / 10.0d) + "DB");
        this.m_binding.eqPic.updateView();
        this.m_binding.db.setProgress(i2);
        sendDbData(this.currentPeakIndex);
    }

    void addOrMinusQ(int i) {
        int i2;
        int data = this.irBoxEq.peakQ[this.currentPeakIndex - 2].getData();
        if (i == 0) {
            if (data == 1) {
                return;
            } else {
                i2 = data - 1;
            }
        } else if (data == 160) {
            return;
        } else {
            i2 = data + 1;
        }
        this.irBoxEq.peakQ[this.currentPeakIndex - 2].setData(i2);
        this.m_binding.qText.setText(i2 + "Q");
        this.m_binding.eqPic.updateView();
        this.m_binding.q.setProgress(i2);
        sendQData(this.currentPeakIndex);
    }

    void resetEq() {
        for (int i = 0; i < 9; i++) {
            this.irBoxEq.enable[i].setData(0);
        }
        this.irBoxEq.hpHz.setData(30);
        this.irBoxEq.lsHz.setData(80);
        this.irBoxEq.lsDb.setData((byte) 0);
        this.irBoxEq.peakHz[0].setData(100);
        this.irBoxEq.peakDb[0].setData((byte) 0);
        this.irBoxEq.peakQ[0].setData(5);
        this.irBoxEq.peakHz[1].setData(400);
        this.irBoxEq.peakDb[1].setData((byte) 0);
        this.irBoxEq.peakQ[1].setData(5);
        this.irBoxEq.peakHz[2].setData(1000);
        this.irBoxEq.peakDb[2].setData((byte) 0);
        this.irBoxEq.peakQ[2].setData(5);
        this.irBoxEq.peakHz[3].setData(MessageHandler.WHAT_SMOOTH_SCROLL);
        this.irBoxEq.peakDb[3].setData((byte) 0);
        this.irBoxEq.peakQ[3].setData(5);
        this.irBoxEq.peakHz[4].setData(4000);
        this.irBoxEq.peakDb[4].setData((byte) 0);
        this.irBoxEq.peakQ[4].setData(5);
        this.irBoxEq.hsHz.setData(12000);
        this.irBoxEq.hsDb.setData((byte) 0);
        this.irBoxEq.lpHz.setData(18000);
        updateHzDbQ();
        this.m_hzDbQListen.resetEq();
    }

    public void sendHzData(int i) {
        if (i == 0) {
            this.m_hzDbQListen.hzChange(this.irBoxEq.hpHz);
            return;
        }
        if (i == 1) {
            this.m_hzDbQListen.hzChange(this.irBoxEq.lsHz);
            return;
        }
        if (i == 7) {
            this.m_hzDbQListen.hzChange(this.irBoxEq.hsHz);
        } else if (i == 8) {
            this.m_hzDbQListen.hzChange(this.irBoxEq.lpHz);
        } else {
            this.m_hzDbQListen.hzChange(this.irBoxEq.peakHz[i - 2]);
        }
    }

    public void sendDbData(int i) {
        if (i == 1) {
            this.m_hzDbQListen.dbChange(this.irBoxEq.lsDb);
            return;
        }
        if (i == 7) {
            this.m_hzDbQListen.dbChange(this.irBoxEq.hsDb);
        } else {
            if (i <= 1 || i >= 7) {
                return;
            }
            this.m_hzDbQListen.dbChange(this.irBoxEq.peakDb[i - 2]);
        }
    }

    public void sendQData(int i) {
        int i2 = this.currentPeakIndex;
        if (i2 <= 1 || i2 >= 7) {
            return;
        }
        this.m_hzDbQListen.qChange(this.irBoxEq.peakQ[i - 2]);
    }

    public void sendEnableData(int i) {
        this.m_hzDbQListen.enChange(this.irBoxEq.enable[i]);
    }

    public void hzChange(int i, int i2) {
        this.currentPeakIndex = i;
        this.m_binding.peakSpinner.setSelection(this.currentPeakIndex);
        this.m_binding.hz.setProgress(this.m_binding.eqPic.getPxByHz(i2));
        this.m_binding.hzText.setText(this.m_binding.eqPic.getPxByHz(i2) + "HZ");
    }

    public void dbChange(int i, int i2) {
        this.currentPeakIndex = i;
        this.m_binding.peakSpinner.setSelection(this.currentPeakIndex);
        this.m_binding.db.setProgress(i2);
    }

    public void setHzDbQListen(HzDbQEListen hzDbQEListen) {
        this.m_hzDbQListen = hzDbQEListen;
    }

    public void setEqData(IRBoxEntry.IRBoxEq iRBoxEq) {
        this.irBoxEq = iRBoxEq;
        this.m_binding.eqPic.setData(iRBoxEq);
        updateHzDbQ();
    }

    /* JADX WARN: Code duplicated, block: B:16:0x00ab  */
    /* JADX WARN: Code duplicated, block: B:17:0x00ad  */
    /* JADX WARN: Code duplicated, block: B:20:0x00b5 A[ADDED_TO_REGION] */
    /* JADX WARN: Code duplicated, block: B:22:0x00b9  */
    /* JADX WARN: Code duplicated, block: B:25:0x00e6 A[ADDED_TO_REGION] */
    void updateHzDbQ() {
        int data;
        byte data2;
        int data3;
        boolean z;
        int i;
        int i2;
        int i3;
        int i4 = this.currentPeakIndex;
        int i5 = 8;
        if (i4 == 0) {
            data = this.irBoxEq.hpHz.getData();
        } else {
            if (i4 == 1) {
                data = this.irBoxEq.lsHz.getData();
                data2 = this.irBoxEq.lsDb.getData();
            } else {
                if (i4 == 7) {
                    data = this.irBoxEq.hsHz.getData();
                    data2 = this.irBoxEq.hsDb.getData();
                } else if (i4 == 8) {
                    data = this.irBoxEq.lpHz.getData();
                } else {
                    data = this.irBoxEq.peakHz[this.currentPeakIndex - 2].getData();
                    data2 = this.irBoxEq.peakDb[this.currentPeakIndex - 2].getData();
                    data3 = this.irBoxEq.peakQ[this.currentPeakIndex - 2].getData();
                }
                this.m_binding.hz.setProgress(this.m_binding.eqPic.getPxByHz(data));
                this.m_binding.hzText.setText(data + "HZ");
                SwitchCompat switchCompat = this.m_binding.enableSwitch;
                if (this.irBoxEq.enable[this.currentPeakIndex].getData() == 1) {
                    z = true;
                } else {
                    z = false;
                }
                switchCompat.setChecked(z);
                i = this.currentPeakIndex;
                if (i > 1 || i >= 7) {
                    i2 = 8;
                } else {
                    i2 = 0;
                }
                this.m_binding.q.setProgress(data3);
                this.m_binding.qText.setText(data3 + "Q");
                this.m_binding.qContainer.setVisibility(i2);
                i3 = this.currentPeakIndex;
                if (i3 > 0 && i3 < 8) {
                    i5 = 0;
                }
                this.m_binding.db.setProgress(data2);
                this.m_binding.dbContainer.setVisibility(i5);
                this.m_binding.dbText.setText((((double) data2) / 10.0d) + "DB");
            }
            data3 = 0;
            this.m_binding.hz.setProgress(this.m_binding.eqPic.getPxByHz(data));
            this.m_binding.hzText.setText(data + "HZ");
            SwitchCompat switchCompat2 = this.m_binding.enableSwitch;
            if (this.irBoxEq.enable[this.currentPeakIndex].getData() == 1) {
                z = true;
            } else {
                z = false;
            }
            switchCompat2.setChecked(z);
            i = this.currentPeakIndex;
            if (i > 1) {
                i2 = 8;
            } else {
                i2 = 8;
            }
            this.m_binding.q.setProgress(data3);
            this.m_binding.qText.setText(data3 + "Q");
            this.m_binding.qContainer.setVisibility(i2);
            i3 = this.currentPeakIndex;
            if (i3 > 0) {
                i5 = 0;
            }
            this.m_binding.db.setProgress(data2);
            this.m_binding.dbContainer.setVisibility(i5);
            this.m_binding.dbText.setText((((double) data2) / 10.0d) + "DB");
        }
        data2 = 0;
        data3 = 0;
        this.m_binding.hz.setProgress(this.m_binding.eqPic.getPxByHz(data));
        this.m_binding.hzText.setText(data + "HZ");
        SwitchCompat switchCompat3 = this.m_binding.enableSwitch;
        if (this.irBoxEq.enable[this.currentPeakIndex].getData() == 1) {
            z = true;
        } else {
            z = false;
        }
        switchCompat3.setChecked(z);
        i = this.currentPeakIndex;
        if (i > 1) {
            i2 = 8;
        } else {
            i2 = 8;
        }
        this.m_binding.q.setProgress(data3);
        this.m_binding.qText.setText(data3 + "Q");
        this.m_binding.qContainer.setVisibility(i2);
        i3 = this.currentPeakIndex;
        if (i3 > 0) {
            i5 = 0;
        }
        this.m_binding.db.setProgress(data2);
        this.m_binding.dbContainer.setVisibility(i5);
        this.m_binding.dbText.setText((((double) data2) / 10.0d) + "DB");
    }
}
