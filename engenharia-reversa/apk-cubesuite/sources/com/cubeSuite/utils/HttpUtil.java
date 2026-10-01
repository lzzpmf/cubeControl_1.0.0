package com.cubeSuite.utils;

import com.cubeSuite.utils.instrument.DownLoadCallback;
import com.cubeSuite.utils.instrument.HttpCallback;
import java.io.IOException;
import java.util.Objects;
import java.util.concurrent.TimeUnit;
import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.FormBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import okhttp3.ResponseBody;

/* JADX INFO: loaded from: classes.dex */
public class HttpUtil {
    public static void post(String str, FormBody formBody, final HttpCallback httpCallback) {
        new OkHttpClient.Builder().readTimeout(10L, TimeUnit.SECONDS).writeTimeout(10L, TimeUnit.SECONDS).connectTimeout(10L, TimeUnit.SECONDS).build().newCall(new Request.Builder().post(formBody).url(str).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.HttpUtil.1
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                httpCallback.failureCallBack();
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                httpCallback.successCallBack(response.body().string());
            }
        });
    }

    public static void get(String str, HttpCallback httpCallback) {
        get(str, "", httpCallback);
    }

    public static void get(String str, String str2, final HttpCallback httpCallback) {
        new OkHttpClient.Builder().readTimeout(5L, TimeUnit.SECONDS).writeTimeout(5L, TimeUnit.SECONDS).connectTimeout(5L, TimeUnit.SECONDS).build().newCall(new Request.Builder().get().url(str + "?" + str2).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.HttpUtil.2
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                httpCallback.failureCallBack();
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                httpCallback.successCallBack(((ResponseBody) Objects.requireNonNull(response.body())).string());
            }
        });
    }

    public static void downLoadFile(String str, final DownLoadCallback downLoadCallback) {
        new OkHttpClient.Builder().readTimeout(5L, TimeUnit.SECONDS).writeTimeout(5L, TimeUnit.SECONDS).connectTimeout(5L, TimeUnit.SECONDS).build().newCall(new Request.Builder().get().url(str).build()).enqueue(new Callback() { // from class: com.cubeSuite.utils.HttpUtil.3
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                downLoadCallback.failure();
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                response.body().bytes();
            }
        });
    }
}
