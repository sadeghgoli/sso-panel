document.addEventListener("click", function (e) {
    var copyBtn = e.target.closest("[data-copy]");
    if (copyBtn) {
        var target = document.querySelector(copyBtn.getAttribute("data-copy"));
        if (!target) return;
        navigator.clipboard.writeText(target.textContent.trim()).then(function () {
            var old = copyBtn.textContent;
            copyBtn.textContent = "کپی شد";
            setTimeout(function () { copyBtn.textContent = old; }, 1500);
        });
        return;
    }

    var confirmBtn = e.target.closest("[data-confirm]");
    if (confirmBtn && !window.confirm(confirmBtn.getAttribute("data-confirm"))) {
        e.preventDefault();
    }
});
