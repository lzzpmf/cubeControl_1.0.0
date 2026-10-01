package es.voghdev.pdfviewpager.library.remote;

/* JADX INFO: loaded from: classes.dex */
public interface DownloadFile {

    public interface Listener {
        void onFailure(Exception exc);

        void onProgressUpdate(int i, int i2);

        void onSuccess(String str, String str2);
    }

    void download(String str, String str2);
}
