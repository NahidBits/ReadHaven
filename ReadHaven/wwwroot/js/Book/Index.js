let userRole = ""; // Global User Role

// Function to check admin status FIRST
function checkRoleStatus() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: "/Auth/GetUserRoleStatus", 
            type: "GET",
            success: function (data) {
                if (data) {
                    userRole = data;
                    resolve();
                } else {
                    reject("Could not determine admin role.");
                }
            },
            error: function (xhr, status, error) {
                reject("Error while checking admin status: " + error);
            }
        });
    });
}


// Form Submission for Creating a New Book
function handleBookFormSubmit(event) {
    event.preventDefault(); 

    const formData = new FormData(document.querySelector("#create-book"));

    $.ajax({
        url: "/Create",
        type: "POST",
        data: formData,
        processData: false,  // Important: Don't process FormData
        contentType: false,  // Important: Let browser set Content-Type
        success: function (response) {
            if (response.success) {
                alert("Book added successfully!");
                form.reset();        
                showBookList();      // Refresh list
            }

             else {
                alert("Error adding book.");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error submitting the book form:", textStatus, errorThrown);
        }
    });
}

// Search book by Title,Genre,price etc
function applyFilters() {
    const title = document.getElementById("searchInput").value.trim();
    const genre = document.getElementById("genreInput").value.trim();
    const priceSort = parseInt(document.getElementById("priceSortSelect").value) || null;

    const searchData = {
        Title: title,
        Genre: genre,
        PriceSort: priceSort
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
            tableBody.innerHTML = "";  

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
                        <a href="/BookDetails/Details/${book.id}" class="btn btn-info btn-sm">Details</a>
                        ${userRole === "Admin" ? `
                            <button type="button" class="btn btn-danger btn-sm" onclick="deleteBook('${book.id}')">Delete</button>
                        ` : ''}
                            <button type="submit" class="btn btn-primary btn-sm" onclick="addToCart('${book.id}')">Add To Cart</button>
                    </td>
                `;

                // Append the row to the table body
                tableBody.appendChild(row);
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading books:", textStatus, errorThrown);
        }
    });
}

function deleteBook(bookId) {

    if (confirm('Are you sure you want to delete this book?')) {

        $.ajax({
            url: '/Delete/' + bookId,  
            type: 'POST',  
            success: function (response) {
                alert('Book deleted successfully!');
                showBookList();  
            },
            error: function (xhr, status, error) {
                alert('Error deleting book: ' + error);
            }
        });
    }
}

window.onload = function () {
    checkRoleStatus()
        .then(() => {
            showBookList();
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