document.addEventListener('DOMContentLoaded', () => {
    const themeToggle = document.getElementById('theme-toggle');
    const kanbanBoard = document.getElementById('kanban-board');
    let currentBoardId = 1; // برای دمو از بورد شماره ۱ استفاده می‌کنیم

    // مدیریت تم
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);

    themeToggle.addEventListener('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
    });

    // اتصال به SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/boardHub")
        .build();

    connection.on("OnCardMoved", (cardId, fromListId, toListId) => {
        console.log(`Card ${cardId} moved from ${fromListId} to ${toListId}`);
        showNotification(`یک کارت جابه‌جا شد`);
        loadBoard(currentBoardId); // رفرش بورد برای نمایش تغییرات دیگران
    });

    connection.start().then(() => {
        console.log("Connected to SignalR");
        connection.invoke("JoinBoard", currentBoardId);
    }).catch(err => console.error(err));

    // بارگذاری داده‌های بورد
    async function loadBoard(id) {
        try {
            const response = await fetch(`/api/boards/${id}`);
            const board = await response.json();

            document.getElementById('board-title').textContent = board.title;
            document.getElementById('board-description').textContent = board.description;

            renderBoard(board);
        } catch (error) {
            console.error("Error loading board:", error);
        }
    }

    function renderBoard(board) {
        kanbanBoard.innerHTML = '';
        board.lists.forEach(list => {
            const column = document.createElement('div');
            column.className = 'kanban-column';
            column.dataset.listId = list.id;
            column.innerHTML = `<h3 style="padding: 0.5rem; font-weight: bold;">${list.title}</h3>`;

            const cardContainer = document.createElement('div');
            cardContainer.className = 'card-container';
            cardContainer.style.flexGrow = '1';

            list.cards.forEach(card => {
                const cardEl = document.createElement('div');
                cardEl.className = `kanban-card priority-${getPriorityClass(card.priority)}`;
                cardEl.draggable = true;
                cardEl.dataset.cardId = card.id;
                cardEl.innerHTML = `
                    <div style="font-weight: bold; margin-bottom: 0.5rem;">${card.title}</div>
                    <div style="font-size: 0.875rem; opacity: 0.8;">${card.description}</div>
                    <div style="margin-top: 0.5rem; font-size: 0.75rem; color: #3b82f6;">${card.assignee}</div>
                `;

                cardEl.addEventListener('dragstart', handleDragStart);
                cardContainer.appendChild(cardEl);
            });

            column.appendChild(cardContainer);
            column.addEventListener('dragover', handleDragOver);
            column.addEventListener('drop', handleDrop);

            kanbanBoard.appendChild(column);
        });
    }

    function getPriorityClass(p) {
        if (p === 3) return 'high';
        if (p === 2) return 'medium';
        return 'low';
    }

    let draggedCardId = null;

    function handleDragStart(e) {
        draggedCardId = e.target.dataset.cardId;
        e.dataTransfer.setData('text/plain', draggedCardId);
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
    }

    async function moveCard(cardId, toListId) {
        try {
            await fetch(`/api/cards/${cardId}/move`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ toListId: parseInt(toListId), newOrder: 0 })
            });
            loadBoard(currentBoardId);
        } catch (error) {
            console.error("Error moving card:", error);
        }
    }

    function showNotification(message) {
        const area = document.getElementById('notification-area');
        const note = document.createElement('div');
        note.style.backgroundColor = '#10b981';
        note.style.color = 'white';
        note.style.padding = '0.75rem 1.5rem';
        note.style.borderRadius = '0.5rem';
        note.style.marginBottom = '0.5rem';
        note.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
        note.textContent = message;

        area.appendChild(note);
        setTimeout(() => note.remove(), 3000);
    }

    loadBoard(currentBoardId);
});
