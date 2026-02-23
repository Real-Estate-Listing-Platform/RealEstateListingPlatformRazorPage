(() => {
    const fileInput = document.getElementById("mediaFiles");
    const preview = document.getElementById("mediaPreview");

    if (fileInput && preview) {
        fileInput.addEventListener("change", (event) => {
            preview.innerHTML = "";
            const files = Array.from(event.target.files || []);

            files.forEach((file) => {
                const col = document.createElement("div");
                col.className = "col-md-3";

                const tile = document.createElement("div");
                tile.className = "media-tile";

                if (file.type.startsWith("image/")) {
                    const img = document.createElement("img");
                    img.alt = file.name;
                    const reader = new FileReader();
                    reader.onload = (e) => {
                        img.src = e.target?.result;
                    };
                    reader.readAsDataURL(file);
                    tile.appendChild(img);
                } else if (file.type.startsWith("video/")) {
                    const video = document.createElement("video");
                    video.controls = true;
                    video.src = URL.createObjectURL(file);
                    tile.appendChild(video);
                } else {
                    const label = document.createElement("div");
                    label.textContent = file.name;
                    tile.appendChild(label);
                }

                col.appendChild(tile);
                preview.appendChild(col);
            });
        });
    }

    const deleteButtons = document.querySelectorAll(".js-delete-media");
    if (deleteButtons.length > 0) {
        deleteButtons.forEach((button) => {
            button.addEventListener("click", async () => {
                if (!confirm("Delete this media?")) return;

                const mediaId = button.getAttribute("data-media-id");
                if (!mediaId) return;

                const response = await fetch(`/Listings/DeleteMedia?mediaId=${mediaId}`, {
                    method: "POST"
                });

                if (response.ok) {
                    const container = button.closest("[data-media-id]");
                    if (container) container.remove();
                } else {
                    alert("Failed to delete media.");
                }
            });
        });
    }
})();
