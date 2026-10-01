package com.cubeSuite.entity.midikeyboard;

/* JADX INFO: loaded from: classes.dex */
public class Usr {
    private BtnPara[] btnPara;
    private EncoderPara[] encoderPara;
    private KeyPara keyPara;
    private ModPara modPara;
    private PadPara[] padPara;
    private PedalPara pedalPara;
    private PitchPara pitchPara;

    public Usr() {
        this.keyPara = new KeyPara();
        this.btnPara = new BtnPara[3];
        this.encoderPara = new EncoderPara[16];
        this.padPara = new PadPara[16];
        this.pitchPara = new PitchPara();
        this.modPara = new ModPara();
        this.pedalPara = new PedalPara();
        int i = 0;
        int i2 = 0;
        while (true) {
            BtnPara[] btnParaArr = this.btnPara;
            if (i2 >= btnParaArr.length) {
                break;
            }
            btnParaArr[i2] = new BtnPara();
            i2++;
        }
        int i3 = 0;
        while (true) {
            EncoderPara[] encoderParaArr = this.encoderPara;
            if (i3 >= encoderParaArr.length) {
                break;
            }
            encoderParaArr[i3] = new EncoderPara();
            i3++;
        }
        while (true) {
            PadPara[] padParaArr = this.padPara;
            if (i >= padParaArr.length) {
                return;
            }
            padParaArr[i] = new PadPara();
            i++;
        }
    }

    public Usr(KeyPara keyPara, BtnPara[] btnParaArr, EncoderPara[] encoderParaArr, PadPara[] padParaArr, PitchPara pitchPara, ModPara modPara, PedalPara pedalPara) {
        this.keyPara = new KeyPara();
        this.btnPara = new BtnPara[3];
        this.encoderPara = new EncoderPara[16];
        this.padPara = new PadPara[16];
        this.pitchPara = new PitchPara();
        this.modPara = new ModPara();
        new PedalPara();
        this.keyPara = keyPara;
        this.btnPara = btnParaArr;
        this.encoderPara = encoderParaArr;
        this.padPara = padParaArr;
        this.pitchPara = pitchPara;
        this.modPara = modPara;
        this.pedalPara = pedalPara;
    }

    public KeyPara getKeyPara() {
        return this.keyPara;
    }

    public void setKeyPara(KeyPara keyPara) {
        this.keyPara = keyPara;
    }

    public BtnPara[] getBtnPara() {
        return this.btnPara;
    }

    public void setBtnPara(BtnPara[] btnParaArr) {
        this.btnPara = btnParaArr;
    }

    public EncoderPara[] getEncoderPara() {
        return this.encoderPara;
    }

    public void setEncoderPara(EncoderPara[] encoderParaArr) {
        this.encoderPara = encoderParaArr;
    }

    public PadPara[] getPadPara() {
        return this.padPara;
    }

    public void setPadPara(PadPara[] padParaArr) {
        this.padPara = padParaArr;
    }

    public PitchPara getPitchPara() {
        return this.pitchPara;
    }

    public void setPitchPara(PitchPara pitchPara) {
        this.pitchPara = pitchPara;
    }

    public ModPara getModPara() {
        return this.modPara;
    }

    public void setModPara(ModPara modPara) {
        this.modPara = modPara;
    }

    public PedalPara getPedalPara() {
        return this.pedalPara;
    }

    public void setPedalPara(PedalPara pedalPara) {
        this.pedalPara = pedalPara;
    }
}
