package com.cubeSuite.entity.smk25II;

import com.cubeSuite.entity.midikeyboard.EncoderPara;
import com.cubeSuite.entity.midikeyboard.KeyPara;
import com.cubeSuite.entity.midikeyboard.ModPara;
import com.cubeSuite.entity.midikeyboard.PedalPara;
import com.cubeSuite.entity.midikeyboard.PitchPara;

/* JADX INFO: loaded from: classes.dex */
public class Smk25IIUsr {
    private KeyPara keyPara = new KeyPara();
    private EncoderPara[] encoderPara = new EncoderPara[16];
    private Smk25IIPadPara[] padPara = new Smk25IIPadPara[32];
    private PitchPara pitchPara = new PitchPara();
    private ModPara modPara = new ModPara();
    private PedalPara pedalPara = new PedalPara();

    public Smk25IIUsr() {
        int i = 0;
        int i2 = 0;
        while (true) {
            EncoderPara[] encoderParaArr = this.encoderPara;
            if (i2 >= encoderParaArr.length) {
                break;
            }
            encoderParaArr[i2] = new EncoderPara();
            i2++;
        }
        while (true) {
            Smk25IIPadPara[] smk25IIPadParaArr = this.padPara;
            if (i >= smk25IIPadParaArr.length) {
                return;
            }
            smk25IIPadParaArr[i] = new Smk25IIPadPara();
            i++;
        }
    }

    public KeyPara getKeyPara() {
        return this.keyPara;
    }

    public void setKeyPara(KeyPara keyPara) {
        this.keyPara = keyPara;
    }

    public EncoderPara[] getEncoderPara() {
        return this.encoderPara;
    }

    public void setEncoderPara(EncoderPara[] encoderParaArr) {
        this.encoderPara = encoderParaArr;
    }

    public Smk25IIPadPara[] getPadPara() {
        return this.padPara;
    }

    public void setPadPara(Smk25IIPadPara[] smk25IIPadParaArr) {
        this.padPara = smk25IIPadParaArr;
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
