package com.cubeSuite.entity.SmcMixerEntry;

/* JADX INFO: loaded from: classes.dex */
public class SmcMixerUsr {
    public SmcBtnPara[] track = new SmcBtnPara[32];
    public SmcBtnPara[] globe = new SmcBtnPara[11];
    public SmcEncoderPara[] knob = new SmcEncoderPara[8];
    public SmcSpPara[] slider = new SmcSpPara[8];

    public SmcMixerUsr() {
        int i = 0;
        int i2 = 0;
        while (true) {
            SmcBtnPara[] smcBtnParaArr = this.track;
            if (i2 >= smcBtnParaArr.length) {
                break;
            }
            smcBtnParaArr[i2] = new SmcBtnPara();
            i2++;
        }
        int i3 = 0;
        while (true) {
            SmcBtnPara[] smcBtnParaArr2 = this.globe;
            if (i3 >= smcBtnParaArr2.length) {
                break;
            }
            smcBtnParaArr2[i3] = new SmcBtnPara();
            i3++;
        }
        int i4 = 0;
        while (true) {
            SmcEncoderPara[] smcEncoderParaArr = this.knob;
            if (i4 >= smcEncoderParaArr.length) {
                break;
            }
            smcEncoderParaArr[i4] = new SmcEncoderPara();
            i4++;
        }
        while (true) {
            SmcSpPara[] smcSpParaArr = this.slider;
            if (i >= smcSpParaArr.length) {
                return;
            }
            smcSpParaArr[i] = new SmcSpPara();
            i++;
        }
    }
}
