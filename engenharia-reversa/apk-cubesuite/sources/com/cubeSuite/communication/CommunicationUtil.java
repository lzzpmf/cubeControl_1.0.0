package com.cubeSuite.communication;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.util.Log;
import androidx.core.app.NotificationManagerCompat;
import com.cubeSuite.R;
import com.cubeSuite.activitys.ActivityStackUtil;
import com.cubeSuite.bluetooth.BleHandle;
import com.cubeSuite.callback.Callback;
import com.cubeSuite.config.BleEnum;
import com.cubeSuite.customControl.AlertDialogUtil;
import com.cubeSuite.entity.addrData.AddrI8;
import com.cubeSuite.entity.addrData.AddrU16;
import com.cubeSuite.entity.addrData.AddrU8;
import com.fastble.BleManager;
import com.fastble.callback.BleNotifyCallback;
import com.fastble.callback.BleWriteCallback;
import com.fastble.exception.BleException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.locks.ReentrantLock;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class CommunicationUtil {
    private static final int DATA_CALLBACK = 10000;
    private static final int MSG_ERASE_CALLBACK = 10004;
    private static final int MSG_QUERY_CALLBACK = 10003;
    private static final int MSG_READ_CALLBACK = 10001;
    private static final int MSG_WRITE_CALLBACK = 10002;
    private static ReentrantLock m_lock = new ReentrantLock();
    private Callback.OpenNotify notifyCallback;
    private String writeService = BleEnum.SINCO_BLE_SERVICE;
    private String write = BleEnum.SINCO_BLE_WRITE;
    private final int WAIT_TIME = 3000;
    private volatile boolean isReadData = false;
    private final long ERASE_FLASE_BLOCK_SIZE = 4096;
    private final Handler handler = new Handler(Looper.getMainLooper(), new Handler.Callback() { // from class: com.cubeSuite.communication.CommunicationUtil.1
        @Override // android.os.Handler.Callback
        public boolean handleMessage(Message message) {
            SendMSG sendMSG = message.obj != null ? (SendMSG) message.obj : null;
            if (message.what != 10000 || sendMSG == null) {
                return false;
            }
            sendMSG.callback();
            return false;
        }
    });
    public Callback.BleCommunicationCallback sendLongDataMsg = new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.2
        @Override // com.cubeSuite.callback.Callback.BleCommunicationCallback
        public void writeDataCallback(boolean z) {
            if (z) {
                AlertDialogUtil.getInstance(ActivityStackUtil.getInstance().getActivity()).setTitle(R.string.hint).setContent(R.string.sendDataSuccess).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showDialog();
            } else {
                AlertDialogUtil.getInstance(ActivityStackUtil.getInstance().getActivity()).setTitle(R.string.hint).setContent(R.string.dataSendFail).setBtnVisible(AlertDialogUtil.SelectBtn.CONFIRM).showErrorDialog();
            }
        }
    };
    private volatile boolean m_isRead = false;
    private volatile boolean m_isWrite = false;
    private final BleWriteCallback writeCallback = new BleWriteCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.6
        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteFailure(BleException bleException) {
        }

        @Override // com.fastble.callback.BleWriteCallback
        public void onWriteSuccess(int i, int i2, byte[] bArr) {
        }
    };
    ArrayList<Byte> notifyData = new ArrayList<>(4096);
    private BleNotifyCallback bleNotifyCallback = new BleNotifyCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.7
        @Override // com.fastble.callback.BleNotifyCallback
        public void onNotifySuccess() {
            CommunicationUtil.this.notifyCallback.openStatus(true);
        }

        @Override // com.fastble.callback.BleNotifyCallback
        public void onNotifyFailure(BleException bleException) {
            CommunicationUtil.this.notifyCallback.openStatus(false);
            Log.e("BLE", "open notify failure Exception: " + bleException);
        }

        @Override // com.fastble.callback.BleNotifyCallback
        public void onCharacteristicChanged(byte[] bArr) {
            for (byte b : bArr) {
                CommunicationUtil.this.notifyData.add(Byte.valueOf(b));
            }
        }
    };

    private enum QueryResDataParsing {
        dataHead,
        cmdType,
        dataLen,
        data,
        checkSum
    }

    private enum ReadResDataParsing {
        dataHead,
        cmdType,
        dataAllLen,
        flashType,
        dataAddr,
        dataLen,
        data,
        checkSum
    }

    private enum WriteResDataParsing {
        dataHead,
        cmdType,
        Res
    }

    private static class ResData {
        ArrayList<Byte> data;
        boolean isSuccess;

        public ResData(boolean z, ArrayList<Byte> arrayList) {
            this.isSuccess = z;
            this.data = arrayList;
        }

        public ResData(boolean z) {
            this.isSuccess = z;
            this.data = new ArrayList<>(0);
        }
    }

    private static class SendMSG {
        public Callback.BleCommunicationCallback callback;
        public int msgWhat;
        public ResData resData;

        public SendMSG(int i, ResData resData, Callback.BleCommunicationCallback bleCommunicationCallback) {
            this.callback = bleCommunicationCallback;
            this.resData = resData;
            this.msgWhat = i;
        }

        public void callback() {
            int size = this.resData.data.size();
            byte[] bArr = new byte[size];
            for (int i = 0; i < size; i++) {
                bArr[i] = this.resData.data.get(i).byteValue();
            }
            switch (this.msgWhat) {
                case CommunicationUtil.MSG_READ_CALLBACK /* 10001 */:
                    this.callback.readDataCallback(this.resData.isSuccess, bArr);
                    break;
                case CommunicationUtil.MSG_WRITE_CALLBACK /* 10002 */:
                    this.callback.writeDataCallback(this.resData.isSuccess);
                    break;
                case CommunicationUtil.MSG_QUERY_CALLBACK /* 10003 */:
                    this.callback.queryDataCallback(this.resData.isSuccess, bArr);
                    break;
                case CommunicationUtil.MSG_ERASE_CALLBACK /* 10004 */:
                    this.callback.eraseCallback(this.resData.isSuccess);
                    break;
            }
        }
    }

    public void clearBuf() {
        this.notifyData.clear();
    }

    public void writeDataToBleNotRes(byte[] bArr) {
        BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, bArr, this.writeCallback);
        this.notifyData.clear();
    }

    private void sendMsg(int i, ResData resData, Callback.BleCommunicationCallback bleCommunicationCallback) {
        Message message = new Message();
        message.what = 10000;
        message.obj = new SendMSG(i, resData, bleCommunicationCallback);
        this.handler.sendMessage(message);
    }

    public void splitReadData(final byte b, final long j, final int i, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda0
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m97x8151c8b0(j, i, b, bleCommunicationCallback);
            }
        }).start();
    }

    /* JADX INFO: renamed from: lambda$splitReadData$0$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m97x8151c8b0(long j, int i, byte b, Callback.BleCommunicationCallback bleCommunicationCallback) {
        while (this.m_isRead) {
            try {
                Thread.sleep(1L);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
        this.m_isRead = true;
        ArrayList arrayList = new ArrayList(i);
        int i2 = i;
        long j2 = j;
        while (i2 > 0) {
            if (i2 < 1000) {
                BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeReadPacketByType(b, j2, i2), this.writeCallback);
                ResData readResData = getReadResData(j2, i2, b);
                arrayList.addAll(readResData.data);
                if (readResData.isSuccess) {
                    sendMsg(MSG_READ_CALLBACK, new ResData(true, arrayList), bleCommunicationCallback);
                } else {
                    sendMsg(MSG_READ_CALLBACK, readResData, bleCommunicationCallback);
                }
                this.m_isRead = false;
                return;
            }
            BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeReadPacketByType(b, j2, 1000), this.writeCallback);
            long j3 = 1000;
            ResData readResData2 = getReadResData(j2, j3, b);
            arrayList.addAll(readResData2.data);
            if (!readResData2.isSuccess) {
                sendMsg(MSG_READ_CALLBACK, readResData2, bleCommunicationCallback);
                this.m_isRead = false;
                return;
            } else {
                i2 += NotificationManagerCompat.IMPORTANCE_UNSPECIFIED;
                j2 += j3;
            }
        }
    }

    public void sendI8Data(byte b, AddrI8 addrI8) {
        splitWriteData(b, new byte[]{addrI8.getData()}, addrI8.getAddr());
    }

    public void sendU8Data(byte b, AddrU8 addrU8) {
        splitWriteData(b, new byte[]{(byte) addrU8.getData()}, addrU8.getAddr());
    }

    public void sendU16Data(byte b, AddrU16 addrU16) {
        splitWriteData(b, Communication.u16AddU8Arr(addrU16.getData()), addrU16.getAddr());
    }

    public void splitWriteData(byte b, byte[] bArr, long j) {
        splitWriteData(b, bArr, j, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.3
        });
    }

    public void splitWriteData(byte b, List<Byte> list, long j) {
        int size = list.size();
        byte[] bArr = new byte[size];
        for (int i = 0; i < size; i++) {
            bArr[i] = list.get(i).byteValue();
        }
        splitWriteData(b, bArr, j, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.4
        });
    }

    public void sendLongData(byte b, byte[] bArr, long j) {
        AlertDialogUtil.getInstance(ActivityStackUtil.getInstance().getActivity()).setTitle(R.string.hint).setContent(R.string.sendingData).setBtnVisible(AlertDialogUtil.SelectBtn.ALL_INVISIBLE).showDialog();
        splitWriteData(b, bArr, j, this.sendLongDataMsg);
    }

    public void splitWriteData(final byte b, final byte[] bArr, final long j, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda5
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m98x3c29b420(bArr, b, j, bleCommunicationCallback);
            }
        }).start();
    }

    /* JADX INFO: renamed from: lambda$splitWriteData$1$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m98x3c29b420(byte[] bArr, byte b, long j, Callback.BleCommunicationCallback bleCommunicationCallback) {
        byte[] bArr2;
        while (this.m_isWrite) {
            try {
                Thread.sleep(1L);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
        this.m_isWrite = true;
        int length = bArr.length / 173;
        int i = 0;
        if (length == 0) {
            BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeWritePacketByType(b, bArr, j), this.writeCallback);
            sendMsg(MSG_WRITE_CALLBACK, new ResData(getWriteResData()), bleCommunicationCallback);
            this.m_isWrite = false;
            return;
        }
        if (bArr.length % 173 != 0) {
            length++;
        }
        int i2 = 0;
        while (i2 < length) {
            int i3 = length - 1;
            if (i2 != i3 || bArr.length % 173 == 0) {
                bArr2 = new byte[173];
                System.arraycopy(bArr, i2 * 173, bArr2, i, 173);
            } else {
                bArr2 = new byte[bArr.length % 173];
                System.arraycopy(bArr, i2 * 173, bArr2, i, bArr.length % 173);
            }
            BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeWritePacketByType(b, bArr2, j + (((long) i2) * ((long) 173))), this.writeCallback);
            if (!getWriteResData()) {
                sendMsg(MSG_WRITE_CALLBACK, new ResData(false), bleCommunicationCallback);
                this.m_isWrite = false;
                return;
            } else if (i2 == i3) {
                sendMsg(MSG_WRITE_CALLBACK, new ResData(true), bleCommunicationCallback);
                this.m_isWrite = false;
                return;
            } else {
                i2++;
                i = 0;
            }
        }
    }

    public void queryData(final byte b, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda4
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m95lambda$queryData$2$comcubeSuitecommunicationCommunicationUtil(b, bleCommunicationCallback);
            }
        }).start();
    }

    /* JADX INFO: renamed from: lambda$queryData$2$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m95lambda$queryData$2$comcubeSuitecommunicationCommunicationUtil(byte b, Callback.BleCommunicationCallback bleCommunicationCallback) {
        BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeQueryPacket(b), this.writeCallback);
        sendMsg(MSG_QUERY_CALLBACK, getQueryResData(b, 34), bleCommunicationCallback);
    }

    public void eraseFlash(final byte b, final long j, final int i, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda3
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m94x352cd3dd(i, b, j, bleCommunicationCallback);
            }
        });
    }

    /* JADX INFO: renamed from: lambda$eraseFlash$3$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m94x352cd3dd(int i, byte b, long j, Callback.BleCommunicationCallback bleCommunicationCallback) {
        for (int i2 = 0; i2 < i; i2++) {
            BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, Communication.makeCustomErasePacket(b, (((long) i2) * 4096) + j), this.writeCallback);
            if (!getWriteResData()) {
                sendMsg(MSG_ERASE_CALLBACK, new ResData(false), bleCommunicationCallback);
                return;
            }
        }
        sendMsg(MSG_ERASE_CALLBACK, new ResData(true), bleCommunicationCallback);
    }

    public void readData(final byte[] bArr, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda2
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m96lambda$readData$4$comcubeSuitecommunicationCommunicationUtil(bArr, bleCommunicationCallback);
            }
        }).start();
    }

    /* JADX INFO: renamed from: lambda$readData$4$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m96lambda$readData$4$comcubeSuitecommunicationCommunicationUtil(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        long j = (((long) bArr[13]) << 16) | (((long) bArr[12]) << 8) | ((long) bArr[11]);
        long j2 = (((long) bArr[10]) << 24) | (((long) bArr[9]) << 16) | (((long) bArr[8]) << 8) | ((long) bArr[7]);
        byte b = bArr[6];
        BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, bArr, this.writeCallback);
        sendMsg(MSG_READ_CALLBACK, getReadResData(j2, j, b), bleCommunicationCallback);
    }

    public void writeDataToBle(byte[] bArr) {
        writeDataToBle(bArr, new Callback.BleCommunicationCallback() { // from class: com.cubeSuite.communication.CommunicationUtil.5
        });
    }

    public void writeDataToBle(final byte[] bArr, final Callback.BleCommunicationCallback bleCommunicationCallback) {
        new Thread(new Runnable() { // from class: com.cubeSuite.communication.CommunicationUtil$$ExternalSyntheticLambda1
            @Override // java.lang.Runnable
            public final void run() {
                this.f$0.m99xc951352c(bArr, bleCommunicationCallback);
            }
        }).start();
    }

    /* JADX INFO: renamed from: lambda$writeDataToBle$5$com-cubeSuite-communication-CommunicationUtil, reason: not valid java name */
    /* synthetic */ void m99xc951352c(byte[] bArr, Callback.BleCommunicationCallback bleCommunicationCallback) {
        BleManager.getInstance().write(BleHandle.connectDeviceInfo.getBleDevice(), this.writeService, this.write, bArr, this.writeCallback);
        sendMsg(MSG_WRITE_CALLBACK, new ResData(getWriteResData()), bleCommunicationCallback);
    }

    private boolean getWriteResData() {
        m_lock.lock();
        WriteResDataParsing writeResDataParsing = WriteResDataParsing.dataHead;
        synchronized (this) {
            int i = 3000;
            int i2 = 0;
            while (i > 0) {
                while (i2 < this.notifyData.size()) {
                    int iOrdinal = writeResDataParsing.ordinal();
                    if (iOrdinal != 0) {
                        if (iOrdinal != 1) {
                            if (iOrdinal == 2) {
                                if (this.notifyData.get(i2).byteValue() == 0) {
                                    ArrayList<Byte> arrayList = this.notifyData;
                                    this.notifyData = new ArrayList<>(arrayList.subList(i2 + 2, arrayList.size()));
                                    this.isReadData = false;
                                    m_lock.unlock();
                                    return true;
                                }
                                this.notifyData.clear();
                                this.isReadData = false;
                                m_lock.unlock();
                                return false;
                            }
                        } else if (this.notifyData.get(i2).byteValue() == 0) {
                            writeResDataParsing = WriteResDataParsing.Res;
                            i2 += 4;
                        } else {
                            writeResDataParsing = WriteResDataParsing.dataHead;
                        }
                    } else if (this.notifyData.get(i2).byteValue() == 0 && this.notifyData.get(i2 + 1).byteValue() == 89) {
                        i2 += 2;
                        writeResDataParsing = WriteResDataParsing.cmdType;
                    } else {
                        i2++;
                    }
                }
                try {
                    Thread.sleep(1L);
                    i--;
                } catch (InterruptedException unused) {
                }
            }
            this.notifyData.clear();
            m_lock.unlock();
            this.isReadData = false;
            return false;
        }
    }

    private ResData getQueryResData(byte b, int i) {
        m_lock.lock();
        QueryResDataParsing queryResDataParsing = QueryResDataParsing.dataHead;
        ArrayList arrayList = new ArrayList();
        long jByteValue = 0;
        int i2 = 0;
        byte bByteValue = 0;
        for (int i3 = 3000; i3 > 0; i3--) {
            if (i > this.notifyData.size() || this.isReadData) {
                Thread.sleep(1L);
            } else {
                boolean z = true;
                this.isReadData = true;
                while (i2 < this.notifyData.size()) {
                    int iOrdinal = queryResDataParsing.ordinal();
                    if (iOrdinal != 0) {
                        if (iOrdinal != z) {
                            if (iOrdinal == 2) {
                                jByteValue = (((long) (this.notifyData.get(i2 + 2).byteValue() & UByte.MAX_VALUE)) << 16) | (((long) (this.notifyData.get(i2 + 1).byteValue() & UByte.MAX_VALUE)) << 8) | ((long) (this.notifyData.get(i2).byteValue() & UByte.MAX_VALUE));
                                queryResDataParsing = QueryResDataParsing.data;
                                i2 += 3;
                            } else if (iOrdinal == 3) {
                                for (int i4 = i2; i4 < ((long) i2) + jByteValue; i4++) {
                                    arrayList.add(this.notifyData.get(i4));
                                    bByteValue = (byte) (bByteValue + this.notifyData.get(i4).byteValue());
                                }
                                i2 += (int) jByteValue;
                                queryResDataParsing = QueryResDataParsing.checkSum;
                            } else if (iOrdinal == 4) {
                                int i5 = i2 + 1;
                                if ((~bByteValue) == this.notifyData.get(i2).byteValue()) {
                                    ArrayList<Byte> arrayList2 = this.notifyData;
                                    this.notifyData = new ArrayList<>(arrayList2.subList(i5, arrayList2.size()));
                                    this.isReadData = false;
                                    m_lock.unlock();
                                    return new ResData(z, arrayList);
                                }
                                i2 = i5;
                            }
                        } else if (this.notifyData.get(i2).byteValue() == b) {
                            queryResDataParsing = QueryResDataParsing.dataLen;
                            i2++;
                        } else {
                            queryResDataParsing = QueryResDataParsing.dataHead;
                        }
                    } else if (this.notifyData.get(i2).byteValue() == 0 && this.notifyData.get(i2 + 1).byteValue() == 89) {
                        i2 += 2;
                        queryResDataParsing = QueryResDataParsing.cmdType;
                    } else {
                        i2++;
                    }
                    z = true;
                }
                try {
                    Thread.sleep(1L);
                } catch (InterruptedException unused) {
                }
            }
        }
        this.notifyData.clear();
        this.isReadData = false;
        m_lock.unlock();
        return new ResData(false, arrayList);
    }

    /* JADX WARN: Can't fix incorrect switch cases order, some code will duplicate */
    private ResData getReadResData(long j, long j2, byte b) {
        int i;
        m_lock.lock();
        ReadResDataParsing readResDataParsing = ReadResDataParsing.dataHead;
        ArrayList arrayList = new ArrayList();
        int i2 = 0;
        byte bByteValue = 0;
        for (int i3 = 3000; i3 > 0; i3--) {
            if (this.notifyData.size() < j2 + 15 || this.isReadData) {
                Thread.sleep(1L);
            } else {
                boolean z = true;
                this.isReadData = true;
                while (i2 < this.notifyData.size()) {
                    switch (readResDataParsing) {
                        case dataHead:
                            if (this.notifyData.get(i2).byteValue() == 0 && this.notifyData.get(i2 + 1).byteValue() == 89) {
                                i2 += 2;
                                readResDataParsing = ReadResDataParsing.cmdType;
                            } else {
                                i2++;
                            }
                            z = true;
                            break;
                        case cmdType:
                            if (this.notifyData.get(i2).byteValue() == 35) {
                                readResDataParsing = ReadResDataParsing.dataAllLen;
                                i2++;
                            } else {
                                readResDataParsing = ReadResDataParsing.dataHead;
                            }
                            z = true;
                            break;
                        case dataAllLen:
                            if (((((long) (this.notifyData.get(i2 + 2).byteValue() & UByte.MAX_VALUE)) << 16) | (((long) (this.notifyData.get(i2 + 1).byteValue() & UByte.MAX_VALUE)) << 8) | ((long) (this.notifyData.get(i2).byteValue() & UByte.MAX_VALUE))) == j2 + 8) {
                                readResDataParsing = ReadResDataParsing.flashType;
                                i2 += 3;
                            } else {
                                readResDataParsing = ReadResDataParsing.dataHead;
                            }
                            z = true;
                            break;
                        case flashType:
                            if (this.notifyData.get(i2).byteValue() == b) {
                                byte bByteValue2 = (byte) (bByteValue + this.notifyData.get(i2).byteValue());
                                i2++;
                                bByteValue = bByteValue2;
                                readResDataParsing = ReadResDataParsing.dataAddr;
                            } else {
                                readResDataParsing = ReadResDataParsing.dataHead;
                            }
                            z = true;
                            break;
                        case dataAddr:
                            if (((((long) (this.notifyData.get(i2 + 2).byteValue() & UByte.MAX_VALUE)) << 16) | (((long) (this.notifyData.get(i2 + 3).byteValue() & UByte.MAX_VALUE)) << 24) | (((long) (this.notifyData.get(i2 + 1).byteValue() & UByte.MAX_VALUE)) << 8) | ((long) (this.notifyData.get(i2).byteValue() & UByte.MAX_VALUE))) == j) {
                                int i4 = i2;
                                while (true) {
                                    int i5 = i2 + 4;
                                    if (i4 < i5) {
                                        bByteValue = (byte) (bByteValue + this.notifyData.get(i4).byteValue());
                                        i4++;
                                    } else {
                                        readResDataParsing = ReadResDataParsing.dataLen;
                                        i2 = i5;
                                    }
                                }
                            } else {
                                readResDataParsing = ReadResDataParsing.dataHead;
                            }
                            z = true;
                            break;
                        case dataLen:
                            if (((((long) (this.notifyData.get(i2 + 1).byteValue() & UByte.MAX_VALUE)) << 8) | (((long) (this.notifyData.get(i2 + 2).byteValue() & UByte.MAX_VALUE)) << 16) | ((long) (this.notifyData.get(i2).byteValue() & UByte.MAX_VALUE))) == j2) {
                                int i6 = i2;
                                while (true) {
                                    i = i2 + 3;
                                    if (i6 < i) {
                                        bByteValue = (byte) (bByteValue + this.notifyData.get(i6).byteValue());
                                        i6++;
                                    } else {
                                        readResDataParsing = ReadResDataParsing.data;
                                        i2 = i;
                                        z = true;
                                    }
                                }
                            } else {
                                readResDataParsing = ReadResDataParsing.dataHead;
                                z = true;
                            }
                            break;
                        case data:
                            int i7 = i2;
                            while (true) {
                                long j3 = ((long) i2) + j2;
                                if (i7 < j3) {
                                    arrayList.add(this.notifyData.get(i7));
                                    bByteValue = (byte) (bByteValue + this.notifyData.get(i7).byteValue());
                                    i7++;
                                } else {
                                    i2 = (int) j3;
                                    readResDataParsing = ReadResDataParsing.checkSum;
                                    z = true;
                                }
                                break;
                            }
                            break;
                        case checkSum:
                            i = i2 + 1;
                            if ((~bByteValue) == this.notifyData.get(i2).byteValue()) {
                                ArrayList<Byte> arrayList2 = this.notifyData;
                                this.notifyData = new ArrayList<>(arrayList2.subList(i, arrayList2.size()));
                                this.isReadData = false;
                                m_lock.unlock();
                                return new ResData(z, arrayList);
                            }
                            i2 = i;
                            z = true;
                            break;
                        default:
                            z = true;
                            break;
                    }
                }
                try {
                    Thread.sleep(1L);
                } catch (InterruptedException unused) {
                }
            }
        }
        this.notifyData.clear();
        this.isReadData = false;
        m_lock.unlock();
        return new ResData(false, arrayList);
    }

    public void connect(Callback.OpenNotify openNotify) {
        this.notifyCallback = openNotify;
        BleManager.getInstance().notify(BleHandle.connectDeviceInfo.getBleDevice(), BleEnum.SINCO_BLE_SERVICE, BleEnum.SINCO_BLE_NOTIFICATION, false, this.bleNotifyCallback);
    }
}
