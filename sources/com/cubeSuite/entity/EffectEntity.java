package com.cubeSuite.entity;

import java.io.Serializable;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class EffectEntity implements Serializable {
    protected String adName;
    protected List<EffectParamEntity> effectParamEntries;
    protected int id;
    protected int imageId;
    protected int index;
    protected String name;
    protected boolean onOff;

    public String getAdName() {
        return this.adName;
    }

    public void setAdName(String str) {
        this.adName = str;
    }

    public boolean isOnOff() {
        return this.onOff;
    }

    public void setOnOff(boolean z) {
        this.onOff = z;
    }

    public int getId() {
        return this.id;
    }

    public void setId(int i) {
        this.id = i;
    }

    public int getIndex() {
        return this.index;
    }

    public void setIndex(int i) {
        this.index = i;
    }

    public String getName() {
        return this.name;
    }

    public void setName(String str) {
        this.name = str;
    }

    public int getImageId() {
        return this.imageId;
    }

    public void setImageId(int i) {
        this.imageId = i;
    }

    public List<EffectParamEntity> getEffectParamEntries() {
        return this.effectParamEntries;
    }

    public void setEffectParamEntries(List<EffectParamEntity> list) {
        this.effectParamEntries = list;
    }
}
