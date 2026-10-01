package com.cubeSuite.entity;

import java.io.Serializable;

/* JADX INFO: loaded from: classes.dex */
public class EffectParamEntity implements Serializable {
    private int id = 0;
    private String name = "xx";
    private String unit = "%";
    private int value = 30;
    private int minValue = 0;
    private int maxValue = 100;

    public String getUnit() {
        return this.unit;
    }

    public void setUnit(String str) {
        this.unit = str;
    }

    public int getId() {
        return this.id;
    }

    public void setId(int i) {
        this.id = i;
    }

    public String getName() {
        return this.name;
    }

    public void setName(String str) {
        this.name = str;
    }

    public int getValue() {
        return this.value;
    }

    public void setValue(int i) {
        this.value = i;
    }

    public int getMinValue() {
        return this.minValue;
    }

    public void setMinValue(int i) {
        this.minValue = i;
    }

    public int getMaxValue() {
        return this.maxValue;
    }

    public void setMaxValue(int i) {
        this.maxValue = i;
    }
}
