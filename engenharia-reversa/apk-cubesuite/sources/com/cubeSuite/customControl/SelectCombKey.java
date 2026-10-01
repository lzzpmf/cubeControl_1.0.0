package com.cubeSuite.customControl;

import android.app.AlertDialog;
import android.content.Context;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.GridLayout;
import android.widget.SpinnerAdapter;
import android.widget.Toast;
import androidx.appcompat.widget.AppCompatSpinner;
import com.cubeSuite.R;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class SelectCombKey {
    private AppCompatSpinner comb1;
    private AppCompatSpinner comb2;
    private AppCompatSpinner comb3;
    private GridLayout contentList;
    private final Context context;
    private final AlertDialog dialog;
    private Listening listening;
    private final TextIconView title;
    int[] keyboard_data = {0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 82, 81, 80, 79, 75, 78, 44, 40, 42, 57, 43, 39, 30, 31, 32, 33, 34, 35, 36, 37, 38, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 1, 2, 4, 8};
    int com1Index = 0;
    int com2Index = 0;
    int com3Index = 0;

    public interface Listening {
        default void dataChange(byte b, byte b2, byte b3) {
        }
    }

    public SelectCombKey(final Context context) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(context.getResources().getStringArray(R.array.comb_csa)[0]);
        for (int i = 0; i < 26; i++) {
            arrayList.add("" + ((char) (i + 65)));
        }
        arrayList.add("↑");
        arrayList.add("↓");
        arrayList.add("←");
        arrayList.add("→");
        arrayList.add("PAGE UP");
        arrayList.add("PAGE DOWN");
        arrayList.add("SPACE");
        arrayList.add("Enter");
        arrayList.add("ESC");
        arrayList.add("Delete");
        arrayList.add("Capslock");
        arrayList.add("Tab");
        for (int i2 = 0; i2 < 10; i2++) {
            arrayList.add(String.valueOf(i2));
        }
        arrayList.add("+");
        arrayList.add("-");
        for (int i3 = 1; i3 < 13; i3++) {
            arrayList.add("F" + i3);
        }
        this.context = context;
        AlertDialog.Builder builder = new AlertDialog.Builder(context);
        View viewInflate = View.inflate(context, R.layout.select_comb_key, null);
        this.title = (TextIconView) viewInflate.findViewById(R.id.title);
        this.contentList = (GridLayout) viewInflate.findViewById(R.id.contentList);
        Button button = (Button) viewInflate.findViewById(R.id.close);
        builder.setCancelable(false);
        builder.setView(viewInflate);
        button.setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.SelectCombKey$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.m105lambda$new$0$comcubeSuitecustomControlSelectCombKey(view);
            }
        });
        this.comb1 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb1);
        this.comb2 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb2);
        this.comb3 = (AppCompatSpinner) viewInflate.findViewById(R.id.comb3);
        this.comb1.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, context.getResources().getStringArray(R.array.comb_csa)));
        this.comb2.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, context.getResources().getStringArray(R.array.comb_csa)));
        this.comb3.setAdapter((SpinnerAdapter) new ArrayAdapter(context, R.layout.custom_spiner_item, arrayList));
        this.comb1.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.SelectCombKey.1
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (SelectCombKey.this.com1Index == i4) {
                    return;
                }
                if (i4 != 0 && i4 == SelectCombKey.this.com2Index) {
                    Toast.makeText(context, R.string.KeyCombinationError, 0).show();
                    SelectCombKey.this.comb1.setSelection(SelectCombKey.this.com1Index);
                } else {
                    SelectCombKey.this.com1Index = i4;
                    if (SelectCombKey.this.listening != null) {
                        SelectCombKey.this.listening.dataChange((byte) (SelectCombKey.this.com1Index == 0 ? 0 : SelectCombKey.this.com1Index + 62), (byte) (SelectCombKey.this.com2Index != 0 ? SelectCombKey.this.com2Index + 62 : 0), (byte) SelectCombKey.this.com3Index);
                    }
                }
            }
        });
        this.comb2.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.SelectCombKey.2
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (SelectCombKey.this.com2Index == i4) {
                    return;
                }
                if (i4 != 0 && i4 == SelectCombKey.this.com1Index) {
                    Toast.makeText(context, R.string.KeyCombinationError, 0).show();
                    SelectCombKey.this.comb2.setSelection(SelectCombKey.this.com2Index);
                } else {
                    SelectCombKey.this.com2Index = i4;
                    if (SelectCombKey.this.listening != null) {
                        SelectCombKey.this.listening.dataChange((byte) (SelectCombKey.this.com1Index == 0 ? 0 : SelectCombKey.this.com1Index + 62), (byte) (SelectCombKey.this.com2Index != 0 ? SelectCombKey.this.com2Index + 62 : 0), (byte) SelectCombKey.this.com3Index);
                    }
                }
            }
        });
        this.comb3.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.SelectCombKey.3
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i4, long j) {
                if (SelectCombKey.this.com3Index == i4) {
                    return;
                }
                SelectCombKey.this.com3Index = i4;
                if (SelectCombKey.this.listening != null) {
                    SelectCombKey.this.listening.dataChange((byte) (SelectCombKey.this.com1Index == 0 ? 0 : SelectCombKey.this.com1Index + 62), (byte) (SelectCombKey.this.com2Index != 0 ? SelectCombKey.this.com2Index + 62 : 0), (byte) SelectCombKey.this.com3Index);
                }
            }
        });
        this.dialog = builder.create();
    }

    /* JADX INFO: renamed from: lambda$new$0$com-cubeSuite-customControl-SelectCombKey, reason: not valid java name */
    /* synthetic */ void m105lambda$new$0$comcubeSuitecustomControlSelectCombKey(View view) {
        closeDialog();
    }

    public SelectCombKey setData(int i, int i2, int i3) {
        if (i != 0) {
            this.com1Index = i - 62;
        } else {
            this.com1Index = i;
        }
        if (i2 != 0) {
            this.com2Index = i2 - 62;
        } else {
            this.com2Index = i2;
        }
        this.com3Index = i3;
        this.comb1.setSelection(this.com1Index);
        this.comb2.setSelection(this.com2Index);
        this.comb3.setSelection(this.com3Index);
        return this;
    }

    public SelectCombKey setTitle(String str) {
        this.title.setText(str);
        return this;
    }

    public SelectCombKey setTitle(int i) {
        this.title.setText(i);
        return this;
    }

    public void setListening(Listening listening) {
        this.listening = listening;
    }

    public void closeDialog() {
        if (this.context == null) {
            return;
        }
        this.dialog.dismiss();
    }

    public SelectCombKey showDialog() {
        if (this.context == null) {
            return this;
        }
        this.dialog.show();
        return this;
    }
}
