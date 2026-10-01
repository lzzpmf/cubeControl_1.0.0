package kotlinx.coroutines.flow;

import kotlin.Metadata;
import kotlin.Unit;
import kotlin.coroutines.Continuation;
import kotlin.coroutines.jvm.internal.DebugMetadata;
import kotlin.coroutines.jvm.internal.SuspendLambda;
import kotlin.jvm.functions.Function1;
import kotlin.jvm.functions.Function3;
import kotlinx.coroutines.CoroutineScope;

/* JADX INFO: Add missing generic type declarations: [T] */
/* JADX INFO: compiled from: Delay.kt */
/* JADX INFO: loaded from: classes2.dex */
@Metadata(d1 = {"\u0000\u0012\n\u0000\n\u0002\u0010\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\u0010\u0000\u001a\u00020\u0001\"\u0004\b\u0000\u0010\u0002*\u00020\u00032\f\u0010\u0004\u001a\b\u0012\u0004\u0012\u0002H\u00020\u0005H\u008a@"}, d2 = {"<anonymous>", "", "T", "Lkotlinx/coroutines/CoroutineScope;", "downstream", "Lkotlinx/coroutines/flow/FlowCollector;"}, k = 3, mv = {1, 6, 0}, xi = 48)
@DebugMetadata(c = "kotlinx.coroutines.flow.FlowKt__DelayKt$debounceInternal$1", f = "Delay.kt", i = {0, 0, 0, 0, 1, 1, 1, 1}, l = {222, 355}, m = "invokeSuspend", n = {"downstream", "values", "lastValue", "timeoutMillis", "downstream", "values", "lastValue", "timeoutMillis"}, s = {"L$0", "L$1", "L$2", "L$3", "L$0", "L$1", "L$2", "L$3"})
final class FlowKt__DelayKt$debounceInternal$1<T> extends SuspendLambda implements Function3<CoroutineScope, FlowCollector<? super T>, Continuation<? super Unit>, Object> {
    final /* synthetic */ Flow<T> $this_debounceInternal;
    final /* synthetic */ Function1<T, Long> $timeoutMillisSelector;
    private /* synthetic */ Object L$0;
    /* synthetic */ Object L$1;
    Object L$2;
    Object L$3;
    int label;

    /* JADX WARN: 'super' call moved to the top of the method (can break code semantics) */
    /* JADX WARN: Multi-variable type inference failed */
    FlowKt__DelayKt$debounceInternal$1(Function1<? super T, Long> function1, Flow<? extends T> flow, Continuation<? super FlowKt__DelayKt$debounceInternal$1> continuation) {
        super(3, continuation);
        this.$timeoutMillisSelector = function1;
        this.$this_debounceInternal = flow;
    }

    @Override // kotlin.jvm.functions.Function3
    public final Object invoke(CoroutineScope coroutineScope, FlowCollector<? super T> flowCollector, Continuation<? super Unit> continuation) {
        FlowKt__DelayKt$debounceInternal$1 flowKt__DelayKt$debounceInternal$1 = new FlowKt__DelayKt$debounceInternal$1(this.$timeoutMillisSelector, this.$this_debounceInternal, continuation);
        flowKt__DelayKt$debounceInternal$1.L$0 = coroutineScope;
        flowKt__DelayKt$debounceInternal$1.L$1 = flowCollector;
        return flowKt__DelayKt$debounceInternal$1.invokeSuspend(Unit.INSTANCE);
    }

    /* JADX WARN: Code duplicated, block: B:11:0x006a A[PHI: r7 r8 r9
  0x006a: PHI (r7v5 kotlin.jvm.internal.Ref$ObjectRef) = 
  (r7v2 kotlin.jvm.internal.Ref$ObjectRef)
  (r7v3 kotlin.jvm.internal.Ref$ObjectRef)
  (r7v3 kotlin.jvm.internal.Ref$ObjectRef)
  (r7v10 kotlin.jvm.internal.Ref$ObjectRef)
 binds: [B:10:0x0045, B:57:0x006a, B:51:0x0120, B:6:0x0011] A[DONT_GENERATE, DONT_INLINE]
  0x006a: PHI (r8v5 kotlinx.coroutines.channels.ReceiveChannel) = 
  (r8v2 kotlinx.coroutines.channels.ReceiveChannel)
  (r8v3 kotlinx.coroutines.channels.ReceiveChannel)
  (r8v3 kotlinx.coroutines.channels.ReceiveChannel)
  (r8v10 kotlinx.coroutines.channels.ReceiveChannel)
 binds: [B:10:0x0045, B:57:0x006a, B:51:0x0120, B:6:0x0011] A[DONT_GENERATE, DONT_INLINE]
  0x006a: PHI (r9v4 kotlinx.coroutines.flow.FlowCollector) = 
  (r9v1 kotlinx.coroutines.flow.FlowCollector)
  (r9v2 kotlinx.coroutines.flow.FlowCollector)
  (r9v2 kotlinx.coroutines.flow.FlowCollector)
  (r9v7 kotlinx.coroutines.flow.FlowCollector)
 binds: [B:10:0x0045, B:57:0x006a, B:51:0x0120, B:6:0x0011] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:13:0x0070  */
    /* JADX WARN: Code duplicated, block: B:15:0x0079  */
    /* JADX WARN: Code duplicated, block: B:17:0x0081  */
    /* JADX WARN: Code duplicated, block: B:20:0x0094  */
    /* JADX WARN: Code duplicated, block: B:22:0x009a  */
    /* JADX WARN: Code duplicated, block: B:24:0x00a0  */
    /* JADX WARN: Code duplicated, block: B:29:0x00b8  */
    /* JADX WARN: Code duplicated, block: B:31:0x00c4 A[PHI: r0 r7 r8 r9
  0x00c4: PHI (r0v3 kotlin.jvm.internal.Ref$LongRef) = 
  (r0v9 kotlin.jvm.internal.Ref$LongRef)
  (r0v11 kotlin.jvm.internal.Ref$LongRef)
  (r0v11 kotlin.jvm.internal.Ref$LongRef)
 binds: [B:28:0x00b5, B:14:0x0077, B:21:0x0098] A[DONT_GENERATE, DONT_INLINE]
  0x00c4: PHI (r7v3 kotlin.jvm.internal.Ref$ObjectRef) = 
  (r7v4 kotlin.jvm.internal.Ref$ObjectRef)
  (r7v5 kotlin.jvm.internal.Ref$ObjectRef)
  (r7v5 kotlin.jvm.internal.Ref$ObjectRef)
 binds: [B:28:0x00b5, B:14:0x0077, B:21:0x0098] A[DONT_GENERATE, DONT_INLINE]
  0x00c4: PHI (r8v3 kotlinx.coroutines.channels.ReceiveChannel) = 
  (r8v4 kotlinx.coroutines.channels.ReceiveChannel)
  (r8v5 kotlinx.coroutines.channels.ReceiveChannel)
  (r8v5 kotlinx.coroutines.channels.ReceiveChannel)
 binds: [B:28:0x00b5, B:14:0x0077, B:21:0x0098] A[DONT_GENERATE, DONT_INLINE]
  0x00c4: PHI (r9v2 kotlinx.coroutines.flow.FlowCollector) = 
  (r9v3 kotlinx.coroutines.flow.FlowCollector)
  (r9v4 kotlinx.coroutines.flow.FlowCollector)
  (r9v4 kotlinx.coroutines.flow.FlowCollector)
 binds: [B:28:0x00b5, B:14:0x0077, B:21:0x0098] A[DONT_GENERATE, DONT_INLINE]] */
    /* JADX WARN: Code duplicated, block: B:33:0x00ca  */
    /* JADX WARN: Code duplicated, block: B:43:0x00f4 A[Catch: all -> 0x010f, TryCatch #0 {all -> 0x010f, blocks: (B:41:0x00ed, B:43:0x00f4, B:44:0x0100), top: B:55:0x00ed }] */
    /* JADX WARN: Code duplicated, block: B:50:0x011d  */
    /*  JADX ERROR: JadxOverflowException in pass: RegionMakerVisitor
        jadx.core.utils.exceptions.JadxOverflowException: Regions stack size limit reached
        	at jadx.core.utils.ErrorsCounter.addError(ErrorsCounter.java:59)
        	at jadx.core.utils.ErrorsCounter.error(ErrorsCounter.java:31)
        	at jadx.core.dex.attributes.nodes.NotificationAttrNode.addError(NotificationAttrNode.java:19)
        */
    @Override // kotlin.coroutines.jvm.internal.BaseContinuationImpl
    public final java.lang.Object invokeSuspend(java.lang.Object r15) {
        /*
            Method dump skipped, instruction units count: 294
            To view this dump change 'Code comments level' option to 'DEBUG'
        */
        throw new UnsupportedOperationException("Method not decompiled: kotlinx.coroutines.flow.FlowKt__DelayKt$debounceInternal$1.invokeSuspend(java.lang.Object):java.lang.Object");
    }
}
