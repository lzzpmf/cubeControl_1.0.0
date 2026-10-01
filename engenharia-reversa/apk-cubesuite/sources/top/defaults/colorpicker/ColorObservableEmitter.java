package top.defaults.colorpicker;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/* JADX INFO: loaded from: classes2.dex */
class ColorObservableEmitter implements ColorObservable {
    private int color;
    private List<ColorObserver> observers = new ArrayList();

    ColorObservableEmitter() {
    }

    @Override // top.defaults.colorpicker.ColorObservable
    public void subscribe(ColorObserver colorObserver) {
        if (colorObserver == null) {
            return;
        }
        this.observers.add(colorObserver);
    }

    @Override // top.defaults.colorpicker.ColorObservable
    public void unsubscribe(ColorObserver colorObserver) {
        if (colorObserver == null) {
            return;
        }
        this.observers.remove(colorObserver);
    }

    @Override // top.defaults.colorpicker.ColorObservable
    public int getColor() {
        return this.color;
    }

    void onColor(int i, boolean z, boolean z2) {
        this.color = i;
        Iterator<ColorObserver> it = this.observers.iterator();
        while (it.hasNext()) {
            it.next().onColor(i, z, z2);
        }
    }
}
