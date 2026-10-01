package com.cubeSuite.customControl;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.text.InputFilter;
import android.text.Spanned;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.SpinnerAdapter;
import android.widget.Toast;
import androidx.appcompat.widget.AppCompatSpinner;
import com.cubeSuite.R;
import java.util.regex.Pattern;

/* JADX INFO: loaded from: classes.dex */
public class IRBoxAlert extends Dialog {
    IRBoxAlertCallback callback;
    EditText editText;
    String[] nameArr;
    int saveIndex;

    public interface IRBoxAlertCallback {
        void onSave(String str, int i);
    }

    public IRBoxAlert setCallback(IRBoxAlertCallback iRBoxAlertCallback) {
        this.callback = iRBoxAlertCallback;
        return this;
    }

    public IRBoxAlert(Context context, String[] strArr, int i) {
        super(context);
        this.nameArr = strArr;
        this.saveIndex = i;
    }

    @Override // android.app.Dialog
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.ir_box_alert);
        this.editText = (EditText) findViewById(R.id.edit_name);
        setEditTextContent();
        this.editText.setFilters(new InputFilter[]{new InputFilter() { // from class: com.cubeSuite.customControl.IRBoxAlert.1
            Pattern pwdPattern = Pattern.compile("[^ -~]", 66);

            @Override // android.text.InputFilter
            public CharSequence filter(CharSequence charSequence, int i, int i2, Spanned spanned, int i3, int i4) {
                if (this.pwdPattern.matcher(charSequence).find()) {
                    return "";
                }
                return null;
            }
        }});
        ((Button) findViewById(R.id.save_but)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.customControl.IRBoxAlert.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                String string = IRBoxAlert.this.editText.getText().toString();
                if (string.isEmpty()) {
                    Toast.makeText(IRBoxAlert.this.getContext(), "Name can not empty", 0).show();
                } else {
                    IRBoxAlert.this.dismiss();
                    IRBoxAlert.this.callback.onSave(string, IRBoxAlert.this.saveIndex);
                }
            }
        });
        AppCompatSpinner appCompatSpinner = (AppCompatSpinner) findViewById(R.id.name_spinner);
        appCompatSpinner.setAdapter((SpinnerAdapter) new ArrayAdapter(getContext(), R.layout.spinner_item, R.id.tvItem, this.nameArr));
        appCompatSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() { // from class: com.cubeSuite.customControl.IRBoxAlert.3
            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onNothingSelected(AdapterView<?> adapterView) {
            }

            @Override // android.widget.AdapterView.OnItemSelectedListener
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long j) {
                IRBoxAlert.this.saveIndex = i;
                IRBoxAlert.this.setEditTextContent();
            }
        });
        appCompatSpinner.setSelection(this.saveIndex);
    }

    void setEditTextContent() {
        this.editText.setText(replaceNub(this.nameArr[this.saveIndex]));
    }

    String replaceNub(String str) {
        return str.replaceAll("\\([0-9]*\\)", "");
    }
}
