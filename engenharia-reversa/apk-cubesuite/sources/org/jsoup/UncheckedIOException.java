package org.jsoup;

import java.io.IOException;

/* JADX INFO: loaded from: classes2.dex */
public class UncheckedIOException extends RuntimeException {
    public UncheckedIOException(IOException iOException) {
        super(iOException);
    }

    public UncheckedIOException(String str) {
        super(new IOException(str));
    }

    public IOException ioException() {
        return (IOException) getCause();
    }
}
