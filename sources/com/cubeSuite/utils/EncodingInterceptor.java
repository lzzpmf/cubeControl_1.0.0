package com.cubeSuite.utils;

import java.io.IOException;
import java.lang.reflect.Field;
import okhttp3.Interceptor;
import okhttp3.Response;
import okhttp3.ResponseBody;

/* JADX INFO: loaded from: classes.dex */
public class EncodingInterceptor implements Interceptor {
    private String encoding;

    public EncodingInterceptor(String str) {
        this.encoding = str;
    }

    @Override // okhttp3.Interceptor
    public Response intercept(Interceptor.Chain chain) throws IOException {
        Response responseProceed = chain.proceed(chain.request());
        settingClientCustomEncoding(responseProceed);
        return responseProceed;
    }

    private void settingClientCustomEncoding(Response response) throws IOException {
        setBodyContentType(response);
    }

    private void setBodyContentType(Response response) throws IOException {
        ResponseBody responseBodyBody = response.body();
        try {
            Field declaredField = responseBodyBody.getClass().getDeclaredField("contentTypeString");
            declaredField.setAccessible(true);
            String.valueOf(declaredField.get(responseBodyBody));
            declaredField.set(responseBodyBody, "application/rss+xml;charset=" + this.encoding);
        } catch (IllegalAccessException e) {
            throw new IOException("use reflect to setting header occurred an error", e);
        } catch (NoSuchFieldException e2) {
            throw new IOException("use reflect to setting header occurred an error", e2);
        }
    }
}
