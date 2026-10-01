package org.jsoup.nodes;

import org.jsoup.parser.Tag;

/* JADX INFO: loaded from: classes2.dex */
public class PseudoTextElement extends Element {
    @Override // org.jsoup.nodes.Element, org.jsoup.nodes.Node
    void outerHtmlHead(Appendable appendable, int i, Document.OutputSettings outputSettings) {
    }

    @Override // org.jsoup.nodes.Element, org.jsoup.nodes.Node
    void outerHtmlTail(Appendable appendable, int i, Document.OutputSettings outputSettings) {
    }

    public PseudoTextElement(Tag tag, String str, Attributes attributes) {
        super(tag, str, attributes);
    }
}
