package com.cubeSuite.utils;

import com.cubeSuite.communication.Communication;
import java.util.ArrayList;
import java.util.List;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class WaveUtils {
    private int irLen = 12288;
    private final int wordLen = 2;
    private final int dwordLen = 4;
    private final int WAVE_FORMAT_PCM = 1;
    private WaveFormat waveFormat = new WaveFormat();
    private WaveFormat targetWaveFormat = new WaveFormat();
    private WaveChunkHead waveChunkHead = new WaveChunkHead();
    private List<Byte> wavData = new ArrayList(this.irLen);
    private List<Long> mono24BitWaveData = new ArrayList(this.irLen / 3);
    private final float FACTOR_FOR_24_BIT_SAMPLE = 1.1920929E-7f;
    private final float FACTOR_FOR_16_BIT_SAMPLE = 3.051851E-5f;

    private long btToLong(byte b) {
        return b & UByte.MAX_VALUE;
    }

    static class WaveFormat {
        public int wFormatTag = 1;
        public int channels = 1;
        public long samplesPerSec = 44100;
        public long avgBytesPerSec = 132300;
        public int blockAlign = 3;
        public int wbitsPerSample = 24;

        WaveFormat() {
        }
    }

    static class WaveChunkHead {
        public String dwTag = "";
        public long dwChunkLen = 0;

        WaveChunkHead() {
        }
    }

    public void setFormat(WaveFormat waveFormat) {
        this.targetWaveFormat = waveFormat;
        long j = this.waveFormat.samplesPerSec;
        long j2 = waveFormat.samplesPerSec;
        int i = this.waveFormat.channels;
        int i2 = waveFormat.channels;
    }

    public List<Byte> outWaveData() {
        List<Byte> listLinearResampling = linearResampling(1.0d);
        ArrayList arrayList = new ArrayList(listLinearResampling.size() + 36);
        Communication.strAddU8Arr(arrayList, "RIFF");
        Communication.u32AddU8Arr(arrayList, ((long) listLinearResampling.size()) + 36);
        Communication.strAddU8Arr(arrayList, "WAVE");
        Communication.strAddU8Arr(arrayList, "fmt ");
        Communication.u32AddU8Arr(arrayList, 16L);
        Communication.u16AddU8Arr(arrayList, this.targetWaveFormat.wFormatTag);
        Communication.u16AddU8Arr(arrayList, this.targetWaveFormat.channels);
        Communication.u32AddU8Arr(arrayList, this.targetWaveFormat.samplesPerSec);
        Communication.u32AddU8Arr(arrayList, this.targetWaveFormat.avgBytesPerSec);
        Communication.u16AddU8Arr(arrayList, this.targetWaveFormat.blockAlign);
        Communication.u16AddU8Arr(arrayList, this.targetWaveFormat.wbitsPerSample);
        Communication.strAddU8Arr(arrayList, "data");
        Communication.u32AddU8Arr(arrayList, listLinearResampling.size());
        arrayList.addAll(listLinearResampling);
        return arrayList;
    }

    public List<Byte> linearResampling(double d) {
        conversionMono24Bit();
        ArrayList arrayList = new ArrayList(this.irLen);
        double d2 = this.waveFormat.samplesPerSec / this.targetWaveFormat.samplesPerSec;
        for (double d3 = 0.0d; d3 < this.mono24BitWaveData.size() - 1; d3 += d2) {
            int i = (int) d3;
            double d4 = d3 - ((double) i);
            Communication.u24AddU8Arr(arrayList, (long) (((this.mono24BitWaveData.get(i).longValue() * (1.0d - d4)) + (this.mono24BitWaveData.get(i + 1).longValue() * d4)) * d));
        }
        int size = arrayList.size();
        int i2 = this.irLen;
        if (size < i2) {
            for (int size2 = arrayList.size(); size2 < this.irLen; size2++) {
                arrayList.add((byte) 0);
            }
            return arrayList;
        }
        return arrayList.subList(0, i2);
    }

    public boolean load(List<Byte> list, int i) {
        this.irLen = i;
        String strU8ArrToString = Communication.u8ArrToString(list.subList(0, 4));
        int i2 = 12;
        String strU8ArrToString2 = Communication.u8ArrToString(list.subList(8, 12));
        if (strU8ArrToString.equals("RIFF") && strU8ArrToString2.equals("WAVE")) {
            boolean z = false;
            while (i2 < list.size() - 4) {
                int i3 = i2 + 4;
                this.waveChunkHead.dwTag = Communication.u8ArrToString(list.subList(i2, i3));
                int i4 = i2 + 8;
                this.waveChunkHead.dwChunkLen = Communication.u8ArrToU32(list.subList(i3, i4));
                if (this.waveChunkHead.dwTag.equals("data")) {
                    if (!z) {
                        return z;
                    }
                    this.wavData = list.subList(i4, (int) (((long) i4) + this.waveChunkHead.dwChunkLen));
                    return true;
                }
                if (this.waveChunkHead.dwTag.equals("fmt ")) {
                    this.waveFormat.wFormatTag = Communication.u8ArrToU16(list.get(i4).byteValue(), list.get(i2 + 9).byteValue());
                    this.waveFormat.channels = Communication.u8ArrToU16(list.get(i2 + 10).byteValue(), list.get(i2 + 11).byteValue());
                    int i5 = i2 + 16;
                    this.waveFormat.samplesPerSec = Communication.u8ArrToU32(list.subList(i2 + 12, i5));
                    int i6 = i2 + 20;
                    this.waveFormat.avgBytesPerSec = Communication.u8ArrToU32(list.subList(i5, i6));
                    this.waveFormat.blockAlign = Communication.u8ArrToU16(list.get(i6).byteValue(), list.get(i2 + 21).byteValue());
                    this.waveFormat.wbitsPerSample = Communication.u8ArrToU16(list.get(i2 + 22).byteValue(), list.get(i2 + 23).byteValue());
                    i2 = i4 + ((int) this.waveChunkHead.dwChunkLen);
                    if (1 != this.waveFormat.wFormatTag) {
                        return false;
                    }
                    z = true;
                } else {
                    i2 = i4 + ((int) this.waveChunkHead.dwChunkLen);
                }
            }
        }
        return false;
    }

    public void conversionMono24Bit() {
        int iMin = Math.min(this.wavData.size() / this.waveFormat.blockAlign, this.irLen);
        this.mono24BitWaveData.clear();
        int i = this.waveFormat.blockAlign;
        int i2 = 0;
        if (i == 2) {
            while (i2 < iMin) {
                int i3 = i2 * 2;
                long jBtToLong = (btToLong(this.wavData.get(i3 + 1).byteValue()) << 8) | btToLong(this.wavData.get(i3).byteValue());
                if ((jBtToLong & 32768) != 0) {
                    jBtToLong |= -65536;
                }
                this.mono24BitWaveData.add(Long.valueOf(jBtToLong));
                i2++;
            }
        } else if (i == 3) {
            while (i2 < iMin) {
                int i4 = i2 * 3;
                long jBtToLong2 = (btToLong(this.wavData.get(i4 + 2).byteValue()) << 16) | btToLong(this.wavData.get(i4).byteValue()) | (btToLong(this.wavData.get(i4 + 1).byteValue()) << 8);
                if ((jBtToLong2 & 8388608) != 0) {
                    jBtToLong2 |= -16777216;
                }
                this.mono24BitWaveData.add(Long.valueOf(jBtToLong2));
                i2++;
            }
        } else if (i == 4) {
            while (i2 < iMin) {
                int i5 = i2 * 4;
                long jBtToLong3 = (btToLong(this.wavData.get(i5 + 1).byteValue()) << 8) | btToLong(this.wavData.get(i5).byteValue());
                if ((jBtToLong3 & 32768) != 0) {
                    jBtToLong3 |= -65536;
                }
                this.mono24BitWaveData.add(Long.valueOf(jBtToLong3));
                i2++;
            }
        } else if (i == 6) {
            while (i2 < iMin) {
                int i6 = i2 * 6;
                long jBtToLong4 = (btToLong(this.wavData.get(i6 + 2).byteValue()) << 16) | btToLong(this.wavData.get(i6).byteValue()) | (btToLong(this.wavData.get(i6 + 1).byteValue()) << 8);
                if ((jBtToLong4 & 8388608) != 0) {
                    jBtToLong4 |= -16777216;
                }
                this.mono24BitWaveData.add(Long.valueOf(jBtToLong4));
                i2++;
            }
        }
        int i7 = (int) ((((double) this.irLen) / ((double) this.targetWaveFormat.blockAlign)) * (this.waveFormat.samplesPerSec / this.targetWaveFormat.samplesPerSec));
        if (iMin < i7) {
            while (iMin < i7) {
                this.mono24BitWaveData.add(0L);
                iMin++;
            }
        }
    }
}
