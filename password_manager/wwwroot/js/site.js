// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var toggleVisible = (id) =>
{
    var passwordInput = document.getElementById(id);
    if (passwordInput.type === "password") {
        passwordInput.type = "text";
    } else {
        passwordInput.type = "password";
    }
}

// var handleDelete = (id) =>
// {
//     $.ajax({
//         url: `Home/Delete/${id}`,
//         type: 'DELETE',
//         // data: {
//         //     id: id
//         // },
//         success: () =>
//         {
//             window.location.reload()
//         },
//         error: (err) =>
//         {
//             console.log(err)
//         }
//     })
// }