// Form Submission for Creating a New Book
function handleBookFormSubmit(event) {
    event.preventDefault();

    const form = document.querySelector("#create-book");
    const formData = new FormData(document.querySelector("#create-book"));

    $.ajax({
        url: "/Create",
        type: "POST",
        data: formData,
        processData: false,  // Important: Don't process FormData
        contentType: false,  // Important: Let browser set Content-Type
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

// Search book by Title, Genre, price, etc.
function applyFilters() {
    const title = document.getElementById("searchInput").value.trim();
    const genre = document.getElementById("genreInput").value.trim();
    const priceSort = parseInt(document.getElementById("priceSortSelect").value) || null;

    // Search parameters setup
    const searchData = {
        Title: title.length ? title : null,  // Only add to searchData if not empty
        Genre: genre.length ? genre : null,
        PriceSort: priceSort || null  // Null if invalid or missing
    };

    showBookList(searchData);
}

// Function to load and show the book list in the table
function showBookList(searchData) {
    $.ajax({
        url: "/GetBookList",
        dataType: "json",
        type: "GET",
        data: searchData,
        success: function (data) {
            const tableBody = document.querySelector("#bookTableBody");
            tableBody.innerHTML = "";  // Clear existing entries

            data.forEach(book => {
                const formattedPrice = new Intl.NumberFormat('en-US', {
                    style: 'currency',
                    currency: 'USD'
                }).format(book.price);

                const row = document.createElement("tr");

                const bookTitle = book.title ? book.title : 'N/A';
                const bookGenre = book.genre ? book.genre : 'N/A';
                const imagePath = book.imagePath ? book.imagePath : '/uploads/book/Default_image.webp';

                row.innerHTML = `
                    <td>${bookTitle}</td>
                    <td>${bookGenre}</td>
                    <td>${formattedPrice}</td>
                    <td>
                        <img src="${imagePath}" alt="${bookTitle}" style="max-width: 100px; height: auto;"/>
                    </td>
                    <td>
                        <a href="/Details/${book.id}" class="btn btn-info btn-sm">Details</a>
                        ${userRole === "Admin" ? `
                            <button type="button" class="btn btn-danger btn-sm" onclick="deleteBook('${book.id}')">Delete</button>
                        ` : ''}
                        <button type="submit" class="btn btn-primary btn-sm" onclick="addToCart('${book.id}')">Add To Cart</button>
                    </td>
                `;

                tableBody.appendChild(row);
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading books:", textStatus, errorThrown);
        }
    });
}

// Function to delete a book
function deleteBook(bookId) {
    if (confirm('Are you sure you want to delete this book?')) {
        $.ajax({
            url: '/Delete/' + bookId,
            type: 'POST',
            success: function (response) {
                showToastMessage("Book deleted successfully!");
                showBookList();  // Refresh after deletion
            },
            error: function (xhr, status, error) {
                showToastMessage("Error deleting book: " + error, "error");
            }
        });
    }
}

// Function to add a book to the cart
function addToCart(bookId) {
    $.ajax({
        url: "/BookCart/AddToCart",
        type: "POST",
        data: { bookId: bookId },
        success: function () {
            showToastMessage("Book added to cart!");
            updateCartCountBadge();
        },
        error: function (xhr, status, error) {
            console.error("Failed to add to cart:", error);
            alert("Something went wrong while adding to cart.");
        }
    });
}

// Handle the page load event and check for role status
window.onload = function () {
    updateCartCountBadge();

    checkRoleStatus()
        .then(() => {
            showBookList();  // Show book list after role check
            if (userRole === "Admin") {
                const bookForm = document.querySelector("#create-book");
                if (bookForm) {
                    bookForm.addEventListener("submit", handleBookFormSubmit);
                }
            }
        })
        .catch((error) => {
            console.error(error);
        });
};