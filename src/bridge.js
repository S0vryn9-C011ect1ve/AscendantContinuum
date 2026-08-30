// ponytail: tiny WebView/TWA bridge surface, no framework
window.ACBridge = {
  isAndroid: () => /Android/.test(navigator.userAgent),
  share: (payload) => {
    if (window.ACBridge._nativeShare) return window.ACBridge._nativeShare(JSON.stringify(payload));
    const blob = new Blob([JSON.stringify(payload)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'ascendant-share.json'; document.body.appendChild(a); a.click();
    document.body.removeChild(a); URL.revokeObjectURL(url);
  }
};
