package com.bigkoo.pickerview.utils;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.GregorianCalendar;
import java.util.Locale;
import okio.SegmentPool;

/* JADX INFO: loaded from: classes.dex */
public class ChinaDate {
    private static final long[] lunarInfo = {19416, 19168, 42352, 21717, 53856, 55632, 91476, 22176, 39632, 21970, 19168, 42422, 42192, 53840, 119381, 46400, 54944, 44450, 38320, 84343, 18800, 42160, 46261, 27216, 27968, 109396, 11104, 38256, 21234, 18800, 25958, 54432, 59984, 28309, 23248, 11104, 100067, 37600, 116951, 51536, 54432, 120998, 46416, 22176, 107956, 9680, 37584, 53938, 43344, 46423, 27808, 46416, 86869, 19872, 42416, 83315, 21168, 43432, 59728, 27296, 44710, 43856, 19296, 43748, 42352, 21088, 62051, 55632, 23383, 22176, 38608, 19925, 19152, 42192, 54484, 53840, 54616, 46400, 46752, 103846, 38320, 18864, 43380, 42160, 45690, 27216, 27968, 44870, 43872, 38256, 19189, 18800, 25776, 29859, 59984, 27480, 21952, 43872, 38613, 37600, 51552, 55636, 54432, 55888, 30034, 22176, 43959, 9680, 37584, 51893, 43344, 46240, 47780, 44368, 21977, 19360, 42416, 86390, 21168, 43312, 31060, 27296, 44368, 23378, 19296, 42726, 42208, 53856, 60005, 54576, 23200, 30371, 38608, 19195, 19152, 42192, 118966, 53840, 54560, 56645, 46496, 22224, 21938, 18864, 42359, 42160, 43600, 111189, 27936, 44448, 84835, 37744, 18936, 18800, 25776, 92326, 59984, 27424, 108228, 43744, 41696, 53987, 51552, 54615, 54432, 55888, 23893, 22176, 42704, 21972, 21200, 43448, 43344, 46240, 46758, 44368, 21920, 43940, 42416, 21168, 45683, 26928, 29495, 27296, 44368, 84821, 19296, 42352, 21732, 53600, 59752, 54560, 55968, 92838, 22224, 19168, 43476, 41680, 53584, 62034, 54560};
    private static final String[] nStr1 = {"", "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊"};
    private static final String[] Gan = {"甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"};
    private static final String[] Zhi = {"子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"};
    private static final String[] Animals = {"鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪"};
    private static SimpleDateFormat sdf = new SimpleDateFormat("yyyy年M月d日 EEEEE");

    private static final int lYearDays(int i) {
        int i2 = 348;
        for (int i3 = 32768; i3 > 8; i3 >>= 1) {
            if ((lunarInfo[i - 1900] & ((long) i3)) != 0) {
                i2++;
            }
        }
        return i2 + leapDays(i);
    }

    public static final int leapDays(int i) {
        if (leapMonth(i) != 0) {
            return (lunarInfo[i + (-1900)] & SegmentPool.MAX_SIZE) != 0 ? 30 : 29;
        }
        return 0;
    }

    public static final int leapMonth(int i) {
        return (int) (lunarInfo[i - 1900] & 15);
    }

    public static final int monthDays(int i, int i2) {
        return (((long) (65536 >> i2)) & lunarInfo[i + (-1900)]) == 0 ? 29 : 30;
    }

    public static final String AnimalsYear(int i) {
        return Animals[(i - 4) % 12];
    }

    private static final String cyclicalm(int i) {
        return Gan[i % 10] + Zhi[i % 12];
    }

    public static final String cyclical(int i) {
        return cyclicalm(i - 1864);
    }

    public static final long[] calElement(int i, int i2, int i3) {
        char c;
        long[] jArr = new long[7];
        int i4 = LunarCalendar.MIN_YEAR;
        char c2 = 0;
        long time = (new GregorianCalendar(i, i2 - 1, i3).getTime().getTime() - new GregorianCalendar(LunarCalendar.MIN_YEAR, 0, 31).getTime().getTime()) / 86400000;
        jArr[5] = 40 + time;
        jArr[4] = 14;
        int iMonthDays = 0;
        while (i4 < 2100 && time > 0) {
            iMonthDays = lYearDays(i4);
            time -= (long) iMonthDays;
            jArr[4] = jArr[4] + 12;
            i4++;
        }
        if (time < 0) {
            time += (long) iMonthDays;
            i4--;
            jArr[4] = jArr[4] - 12;
        }
        jArr[0] = i4;
        jArr[3] = i4 - 1864;
        int iLeapMonth = leapMonth(i4);
        char c3 = 6;
        jArr[6] = 0;
        int i5 = 1;
        while (i5 < 13 && time > 0) {
            if (iLeapMonth > 0 && i5 == iLeapMonth + 1 && jArr[c3] == 0) {
                i5--;
                jArr[c3] = 1;
                c = c2;
                iMonthDays = leapDays((int) jArr[c]);
            } else {
                c = c2;
                iMonthDays = monthDays((int) jArr[c], i5);
            }
            if (jArr[c3] == 1 && i5 == iLeapMonth + 1) {
                jArr[c3] = 0;
            }
            time -= (long) iMonthDays;
            if (jArr[c3] == 0) {
                jArr[4] = jArr[4] + 1;
            }
            i5++;
            c3 = c3;
            c2 = c;
        }
        char c4 = c3;
        if (time == 0 && iLeapMonth > 0 && i5 == iLeapMonth + 1) {
            if (jArr[c4] == 1) {
                jArr[c4] = 0;
            } else {
                jArr[c4] = 1;
                i5--;
                jArr[4] = jArr[4] - 1;
            }
        }
        if (time < 0) {
            time += (long) iMonthDays;
            i5--;
            jArr[4] = jArr[4] - 1;
        }
        jArr[1] = i5;
        jArr[2] = time + 1;
        return jArr;
    }

    public static final String getChinaDate(int i) {
        String str;
        if (i == 10) {
            return "初十";
        }
        if (i == 20) {
            return "二十";
        }
        if (i == 30) {
            return "三十";
        }
        int i2 = i / 10;
        if (i2 != 0) {
            str = "";
        } else {
            str = "初";
        }
        if (i2 == 1) {
            str = "十";
        }
        if (i2 == 2) {
            str = "廿";
        }
        if (i2 == 3) {
            str = "三";
        }
        switch (i % 10) {
            case 1:
                return str.concat("一");
            case 2:
                return str.concat("二");
            case 3:
                return str.concat("三");
            case 4:
                return str.concat("四");
            case 5:
                return str.concat("五");
            case 6:
                return str.concat("六");
            case 7:
                return str.concat("七");
            case 8:
                return str.concat("八");
            case 9:
                return str.concat("九");
            default:
                return str;
        }
    }

    public static String getCurrentLunarDate() {
        Calendar calendar = Calendar.getInstance(Locale.SIMPLIFIED_CHINESE);
        int i = calendar.get(1);
        long[] jArrCalElement = calElement(i, calendar.get(2) + 1, calendar.get(5));
        StringBuffer stringBuffer = new StringBuffer();
        stringBuffer.append(sdf.format(calendar.getTime()));
        stringBuffer.append(" 农历");
        stringBuffer.append(cyclical(i));
        stringBuffer.append('(');
        stringBuffer.append(AnimalsYear(i));
        stringBuffer.append(")年");
        stringBuffer.append(nStr1[(int) jArrCalElement[1]]);
        stringBuffer.append("月");
        stringBuffer.append(getChinaDate((int) jArrCalElement[2]));
        return stringBuffer.toString();
    }

    public static String oneDay(int i, int i2, int i3) {
        long[] jArrCalElement = calElement(i, i2, i3);
        StringBuffer stringBuffer = new StringBuffer(" 农历");
        stringBuffer.append(cyclical(i));
        stringBuffer.append('(');
        stringBuffer.append(AnimalsYear(i));
        stringBuffer.append(")年");
        stringBuffer.append(nStr1[(int) jArrCalElement[1]]);
        stringBuffer.append("月");
        stringBuffer.append(getChinaDate((int) jArrCalElement[2]));
        return stringBuffer.toString();
    }

    public static String getLunarYearText(int i) {
        int i2 = i - 4;
        return Gan[i2 % 10] + Zhi[i2 % 12] + "年";
    }

    public static ArrayList<String> getYears(int i, int i2) {
        ArrayList<String> arrayList = new ArrayList<>();
        while (i < i2) {
            arrayList.add(String.format("%s(%d)", getLunarYearText(i), Integer.valueOf(i)));
            i++;
        }
        return arrayList;
    }

    public static ArrayList<String> getMonths(int i) {
        String[] strArr;
        ArrayList<String> arrayList = new ArrayList<>();
        int i2 = 1;
        while (true) {
            strArr = nStr1;
            if (i2 >= strArr.length) {
                break;
            }
            arrayList.add(strArr[i2] + "月");
            i2++;
        }
        if (leapMonth(i) != 0) {
            arrayList.add(leapMonth(i), "闰" + strArr[leapMonth(i)] + "月");
        }
        return arrayList;
    }

    public static ArrayList<String> getLunarDays(int i) {
        ArrayList<String> arrayList = new ArrayList<>();
        for (int i2 = 1; i2 <= i; i2++) {
            arrayList.add(getChinaDate(i2));
        }
        return arrayList;
    }
}
