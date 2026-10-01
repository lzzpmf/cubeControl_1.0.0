package com.cubeSuite.entity.smk25II;

import com.cubeSuite.communication.Communication;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.cubeSuite.entity.midikeyboard.Glob;

/* JADX INFO: loaded from: classes.dex */
public class Smk25IIEntry {
    public final int GLOB_SIZE = 19;
    public final int USR_SIZE = 2504;
    public final int USR_ARR_SIZE = 20032;
    private Glob glob = new Glob();
    private Smk25IIUsr[] usr = new Smk25IIUsr[8];

    public Smk25IIEntry() {
        int i = 0;
        while (true) {
            Smk25IIUsr[] smk25IIUsrArr = this.usr;
            if (i >= smk25IIUsrArr.length) {
                return;
            }
            smk25IIUsrArr[i] = new Smk25IIUsr();
            i++;
        }
    }

    public Glob getGlob() {
        return this.glob;
    }

    public void setGlob(Glob glob) {
        this.glob = glob;
    }

    public Smk25IIUsr[] getUsr() {
        return this.usr;
    }

    public void setUsr(Smk25IIUsr[] smk25IIUsrArr) {
        this.usr = smk25IIUsrArr;
    }

    public void setGlobData(byte[] bArr) {
        if (bArr.length != 19) {
            return;
        }
        this.glob.setPreset(new AddrU8(bArr[0], 0L));
        this.glob.setPadCurve(new AddrU8(bArr[1], 1L));
        this.glob.setKeyCurve(new AddrU8(bArr[2], 2L));
        this.glob.setPitchDet(new AddrU8(bArr[3], 3L));
        this.glob.setPadAft(new AddrU8(bArr[4], 4L));
        this.glob.getShift().setMajor(new AddrU8(bArr[5], 5L));
        this.glob.getShift().setScale(new AddrU8(bArr[6], 6L));
        this.glob.getShift().setChord(new AddrU8(bArr[7], 7L));
        this.glob.getArp().setMode(new AddrU8(bArr[8], 8L));
        this.glob.getArp().setOct(new AddrU8(bArr[9], 9L));
        this.glob.getArp().setGate(new AddrU8(bArr[10], 10L));
        this.glob.getArp().setSwing(new AddrU8(bArr[11], 11L));
        this.glob.getArp().setLatch(new AddrU8(bArr[12], 12L));
        this.glob.getArp().setTime(new AddrU8(bArr[13], 13L));
        this.glob.getArp().setSync(new AddrU8(bArr[14], 14L));
        this.glob.getArp().setTempo(new AddrU16(Communication.u8ArrToU16(bArr[16], bArr[17]), 16L));
        this.glob.setInputMode(new AddrU8(bArr[18], 18L));
    }

    public void setUsrData(byte[] bArr) {
        if (bArr.length < 20032) {
            return;
        }
        for (int i = 0; i < 8; i++) {
            int i2 = i * 2504;
            this.usr[i].getKeyPara().setCh(new AddrU8(bArr[i2], i2));
            int i3 = i2 + 1;
            this.usr[i].getKeyPara().setOctave(new AddrI8(bArr[i3], i3));
            int i4 = i2 + 2;
            this.usr[i].getKeyPara().setTranspose(new AddrI8(bArr[i4], i4));
            int i5 = i2 + 3;
            this.usr[i].getKeyPara().setMin(new AddrU8(bArr[i5], i5));
            int i6 = i2 + 4;
            this.usr[i].getKeyPara().setMax(new AddrU8(bArr[i6], i6));
            for (int i7 = 0; i7 < 25; i7++) {
                int i8 = i2 + 5 + i7;
                this.usr[i].getKeyPara().getMackie()[i7] = new AddrU8(bArr[i8], i8);
            }
            for (int i9 = 0; i9 < 16; i9++) {
                int i10 = (i9 * 6) + i2;
                int i11 = i10 + 30;
                this.usr[i].getEncoderPara()[i9].setType(new AddrU8(bArr[i11], i11, 0, 3));
                int i12 = i10 + 31;
                this.usr[i].getEncoderPara()[i9].setCurve(new AddrU8(bArr[i12], i12));
                int i13 = i10 + 32;
                this.usr[i].getEncoderPara()[i9].setCh(new AddrU8(bArr[i13], i13));
                for (int i14 = 0; i14 < 3; i14++) {
                    int i15 = i10 + 33 + i14;
                    this.usr[i].getEncoderPara()[i9].getDat()[i14] = new AddrU8(bArr[i15], i15);
                }
            }
            for (int i16 = 0; i16 < 32; i16++) {
                int i17 = (i16 * 74) + i2;
                int i18 = i17 + 126;
                this.usr[i].getPadPara()[i16].setType(new AddrU8(bArr[i18], i18));
                int i19 = i17 + 127;
                this.usr[i].getPadPara()[i16].setCh(new AddrU8(bArr[i19], i19));
                for (int i20 = 0; i20 < 3; i20++) {
                    int i21 = i17 + 128 + i20;
                    this.usr[i].getPadPara()[i16].getDat()[i20] = new AddrU8(bArr[i21], i21);
                }
                for (int i22 = 0; i22 < 3; i22++) {
                    int i23 = i17 + 131 + i22;
                    this.usr[i].getPadPara()[i16].getColor()[i22] = new AddrU8(bArr[i23], i23);
                }
                int i24 = i17 + 134;
                this.usr[i].getPadPara()[i16].setLed(new AddrU8(bArr[i24], i24));
                int i25 = i17 + 135;
                this.usr[i].getPadPara()[i16].getSys().setLen(new AddrU8(bArr[i25], i25));
                for (int i26 = 0; i26 < 64; i26++) {
                    int i27 = i17 + 136 + i26;
                    this.usr[i].getPadPara()[i16].getSys().getVal()[i26] = new AddrU8(bArr[i27], i27);
                }
            }
            int i28 = i2 + 2494;
            this.usr[i].getPitchPara().setCh(new AddrU8(bArr[i28], i28));
            int i29 = i2 + 2495;
            this.usr[i].getModPara().setCh(new AddrU8(bArr[i29], i29));
            for (int i30 = 0; i30 < 3; i30++) {
                int i31 = i2 + 2496 + i30;
                this.usr[i].getModPara().getDat()[i30] = new AddrU8(bArr[i31], i31);
            }
            int i32 = i2 + 2499;
            this.usr[i].getPedalPara().setType(new AddrU8(bArr[i32], i32));
            int i33 = i2 + 2500;
            this.usr[i].getPedalPara().setCh(new AddrU8(bArr[i33], i33));
            for (int i34 = 0; i34 < 3; i34++) {
                int i35 = i2 + 2501 + i34;
                this.usr[i].getPedalPara().getDat()[i34] = new AddrU8(bArr[i35], i35);
            }
        }
    }
}
