package okhttp3.internal.platform.android;

import android.util.Log;
import kotlin.Metadata;
import kotlin.TypeCastException;
import kotlin.jvm.internal.Intrinsics;
import kotlin.text.StringsKt;

/* JADX INFO: compiled from: util.kt */
/* JADX INFO: loaded from: classes2.dex */
@Metadata(bv = {1, 0, 3}, d1 = {"\u0000\u001c\n\u0000\n\u0002\u0010\b\n\u0000\n\u0002\u0010\u0002\n\u0002\b\u0002\n\u0002\u0010\u000e\n\u0000\n\u0002\u0010\u0003\n\u0000\u001a\"\u0010\u0002\u001a\u00020\u00032\u0006\u0010\u0004\u001a\u00020\u00012\u0006\u0010\u0005\u001a\u00020\u00062\b\u0010\u0007\u001a\u0004\u0018\u00010\bH\u0000\"\u000e\u0010\u0000\u001a\u00020\u0001X\u0082T¢\u0006\u0002\n\u0000¨\u0006\t"}, d2 = {"MAX_LOG_LENGTH", "", "androidLog", "", "level", "message", "", "t", "", "okhttp"}, k = 2, mv = {1, 1, 16})
public final class UtilKt {
    private static final int MAX_LOG_LENGTH = 4000;

    public static final void androidLog(int i, String message, Throwable th) {
        int iMin;
        Intrinsics.checkParameterIsNotNull(message, "message");
        int i2 = i != 5 ? 3 : 5;
        if (th != null) {
            message = message + "\n" + Log.getStackTraceString(th);
        }
        int length = message.length();
        int i3 = 0;
        while (i3 < length) {
            int iIndexOf$default = StringsKt.indexOf$default((CharSequence) message, '\n', i3, false, 4, (Object) null);
            if (iIndexOf$default == -1) {
                iIndexOf$default = length;
            }
            while (true) {
                iMin = Math.min(iIndexOf$default, i3 + MAX_LOG_LENGTH);
                if (message == null) {
                    throw new TypeCastException("null cannot be cast to non-null type java.lang.String");
                }
                String strSubstring = message.substring(i3, iMin);
                Intrinsics.checkExpressionValueIsNotNull(strSubstring, "(this as java.lang.Strin…ing(startIndex, endIndex)");
                Log.println(i2, "OkHttp", strSubstring);
                if (iMin >= iIndexOf$default) {
                    break;
                } else {
                    i3 = iMin;
                }
            }
            i3 = iMin + 1;
        }
    }
}
