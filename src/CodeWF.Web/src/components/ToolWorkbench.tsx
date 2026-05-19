"use client";

import { useEffect, useMemo, useState } from "react";
import QRCode from "qrcode";
import { marked } from "marked";
import { Clipboard, Play, RotateCcw } from "lucide-react";
import type { Locale } from "@/types";
import type { ToolNode } from "@/types";
import { HtmlContent } from "./HtmlContent";

const nato: Record<string, string> = {
  a: "Alpha",
  b: "Bravo",
  c: "Charlie",
  d: "Delta",
  e: "Echo",
  f: "Foxtrot",
  g: "Golf",
  h: "Hotel",
  i: "India",
  j: "Juliett",
  k: "Kilo",
  l: "Lima",
  m: "Mike",
  n: "November",
  o: "Oscar",
  p: "Papa",
  q: "Quebec",
  r: "Romeo",
  s: "Sierra",
  t: "Tango",
  u: "Uniform",
  v: "Victor",
  w: "Whiskey",
  x: "X-ray",
  y: "Yankee",
  z: "Zulu"
};

const httpCodes: Record<string, string> = {
  "200": "OK",
  "201": "Created",
  "204": "No Content",
  "301": "Moved Permanently",
  "302": "Found",
  "304": "Not Modified",
  "400": "Bad Request",
  "401": "Unauthorized",
  "403": "Forbidden",
  "404": "Not Found",
  "409": "Conflict",
  "422": "Unprocessable Content",
  "429": "Too Many Requests",
  "500": "Internal Server Error",
  "502": "Bad Gateway",
  "503": "Service Unavailable"
};

const mimeTypes: Record<string, string> = {
  json: "application/json",
  html: "text/html",
  css: "text/css",
  js: "text/javascript",
  md: "text/markdown",
  png: "image/png",
  jpg: "image/jpeg",
  jpeg: "image/jpeg",
  webp: "image/webp",
  svg: "image/svg+xml",
  pdf: "application/pdf",
  zip: "application/zip"
};

export function ToolWorkbench({ tool, locale }: { tool: ToolNode; locale: Locale }) {
  const slug = tool.slug ?? "";
  const [input, setInput] = useState("Hello CodeWF");
  const [secondInput, setSecondInput] = useState("");
  const [output, setOutput] = useState("");
  const [qr, setQr] = useState("");
  const [keyInfo, setKeyInfo] = useState("");
  const [copied, setCopied] = useState(false);
  const ui = locale === "zh-CN"
    ? {
        input: "输入",
        secondInput: "选项 / 第二输入",
        output: "输出",
        run: "运行",
        clear: "清空",
        copy: "复制",
        copied: "已复制",
        ready: "就绪。",
        keyHint: "按下任意键，当前页面聚焦时会显示按键信息。",
        regexPlaceholder: "正则表达式",
        tokenPlaceholder: "长度，默认 32"
      }
    : {
        input: "Input",
        secondInput: "Option / second input",
        output: "Output",
        run: "Run",
        clear: "Clear",
        copy: "Copy",
        copied: "Copied",
        ready: "Ready.",
        keyHint: "Press any key while this page is focused.",
        regexPlaceholder: "Regular expression",
        tokenPlaceholder: "Length, default 32"
      };

  const title = tool.name ?? slug;
  const mode = useMemo(() => detectMode(slug), [slug]);

  useEffect(() => {
    if (mode === "device") {
      setOutput(
        [
          `User agent: ${navigator.userAgent}`,
          `Language: ${navigator.language}`,
          `Platform: ${navigator.platform}`,
          `Viewport: ${window.innerWidth} x ${window.innerHeight}`,
          `Screen: ${window.screen.width} x ${window.screen.height}`,
          `Online: ${navigator.onLine ? "yes" : "no"}`
        ].join("\n")
      );
    }
  }, [mode]);

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if (mode === "keycode") {
        setKeyInfo(`key=${event.key}\ncode=${event.code}\nkeyCode=${event.keyCode}\nctrl=${event.ctrlKey}\nshift=${event.shiftKey}\nalt=${event.altKey}`);
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [mode]);

  async function run() {
    setQr("");
    setCopied(false);
    try {
      switch (mode) {
        case "json-pretty":
          setOutput(JSON.stringify(JSON.parse(input), null, 2));
          break;
        case "json-min":
          setOutput(JSON.stringify(JSON.parse(input)));
          break;
        case "json-diff":
          setOutput(diffJson(input, secondInput));
          break;
        case "url":
          setOutput(`encodeURI:\n${encodeURI(input)}\n\nencodeURIComponent:\n${encodeURIComponent(input)}\n\ndecode:\n${decodeURIComponent(input)}`);
          break;
        case "html":
          setOutput(escapeHtml(input));
          break;
        case "basic-auth":
          setOutput(`Authorization: Basic ${btoa(input)}`);
          break;
        case "jwt":
          setOutput(parseJwt(input));
          break;
        case "base64": {
          const encoded = btoa(unescape(encodeURIComponent(input)));
          const decoded = tryDecodeBase64(input);
          setOutput(`Encoded:\n${encoded}\n\nDecoded:\n${decoded ?? (locale === "zh-CN" ? "输入不是有效 Base64。" : "Input is not valid Base64.")}`);
          break;
        }
        case "case":
          setOutput(convertCases(input));
          break;
        case "binary":
          setOutput(input.split("").map((char) => char.charCodeAt(0).toString(2).padStart(8, "0")).join(" "));
          break;
        case "unicode":
          setOutput(input.split("").map((char) => `\\u${char.charCodeAt(0).toString(16).padStart(4, "0")}`).join(""));
          break;
        case "nato":
          setOutput(input.toLowerCase().split("").map((char) => nato[char] ?? char).join(" "));
          break;
        case "list":
          setOutput([...new Set(input.split(/\r?\n/).map((line) => line.trim()).filter(Boolean))].sort().join("\n"));
          break;
        case "stats":
          setOutput(textStats(input));
          break;
        case "regex":
          setOutput(regexMatches(input, secondInput || "\\w+"));
          break;
        case "markdown":
          setOutput(await marked.parse(input));
          break;
        case "hash":
          setOutput(await hashText(input));
          break;
        case "token":
          setOutput(randomToken(Number(secondInput) || 32));
          break;
        case "uuid":
          setOutput(crypto.randomUUID());
          break;
        case "ulid":
          setOutput(createUlid());
          break;
        case "timestamp":
          setOutput(timestampInfo(input));
          break;
        case "base":
          setOutput(baseConvert(input));
          break;
        case "color":
          setOutput(colorConvert(input));
          break;
        case "percentage":
          setOutput(percentage(input));
          break;
        case "compound":
          setOutput(compoundInterest(input));
          break;
        case "temperature":
          setOutput(temperature(input));
          break;
        case "port":
          setOutput(String(Math.floor(Math.random() * (65535 - 1024 + 1)) + 1024));
          break;
        case "slug":
          setOutput(slugify(input));
          break;
        case "qrcode": {
          const url = await QRCode.toDataURL(input || "https://dotnet9.com", { margin: 1, width: 280 });
          setQr(url);
          setOutput(url);
          break;
        }
        case "wifi": {
          const [ssid, password, encryption = "WPA"] = input.split("\n");
          const payload = `WIFI:T:${encryption};S:${ssid};P:${password};;`;
          const url = await QRCode.toDataURL(payload, { margin: 1, width: 280 });
          setQr(url);
          setOutput(payload);
          break;
        }
        case "svg":
          setOutput(svgPlaceholder(input));
          break;
        case "http":
          setOutput(Object.entries(httpCodes).map(([code, text]) => `${code} ${text}`).join("\n"));
          break;
        case "mime":
          setOutput(mimeLookup(input));
          break;
        case "keycode":
          setOutput(keyInfo || ui.keyHint);
          break;
        default:
          setOutput(genericTextOutput(input));
          break;
      }
    } catch (error) {
      setOutput(error instanceof Error ? error.message : String(error));
    }
  }

  function clear() {
    setInput("");
    setSecondInput("");
    setOutput("");
    setQr("");
    setCopied(false);
  }

  async function copyOutput() {
    const value = output || qr;
    if (!value) {
      return;
    }

    await navigator.clipboard.writeText(value);
    setCopied(true);
  }

  return (
    <section className="tool-workbench">
      <div className="tool-panel">
        <div className="tool-heading">
          <h2>{title}</h2>
          <p>{tool.memo}</p>
        </div>
        <label>
          {ui.input}
          <textarea value={input} onChange={(event) => setInput(event.target.value)} rows={mode === "qrcode" ? 4 : 10} />
        </label>
        {needsSecondInput(mode) ? (
          <label>
            {ui.secondInput}
            <textarea value={secondInput} onChange={(event) => setSecondInput(event.target.value)} rows={4} placeholder={secondPlaceholder(mode, ui)} />
          </label>
        ) : null}
        <div className="tool-actions">
          <button type="button" className="tool-run-button" onClick={run}>
            <Play size={16} aria-hidden="true" />
            {ui.run}
          </button>
          <button type="button" className="tool-secondary-button" onClick={clear}>
            <RotateCcw size={16} aria-hidden="true" />
            {ui.clear}
          </button>
        </div>
      </div>
      <div className="tool-panel output-panel">
        <div className="tool-output-head">
          <h2>{ui.output}</h2>
          <button type="button" className="tool-secondary-button" onClick={copyOutput} disabled={!output && !qr}>
            <Clipboard size={16} aria-hidden="true" />
            {copied ? ui.copied : ui.copy}
          </button>
        </div>
        {qr ? <img src={qr} alt="QR code" className="qr-output" /> : null}
        {mode === "markdown" && output.startsWith("<") ? (
          <HtmlContent html={output} />
        ) : (
          <pre>{output || ui.ready}</pre>
        )}
      </div>
    </section>
  );
}

function detectMode(slug: string) {
  if (slug.includes("json-diff")) return "json-diff";
  if (slug.includes("json-prettify") || slug.includes("yaml-prettify") || slug.includes("sql-prettify") || slug.includes("xml-formatter")) return "json-pretty";
  if (slug.includes("json-minify")) return "json-min";
  if (slug.includes("url-encoder") || slug.includes("url-parser") || slug.includes("safelink")) return "url";
  if (slug.includes("html-entities")) return "html";
  if (slug.includes("basic-auth")) return "basic-auth";
  if (slug.includes("jwt")) return "jwt";
  if (slug.includes("base64")) return "base64";
  if (slug.includes("case-converter")) return "case";
  if (slug.includes("binary")) return "binary";
  if (slug.includes("unicode")) return "unicode";
  if (slug.includes("nato")) return "nato";
  if (slug.includes("list-converter")) return "list";
  if (slug.includes("text-statistics")) return "stats";
  if (slug.includes("regex")) return "regex";
  if (slug.includes("markdown-to-html")) return "markdown";
  if (slug.includes("hash") || slug.includes("hmac")) return "hash";
  if (slug.includes("token")) return "token";
  if (slug.includes("uuid")) return "uuid";
  if (slug.includes("ulid")) return "ulid";
  if (slug.includes("timestamp") || slug.includes("date-converter")) return "timestamp";
  if (slug.includes("base-converter") || slug.includes("roman")) return "base";
  if (slug.includes("color")) return "color";
  if (slug.includes("percentage")) return "percentage";
  if (slug.includes("compound-interest")) return "compound";
  if (slug.includes("temperature")) return "temperature";
  if (slug.includes("random-port")) return "port";
  if (slug.includes("slugify")) return "slug";
  if (slug.includes("wifi-qrcode")) return "wifi";
  if (slug.includes("qrcode")) return "qrcode";
  if (slug.includes("svg-placeholder")) return "svg";
  if (slug.includes("device-information")) return "device";
  if (slug.includes("keycode")) return "keycode";
  if (slug.includes("http-status")) return "http";
  if (slug.includes("mime")) return "mime";
  return "generic";
}

function needsSecondInput(mode: string) {
  return ["json-diff", "regex", "token"].includes(mode);
}

function secondPlaceholder(mode: string, ui: { regexPlaceholder: string; tokenPlaceholder: string }) {
  if (mode === "regex") return ui.regexPlaceholder;
  if (mode === "token") return ui.tokenPlaceholder;
  return "Second input";
}

function escapeHtml(value: string) {
  return value.replace(/[&<>"']/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" })[char] ?? char);
}

function parseJwt(value: string) {
  const parts = value.split(".");
  return parts
    .slice(0, 2)
    .map((part, index) => `${index === 0 ? "Header" : "Payload"}:\n${JSON.stringify(JSON.parse(atob(part.replace(/-/g, "+").replace(/_/g, "/"))), null, 2)}`)
    .join("\n\n");
}

function tryDecodeBase64(value: string) {
  const normalized = value.trim();
  if (!normalized || !/^[A-Za-z0-9+/=_-]+$/.test(normalized)) {
    return null;
  }

  try {
    const padded = normalized.replace(/-/g, "+").replace(/_/g, "/").padEnd(Math.ceil(normalized.length / 4) * 4, "=");
    return decodeURIComponent(escape(atob(padded)));
  } catch {
    return null;
  }
}

function convertCases(value: string) {
  const words = value.trim().split(/[^a-zA-Z0-9]+/).filter(Boolean);
  const pascal = words.map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()).join("");
  const camel = pascal.charAt(0).toLowerCase() + pascal.slice(1);
  return [`camelCase: ${camel}`, `PascalCase: ${pascal}`, `snake_case: ${words.join("_").toLowerCase()}`, `kebab-case: ${words.join("-").toLowerCase()}`].join("\n");
}

function textStats(value: string) {
  const words = value.trim() ? value.trim().split(/\s+/).length : 0;
  return [`Characters: ${value.length}`, `Bytes: ${new Blob([value]).size}`, `Words: ${words}`, `Lines: ${value.split(/\r?\n/).length}`].join("\n");
}

function regexMatches(value: string, pattern: string) {
  const regex = new RegExp(pattern, "g");
  return [...value.matchAll(regex)].map((match) => `${match.index}: ${match[0]}`).join("\n") || "No matches.";
}

async function hashText(value: string) {
  const data = new TextEncoder().encode(value);
  const rows = await Promise.all(
    ["SHA-1", "SHA-256", "SHA-384", "SHA-512"].map(async (algorithm) => {
      const hash = await crypto.subtle.digest(algorithm, data);
      return `${algorithm}: ${[...new Uint8Array(hash)].map((byte) => byte.toString(16).padStart(2, "0")).join("")}`;
    })
  );
  return rows.join("\n");
}

function randomToken(length: number) {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  const bytes = new Uint8Array(Math.max(1, Math.min(length, 512)));
  crypto.getRandomValues(bytes);
  return [...bytes].map((byte) => chars[byte % chars.length]).join("");
}

function createUlid() {
  const time = Date.now().toString(32).padStart(10, "0");
  return `${time}${randomToken(16)}`.toUpperCase();
}

function timestampInfo(value: string) {
  const date = value.trim() ? new Date(Number(value) > 9999999999 ? Number(value) : Number(value) * 1000) : new Date();
  return [`Local: ${date.toLocaleString()}`, `ISO: ${date.toISOString()}`, `Seconds: ${Math.floor(date.getTime() / 1000)}`, `Milliseconds: ${date.getTime()}`].join("\n");
}

function baseConvert(value: string) {
  const number = Number.parseInt(value || "0", 10);
  return [`Binary: ${number.toString(2)}`, `Octal: ${number.toString(8)}`, `Decimal: ${number}`, `Hex: ${number.toString(16).toUpperCase()}`, `Base36: ${number.toString(36)}`].join("\n");
}

function colorConvert(value: string) {
  const hex = value.replace("#", "").trim();
  const normalized = hex.length === 3 ? hex.split("").map((char) => char + char).join("") : hex.padEnd(6, "0").slice(0, 6);
  const r = Number.parseInt(normalized.slice(0, 2), 16);
  const g = Number.parseInt(normalized.slice(2, 4), 16);
  const b = Number.parseInt(normalized.slice(4, 6), 16);
  return [`HEX: #${normalized.toUpperCase()}`, `RGB: rgb(${r}, ${g}, ${b})`, `HSL: ${rgbToHsl(r, g, b)}`].join("\n");
}

function rgbToHsl(r: number, g: number, b: number) {
  r /= 255;
  g /= 255;
  b /= 255;
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  let h = 0;
  let s = 0;
  const l = (max + min) / 2;
  if (max !== min) {
    const delta = max - min;
    s = l > 0.5 ? delta / (2 - max - min) : delta / (max + min);
    h = max === r ? (g - b) / delta + (g < b ? 6 : 0) : max === g ? (b - r) / delta + 2 : (r - g) / delta + 4;
    h /= 6;
  }
  return `hsl(${Math.round(h * 360)}, ${Math.round(s * 100)}%, ${Math.round(l * 100)}%)`;
}

function percentage(value: string) {
  const [a = 0, b = 0] = value.split(/[,\s]+/).map(Number);
  return [`${a} is ${b ? ((a / b) * 100).toFixed(2) : "0"}% of ${b}`, `${b}% of ${a} = ${((a * b) / 100).toFixed(2)}`, `Change: ${a ? (((b - a) / a) * 100).toFixed(2) : "0"}%`].join("\n");
}

function compoundInterest(value: string) {
  const [principal = 10000, rate = 5, years = 10] = value.split(/[,\s]+/).map(Number);
  const amount = principal * Math.pow(1 + rate / 100, years);
  return `Final: ${amount.toFixed(2)}\nInterest: ${(amount - principal).toFixed(2)}`;
}

function temperature(value: string) {
  const c = Number(value || "0");
  return [`Celsius: ${c.toFixed(2)} C`, `Fahrenheit: ${(c * 1.8 + 32).toFixed(2)} F`, `Kelvin: ${(c + 273.15).toFixed(2)} K`].join("\n");
}

function slugify(value: string) {
  return value.trim().toLowerCase().normalize("NFKD").replace(/[^\p{Letter}\p{Number}]+/gu, "-").replace(/^-+|-+$/g, "");
}

function svgPlaceholder(value: string) {
  const [width = "800", height = "450", text = "CodeWF"] = value.split(/[,\n]/).map((item) => item.trim());
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}"><rect width="100%" height="100%" fill="#f1f5f9"/><text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" fill="#334155" font-family="system-ui" font-size="32">${escapeHtml(text)}</text></svg>`;
}

function mimeLookup(value: string) {
  const key = value.replace(".", "").trim().toLowerCase();
  if (!key) {
    return Object.entries(mimeTypes).map(([ext, type]) => `.${ext}: ${type}`).join("\n");
  }
  return mimeTypes[key] ?? "Unknown";
}

function diffJson(left: string, right: string) {
  const a = JSON.parse(left);
  const b = JSON.parse(right || "{}");
  const keys = new Set([...Object.keys(a), ...Object.keys(b)]);
  return [...keys].map((key) => `${key}: ${JSON.stringify(a[key])} -> ${JSON.stringify(b[key])}`).join("\n");
}

function genericTextOutput(value: string) {
  return [`Original:\n${value}`, `\nUpper:\n${value.toUpperCase()}`, `\nLower:\n${value.toLowerCase()}`, `\nLength: ${value.length}`].join("\n");
}
