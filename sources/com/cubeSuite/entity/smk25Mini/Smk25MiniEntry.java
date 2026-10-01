package com.cubeSuite.entity.smk25Mini;

import com.cubeSuite.communication.Communication;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniEntry {
    public final int GLOB_SIZE = 16;
    public final int USR_SIZE = 93;
    public final int USR_ARR_SIZE = 93;
    private Smk25MiniGlob glob = new Smk25MiniGlob();
    private Smk25MiniUsr usr = new Smk25MiniUsr();

    public void notifyChange() {
    }

    public Smk25MiniGlob getGlob() {
        return this.glob;
    }

    public void setGlob(Smk25MiniGlob smk25MiniGlob) {
        this.glob = smk25MiniGlob;
    }

    public Smk25MiniUsr getUsr() {
        return this.usr;
    }

    public void setUsr(Smk25MiniUsr smk25MiniUsr) {
        this.usr = smk25MiniUsr;
    }

    public void setGlobData(byte[] bArr) {
        if (bArr.length != 16) {
            return;
        }
        this.glob.setKeyCurve(new AddrU8(bArr[0], 0L));
        this.glob.getShift().setMajor(new AddrU8(bArr[1], 1L));
        this.glob.getShift().setScale(new AddrU8(bArr[2], 2L));
        this.glob.getShift().setChord(new AddrU8(bArr[3], 3L));
        this.glob.getArp().setMode(new AddrU8(bArr[4], 4L));
        this.glob.getArp().setOct(new AddrU8(bArr[5], 5L));
        this.glob.getArp().setGate(new AddrU8(bArr[6], 6L));
        this.glob.getArp().setSwing(new AddrU8(bArr[7], 7L));
        this.glob.getArp().setLatch(new AddrU8(bArr[8], 8L));
        this.glob.getArp().setTime(new AddrU8(bArr[9], 9L));
        this.glob.getArp().setSync(new AddrU8(bArr[10], 10L));
        this.glob.getArp().setTempo(new AddrU16(Communication.u8ArrToU16(bArr[12], bArr[13]), 12L));
        this.glob.setInputMode(new AddrU8(bArr[14], 14L));
    }

    public void setUsrData(byte[] bArr) {
        if (bArr.length < 93) {
            return;
        }
        this.usr.getKeyPara().setCh(new AddrU8(bArr[0], 0L));
        this.usr.getKeyPara().setOctave(new AddrI8(bArr[1], 1L));
        this.usr.getKeyPara().setTranspose(new AddrI8(bArr[2], 2L));
        this.usr.getKeyPara().setMin(new AddrU8(bArr[3], 3L));
        this.usr.getKeyPara().setMax(new AddrU8(bArr[4], 4L));
        this.usr.getBtnPara().setType(new AddrU8(bArr[5], 5L));
        this.usr.getBtnPara().setCh(new AddrU8(bArr[6], 6L));
        for (int i = 0; i < 3; i++) {
            int i2 = i + 7;
            this.usr.getBtnPara().getDat()[i] = new AddrU8(bArr[i2], i2);
        }
        this.usr.getBtnPara().getSys().setLen(new AddrU8(bArr[10], 10L));
        for (int i3 = 0; i3 < 64; i3++) {
            int i4 = i3 + 11;
            this.usr.getBtnPara().getSys().getVal()[i3] = new AddrU8(bArr[i4], i4);
        }
        this.usr.getEncoderPara().setType(new AddrU8(bArr[75], 75L));
        this.usr.getEncoderPara().setCurve(new AddrU8(bArr[76], 76L));
        this.usr.getEncoderPara().setCh(new AddrU8(bArr[77], 77L));
        for (int i5 = 0; i5 < 3; i5++) {
            int i6 = i5 + 78;
            this.usr.getEncoderPara().getDat()[i5] = new AddrU8(bArr[i6], i6);
        }
        this.usr.getPitchPara().setCh(new AddrU8(bArr[81], 81L));
        this.usr.getPitchPara().setSpeed(new AddrU8(bArr[82], 82L));
        this.usr.getModPara().setCh(new AddrU8(bArr[83], 83L));
        this.usr.getModPara().setSpeed(new AddrU8(bArr[84], 84L));
        for (int i7 = 0; i7 < 3; i7++) {
            int i8 = i7 + 85;
            this.usr.getModPara().getDat()[i7] = new AddrU8(bArr[i8], i8);
        }
        this.usr.getPedalPara().setType(new AddrU8(bArr[88], 88L));
        this.usr.getPedalPara().setCh(new AddrU8(bArr[89], 89L));
        for (int i9 = 0; i9 < 3; i9++) {
            int i10 = i9 + 90;
            this.usr.getPedalPara().getDat()[i9] = new AddrU8(bArr[i10], i10);
        }
    }
}
