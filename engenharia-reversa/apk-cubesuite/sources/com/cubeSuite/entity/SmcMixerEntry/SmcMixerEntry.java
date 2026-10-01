package com.cubeSuite.entity.SmcMixerEntry;

import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class SmcMixerEntry {
    public final int USR_ARR_SIZE = 6282;
    public final int USR_SIZE = 3141;
    public SmcMixerUsr[] usr = new SmcMixerUsr[2];

    public SmcMixerEntry() {
        int i = 0;
        while (true) {
            SmcMixerUsr[] smcMixerUsrArr = this.usr;
            if (i >= smcMixerUsrArr.length) {
                return;
            }
            smcMixerUsrArr[i] = new SmcMixerUsr();
            i++;
        }
    }

    public void setUsrData(byte[] bArr) {
        if (bArr.length != 6282) {
            return;
        }
        for (int i = 0; i < this.usr.length; i++) {
            int i2 = i * 3141;
            for (int i3 = 0; i3 < this.usr[i].track.length; i3++) {
                int i4 = (i3 * 71) + i2;
                this.usr[i].track[i3].type = new AddrU8(bArr[i4], i4, 0, 5);
                int i5 = i4 + 1;
                this.usr[i].track[i3].ch = new AddrU8(bArr[i5], i5, 0, 15);
                for (int i6 = 0; i6 < 3; i6++) {
                    int i7 = i4 + 2 + i6;
                    this.usr[i].track[i3].dat[i6] = new AddrU8(bArr[i7], i7, 0, 127);
                }
                int i8 = i4 + 5;
                this.usr[i].track[i3].sys.len = new AddrU8(bArr[i8], i8, 0, 127);
                for (int i9 = 0; i9 < 64; i9++) {
                    int i10 = i4 + 6 + i9;
                    this.usr[i].track[i3].sys.val[i9] = new AddrU8(bArr[i10], i10);
                }
                int i11 = i4 + 70;
                this.usr[i].track[i3].softled = new AddrU8(bArr[i11], i11, 0, 255);
            }
            for (int i12 = 0; i12 < this.usr[i].globe.length; i12++) {
                int i13 = (i12 * 71) + i2;
                int i14 = i13 + 2272;
                this.usr[i].globe[i12].type = new AddrU8(bArr[i14], i14, 0, 5);
                int i15 = i13 + 2273;
                this.usr[i].globe[i12].ch = new AddrU8(bArr[i15], i15, 0, 15);
                for (int i16 = 0; i16 < 3; i16++) {
                    int i17 = i13 + 2274 + i16;
                    this.usr[i].globe[i12].dat[i16] = new AddrU8(bArr[i17], i17, 0, 127);
                }
                int i18 = i13 + 2277;
                this.usr[i].globe[i12].sys.len = new AddrU8(bArr[i18], i18, 0, 127);
                for (int i19 = 0; i19 < 64; i19++) {
                    int i20 = i13 + 2278 + i19;
                    this.usr[i].globe[i12].sys.val[i19] = new AddrU8(bArr[i20], i20);
                }
                int i21 = i13 + 2342;
                this.usr[i].globe[i12].softled = new AddrU8(bArr[i21], i21, 0, 255);
            }
            for (int i22 = 0; i22 < this.usr[i].knob.length; i22++) {
                int i23 = (i22 * 6) + i2;
                int i24 = i23 + 3053;
                this.usr[i].knob[i22].type = new AddrU8(bArr[i24], i24, 0, 3);
                int i25 = i23 + 3054;
                this.usr[i].knob[i22].curve = new AddrU8(bArr[i25], i25, 0, 4);
                int i26 = i23 + 3055;
                this.usr[i].knob[i22].ch = new AddrU8(bArr[i26], i26, 0, 15);
                for (int i27 = 0; i27 < 3; i27++) {
                    int i28 = i23 + 3056 + i27;
                    this.usr[i].knob[i22].dat[i27] = new AddrU8(bArr[i28], i28, 0, 127);
                }
            }
            for (int i29 = 0; i29 < this.usr[i].slider.length; i29++) {
                int i30 = (i29 * 5) + i2;
                int i31 = i30 + 3101;
                this.usr[i].slider[i29].type = new AddrU8(bArr[i31], i31, 0, 3);
                int i32 = i30 + 3102;
                this.usr[i].slider[i29].ch = new AddrU8(bArr[i32], i32, 0, 15);
                for (int i33 = 0; i33 < 3; i33++) {
                    int i34 = i30 + 3103 + i33;
                    this.usr[i].slider[i29].dat[i33] = new AddrU8(bArr[i34], i34, 0, 127);
                }
            }
        }
    }
}
