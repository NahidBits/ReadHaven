// Show success or error toast
function showToastMessage(message, type = "success") {
    const toastClass = type === "error" ? "bg-danger text-white" : "bg-success text-white";
    const toast = document.createElement("div");
    toast.className = `toast align-items-center ${toastClass} border-0 position-fixed bottom-0 end-0 m-3`;
    toast.role = "alert";
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    document.body.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    setTimeout(() => toast.remove(), 3000);
}

// Submit New Book Form
function handleBookFormSubmit(event) {
    event.preventDefault();
    const form = document.querySelector("#create-book");
    const formData = new FormData(form);

    $.ajax({
        url: "/Create",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                showToastMessage("Book added successfully!");
                form.reset();
                showBookList();
            } else {
                showToastMessage("Error adding book.", "error");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error submitting book:", textStatus, errorThrown);
            showToastMessage("Something went wrong!", "error");
        }
    });
}

// Apply filters for search
function applyFilters() {
    const title = document.getElementById("searchInput").value.trim();
    const genre = document.getElementById("genreInput").value.trim();
    const priceSort = parseInt(document.getElementById("priceSortSelect").value) || null;

    const searchData = {
        Title: title || null,
        Genre: genre || null,
        PriceSort: priceSort || null
    };

    showBookList(searchData);
}

// Render Book List
// Render Book List
function showBookList(searchData = {}) {
    $.ajax({
        url: "/GetBookListWithRating",
        type: "GET",
        data: searchData,
        dataType: "json",
        success: function (books) {
            const container = document.querySelector("#bookCardContainer");
            container.innerHTML = "";

            books.forEach(book => {
                const formattedPrice = new Intl.NumberFormat('en-US', {
                    style: 'currency',
                    currency: 'USD'
                }).format(book.price || 0);

                const imagePath = book.imagePath || '/uploads/book/Default_image.webp';
                const title = book.title || 'N/A';
                const genre = book.genre || 'N/A';
                const stars = "★".repeat(book.rating) + "☆".repeat(5 - book.rating);

                const col = document.createElement("div");
                col.className = "col-md-6 col-lg-4";

                col.innerHTML = `
                    <div class="card shadow-sm h-100 book-card">
                        <div class="card-body d-flex">
                            <div class="image-container">
                                <img src="${imagePath}" alt="${title}" class="rounded shadow-sm" style="width: 110px; height: 160px; object-fit: cover;">
                            </div>
                            <div class="book-info ms-3 d-flex flex-column justify-content-between">
                                <div>
                                    <h5>${title}</h5>
                                    <p class="mb-1"><strong>Genre:</strong> ${genre}</p>
                                    <p class="mb-0"><strong>Price:</strong> ${formattedPrice}</p>
                                    <p class="mb-1 text-warning fs-6" title="${book.rating}/5">${stars}</p>
                                </div>
                                <div class="d-flex justify-content-start gap-2 book-actions">
                                    <a href="/Details/${book.id}" class="btn btn-outline-info btn-sm" title="View Details">
                                        <i class="bi bi-eye"></i>
                                    </a>
                                    ${userRole === "Admin" ? `
                                        <button class="btn btn-outline-danger btn-sm" onclick="deleteBook('${book.id}')" title="Delete Book">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    ` : ""}
                                    <button class="btn btn-outline-primary btn-sm" onclick="addToCart('${book.id}')" title="Add to Cart">
                                        <i class="bi bi-cart-plus"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;

                container.appendChild(col);
            });
        },
        error: function (xhr, textStatus, error) {
            console.error("Error loading books:", textStatus, error);
            showToastMessage("Failed to load book list", "error");
        }
    });
}

// Delete a book
function deleteBook(bookId) {
    if (confirm('Are you sure you want to delete this book?')) {
        $.ajax({
            url: '/Delete/' + bookId,
            type: 'POST',
            success: function () {
                showToastMessage("Book deleted successfully!");
                showBookList();
            },
            error: function (xhr, status, error) {
                showToastMessage("Error deleting book: " + error, "error");
            }
        });
    }
}

// Add book to cart
function addToCart(bookId) {
    $.ajax({
        url: "/BookCart/AddToCart",
        type: "POST",
        data: { bookId },
        success: function () {
            showToastMessage("Book added to cart!");
            updateCartCountBadge();
        },
        error: function (xhr, status, error) {
            console.error("Failed to add to cart:", error);
            showToastMessage("Failed to add book to cart.", "error");
        }
    });
}

// Update the cart badge count
function updateCartCountBadge() {
    $.ajax({
        url: "/BookCart/GetCartCount",
        type: "GET",
        success: function (count) {
            const cartCountBadge = document.getElementById("cartCountBadge");
            if (cartCountBadge) {
                cartCountBadge.textContent = count;
            }
        },
        error: function (xhr, status, error) {
            console.error("Failed to update cart count:", error);
        }
    });
}

// Initialize on page load
window.onload = function () {
    updateCartCountBadge();
    showBookList();

    if (userRole === "Admin") {
        const form = document.querySelector("#create-book");
        if (form) {
            form.addEventListener("submit", handleBookFormSubmit);
        }
    }
};