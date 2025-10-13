(() => {
  // Simple unique id
  const uid = () => Math.random().toString(36).slice(2) + Date.now().toString(36);

  // Elements
  const els = {
    messages: document.getElementById('chatMessages'),
    quickOptions: document.getElementById('quickOptions'),
    form: document.getElementById('chatForm'),
    input: document.getElementById('userInput'),
    fileInput: document.getElementById('fileInput'),
    attachments: document.getElementById('attachments'),

    // Header buttons
    btnOpenSecret: document.getElementById('btnOpenSecret'),
    btnOpenSelection: document.getElementById('btnOpenSelection'),

    // Secret modal
    secretModal: document.getElementById('secretModal'),
    secretCloseX: document.getElementById('secretCloseX'),
    secretCancelBtn: document.getElementById('secretCancelBtn'),
    secretSaveBtn: document.getElementById('secretSaveBtn'),
    secretLabel: document.getElementById('secretLabel'),
    secretValue: document.getElementById('secretValue'),

    // Selection modal
    selectionModal: document.getElementById('selectionModal'),
    selectionCloseX: document.getElementById('selectionCloseX'),
    selectionCancelBtn: document.getElementById('selectionCancelBtn'),
    selectionSaveBtn: document.getElementById('selectionSaveBtn'),
    categorySelect: document.getElementById('categorySelect'),
  };

  // State
  const state = {
    messages: [],
    quickOptions: ["Track order", "Reset password", "Pricing", "Contact support"],
    pendingAttachments: [], // { name, size, file }
    typingId: null,
    // Selection defaults
    selectedCategory: "General",
    selectedPriority: "Normal",
  };

  // Init default selection
  els.categorySelect.value = state.selectedCategory;
  const prioInputs = () => Array.from(document.querySelectorAll('input[name="priority"]'));

  // Utilities
  const formatSize = (size) => {
    if (size < 1024) return `${size} B`;
    const kb = size / 1024;
    if (kb < 1024) return `${kb.toFixed(kb < 10 ? 1 : 0)} KB`;
    const mb = kb / 1024;
    if (mb < 1024) return `${mb.toFixed(mb < 10 ? 1 : 0)} MB`;
    const gb = mb / 1024;
    return `${gb.toFixed(gb < 10 ? 1 : 0)} GB`;
  };

  const scheduleScrollToBottom = () => {
    // Double rAF to run after layout/paint
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        els.messages.scrollTop = els.messages.scrollHeight;
      });
    });
  };

  // Rendering
  const renderMessages = () => {
    els.messages.innerHTML = '';
    for (const msg of state.messages) {
      const row = document.createElement('div');
      row.className = `message-row ${msg.sender}`;

      const bubble = document.createElement('div');
      bubble.className = `message-bubble ${msg.sender}`;

      const text = document.createElement('div');
      text.className = 'message-text';
      text.textContent = msg.text || '';

      bubble.appendChild(text);

      if (msg.attachments && msg.attachments.length > 0) {
        const filesWrap = document.createElement('div');
        filesWrap.className = 'mt-1 small text-muted';
        for (const a of msg.attachments) {
          const badge = document.createElement('span');
          badge.className = 'badge text-bg-light border me-1 mb-1';
          badge.textContent = `${a.name} (${formatSize(a.size)})`;
          filesWrap.appendChild(badge);
        }
        bubble.appendChild(filesWrap);
      }

      const meta = document.createElement('div');
      meta.className = 'message-meta';
      const small = document.createElement('small');
      small.className = 'text-muted';
      small.textContent = msg.timestamp.toLocaleString();
      meta.appendChild(small);
      bubble.appendChild(meta);

      row.appendChild(bubble);
      els.messages.appendChild(row);
    }
    scheduleScrollToBottom(); // always keep at latest
  };

  const renderQuickOptions = () => {
    els.quickOptions.innerHTML = '';
    for (const opt of state.quickOptions) {
      const btn = document.createElement('button');
      btn.className = 'btn btn-outline-primary btn-sm';
      btn.type = 'button';
      btn.textContent = opt;
      btn.addEventListener('click', () => {
        els.input.value = opt;
        send();
      });
      els.quickOptions.appendChild(btn);
    }
  };

  const renderAttachments = () => {
    els.attachments.innerHTML = '';
    state.pendingAttachments.forEach((att, i) => {
      const chip = document.createElement('span');
      chip.className = 'attachment-chip';
      chip.dataset.index = String(i);

      const name = document.createElement('span');
      name.className = 'text-truncate';
      name.style.maxWidth = '12rem';
      name.textContent = att.name;

      const size = document.createElement('small');
      size.className = 'text-muted';
      size.textContent = ` (${formatSize(att.size)})`;

      const closeBtn = document.createElement('button');
      closeBtn.type = 'button';
      closeBtn.className = 'btn-close btn-close-sm ms-1';
      closeBtn.setAttribute('aria-label', 'Remove');
      closeBtn.addEventListener('click', () => {
        removeAttachment(i);
      });

      chip.appendChild(name);
      chip.appendChild(size);
      chip.appendChild(closeBtn);
      els.attachments.appendChild(chip);
    });
  };

  // State updates
  const addMessage = (msg) => {
    state.messages.push(msg);
    renderMessages();
  };

  const removeTyping = () => {
    if (!state.typingId) return;
    const idx = state.messages.findIndex(m => m.id === state.typingId);
    if (idx >= 0) state.messages.splice(idx, 1);
    state.typingId = null;
    renderMessages();
  };

  // File handling
  const onFilesSelected = (e) => {
    const files = Array.from(e.target.files || []);
    for (const f of files) {
      state.pendingAttachments.push({
        name: f.name,
        size: f.size,
        file: f
      });
    }
    renderAttachments();
    // Allow re-selecting same file names
    els.fileInput.value = '';
  };

  const removeAttachment = (index) => {
    if (index >= 0 && index < state.pendingAttachments.length) {
      state.pendingAttachments.splice(index, 1);
      renderAttachments();
    }
  };

  // Quick options heuristic
  const updateQuickOptions = (lastText) => {
    const t = (lastText || '').toLowerCase();
    if (t.includes('price') || t.includes('pricing')) {
      state.quickOptions = ["Monthly plans", "Annual plans", "Discounts", "Talk to sales"];
    } else if (t.includes('order')) {
      state.quickOptions = ["Track order", "Cancel order", "Return item", "Contact support"];
    } else {
      state.quickOptions = ["Track order", "Reset password", "Pricing", "Contact support"];
    }
    renderQuickOptions();
  };

  // Modal helpers (vanilla, no Bootstrap JS dependency)
  const openModal = (modalEl) => {
    if (!modalEl) return;
    modalEl.classList.add('show', 'd-block');
    modalEl.setAttribute('aria-hidden', 'false');

    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop custom-backdrop fade show';
    backdrop.dataset.for = modalEl.id;
    document.body.appendChild(backdrop);

    // Close when clicking backdrop
    backdrop.addEventListener('click', () => closeModal(modalEl));
  };

  const closeModal = (modalEl) => {
    if (!modalEl) return;
    modalEl.classList.remove('show', 'd-block');
    modalEl.setAttribute('aria-hidden', 'true');

    const backdrop = Array.from(document.querySelectorAll('.modal-backdrop.custom-backdrop'))
      .find(b => b.dataset.for === modalEl.id);
    if (backdrop) backdrop.remove();
  };

  // Secret modal actions
  const openSecret = () => { openModal(els.secretModal); };
  const closeSecret = () => { closeModal(els.secretModal); };
  const saveSecret = () => {
    const label = (els.secretLabel.value || '').trim() || 'Secret';
    // Not persisting any secret value in this demo
    closeSecret();
    addMessage({
      id: uid(),
      sender: 'system',
      text: `${label} captured.`,
      timestamp: new Date(),
      attachments: []
    });
    els.secretLabel.value = '';
    els.secretValue.value = '';
  };

  // Selection modal actions
  const openSelection = () => {
    // sync defaults
    els.categorySelect.value = state.selectedCategory;
    prioInputs().forEach(i => { i.checked = (i.value === state.selectedPriority); });
    openModal(els.selectionModal);
  };
  const closeSelection = () => { closeModal(els.selectionModal); };
  const saveSelection = () => {
    state.selectedCategory = els.categorySelect.value;
    const selectedPrio = prioInputs().find(i => i.checked);
    state.selectedPriority = selectedPrio ? selectedPrio.value : 'Normal';

    closeSelection();
    addMessage({
      id: uid(),
      sender: 'bot',
      text: `Selection saved. Category: ${state.selectedCategory}. Priority: ${state.selectedPriority}.`,
      timestamp: new Date(),
      attachments: []
    });
  };

  // Simulated NLP/API hook
  // Replace this with your real API call as needed
  const callNlpApiAndHandleFlows = async (userMsg) => {
    // Demo logic
    await new Promise(r => setTimeout(r, 650));
    const t = (userMsg.text || '').toLowerCase();

    if (t.includes('secret')) {
      openSecret();
      return {
        id: uid(),
        sender: 'bot',
        text: 'It looks like you want to provide sensitive info. I opened the secret popup.',
        timestamp: new Date(),
        attachments: []
      };
    }

    if (t.includes('select') || t.includes('choose') || t.includes('options')) {
      openSelection();
      return {
        id: uid(),
        sender: 'bot',
        text: 'I opened the selection popup so you can choose specifics.',
        timestamp: new Date(),
        attachments: []
      };
    }

    const filePart = (userMsg.attachments && userMsg.attachments.length > 0)
      ? ` I see ${userMsg.attachments.length} attachment(s).`
      : '';

    return {
      id: uid(),
      sender: 'bot',
      text: `You said: "${userMsg.text}".${filePart}`,
      timestamp: new Date(),
      attachments: []
    };
  };

  // Send flow
  const send = async () => {
    const text = (els.input.value || '').trim();
    const hasTextOrFiles = text.length > 0 || state.pendingAttachments.length > 0;
    if (!hasTextOrFiles) return;

    const userMsg = {
      id: uid(),
      sender: 'user',
      text: text.length ? text : '(No message)',
      timestamp: new Date(),
      attachments: state.pendingAttachments.map(a => ({ name: a.name, size: a.size }))
    };
    addMessage(userMsg);

    // Reset input and pending files
    els.input.value = '';
    state.pendingAttachments = [];
    renderAttachments();

    // Typing indicator
    const typing = {
      id: uid(),
      sender: 'system',
      text: 'Assistant is typing...',
      timestamp: new Date(),
      attachments: []
    };
    state.typingId = typing.id;
    addMessage(typing);

    // "API" call
    const reply = await callNlpApiAndHandleFlows(userMsg);

    // Replace typing with reply
    removeTyping();
    if (reply) addMessage(reply);

    // Update quick options
    updateQuickOptions(text);
  };

  // Event bindings
  els.form.addEventListener('submit', (e) => {
    e.preventDefault();
    send();
  });

  // Enter to send; Shift+Enter for newline
  els.input.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      send();
    }
  });

  els.fileInput.addEventListener('change', onFilesSelected);

  // Header modal buttons
  els.btnOpenSecret.addEventListener('click', openSecret);
  els.btnOpenSelection.addEventListener('click', openSelection);

  // Secret modal controls
  els.secretCloseX.addEventListener('click', closeSecret);
  els.secretCancelBtn.addEventListener('click', closeSecret);
  els.secretSaveBtn.addEventListener('click', saveSecret);

  // Selection modal controls
  els.selectionCloseX.addEventListener('click', closeSelection);
  els.selectionCancelBtn.addEventListener('click', closeSelection);
  els.selectionSaveBtn.addEventListener('click', saveSelection);

  // Handle viewport changes (keyboard open/resizes)
  window.addEventListener('resize', () => {
    scheduleScrollToBottom();
  });
  els.input.addEventListener('focus', () => {
    // Delay to let virtual keyboard adjust viewport
    setTimeout(() => { scheduleScrollToBottom(); }, 100);
  });

  // Startup
  (function init() {
    addMessage({
      id: uid(),
      sender: 'bot',
      text: 'Hi! I’m your assistant. Pick an option below or type your question.',
      timestamp: new Date(),
      attachments: []
    });
    renderQuickOptions();
    renderAttachments();
    scheduleScrollToBottom();
  })();
})();