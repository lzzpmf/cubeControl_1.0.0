package com.cubeSuite.entity.smcPad;

import com.cubeSuite.communication.Communication;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.google.android.material.card.MaterialCardViewHelper;

/* JADX INFO: loaded from: classes.dex */
public class SmcPadEntry {
    public final int USR_ARR_SIZE = 28312;
    public final int GLOB_SIZE = 12;
    public SmcPadUser[] usr = new SmcPadUser[8];
    public SmcPadGlob smcPadGlob = new SmcPadGlob();

    public SmcPadEntry() {
        int i = 0;
        while (true) {
            SmcPadUser[] smcPadUserArr = this.usr;
            if (i >= smcPadUserArr.length) {
                return;
            }
            smcPadUserArr[i] = new SmcPadUser();
            i++;
        }
    }

    public class SmcPadUser {
        public SmcPadButton[] smcPadButtons = new SmcPadButton[5];
        public SmcPadKnob[] smcPadKnobs = new SmcPadKnob[16];
        public SmcPadPad[] smcPadPads = new SmcPadPad[128];

        public SmcPadUser() {
            int i = 0;
            int i2 = 0;
            while (true) {
                SmcPadButton[] smcPadButtonArr = this.smcPadButtons;
                if (i2 >= smcPadButtonArr.length) {
                    break;
                }
                smcPadButtonArr[i2] = new SmcPadButton();
                i2++;
            }
            int i3 = 0;
            while (true) {
                SmcPadKnob[] smcPadKnobArr = this.smcPadKnobs;
                if (i3 >= smcPadKnobArr.length) {
                    break;
                }
                smcPadKnobArr[i3] = new SmcPadKnob();
                i3++;
            }
            while (true) {
                SmcPadPad[] smcPadPadArr = this.smcPadPads;
                if (i >= smcPadPadArr.length) {
                    return;
                }
                smcPadPadArr[i] = new SmcPadPad();
                i++;
            }
        }

        public class SmcPadKnob {
            public AddrU8 ch;
            public AddrU8 curve;
            public AddrU8[] dat = new AddrU8[3];
            public AddrU8 type;

            public SmcPadKnob() {
            }
        }

        public class SmcPadPad {
            public AddrU8 ch;
            public AddrU8 softLed;
            public SmcPadSys sys;
            public AddrU8 type;
            public AddrU8[] dat = new AddrU8[3];
            public AddrU8[] color = new AddrU8[3];

            public SmcPadPad() {
                this.sys = SmcPadUser.this.new SmcPadSys();
            }
        }

        public class SmcPadButton {
            public AddrU8 ch;
            public AddrU8[] dat = new AddrU8[3];
            public AddrU8 ledMode;
            public SmcPadSys sys;
            public AddrU8 type;

            public SmcPadButton() {
                this.sys = SmcPadUser.this.new SmcPadSys();
            }
        }

        public class SmcPadSys {
            public AddrU8 len;
            public AddrU8[] val = new AddrU8[16];

            public SmcPadSys() {
            }
        }
    }

    public class SmcPadGlob {
        public AddrU8 latch;
        public AddrU8 octave;
        public AddrU8 padaft;
        public AddrU8 padbank;
        public AddrU8 padcurve;
        public AddrU8 preset;
        public AddrU8 swing;
        public AddrU8 sync;
        public AddrU16 tempo;
        public AddrU8 time;
        public AddrI8 transpose;

        public SmcPadGlob() {
        }
    }

    public void setUsrData(byte[] bArr) {
        if (bArr.length != 28312) {
            return;
        }
        int i = 0;
        for (int i2 = 0; i2 < this.usr.length; i2++) {
            for (int i3 = 0; i3 < this.usr[i2].smcPadButtons.length; i3++) {
                this.usr[i2].smcPadButtons[i3].type = new AddrU8(bArr[i], i, 0, 5);
                int i4 = i + 1;
                this.usr[i2].smcPadButtons[i3].ch = new AddrU8(bArr[i4], i4, 0, 15);
                int i5 = i + 2;
                for (int i6 = 0; i6 < 3; i6++) {
                    this.usr[i2].smcPadButtons[i3].dat[i6] = new AddrU8(bArr[i5], i5, 0, 127);
                    i5++;
                }
                this.usr[i2].smcPadButtons[i3].sys.len = new AddrU8(bArr[i5], i5, 0, 127);
                int i7 = i5 + 1;
                for (int i8 = 0; i8 < 16; i8++) {
                    this.usr[i2].smcPadButtons[i3].sys.val[i8] = new AddrU8(bArr[i7], i7);
                    i7++;
                }
                this.usr[i2].smcPadButtons[i3].ledMode = new AddrU8(bArr[i7], i7, 0, 255);
                i = i7 + 1;
            }
            for (int i9 = 0; i9 < this.usr[i2].smcPadKnobs.length; i9++) {
                this.usr[i2].smcPadKnobs[i9].type = new AddrU8(bArr[i], i, 0, 3);
                int i10 = i + 1;
                this.usr[i2].smcPadKnobs[i9].curve = new AddrU8(bArr[i10], i10, 0, 4);
                int i11 = i + 2;
                this.usr[i2].smcPadKnobs[i9].ch = new AddrU8(bArr[i11], i11, 0, 15);
                i += 3;
                for (int i12 = 0; i12 < 3; i12++) {
                    this.usr[i2].smcPadKnobs[i9].dat[i12] = new AddrU8(bArr[i], i, 0, 127);
                    i++;
                }
            }
            for (int i13 = 0; i13 < this.usr[i2].smcPadPads.length; i13++) {
                this.usr[i2].smcPadPads[i13].type = new AddrU8(bArr[i], i, 0, 6);
                int i14 = i + 1;
                this.usr[i2].smcPadPads[i13].ch = new AddrU8(bArr[i14], i14, 0, 15);
                int i15 = i + 2;
                for (int i16 = 0; i16 < 3; i16++) {
                    this.usr[i2].smcPadPads[i13].dat[i16] = new AddrU8(bArr[i15], i15, 0, 127);
                    i15++;
                }
                for (int i17 = 0; i17 < 3; i17++) {
                    this.usr[i2].smcPadPads[i13].color[i17] = new AddrU8(bArr[i15], i15, 0, 255);
                    i15++;
                }
                this.usr[i2].smcPadPads[i13].softLed = new AddrU8(bArr[i15], i15, 0, 255);
                int i18 = i15 + 1;
                this.usr[i2].smcPadPads[i13].sys.len = new AddrU8(bArr[i18], i18, 0, 127);
                i = i15 + 2;
                for (int i19 = 0; i19 < 16; i19++) {
                    this.usr[i2].smcPadPads[i13].sys.val[i19] = new AddrU8(bArr[i], i);
                    i++;
                }
            }
        }
    }

    public void setGlobData(byte[] bArr) {
        if (bArr.length != 12) {
            return;
        }
        this.smcPadGlob.tempo = new AddrU16(Communication.u8ArrToU16(bArr[0], bArr[1]), 0L, 30, MaterialCardViewHelper.DEFAULT_FADE_ANIM_DURATION);
        this.smcPadGlob.swing = new AddrU8(bArr[2], 2, 50, 75);
        this.smcPadGlob.time = new AddrU8(bArr[3], 3, 0, 7);
        this.smcPadGlob.sync = new AddrU8(bArr[4], 4, 0, 1);
        this.smcPadGlob.latch = new AddrU8(bArr[5], 5, 0, 1);
        this.smcPadGlob.octave = new AddrU8(bArr[6], 6, 0, 6);
        this.smcPadGlob.transpose = new AddrI8(bArr[7], 7, -16, 16);
        this.smcPadGlob.padcurve = new AddrU8(bArr[8], 8, 0, 3);
        this.smcPadGlob.padaft = new AddrU8(bArr[9], 9, 0, 1);
        this.smcPadGlob.preset = new AddrU8(bArr[10], 10, 0, 7);
        this.smcPadGlob.padbank = new AddrU8(bArr[11], 11, 0, 1);
    }
}
