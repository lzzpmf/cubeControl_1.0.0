package es.voghdev.pdfviewpager.library;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.MotionEvent;
import es.voghdev.pdfviewpager.library.adapter.PDFPagerAdapter;

/* JADX INFO: loaded from: classes.dex */
public class PDFViewPagerZoom extends PDFViewPager {
    public PDFViewPagerZoom(Context context, String str) {
        super(context, str);
    }

    public PDFViewPagerZoom(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
    }

    @Override // es.voghdev.pdfviewpager.library.PDFViewPager
    protected void initAdapter(Context context, String str) {
        setAdapter(new PDFPagerAdapter.Builder(context).setPdfPath(str).setOffScreenSize(getOffscreenPageLimit()).create());
    }

    @Override // es.voghdev.pdfviewpager.library.PDFViewPager
    protected void init(AttributeSet attributeSet) {
        if (isInEditMode()) {
            setBackgroundResource(R.drawable.flaticon_pdf_dummy);
            return;
        }
        if (attributeSet != null) {
            TypedArray typedArrayObtainStyledAttributes = this.context.obtainStyledAttributes(attributeSet, R.styleable.PDFViewPager);
            String string = typedArrayObtainStyledAttributes.getString(R.styleable.PDFViewPager_assetFileName);
            float f = typedArrayObtainStyledAttributes.getFloat(R.styleable.PDFViewPager_scale, 1.0f);
            if (string != null && string.length() > 0) {
                setAdapter(new PDFPagerAdapter.Builder(this.context).setPdfPath(string).setScale(f).setOffScreenSize(getOffscreenPageLimit()).create());
            }
            typedArrayObtainStyledAttributes.recycle();
        }
    }

    @Override // es.voghdev.pdfviewpager.library.PDFViewPager, androidx.viewpager.widget.ViewPager, android.view.ViewGroup
    public boolean onInterceptTouchEvent(MotionEvent motionEvent) {
        try {
            return super.onInterceptTouchEvent(motionEvent);
        } catch (IllegalArgumentException e) {
            e.printStackTrace();
            return false;
        }
    }
}
