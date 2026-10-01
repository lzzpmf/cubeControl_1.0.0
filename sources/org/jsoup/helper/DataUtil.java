package org.jsoup.helper;

import java.io.BufferedReader;
import java.io.CharArrayReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.nio.ByteBuffer;
import java.nio.CharBuffer;
import java.nio.charset.Charset;
import java.nio.charset.IllegalCharsetNameException;
import java.util.Locale;
import java.util.Random;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.jsoup.UncheckedIOException;
import org.jsoup.internal.ConstrainableInputStream;
import org.jsoup.internal.StringUtil;
import org.jsoup.nodes.Comment;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import org.jsoup.nodes.Node;
import org.jsoup.nodes.XmlDeclaration;
import org.jsoup.parser.Parser;

/* JADX INFO: loaded from: classes2.dex */
public final class DataUtil {
    static final int boundaryLength = 32;
    static final int bufferSize = 32768;
    static final String defaultCharset = "UTF-8";
    private static final int firstReadBufferSize = 5120;
    private static final Pattern charsetPattern = Pattern.compile("(?i)\\bcharset=\\s*(?:[\"'])?([^\\s,;\"']*)");
    private static final char[] mimeBoundaryChars = "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".toCharArray();

    private DataUtil() {
    }

    public static Document load(File file, String str, String str2) throws IOException {
        return parseInputStream(new FileInputStream(file), str, str2, Parser.htmlParser());
    }

    public static Document load(InputStream inputStream, String str, String str2) throws IOException {
        return parseInputStream(inputStream, str, str2, Parser.htmlParser());
    }

    public static Document load(InputStream inputStream, String str, String str2, Parser parser) throws IOException {
        return parseInputStream(inputStream, str, str2, parser);
    }

    static void crossStreams(InputStream inputStream, OutputStream outputStream) throws IOException {
        byte[] bArr = new byte[32768];
        while (true) {
            int i = inputStream.read(bArr);
            if (i == -1) {
                return;
            } else {
                outputStream.write(bArr, 0, i);
            }
        }
    }

    /* JADX WARN: Code duplicated, block: B:42:0x00c0  */
    static Document parseInputStream(InputStream inputStream, String str, String str2, Parser parser) throws IOException {
        Document input;
        XmlDeclaration xmlDeclarationAsXmlDeclaration;
        if (inputStream == null) {
            return new Document(str2);
        }
        ConstrainableInputStream constrainableInputStreamWrap = ConstrainableInputStream.wrap(inputStream, 32768, 0);
        constrainableInputStreamWrap.mark(32768);
        ByteBuffer toByteBuffer = readToByteBuffer(constrainableInputStreamWrap, 5119);
        boolean z = constrainableInputStreamWrap.read() == -1;
        constrainableInputStreamWrap.reset();
        BomCharset bomCharsetDetectCharsetFromBom = detectCharsetFromBom(toByteBuffer);
        String strReplaceAll = bomCharsetDetectCharsetFromBom != null ? bomCharsetDetectCharsetFromBom.charset : str;
        Document input2 = null;
        if (strReplaceAll == null) {
            try {
                CharBuffer charBufferDecode = Charset.forName(defaultCharset).decode(toByteBuffer);
                if (charBufferDecode.hasArray()) {
                    input = parser.parseInput(new CharArrayReader(charBufferDecode.array()), str2);
                } else {
                    input = parser.parseInput(charBufferDecode.toString(), str2);
                }
                String strAttr = null;
                for (Element element : input.select("meta[http-equiv=content-type], meta[charset]")) {
                    if (element.hasAttr("http-equiv")) {
                        strAttr = getCharsetFromContentType(element.attr("content"));
                    }
                    if (strAttr == null && element.hasAttr("charset")) {
                        strAttr = element.attr("charset");
                    }
                    if (strAttr != null) {
                        break;
                    }
                }
                if (strAttr == null && input.childNodeSize() > 0) {
                    Node nodeChildNode = input.childNode(0);
                    if (nodeChildNode instanceof XmlDeclaration) {
                        xmlDeclarationAsXmlDeclaration = (XmlDeclaration) nodeChildNode;
                    } else if (nodeChildNode instanceof Comment) {
                        Comment comment = (Comment) nodeChildNode;
                        if (comment.isXmlDeclaration()) {
                            xmlDeclarationAsXmlDeclaration = comment.asXmlDeclaration();
                        } else {
                            xmlDeclarationAsXmlDeclaration = null;
                        }
                    } else {
                        xmlDeclarationAsXmlDeclaration = null;
                    }
                    if (xmlDeclarationAsXmlDeclaration != null && xmlDeclarationAsXmlDeclaration.name().equalsIgnoreCase("xml")) {
                        strAttr = xmlDeclarationAsXmlDeclaration.attr("encoding");
                    }
                }
                String strValidateCharset = validateCharset(strAttr);
                if (strValidateCharset != null && !strValidateCharset.equalsIgnoreCase(defaultCharset)) {
                    strReplaceAll = strValidateCharset.trim().replaceAll("[\"']", "");
                } else if (z) {
                    input2 = input;
                }
            } catch (UncheckedIOException e) {
                throw e.ioException();
            }
        } else {
            Validate.notEmpty(strReplaceAll, "Must set charset arg to character set of file to parse. Set to null to attempt to detect from HTML");
        }
        if (input2 == null) {
            if (strReplaceAll == null) {
                strReplaceAll = defaultCharset;
            }
            BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(constrainableInputStreamWrap, strReplaceAll), 32768);
            if (bomCharsetDetectCharsetFromBom != null && bomCharsetDetectCharsetFromBom.offset) {
                Validate.isTrue(bufferedReader.skip(1L) == 1);
            }
            try {
                input2 = parser.parseInput(bufferedReader, str2);
                Charset charsetForName = Charset.forName(strReplaceAll);
                input2.outputSettings().charset(charsetForName);
                if (!charsetForName.canEncode()) {
                    input2.charset(Charset.forName(defaultCharset));
                }
            } catch (UncheckedIOException e2) {
                throw e2.ioException();
            }
        }
        constrainableInputStreamWrap.close();
        return input2;
    }

    public static ByteBuffer readToByteBuffer(InputStream inputStream, int i) throws IOException {
        Validate.isTrue(i >= 0, "maxSize must be 0 (unlimited) or larger");
        return ConstrainableInputStream.wrap(inputStream, 32768, i).readToByteBuffer(i);
    }

    static ByteBuffer emptyByteBuffer() {
        return ByteBuffer.allocate(0);
    }

    static String getCharsetFromContentType(String str) {
        if (str == null) {
            return null;
        }
        Matcher matcher = charsetPattern.matcher(str);
        if (matcher.find()) {
            return validateCharset(matcher.group(1).trim().replace("charset=", ""));
        }
        return null;
    }

    private static String validateCharset(String str) {
        if (str != null && str.length() != 0) {
            String strReplaceAll = str.trim().replaceAll("[\"']", "");
            try {
                if (Charset.isSupported(strReplaceAll)) {
                    return strReplaceAll;
                }
                String upperCase = strReplaceAll.toUpperCase(Locale.ENGLISH);
                if (Charset.isSupported(upperCase)) {
                    return upperCase;
                }
            } catch (IllegalCharsetNameException unused) {
            }
        }
        return null;
    }

    static String mimeBoundary() {
        StringBuilder sbBorrowBuilder = StringUtil.borrowBuilder();
        Random random = new Random();
        for (int i = 0; i < 32; i++) {
            char[] cArr = mimeBoundaryChars;
            sbBorrowBuilder.append(cArr[random.nextInt(cArr.length)]);
        }
        return StringUtil.releaseBuilder(sbBorrowBuilder);
    }

    private static BomCharset detectCharsetFromBom(ByteBuffer byteBuffer) {
        byteBuffer.mark();
        byte[] bArr = new byte[4];
        if (byteBuffer.remaining() >= 4) {
            byteBuffer.get(bArr);
            byteBuffer.rewind();
        }
        byte b = bArr[0];
        if ((b == 0 && bArr[1] == 0 && bArr[2] == -2 && bArr[3] == -1) || (b == -1 && bArr[1] == -2 && bArr[2] == 0 && bArr[3] == 0)) {
            return new BomCharset("UTF-32", false);
        }
        if ((b == -2 && bArr[1] == -1) || (b == -1 && bArr[1] == -2)) {
            return new BomCharset("UTF-16", false);
        }
        if (b == -17 && bArr[1] == -69 && bArr[2] == -65) {
            return new BomCharset(defaultCharset, true);
        }
        return null;
    }

    private static class BomCharset {
        private final String charset;
        private final boolean offset;

        public BomCharset(String str, boolean z) {
            this.charset = str;
            this.offset = z;
        }
    }
}
