window.layoutHelpers = {
    setSidebarWidth: function (px) {
        const el = document.getElementById('sidebarContainer');
        if (el) el.style.width = px + 'px';
    }
};
