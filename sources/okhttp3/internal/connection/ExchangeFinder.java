package okhttp3.internal.connection;

import androidx.core.app.NotificationCompat;
import java.io.IOException;
import java.net.Socket;
import java.util.List;
import kotlin.Metadata;
import kotlin.Unit;
import kotlin.jvm.internal.Intrinsics;
import kotlin.jvm.internal.Ref;
import okhttp3.Address;
import okhttp3.EventListener;
import okhttp3.OkHttpClient;
import okhttp3.Route;
import okhttp3.internal.Util;
import okhttp3.internal.http.ExchangeCodec;
import okhttp3.internal.http.RealInterceptorChain;
import okhttp3.internal.http2.ConnectionShutdownException;
import okhttp3.internal.http2.ErrorCode;
import okhttp3.internal.http2.StreamResetException;

/* JADX INFO: compiled from: ExchangeFinder.kt */
/* JADX INFO: loaded from: classes2.dex */
@Metadata(bv = {1, 0, 3}, d1 = {"\u0000l\n\u0002\u0018\u0002\n\u0002\u0010\u0000\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0002\b\u0004\n\u0002\u0018\u0002\n\u0000\n\u0002\u0010\b\n\u0000\n\u0002\u0018\u0002\n\u0002\b\u0003\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0002\b\u0006\n\u0002\u0010\u000b\n\u0002\b\u0005\n\u0002\u0010\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\u0018\u00002\u00020\u0001B%\u0012\u0006\u0010\u0002\u001a\u00020\u0003\u0012\u0006\u0010\u0004\u001a\u00020\u0005\u0012\u0006\u0010\u0006\u001a\u00020\u0007\u0012\u0006\u0010\b\u001a\u00020\t¢\u0006\u0002\u0010\nJ\b\u0010\r\u001a\u0004\u0018\u00010\u000eJ\u0016\u0010\u0019\u001a\u00020\u001a2\u0006\u0010\u001b\u001a\u00020\u001c2\u0006\u0010\u001d\u001a\u00020\u001eJ0\u0010\u001f\u001a\u00020\u000e2\u0006\u0010 \u001a\u00020\u00102\u0006\u0010!\u001a\u00020\u00102\u0006\u0010\"\u001a\u00020\u00102\u0006\u0010#\u001a\u00020\u00102\u0006\u0010$\u001a\u00020%H\u0002J8\u0010&\u001a\u00020\u000e2\u0006\u0010 \u001a\u00020\u00102\u0006\u0010!\u001a\u00020\u00102\u0006\u0010\"\u001a\u00020\u00102\u0006\u0010#\u001a\u00020\u00102\u0006\u0010$\u001a\u00020%2\u0006\u0010'\u001a\u00020%H\u0002J\u0006\u0010(\u001a\u00020%J\b\u0010)\u001a\u00020%H\u0002J\u000e\u0010*\u001a\u00020+2\u0006\u0010,\u001a\u00020-R\u0014\u0010\u0004\u001a\u00020\u0005X\u0080\u0004¢\u0006\b\n\u0000\u001a\u0004\b\u000b\u0010\fR\u000e\u0010\u0006\u001a\u00020\u0007X\u0082\u0004¢\u0006\u0002\n\u0000R\u0010\u0010\r\u001a\u0004\u0018\u00010\u000eX\u0082\u000e¢\u0006\u0002\n\u0000R\u000e\u0010\u0002\u001a\u00020\u0003X\u0082\u0004¢\u0006\u0002\n\u0000R\u000e\u0010\u000f\u001a\u00020\u0010X\u0082\u000e¢\u0006\u0002\n\u0000R\u000e\u0010\b\u001a\u00020\tX\u0082\u0004¢\u0006\u0002\n\u0000R\u0010\u0010\u0011\u001a\u0004\u0018\u00010\u0012X\u0082\u000e¢\u0006\u0002\n\u0000R\u000e\u0010\u0013\u001a\u00020\u0010X\u0082\u000e¢\u0006\u0002\n\u0000R\u000e\u0010\u0014\u001a\u00020\u0010X\u0082\u000e¢\u0006\u0002\n\u0000R\u0010\u0010\u0015\u001a\u0004\u0018\u00010\u0016X\u0082\u000e¢\u0006\u0002\n\u0000R\u0010\u0010\u0017\u001a\u0004\u0018\u00010\u0018X\u0082\u000e¢\u0006\u0002\n\u0000¨\u0006."}, d2 = {"Lokhttp3/internal/connection/ExchangeFinder;", "", "connectionPool", "Lokhttp3/internal/connection/RealConnectionPool;", "address", "Lokhttp3/Address;", NotificationCompat.CATEGORY_CALL, "Lokhttp3/internal/connection/RealCall;", "eventListener", "Lokhttp3/EventListener;", "(Lokhttp3/internal/connection/RealConnectionPool;Lokhttp3/Address;Lokhttp3/internal/connection/RealCall;Lokhttp3/EventListener;)V", "getAddress$okhttp", "()Lokhttp3/Address;", "connectingConnection", "Lokhttp3/internal/connection/RealConnection;", "connectionShutdownCount", "", "nextRouteToTry", "Lokhttp3/Route;", "otherFailureCount", "refusedStreamCount", "routeSelection", "Lokhttp3/internal/connection/RouteSelector$Selection;", "routeSelector", "Lokhttp3/internal/connection/RouteSelector;", "find", "Lokhttp3/internal/http/ExchangeCodec;", "client", "Lokhttp3/OkHttpClient;", "chain", "Lokhttp3/internal/http/RealInterceptorChain;", "findConnection", "connectTimeout", "readTimeout", "writeTimeout", "pingIntervalMillis", "connectionRetryEnabled", "", "findHealthyConnection", "doExtensiveHealthChecks", "retryAfterFailure", "retryCurrentRoute", "trackFailure", "", "e", "Ljava/io/IOException;", "okhttp"}, k = 1, mv = {1, 1, 16})
public final class ExchangeFinder {
    private final Address address;
    private final RealCall call;
    private RealConnection connectingConnection;
    private final RealConnectionPool connectionPool;
    private int connectionShutdownCount;
    private final EventListener eventListener;
    private Route nextRouteToTry;
    private int otherFailureCount;
    private int refusedStreamCount;
    private RouteSelector.Selection routeSelection;
    private RouteSelector routeSelector;

    public ExchangeFinder(RealConnectionPool connectionPool, Address address, RealCall call, EventListener eventListener) {
        Intrinsics.checkParameterIsNotNull(connectionPool, "connectionPool");
        Intrinsics.checkParameterIsNotNull(address, "address");
        Intrinsics.checkParameterIsNotNull(call, "call");
        Intrinsics.checkParameterIsNotNull(eventListener, "eventListener");
        this.connectionPool = connectionPool;
        this.address = address;
        this.call = call;
        this.eventListener = eventListener;
    }

    /* JADX INFO: renamed from: getAddress$okhttp, reason: from getter */
    public final Address getAddress() {
        return this.address;
    }

    public final ExchangeCodec find(OkHttpClient client, RealInterceptorChain chain) {
        Intrinsics.checkParameterIsNotNull(client, "client");
        Intrinsics.checkParameterIsNotNull(chain, "chain");
        try {
            try {
                return findHealthyConnection(chain.getConnectTimeoutMillis$okhttp(), chain.getReadTimeoutMillis(), chain.getWriteTimeoutMillis(), client.pingIntervalMillis(), client.retryOnConnectionFailure(), !Intrinsics.areEqual(chain.getRequest().method(), "GET")).newCodec$okhttp(client, chain);
            } catch (IOException e) {
                e = e;
                IOException iOException = e;
                trackFailure(iOException);
                throw new RouteException(iOException);
            } catch (RouteException e2) {
                e = e2;
                RouteException routeException = e;
                trackFailure(routeException.getLastConnectException());
                throw routeException;
            }
        } catch (IOException e3) {
            e = e3;
        } catch (RouteException e4) {
            e = e4;
        }
    }

    private final RealConnection findHealthyConnection(int connectTimeout, int readTimeout, int writeTimeout, int pingIntervalMillis, boolean connectionRetryEnabled, boolean doExtensiveHealthChecks) throws Throwable {
        while (true) {
            RealConnection realConnectionFindConnection = findConnection(connectTimeout, readTimeout, writeTimeout, pingIntervalMillis, connectionRetryEnabled);
            if (realConnectionFindConnection.isHealthy(doExtensiveHealthChecks)) {
                return realConnectionFindConnection;
            }
            realConnectionFindConnection.noNewExchanges();
        }
    }

    /* JADX WARN: Code duplicated, block: B:19:0x0056  */
    /* JADX WARN: Code duplicated, block: B:32:0x0095  */
    /* JADX WARN: Code duplicated, block: B:64:0x010a  */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Type inference failed for: r3v3, types: [T, okhttp3.internal.connection.RealConnection] */
    private final RealConnection findConnection(int connectTimeout, int readTimeout, int writeTimeout, int pingIntervalMillis, boolean connectionRetryEnabled) throws Throwable {
        Socket socketReleaseConnectionNoEvents$okhttp;
        RealConnection realConnection;
        Route next;
        boolean z;
        boolean z2;
        List<Route> routes;
        RealConnection connection;
        Route route;
        Socket socket = null;
        Ref.ObjectRef objectRef = new Ref.ObjectRef();
        synchronized (this.connectionPool) {
            if (this.call.isCanceled()) {
                throw new IOException("Canceled");
            }
            objectRef.element = this.call.getConnection();
            if (this.call.getConnection() == null) {
                socketReleaseConnectionNoEvents$okhttp = null;
            } else {
                RealConnection connection2 = this.call.getConnection();
                if (connection2 == null) {
                    Intrinsics.throwNpe();
                }
                if (!connection2.getNoNewExchanges()) {
                    RealConnection connection3 = this.call.getConnection();
                    if (connection3 == null) {
                        Intrinsics.throwNpe();
                    }
                    if (connection3.supportsUrl(this.address.url())) {
                        socketReleaseConnectionNoEvents$okhttp = null;
                    }
                }
                socketReleaseConnectionNoEvents$okhttp = this.call.releaseConnectionNoEvents$okhttp();
            }
            if (this.call.getConnection() != null) {
                realConnection = this.call.getConnection();
                objectRef.element = null;
            } else {
                realConnection = null;
            }
            if (realConnection == null) {
                this.refusedStreamCount = 0;
                this.connectionShutdownCount = 0;
                this.otherFailureCount = 0;
                if (this.connectionPool.callAcquirePooledConnection(this.address, this.call, null, false)) {
                    realConnection = this.call.getConnection();
                    next = null;
                    z = true;
                } else {
                    next = this.nextRouteToTry;
                    if (next != null) {
                        this.nextRouteToTry = null;
                    } else {
                        next = null;
                    }
                    z = false;
                }
            } else {
                next = null;
                z = false;
            }
            Unit unit = Unit.INSTANCE;
        }
        if (socketReleaseConnectionNoEvents$okhttp != null) {
            Util.closeQuietly(socketReleaseConnectionNoEvents$okhttp);
        }
        if (((RealConnection) objectRef.element) != null) {
            EventListener eventListener = this.eventListener;
            RealCall realCall = this.call;
            RealConnection realConnection2 = (RealConnection) objectRef.element;
            if (realConnection2 == null) {
                Intrinsics.throwNpe();
            }
            eventListener.connectionReleased(realCall, realConnection2);
        }
        if (z) {
            EventListener eventListener2 = this.eventListener;
            RealCall realCall2 = this.call;
            if (realConnection == null) {
                Intrinsics.throwNpe();
            }
            eventListener2.connectionAcquired(realCall2, realConnection);
        }
        if (realConnection != null) {
            if (realConnection == null) {
                Intrinsics.throwNpe();
            }
            return realConnection;
        }
        if (next != null) {
            z2 = false;
        } else {
            RouteSelector.Selection selection = this.routeSelection;
            if (selection != null) {
                if (selection == null) {
                    Intrinsics.throwNpe();
                }
                if (selection.hasNext()) {
                    z2 = false;
                }
            }
            RouteSelector routeSelector = this.routeSelector;
            if (routeSelector == null) {
                routeSelector = new RouteSelector(this.address, this.call.getClient().getRouteDatabase(), this.call, this.eventListener);
                this.routeSelector = routeSelector;
            }
            this.routeSelection = routeSelector.next();
            z2 = true;
        }
        synchronized (this.connectionPool) {
            if (this.call.isCanceled()) {
                throw new IOException("Canceled");
            }
            if (z2) {
                RouteSelector.Selection selection2 = this.routeSelection;
                if (selection2 == null) {
                    Intrinsics.throwNpe();
                }
                routes = selection2.getRoutes();
                if (this.connectionPool.callAcquirePooledConnection(this.address, this.call, routes, false)) {
                    realConnection = this.call.getConnection();
                    z = true;
                }
            } else {
                routes = null;
            }
            if (!z) {
                if (next == null) {
                    RouteSelector.Selection selection3 = this.routeSelection;
                    if (selection3 == null) {
                        Intrinsics.throwNpe();
                    }
                    next = selection3.next();
                }
                RealConnectionPool realConnectionPool = this.connectionPool;
                if (next == null) {
                    Intrinsics.throwNpe();
                }
                realConnection = new RealConnection(realConnectionPool, next);
                this.connectingConnection = realConnection;
            }
            connection = realConnection;
            route = next;
            Unit unit2 = Unit.INSTANCE;
        }
        if (z) {
            EventListener eventListener3 = this.eventListener;
            RealCall realCall3 = this.call;
            if (connection == null) {
                Intrinsics.throwNpe();
            }
            eventListener3.connectionAcquired(realCall3, connection);
            if (connection == null) {
                Intrinsics.throwNpe();
            }
            return connection;
        }
        if (connection == null) {
            Intrinsics.throwNpe();
        }
        connection.connect(connectTimeout, readTimeout, writeTimeout, pingIntervalMillis, connectionRetryEnabled, this.call, this.eventListener);
        RouteDatabase routeDatabase = this.call.getClient().getRouteDatabase();
        if (connection == null) {
            Intrinsics.throwNpe();
        }
        routeDatabase.connected(connection.getRoute());
        synchronized (this.connectionPool) {
            this.connectingConnection = null;
            if (this.connectionPool.callAcquirePooledConnection(this.address, this.call, routes, true)) {
                if (connection == null) {
                    Intrinsics.throwNpe();
                }
                connection.setNoNewExchanges(true);
                if (connection == null) {
                    Intrinsics.throwNpe();
                }
                socket = connection.socket();
                connection = this.call.getConnection();
                this.nextRouteToTry = route;
            } else {
                RealConnectionPool realConnectionPool2 = this.connectionPool;
                if (connection == null) {
                    Intrinsics.throwNpe();
                }
                realConnectionPool2.put(connection);
                RealCall realCall4 = this.call;
                if (connection == null) {
                    Intrinsics.throwNpe();
                }
                realCall4.acquireConnectionNoEvents(connection);
            }
            Unit unit3 = Unit.INSTANCE;
        }
        if (socket != null) {
            Util.closeQuietly(socket);
        }
        EventListener eventListener4 = this.eventListener;
        RealCall realCall5 = this.call;
        if (connection == null) {
            Intrinsics.throwNpe();
        }
        eventListener4.connectionAcquired(realCall5, connection);
        if (connection == null) {
            Intrinsics.throwNpe();
        }
        return connection;
    }

    public final RealConnection connectingConnection() {
        RealConnectionPool realConnectionPool = this.connectionPool;
        if (!Util.assertionsEnabled || Thread.holdsLock(realConnectionPool)) {
            return this.connectingConnection;
        }
        StringBuilder sb = new StringBuilder("Thread ");
        Thread threadCurrentThread = Thread.currentThread();
        Intrinsics.checkExpressionValueIsNotNull(threadCurrentThread, "Thread.currentThread()");
        throw new AssertionError(sb.append(threadCurrentThread.getName()).append(" MUST hold lock on ").append(realConnectionPool).toString());
    }

    public final void trackFailure(IOException e) {
        Intrinsics.checkParameterIsNotNull(e, "e");
        RealConnectionPool realConnectionPool = this.connectionPool;
        if (!Util.assertionsEnabled || !Thread.holdsLock(realConnectionPool)) {
            synchronized (this.connectionPool) {
                this.nextRouteToTry = null;
                if ((e instanceof StreamResetException) && ((StreamResetException) e).errorCode == ErrorCode.REFUSED_STREAM) {
                    this.refusedStreamCount++;
                } else if (e instanceof ConnectionShutdownException) {
                    this.connectionShutdownCount++;
                } else {
                    this.otherFailureCount++;
                }
            }
            return;
        }
        StringBuilder sb = new StringBuilder("Thread ");
        Thread threadCurrentThread = Thread.currentThread();
        Intrinsics.checkExpressionValueIsNotNull(threadCurrentThread, "Thread.currentThread()");
        throw new AssertionError(sb.append(threadCurrentThread.getName()).append(" MUST NOT hold lock on ").append(realConnectionPool).toString());
    }

    public final boolean retryAfterFailure() {
        synchronized (this.connectionPool) {
            if (this.refusedStreamCount == 0 && this.connectionShutdownCount == 0 && this.otherFailureCount == 0) {
                return false;
            }
            if (this.nextRouteToTry != null) {
                return true;
            }
            if (retryCurrentRoute()) {
                RealConnection connection = this.call.getConnection();
                if (connection == null) {
                    Intrinsics.throwNpe();
                }
                this.nextRouteToTry = connection.getRoute();
                return true;
            }
            RouteSelector.Selection selection = this.routeSelection;
            if (selection != null && selection.hasNext()) {
                return true;
            }
            RouteSelector routeSelector = this.routeSelector;
            if (routeSelector == null) {
                return true;
            }
            return routeSelector.hasNext();
        }
    }

    private final boolean retryCurrentRoute() {
        RealConnection connection;
        return this.refusedStreamCount <= 1 && this.connectionShutdownCount <= 1 && this.otherFailureCount <= 0 && (connection = this.call.getConnection()) != null && connection.getRouteFailureCount() == 0 && Util.canReuseConnectionFor(connection.getRoute().address().url(), this.address.url());
    }
}
