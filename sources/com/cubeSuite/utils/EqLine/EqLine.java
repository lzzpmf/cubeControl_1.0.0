package com.cubeSuite.utils.EqLine;

import androidx.core.view.accessibility.AccessibilityNodeInfoCompat;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class EqLine {
    private final double DB_STEP = 0.10000000149011612d;
    private final double Q_STEP = 0.10000000149011612d;
    private final int B0_IDX = 0;
    private final int B1_IDX = 1;
    private final int B2_IDX = 2;
    private final int A1_IDX = 3;
    private final int A2_IDX = 4;
    private final double shelf_default_Q = 0.7070000171661377d;
    private final double M_PI = 3.1415927410125732d;
    public EqResp m_er = new EqResp();

    enum EqRespCmd {
        GET_PARA,
        GET_FREQS,
        GET_RESPS,
        SET_CFG
    }

    public class Teq {
        public List<Integer> enable;
        public int eq_bands;
        public int freq_bins;
        public int fs;
        public int hphz;
        public int hsdb;
        public int hshz;
        public int lphz;
        public int lsdb;
        public int lshz;
        public List<Integer> peakhz;
        public List<Integer> peakq;
        public List<Integer> peakqdb;

        public Teq() {
        }
    }

    public class EqResp {
        List<Double> freqs;
        List<Double> iirs;
        public Teq para;
        public List<Double> respf;

        public EqResp() {
            this.para = EqLine.this.new Teq();
        }
    }

    public void shelf_high_config(List<Double> list, int i, double d, double d2, double d3) {
        double dTan = Math.tan(((6.2831854820251465d * d) / d3) / 2.0d);
        double d4 = dTan * dTan;
        double dSqrt = Math.sqrt(Math.pow(10.0d, 0.05000000074505806d * d2));
        double d5 = dSqrt * d4;
        double dSqrt2 = dTan * 1.4144271226587684d * Math.sqrt(dSqrt);
        double d6 = 1.0d / ((d5 + dSqrt2) + 1.0d);
        double d7 = (d5 - 1.0d) * 2.0d;
        double d8 = (dSqrt + dSqrt2 + d4) * dSqrt;
        double d9 = (d4 - dSqrt) * 2.0d * dSqrt;
        list.set(i, Double.valueOf(d8 * d6));
        list.set(i + 1, Double.valueOf(d9 * d6));
        list.set(i + 2, Double.valueOf(dSqrt * ((dSqrt - dSqrt2) + d4) * d6));
        list.set(i + 3, Double.valueOf(d7 * d6));
        list.set(i + 4, Double.valueOf(((d5 - dSqrt2) + 1.0d) * d6));
    }

    public void shelf_low_config(List<Double> list, int i, double d, double d2, double d3) {
        double dTan = Math.tan(((6.2831854820251465d * d) / d3) / 2.0d);
        double d4 = dTan * dTan;
        double dSqrt = Math.sqrt(Math.pow(10.0d, 0.05000000074505806d * d2));
        double dSqrt2 = Math.sqrt(dSqrt);
        double d5 = (1.0d / dSqrt) * d4;
        double d6 = dTan * 1.4144271226587684d;
        double d7 = (1.0d / dSqrt2) * d6;
        double d8 = 1.0d / ((d5 + d7) + 1.0d);
        double d9 = dSqrt * d4;
        double d10 = d6 * dSqrt2;
        double d11 = d9 + d10 + 1.0d;
        double d12 = (d9 - 1.0d) * 2.0d;
        list.set(i + 3, Double.valueOf((d5 - 1.0d) * 2.0d * d8));
        list.set(i + 4, Double.valueOf(((d5 - d7) + 1.0d) * d8));
        list.set(i, Double.valueOf(d11 * d8));
        list.set(i + 1, Double.valueOf(d12 * d8));
        list.set(i + 2, Double.valueOf(((d9 - d10) + 1.0d) * d8));
    }

    public void peaknotch_config(List<Double> list, int i, double d, double d2, double d3, double d4) {
        double d5 = (d * 6.2831854820251465d) / d4;
        double dPow = Math.pow(10.0d, d2 * 0.05000000074505806d);
        double dTan = Math.tan((d5 / d3) / 2.0d);
        double dCos = Math.cos(d5);
        double dSqrt = Math.sqrt(dPow);
        double d6 = 1.0d / (dSqrt + dTan);
        double d7 = (-2.0d) * dSqrt * dCos;
        double d8 = dSqrt - dTan;
        double d9 = dPow * dTan;
        double d10 = dSqrt + d9;
        double d11 = d7 * d6;
        list.set(i + 3, Double.valueOf(d11));
        list.set(i + 4, Double.valueOf(d8 * d6));
        list.set(i, Double.valueOf(d10 * d6));
        list.set(i + 1, Double.valueOf(d11));
        list.set(i + 2, Double.valueOf((dSqrt - d9) * d6));
    }

    public void lowpass(List<Double> list, int i, double d, double d2) {
        double dTan = Math.tan(((6.2831854820251465d * d) / d2) * 0.5d);
        double d3 = dTan * dTan;
        double d4 = dTan * 1.4144271226587684d;
        double d5 = d3 * 2.0d;
        double d6 = 1.0d / ((d4 + 1.0d) + d3);
        list.set(i + 3, Double.valueOf((d3 - 1.0d) * 2.0d * d6));
        list.set(i + 4, Double.valueOf(((1.0d - d4) + d3) * d6));
        double d7 = d3 * d6;
        list.set(i, Double.valueOf(d7));
        list.set(i + 1, Double.valueOf(d5 * d6));
        list.set(i + 2, Double.valueOf(d7));
    }

    public void highpass(List<Double> list, int i, double d, double d2) {
        double dTan = Math.tan(((d * 6.2831854820251465d) / d2) * 0.5d);
        double d3 = dTan * dTan;
        double d4 = dTan * 1.4144271226587684d;
        double d5 = 1.0d / ((d4 + 1.0d) + d3);
        list.set(i + 3, Double.valueOf((d3 - 1.0d) * 2.0d * d5));
        list.set(i + 4, Double.valueOf(((1.0d - d4) + d3) * d5));
        double d6 = 1.0d * d5;
        list.set(i, Double.valueOf(d6));
        list.set(i + 1, Double.valueOf(d5 * (-2.0d)));
        list.set(i + 2, Double.valueOf(d6));
    }

    public void eq_resp_config() {
        EqResp eqResp = this.m_er;
        if (eqResp != null) {
            highpass(eqResp.iirs, 0, this.m_er.para.hphz, this.m_er.para.fs);
            shelf_low_config(this.m_er.iirs, 5, this.m_er.para.lshz, ((double) this.m_er.para.lsdb) * 0.10000000149011612d, this.m_er.para.fs);
            int i = 10;
            for (int i2 = 0; i2 < this.m_er.para.eq_bands; i2++) {
                peaknotch_config(this.m_er.iirs, i, this.m_er.para.peakhz.get(i2).intValue(), ((double) this.m_er.para.peakqdb.get(i2).intValue()) * 0.10000000149011612d, ((double) this.m_er.para.peakq.get(i2).intValue()) * 0.10000000149011612d, this.m_er.para.fs);
                i += 5;
            }
            shelf_high_config(this.m_er.iirs, i, this.m_er.para.hshz, ((double) this.m_er.para.hsdb) * 0.10000000149011612d, this.m_er.para.fs);
            lowpass(this.m_er.iirs, i + 5, this.m_er.para.lphz, this.m_er.para.fs);
            eq_resp_process();
        }
    }

    public void eq_resp_init(int i, int i2, int i3) {
        this.m_er.para.fs = i3;
        this.m_er.para.eq_bands = i;
        this.m_er.para.freq_bins = i2;
        this.m_er.iirs = new ArrayList((this.m_er.para.eq_bands + 4) * 5);
        for (int i4 = 0; i4 < (this.m_er.para.eq_bands + 4) * 5; i4++) {
            this.m_er.iirs.add(Double.valueOf(0.0d));
        }
        this.m_er.freqs = new ArrayList(this.m_er.para.freq_bins);
        this.m_er.respf = new ArrayList(this.m_er.para.freq_bins);
        for (int i5 = 0; i5 < this.m_er.para.freq_bins; i5++) {
            this.m_er.freqs.add(Double.valueOf(0.0d));
            this.m_er.respf.add(Double.valueOf(0.0d));
        }
        this.m_er.para.enable = new ArrayList(this.m_er.para.eq_bands + 4);
        for (int i6 = 0; i6 < this.m_er.para.eq_bands + 4; i6++) {
            this.m_er.para.enable.add(0);
        }
        this.m_er.para.peakq = new ArrayList(this.m_er.para.eq_bands);
        this.m_er.para.peakhz = new ArrayList(this.m_er.para.eq_bands);
        this.m_er.para.peakqdb = new ArrayList(this.m_er.para.eq_bands);
        for (int i7 = 0; i7 < this.m_er.para.eq_bands; i7++) {
            this.m_er.para.peakq.add(0);
            this.m_er.para.peakhz.add(0);
            this.m_er.para.peakqdb.add(0);
        }
        double dPow = Math.pow(1000.0d, 1.0f / this.m_er.para.freq_bins);
        this.m_er.freqs.set(0, Double.valueOf(20.0d));
        for (int i8 = 1; i8 < this.m_er.para.freq_bins; i8++) {
            this.m_er.freqs.set(i8, Double.valueOf(this.m_er.freqs.get(i8 - 1).doubleValue() * dPow));
        }
        this.m_er.para.hphz = 20;
        this.m_er.para.lshz = 50;
        this.m_er.para.lsdb = 0;
        this.m_er.para.enable.set(0, 1);
        this.m_er.para.enable.set(1, 1);
        for (int i9 = 0; i9 < this.m_er.para.eq_bands; i9++) {
            this.m_er.para.peakhz.set(i9, 1000);
            this.m_er.para.peakqdb.set(i9, 0);
            this.m_er.para.peakq.set(i9, 7);
        }
        this.m_er.para.hshz = 16000;
        this.m_er.para.hsdb = 0;
        this.m_er.para.lphz = AccessibilityNodeInfoCompat.EXTRA_DATA_TEXT_CHARACTER_LOCATION_ARG_MAX_LENGTH;
        this.m_er.para.enable.set(this.m_er.para.eq_bands + 2, 1);
        this.m_er.para.enable.set(this.m_er.para.eq_bands + 3, 1);
        eq_resp_config();
    }

    public int eq_resp_process() {
        EqResp eqResp = this.m_er;
        if (eqResp == null) {
            return -1;
        }
        double d = 6.2831854820251465d / ((double) eqResp.para.fs);
        for (int i = 0; i < this.m_er.para.freq_bins; i++) {
            double dDoubleValue = this.m_er.freqs.get(i).doubleValue() * d;
            double dSqrt = 1.0d;
            int i2 = 0;
            for (int i3 = 0; i3 < this.m_er.para.eq_bands + 4; i3++) {
                if (this.m_er.para.enable.get(i3).intValue() != 0) {
                    double dDoubleValue2 = this.m_er.iirs.get(i2).doubleValue();
                    double dDoubleValue3 = this.m_er.iirs.get(i2 + 1).doubleValue();
                    double dDoubleValue4 = this.m_er.iirs.get(i2 + 2).doubleValue();
                    double dDoubleValue5 = this.m_er.iirs.get(i2 + 3).doubleValue();
                    double dDoubleValue6 = this.m_er.iirs.get(i2 + 4).doubleValue();
                    double d2 = 2.0d * dDoubleValue;
                    double dCos = dDoubleValue2 + (Math.cos(dDoubleValue) * dDoubleValue3) + (Math.cos(d2) * dDoubleValue4);
                    double dSin = (dDoubleValue3 * Math.sin(dDoubleValue)) + (dDoubleValue4 * Math.sin(d2));
                    double dCos2 = (Math.cos(dDoubleValue) * dDoubleValue5) + 1.0d + (Math.cos(d2) * dDoubleValue6);
                    double dSin2 = (dDoubleValue5 * Math.sin(dDoubleValue)) + (dDoubleValue6 * Math.sin(d2));
                    dSqrt *= Math.sqrt(((dCos * dCos) + (dSin * dSin)) / ((dCos2 * dCos2) + (dSin2 * dSin2)));
                }
                i2 += 5;
            }
            this.m_er.respf.set(i, Double.valueOf(Math.log10(dSqrt) * 20.0d));
        }
        return 0;
    }
}
