package es.voghdev.pdfviewpager.library.subscaleview.decoder;

import java.lang.reflect.InvocationTargetException;

/* JADX INFO: loaded from: classes.dex */
public interface DecoderFactory<T> {
    T make() throws IllegalAccessException, NoSuchMethodException, InstantiationException, InvocationTargetException;
}
