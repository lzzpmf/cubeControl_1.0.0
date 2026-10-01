package com.cubeSuite.entity;

import java.io.Serializable;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class InstrumentEntity implements Serializable {
    protected List<EffectEntity> effectEntries;
    protected int id;
    protected int imageId;
    protected String name;

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

    public int getImageId() {
        return this.imageId;
    }

    public void setImageId(int i) {
        this.imageId = i;
    }

    public List<EffectEntity> getEffectEntries() {
        return this.effectEntries;
    }

    public void setEffectEntries(List<EffectEntity> list) {
        this.effectEntries = list;
    }
}
