package com.cubeSuite.databinding;

import android.text.Editable;
import android.util.SparseIntArray;
import android.view.View;
import android.widget.AdapterView;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.LinearLayout;
import androidx.appcompat.widget.AppCompatSpinner;
import androidx.appcompat.widget.SwitchCompat;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import androidx.databinding.adapters.AdapterViewBindingAdapter;
import androidx.databinding.adapters.CompoundButtonBindingAdapter;
import androidx.databinding.adapters.TextViewBindingAdapter;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Arp;
import com.cubeSuite.fragment.smk25Mini.Smk25MiniArpFragment;
import com.cubeSuite.generated.callback.AfterTextChanged;
import com.cubeSuite.generated.callback.OnCheckedChangeListener1;
import com.cubeSuite.generated.callback.OnItemSelected;
import com.google.android.material.card.MaterialCardViewHelper;
import okhttp3.internal.http2.Http2Stream;
import okio.SegmentPool;

/* JADX INFO: loaded from: classes.dex */
public class Smk25miniArpFragmentBindingImpl extends Smk25miniArpFragmentBinding implements OnItemSelected.Listener, OnCheckedChangeListener1.Listener, AfterTextChanged.Listener {
    private static final ViewDataBinding.IncludedLayouts sIncludes = null;
    private static final SparseIntArray sViewsWithIds = null;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback38;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback39;
    private final TextViewBindingAdapter.AfterTextChanged mCallback40;
    private final TextViewBindingAdapter.AfterTextChanged mCallback41;
    private final CompoundButton.OnCheckedChangeListener mCallback42;
    private final AdapterViewBindingAdapter.OnItemSelected mCallback43;
    private final CompoundButton.OnCheckedChangeListener mCallback44;
    private final TextViewBindingAdapter.AfterTextChanged mCallback45;
    private long mDirtyFlags;
    private final LinearLayout mboundView0;

    public Smk25miniArpFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view) {
        this(dataBindingComponent, view, mapBindings(dataBindingComponent, view, 9, sIncludes, sViewsWithIds));
    }

    private Smk25miniArpFragmentBindingImpl(DataBindingComponent dataBindingComponent, View view, Object[] objArr) {
        super(dataBindingComponent, view, 8, (EditText) objArr[3], (SwitchCompat) objArr[5], (AppCompatSpinner) objArr[1], (AppCompatSpinner) objArr[2], (EditText) objArr[4], (SwitchCompat) objArr[7], (EditText) objArr[8], (AppCompatSpinner) objArr[6]);
        this.mDirtyFlags = -1L;
        this.gate.setTag(null);
        this.latch.setTag(null);
        LinearLayout linearLayout = (LinearLayout) objArr[0];
        this.mboundView0 = linearLayout;
        linearLayout.setTag(null);
        this.mode.setTag(null);
        this.octive.setTag(null);
        this.swing.setTag(null);
        this.sync.setTag(null);
        this.tempo.setTag(null);
        this.time.setTag(null);
        setRootTag(view);
        this.mCallback39 = new OnItemSelected(this, 2);
        this.mCallback43 = new OnItemSelected(this, 6);
        this.mCallback42 = new OnCheckedChangeListener1(this, 5);
        this.mCallback44 = new OnCheckedChangeListener1(this, 7);
        this.mCallback45 = new AfterTextChanged(this, 8);
        this.mCallback40 = new AfterTextChanged(this, 3);
        this.mCallback38 = new OnItemSelected(this, 1);
        this.mCallback41 = new AfterTextChanged(this, 4);
        invalidateAll();
    }

    @Override // androidx.databinding.ViewDataBinding
    public void invalidateAll() {
        synchronized (this) {
            this.mDirtyFlags = 131072L;
        }
        requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean hasPendingBindings() {
        synchronized (this) {
            return this.mDirtyFlags != 0;
        }
    }

    @Override // androidx.databinding.ViewDataBinding
    public boolean setVariable(int i, Object obj) {
        if (9 == i) {
            setListener((Smk25MiniArpFragment) obj);
            return true;
        }
        if (2 != i) {
            return false;
        }
        setData((Arp) obj);
        return true;
    }

    @Override // com.cubeSuite.databinding.Smk25miniArpFragmentBinding
    public void setListener(Smk25MiniArpFragment smk25MiniArpFragment) {
        this.mListener = smk25MiniArpFragment;
        synchronized (this) {
            this.mDirtyFlags |= 256;
        }
        notifyPropertyChanged(9);
        super.requestRebind();
    }

    @Override // com.cubeSuite.databinding.Smk25miniArpFragmentBinding
    public void setData(Arp arp) {
        this.mData = arp;
        synchronized (this) {
            this.mDirtyFlags |= 512;
        }
        notifyPropertyChanged(2);
        super.requestRebind();
    }

    @Override // androidx.databinding.ViewDataBinding
    protected boolean onFieldChange(int i, Object obj, int i2) {
        switch (i) {
            case 0:
                return onChangeDataSwing((AddrU8) obj, i2);
            case 1:
                return onChangeDataTime((AddrU8) obj, i2);
            case 2:
                return onChangeDataLatch((AddrU8) obj, i2);
            case 3:
                return onChangeDataTempo((AddrU16) obj, i2);
            case 4:
                return onChangeDataMode((AddrU8) obj, i2);
            case 5:
                return onChangeDataGate((AddrU8) obj, i2);
            case 6:
                return onChangeDataOct((AddrU8) obj, i2);
            case 7:
                return onChangeDataSync((AddrU8) obj, i2);
            default:
                return false;
        }
    }

    private boolean onChangeDataSwing(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 1;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 1024;
        }
        return true;
    }

    private boolean onChangeDataTime(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 2;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 2048;
        }
        return true;
    }

    private boolean onChangeDataLatch(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 4;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 4096;
        }
        return true;
    }

    private boolean onChangeDataTempo(AddrU16 addrU16, int i) {
        if (i != 0) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 8;
        }
        return true;
    }

    private boolean onChangeDataMode(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 16;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 8192;
        }
        return true;
    }

    private boolean onChangeDataGate(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 32;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= Http2Stream.EMIT_BUFFER_SIZE;
        }
        return true;
    }

    private boolean onChangeDataOct(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 64;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= 32768;
        }
        return true;
    }

    private boolean onChangeDataSync(AddrU8 addrU8, int i) {
        if (i == 0) {
            synchronized (this) {
                this.mDirtyFlags |= 128;
            }
            return true;
        }
        if (i != 2) {
            return false;
        }
        synchronized (this) {
            this.mDirtyFlags |= SegmentPool.MAX_SIZE;
        }
        return true;
    }

    /* JADX WARN: Code duplicated, block: B:31:0x0073 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:32:0x0075  */
    /* JADX WARN: Code duplicated, block: B:33:0x007a  */
    /* JADX WARN: Code duplicated, block: B:36:0x0084  */
    /* JADX WARN: Code duplicated, block: B:37:0x0089  */
    /* JADX WARN: Code duplicated, block: B:39:0x008c  */
    /* JADX WARN: Code duplicated, block: B:40:0x008e  */
    /* JADX WARN: Code duplicated, block: B:44:0x0098 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:45:0x009a  */
    /* JADX WARN: Code duplicated, block: B:46:0x009f  */
    /* JADX WARN: Code duplicated, block: B:49:0x00a6  */
    /* JADX WARN: Code duplicated, block: B:50:0x00ab  */
    /* JADX WARN: Code duplicated, block: B:52:0x00b1  */
    /* JADX WARN: Code duplicated, block: B:55:0x00b8 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:56:0x00ba  */
    /* JADX WARN: Code duplicated, block: B:57:0x00bf  */
    /* JADX WARN: Code duplicated, block: B:60:0x00c8  */
    /* JADX WARN: Code duplicated, block: B:61:0x00cd  */
    /* JADX WARN: Code duplicated, block: B:65:0x00d6 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:66:0x00d8  */
    /* JADX WARN: Code duplicated, block: B:67:0x00dd  */
    /* JADX WARN: Code duplicated, block: B:70:0x00e4  */
    /* JADX WARN: Code duplicated, block: B:71:0x00e9  */
    /* JADX WARN: Code duplicated, block: B:73:0x00ef  */
    /* JADX WARN: Code duplicated, block: B:76:0x00f6 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:77:0x00f8  */
    /* JADX WARN: Code duplicated, block: B:78:0x00fd  */
    /* JADX WARN: Code duplicated, block: B:81:0x0106  */
    /* JADX WARN: Code duplicated, block: B:82:0x010b  */
    /* JADX WARN: Code duplicated, block: B:86:0x0114 A[DONT_INVERT] */
    /* JADX WARN: Code duplicated, block: B:87:0x0116  */
    /* JADX WARN: Code duplicated, block: B:88:0x011b  */
    /* JADX WARN: Code duplicated, block: B:91:0x0122  */
    /* JADX WARN: Code duplicated, block: B:92:0x0127  */
    /* JADX WARN: Code duplicated, block: B:95:0x012b  */
    /* JADX WARN: Code duplicated, block: B:97:0x012f  */
    @Override // androidx.databinding.ViewDataBinding
    protected void executeBindings() {
        long j;
        long j2;
        long j3;
        long j4;
        long j5;
        String strValueOf;
        String strValueOf2;
        String strValueOf3;
        boolean z;
        boolean z2;
        int data;
        int data2;
        int data3;
        boolean z3;
        AddrU8 sync;
        int data4;
        AddrU8 oct;
        AddrU8 gate;
        int data5;
        AddrU8 mode;
        AddrU16 tempo;
        int data6;
        AddrU8 latch;
        int data7;
        synchronized (this) {
            j = this.mDirtyFlags;
            this.mDirtyFlags = 0L;
        }
        Smk25MiniArpFragment smk25MiniArpFragment = this.mListener;
        Arp arp = this.mData;
        if ((261887 & j) != 0) {
            if ((j & 132609) != 0) {
                AddrU8 swing = arp != null ? arp.getSwing() : null;
                updateRegistration(0, swing);
                strValueOf = String.valueOf(swing != null ? swing.getData() : 0);
            } else {
                strValueOf = null;
            }
            if ((j & 133634) != 0) {
                AddrU8 time = arp != null ? arp.getTime() : null;
                j4 = 197248;
                updateRegistration(1, time);
                data = time != null ? time.getData() : 0;
                if ((j & 135684) != 0) {
                    if (arp != null) {
                        latch = arp.getLatch();
                    } else {
                        latch = null;
                    }
                    j5 = 164416;
                    updateRegistration(2, latch);
                    if (latch != null) {
                        data7 = latch.getData();
                    } else {
                        data7 = 0;
                    }
                    z3 = data7 == 1;
                    if ((j & 131592) != 0) {
                        if (arp != null) {
                            tempo = arp.getTempo();
                        } else {
                            tempo = null;
                        }
                        updateRegistration(3, tempo);
                        if (tempo != null) {
                            data6 = tempo.getData();
                        } else {
                            data6 = 0;
                        }
                        strValueOf2 = String.valueOf(data6);
                    } else {
                        strValueOf2 = null;
                    }
                    if ((j & 139792) != 0) {
                        if (arp != null) {
                            mode = arp.getMode();
                        } else {
                            mode = null;
                        }
                        j2 = 131592;
                        updateRegistration(4, mode);
                        data2 = mode != null ? mode.getData() : 0;
                        if ((j & 148000) != 0) {
                            if (arp != null) {
                                gate = arp.getGate();
                            } else {
                                gate = null;
                            }
                            updateRegistration(5, gate);
                            if (gate != null) {
                                data5 = gate.getData();
                            } else {
                                data5 = 0;
                            }
                            strValueOf3 = String.valueOf(data5);
                        } else {
                            strValueOf3 = null;
                        }
                        if ((j & j5) != 0) {
                            if (arp != null) {
                                oct = arp.getOct();
                            } else {
                                oct = null;
                            }
                            j3 = 139792;
                            updateRegistration(6, oct);
                            data3 = oct != null ? oct.getData() : 0;
                            if ((j & j4) != 0) {
                                if (arp != null) {
                                    sync = arp.getSync();
                                } else {
                                    sync = null;
                                }
                                updateRegistration(7, sync);
                                if (sync != null) {
                                    data4 = sync.getData();
                                } else {
                                    data4 = 0;
                                }
                                z = data4 == 1;
                                z2 = z3;
                            } else {
                                z2 = z3;
                                z = false;
                            }
                        } else {
                            j3 = 139792;
                        }
                        if ((j & j4) != 0) {
                            if (arp != null) {
                                sync = arp.getSync();
                            } else {
                                sync = null;
                            }
                            updateRegistration(7, sync);
                            if (sync != null) {
                                data4 = sync.getData();
                            } else {
                                data4 = 0;
                            }
                            z = data4 == 1;
                            z2 = z3;
                        } else {
                            z2 = z3;
                            z = false;
                        }
                    } else {
                        j2 = 131592;
                    }
                    if ((j & 148000) != 0) {
                        if (arp != null) {
                            gate = arp.getGate();
                        } else {
                            gate = null;
                        }
                        updateRegistration(5, gate);
                        if (gate != null) {
                            data5 = gate.getData();
                        } else {
                            data5 = 0;
                        }
                        strValueOf3 = String.valueOf(data5);
                    } else {
                        strValueOf3 = null;
                    }
                    if ((j & j5) != 0) {
                        if (arp != null) {
                            oct = arp.getOct();
                        } else {
                            oct = null;
                        }
                        j3 = 139792;
                        updateRegistration(6, oct);
                        if (oct != null) {
                        }
                        if ((j & j4) != 0) {
                            if (arp != null) {
                                sync = arp.getSync();
                            } else {
                                sync = null;
                            }
                            updateRegistration(7, sync);
                            if (sync != null) {
                                data4 = sync.getData();
                            } else {
                                data4 = 0;
                            }
                            z = data4 == 1;
                            z2 = z3;
                        } else {
                            z2 = z3;
                            z = false;
                        }
                    } else {
                        j3 = 139792;
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j5 = 164416;
                }
                if ((j & 131592) != 0) {
                    if (arp != null) {
                        tempo = arp.getTempo();
                    } else {
                        tempo = null;
                    }
                    updateRegistration(3, tempo);
                    if (tempo != null) {
                        data6 = tempo.getData();
                    } else {
                        data6 = 0;
                    }
                    strValueOf2 = String.valueOf(data6);
                } else {
                    strValueOf2 = null;
                }
                if ((j & 139792) != 0) {
                    if (arp != null) {
                        mode = arp.getMode();
                    } else {
                        mode = null;
                    }
                    j2 = 131592;
                    updateRegistration(4, mode);
                    if (mode != null) {
                    }
                    if ((j & 148000) != 0) {
                        if (arp != null) {
                            gate = arp.getGate();
                        } else {
                            gate = null;
                        }
                        updateRegistration(5, gate);
                        if (gate != null) {
                            data5 = gate.getData();
                        } else {
                            data5 = 0;
                        }
                        strValueOf3 = String.valueOf(data5);
                    } else {
                        strValueOf3 = null;
                    }
                    if ((j & j5) != 0) {
                        if (arp != null) {
                            oct = arp.getOct();
                        } else {
                            oct = null;
                        }
                        j3 = 139792;
                        updateRegistration(6, oct);
                        if (oct != null) {
                        }
                        if ((j & j4) != 0) {
                            if (arp != null) {
                                sync = arp.getSync();
                            } else {
                                sync = null;
                            }
                            updateRegistration(7, sync);
                            if (sync != null) {
                                data4 = sync.getData();
                            } else {
                                data4 = 0;
                            }
                            z = data4 == 1;
                            z2 = z3;
                        } else {
                            z2 = z3;
                            z = false;
                        }
                    } else {
                        j3 = 139792;
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j2 = 131592;
                }
                if ((j & 148000) != 0) {
                    if (arp != null) {
                        gate = arp.getGate();
                    } else {
                        gate = null;
                    }
                    updateRegistration(5, gate);
                    if (gate != null) {
                        data5 = gate.getData();
                    } else {
                        data5 = 0;
                    }
                    strValueOf3 = String.valueOf(data5);
                } else {
                    strValueOf3 = null;
                }
                if ((j & j5) != 0) {
                    if (arp != null) {
                        oct = arp.getOct();
                    } else {
                        oct = null;
                    }
                    j3 = 139792;
                    updateRegistration(6, oct);
                    if (oct != null) {
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j3 = 139792;
                }
                if ((j & j4) != 0) {
                    if (arp != null) {
                        sync = arp.getSync();
                    } else {
                        sync = null;
                    }
                    updateRegistration(7, sync);
                    if (sync != null) {
                        data4 = sync.getData();
                    } else {
                        data4 = 0;
                    }
                    z = data4 == 1;
                    z2 = z3;
                } else {
                    z2 = z3;
                    z = false;
                }
            } else {
                j4 = 197248;
            }
            if ((j & 135684) != 0) {
                if (arp != null) {
                    latch = arp.getLatch();
                } else {
                    latch = null;
                }
                j5 = 164416;
                updateRegistration(2, latch);
                if (latch != null) {
                    data7 = latch.getData();
                } else {
                    data7 = 0;
                }
                if (data7 == 1) {
                }
                if ((j & 131592) != 0) {
                    if (arp != null) {
                        tempo = arp.getTempo();
                    } else {
                        tempo = null;
                    }
                    updateRegistration(3, tempo);
                    if (tempo != null) {
                        data6 = tempo.getData();
                    } else {
                        data6 = 0;
                    }
                    strValueOf2 = String.valueOf(data6);
                } else {
                    strValueOf2 = null;
                }
                if ((j & 139792) != 0) {
                    if (arp != null) {
                        mode = arp.getMode();
                    } else {
                        mode = null;
                    }
                    j2 = 131592;
                    updateRegistration(4, mode);
                    if (mode != null) {
                    }
                    if ((j & 148000) != 0) {
                        if (arp != null) {
                            gate = arp.getGate();
                        } else {
                            gate = null;
                        }
                        updateRegistration(5, gate);
                        if (gate != null) {
                            data5 = gate.getData();
                        } else {
                            data5 = 0;
                        }
                        strValueOf3 = String.valueOf(data5);
                    } else {
                        strValueOf3 = null;
                    }
                    if ((j & j5) != 0) {
                        if (arp != null) {
                            oct = arp.getOct();
                        } else {
                            oct = null;
                        }
                        j3 = 139792;
                        updateRegistration(6, oct);
                        if (oct != null) {
                        }
                        if ((j & j4) != 0) {
                            if (arp != null) {
                                sync = arp.getSync();
                            } else {
                                sync = null;
                            }
                            updateRegistration(7, sync);
                            if (sync != null) {
                                data4 = sync.getData();
                            } else {
                                data4 = 0;
                            }
                            z = data4 == 1;
                            z2 = z3;
                        } else {
                            z2 = z3;
                            z = false;
                        }
                    } else {
                        j3 = 139792;
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j2 = 131592;
                }
                if ((j & 148000) != 0) {
                    if (arp != null) {
                        gate = arp.getGate();
                    } else {
                        gate = null;
                    }
                    updateRegistration(5, gate);
                    if (gate != null) {
                        data5 = gate.getData();
                    } else {
                        data5 = 0;
                    }
                    strValueOf3 = String.valueOf(data5);
                } else {
                    strValueOf3 = null;
                }
                if ((j & j5) != 0) {
                    if (arp != null) {
                        oct = arp.getOct();
                    } else {
                        oct = null;
                    }
                    j3 = 139792;
                    updateRegistration(6, oct);
                    if (oct != null) {
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j3 = 139792;
                }
                if ((j & j4) != 0) {
                    if (arp != null) {
                        sync = arp.getSync();
                    } else {
                        sync = null;
                    }
                    updateRegistration(7, sync);
                    if (sync != null) {
                        data4 = sync.getData();
                    } else {
                        data4 = 0;
                    }
                    z = data4 == 1;
                    z2 = z3;
                } else {
                    z2 = z3;
                    z = false;
                }
            } else {
                j5 = 164416;
            }
            if ((j & 131592) != 0) {
                if (arp != null) {
                    tempo = arp.getTempo();
                } else {
                    tempo = null;
                }
                updateRegistration(3, tempo);
                if (tempo != null) {
                    data6 = tempo.getData();
                } else {
                    data6 = 0;
                }
                strValueOf2 = String.valueOf(data6);
            } else {
                strValueOf2 = null;
            }
            if ((j & 139792) != 0) {
                if (arp != null) {
                    mode = arp.getMode();
                } else {
                    mode = null;
                }
                j2 = 131592;
                updateRegistration(4, mode);
                if (mode != null) {
                }
                if ((j & 148000) != 0) {
                    if (arp != null) {
                        gate = arp.getGate();
                    } else {
                        gate = null;
                    }
                    updateRegistration(5, gate);
                    if (gate != null) {
                        data5 = gate.getData();
                    } else {
                        data5 = 0;
                    }
                    strValueOf3 = String.valueOf(data5);
                } else {
                    strValueOf3 = null;
                }
                if ((j & j5) != 0) {
                    if (arp != null) {
                        oct = arp.getOct();
                    } else {
                        oct = null;
                    }
                    j3 = 139792;
                    updateRegistration(6, oct);
                    if (oct != null) {
                    }
                    if ((j & j4) != 0) {
                        if (arp != null) {
                            sync = arp.getSync();
                        } else {
                            sync = null;
                        }
                        updateRegistration(7, sync);
                        if (sync != null) {
                            data4 = sync.getData();
                        } else {
                            data4 = 0;
                        }
                        z = data4 == 1;
                        z2 = z3;
                    } else {
                        z2 = z3;
                        z = false;
                    }
                } else {
                    j3 = 139792;
                }
                if ((j & j4) != 0) {
                    if (arp != null) {
                        sync = arp.getSync();
                    } else {
                        sync = null;
                    }
                    updateRegistration(7, sync);
                    if (sync != null) {
                        data4 = sync.getData();
                    } else {
                        data4 = 0;
                    }
                    z = data4 == 1;
                    z2 = z3;
                } else {
                    z2 = z3;
                    z = false;
                }
            } else {
                j2 = 131592;
            }
            if ((j & 148000) != 0) {
                if (arp != null) {
                    gate = arp.getGate();
                } else {
                    gate = null;
                }
                updateRegistration(5, gate);
                if (gate != null) {
                    data5 = gate.getData();
                } else {
                    data5 = 0;
                }
                strValueOf3 = String.valueOf(data5);
            } else {
                strValueOf3 = null;
            }
            if ((j & j5) != 0) {
                if (arp != null) {
                    oct = arp.getOct();
                } else {
                    oct = null;
                }
                j3 = 139792;
                updateRegistration(6, oct);
                if (oct != null) {
                }
                if ((j & j4) != 0) {
                    if (arp != null) {
                        sync = arp.getSync();
                    } else {
                        sync = null;
                    }
                    updateRegistration(7, sync);
                    if (sync != null) {
                        data4 = sync.getData();
                    } else {
                        data4 = 0;
                    }
                    z = data4 == 1;
                    z2 = z3;
                } else {
                    z2 = z3;
                    z = false;
                }
            } else {
                j3 = 139792;
            }
            if ((j & j4) != 0) {
                if (arp != null) {
                    sync = arp.getSync();
                } else {
                    sync = null;
                }
                updateRegistration(7, sync);
                if (sync != null) {
                    data4 = sync.getData();
                } else {
                    data4 = 0;
                }
                z = data4 == 1;
                z2 = z3;
            } else {
                z2 = z3;
                z = false;
            }
        } else {
            j2 = 131592;
            j3 = 139792;
            j4 = 197248;
            j5 = 164416;
            strValueOf = null;
            strValueOf2 = null;
            strValueOf3 = null;
            z = false;
            z2 = false;
            data = 0;
            data2 = 0;
            data3 = 0;
        }
        if ((j & 148000) != 0) {
            TextViewBindingAdapter.setText(this.gate, strValueOf3);
        }
        if ((131072 & j) != 0) {
            TextViewBindingAdapter.setTextWatcher(this.gate, null, null, this.mCallback40, null);
            CompoundButtonBindingAdapter.setListeners(this.latch, this.mCallback42, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.mode, this.mCallback38, null, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.octive, this.mCallback39, null, null);
            TextViewBindingAdapter.setTextWatcher(this.swing, null, null, this.mCallback41, null);
            CompoundButtonBindingAdapter.setListeners(this.sync, this.mCallback44, null);
            TextViewBindingAdapter.setTextWatcher(this.tempo, null, null, this.mCallback45, null);
            AdapterViewBindingAdapter.setOnItemSelectedListener(this.time, this.mCallback43, null, null);
        }
        if ((j & 135684) != 0) {
            CompoundButtonBindingAdapter.setChecked(this.latch, z2);
        }
        if ((j & j3) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.mode, data2);
        }
        if ((j & j5) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.octive, data3);
        }
        if ((j & 132609) != 0) {
            TextViewBindingAdapter.setText(this.swing, strValueOf);
        }
        if ((j & j4) != 0) {
            CompoundButtonBindingAdapter.setChecked(this.sync, z);
        }
        if ((j & j2) != 0) {
            TextViewBindingAdapter.setText(this.tempo, strValueOf2);
        }
        if ((j & 133634) != 0) {
            AdapterViewBindingAdapter.setSelectedItemPosition(this.time, data);
        }
    }

    @Override // com.cubeSuite.generated.callback.OnItemSelected.Listener
    public final void _internalCallbackOnItemSelected(int i, AdapterView adapterView, View view, int i2, long j) {
        if (i == 1) {
            Smk25MiniArpFragment smk25MiniArpFragment = this.mListener;
            Arp arp = this.mData;
            if (smk25MiniArpFragment == null || arp == null) {
                return;
            }
            smk25MiniArpFragment.U8DataChange(i2, arp.getMode());
            return;
        }
        if (i == 2) {
            Smk25MiniArpFragment smk25MiniArpFragment2 = this.mListener;
            Arp arp2 = this.mData;
            if (smk25MiniArpFragment2 == null || arp2 == null) {
                return;
            }
            smk25MiniArpFragment2.U8DataChange(i2, arp2.getOct());
            return;
        }
        if (i != 6) {
            return;
        }
        Smk25MiniArpFragment smk25MiniArpFragment3 = this.mListener;
        Arp arp3 = this.mData;
        if (smk25MiniArpFragment3 == null || arp3 == null) {
            return;
        }
        smk25MiniArpFragment3.U8DataChange(i2, arp3.getTime());
    }

    @Override // com.cubeSuite.generated.callback.OnCheckedChangeListener1.Listener
    public final void _internalCallbackOnCheckedChanged1(int i, CompoundButton compoundButton, boolean z) {
        if (i == 5) {
            Smk25MiniArpFragment smk25MiniArpFragment = this.mListener;
            Arp arp = this.mData;
            if (smk25MiniArpFragment != null) {
                if (z) {
                    if (arp != null) {
                        smk25MiniArpFragment.U8DataChange(z ? 1 : 0, arp.getLatch());
                        return;
                    }
                    return;
                }
                if (arp != null) {
                    smk25MiniArpFragment.U8DataChange(z ? 1 : 0, arp.getLatch());
                    return;
                }
                return;
            }
            return;
        }
        if (i != 7) {
            return;
        }
        Smk25MiniArpFragment smk25MiniArpFragment2 = this.mListener;
        Arp arp2 = this.mData;
        if (smk25MiniArpFragment2 != null) {
            if (z) {
                if (arp2 != null) {
                    smk25MiniArpFragment2.U8DataChange(z ? 1 : 0, arp2.getSync());
                    return;
                }
                return;
            }
            if (arp2 != null) {
                smk25MiniArpFragment2.U8DataChange(z ? 1 : 0, arp2.getSync());
            }
        }
    }

    @Override // com.cubeSuite.generated.callback.AfterTextChanged.Listener
    public final void _internalCallbackAfterTextChanged(int i, Editable editable) {
        if (i == 3) {
            Smk25MiniArpFragment smk25MiniArpFragment = this.mListener;
            Arp arp = this.mData;
            if (smk25MiniArpFragment == null || arp == null) {
                return;
            }
            smk25MiniArpFragment.setNum(editable, arp.getGate(), 0, 100);
            return;
        }
        if (i == 4) {
            Smk25MiniArpFragment smk25MiniArpFragment2 = this.mListener;
            Arp arp2 = this.mData;
            if (smk25MiniArpFragment2 == null || arp2 == null) {
                return;
            }
            smk25MiniArpFragment2.setNum(editable, arp2.getSwing(), 0, 75);
            return;
        }
        if (i != 8) {
            return;
        }
        Smk25MiniArpFragment smk25MiniArpFragment3 = this.mListener;
        Arp arp3 = this.mData;
        if (smk25MiniArpFragment3 == null || arp3 == null) {
            return;
        }
        smk25MiniArpFragment3.setU16Num(editable, arp3.getTempo(), 0, MaterialCardViewHelper.DEFAULT_FADE_ANIM_DURATION);
    }
}
