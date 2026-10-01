package com.cubeSuite;

import android.util.SparseArray;
import android.util.SparseIntArray;
import android.view.View;
import androidx.databinding.DataBinderMapper;
import androidx.databinding.DataBindingComponent;
import androidx.databinding.ViewDataBinding;
import com.cubeSuite.databinding.ArpFragmentBindingImpl;
import com.cubeSuite.databinding.EqViewBindingImpl;
import com.cubeSuite.databinding.GlobFragmentBindingImpl;
import com.cubeSuite.databinding.IrBoxContorlFragmentBindingImpl;
import com.cubeSuite.databinding.MidikeyboardControlFragmentBindingImpl;
import com.cubeSuite.databinding.PresetsFragmentBindingImpl;
import com.cubeSuite.databinding.ScChFragmentBindingImpl;
import com.cubeSuite.databinding.Smk25MiniGlobFragmentBindingImpl;
import com.cubeSuite.databinding.Smk25iiPresetsBindingImpl;
import com.cubeSuite.databinding.Smk25iiUsrFBindingImpl;
import com.cubeSuite.databinding.Smk25miniArpFragmentBindingImpl;
import com.cubeSuite.databinding.Smk25miniPresetsFragmentBindingImpl;
import com.cubeSuite.databinding.Smk25miniScChFragmentBindingImpl;
import com.cubeSuite.databinding.Smk25miniUsrFragmentBindingImpl;
import com.cubeSuite.databinding.UsrFragmentBindingImpl;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class DataBinderMapperImpl extends DataBinderMapper {
    private static final SparseIntArray INTERNAL_LAYOUT_ID_LOOKUP;
    private static final int LAYOUT_ARPFRAGMENT = 1;
    private static final int LAYOUT_EQVIEW = 2;
    private static final int LAYOUT_GLOBFRAGMENT = 3;
    private static final int LAYOUT_IRBOXCONTORLFRAGMENT = 4;
    private static final int LAYOUT_MIDIKEYBOARDCONTROLFRAGMENT = 5;
    private static final int LAYOUT_PRESETSFRAGMENT = 6;
    private static final int LAYOUT_SCCHFRAGMENT = 7;
    private static final int LAYOUT_SMK25IIPRESETS = 9;
    private static final int LAYOUT_SMK25IIUSRF = 10;
    private static final int LAYOUT_SMK25MINIARPFRAGMENT = 11;
    private static final int LAYOUT_SMK25MINIGLOBFRAGMENT = 8;
    private static final int LAYOUT_SMK25MINIPRESETSFRAGMENT = 12;
    private static final int LAYOUT_SMK25MINISCCHFRAGMENT = 13;
    private static final int LAYOUT_SMK25MINIUSRFRAGMENT = 14;
    private static final int LAYOUT_USRFRAGMENT = 15;

    static {
        SparseIntArray sparseIntArray = new SparseIntArray(15);
        INTERNAL_LAYOUT_ID_LOOKUP = sparseIntArray;
        sparseIntArray.put(R.layout.arp_fragment, 1);
        sparseIntArray.put(R.layout.eq_view, 2);
        sparseIntArray.put(R.layout.glob_fragment, 3);
        sparseIntArray.put(R.layout.ir_box_contorl_fragment, 4);
        sparseIntArray.put(R.layout.midikeyboard_control_fragment, 5);
        sparseIntArray.put(R.layout.presets_fragment, 6);
        sparseIntArray.put(R.layout.sc_ch_fragment, 7);
        sparseIntArray.put(R.layout.smk25_mini_glob_fragment, 8);
        sparseIntArray.put(R.layout.smk25ii_presets, 9);
        sparseIntArray.put(R.layout.smk25ii_usr_f, 10);
        sparseIntArray.put(R.layout.smk25mini_arp_fragment, 11);
        sparseIntArray.put(R.layout.smk25mini_presets_fragment, 12);
        sparseIntArray.put(R.layout.smk25mini_sc_ch_fragment, 13);
        sparseIntArray.put(R.layout.smk25mini_usr_fragment, 14);
        sparseIntArray.put(R.layout.usr_fragment, 15);
    }

    @Override // androidx.databinding.DataBinderMapper
    public ViewDataBinding getDataBinder(DataBindingComponent dataBindingComponent, View view, int i) {
        int i2 = INTERNAL_LAYOUT_ID_LOOKUP.get(i);
        if (i2 <= 0) {
            return null;
        }
        Object tag = view.getTag();
        if (tag == null) {
            throw new RuntimeException("view must have a tag");
        }
        switch (i2) {
            case 1:
                if ("layout/arp_fragment_0".equals(tag)) {
                    return new ArpFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for arp_fragment is invalid. Received: " + tag);
            case 2:
                if ("layout/eq_view_0".equals(tag)) {
                    return new EqViewBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for eq_view is invalid. Received: " + tag);
            case 3:
                if ("layout/glob_fragment_0".equals(tag)) {
                    return new GlobFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for glob_fragment is invalid. Received: " + tag);
            case 4:
                if ("layout/ir_box_contorl_fragment_0".equals(tag)) {
                    return new IrBoxContorlFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for ir_box_contorl_fragment is invalid. Received: " + tag);
            case 5:
                if ("layout/midikeyboard_control_fragment_0".equals(tag)) {
                    return new MidikeyboardControlFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for midikeyboard_control_fragment is invalid. Received: " + tag);
            case 6:
                if ("layout/presets_fragment_0".equals(tag)) {
                    return new PresetsFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for presets_fragment is invalid. Received: " + tag);
            case 7:
                if ("layout/sc_ch_fragment_0".equals(tag)) {
                    return new ScChFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for sc_ch_fragment is invalid. Received: " + tag);
            case 8:
                if ("layout/smk25_mini_glob_fragment_0".equals(tag)) {
                    return new Smk25MiniGlobFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25_mini_glob_fragment is invalid. Received: " + tag);
            case 9:
                if ("layout/smk25ii_presets_0".equals(tag)) {
                    return new Smk25iiPresetsBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25ii_presets is invalid. Received: " + tag);
            case 10:
                if ("layout/smk25ii_usr_f_0".equals(tag)) {
                    return new Smk25iiUsrFBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25ii_usr_f is invalid. Received: " + tag);
            case 11:
                if ("layout/smk25mini_arp_fragment_0".equals(tag)) {
                    return new Smk25miniArpFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25mini_arp_fragment is invalid. Received: " + tag);
            case 12:
                if ("layout/smk25mini_presets_fragment_0".equals(tag)) {
                    return new Smk25miniPresetsFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25mini_presets_fragment is invalid. Received: " + tag);
            case 13:
                if ("layout/smk25mini_sc_ch_fragment_0".equals(tag)) {
                    return new Smk25miniScChFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25mini_sc_ch_fragment is invalid. Received: " + tag);
            case 14:
                if ("layout/smk25mini_usr_fragment_0".equals(tag)) {
                    return new Smk25miniUsrFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for smk25mini_usr_fragment is invalid. Received: " + tag);
            case 15:
                if ("layout/usr_fragment_0".equals(tag)) {
                    return new UsrFragmentBindingImpl(dataBindingComponent, view);
                }
                throw new IllegalArgumentException("The tag for usr_fragment is invalid. Received: " + tag);
            default:
                return null;
        }
    }

    @Override // androidx.databinding.DataBinderMapper
    public ViewDataBinding getDataBinder(DataBindingComponent dataBindingComponent, View[] viewArr, int i) {
        if (viewArr == null || viewArr.length == 0 || INTERNAL_LAYOUT_ID_LOOKUP.get(i) <= 0 || viewArr[0].getTag() != null) {
            return null;
        }
        throw new RuntimeException("view must have a tag");
    }

    @Override // androidx.databinding.DataBinderMapper
    public int getLayoutId(String str) {
        Integer num;
        if (str == null || (num = InnerLayoutIdLookup.sKeys.get(str)) == null) {
            return 0;
        }
        return num.intValue();
    }

    @Override // androidx.databinding.DataBinderMapper
    public String convertBrIdToString(int i) {
        return InnerBrLookup.sKeys.get(i);
    }

    @Override // androidx.databinding.DataBinderMapper
    public List<DataBinderMapper> collectDependencies() {
        ArrayList arrayList = new ArrayList(1);
        arrayList.add(new androidx.databinding.library.baseAdapters.DataBinderMapperImpl());
        return arrayList;
    }

    private static class InnerBrLookup {
        static final SparseArray<String> sKeys;

        private InnerBrLookup() {
        }

        static {
            SparseArray<String> sparseArray = new SparseArray<>(22);
            sKeys = sparseArray;
            sparseArray.put(0, "_all");
            sparseArray.put(1, "byteData");
            sparseArray.put(2, "data");
            sparseArray.put(3, "globData");
            sparseArray.put(4, "keyBedMcpStr");
            sparseArray.put(5, "knobCCStr");
            sparseArray.put(6, "knobIndex");
            sparseArray.put(7, "knobMaxText");
            sparseArray.put(8, "knobMinText");
            sparseArray.put(9, "listener");
            sparseArray.put(10, "modCCStr");
            sparseArray.put(11, "padIndex");
            sparseArray.put(12, "padMaxText");
            sparseArray.put(13, "padMinText");
            sparseArray.put(14, "padNoteStr");
            sparseArray.put(15, "padNoteText");
            sparseArray.put(16, "psrCCStr");
            sparseArray.put(17, "psrCCText");
            sparseArray.put(18, "psrIndex");
            sparseArray.put(19, "psrValue1Text");
            sparseArray.put(20, "psrValue2Text");
            sparseArray.put(21, "usrBinding");
        }
    }

    private static class InnerLayoutIdLookup {
        static final HashMap<String, Integer> sKeys;

        private InnerLayoutIdLookup() {
        }

        static {
            HashMap<String, Integer> map = new HashMap<>(15);
            sKeys = map;
            map.put("layout/arp_fragment_0", Integer.valueOf(R.layout.arp_fragment));
            map.put("layout/eq_view_0", Integer.valueOf(R.layout.eq_view));
            map.put("layout/glob_fragment_0", Integer.valueOf(R.layout.glob_fragment));
            map.put("layout/ir_box_contorl_fragment_0", Integer.valueOf(R.layout.ir_box_contorl_fragment));
            map.put("layout/midikeyboard_control_fragment_0", Integer.valueOf(R.layout.midikeyboard_control_fragment));
            map.put("layout/presets_fragment_0", Integer.valueOf(R.layout.presets_fragment));
            map.put("layout/sc_ch_fragment_0", Integer.valueOf(R.layout.sc_ch_fragment));
            map.put("layout/smk25_mini_glob_fragment_0", Integer.valueOf(R.layout.smk25_mini_glob_fragment));
            map.put("layout/smk25ii_presets_0", Integer.valueOf(R.layout.smk25ii_presets));
            map.put("layout/smk25ii_usr_f_0", Integer.valueOf(R.layout.smk25ii_usr_f));
            map.put("layout/smk25mini_arp_fragment_0", Integer.valueOf(R.layout.smk25mini_arp_fragment));
            map.put("layout/smk25mini_presets_fragment_0", Integer.valueOf(R.layout.smk25mini_presets_fragment));
            map.put("layout/smk25mini_sc_ch_fragment_0", Integer.valueOf(R.layout.smk25mini_sc_ch_fragment));
            map.put("layout/smk25mini_usr_fragment_0", Integer.valueOf(R.layout.smk25mini_usr_fragment));
            map.put("layout/usr_fragment_0", Integer.valueOf(R.layout.usr_fragment));
        }
    }
}
