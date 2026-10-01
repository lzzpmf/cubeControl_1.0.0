package es.voghdev.pdfviewpager.library.asset;

/* JADX INFO: loaded from: classes.dex */
public interface CopyAsset {

    public interface Listener {
        void failure(Exception exc);

        void success(String str, String str2);
    }

    void copy(String str, String str2);
}
