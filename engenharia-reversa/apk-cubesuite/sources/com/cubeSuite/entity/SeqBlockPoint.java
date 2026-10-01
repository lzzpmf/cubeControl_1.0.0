package com.cubeSuite.entity;

import com.cubeSuite.customControl.BlockProgressBar;

/* JADX INFO: loaded from: classes.dex */
public class SeqBlockPoint {
    public BlockProgressBar block;
    public int force = 50;
    public boolean isEnable = false;
    public boolean isAdjustable = false;

    /* JADX INFO: renamed from: top, reason: collision with root package name */
    public float f11top = 0.0f;
    public float bottom = 0.0f;
    public float left = 0.0f;
    public float right = 0.0f;

    public void setPos(float f, float f2, float f3, float f4) {
        this.f11top = f;
        this.bottom = f2;
        this.left = f3;
        this.right = f4;
    }
}
