'use strict';

const NATIVE_HOST = 'com.nisar.youtube_mp3_downloader';

let currentUrl = null;

function isSupportedYouTubeUrl(raw) {
  let url;
  try {
    url = new URL(raw);
  } catch (err) {
    return false;
  }

  if (url.protocol !== 'http:' && url.protocol !== 'https:') {
    return false;
  }

  const host = url.hostname.toLowerCase();
  const isShortLink = host === 'youtu.be';
  const isYoutube = host === 'youtube.com' || host.endsWith('.youtube.com');
  if (!isShortLink && !isYoutube) {
    return false;
  }

  if (isShortLink) {
    return url.pathname.split('/').filter(Boolean).length > 0;
  }

  return url.pathname === '/watch' && url.searchParams.has('v') && url.searchParams.get('v').length > 0;
}

function showStatus(text, isError) {
  const el = document.getElementById('status');
  el.textContent = text;
  el.hidden = false;
  el.className = isError ? 'status error' : 'status';
}

function send(action) {
  if (!currentUrl) {
    return;
  }

  showStatus('Sending\u2026', false);

  try {
    chrome.runtime.sendNativeMessage(
      NATIVE_HOST,
      { action: action, url: currentUrl },
      (response) => {
        if (chrome.runtime.lastError) {
          console.error(
            '[Nisar Companion] Native messaging error:',
            chrome.runtime.lastError.message
          );
          showStatus('Could not connect to the desktop downloader.', true);
          return;
        }

        if (response && response.success) {
          showStatus(
            response.message || (action === 'addAndStart' ? 'Added and started.' : 'Added to queue.'),
            false
          );
        } else {
          showStatus(
            (response && response.message) || 'The downloader did not accept the link.',
            true
          );
        }
      }
    );
  } catch (err) {
    console.error('[Nisar Companion] sendNativeMessage threw:', err);
    showStatus('Could not connect to the desktop downloader.', true);
  }
}

async function init() {
  const pageEl = document.getElementById('currentPage');
  let url = '';
  let title = '';

  try {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    if (tab && tab.url) {
      url = tab.url;
    }
    if (tab && tab.title) {
      title = tab.title;
    }
  } catch (err) {
    url = '';
  }

  pageEl.textContent = url || 'No page detected';
  pageEl.title = url;

  if (!isSupportedYouTubeUrl(url)) {
    document.getElementById('unsupported').hidden = false;
    return;
  }

  currentUrl = url;
  const cleanTitle = title.replace(/\s*-\s*YouTube\s*$/i, '').trim();
  pageEl.textContent = cleanTitle || url;
  document.getElementById('actions').hidden = false;
}

document.getElementById('addQueue').addEventListener('click', () => send('add'));
document.getElementById('addStart').addEventListener('click', () => send('addAndStart'));

init();