package es.voghdev.pdfviewpager.library.service;

import android.app.IntentService;
import android.content.Context;
import android.content.Intent;
import es.voghdev.pdfviewpager.library.util.FileUtil;
import java.io.IOException;

/* JADX INFO: loaded from: classes.dex */
public class CopyAssetService extends IntentService {
    private static final String ACTION_COPY_ASSET = "es.voghdev.pdfviewpager.library.copy_asset";
    private static final String EXTRA_ASSET = "es.voghdev.pdfviewpager.library.asset";
    private static final String EXTRA_DESTINATION = "es.voghdev.pdfviewpager.library.destination_path";

    public CopyAssetService() {
        super("CopyAssetService");
    }

    public static void startCopyAction(Context context, String str, String str2) {
        Intent intent = new Intent(context, (Class<?>) CopyAssetService.class);
        intent.setAction(ACTION_COPY_ASSET);
        intent.putExtra(EXTRA_ASSET, str);
        intent.putExtra(EXTRA_DESTINATION, str2);
        context.startService(intent);
    }

    @Override // android.app.IntentService
    protected void onHandleIntent(Intent intent) {
        if (intent == null || !ACTION_COPY_ASSET.equals(intent.getAction())) {
            return;
        }
        handleActionCopyAsset(intent.getStringExtra(EXTRA_ASSET), intent.getStringExtra(EXTRA_DESTINATION));
    }

    private void handleActionCopyAsset(String str, String str2) {
        try {
            FileUtil.copyAsset(this, str, str2);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
