document.addEventListener('DOMContentLoaded', function () {
    const mergeButton = document.getElementById('mergeButton');
    const singleButton = document.getElementById('singleButton');
    const fileInput = document.getElementById('fileInput');
    const sizeSelect = document.getElementById('sizeSelect');
    const downloadLink = document.getElementById('downloadLink');

    mergeButton.addEventListener('click', function () {
        const file = fileInput.files[0];
        const size = sizeSelect.value;
        // 调用合并生成API
        fetch('/merge-api', {
            method: 'POST',
            body: JSON.stringify({ file: file, size: size }),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => response.blob())
            .then(blob => {
                const url = URL.createObjectURL(blob);
                downloadLink.href = url;
                downloadLink.style.display = 'block';
            });
    });

    singleButton.addEventListener('click', function () {
        const file = fileInput.files[0];
        const size = sizeSelect.value;
        // 调用单独生成API
        fetch('/single-api', {
            method: 'POST',
            body: JSON.stringify({ file: file, size: size }),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => response.blob())
            .then(blob => {
                const url = URL.createObjectURL(blob);
                downloadLink.href = url;
                downloadLink.style.display = 'block';
            });
    });
});