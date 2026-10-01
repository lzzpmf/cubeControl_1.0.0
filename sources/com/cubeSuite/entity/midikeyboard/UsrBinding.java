package com.cubeSuite.entity.midikeyboard;

import androidx.databinding.Bindable;
import com.cubeSuite.Global.Global;
import com.cubeSuite.entity.addrData.BaseDataAddr;

/* JADX INFO: loaded from: classes.dex */
public class UsrBinding extends BaseDataAddr {
    private int knobIndex;
    private int padIndex;
    private int psrIndex;
    private String[] padNoteStr = Global.BluetoothPedalCustomCommand.OCTAVE;
    private String[] keyBedMcpStr = Global.BluetoothPedalCustomCommand.OCTAVE;
    private String padNoteText = "CC";
    private String padMinText = "Min Vel";
    private String padMaxText = "Max Vel";
    private String[] knobCCStr = Global.BluetoothPedalCustomCommand.MIDI_STR;
    private String knobMinText = "Min";
    private String knobMaxText = "Max";
    private String[] modCCStr = Global.BluetoothPedalCustomCommand.MIDI_STR;
    private String psrCCText = "CC";
    private String[] psrCCStr = Global.BluetoothPedalCustomCommand.MIDI_STR;
    private String psrValue1Text = "Value1";
    private String psrValue2Text = "Value2";

    @Bindable
    public int getPadIndex() {
        return this.padIndex;
    }

    @Bindable
    public int getKnobIndex() {
        return this.knobIndex;
    }

    @Bindable
    public String[] getPadNoteStr() {
        return this.padNoteStr;
    }

    @Bindable
    public String getPadNoteText() {
        return this.padNoteText;
    }

    @Bindable
    public String getPadMinText() {
        return this.padMinText;
    }

    @Bindable
    public String getPadMaxText() {
        return this.padMaxText;
    }

    @Bindable
    public String[] getKeyBedMcpStr() {
        return this.keyBedMcpStr;
    }

    @Bindable
    public String[] getKnobCCStr() {
        return this.knobCCStr;
    }

    @Bindable
    public String getKnobMinText() {
        return this.knobMinText;
    }

    @Bindable
    public String getKnobMaxText() {
        return this.knobMaxText;
    }

    @Bindable
    public String[] getModCCStr() {
        return this.modCCStr;
    }

    @Bindable
    public int getPsrIndex() {
        return this.psrIndex;
    }

    @Bindable
    public String getPsrCCText() {
        return this.psrCCText;
    }

    @Bindable
    public String[] getPsrCCStr() {
        return this.psrCCStr;
    }

    @Bindable
    public String getPsrValue1Text() {
        return this.psrValue1Text;
    }

    @Bindable
    public String getPsrValue2Text() {
        return this.psrValue2Text;
    }

    public void setPsrValue1Text(String str) {
        this.psrValue1Text = str;
        notifyPropertyChanged(19);
    }

    public void setPsrValue2Text(String str) {
        this.psrValue2Text = str;
        notifyPropertyChanged(20);
    }

    public void setPsrCCText(String str) {
        this.psrCCText = str;
        notifyPropertyChanged(17);
    }

    public void setPsrCCStr(String[] strArr) {
        this.psrCCStr = strArr;
        notifyPropertyChanged(16);
    }

    public void setPsrIndex(int i) {
        this.psrIndex = i;
        notifyPropertyChanged(18);
    }

    public void setModCCStr(String[] strArr) {
        this.modCCStr = strArr;
        notifyPropertyChanged(10);
    }

    public void setKnobMinText(String str) {
        this.knobMinText = str;
        notifyPropertyChanged(8);
    }

    public void setKnobMaxText(String str) {
        this.knobMaxText = str;
        notifyPropertyChanged(7);
    }

    public void setKnobCCStr(String[] strArr) {
        this.knobCCStr = strArr;
        notifyPropertyChanged(5);
    }

    public void setKeyBedMcpStr(String[] strArr) {
        this.keyBedMcpStr = strArr;
        notifyPropertyChanged(4);
    }

    public void setPadMinText(String str) {
        this.padMinText = str;
        notifyPropertyChanged(13);
    }

    public void setPadMaxText(String str) {
        this.padMaxText = str;
        notifyPropertyChanged(12);
    }

    public void setPadNoteStr(String[] strArr) {
        this.padNoteStr = strArr;
        notifyPropertyChanged(14);
    }

    public void setPadNoteText(String str) {
        this.padNoteText = str;
        notifyPropertyChanged(15);
    }

    public void setPadIndex(int i) {
        this.padIndex = i;
        notifyPropertyChanged(11);
    }

    public void setKnobIndex(int i) {
        this.knobIndex = i;
        notifyPropertyChanged(6);
    }
}
