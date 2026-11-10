/*
 * CCAP referral PDF extraction, executed in the user's browser.
 * PDF.js reads text layers and renders scanned pages; Tesseract.js recognizes
 * rendered images. The PDF bytes are never sent to a third-party OCR provider.
 * Only the extracted page text is returned to CCAP for its existing field parser
 * and safety validation.
 *
 * The JavaScript/WASM engines are version-pinned and loaded from public CDNs at
 * runtime so Plesk does not need Poppler, native Tesseract, Python, Docker, or
 * server-level installation rights. The English model is served from CCAP.Web.
 * Browsers must be able to reach jsDelivr on first use (and while loading the engine).
 */
(function () {
    "use strict";

    const PDFJS_VERSION = "6.4.299";
    const TESSERACT_VERSION = "7.0.0";
    const TESSERACT_CORE_VERSION = "7.0.0";
    const PDFJS_BASE = `https://cdn.jsdelivr.net/npm/pdfjs-dist@${PDFJS_VERSION}/build`;
    const TESSERACT_BASE = `https://cdn.jsdelivr.net/npm/tesseract.js@${TESSERACT_VERSION}/dist`;
    const TESSERACT_CORE_BASE = `https://cdn.jsdelivr.net/npm/tesseract.js-core@${TESSERACT_CORE_VERSION}`;
    const TESSDATA_BASE = new URL("ocr/lang/", document.baseURI).href.replace(/\/$/, "");
    const MAX_FILE_BYTES = 10 * 1024 * 1024;
    const MAX_TEXT_CHARS = 1_000_000;
    const DEFAULT_MAX_PAGES = 25;

    function loadScriptOnce(key, url, testGlobal) {
        if (testGlobal()) return Promise.resolve();

        const promiseKey = key === "pdfjs" ? "pdfJsPromise" : "tesseractPromise";
        if (window.ccapReferralPdfOcr[promiseKey]) {
            return window.ccapReferralPdfOcr[promiseKey];
        }

        const promise = new Promise((resolve, reject) => {
            const existing = document.querySelector(`script[data-ccap-ocr="${key}"]`);
            if (existing) {
                existing.addEventListener("load", resolve, { once: true });
                existing.addEventListener("error", () => reject(new Error(`Unable to load ${key} library.`)), { once: true });
                // A script that has already completed may not fire load a second time.
                if (testGlobal()) resolve();
                return;
            }

            const script = document.createElement("script");
            script.src = url;
            script.async = true;
            script.crossOrigin = "anonymous";
            script.dataset.ccapOcr = key;
            script.onload = () => testGlobal()
                ? resolve()
                : reject(new Error(`${key} loaded, but its browser API was unavailable.`));
            script.onerror = () => {
                script.remove();
                reject(new Error(`Unable to load ${key}. Check browser internet access to the OCR libraries.`));
            };
            document.head.appendChild(script);
        });

        window.ccapReferralPdfOcr[promiseKey] = promise.catch(error => {
            window.ccapReferralPdfOcr[promiseKey] = null;
            throw error;
        });
        return window.ccapReferralPdfOcr[promiseKey];
    }

    async function ensurePdfJs() {
        const state = window.ccapReferralPdfOcr;
        if (!state.pdfJsPromise) {
            // PDF.js 6 is an ES module build. Load the pinned version directly from
            // jsDelivr; this does not send the user's PDF to the CDN.
            state.pdfJsPromise = import(`${PDFJS_BASE}/pdf.mjs`)
                .then(pdfjs => {
                    pdfjs.GlobalWorkerOptions.workerSrc = `${PDFJS_BASE}/pdf.worker.mjs`;
                    return pdfjs;
                })
                .catch(error => {
                    state.pdfJsPromise = null;
                    throw new Error(`Unable to load PDF.js. Check browser access to ${PDFJS_BASE}.`);
                });
        }
        return state.pdfJsPromise;
    }

    async function ensureTesseract() {
        await loadScriptOnce("tesseract", `${TESSERACT_BASE}/tesseract.min.js`, () => !!window.Tesseract);
        return window.Tesseract;
    }

    async function reportProgress(dotNetRef, currentPage, totalPages, stage) {
        if (!dotNetRef) return;
        try {
            await dotNetRef.invokeMethodAsync("ReportProgress", currentPage, totalPages, stage);
        } catch (_) {
            // Progress reporting is optional; extraction can continue if the UI circuit reconnects.
        }
    }

    function buildPositionedText(items) {
        const words = (items || [])
            .filter(item => item && typeof item.str === "string" && item.str.trim() &&
                Array.isArray(item.transform) && item.transform.length >= 6)
            .map(item => ({
                text: item.str,
                x: Number(item.transform[4]) || 0,
                y: Number(item.transform[5]) || 0
            }))
            .sort((a, b) => b.y - a.y || a.x - b.x);

        if (!words.length) return "";

        const rows = [];
        const yTolerance = 2.8;
        for (const word of words) {
            let row = rows.find(candidate => Math.abs(candidate.y - word.y) <= yTolerance);
            if (!row) {
                row = { y: word.y, words: [] };
                rows.push(row);
            }
            row.words.push(word);
            // Reduce accumulated y drift when PDF text items have slightly different baselines.
            row.y = row.words.reduce((sum, item) => sum + item.y, 0) / row.words.length;
        }

        const lines = [];
        for (const row of rows.sort((a, b) => b.y - a.y)) {
            let line = "";
            let currentColumn = 0;
            for (const word of row.words.sort((a, b) => a.x - b.x)) {
                const targetColumn = Math.min(180, Math.max(0, Math.round(word.x / 3.2)));
                if (targetColumn > currentColumn) {
                    line += " ".repeat(targetColumn - currentColumn);
                } else if (line.length > 0) {
                    line += " ";
                }
                line += word.text;
                currentColumn = line.length;
            }
            if (line.trim()) lines.push(line.replace(/\s+$/, ""));
        }
        return lines.join("\n");
    }

    async function createOcrWorker(Tesseract) {
        return Tesseract.createWorker("eng", 1, {
            workerPath: `${TESSERACT_BASE}/worker.min.js`,
            corePath: TESSERACT_CORE_BASE,
            langPath: TESSDATA_BASE,
            gzip: true,
            logger: () => { /* Avoid sending sensitive page content to logs. */ }
        });
    }

    async function extract(inputId, maxPages, dotNetRef) {
        const input = document.getElementById(inputId);
        const file = input && input.files && input.files[0];
        if (!file) throw new Error("Choose a referral PDF before starting extraction.");
        if (file.size <= 0) throw new Error("The selected referral PDF is empty.");
        if (file.size > MAX_FILE_BYTES) throw new Error("The referral PDF cannot exceed 10 MB.");

        const safeMaxPages = Math.max(1, Math.min(Number(maxPages) || DEFAULT_MAX_PAGES, DEFAULT_MAX_PAGES));
        const bytes = await file.arrayBuffer();
        const header = new TextDecoder("ascii").decode(bytes.slice(0, 5));
        if (header !== "%PDF-") throw new Error("The selected file does not appear to be a valid PDF.");

        await reportProgress(dotNetRef, 0, 0, "Loading browser PDF reader…");
        const pdfjsLib = await ensurePdfJs();
        const loadingTask = pdfjsLib.getDocument({
            data: new Uint8Array(bytes),
            isEvalSupported: false,
            useSystemFonts: true
        });

        let pdf;
        let worker;
        let canvas;
        let ocrUsed = false;
        let confidenceTotal = 0;
        let confidencePages = 0;
        const pageTexts = [];
        let totalCharacters = 0;

        try {
            pdf = await loadingTask.promise;
            if (pdf.numPages > safeMaxPages) {
                throw new Error(`This PDF has ${pdf.numPages} pages. CCAP can process up to ${safeMaxPages} pages per upload.`);
            }

            for (let pageNumber = 1; pageNumber <= pdf.numPages; pageNumber++) {
                const page = await pdf.getPage(pageNumber);
                let digitalText = "";
                let pageText = "";
                try {
                    const textContent = await page.getTextContent({ includeMarkedContent: false });
                    digitalText = buildPositionedText(textContent.items);
                    const compactText = digitalText.replace(/\s/g, "");
                    const hasUsableTextLayer = compactText.length >= 80 &&
                        (textContent.items || []).filter(item => item && typeof item.str === "string" && item.str.trim()).length >= 8;

                    if (hasUsableTextLayer) {
                        pageText = digitalText;
                        await reportProgress(dotNetRef, pageNumber, pdf.numPages, "Reading searchable PDF text");
                    } else {
                        ocrUsed = true;
                        await reportProgress(dotNetRef, pageNumber, pdf.numPages, "Loading OCR and rendering scanned page");
                        if (!worker) {
                            const Tesseract = await ensureTesseract();
                            worker = await createOcrWorker(Tesseract);
                        }

                        const baseViewport = page.getViewport({ scale: 1 });
                        const scale = Math.min(2.25, 2600 / Math.max(baseViewport.width, baseViewport.height));
                        const viewport = page.getViewport({ scale: Math.max(1, scale) });
                        canvas = document.createElement("canvas");
                        canvas.width = Math.ceil(viewport.width);
                        canvas.height = Math.ceil(viewport.height);
                        const context = canvas.getContext("2d", { alpha: false, willReadFrequently: true });
                        if (!context) throw new Error("This browser could not create a canvas for scanned-PDF OCR.");
                        context.fillStyle = "#ffffff";
                        context.fillRect(0, 0, canvas.width, canvas.height);
                        await page.render({ canvasContext: context, viewport }).promise;

                        const recognized = await worker.recognize(canvas);
                        const ocrText = (recognized && recognized.data && recognized.data.text || "").trim();
                        pageText = ocrText || digitalText;
                        if (recognized && recognized.data && Number.isFinite(recognized.data.confidence)) {
                            confidenceTotal += Math.max(0, Math.min(100, recognized.data.confidence)) / 100;
                            confidencePages++;
                        }

                        // Release large pixel buffers immediately; only text remains in memory.
                        canvas.width = 0;
                        canvas.height = 0;
                        canvas = null;
                    }
                } finally {
                    if (canvas) {
                        canvas.width = 0;
                        canvas.height = 0;
                        canvas = null;
                    }
                    if (page && typeof page.cleanup === "function") page.cleanup();
                }

                pageTexts.push(pageText);
                totalCharacters += pageText.length;
                if (totalCharacters > MAX_TEXT_CHARS) {
                    throw new Error("This referral produced too much extracted text to process safely. Please split the document and upload the relevant pages.");
                }
                await reportProgress(dotNetRef, pageNumber, pdf.numPages, ocrUsed ? "Extracted page text" : "Read digital text");
            }

            if (!pageTexts.some(text => text && text.trim().length > 0)) {
                throw new Error("No readable text was found. Check the scan quality and try again.");
            }

            return {
                pageTexts: pageTexts,
                ocrUsed: ocrUsed,
                confidence: confidencePages > 0 ? confidenceTotal / confidencePages : null,
                pageCount: pdf.numPages
            };
        } finally {
            if (canvas) {
                canvas.width = 0;
                canvas.height = 0;
            }
            if (worker) {
                try { await worker.terminate(); } catch (_) { /* Best-effort worker cleanup. */ }
            }
            if (pdf && typeof pdf.destroy === "function") {
                try { await pdf.destroy(); } catch (_) { /* Best-effort PDF cleanup. */ }
            } else if (loadingTask && typeof loadingTask.destroy === "function") {
                try { await loadingTask.destroy(); } catch (_) { /* Best-effort PDF cleanup. */ }
            }
        }
    }

    window.ccapReferralPdfOcr = {
        pdfJsPromise: null,
        tesseractPromise: null,
        extract: extract
    };
})();
