package com.cubeSuite.entity.smk25Mini;

import com.cubeSuite.entity.midikeyboard.BtnPara;
import com.cubeSuite.entity.midikeyboard.EncoderPara;
import com.cubeSuite.entity.midikeyboard.PedalPara;

/* JADX INFO: loaded from: classes.dex */
public class Smk25MiniUsr {
    private BtnPara btnPara;
    private EncoderPara encoderPara;
    private Smk25MiniKeyPara keyPara;
    private Smk25MiniMod modPara;
    private PedalPara pedalPara;
    private Smk25MiniPitch pitchPara;

    public Smk25MiniUsr() {
        this.keyPara = new Smk25MiniKeyPara();
        this.btnPara = new BtnPara();
        this.encoderPara = new EncoderPara();
        this.pitchPara = new Smk25MiniPitch();
        this.modPara = new Smk25MiniMod();
        this.pedalPara = new PedalPara();
    }

    public Smk25MiniUsr(Smk25MiniKeyPara smk25MiniKeyPara, BtnPara btnPara, EncoderPara encoderPara, Smk25MiniPitch smk25MiniPitch, Smk25MiniMod smk25MiniMod, PedalPara pedalPara) {
        this.keyPara = new Smk25MiniKeyPara();
        this.btnPara = new BtnPara();
        this.encoderPara = new EncoderPara();
        this.pitchPara = new Smk25MiniPitch();
        this.modPara = new Smk25MiniMod();
        new PedalPara();
        this.keyPara = smk25MiniKeyPara;
        this.btnPara = btnPara;
        this.encoderPara = encoderPara;
        this.pitchPara = smk25MiniPitch;
        this.modPara = smk25MiniMod;
        this.pedalPara = pedalPara;
    }

    public Smk25MiniKeyPara getKeyPara() {
        return this.keyPara;
    }

    public void setKeyPara(Smk25MiniKeyPara smk25MiniKeyPara) {
        this.keyPara = smk25MiniKeyPara;
    }

    public BtnPara getBtnPara() {
        return this.btnPara;
    }

    public void setBtnPara(BtnPara btnPara) {
        this.btnPara = btnPara;
    }

    public EncoderPara getEncoderPara() {
        return this.encoderPara;
    }

    public void setEncoderPara(EncoderPara encoderPara) {
        this.encoderPara = encoderPara;
    }

    public Smk25MiniPitch getPitchPara() {
        return this.pitchPara;
    }

    public void setPitchPara(Smk25MiniPitch smk25MiniPitch) {
        this.pitchPara = smk25MiniPitch;
    }

    public Smk25MiniMod getModPara() {
        return this.modPara;
    }

    public void setModPara(Smk25MiniMod smk25MiniMod) {
        this.modPara = smk25MiniMod;
    }

    public PedalPara getPedalPara() {
        return this.pedalPara;
    }

    public void setPedalPara(PedalPara pedalPara) {
        this.pedalPara = pedalPara;
    }
}
