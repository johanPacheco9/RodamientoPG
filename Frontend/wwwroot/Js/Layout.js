window.layoutHelpers = {
    setSidebarWidth: function (px) {
        const el = document.getElementById('sidebarContainer');
        if (el) el.style.width = px + 'px';
    }
};

window.downloadFileFromStream = async (fileName, streamRef) => {
    try {
        const arrayBuffer = await streamRef.arrayBuffer();
        const blob = new Blob([arrayBuffer], { type: 'application/pdf' });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        
        setTimeout(() => {
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }, 1000);
    } catch (e) {
        console.error("Error en downloadFileFromStream:", e);
    }
};

window.downloadFileFromUrl = async (fileUrl, fileName) => {
    try {
        const response = await fetch(fileUrl);
        if (!response.ok) {
            throw new Error(`Error HTTP: ${response.status} ${response.statusText}`);
        }
        const blob = await response.blob();
        const pdfBlob = new Blob([blob], { type: 'application/pdf' });
        const url = URL.createObjectURL(pdfBlob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        
        setTimeout(() => {
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }, 1500);
    } catch (e) {
        console.error("Error en downloadFileFromUrl:", e);
        window.open(fileUrl, '_blank');
    }
};
