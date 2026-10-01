package com.cubeSuite.utils;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Environment;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.os.Process;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.core.content.FileProvider;
import com.cubeSuite.R;
import com.cubeSuite.config.WebConfig;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/* JADX INFO: loaded from: classes.dex */
public class AppAutoUpdate {
    private static final int DOWN_OVER = 2;
    private static final int DOWN_UPDATE = 1;
    private static final int SHOWDOWN = 3;
    private static File apkFile = null;
    private static final String saveFileName = "CubeSuite.apk";
    private Thread downLoadThread;
    private Context mContext;
    private ProgressBar mProgress;
    private int progress;
    private TextView txtStatus;
    private String apkUrl = WebConfig.APP_URL_PATH;
    protected String checkUrl = this.apkUrl + "output-metadata.json";
    private boolean isNew = false;
    private boolean intercept = false;
    private Runnable DownApkWork = new Runnable() { // from class: com.cubeSuite.utils.AppAutoUpdate.5
        @Override // java.lang.Runnable
        public void run() {
            try {
                HttpURLConnection httpURLConnection = (HttpURLConnection) new URL(AppAutoUpdate.this.apkUrl).openConnection();
                httpURLConnection.connect();
                int contentLength = httpURLConnection.getContentLength();
                InputStream inputStream = httpURLConnection.getInputStream();
                FileOutputStream fileOutputStream = new FileOutputStream(AppAutoUpdate.apkFile);
                byte[] bArr = new byte[1024];
                int i = 0;
                while (!AppAutoUpdate.this.intercept) {
                    int i2 = inputStream.read(bArr);
                    i += i2;
                    AppAutoUpdate.this.progress = (int) ((i / contentLength) * 100.0f);
                    AppAutoUpdate.this.mHandler.sendEmptyMessage(1);
                    if (i2 <= 0) {
                        AppAutoUpdate.this.mHandler.sendEmptyMessage(2);
                        break;
                    }
                    fileOutputStream.write(bArr, 0, i2);
                }
                fileOutputStream.close();
                inputStream.close();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    };
    private Handler mHandler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.utils.AppAutoUpdate.6
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            int i = message.what;
            if (i == 1) {
                AppAutoUpdate.this.txtStatus.setText(AppAutoUpdate.this.progress + "%");
                AppAutoUpdate.this.mProgress.setProgress(AppAutoUpdate.this.progress);
            } else if (i == 2) {
                Toast.makeText(AppAutoUpdate.this.mContext, "下载完毕", 0).show();
                AppAutoUpdate.this.installAPK();
            } else if (i == 3) {
                AppAutoUpdate.this.ShowUpdateDialog();
            }
            return false;
        }
    });

    public AppAutoUpdate(Context context) {
        this.mContext = context;
        apkFile = new File(this.mContext.getExternalFilesDir(Environment.DIRECTORY_DOWNLOADS), saveFileName);
    }

    public void ShowUpdateDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this.mContext);
        builder.setTitle("软件版本更新");
        builder.setMessage("有最新的软件包，请下载并安装!");
        builder.setPositiveButton("立即下载", new DialogInterface.OnClickListener() { // from class: com.cubeSuite.utils.AppAutoUpdate.1
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                AppAutoUpdate.this.ShowDownloadDialog();
            }
        });
        builder.setNegativeButton("以后再说", new DialogInterface.OnClickListener() { // from class: com.cubeSuite.utils.AppAutoUpdate.2
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
            }
        });
        builder.create().show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void ShowDownloadDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this.mContext);
        builder.setTitle("软件版本更新");
        View viewInflate = LayoutInflater.from(this.mContext).inflate(R.layout.update_app_progress, (ViewGroup) null);
        this.mProgress = (ProgressBar) viewInflate.findViewById(R.id.progress);
        this.txtStatus = (TextView) viewInflate.findViewById(R.id.txtStatus);
        builder.setView(viewInflate);
        builder.setNegativeButton("取消", new DialogInterface.OnClickListener() { // from class: com.cubeSuite.utils.AppAutoUpdate.3
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                AppAutoUpdate.this.intercept = true;
            }
        });
        builder.show();
        DownloadApk();
    }

    public void CheckUpdate() {
        new Thread(new Runnable() { // from class: com.cubeSuite.utils.AppAutoUpdate.4
            @Override // java.lang.Runnable
            public void run() throws Throwable {
                String str;
                String strGroup;
                String strReplace = "1";
                try {
                    str = AppAutoUpdate.this.mContext.getPackageManager().getPackageInfo(AppAutoUpdate.this.mContext.getPackageName(), 0).versionName;
                } catch (PackageManager.NameNotFoundException e) {
                    e.printStackTrace();
                    str = "1";
                }
                String strDoGet = AppAutoUpdate.doGet(AppAutoUpdate.this.checkUrl);
                String str2 = "";
                if (strDoGet != null && strDoGet.length() > 0 && Build.VERSION.SDK_INT >= 26) {
                    Matcher matcher = Pattern.compile("\"outputFile\":\\s*\"(?<m>[^\"]*?)\"").matcher(strDoGet);
                    if (!matcher.find()) {
                        strGroup = "";
                    } else {
                        strGroup = matcher.group("m");
                    }
                    Matcher matcher2 = Pattern.compile("\"versionName\":\\s*\"(?<m>[^\"]*?)\"").matcher(strDoGet);
                    if (matcher2.find()) {
                        matcher2.group("m");
                        strReplace = matcher2.group("m").replace("v1.0.", "");
                    }
                    str2 = strGroup;
                }
                if (Long.parseLong(str) < Long.parseLong(strReplace)) {
                    AppAutoUpdate.this.apkUrl += str2;
                    AppAutoUpdate.this.mHandler.sendEmptyMessage(3);
                }
            }
        }).start();
    }

    public void DownloadApk() {
        Thread thread = new Thread(this.DownApkWork);
        this.downLoadThread = thread;
        thread.start();
    }

    public void installAPK() {
        try {
            if (apkFile.exists()) {
                Intent intent = new Intent("android.intent.action.VIEW");
                intent.addFlags(268435456);
                intent.addFlags(1);
                intent.setDataAndType(FileProvider.getUriForFile(this.mContext, this.mContext.getApplicationContext().getPackageName() + ".fileprovider", apkFile), "application/vnd.android.package-archive");
                this.mContext.startActivity(intent);
                Process.killProcess(Process.myPid());
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /* JADX WARN: Code duplicated, block: B:89:0x00af A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:95:0x009b A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:97:0x00b9 A[EXC_TOP_SPLITTER, PHI: r1 r6
  0x00b9: PHI (r1v8 java.io.InputStream) = (r1v7 java.io.InputStream), (r1v10 java.io.InputStream) binds: [B:53:0x00a3, B:64:0x00b7] A[DONT_GENERATE, DONT_INLINE]
  0x00b9: PHI (r6v9 ??) = (r6v8 ??), (r6v11 ??) binds: [B:53:0x00a3, B:64:0x00b7] A[DONT_GENERATE, DONT_INLINE], SYNTHETIC] */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r0v0 */
    /* JADX WARN: Type inference failed for: r0v17 */
    /* JADX WARN: Type inference failed for: r0v9, types: [java.io.BufferedReader] */
    /* JADX WARN: Type inference failed for: r2v1 */
    /* JADX WARN: Type inference failed for: r2v15 */
    /* JADX WARN: Type inference failed for: r2v16 */
    /* JADX WARN: Type inference failed for: r2v17, types: [java.io.BufferedReader] */
    /* JADX WARN: Type inference failed for: r2v2 */
    /* JADX WARN: Type inference failed for: r2v5, types: [java.io.BufferedReader] */
    /* JADX WARN: Type inference failed for: r2v7, types: [java.io.BufferedReader] */
    /* JADX WARN: Type inference failed for: r6v0, types: [java.lang.String] */
    /* JADX WARN: Type inference failed for: r6v10, types: [java.net.HttpURLConnection] */
    /* JADX WARN: Type inference failed for: r6v11 */
    /* JADX WARN: Type inference failed for: r6v13, types: [java.net.HttpURLConnection] */
    /* JADX WARN: Type inference failed for: r6v14 */
    /* JADX WARN: Type inference failed for: r6v15 */
    /* JADX WARN: Type inference failed for: r6v2 */
    /* JADX WARN: Type inference failed for: r6v3 */
    /* JADX WARN: Type inference failed for: r6v4 */
    /* JADX WARN: Type inference failed for: r6v5 */
    /* JADX WARN: Type inference failed for: r6v6 */
    /* JADX WARN: Type inference failed for: r6v7, types: [java.net.HttpURLConnection] */
    /* JADX WARN: Type inference failed for: r6v8 */
    /* JADX WARN: Type inference failed for: r6v9 */
    public static String doGet(String str) throws Throwable {
        InputStream inputStream;
        BufferedReader bufferedReader;
        Throwable th;
        ?? bufferedReader2;
        String string;
        ?? r0 = 0;
        try {
            try {
                str = (HttpURLConnection) new URL(str).openConnection();
                try {
                    str.setRequestMethod("GET");
                    str.setConnectTimeout(15000);
                    str.setReadTimeout(60000);
                    str.connect();
                    if (str.getResponseCode() == 200) {
                        inputStream = str.getInputStream();
                        try {
                            bufferedReader2 = new BufferedReader(new InputStreamReader(inputStream, "UTF-8"));
                            try {
                                StringBuffer stringBuffer = new StringBuffer();
                                while (true) {
                                    String line = bufferedReader2.readLine();
                                    if (line == null) {
                                        break;
                                    }
                                    stringBuffer.append(line);
                                    stringBuffer.append("\r\n");
                                }
                                string = stringBuffer.toString();
                                r0 = bufferedReader2;
                            } catch (MalformedURLException e) {
                                e = e;
                                e.printStackTrace();
                                if (bufferedReader2 != 0) {
                                    try {
                                        bufferedReader2.close();
                                    } catch (IOException e2) {
                                        e2.printStackTrace();
                                    }
                                }
                                if (inputStream != null) {
                                    try {
                                        inputStream.close();
                                    } catch (IOException e3) {
                                        e3.printStackTrace();
                                    }
                                }
                                str.disconnect();
                                return null;
                            } catch (IOException e4) {
                                e = e4;
                                e.printStackTrace();
                                if (bufferedReader2 != 0) {
                                    try {
                                        bufferedReader2.close();
                                    } catch (IOException e5) {
                                        e5.printStackTrace();
                                    }
                                }
                                if (inputStream != null) {
                                    inputStream.close();
                                }
                                str.disconnect();
                                return null;
                            }
                        } catch (MalformedURLException e6) {
                            e = e6;
                            bufferedReader2 = 0;
                        } catch (IOException e7) {
                            e = e7;
                            bufferedReader2 = 0;
                        } catch (Throwable th2) {
                            bufferedReader = null;
                            th = th2;
                            if (bufferedReader != null) {
                                try {
                                    bufferedReader.close();
                                } catch (IOException e8) {
                                    e8.printStackTrace();
                                }
                            }
                            if (inputStream != null) {
                                try {
                                    inputStream.close();
                                } catch (IOException e9) {
                                    e9.printStackTrace();
                                }
                            }
                            str.disconnect();
                            throw th;
                        }
                    } else {
                        inputStream = null;
                        string = null;
                    }
                    if (r0 != 0) {
                        try {
                            r0.close();
                        } catch (IOException e10) {
                            e10.printStackTrace();
                        }
                    }
                    if (inputStream != null) {
                        try {
                            inputStream.close();
                        } catch (IOException e11) {
                            e11.printStackTrace();
                        }
                    }
                    str.disconnect();
                    return string;
                } catch (MalformedURLException e12) {
                    e = e12;
                    inputStream = null;
                    str = str;
                    bufferedReader2 = inputStream;
                    e.printStackTrace();
                    if (bufferedReader2 != 0) {
                        bufferedReader2.close();
                    }
                    if (inputStream != null) {
                        inputStream.close();
                    }
                    str.disconnect();
                    return null;
                } catch (IOException e13) {
                    e = e13;
                    inputStream = null;
                    str = str;
                    bufferedReader2 = inputStream;
                    e.printStackTrace();
                    if (bufferedReader2 != 0) {
                        bufferedReader2.close();
                    }
                    if (inputStream != null) {
                        inputStream.close();
                    }
                    str.disconnect();
                    return null;
                } catch (Throwable th3) {
                    bufferedReader = null;
                    th = th3;
                    inputStream = null;
                }
            } catch (MalformedURLException e14) {
                e = e14;
                str = 0;
                inputStream = null;
            } catch (IOException e15) {
                e = e15;
                str = 0;
                inputStream = null;
            } catch (Throwable th4) {
                inputStream = null;
                bufferedReader = null;
                th = th4;
                str = 0;
            }
        } catch (Throwable th5) {
            th = th5;
        }
    }
}
