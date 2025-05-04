let currentPage = 1;
let totalPages = 1;
let searchData = {
    Title: null,
    Genre: null,
    PriceSort: null,
    StartIndex: 1,
    EndIndex: 9
};

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
    searchData.Title = document.getElementById("searchInput").value.trim() || null;
    searchData.Genre = document.getElementById("genreInput").value.trim() || null;
    searchData.PriceSort = parseInt(document.getElementById("priceSortSelect").value) || null;

    showBookList();
}

// Render Book List
function showBookList() {
    $.ajax({
        url: "/GetBookList",
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

                const isLoved = book.isLoved === true;
                const loveButton = isAuthenticated ? `
                    <button 
                        class="btn btn-outline-danger btn-sm wishlist-btn ${isLoved ? 'isLoved' : ''}" 
                        onclick="toggleWishlist('${book.id}', this)" 
                        title="${isLoved ? 'Remove from Wishlist' : 'Add to Wishlist'}">
                        <i class="bi ${isLoved ? 'bi-heart-fill' : 'bi-heart'}"></i>
                    </button>` : '';

                const col = document.createElement("div");
                col.className = "col-md-6 col-lg-4";

                col.innerHTML = `
                    <div class="card shadow-sm h-100 book-card">
                        <div class="card-body d-flex">
                            <div class="image-container">
                                <a href="/Details/${book.id}">
                                    <img src="${imagePath}" alt="${title}" class="rounded shadow-sm" style="width: 110px; height: 160px; object-fit: cover;">
                                </a>
                            </div>
                            <div class="book-info ms-3 d-flex flex-column justify-content-between">
                                <div>
                                    <h5>${title}</h5>
                                    <p class="mb-1"><strong></strong> ${genre}</p>
                                    <p class="mb-0"><strong></strong> ${formattedPrice}</p>
                                    <p class="mb-1 text-warning fs-6" title="${book.rating}/5">${stars}</p>
                                </div>
                                <div class="d-flex justify-content-start gap-2 book-actions">
                                    <a href="/Details/${book.id}" class="btn btn-outline-info btn-sm" title="View Details">
                                        <i class="bi bi-eye"></i>
                                    </a>
                                    ${userRole === "Admin" ? `
                                        <button class="btn btn-outline-danger btn-sm" onclick="deleteBook('${book.id}')" title="Delete Book">
                                            <i class="bi bi-trash"></i>
                                        </button>` : ""}
                                    <button class="btn btn-outline-primary btn-sm" onclick="addToCart('${book.id}')" title="Add to Cart">
                                        <i class="bi bi-cart-plus"></i>
                                    </button>
                                    ${loveButton}
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

function toggleWishlist(bookId, btn) {
    $.ajax({
        url: `/Wishlist`,
        type: "POST",
        data: { bookId: bookId },
        success: function () {
            const icon = btn.querySelector("i");
            btn.classList.toggle("isLoved");

            const isWishlisted = btn.classList.contains("isLoved");
            icon.className = `bi ${isWishlisted ? "bi-heart-fill" : "bi-heart"}`;
            btn.setAttribute("title", isWishlisted ? "Remove from Wishlist" : "Add to Wishlist");
        },
        error: function (xhr) {
            console.error("Error updating wishlist:", xhr.responseText);
            showToastMessage("Could not update wishlist", "error");
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

// Pagination
function renderPagination() {
    const container = document.getElementById("paginationContainer");
    container.innerHTML = "";

    const createPageButton = (pageNum, innerHtml, isActive = false, disabled = false) => {
        const btn = document.createElement("button");
        btn.innerHTML = innerHtml;
        btn.disabled = isActive || disabled;

        btn.classList.add("btn", "btn-sm", "mx-1", "px-2", "py-1", "transition");

        if (isActive) {
            // Current page style (flat gray)
            btn.classList.add("btn-secondary", "fw-bold", "text-white");
            btn.style.cursor = "default";
        } else {
            // Other pages
            btn.classList.add("btn-outline-secondary");
            btn.onclick = () => {
                if (!disabled && pageNum !== currentPage) {
                    currentPage = Math.min(Math.max(pageNum, 1), totalPages);
                    sessionStorage.setItem("bookCurrentPage", currentPage);

                    // Update the URL without reloading the page
                    history.pushState(null, "", `/page/${currentPage}`);

                    searchData.StartIndex = (currentPage - 1) * 9 + 1;
                    searchData.EndIndex = searchData.StartIndex + 8;
                    showBookList();
                    renderPagination();
                }
            };
        }

        container.appendChild(btn);
    };

    createPageButton(currentPage - 1, '<i class="bi bi-chevron-left"></i>', false, currentPage === 1);

    let startPage = Math.max(1, currentPage - 1);
    let endPage = Math.min(totalPages, startPage + 2);
    if (endPage - startPage < 2) startPage = Math.max(1, endPage - 2);

    for (let i = startPage; i <= endPage; i++) {
        createPageButton(i, i, i === currentPage);
    }

    createPageButton(currentPage + 1, '<i class="bi bi-chevron-right"></i>', false, currentPage === totalPages);
}



function loadTotalPages() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: "/GetBookCount",
            type: "GET",
            success: function (count) {
                totalPages = Math.ceil(count / 9); 
                resolve();
            },
            error: function (xhr, status, error) {
                console.error("Failed to get total pages:", error);
                reject(error);
            }
        });
    });
}

// Initialize on page load
window.onload = async function () {
    updateCartCountBadge();

    const urlParams = new URLSearchParams(window.location.search);
    const pageFromUrl = window.location.pathname.split("/").pop();

    currentPage = 1;

    if (pageFromUrl && !isNaN(pageFromUrl)) {
        currentPage = parseInt(pageFromUrl);
    }

    try {
        await loadTotalPages();

        if (currentPage < 1 || currentPage > totalPages) {
            currentPage = 1;
            history.pushState(null, "", "/page/1");
        }

        renderPagination();
        showBookList();
    } catch (error) {
        showToastMessage("Failed to initialize page.", "error");
    }

    if (userRole === "Admin") {
        const form = document.querySelector("#create-book");
        if (form) {
            form.addEventListener("submit", handleBookFormSubmit);
        }
    }
    localStorage.setItem("returnUrl", "/Book/Index");
};

