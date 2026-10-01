package es.voghdev.pdfviewpager.library.asset;

import android.content.Context;
import es.voghdev.pdfviewpager.library.service.CopyAssetService;

/* JADX INFO: loaded from: classes.dex */
public class CopyAssetServiceImpl implements CopyAsset {
    private Context context;

    public CopyAssetServiceImpl(Context context) {
        this.context = context;
    }

    @Override // es.voghdev.pdfviewpager.library.asset.CopyAsset
    public void copy(String str, String str2) {
        CopyAssetService.startCopyAction(this.context, str, str2);
    }
}
