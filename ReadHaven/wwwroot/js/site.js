// ✅ Update Cart Count Badge
function updateCartCountBadge() {
    $.ajax({
        url: "/BookCart/GetCartItemCount",
        method: "GET",
        success: function (count) {
            const badge = document.getElementById("cart-count-badge");
            if (badge) {
                badge.textContent = count;
                badge.style.display = count > 0 ? "inline-block" : "none";
            }
        },
        error: function (err) {
            console.error("Failed to fetch cart count:", err);
        }
    });
}

// ✅ Format Date to Friendly Format
function formatDate(dateString) {
    const date = new Date(dateString);
    const day = date.getDate();
    const dayName = date.toLocaleString("default", { weekday: "long" });
    const month = date.toLocaleString("default", { month: "long" });
    const year = date.getFullYear();

    const getOrdinalSuffix = (n) => {
        if (n > 3 && n < 21) return "th";
        switch (n % 10) {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    };

    return `${dayName}, ${day}${getOrdinalSuffix(day)} ${month} ${year}`;
}

// ✅ Global Toast Notification
function showToastMessage(message, type = "success", delay = 2000) {
    const toastEl = document.getElementById("globalToast");
    const toastBody = document.getElementById("globalToastBody");

    if (!toastEl || !toastBody) {
        console.warn("Toast elements not found in DOM.");
        return;
    }

    toastBody.textContent = message;

    toastEl.className = "toast align-items-center text-white border-0 show";

    toastEl.classList.add({
        success: "bg-success",
        error: "bg-danger",
        info: "bg-info",
        warning: "bg-warning"
    }[type] || "bg-success");

    const toast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: delay });
    toast.show();
}

// ✅ Save scroll position before navigating away
window.addEventListener('beforeunload', function () {
    sessionStorage.setItem('scrollPos_' + location.pathname, window.scrollY);
});

// ✅ Restore scroll position safely using load event listener
window.addEventListener('load', function () {
    setTimeout(() => {
        const scrollKey = 'scrollPos_' + location.pathname;
        const scrollPos = sessionStorage.getItem(scrollKey);
        if (scrollPos !== null) {
            window.scrollTo(0, parseInt(scrollPos));
        }
    }, 100); // Give some delay for other content to load
});
