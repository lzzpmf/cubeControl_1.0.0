package com.cubeSuite.entity.midikeyboard;

import com.cubeSuite.communication.Communication;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class MidikeyboardEntry {
    public final int GLOB_SIZE = 19;
    public final int USR_SIZE = 474;
    public final int USR_ARR_SIZE = 3792;
    private Glob glob = new Glob();
    private Usr[] usr = new Usr[8];

    public void notifyChange() {
    }

    public MidikeyboardEntry() {
        int i = 0;
        while (true) {
            Usr[] usrArr = this.usr;
            if (i >= usrArr.length) {
                return;
            }
            usrArr[i] = new Usr();
            i++;
        }
    }

    public Glob getGlob() {
        return this.glob;
    }

    public void setGlob(Glob glob) {
        this.glob = glob;
    }

    public Usr[] getUsr() {
        return this.usr;
    }

    public void setUsr(Usr[] usrArr) {
        this.usr = usrArr;
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
        if (bArr.length < 3792) {
            return;
        }
        for (int i = 0; i < 8; i++) {
            int i2 = i * 474;
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
            for (int i9 = 0; i9 < 3; i9++) {
                int i10 = (i9 * 70) + i2;
                int i11 = i10 + 30;
                this.usr[i].getBtnPara()[i9].setType(new AddrU8(bArr[i11], i11));
                int i12 = i10 + 31;
                this.usr[i].getBtnPara()[i9].setCh(new AddrU8(bArr[i12], i12));
                for (int i13 = 0; i13 < 3; i13++) {
                    int i14 = i10 + 32 + i13;
                    this.usr[i].getBtnPara()[i9].getDat()[i13] = new AddrU8(bArr[i14], i14);
                }
                int i15 = i10 + 35;
                this.usr[i].getBtnPara()[i9].getSys().setLen(new AddrU8(bArr[i15], i15));
                for (int i16 = 0; i16 < 64; i16++) {
                    int i17 = i10 + 36 + i16;
                    this.usr[i].getBtnPara()[i9].getSys().getVal()[i16] = new AddrU8(bArr[i17], i17);
                }
            }
            for (int i18 = 0; i18 < 16; i18++) {
                int i19 = (i18 * 6) + i2;
                int i20 = i19 + 240;
                this.usr[i].getEncoderPara()[i18].setType(new AddrU8(bArr[i20], i20));
                int i21 = i19 + 241;
                this.usr[i].getEncoderPara()[i18].setCurve(new AddrU8(bArr[i21], i21));
                int i22 = i19 + 242;
                this.usr[i].getEncoderPara()[i18].setCh(new AddrU8(bArr[i22], i22));
                for (int i23 = 0; i23 < 3; i23++) {
                    int i24 = i19 + 243 + i23;
                    this.usr[i].getEncoderPara()[i18].getDat()[i23] = new AddrU8(bArr[i24], i24);
                }
            }
            for (int i25 = 0; i25 < 16; i25++) {
                int i26 = (i25 * 8) + i2;
                int i27 = i26 + 336;
                this.usr[i].getPadPara()[i25].setType(new AddrU8(bArr[i27], i27));
                int i28 = i26 + 337;
                this.usr[i].getPadPara()[i25].setCh(new AddrU8(bArr[i28], i28));
                for (int i29 = 0; i29 < 3; i29++) {
                    int i30 = i26 + 338 + i29;
                    this.usr[i].getPadPara()[i25].getDat()[i29] = new AddrU8(bArr[i30], i30);
                }
                for (int i31 = 0; i31 < 3; i31++) {
                    int i32 = i26 + 341 + i31;
                    this.usr[i].getPadPara()[i25].getColor()[i31] = new AddrU8(bArr[i32], i32);
                }
            }
            int i33 = i2 + 464;
            this.usr[i].getPitchPara().setCh(new AddrU8(bArr[i33], i33));
            int i34 = i2 + 465;
            this.usr[i].getModPara().setCh(new AddrU8(bArr[i34], i34));
            for (int i35 = 0; i35 < 3; i35++) {
                int i36 = i2 + 466 + i35;
                this.usr[i].getModPara().getDat()[i35] = new AddrU8(bArr[i36], i36);
            }
            int i37 = i2 + 469;
            this.usr[i].getPedalPara().setType(new AddrU8(bArr[i37], i37));
            int i38 = i2 + 470;
            this.usr[i].getPedalPara().setCh(new AddrU8(bArr[i38], i38));
            for (int i39 = 0; i39 < 3; i39++) {
                int i40 = i2 + 471 + i39;
                this.usr[i].getPedalPara().getDat()[i39] = new AddrU8(bArr[i40], i40);
            }
        }
    }
}
