package com.cubeSuite.entity.other;

import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class AppUpdateInfo {
    private int appVer;
    private boolean needUpdate = false;
    private List<String> updateInfo;
    private String updateTitle;

    public boolean isNeedUpdate() {
        return this.needUpdate;
    }

    public void setNeedUpdate(boolean z) {
        this.needUpdate = z;
    }

    public int getAppVer() {
        return this.appVer;
    }

    public void setAppVer(int i) {
        this.appVer = i;
    }

    public String getUpdateTitle() {
        return this.updateTitle;
    }

    public void setUpdateTitle(String str) {
        this.updateTitle = str;
    }

    public List<String> getUpdateInfo() {
        return this.updateInfo;
    }

    public void setUpdateInfo(List<String> list) {
        this.updateInfo = list;
    }
}
