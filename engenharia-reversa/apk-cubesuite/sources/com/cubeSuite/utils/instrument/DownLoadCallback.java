package com.cubeSuite.utils.instrument;

import java.io.File;

/* JADX INFO: loaded from: classes.dex */
public interface DownLoadCallback {
    void failure();

    void success(File file);
}
