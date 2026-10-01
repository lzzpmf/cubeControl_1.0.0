package es.voghdev.pdfviewpager.library;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.MotionEvent;
import androidx.viewpager.widget.ViewPager;
import es.voghdev.pdfviewpager.library.adapter.PDFPagerAdapter;

/* JADX INFO: loaded from: classes.dex */
public class PDFViewPager extends ViewPager {
    protected Context context;

    public PDFViewPager(Context context, String str) {
        super(context);
        this.context = context;
        init(str);
    }

    public PDFViewPager(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.context = context;
        init(attributeSet);
    }

    protected void init(String str) {
        initAdapter(this.context, str);
    }

    protected void init(AttributeSet attributeSet) {
        if (isInEditMode()) {
            setBackgroundResource(R.drawable.flaticon_pdf_dummy);
            return;
        }
        if (attributeSet != null) {
            TypedArray typedArrayObtainStyledAttributes = this.context.obtainStyledAttributes(attributeSet, R.styleable.PDFViewPager);
            String string = typedArrayObtainStyledAttributes.getString(R.styleable.PDFViewPager_assetFileName);
            if (string != null && string.length() > 0) {
                initAdapter(this.context, string);
            }
            typedArrayObtainStyledAttributes.recycle();
        }
    }

    protected void initAdapter(Context context, String str) {
        setAdapter(new PDFPagerAdapter.Builder(context).setPdfPath(str).setOffScreenSize(getOffscreenPageLimit()).create());
    }

    @Override // androidx.viewpager.widget.ViewPager, android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        try {
            return super.onInterceptTouchEvent(motionEvent);
        } catch (IllegalArgumentException e) {
            e.printStackTrace();
            return false;
        }
    }
}
