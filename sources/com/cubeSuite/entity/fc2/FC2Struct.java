package com.cubeSuite.entity.fc2;

import com.cubeSuite.entity.addrData.AddrU8;
import java.lang.reflect.Array;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class FC2Struct {
    public AddrU8 bankMidi;
    public AddrU8 ch;
    public AddrU8 hidpage;
    public AddrU8 mixpage;
    public AddrU8 pcdisp;
    public AddrU8 polar;
    public AddrU8 state;
    public AddrU8 trs;
    public AddrU8 usrpage;
    public AddrU8[][] usr = (AddrU8[][]) Array.newInstance((Class<?>) AddrU8.class, 5, 2);
    public MidiCodeStruct[] midiCodeTap = new MidiCodeStruct[16];
    public AdvCustomStruct1[][] advCustom1 = (AdvCustomStruct1[][]) Array.newInstance((Class<?>) AdvCustomStruct1.class, 8, 4);
    public AddrU8[][][] customKey = (AddrU8[][][]) Array.newInstance((Class<?>) AddrU8.class, 6, 4, 3);
    public MixKeyStruct[][] mixKey = (MixKeyStruct[][]) Array.newInstance((Class<?>) MixKeyStruct.class, 6, 4);
    public AddrU8[] bankMax = new AddrU8[3];
    public final int DATA_SIZE = 23646;

    public FC2Struct() {
        int i = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeTap;
            if (i >= midiCodeStructArr.length) {
                break;
            }
            midiCodeStructArr[i] = new MidiCodeStruct();
            i++;
        }
        for (int i2 = 0; i2 < this.advCustom1.length; i2++) {
            int i3 = 0;
            while (true) {
                AdvCustomStruct1[] advCustomStruct1Arr = this.advCustom1[i2];
                if (i3 < advCustomStruct1Arr.length) {
                    advCustomStruct1Arr[i3] = new AdvCustomStruct1();
                    i3++;
                }
            }
        }
        for (int i4 = 0; i4 < this.mixKey.length; i4++) {
            int i5 = 0;
            while (true) {
                MixKeyStruct[] mixKeyStructArr = this.mixKey[i4];
                if (i5 < mixKeyStructArr.length) {
                    mixKeyStructArr[i5] = new MixKeyStruct();
                    i5++;
                }
            }
        }
    }

    public ArrayList<Byte> getData() {
        ArrayList<Byte> arrayList = new ArrayList<>();
        arrayList.add(Byte.valueOf(this.state.getByteData()));
        arrayList.add(Byte.valueOf(this.trs.getByteData()));
        arrayList.add(Byte.valueOf(this.ch.getByteData()));
        int i = 0;
        for (int i2 = 0; i2 < this.usr.length; i2++) {
            int i3 = 0;
            while (true) {
                AddrU8[] addrU8Arr = this.usr[i2];
                if (i3 < addrU8Arr.length) {
                    arrayList.add(Byte.valueOf(addrU8Arr[i3].getByteData()));
                    i3++;
                }
            }
        }
        int i4 = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeTap;
            if (i4 >= midiCodeStructArr.length) {
                break;
            }
            arrayList.add(Byte.valueOf(midiCodeStructArr[i4].isEnable.getByteData()));
            arrayList.add(Byte.valueOf(this.midiCodeTap[i4].channel.getByteData()));
            arrayList.add(Byte.valueOf(this.midiCodeTap[i4].type.getByteData()));
            arrayList.add(Byte.valueOf(this.midiCodeTap[i4].data1.getByteData()));
            arrayList.add(Byte.valueOf(this.midiCodeTap[i4].data2.getByteData()));
            i4++;
        }
        for (int i5 = 0; i5 < this.advCustom1.length; i5++) {
            int i6 = 0;
            while (true) {
                AdvCustomStruct1[] advCustomStruct1Arr = this.advCustom1[i5];
                if (i6 < advCustomStruct1Arr.length) {
                    arrayList.add(Byte.valueOf(advCustomStruct1Arr[i6].mode.getByteData()));
                    for (int i7 = 0; i7 < this.advCustom1[i5][i6].midiCodeA.length; i7++) {
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeA[i7].isEnable.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeA[i7].channel.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeA[i7].type.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeA[i7].data1.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeA[i7].data2.getByteData()));
                    }
                    for (int i8 = 0; i8 < this.advCustom1[i5][i6].midiCodeB.length; i8++) {
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeB[i8].isEnable.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeB[i8].channel.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeB[i8].type.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeB[i8].data1.getByteData()));
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].midiCodeB[i8].data2.getByteData()));
                    }
                    for (int i9 = 0; i9 < this.advCustom1[i5][i6].sysExA.length; i9++) {
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].sysExA[i9]));
                    }
                    for (int i10 = 0; i10 < this.advCustom1[i5][i6].sysExB.length; i10++) {
                        arrayList.add(Byte.valueOf(this.advCustom1[i5][i6].sysExB[i10]));
                    }
                    i6++;
                }
            }
        }
        for (int i11 = 0; i11 < this.customKey.length; i11++) {
            for (int i12 = 0; i12 < this.customKey[i11].length; i12++) {
                int i13 = 0;
                while (true) {
                    AddrU8[] addrU8Arr2 = this.customKey[i11][i12];
                    if (i13 < addrU8Arr2.length) {
                        arrayList.add(Byte.valueOf(addrU8Arr2[i13].getByteData()));
                        i13++;
                    }
                }
            }
        }
        for (int i14 = 0; i14 < this.mixKey.length; i14++) {
            int i15 = 0;
            while (true) {
                MixKeyStruct[] mixKeyStructArr = this.mixKey[i14];
                if (i15 < mixKeyStructArr.length) {
                    arrayList.add(Byte.valueOf(mixKeyStructArr[i15].type.getByteData()));
                    arrayList.add(Byte.valueOf(this.mixKey[i14][i15].tunerdat.getByteData()));
                    for (int i16 = 0; i16 < this.mixKey[i14][i15].hidkeydat.length; i16++) {
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].hidkeydat[i16].getByteData()));
                    }
                    arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.mode.getByteData()));
                    for (int i17 = 0; i17 < this.mixKey[i14][i15].midiUsrPlus.midiCodeA.length; i17++) {
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeA[i17].isEnable.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeA[i17].channel.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeA[i17].type.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeA[i17].data1.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeA[i17].data2.getByteData()));
                    }
                    for (int i18 = 0; i18 < this.mixKey[i14][i15].midiUsrPlus.midiCodeB.length; i18++) {
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeB[i18].isEnable.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeB[i18].channel.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeB[i18].type.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeB[i18].data1.getByteData()));
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.midiCodeB[i18].data2.getByteData()));
                    }
                    for (int i19 = 0; i19 < this.mixKey[i14][i15].midiUsrPlus.sysExA.length; i19++) {
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.sysExA[i19]));
                    }
                    for (int i20 = 0; i20 < this.mixKey[i14][i15].midiUsrPlus.sysExB.length; i20++) {
                        arrayList.add(Byte.valueOf(this.mixKey[i14][i15].midiUsrPlus.sysExB[i20]));
                    }
                    i15++;
                }
            }
        }
        while (true) {
            AddrU8[] addrU8Arr3 = this.bankMax;
            if (i < addrU8Arr3.length) {
                arrayList.add(Byte.valueOf(addrU8Arr3[i].getByteData()));
                i++;
            } else {
                arrayList.add(Byte.valueOf(this.usrpage.getByteData()));
                arrayList.add(Byte.valueOf(this.hidpage.getByteData()));
                arrayList.add(Byte.valueOf(this.polar.getByteData()));
                arrayList.add(Byte.valueOf(this.bankMidi.getByteData()));
                arrayList.add(Byte.valueOf(this.pcdisp.getByteData()));
                arrayList.add(Byte.valueOf(this.mixpage.getByteData()));
                return arrayList;
            }
        }
    }

    public boolean setData(byte[] bArr) {
        int i = 0;
        if (bArr.length != 23646) {
            return false;
        }
        this.state = new AddrU8(bArr[0], 0, 0, 12);
        this.trs = new AddrU8(bArr[1], 1, 0, 1);
        this.ch = new AddrU8(bArr[2], 2, 0, 16);
        int i2 = 3;
        for (int i3 = 0; i3 < this.usr.length; i3++) {
            int i4 = 0;
            while (true) {
                AddrU8[] addrU8Arr = this.usr[i3];
                if (i4 < addrU8Arr.length) {
                    addrU8Arr[i4] = new AddrU8(bArr[i2], i2);
                    i2++;
                    i4++;
                }
            }
        }
        int i5 = 0;
        while (true) {
            MidiCodeStruct[] midiCodeStructArr = this.midiCodeTap;
            if (i5 >= midiCodeStructArr.length) {
                break;
            }
            midiCodeStructArr[i5].isEnable = new AddrU8(bArr[i2], i2, 0, 1);
            int i6 = i2 + 1;
            this.midiCodeTap[i5].channel = new AddrU8(bArr[i6], i6, 0, 15);
            int i7 = i2 + 2;
            this.midiCodeTap[i5].type = new AddrU8(bArr[i7], i7);
            int i8 = i2 + 3;
            this.midiCodeTap[i5].data1 = new AddrU8(bArr[i8], i8);
            int i9 = i2 + 4;
            this.midiCodeTap[i5].data2 = new AddrU8(bArr[i9], i9);
            i2 += 5;
            i5++;
        }
        for (int i10 = 0; i10 < this.advCustom1.length; i10++) {
            int i11 = 0;
            while (true) {
                AdvCustomStruct1[] advCustomStruct1Arr = this.advCustom1[i10];
                if (i11 < advCustomStruct1Arr.length) {
                    advCustomStruct1Arr[i11].mode = new AddrU8(bArr[i2], i2);
                    i2++;
                    for (int i12 = 0; i12 < this.advCustom1[i10][i11].midiCodeA.length; i12++) {
                        this.advCustom1[i10][i11].midiCodeA[i12].isEnable = new AddrU8(bArr[i2], i2, 0, 1);
                        int i13 = i2 + 1;
                        this.advCustom1[i10][i11].midiCodeA[i12].channel = new AddrU8(bArr[i13], i13, 0, 15);
                        int i14 = i2 + 2;
                        this.advCustom1[i10][i11].midiCodeA[i12].type = new AddrU8(bArr[i14], i14);
                        int i15 = i2 + 3;
                        this.advCustom1[i10][i11].midiCodeA[i12].data1 = new AddrU8(bArr[i15], i15);
                        int i16 = i2 + 4;
                        this.advCustom1[i10][i11].midiCodeA[i12].data2 = new AddrU8(bArr[i16], i16);
                        i2 += 5;
                    }
                    for (int i17 = 0; i17 < this.advCustom1[i10][i11].midiCodeB.length; i17++) {
                        this.advCustom1[i10][i11].midiCodeB[i17].isEnable = new AddrU8(bArr[i2], i2, 0, 1);
                        int i18 = i2 + 1;
                        this.advCustom1[i10][i11].midiCodeB[i17].channel = new AddrU8(bArr[i18], i18, 0, 15);
                        int i19 = i2 + 2;
                        this.advCustom1[i10][i11].midiCodeB[i17].type = new AddrU8(bArr[i19], i19);
                        int i20 = i2 + 3;
                        this.advCustom1[i10][i11].midiCodeB[i17].data1 = new AddrU8(bArr[i20], i20);
                        int i21 = i2 + 4;
                        this.advCustom1[i10][i11].midiCodeB[i17].data2 = new AddrU8(bArr[i21], i21);
                        i2 += 5;
                    }
                    for (int i22 = 0; i22 < this.advCustom1[i10][i11].sysExA.length; i22++) {
                        this.advCustom1[i10][i11].sysExA[i22] = bArr[i2];
                        i2++;
                    }
                    for (int i23 = 0; i23 < this.advCustom1[i10][i11].sysExB.length; i23++) {
                        this.advCustom1[i10][i11].sysExB[i23] = bArr[i2];
                        i2++;
                    }
                    i11++;
                }
            }
        }
        for (int i24 = 0; i24 < this.customKey.length; i24++) {
            for (int i25 = 0; i25 < this.customKey[i24].length; i25++) {
                int i26 = 0;
                while (true) {
                    AddrU8[] addrU8Arr2 = this.customKey[i24][i25];
                    if (i26 < addrU8Arr2.length) {
                        addrU8Arr2[i26] = new AddrU8(bArr[i2], i2);
                        i2++;
                        i26++;
                    }
                }
            }
        }
        for (int i27 = 0; i27 < this.mixKey.length; i27++) {
            int i28 = 0;
            while (true) {
                MixKeyStruct[] mixKeyStructArr = this.mixKey[i27];
                if (i28 < mixKeyStructArr.length) {
                    mixKeyStructArr[i28].type = new AddrU8(bArr[i2], i2);
                    int i29 = i2 + 1;
                    this.mixKey[i27][i28].tunerdat = new AddrU8(bArr[i29], i29);
                    int i30 = i2 + 2;
                    for (int i31 = 0; i31 < this.mixKey[i27][i28].hidkeydat.length; i31++) {
                        this.mixKey[i27][i28].hidkeydat[i31] = new AddrU8(bArr[i30], i30);
                        i30++;
                    }
                    this.mixKey[i27][i28].midiUsrPlus.mode = new AddrU8(bArr[i30], i30, 0, 1);
                    i2 = i30 + 1;
                    for (int i32 = 0; i32 < this.mixKey[i27][i28].midiUsrPlus.midiCodeA.length; i32++) {
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeA[i32].isEnable = new AddrU8(bArr[i2], i2, 0, 1);
                        int i33 = i2 + 1;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeA[i32].channel = new AddrU8(bArr[i33], i33, 0, 15);
                        int i34 = i2 + 2;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeA[i32].type = new AddrU8(bArr[i34], i34);
                        int i35 = i2 + 3;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeA[i32].data1 = new AddrU8(bArr[i35], i35);
                        int i36 = i2 + 4;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeA[i32].data2 = new AddrU8(bArr[i36], i36);
                        i2 += 5;
                    }
                    for (int i37 = 0; i37 < this.mixKey[i27][i28].midiUsrPlus.midiCodeB.length; i37++) {
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeB[i37].isEnable = new AddrU8(bArr[i2], i2, 0, 1);
                        int i38 = i2 + 1;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeB[i37].channel = new AddrU8(bArr[i38], i38, 0, 15);
                        int i39 = i2 + 2;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeB[i37].type = new AddrU8(bArr[i39], i39);
                        int i40 = i2 + 3;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeB[i37].data1 = new AddrU8(bArr[i40], i40);
                        int i41 = i2 + 4;
                        this.mixKey[i27][i28].midiUsrPlus.midiCodeB[i37].data2 = new AddrU8(bArr[i41], i41);
                        i2 += 5;
                    }
                    for (int i42 = 0; i42 < this.mixKey[i27][i28].midiUsrPlus.sysExA.length; i42++) {
                        this.mixKey[i27][i28].midiUsrPlus.sysExA[i42] = bArr[i2];
                        i2++;
                    }
                    for (int i43 = 0; i43 < this.mixKey[i27][i28].midiUsrPlus.sysExB.length; i43++) {
                        this.mixKey[i27][i28].midiUsrPlus.sysExB[i43] = bArr[i2];
                        i2++;
                    }
                    i28++;
                }
            }
        }
        while (true) {
            AddrU8[] addrU8Arr3 = this.bankMax;
            if (i < addrU8Arr3.length) {
                addrU8Arr3[i] = new AddrU8(bArr[i2], i2);
                i2++;
                i++;
            } else {
                this.usrpage = new AddrU8(bArr[i2], i2);
                int i44 = i2 + 1;
                this.hidpage = new AddrU8(bArr[i44], i44);
                int i45 = i2 + 2;
                this.polar = new AddrU8(bArr[i45], i45);
                int i46 = i2 + 3;
                this.bankMidi = new AddrU8(bArr[i46], i46);
                int i47 = i2 + 4;
                this.pcdisp = new AddrU8(bArr[i47], i47);
                int i48 = i2 + 5;
                this.mixpage = new AddrU8(bArr[i48], i48);
                return true;
            }
        }
    }
}
