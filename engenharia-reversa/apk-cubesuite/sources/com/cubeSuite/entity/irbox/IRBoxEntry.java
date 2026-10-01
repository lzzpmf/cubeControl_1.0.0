package com.cubeSuite.entity.irbox;

import androidx.core.view.accessibility.AccessibilityNodeInfoCompat;
import com.cubeSuite.communication.Communication;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class IRBoxEntry {
    public final int IRR_SIZE = 6144;
    public final int DATA_SIZE = 8192;
    public final int NAME_SIZE = 17;
    public final int ALL_NAME_SIZE = 544;
    public IRBox irBox = new IRBox();

    public class IRBoxEq {
        public AddrU16 hpHz;
        public AddrI8 hsDb;
        public AddrU16 hsHz;
        public AddrU16 lpHz;
        public AddrI8 lsDb;
        public AddrU16 lsHz;
        public AddrU8 rsv;
        public AddrU8[] enable = new AddrU8[9];
        public AddrU8[] peakQ = new AddrU8[5];
        public AddrI8[] peakDb = new AddrI8[5];
        public AddrU16[] peakHz = new AddrU16[5];

        public IRBoxEq() {
        }
    }

    public class IRBoxCad {
        public int irrAddr;
        public AddrU8 lvl;
        public AddrU8 type;
        public AddrU8[] magic = new AddrU8[4];
        public byte[] name = new byte[14];
        public byte[] ir_ram = new byte[6144];

        public IRBoxCad() {
        }
    }

    public class IRBoxHead {
        public AddrU8 cabon;
        public AddrU8 eqon;
        public String name;
        public int nameAddr;
        public byte[] nameArr = new byte[17];
        public AddrU8[] magic = new AddrU8[5];

        public IRBoxHead() {
        }
    }

    public class IRBox {
        public IRBoxCad cad;
        public IRBoxEq eq;
        public IRBoxHead head;
        public String[] nameArr;

        IRBox() {
            this.head = IRBoxEntry.this.new IRBoxHead();
            this.cad = IRBoxEntry.this.new IRBoxCad();
            this.eq = IRBoxEntry.this.new IRBoxEq();
            String[] strArr = new String[32];
            this.nameArr = strArr;
            Arrays.fill(strArr, "UNKNOWN");
        }
    }

    public boolean setNameData(byte[] bArr) {
        if (bArr.length != 544) {
            return false;
        }
        for (int i = 0; i < this.irBox.nameArr.length; i++) {
            byte[] bArr2 = new byte[17];
            System.arraycopy(bArr, i * 17, bArr2, 0, 17);
            ArrayList arrayList = new ArrayList();
            for (int i2 = 0; i2 < 17; i2++) {
                byte b = bArr2[i2];
                if (b == 0) {
                    break;
                }
                if (b >= 32 && b <= 126) {
                    arrayList.add(Byte.valueOf(b));
                }
            }
            String str = new String(Communication.byteListToByteArr(arrayList), StandardCharsets.UTF_8);
            if (!str.isEmpty()) {
                this.irBox.nameArr[i] = "(" + (i + 1) + ")" + str;
            } else {
                this.irBox.nameArr[i] = "(" + (i + 1) + ")UNKNOWN";
            }
        }
        return true;
    }

    public boolean setData(byte[] bArr) {
        if (bArr.length != 8192) {
            return false;
        }
        System.arraycopy(bArr, 0, this.irBox.head.nameArr, 0, 17);
        this.irBox.head.name = new String(this.irBox.head.nameArr, StandardCharsets.UTF_8);
        this.irBox.head.nameAddr = 0;
        this.irBox.head.cabon = new AddrU8(bArr[17], 17, 0, 1);
        this.irBox.head.eqon = new AddrU8(bArr[18], 18, 0, 1);
        int i = 19;
        for (int i2 = 0; i2 < this.irBox.head.magic.length; i2++) {
            this.irBox.head.magic[i2] = new AddrU8(bArr[i], i, 0, 1);
            i++;
        }
        for (int i3 = 0; i3 < this.irBox.cad.magic.length; i3++) {
            this.irBox.cad.magic[i3] = new AddrU8(bArr[i], i, 0, 1);
            i++;
        }
        System.arraycopy(bArr, i, this.irBox.cad.name, 0, 14);
        int i4 = i + 14;
        this.irBox.cad.type = new AddrU8(bArr[i4], i4, 0, 1);
        int i5 = i + 15;
        this.irBox.cad.lvl = new AddrU8(bArr[i5], i5, 0, 127);
        int i6 = i + 16;
        this.irBox.cad.irrAddr = i6;
        System.arraycopy(bArr, i6, this.irBox.cad.ir_ram, 0, 6144);
        int i7 = 7168;
        for (int i8 = 0; i8 < this.irBox.eq.enable.length; i8++) {
            this.irBox.eq.enable[i8] = new AddrU8(bArr[i7], i7, 0, 1);
            i7++;
        }
        for (int i9 = 0; i9 < this.irBox.eq.peakQ.length; i9++) {
            this.irBox.eq.peakQ[i9] = new AddrU8(bArr[i7], i7, 1, 160);
            i7++;
        }
        for (int i10 = 0; i10 < this.irBox.eq.peakDb.length; i10++) {
            this.irBox.eq.peakDb[i10] = new AddrI8(bArr[i7], i7, -120, 120);
            i7++;
        }
        this.irBox.eq.lsDb = new AddrI8(bArr[i7], i7, -120, 120);
        int i11 = i7 + 1;
        this.irBox.eq.hsDb = new AddrI8(bArr[i11], i11, -120, 120);
        int i12 = i7 + 2;
        this.irBox.eq.rsv = new AddrU8(bArr[i12], i12);
        int i13 = i7 + 3;
        for (int i14 = 0; i14 < this.irBox.eq.peakHz.length; i14++) {
            this.irBox.eq.peakHz[i14] = new AddrU16(Communication.u8ArrToU16(bArr[i13], bArr[i13 + 1]), i13, 20, AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH);
            i13 += 2;
        }
        this.irBox.eq.lsHz = new AddrU16(Communication.u8ArrToU16(bArr[i13], bArr[i13 + 1]), i13, 20, AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH);
        int i15 = i13 + 2;
        this.irBox.eq.hsHz = new AddrU16(Communication.u8ArrToU16(bArr[i15], bArr[i13 + 3]), i15, 20, AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH);
        int i16 = i13 + 4;
        this.irBox.eq.lpHz = new AddrU16(Communication.u8ArrToU16(bArr[i16], bArr[i13 + 5]), i16, 20, AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH);
        int i17 = i13 + 6;
        this.irBox.eq.hpHz = new AddrU16(Communication.u8ArrToU16(bArr[i17], bArr[i13 + 7]), i17, 20, AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH);
        return true;
    }
}
