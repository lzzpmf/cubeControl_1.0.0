package top.defaults.colorpicker;

/* JADX INFO: loaded from: classes2.dex */
public interface ColorObservable {
    int getColor();

    void subscribe(ColorObserver colorObserver);

    void unsubscribe(ColorObserver colorObserver);
}
