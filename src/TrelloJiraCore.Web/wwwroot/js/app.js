document.addEventListener('DOMContentLoaded', () => {
    const themeToggle = document.getElementById('theme-toggle');
    const themeIcon = document.getElementById('theme-icon');
    const boardsView = document.getElementById('boards-view');
    const kanbanView = document.getElementById('kanban-view');
    const boardsContainer = document.getElementById('boards-container');
    const kanbanBoard = document.getElementById('kanban-board');
    const navBoards = document.getElementById('nav-boards');
    const globalSearch = document.getElementById('global-search');

    const cardModal = document.getElementById('card-modal');
    const closeModal = document.getElementById('close-modal');
    const cardForm = document.getElementById('card-form');

    let currentBoardId = null;
    let draggedCardId = null;

    // --- Theme Management ---
    const updateThemeUI = (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
        themeIcon.textContent = theme === 'light' ? '🌙' : '☀️';
        if (theme === 'dark') document.documentElement.classList.add('dark');
        else document.documentElement.classList.remove('dark');
    };

    const savedTheme = localStorage.getItem('theme') || 'light';
    updateThemeUI(savedTheme);

    themeToggle.addEventListener('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        updateThemeUI(newTheme);
        localStorage.setItem('theme', newTheme);
    });

    // --- SignalR Connection ---
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/boardHub")
        .withAutomaticReconnect()
        .build();

    connection.on("OnCardMoved", (cardId, fromListId, toListId) => {
        showNotification("یک کارت توسط کاربر دیگری جابه‌جا شد", "info");
        if (currentBoardId) loadBoard(currentBoardId);
    });

    connection.on("OnCardUpdated", (cardId) => {
        showNotification("یک کارت توسط کاربر دیگری بروزرسانی شد", "info");
        if (currentBoardId) loadBoard(currentBoardId);
    });

    connection.start().catch(err => console.error("SignalR Connection Error: ", err));

    // --- Navigation ---
    const showView = (viewName) => {
        boardsView.classList.add('hidden');
        kanbanView.classList.add('hidden');

        if (viewName === 'boards') {
            boardsView.classList.remove('hidden');
            loadBoards();
            currentBoardId = null;
        } else if (viewName === 'kanban') {
            kanbanView.classList.remove('hidden');
        }
    };

    navBoards.addEventListener('click', (e) => {
        e.preventDefault();
        showView('boards');
    });

    // --- Boards Logic ---
    async function loadBoards(search = '') {
        try {
            const url = search ? `/api/boards?search=${encodeURIComponent(search)}` : '/api/boards';
            const response = await fetch(url);
            const boards = await response.json();
            renderBoards(boards);
        } catch (error) {
            console.error("Error loading boards:", error);
        }
    }

    function renderBoards(boards) {
        boardsContainer.innerHTML = '';
        boards.forEach(board => {
            const boardEl = document.createElement('div');
            boardEl.className = 'bg-white dark:bg-gray-800 p-6 rounded-xl shadow-md border dark:border-gray-700 cursor-pointer hover:border-blue-500 transition-all transform hover:-translate-y-1';
            boardEl.innerHTML = `
                <h3 class="text-xl font-bold mb-2">${board.title}</h3>
                <p class="text-gray-500 dark:text-gray-400 text-sm mb-4 line-clamp-2">${board.description || 'بدون توضیح'}</p>
                <div class="flex justify-between items-center text-xs text-gray-400">
                    <span>ایجاد شده در: ${new Date(board.createdAt).toLocaleDateString('fa-IR')}</span>
                    <span class="bg-blue-100 text-blue-600 px-2 py-1 rounded">مشاهده</span>
                </div>
            `;
            boardEl.onclick = () => openBoard(board.id);
            boardsContainer.appendChild(boardEl);
        });
    }

    async function openBoard(id) {
        if (currentBoardId) connection.invoke("LeaveBoard", currentBoardId);
        currentBoardId = id;
        connection.invoke("JoinBoard", id);
        showView('kanban');
        loadBoard(id);
        loadActivityLogs(id);
    }

    // --- Kanban Logic ---
    async function loadBoard(id) {
        try {
            const response = await fetch(`/api/boards/${id}`);
            const board = await response.json();
            document.getElementById('board-title').textContent = board.title;
            document.getElementById('board-description').textContent = board.description;
            renderKanban(board);
        } catch (error) {
            console.error("Error loading board:", error);
        }
    }

    function renderKanban(board) {
        kanbanBoard.innerHTML = '';
        board.lists.forEach(list => {
            const column = document.createElement('div');
            column.className = 'kanban-column';
            column.dataset.listId = list.id;
            column.innerHTML = `<div class="flex justify-between items-center mb-4 px-2">
                <h3 class="font-black text-gray-700 dark:text-gray-200">${list.title}</h3>
                <span class="bg-gray-300 dark:bg-gray-700 text-xs px-2 py-1 rounded-full">${list.cards.length}</span>
            </div>`;

            const cardContainer = document.createElement('div');
            cardContainer.className = 'flex-grow min-h-[100px]';

            list.cards.forEach(card => {
                const cardEl = document.createElement('div');
                cardEl.className = `kanban-card priority-${getPriorityName(card.priority).toLowerCase()}`;
                cardEl.draggable = true;
                cardEl.dataset.cardId = card.id;

                const dueDate = card.dueDate ? new Date(card.dueDate).toLocaleDateString('fa-IR') : 'بدون تاریخ';

                cardEl.innerHTML = `
                    <div class="text-sm font-bold mb-2 dark:text-white">${card.title}</div>
                    <div class="text-xs text-gray-500 dark:text-gray-400 mb-3 line-clamp-3">${card.description}</div>
                    <div class="flex justify-between items-center mt-2 pt-2 border-t dark:border-gray-700">
                        <span class="text-[10px] bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded">${card.assignee || 'بدون مسئول'}</span>
                        <span class="text-[10px] text-gray-400">📅 ${dueDate}</span>
                    </div>
                `;

                cardEl.addEventListener('dragstart', handleDragStart);
                cardEl.onclick = (e) => {
                    e.stopPropagation();
                    openCardModal(card.id);
                };
                cardContainer.appendChild(cardEl);
            });

            column.appendChild(cardContainer);
            column.addEventListener('dragover', handleDragOver);
            column.addEventListener('drop', handleDrop);

            kanbanBoard.appendChild(column);
        });
    }

    function getPriorityName(p) {
        const names = ['Low', 'Medium', 'High', 'Urgent'];
        return names[p] || 'Low';
    }

    // --- Drag & Drop ---
    function handleDragStart(e) {
        draggedCardId = e.target.dataset.cardId;
        e.dataTransfer.setData('text/plain', draggedCardId);
        e.target.style.opacity = '0.5';
    }

    function handleDragOver(e) {
        e.preventDefault();
    }

    async function handleDrop(e) {
        e.preventDefault();
        const listId = e.currentTarget.dataset.listId;
        if (draggedCardId && listId) {
            await moveCard(draggedCardId, listId);
        }
        draggedCardId = null;
    }

    async function moveCard(cardId, toListId) {
        try {
            const response = await fetch(`/api/cards/${cardId}/move`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ toListId: parseInt(toListId), newOrder: 0 })
            });
            if (response.ok) {
                loadBoard(currentBoardId);
                loadActivityLogs(currentBoardId);
            }
        } catch (error) {
            console.error("Error moving card:", error);
        }
    }

    // --- Modal Logic ---
    async function openCardModal(cardId) {
        try {
            const response = await fetch(`/api/cards/${cardId}`);
            const card = await response.json();

            document.getElementById('modal-card-id').value = card.id;
            document.getElementById('modal-card-title').value = card.title;
            document.getElementById('modal-card-desc').value = card.description;
            document.getElementById('modal-card-priority').value = card.priority;
            document.getElementById('modal-card-assignee').value = card.assignee;
            document.getElementById('modal-card-due').value = card.dueDate ? card.dueDate.split('T')[0] : '';

            cardModal.style.display = 'block';
        } catch (error) {
            console.error("Error opening card modal:", error);
        }
    }

    closeModal.onclick = () => cardModal.style.display = 'none';
    document.getElementById('btn-cancel-card').onclick = () => cardModal.style.display = 'none';

    cardForm.onsubmit = async (e) => {
        e.preventDefault();
        const cardId = document.getElementById('modal-card-id').value;
        const data = {
            title: document.getElementById('modal-card-title').value,
            description: document.getElementById('modal-card-desc').value,
            priority: parseInt(document.getElementById('modal-card-priority').value),
            assignee: document.getElementById('modal-card-assignee').value,
            dueDate: document.getElementById('modal-card-due').value || null
        };

        try {
            const response = await fetch(`/api/cards/${cardId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                cardModal.style.display = 'none';
                showNotification("تغییرات با موفقیت ذخیره شد", "success");
                loadBoard(currentBoardId);
                loadActivityLogs(currentBoardId);
            }
        } catch (error) {
            console.error("Error saving card:", error);
        }
    };

    // --- Activity Logs ---
    async function loadActivityLogs(boardId) {
        try {
            const response = await fetch(`/api/activitylogs/board/${boardId}`);
            const logs = await response.json();
            const container = document.getElementById('activity-log-container');
            container.innerHTML = '';
            logs.forEach(log => {
                const logEl = document.createElement('div');
                logEl.className = 'activity-item';
                logEl.innerHTML = `
                    <div class="font-bold">${log.user}</div>
                    <div>${log.action}</div>
                    <div class="time">${new Date(log.timestamp).toLocaleString('fa-IR')}</div>
                `;
                container.appendChild(logEl);
            });
        } catch (error) {
            console.error("Error loading logs:", error);
        }
    }

    // --- Utilities ---
    function showNotification(message, type = 'info') {
        const area = document.getElementById('notification-area');
        const note = document.createElement('div');
        const colors = {
            success: 'bg-green-500',
            info: 'bg-blue-500',
            error: 'bg-red-500'
        };

        note.className = `${colors[type]} text-white px-6 py-3 rounded-lg shadow-xl animate-bounce-in flex items-center gap-3`;
        note.innerHTML = `<span>${type === 'success' ? '✅' : 'ℹ️'}</span> ${message}`;

        area.appendChild(note);
        setTimeout(() => {
            note.style.opacity = '0';
            setTimeout(() => note.remove(), 500);
        }, 4000);
    }

    globalSearch.addEventListener('input', (e) => {
        const q = e.target.value;
        if (boardsView.classList.contains('hidden') === false) {
            loadBoards(q);
        }
    });

    // Start with boards view
    showView('boards');
});
