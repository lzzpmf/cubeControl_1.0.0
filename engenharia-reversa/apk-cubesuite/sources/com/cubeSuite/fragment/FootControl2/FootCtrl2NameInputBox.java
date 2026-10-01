package com.cubeSuite.fragment.FootControl2;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;
import com.cubeSuite.R;

/* JADX INFO: loaded from: classes.dex */
public class FootCtrl2NameInputBox extends Dialog {
    FootCtrl2NameInputBoxCallback callback;
    EditText editText;

    public interface FootCtrl2NameInputBoxCallback {
        void onSave(String str);
    }

    public FootCtrl2NameInputBox setCallback(FootCtrl2NameInputBoxCallback footCtrl2NameInputBoxCallback) {
        this.callback = footCtrl2NameInputBoxCallback;
        return this;
    }

    public FootCtrl2NameInputBox(Context context) {
        super(context);
    }

    @Override // android.app.Dialog
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.fc2_input_name_box);
        this.editText = (EditText) findViewById(R.id.edit_name);
        ((Button) findViewById(R.id.save_but)).setOnClickListener(new View.OnClickListener() { // from class: com.cubeSuite.fragment.FootControl2.FootCtrl2NameInputBox.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                String string = FootCtrl2NameInputBox.this.editText.getText().toString();
                if (string.isEmpty()) {
                    Toast.makeText(FootCtrl2NameInputBox.this.getContext(), R.string.name_cannot_empty_hint, 0).show();
                } else {
                    FootCtrl2NameInputBox.this.dismiss();
                    FootCtrl2NameInputBox.this.callback.onSave(string);
                }
            }
        });
    }
}
