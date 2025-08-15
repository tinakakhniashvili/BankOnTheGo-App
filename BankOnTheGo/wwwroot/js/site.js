(() => {
    const root = document.documentElement;
    const key = "theme";
    const saved = localStorage.getItem(key);
    if (saved) root.setAttribute("data-bs-theme", saved);

    document.getElementById("themeToggle")?.addEventListener("click", () => {
        const cur = root.getAttribute("data-bs-theme") || "light";
        const next = cur === "light" ? "dark" : "light";
        root.setAttribute("data-bs-theme", next);
        localStorage.setItem(key, next);
    });
})();
